using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;

namespace ASiNet.CustomTelegramBot.Types;
public class Notification
{
    public INotificationPage NotificationPage { get; set; } = null!;
    public NotificationTriggerType TriggerType { get; set; }
    public DateTime NotificationExecuteTime { get; set; }
    public bool IsReusable { get; set; }
    public string? Path { get; set; }
    public long ChatId { get; set; }

    public static Notification CreateNotification(INotificationPage notification, DateTime executeTime, bool reusable = true) => new()
    {
        NotificationExecuteTime = executeTime,
        TriggerType = NotificationTriggerType.DateTimeTrigger,
        NotificationPage = notification,
        IsReusable = reusable,
    };

    public static Notification CreateNotification(INotificationPage notification, string path, bool reusable = true) => new()
    {
        Path = path,
        TriggerType = NotificationTriggerType.DateTimeTrigger,
        NotificationPage = notification,
        IsReusable = reusable,
    };
}
