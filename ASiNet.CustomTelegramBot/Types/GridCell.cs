using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot.Types;
public struct GridCell
{
    public GridCell(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public int Row { get; set; }
    public int Column { get; set; }
}
