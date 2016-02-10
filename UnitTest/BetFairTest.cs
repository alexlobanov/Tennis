using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;

namespace UnitTest
{
    [TestClass]
    public class BetFairTest
    {
        [TestMethod]
        public void TestGetAllInfoBetfair()
        {
            var allMarkets = new AllMarkets();
            var threadControl = new ThreadControl(allMarkets);
            threadControl.GetAll(TypeDBO.BetFair);
            if (allMarkets.AllMarketsHashSet == null) Assert.Fail("Null information from Betfair");
            if (allMarkets.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Betfair");
            Debug.WriteLine("Count get's elems from bet365 = " + allMarkets.AllMarketsHashSet.Count);
        }

        [TestMethod]
        public void TestGetAllScoresBetfair()
        {
            var allMarkets = new AllMarkets();
            var threadControl = new ThreadControl(allMarkets);
            threadControl.GetAll(TypeDBO.BetFair);
            if (allMarkets.AllMarketsHashSet == null) Assert.Fail("Null information from Betfair");
            if (allMarkets.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Betfair");
            foreach (var market in allMarkets.AllMarketsHashSet)
            {
                allMarkets.GetScoreMarket(market.BetfairEventId, TypeDBO.BetFair);
                if ((market.Player1.ScoreBetfair1 == null) && (market.Player1.ScoreBetfair1 == ""))
                    Assert.Fail("Score error from betfair");
                if ((market.Player2.ScoreBetfair1 == null) && (market.Player2.ScoreBetfair1 == ""))
                    Assert.Fail("Score error from betfair");
            }
        }
    }
}
