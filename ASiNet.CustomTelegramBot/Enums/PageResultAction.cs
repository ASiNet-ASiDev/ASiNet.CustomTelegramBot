namespace ASiNet.CustomTelegramBot.Enums;
[Flags]
public enum PageResultAction
{
    None = 0,
    UpdatePage = 1 << 0,
    ToNextPage = 1 << 1,
    ToPreviousPage = 1 << 2,
    AddNotifications = 1 << 3,
    RemoveNotifications = 1 << 4,
    CloseSession = 1 << 5,
}
