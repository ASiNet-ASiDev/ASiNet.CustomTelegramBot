using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Pages;
using ASiNet.CustomTelegramBot.Types;
using System.Data;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Types;
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
        var msg = client.SendTextMessageAsync(Chat, "Initialize...").Result;
        _lastMessage = msg;
        UpdatePage(client, new PageResult(PageResultAction.None));
    }

    public void OnMessage(ITelegramBotClient client, Message msg)
    {
        lock (_locker)
        {

        }
    }

    public void OnCommand(ITelegramBotClient client, Message msg)
    {
        lock (_locker)
        {

        }
    }

    public void OnButtonCallback(ITelegramBotClient client, CallbackQuery callback)
    {
        lock (_locker)
        {
            if (callback.Data is null)
                return;
            var result = ExecuteButtonEvent(callback.Data);
            if (result is null)
                return;
            if(result.Options.UseUpdateStub)
                SetUpdateStubPage(client, result);
            switch (result.Action)
            {
                case PageResultAction.UpdatePage:
                    UpdatePage(client, result);
                    return;
                case PageResultAction.ToNextPage:
                    if (result.NextPage is not null)
                        ToNextPage(client, result, result.NextPage);
                    return;
                case PageResultAction.ToPreviousPage:
                    ToPreviousPage(client, result);
                    return;
                case PageResultAction.Exit:
                    if (result.NextPage is not null)
                        ToNextPage(client, result, result.NextPage);
                    Dispose();
                    return;
            }
        }
    }

    private PageResult? ExecuteButtonEvent(string name)
    {
        try
        {
            var page = Navigator.GetPage();
            var type = page.GetType();

            var method = type.GetMethods().FirstOrDefault(x => x.GetCustomAttribute<ButtonEventAttribute>() is not null && x.Name == name);

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

    private void ToNextPage(ITelegramBotClient client, PageResult pageResult, IPage page)
    {
        try
        {
            Navigator += page;
            var buttons = GenerateButtons(page);
            if (pageResult.Options.EditLastMessage && _lastMessage.MessageId != -1)
                _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Description, replyMarkup: buttons).Result;
            else
                _lastMessage = client.SendTextMessageAsync(Chat, page.Description, replyMarkup: buttons).Result;
        }
        catch (Exception)
        {
        }
    }

    private void ToPreviousPage(ITelegramBotClient client, PageResult pageResult)
    {
        try
        {
            Navigator--;
            var page = Navigator.GetPage();
            var buttons = GenerateButtons(page);
            if (pageResult.Options.EditLastMessage && _lastMessage.MessageId != -1)
                _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Description, replyMarkup: buttons).Result;
            else
                _lastMessage = client.SendTextMessageAsync(Chat, page.Description, replyMarkup: buttons).Result;
        }
        catch (Exception)
        {
        }
    }

    private void UpdatePage(ITelegramBotClient client, PageResult pageResult)
    {
        try
        {
            var page = Navigator.GetPage();
            var buttons = GenerateButtons(page);
            if (_lastMessage.MessageId != -1)
                _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Description, replyMarkup: buttons).Result;
        }
        catch (Exception)
        {
        }
    }

    private void SetUpdateStubPage(ITelegramBotClient client, PageResult pageResult)
    {
        try
        {
            var page = new UpdateStubPage();
            if (pageResult.Options.EditLastMessage && _lastMessage.MessageId != -1)
                _lastMessage = client.EditMessageTextAsync(Chat, _lastMessage.MessageId, page.Description).Result;
            else
                _lastMessage = client.SendTextMessageAsync(Chat, page.Description).Result;
        }
        catch (Exception)
        {
        }
    }

    private InlineKeyboardMarkup GenerateButtons(IPage page)
    {
        try
        {
            var type = page.GetType();
            var grid = new Grid();
            ButtonEventAttribute? attr = null;
            foreach (var item in type.GetMethods().Where(x => (attr = x.GetCustomAttribute<ButtonEventAttribute>()) is not null))
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
