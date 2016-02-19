using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.NewBet365;

namespace Tennis_Betfair.DBO
{
    public interface IBrowserParse
    {
        MarketStatus SuscribeToScore(string competition);
        bool ClickToMarket(int index);
        List<Mathes> GetAlllMathes();
        List<Scores> GetCurrentScoreses();
    }
}
