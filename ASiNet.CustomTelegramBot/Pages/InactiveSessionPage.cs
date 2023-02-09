using ASiNet.CustomTelegramBot.Interfaces;

namespace ASiNet.CustomTelegramBot.Pages
{
    internal class InactiveSessionPage : ITextPage
    {
        public string Text => "Сессия остановлена.\nНапишите /start что бы начать новую сессию.";
        public void Dispose()
        { }
    }
}
