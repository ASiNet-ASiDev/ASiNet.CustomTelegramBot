using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot.Types.Base;

internal class Column
{
    public Column(int index, string text, string name)
    {
        ColumnIndex = index;
        Button = InlineKeyboardButton.WithCallbackData(text, name);
    }

    public int ColumnIndex { get; }

    public InlineKeyboardButton Button { get; }
}
