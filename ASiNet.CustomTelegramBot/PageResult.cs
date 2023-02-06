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
    public PageResult(PageResultAction action, IPage? nextPage = null, ResultOptions options = default)
    {
        Options = options;
        Action = action;
        NextPage = nextPage;
    }

    public ResultOptions Options { get; set; }
    public PageResultAction Action { get; set; }
    public IPage? NextPage { get; set; }

    public const ResultOptions DefaultOptions = ResultOptions.UseUpdateStub | ResultOptions.EditLastMessage;

    public const ResultOptions DefaultOptionsNoUseUpdateStub = ResultOptions.EditLastMessage;

    public const ResultOptions DefaultOptionsNoEditLastMsg = ResultOptions.UseUpdateStub;

    public static PageResult ToNextPage(IPage page, ResultOptions options = DefaultOptions) => new(PageResultAction.ToNextPage, page, options);
    public static PageResult ToPreviousPage(ResultOptions options = DefaultOptions) => new(PageResultAction.ToPreviousPage, options: options);
    public static PageResult UpdateThisPage(ResultOptions options = DefaultOptions) => new(PageResultAction.UpdatePage, options: options);

    public static PageResult Exit(ResultOptions options = DefaultOptions) => new(PageResultAction.Exit, options: options);
    public static PageResult ExitAndSendEndPage(IPage page, ResultOptions options = DefaultOptions) => new(PageResultAction.Exit, page, options);
}
