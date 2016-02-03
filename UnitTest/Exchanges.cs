using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair.DBO;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.Tennis;

namespace UnitTest
{
    [TestClass]
    public class Exchanges
    {
        [TestMethod]
        public void TestGetAllInformationBet365()
        {
            var allMarkets = new AllMarkets(); 
            var threadControl = new ThreadControl(allMarkets);
            threadControl.Get365All();
            if (allMarkets.AllMarketsHashSet == null) Assert.Fail("Null information from Bet365");
            if (allMarkets.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Bet365");
            Debug.WriteLine("Count get's elems from bet365 = " + allMarkets.AllMarketsHashSet.Count);
        }

        [TestMethod]
        public void TestGetAllInfoBetfair()
        {
            var allMarkets = new AllMarkets();
            var threadControl = new ThreadControl(allMarkets);
            threadControl.GetBetfairAll();
            if (allMarkets.AllMarketsHashSet == null) Assert.Fail("Null information from Betfair");
            if (allMarkets.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Betfair");
            Debug.WriteLine("Count get's elems from bet365 = " + allMarkets.AllMarketsHashSet.Count);
        }
    }
}
