namespace ASiNet.CustomTelegramBot.Enums;
[Flags]
public enum PageOptions : long
{
    None = 0,
    UseUpdateStub = 1 << 0,
    HideInlineKeyboard = 1 << 2,
    ShowInlineKeyboard = 1 << 3,
}
