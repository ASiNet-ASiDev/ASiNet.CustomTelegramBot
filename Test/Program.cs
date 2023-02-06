using ASiNet.CustomTelegramBot;
using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var bot = new CustomTelegramBotClient<MainPage>("5890196774:AAHjU1Kwb2H-NThfIPmzIraoffkamqFN2Rk");

Console.ReadLine();


class MainPage : IPage
{
    public string Description => "Hello World";

    [CommandEvent("next")]
    [ButtonEvent("Next", 0, 0)]
    public PageResult ToNextPage() => PageResult.ToNextPage(new OnePage());
    public void Dispose() { }
}

class OnePage : IPage
{
    public string Description => $"Clicks: {_count}";
    private int _count;

    [ButtonEvent("Click", 0, 0)]
    public PageResult Click()
    {
        _count++;
        return PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub);
    }
    [ButtonEvent("Previous", 0, 1)]
    public PageResult ToPreviousPage() => PageResult.ToPreviousPage();
    [CommandEvent("next")]
    [ButtonEvent("Next", 1, 0)]
    public PageResult ToNextPage() => PageResult.ToNextPage(new FooPage());
    public void Dispose() { }
}

class FooPage : IPage
{
    public string Description => $"Foo Page, Enter text: {_text}";
    private string _text = "";
    
    [MessageEvent(MessageTypeFlags.Text | MessageTypeFlags.Photo)]
    public PageResult OnTextMsg(Message msg)
    {
        switch (msg.Type)
        {
            case MessageType.Text:
                _text = msg.Text ?? string.Empty; 
                break;
            case MessageType.Photo:
                _text = msg.Caption ?? string.Empty;
                break;
        }
        return PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub);
    }

    [ButtonEvent("Previous", 0, 1)]
    public PageResult ToPreviousPage() => PageResult.ToPreviousPage();
    [CommandEvent("exit")]
    [ButtonEvent("Exit", 1, 0)]
    public PageResult Exit() => PageResult.ExitAndSendEndPage(new EndPage());
    public void Dispose() { }
}

class EndPage : IPage
{
    public string Description => "Сессия окончена, все данные удалены.";
    public void Dispose() { }
}