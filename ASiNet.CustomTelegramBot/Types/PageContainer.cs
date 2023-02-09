#define CALC_EXECUTE_TIME
#define PRINT_EXCEPTIONS
using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Debugs;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using System.Diagnostics;
using System.Reflection;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot.Types;
public class PageContainer : IDisposable
{

    public PageContainer(IPage page, PageOptions options)
    {
        Page = page;
        Options = options;
    }

    public IPage Page { get; set; }
    public PageOptions Options { get; set; }
    public InlineKeyboardMarkup Buttons => GenerateOrGetButtons();

    private InlineKeyboardMarkup _buttons = null!;

    private InlineKeyboardMarkup GenerateOrGetButtons()
    {
        try
        {
            if (Options.HasFlag(PageOptions.HideInlineKeyboard))
                return InlineKeyboardMarkup.Empty();
            else if (Options.HasFlag(PageOptions.ShowInlineKeyboard))
                return _buttons = _buttons is null ? GenerateButtons() : _buttons;
            else
                return _buttons = _buttons is null ? GenerateButtons() : _buttons;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
            return InlineKeyboardMarkup.Empty();
        }
    }

    private InlineKeyboardMarkup GenerateButtons()
    {
        #region debug init
#if DEBUG && CALC_EXECUTE_TIME
        var sw = new Stopwatch();
        var findMethodsTime = 0L;
        var generateButtonsTime = 0L;
#endif
        #endregion
        try
        {
            #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Start();
#endif
            #endregion

            var type = Page.GetType();
            var grid = new Grid();
            OnButtonCallbackEventAttribute? attr = null;
            foreach (var item in type.GetMethods().Where(x => (attr = x.GetCustomAttribute<OnButtonCallbackEventAttribute>()) is not null))
#pragma warning disable CS8602
                grid.AddItem(attr.Cell, attr.Text, item.Name);
#pragma warning restore CS8602

            #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Stop();
            findMethodsTime = sw.ElapsedMilliseconds;
#endif
            #endregion
            #region debug start calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Restart();
#endif
            #endregion

            var buttons = grid.GetButtons();

            #region debug stop calc time
#if DEBUG && CALC_EXECUTE_TIME
            sw.Stop();
            generateButtonsTime = sw.ElapsedMilliseconds;
#endif
            #endregion

            return buttons;
        }
        catch (Exception ex)
        {
            #region debug print exception
#if DEBUG && PRINT_EXCEPTIONS
            DebugLoger.ErrorLog(ex);
#endif
            #endregion
            return InlineKeyboardMarkup.Empty();
        }
        #region debug result
#if DEBUG && CALC_EXECUTE_TIME
        finally
        {
            DebugLoger.InfoLog($"page: {nameof(Page)}", $"method: {nameof(GenerateButtons)}", $"Total Time: {findMethodsTime + generateButtonsTime}ms", $"Find Time: {findMethodsTime}ms", $"Generate Time: {generateButtonsTime}ms");
        }
#endif
        #endregion
    }

    public void Dispose()
    {
        Page.Dispose();
    }
}
