using ASiNet.CustomTelegramBot.Interfaces;
using ASiNet.CustomTelegramBot.Types;
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
        var container = new PageContainer(basePage, PageResult.DefaultOptions);
        _pages.Push(container);
    }

    private Stack<PageContainer> _pages;

    public PageContainer GetPage() => _pages.Peek();
    

    public PageContainer? PopPage()
    {
        if(_pages.Count > 1)
            return _pages.Pop();
        return null;
    }

    public void PushPage(PageContainer page) => _pages.Push(page);
    
    public void Dispose()
    {
        while (_pages.Count > 0)
        {
            _pages.Pop().Dispose();
        }
    }

    public static Navigator operator +(Navigator nav, PageContainer page)
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
