using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot.Enums;
public enum ResultOptions : long
{
    None = 0,
    UseUpdateStub = 1 << 0,
    EditLastMessage = 1 << 1,

}
