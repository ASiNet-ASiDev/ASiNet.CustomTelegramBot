using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot.Types;

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
