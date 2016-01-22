using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO.BetFair.GetMarkets;

namespace Tennis_Betfair.DBO.Abstract
{
    public interface IExchange
    {
        List<GetMarketData> GetInPlayAllMarkets();
    }
}
