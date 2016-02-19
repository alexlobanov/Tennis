using System;
using System.Diagnostics;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;

namespace UnitTest
{
    [TestClass]
    public class Bet365Test
    {
        [TestMethod]
        public void TestGetAllInformationBet365()
        {
            var startTime = DateTime.Now;
            var allMarkets = new AllMarkets();
            allMarkets.GetAllMarkets(TypeDBO.Bet365);
            if (allMarkets.ParsingInfo.AllMarketsHashSet == null) Assert.Fail("Null information from Bet365");
            if (allMarkets.ParsingInfo.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Bet365");
            var endTime = DateTime.Now;
            Debug.WriteLine("Time for get info: " + (startTime-endTime).TotalSeconds);
            Debug.WriteLine("Count get's elems from bet365 = " + allMarkets.ParsingInfo.AllMarketsHashSet.Count);
        }

        [TestMethod]
        public void TestGetAllScoreBet365()
        {
            var startTime = DateTime.Now;
            var allMarkets = new AllMarkets();
            //var threadControl = new ThreadControl(allMarkets);
            allMarkets.GetAllMarkets(TypeDBO.Bet365);
            if (allMarkets.ParsingInfo.AllMarketsHashSet == null) Assert.Fail("Null information from Bet365");
            if (allMarkets.ParsingInfo.AllMarketsHashSet.Count == 0) Assert.Fail("(Count = 0) information from Bet365");
            foreach (var market in allMarkets.ParsingInfo.AllMarketsHashSet)
            {
                if (market.MarketName == null)
                {
                    Debug.WriteLine("MarketName is null!!! ");
                    continue;
                }
                if (market.Bet365EventId == null)
                {
                    Debug.WriteLine("Empty Event id, market: " + market.MarketName);
                    continue;
                }
                allMarkets.GetScoreMarket(market.Bet365EventId, TypeDBO.Bet365);
                if ((market.Player1.ScoreBet366 == null) && (market.Player1.ScoreBet366 == ""))
                    Assert.Fail("Score error from bet365");
                if ((market.Player2.ScoreBet366 == null) && (market.Player2.ScoreBet366 == ""))
                    Assert.Fail("Score error from bet365");
                Thread.Sleep(100);
            }
            var endTime = DateTime.Now;
            Debug.WriteLine("Time for get info: " + (startTime - endTime).TotalMilliseconds);
        }
    }
}
