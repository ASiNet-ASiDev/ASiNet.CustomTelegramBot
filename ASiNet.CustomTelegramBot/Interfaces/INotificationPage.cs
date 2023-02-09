using ASiNet.CustomTelegramBot.Attributes;

namespace ASiNet.CustomTelegramBot.Interfaces;
public interface INotificationPage : ITextPage
{
    [OnCommandEvent("hide")]
    [OnButtonCallbackEvent("Hide", 500, 0)]
    public PageResult Hide() => PageResult.ToPreviousPage();

    [OnCommandEvent("back")]
    [OnButtonCallbackEvent("Back", 500, 0)]
    public PageResult Back() => PageResult.ToPreviousPage();
}
