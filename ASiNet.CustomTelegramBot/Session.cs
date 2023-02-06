using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Pages;
using ASiNet.CustomTelegramBot.Types;
using System.Data;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot;
public class Session : IDisposable
{
    public Session(Chat chat, Navigator nav, Action<Session> removeSession)
    {
        _removeSession = removeSession;
        Chat = chat;
        Navigator = nav;
        _lastMessage = new()
        { 
            MessageId = -1
        };
    }

    public Chat Chat { get; set; }
    public Navigator Navigator { get; set; }

    private readonly object _locker = new();
    private Message _lastMessage;
    private Action<Session> _removeSession;

    public void Init(ITelegramBotClient client)
    {
        lock (_locker)
        {
            SetUpdateStubPage(client, new(PageResultAction.None), EventTriggerType.None);
            UpdatePage(client, PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub), EventTriggerType.ButtonCallback);
        }
    }

    public void OnMessage(ITelegramBotClient client, Message msg)
    {
        lock (_locker)
        {
            if(msg.Type == MessageType.Text && msg.Text?[0] == '/')
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
            if (callback.Data is null)
                return;
            var pageResult = ExecuteButtonEvent(callback.Data);
            var result = ProcessingPageResult(client, pageResult, EventTriggerType.ButtonCallback);
        }
    }

    private PageResult? ExecuteButtonEvent(string name)
    {
        try
        {
            var page = Navigator.GetPage();
            var type = page.GetType();
            var hashCode = page.GetHashCode();
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnButtonCallbackEventAttribute>() is OnButtonCallbackEventAttribute attr && x.Name == name);

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                    return method.Invoke(page, null) as PageResult;
                else if (parameters.Length == 1
                    && parameters[0].ParameterType.IsArray
                    && parameters[0].ParameterType.GetElementType() == typeof(string))
                    return method.Invoke(page, Array.Empty<string>()) as PageResult;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private PageResult? ExecuteMessageEvent(Message msg)
    {
        try
        {
            var page = Navigator.GetPage();
            var type = page.GetType();
            var msgType = MessageTypeConverterTolFags.GetFlags(msg.Type);
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnMessageEventAttribute>() is OnMessageEventAttribute attr 
            && (attr.MessageTypesFilter == MessageTypeFlags.AllTypes || attr.MessageTypesFilter.HasFlag(msgType)));

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 1 
                    && parameters[0].ParameterType == typeof(Message))
                    return method.Invoke(page, new[] { msg }) as PageResult;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private PageResult? ExecuteCommandEvent(string text)
    {
        try
        {
            var page = Navigator.GetPage();
            var type = page.GetType();

            var commandParameters = text.Trim().TrimStart('/').ToLower().Split(' ');
            if (commandParameters.Length < 1)
                return null;
            var command = commandParameters[0];
            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<OnCommandEventAttribute>() is OnCommandEventAttribute attr && attr.Command == command);

            if (method is not null
                && method.ReturnParameter.ParameterType == typeof(PageResult))
            {
                var parameters = method.GetParameters();
                if (parameters.Length == 0)
                    return method.Invoke(page, null) as PageResult;
                else if (parameters.Length == 1
                    && parameters[0].ParameterType.IsArray
                    && parameters[0].ParameterType.GetElementType() == typeof(string))
                    return method.Invoke(page, commandParameters.Length > 1 ? commandParameters[1..] : Array.Empty<string>()) as PageResult;
            }
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    private bool ProcessingPageResult(ITelegramBotClient client, PageResult? result, EventTriggerType trigger)
    {
        try
        {
            if (result is null)
                return false;
            if (result.Options.HasFlag(ResultOptions.UseUpdateStub))
                SetUpdateStubPage(client, result, trigger);
            switch (result.Action)
            {
                case PageResultAction.UpdatePage:
                    UpdatePage(client, result, trigger);
                    break;
                case PageResultAction.ToNextPage:
                    if (result.NextPage is not null)
                        ToNextPage(client, result, result.NextPage, trigger);
                    break;
                case PageResultAction.ToPreviousPage:
                    ToPreviousPage(client, result, trigger);
                    break;
                case PageResultAction.Exit:
                    if (result.NextPage is not null)
                        ToNextPage(client, result, result.NextPage, trigger);
                    Dispose();
                    break;
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private void ToNextPage(ITelegramBotClient client, PageResult pageResult, IPage page, EventTriggerType trigger)
    {
        try
        {
            Navigator += page;
            var buttons = GenerateButtons(page);
            SendOrEditTextMessage(client, page, pageResult, buttons, trigger);
        }
        catch (Exception)
        {
        }
    }

    private void ToPreviousPage(ITelegramBotClient client, PageResult pageResult, EventTriggerType trigger)
    {
        try
        {
            Navigator--;
            var page = Navigator.GetPage();
            var buttons = GenerateButtons(page);
            SendOrEditTextMessage(client, page, pageResult, buttons, trigger);
        }
        catch (Exception)
        {
        }
    }

    private void UpdatePage(ITelegramBotClient client, PageResult pageResult, EventTriggerType trigger)
    {
        try
        {
            var page = Navigator.GetPage();
            var buttons = GenerateButtons(page);
            SendOrEditTextMessage(client, page, pageResult, buttons, trigger);
        }
        catch (Exception)
        {
        }
    }

    private void SetUpdateStubPage(ITelegramBotClient client, PageResult pageResult, EventTriggerType trigger)
    {
        try
        {
            var page = new UpdateStubPage();
            SendOrEditTextMessage(client, page, PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub), InlineKeyboardMarkup.Empty(), trigger);
        }
        catch (Exception)
        {
        }
    }

    private void SendOrEditTextMessage(ITelegramBotClient client, IPage page, PageResult pageResult, InlineKeyboardMarkup markup, EventTriggerType trigger)
    {
        if (pageResult.Options.HasFlag(ResultOptions.EditLastMessage) 
            && _lastMessage.MessageId != -1
            && (trigger == EventTriggerType.ButtonCallback 
                || pageResult.Options.HasFlag(ResultOptions.UseUpdateStub)))
        {
            _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Description, replyMarkup: markup).Result;
        }
        else
        {
            if(_lastMessage.MessageId != -1 && _lastMessage.ReplyMarkup is not null)
                _lastMessage = client.EditMessageReplyMarkupAsync(Chat, _lastMessage.MessageId, InlineKeyboardMarkup.Empty()).Result;
            _lastMessage = client.SendTextMessageAsync(Chat, page.Description, replyMarkup: markup).Result;
        }
    }

    private InlineKeyboardMarkup GenerateButtons(IPage page)
    {
        try
        {
            var type = page.GetType();
            var grid = new Grid();
            OnButtonCallbackEventAttribute? attr = null;
            foreach (var item in type.GetMethods().Where(x => (attr = x.GetCustomAttribute<OnButtonCallbackEventAttribute>()) is not null))
#pragma warning disable CS8602
                grid.AddItem(attr.Cell, attr.Text, item.Name);
#pragma warning restore CS8602
            return grid.GetButtons();
        }
        catch (Exception)
        {
            return new InlineKeyboardMarkup(new InlineKeyboardButton[0][]);
        }
    }

    public void Dispose()
    {
        _removeSession?.Invoke(this);
        Navigator.Dispose();
    }
}
