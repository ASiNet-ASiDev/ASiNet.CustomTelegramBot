using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot;
public struct PageResultOptions
{
    public PageResultOptions()
    {
        UseUpdateStub = true;
        EditLastMessage = true;
    }

    public bool UseUpdateStub { get; set; }
    public bool EditLastMessage { get; set; }

    public static PageResultOptions Default() => new() { UseUpdateStub = true, EditLastMessage = true };

    public static PageResultOptions DefaultNoUseUpdateStub() => new() { UseUpdateStub = false, EditLastMessage = true };

    public static PageResultOptions DefaultNoUseEditLastMsg() => new() { UseUpdateStub = true, EditLastMessage = false };
}
