using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot.Attributes;
[AttributeUsage(AttributeTargets.Method)]
public class CommandEventAttribute : Attribute
{
    public CommandEventAttribute(string command)
    {
        Command = command;
    }

    public string Command { get; set; }
}
