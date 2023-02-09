using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Types;

namespace ASiNet.CustomTelegramBot;
public class PageResult
{
    public PageResult(PageResultAction action, IPage? nextPage = null, PageOptions options = default)
    {
        Options = options;
        Action = action;
        NextPage = nextPage;
    }

    public PageOptions Options { get; set; }
    public PageResultAction Action { get; set; }
    public IPage? NextPage { get; set; }
    public List<Notification>? RemoveNotifications { get; set; }
    public List<Notification>? AddNotifications { get; set; }

    public const PageOptions DefaultOptions = PageOptions.UseUpdateStub | PageOptions.ShowInlineKeyboard;
    public const PageOptions DefaultOptionsNoUseUpdateStub = PageOptions.ShowInlineKeyboard;
    public const PageOptions DefaultOptionsHideInlineKeyboard = PageOptions.UseUpdateStub | PageOptions.HideInlineKeyboard;

    public static PageResult ToNextPage(IPage page, PageOptions options = DefaultOptions) => new(PageResultAction.ToNextPage, page, options);
    public static PageResult ToPreviousPage(PageOptions options = DefaultOptions) => new(PageResultAction.ToPreviousPage, options: options);
    public static PageResult UpdateThisPage(PageOptions options = DefaultOptions) => new(PageResultAction.UpdatePage, options: options);

    public static PageResult Exit(PageOptions options = DefaultOptions) => new(PageResultAction.CloseSession, options: options);
    public static PageResult ExitAndSendEndPage(IPage page, PageOptions options = DefaultOptions) => new(PageResultAction.CloseSession, page, options);

    public PageResult AddNotification(Notification notify)
    {
        if (AddNotifications is null)
            AddNotifications = new();
        Action |= PageResultAction.AddNotifications;
        AddNotifications.Add(notify);
        return this;
    }

    public PageResult RemoveNotification(Notification notify)
    {
        if (RemoveNotifications is null)
            RemoveNotifications = new();
        Action |= PageResultAction.RemoveNotifications;
        RemoveNotifications.Add(notify);
        return this;
    }
}
