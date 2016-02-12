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
            allMarkets.GetAllMarkets(TypeDBO.BetFair);
            if (allMarkets.ParsingInfo.AllMarketsHashSet == null) Assert.Fail("Null information from Betfair");
            if (allMarkets.ParsingInfo.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Betfair");
            Debug.WriteLine("Count get's elems from bet365 = " + allMarkets.ParsingInfo.AllMarketsHashSet.Count);
        }

        [TestMethod]
        public void TestGetAllScoresBetfair()
        {
            var allMarkets = new AllMarkets();
            allMarkets.GetAllMarkets(TypeDBO.BetFair);
            if (allMarkets.ParsingInfo.AllMarketsHashSet == null) Assert.Fail("Null information from Betfair");
            if (allMarkets.ParsingInfo.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Betfair");
            foreach (var market in allMarkets.ParsingInfo.AllMarketsHashSet)
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
