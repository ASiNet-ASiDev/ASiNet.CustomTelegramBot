#define CALC_EXECUTE_TIME
#define PRINT_EXCEPTIONS
using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Debugs;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Pages;
using ASiNet.CustomTelegramBot.Types;
using System.Diagnostics;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot;

public class PrivateChatSession : IDisposable
{
    public PrivateChatSession(Chat chat, Navigator nav, CustomTelegramBotClient client)
    {
        Client = client;
        Chat = chat;
        Navigator = nav;
        _lastMessage = new()
        {
            MessageId = -1
        };
        LastActiveTime = DateTime.UtcNow;
    }

    public Chat Chat { get; set; }
    public Navigator Navigator { get; set; }
    public DateTime LastActiveTime { get; set; }

    internal bool IsClosed { get; private set; }

    private readonly object _locker = new();
    private Message _lastMessage;

    public readonly CustomTelegramBotClient Client;

    public void Init(ITelegramBotClient client)
    {
        lock (_locker)
        {
            ProcessingPageResult(client, PageResult.UpdateThisPage(PageResult.DefaultOptions), EventTriggerType.None);
        }
    }

    public async Task AddPage(ITelegramBotClient client, IPage page)
    {
        await Task.Run(() =>
        {
            lock (_locker)
            {
                ProcessingPageResult(client, PageResult.ToNextPage(page, PageResult.DefaultOptions), EventTriggerType.None);
            }
        });
    }

    public void OnMessage(ITelegramBotClient client, Message msg)
    {
        lock (_locker)
        {
            LastActiveTime = DateTime.UtcNow;
            if (msg.Type == MessageType.Text && msg.Text?[0] == '/')
            {
                var pageResult = ExecuteCommandEvent(msg.Text);
                var result = ProcessingPageResult(client, pageResult, EventTriggerType.Command);
            }
            else
            {
                var pageResult = ExecuteMessageEvent(msg);
                var result = ProcessingPageResult(client, pageResult, EventTriggerType.Message);
            }
        }
    }


    public void OnButtonCallback(ITelegramBotClient client, CallbackQuery callback)
    {
        lock (_locker)
        {
            LastActiveTime = DateTime.UtcNow;
            if (callback.Data is null)
                return;
            var pageResult = ExecuteButtonEvent(callback.Data);
            var result = ProcessingPageResult(client, pageResult, EventTriggerType.ButtonCallback);
        }
    }

    private bool ProcessingPageResult(ITelegramBotClient client, PageResult? result, EventTriggerType trigger)
    {
        try
        {
            if (result is null)
                return false;
            if (result.Action.HasFlag(PageResultAction.UpdatePage))
            {
                UpdatePage(client, result, result, trigger);
            }
            else if (result.Action.HasFlag(PageResultAction.ToNextPage))
            {
                // TODO: сделать страницу с ошибкой.
                if (result.NextPage is null)
                    return false;
                var container = new PageContainer(result.NextPage, result.Options);
                ToNextPage(client, container, result);
            }
            else if (result.Action.HasFlag(PageResultAction.ToPreviousPage))
            {
                ToPreviousPage(client, result, result);
            }
            else if (result.Action.HasFlag(PageResultAction.CloseSession))
            {
                if (result.NextPage is null)
                {
                    Dispose();
                    return true;
                }
                var container = new PageContainer(result.NextPage, result.Options);
                ToNextPage(client, container, result);
                Dispose();
            }
            if (result.Action.HasFlag(PageResultAction.AddNotifications) && result.AddNotifications is not null)
            {
                foreach (var item in result.AddNotifications)
                    item.ChatId = Chat.Id;

                Client.AddNotifications(result.AddNotifications);
            }
            if (result.Action.HasFlag(PageResultAction.RemoveNotifications) && result.RemoveNotifications is not null)
                Client.RemoveNotifications(result.RemoveNotifications);
            return true;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
            return false;
        }
    }

    private PageResult? ExecuteButtonEvent(string name)
    {
        #region debug init
#if DEBUG && CALC_EXECUTE_TIME
        var sw = new Stopwatch();
        var findMethodTime = 0L;
        var invokeMethodTime = 0L;
#endif
        #endregion
        try
        {
            #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Start();
#endif
            #endregion

            // TODO: вынести методы поиска в отдельный класс и кешировать часто используемые методы.
            var page = Navigator.GetPage().Page;
            var type = page.GetType();
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnButtonCallbackEventAttribute>() is OnButtonCallbackEventAttribute attr && x.Name == name);

            #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Stop();
            findMethodTime = sw.ElapsedMilliseconds;
#endif
            #endregion

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                {
                    #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Restart();
#endif
                    #endregion

                    var result = method.Invoke(page, null) as PageResult;

                    #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Stop();
                    invokeMethodTime = sw.ElapsedMilliseconds;
#endif
                    #endregion

                    return result;
                }
                else if (parameters.Length == 1
                    && parameters[0].ParameterType.IsArray
                    && parameters[0].ParameterType.GetElementType() == typeof(string))
                {
                    #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Restart();
#endif
                    #endregion

                    var result = (PageResult?)method.Invoke(page, Array.Empty<string>());

                    #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Stop();
                    invokeMethodTime = sw.ElapsedMilliseconds;
#endif
                    #endregion

                    return result;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion

            return null;
        }
        #region debug result
#if DEBUG && CALC_EXECUTE_TIME
        finally
        {
            DebugLoger.InfoLog($"chat: {Chat.Id}", $"method: {nameof(ExecuteButtonEvent)}", $"Total Time: {findMethodTime + invokeMethodTime}ms", $"Find Time: {findMethodTime}ms", $"Execute Time: {invokeMethodTime}ms");
        }
#endif
        #endregion
    }

    private PageResult? ExecuteMessageEvent(Message msg)
    {
        #region debug init
#if DEBUG && CALC_EXECUTE_TIME
        var sw = new Stopwatch();
        var findMethodTime = 0L;
        var invokeMethodTime = 0L;
#endif
        #endregion
        try
        {
            #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Start();
#endif
            #endregion
            // TODO: вынести методы поиска в отдельный класс и кешировать часто используемые методы.
            var page = Navigator.GetPage().Page;
            var type = page.GetType();
            var msgType = MessageTypeConverterTolFags.GetFlags(msg.Type);
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnMessageEventAttribute>() is OnMessageEventAttribute attr
                && (attr.MessageTypesFilter == MessageTypeFlags.AllTypes || attr.MessageTypesFilter.HasFlag(msgType)));

            #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Stop();
            findMethodTime = sw.ElapsedMilliseconds;
#endif
            #endregion

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1
                    && parameters[0].ParameterType == typeof(Message))
                {
                    #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Restart();
#endif
                    #endregion

                    var result = (PageResult?)method.Invoke(page, new[] { msg });

                    #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Stop();
                    invokeMethodTime = sw.ElapsedMilliseconds;
#endif
                    #endregion

                    return result;
                }


            }
            return null;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
            return null;
        }
        #region debug result
#if DEBUG && CALC_EXECUTE_TIME
        finally
        {
            DebugLoger.InfoLog($"chat: {Chat.Id}", $"method: {nameof(ExecuteMessageEvent)}", $"Total Time: {findMethodTime + invokeMethodTime}ms", $"Find Time: {findMethodTime}ms", $"Execute Time: {invokeMethodTime}ms");
        }
#endif
        #endregion
    }

    private PageResult? ExecuteCommandEvent(string text)
    {
        #region debug init
#if DEBUG && CALC_EXECUTE_TIME
        var sw = new Stopwatch();
        var findMethodTime = 0L;
        var invokeMethodTime = 0L;
#endif
        #endregion
        try
        {
            #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Start();
#endif
            #endregion
            // TODO: вынести методы поиска в отдельный класс и кешировать часто используемые методы.
            var page = Navigator.GetPage().Page;
            var type = page.GetType();

            var commandParameters = text.Trim().TrimStart('/').ToLower().Split(' ');
            if (commandParameters.Length < 1)
                return null;
            var command = commandParameters[0];
            if (command == "stop")
            {
                Dispose();
                return null;
            }
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnCommandEventAttribute>() is OnCommandEventAttribute attr && attr.Command == command);

            #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Stop();
            findMethodTime = sw.ElapsedMilliseconds;
#endif
            #endregion

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                    return method.Invoke(page, null) as PageResult;
                else if (parameters.Length == 1
                    && parameters[0].ParameterType.IsArray
                    && parameters[0].ParameterType.GetElementType() == typeof(string))
                {
                    #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Restart();
#endif
                    #endregion

                    var result = method.Invoke(page, commandParameters.Length > 1 ? commandParameters[1..] : Array.Empty<string>()) as PageResult;

                    #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
                    sw.Stop();
                    invokeMethodTime = sw.ElapsedMilliseconds;
#endif
                    #endregion

                    return result;
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
            return null;
        }
        #region debug result
#if DEBUG && CALC_EXECUTE_TIME
        finally
        {
            DebugLoger.InfoLog($"chat: {Chat.Id}", $"method: {nameof(ExecuteCommandEvent)}", $"Total Time: {findMethodTime + invokeMethodTime}ms", $"Find Time: {findMethodTime}ms", $"Execute Time: {invokeMethodTime}ms");
        }
#endif
        #endregion
    }

    private void ToNextPage(ITelegramBotClient client, PageContainer container, PageResult pageResult)
    {
        try
        {
            if (pageResult.Options.HasFlag(PageOptions.UseUpdateStub))
            {
                SendUpdateStub(client);
                Navigator += container;
                UpdateTextMessage(client, container);
            }
            else
            {
                Navigator += container;
                SendTextMessage(client, container);
            }
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
        }
    }

    private void ToPreviousPage(ITelegramBotClient client, PageResult result, PageResult pageResult)
    {
        try
        {
            var isUseUpdateStub = pageResult.Options.HasFlag(PageOptions.UseUpdateStub);
            if (isUseUpdateStub)
                SendUpdateStub(client);
            Navigator--;
            var container = Navigator.GetPage();
            container.Options = result.Options;
            if (isUseUpdateStub)
                UpdateTextMessage(client, container);
            else
                SendTextMessage(client, container);
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
        }
    }

    private void UpdatePage(ITelegramBotClient client, PageResult result, PageResult pageResult, EventTriggerType trigger)
    {
        try
        {
            if (trigger == EventTriggerType.ButtonCallback)
            {
                if (pageResult.Options.HasFlag(PageOptions.UseUpdateStub))
                    SetUpdateStub(client);
                var container = Navigator.GetPage();
                container.Options = result.Options;
                UpdateTextMessage(client, container);
            }
            else
            {
                if (pageResult.Options.HasFlag(PageOptions.UseUpdateStub))
                {
                    SendUpdateStub(client);
                    var container = Navigator.GetPage();
                    UpdateTextMessage(client, container);
                }
                else
                {
                    var container = Navigator.GetPage();
                    SendTextMessage(client, container);
                }
            }
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
        }
    }

    private void SetUpdateStub(ITelegramBotClient client)
    {
        try
        {
            if (_lastMessage.MessageId != -1)
            {
                var page = new UpdateStubPage();
                _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Text, replyMarkup: InlineKeyboardMarkup.Empty()).Result;
            }
            else
                SendUpdateStub(client);
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
        }
    }

    private void SendUpdateStub(ITelegramBotClient client)
    {
        try
        {
            if (_lastMessage.ReplyMarkup is not null)
                _lastMessage = client.EditMessageReplyMarkupAsync(Chat, _lastMessage.MessageId, InlineKeyboardMarkup.Empty()).Result;
            var page = new UpdateStubPage();
            _lastMessage = client.SendTextMessageAsync(Chat, page.Text, replyMarkup: InlineKeyboardMarkup.Empty()).Result;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
        }
    }

    private void UpdateTextMessage(ITelegramBotClient client, PageContainer container)
    {
        if (_lastMessage.MessageId != -1)
        {
            var textPage = container.Page as ITextPage;
            _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, textPage?.Text ?? string.Empty, replyMarkup: container.Buttons).Result;
        }
        else
        {
            SendTextMessage(client, container);
        }
    }

    private void SendTextMessage(ITelegramBotClient client, PageContainer container)
    {
        var textPage = container.Page as ITextPage;
        if (_lastMessage.ReplyMarkup is not null)
            _lastMessage = client.EditMessageReplyMarkupAsync(Chat, _lastMessage.MessageId, InlineKeyboardMarkup.Empty()).Result;
        _lastMessage = client.SendTextMessageAsync(Chat, textPage?.Text ?? string.Empty, replyMarkup: container.Buttons).Result;
    }

    public void Dispose()
    {
        try
        {
            IsClosed = true;
            Navigator.Dispose();
        }
        catch (Exception)
        {

        }
    }
}
