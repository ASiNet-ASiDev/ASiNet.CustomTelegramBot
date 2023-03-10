using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot.Types.Base;

internal class Row
{
    public Row(int index)
    {
        RowIndex = index;
        Columns = new();
    }
    public int RowIndex { get; }

    public List<Column> Columns { get; }

    public void AddItem(int column, string name, string path)
    {
        var item = Columns.FirstOrDefault(x => x.ColumnIndex == column);
        if (item is null)
        {
            var c = new Column(column, name, path);
            Columns.Add(c);
            return;
        }
        var co = new Column(column++, name, path);
        Columns.Add(co);
        return;
    }


    public InlineKeyboardButton[] GetButtons()
    {
        var result = Columns.OrderBy(x => x.ColumnIndex)
            .Select(x => x.Button)
            .ToArray();

        return result;
    }

}