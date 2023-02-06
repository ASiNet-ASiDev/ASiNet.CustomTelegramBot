using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot;
public class PageResult
{
    public PageResult(PageResultAction action, IPage? nextPage = null, PageResultOptions options = default)
    {
        Options = options;
        Action = action;
        NextPage = nextPage;
    }

    public PageResultOptions Options { get; set; }
    public PageResultAction Action { get; set; }
    public IPage? NextPage { get; set; }

    public static PageResult ToNextPage(IPage page, PageResultOptions options) => new(PageResultAction.ToNextPage, page, options);
    public static PageResult ToPreviousPage(PageResultOptions options) => new(PageResultAction.ToPreviousPage, options: options);
    public static PageResult UpdateThisPage(PageResultOptions options) => new(PageResultAction.UpdatePage, options: options);

    public static PageResult Exit(PageResultOptions options) => new(PageResultAction.Exit, options: options);
    public static PageResult ExitAndSendEndPage(IPage page, PageResultOptions options) => new(PageResultAction.Exit, page, options);
}
