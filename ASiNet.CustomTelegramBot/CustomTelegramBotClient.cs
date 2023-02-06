using ASiNet.CustomTelegramBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASiNet.CustomTelegramBot;
public class CustomTelegramBotClient<Tbasepage> : IDisposable where Tbasepage : IPage, new()
{
    public CustomTelegramBotClient(string token)
    {
        _source = new();
        _sessions = new();
        _client = new(token);
        _client.StartReceiving(OnUpdate, OnError, cancellationToken: _source.Token);
    }

    public CustomTelegramBotClient(TelegramBotClient client)
    {
        _source = new();
        _sessions = new();
        _client = client;
        _client.StartReceiving(OnUpdate, OnError);
    }

    private TelegramBotClient _client;

    private List<Session> _sessions;
    private CancellationTokenSource _source;
    private readonly object _locker = new();

    private void RemoveSession(Session session)
    {
        lock (_locker)
        {
            _sessions.Remove(session);
        }    
    }

    private async Task OnUpdate(ITelegramBotClient client, Update update, CancellationToken token)
    {
        await Task.Run(() =>
        {
            var chat = update.Message?.Chat ?? update.CallbackQuery?.Message?.Chat;
            if (chat is null)
                return;
            if (chat.Type != ChatType.Private)
                return;
            Session? session = null;
            lock (_locker)
            {
                session = _sessions.FirstOrDefault(x => x.Chat.Id == chat.Id);
                if (session is null)
                {
                    session = new(chat, new(new Tbasepage()), RemoveSession);
                    _sessions.Add(session);
                    session.Init(client);
                }
            }
            if(session is null)
                return;

            switch (update.Type)
            {
                case UpdateType.Message:
                    if (update.Message is null)
                        return;
                    session.OnMessage(client, update.Message);
                    return;
                case UpdateType.CallbackQuery:
                    if (session is null)
                        return;
                    if (update.CallbackQuery is null)
                        return;
                    session.OnButtonCallback(client, update.CallbackQuery);
                    return;
                default:
                    break;
            }
        });
    }

    private Task OnError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
        throw arg2;
    }

    public void Dispose()
    {
        _source.Cancel();
        _source.Dispose();
        while (_sessions.Count > 0)
        {
            var session = _sessions.First();
            session.Dispose();
            _sessions.Remove(session);
        }
    }
}