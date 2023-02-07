using ASiNet.CustomTelegramBot;
using ASiNet.CustomTelegramBot.Attributes;
using ASiNet.CustomTelegramBot.Enums;
using ASiNet.CustomTelegramBot.Interfaces;
using Telegram.Bot.Types;

var bot = new CustomTelegramBotClient<BasePage>("5890196774:AAHjU1Kwb2H-NThfIPmzIraoffkamqFN2Rk");
Console.Read();

class BasePage : IPage
{
    public string Description => $"Clicked Count: {_count}";
    private int _count;

    [OnCommandEvent("next")]
    [OnButtonCallbackEvent("Next Page", 0, 1)]
    public PageResult ToNextPage() => PageResult.ToNextPage(new OnePage());


    [OnCommandEvent("click")]
    [OnButtonCallbackEvent("Click", 0, 0)]
    public PageResult Click()
    {
        _count++;
        return PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub);
    }
    public void Dispose() { }
}

class OnePage : IPage
{
    public string Description => $"Entered Text: {_text}";
    private string _text = string.Empty;

    [OnCommandEvent("back")]
    [OnButtonCallbackEvent("back", 0, 0)]
    public PageResult ToPreviousPage() => PageResult.ToPreviousPage();

    [OnMessageEvent(MessageTypeFlags.Text | MessageTypeFlags.Photo)]
    public PageResult OnEnterText(Message msg)
    {
        switch (msg.Type)
        {
            case Telegram.Bot.Types.Enums.MessageType.Text:
                _text = msg.Text ?? string.Empty;
                break;
            case Telegram.Bot.Types.Enums.MessageType.Photo:
                _text = msg.Caption ?? string.Empty;
                break;
        }
        return PageResult.UpdateThisPage(PageResult.DefaultOptionsNoUseUpdateStub);
    }
    public void Dispose() { }
}