using ASiNet.CustomTelegramBot.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASiNet.CustomTelegramBot;
public class Navigator : IDisposable
{
    public Navigator(IPage basePage)
    {
        _pages = new();
        _pages.Push(basePage);
    }

    private Stack<IPage> _pages;

    public IPage GetPage() => _pages.Peek();
    

    public IPage? PopPage()
    {
        if(_pages.Count > 1)
            return _pages.Pop();
        return null;
    }

    public void PushPage(IPage page) => _pages.Push(page);
    
    public void Dispose()
    {
        while (_pages.Count > 0)
        {
            _pages.Pop().Dispose();
        }
    }

    public static Navigator operator +(Navigator nav, IPage page)
    {
        nav.PushPage(page);
        return nav;
    }

    public static Navigator operator --(Navigator nav)
    {
        nav.PopPage();
        return nav;
    }
}
