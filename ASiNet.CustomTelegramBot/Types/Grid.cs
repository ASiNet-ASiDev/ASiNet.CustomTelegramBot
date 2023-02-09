using Telegram.Bot.Types.ReplyMarkups;

namespace ASiNet.CustomTelegramBot.Types;
internal class Grid
{
    public Grid()
    {
        Rows = new();
    }

    public List<Row> Rows { get; }


    public void AddItem(GridCell cell, string name, string path)
    {
        var item = Rows.FirstOrDefault(x => x.RowIndex == cell.Row);
        if (item is null)
        {
            var r = new Row(cell.Row);
            r.AddItem(cell.Column, name, path);
            Rows.Add(r);
            return;
        }
        item.AddItem(cell.Column, name, path);
    }

    public InlineKeyboardMarkup GetButtons()
    {
        var result = Rows.OrderBy(x => x.RowIndex).ToArray();

        var ikb = new InlineKeyboardButton[result.Length][];

        var y = 0;

        foreach (var row in result)
        {
            var temp = row.GetButtons();
            ikb[y] = temp;
            y++;
        }

        return new(ikb);
    }

}