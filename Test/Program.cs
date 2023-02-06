using ASiNet.CustomTelegramBot;
using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Interfaces;

var bot = new CustomTelegramBotClient<MainPage>("5890196774:AAE76zbTOCuWUmKvhmM_mteBfd169acMj_Q");

Console.ReadLine();


class MainPage : IPage
{
    public string Description => "Hello World";

    [ButtonEvent("Next", 0, 0)]
    public PageResult ToNextPage()
    {
        return PageResult.ToNextPage(new OnePage(), PageResultOptions.Default());
    }

    public void Dispose()
    {
        
    }
}

class OnePage : IPage
{
    public string Description => $"Clicks: {_count}";
    private int _count;

    [ButtonEvent("Click", 0, 0)]
    public PageResult Click()
    {
        _count++;
        return PageResult.UpdateThisPage(PageResultOptions.DefaultNoUseUpdateStub());
    }

    [ButtonEvent("Previous", 0, 1)]
    public PageResult ToPreviousPage()
    {
        return PageResult.ToPreviousPage(PageResultOptions.Default());
    }

    [ButtonEvent("Next", 1, 0)]
    public PageResult ToNextPage()
    {
        return PageResult.ToNextPage(new FooPage(), PageResultOptions.Default());
    }

    public void Dispose()
    {

    }
}

class FooPage : IPage
{
    public string Description => "Foo Page";

    [ButtonEvent("Previous", 0, 1)]
    public PageResult ToPreviousPage()
    {
        return PageResult.ToPreviousPage(PageResultOptions.Default());
    }

    [ButtonEvent("Exit", 1, 0)]
    public PageResult Exit()
    {
        return PageResult.ExitAndSendEndPage(new EndPage(), PageResultOptions.Default());
    }

    public void Dispose()
    {

    }
}

class EndPage : IPage
{
    public string Description => "Сессия окончена, все данные удалены.";

    [ButtonEvent("Restart", 1, 0)]
    public PageResult Exit()
    {
        return PageResult.UpdateThisPage(PageResultOptions.Default());
    }

    public void Dispose()
    {

    }
}