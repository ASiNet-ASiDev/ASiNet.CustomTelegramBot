using ASiNet.CustomTelegramBot.Interfaces;

namespace ASiNet.CustomTelegramBot.Pages;
public class DefaultBasePage : ITextPage
{
    public string Text => "Default base page!";

    public void Dispose()
    {

    }
}