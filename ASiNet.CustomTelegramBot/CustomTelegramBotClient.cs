using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Pages;
using ASiNet.CustomTelegramBot.Types;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace ASiNet.CustomTelegramBot;
public class CustomTelegramBotClient : IDisposable
{
    public CustomTelegramBotClient(string token, Func<Chat, IPage> getBasePage)
    {
        _source = new();
        _notifications = new();
        _sessions = new();
        _client = new(token);
        _client.StartReceiving(OnUpdate, OnError, cancellationToken: _source.Token);
        _updater = new(OnTimeUpdate, null, TimeSpan.Zero, UpdatePeriod);
    }

    public CustomTelegramBotClient(TelegramBotClient client, Func<Chat, IPage> getBasePage)
    {
        _source = new();
        _notifications = new();
        _sessions = new();
        _client = client;
        _client.StartReceiving(OnUpdate, OnError);
        _updater = new(OnTimeUpdate, null, TimeSpan.Zero, UpdatePeriod);
    }

    public TimeSpan UpdatePeriod { get; set; } = new(0, 1, 0);
    public TimeSpan InactiveSessionLifeTime { get; set; } = new(0, 5, 0);

    private TelegramBotClient _client;
    private List<Notification> _notifications;
    private List<PrivateChatSession> _sessions;
    private CancellationTokenSource _source;

    private Timer _updater;
    private readonly object _locker = new();
    private readonly object _notifyLocker = new();

    private Func<Chat, IPage> _getasePage { get; set; }

    // TODO: использовать специальной обьект как в токене отмены, в место пути.
    public void SendNotification(string path)
    {
        lock (_notifyLocker)
        {
            var deleteNotify = new List<Notification>();
            foreach (var notify in _notifications.Where(x => x.TriggerType == NotificationTriggerType.EventTrigger && x.Path == path))
            {
                var session = _sessions.FirstOrDefault(x => x.Chat.Id == notify.ChatId);
                if (session is null)
                {
                    session = new(new Chat() { Id = notify.ChatId }, new(notify.NotificationPage), this);
                    lock (this)
                        _sessions.Add(session);
                }
                else
                {
                    _ = session.AddPage(_client, notify.NotificationPage);
                }
                if (!notify.IsReusable)
                    deleteNotify.Add(notify);
            }
            foreach (var item in deleteNotify)
                _notifications.Remove(item);
        }
    }

    internal void AddNotifications(List<Notification> notifys)
    {
        lock (_notifyLocker)
        {
            _notifications.AddRange(notifys);
        }
    }

    internal void RemoveNotifications(List<Notification> notifys)
    {
        lock (_notifyLocker)
        {
            foreach (var item in notifys)
            {
                _notifications.Remove(item);
            }
        }
    }

    private void OnTimeUpdate(object? obj)
    {
        var sessions = Task.Run(SessionsUpdate);
        var notify = Task.Run(NotificationUpdate);
        Task.WaitAll(sessions, notify);
    }

    private void NotificationUpdate()
    {
        lock (_notifyLocker)
        {
            var time = DateTime.UtcNow;
            var deleteNotify = new List<Notification>();
            foreach (var notify in _notifications.Where(x => x.TriggerType == NotificationTriggerType.DateTimeTrigger && time - x.NotificationExecuteTime.ToUniversalTime() <= UpdatePeriod))
            {
                var session = _sessions.FirstOrDefault(x => x.Chat.Id == notify.ChatId);
                if (session is null)
                {
                    session = new(new Chat() { Id = notify.ChatId }, new(notify.NotificationPage), this);
                    lock (this)
                        _sessions.Add(session);
                }
                else
                {
                    _ = session.AddPage(_client, notify.NotificationPage);
                }
                if (!notify.IsReusable)
                    deleteNotify.Add(notify);
            }
            foreach (var item in deleteNotify)
                _notifications.Remove(item);
        }
    }

    private void SessionsUpdate()
    {
        lock (_locker)
        {
            var thisTime = DateTime.UtcNow;
            for (int i = 0; i < _sessions.Count; i++)
            {
                var session = _sessions[i];
                if (session.IsClosed)
                {
                    session.Dispose();
                    _sessions.Remove(session);
                    continue;
                }
                if (thisTime - session.LastActiveTime >= InactiveSessionLifeTime)
                {
                    _sessions[i].AddPage(_client, new InactiveSessionPage()).Wait(1000);
                    session.Dispose();
                    _sessions.Remove(session);
                    continue;
                }
            }
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
            {
                PrivateChatSession? session = null;

                session = _sessions.FirstOrDefault(x => x.Chat.Id == chat.Id);
                if (session is null)
                {
                    session = new(chat, new(_getasePage?.Invoke(chat) ?? new DefaultBasePage()), this);
                    lock (_locker)
                        _sessions.Add(session);
                    session.Init(client);
                }
                if (session is null)
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
            }
        });
    }


    private Task OnError(ITelegramBotClient arg1, Exception arg2, CancellationToken arg3)
    {
#if DEBUG
        throw arg2;
#endif
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