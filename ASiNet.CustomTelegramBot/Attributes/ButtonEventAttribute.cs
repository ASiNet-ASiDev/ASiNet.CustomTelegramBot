using ASiNet.CustomTelegramBot.Types;

namespace ASiNet.CustomTelegramBot.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class ButtonEventAttribute : Attribute
{
    public ButtonEventAttribute(string text, int row, int column)
    {
        Text = text;
        Cell = new(row, column);
    }
    public string Text { get; set; }
    public GridCell Cell { get; set; }
}
