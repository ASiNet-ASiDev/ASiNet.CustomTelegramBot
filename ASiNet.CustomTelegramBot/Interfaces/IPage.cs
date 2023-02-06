using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot.Interfaces;
public interface IPage : IDisposable
{
    public string Description { get; }

}
