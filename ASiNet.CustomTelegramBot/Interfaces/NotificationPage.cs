using ASiNet.CustomTelegramBot.Attributes;

namespace ASiNet.CustomTelegramBot.Interfaces;
public abstract class NotificationPage : ITextPage
{
    public virtual string Text => "Default notify!";

    [OnCommandEvent("hide")]
    [OnButtonCallbackEvent("Hide", 500, 0)]
    public virtual PageResult Hide() => PageResult.ToPreviousPage();

    [OnCommandEvent("back")]
    [OnButtonCallbackEvent("Back", 500, 0)]
    public virtual PageResult Back() => PageResult.ToPreviousPage();

    public virtual void Dispose() { }
}
