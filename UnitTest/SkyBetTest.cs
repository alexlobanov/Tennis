using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.SkyBet;

namespace UnitTest
{
    [TestClass]
    public class SkyBetTest
    {

        [TestMethod]
        public void TestScrapeTennisPageSkybet()
        {
            var allmarkets = new AllMarkets();
            var result = allmarkets.GetAllMarkets(TypeDBO.SkyBet);
            if (result == false) Assert.Fail("No info [market == null]");
            if (allmarkets.AllMarketsHashSet.Count == 0) Assert.Fail("No elems from parse. Maybee no inplay games?");
            foreach (var marketInfo in allmarkets.AllMarketsHashSet)
            {
                if (marketInfo.Player1 == null) Assert.Fail("No info about player1 in some markets");
                if (marketInfo.Player2 == null) Assert.Fail("No info about player2 in some markets");
                if (marketInfo.MarketName.Length < 4) Assert.Fail("No info about player1 in some markets");
            }
            Debug.WriteLine("Elems from skyBet: " + allmarkets.AllMarketsHashSet.Count);
        }



        [TestMethod]
        public void TestScrapeScorePageSkyBet()
        {
            var allmarkets = new AllMarkets();
            var result = allmarkets.GetAllMarkets(TypeDBO.SkyBet);
            if (result == false) Assert.Fail("No info [market == null]");
            if (allmarkets.AllMarketsHashSet.Count == 0) Assert.Fail("No elems from parse. Maybee no inplay games?");
            foreach (var marketInfo in allmarkets.AllMarketsHashSet)
            {
                if (marketInfo.SkyBetEventId == null) Assert.Fail("No info eventId from skybet");
                var statusScore = allmarkets.GetScoreMarket(marketInfo.SkyBetEventId, TypeDBO.SkyBet);
                if (statusScore == false)
                    Assert.Fail("No info result from skybet");
                if (marketInfo.Player1 == null) Assert.Fail("No info about player1 in some markets");
                if (marketInfo.Player1 == null) Assert.Fail("No info about player2 in some markets");
                if ((marketInfo.Player1.ScoreSkyBet == null) || (marketInfo.Player1.ScoreSkyBet == ""))
                {
                    Debug.WriteLine("ScoreBet366:" + marketInfo.Player2.ScoreBet366);
                    Debug.WriteLine("ScoreBetfair:" + marketInfo.Player2.ScoreBetfair1);
                    Assert.Fail("No info about score1 one in some markets");
                }
                if ((marketInfo.Player2.ScoreSkyBet == null) || (marketInfo.Player2.ScoreSkyBet == ""))
                {
                    Debug.WriteLine("ScoreBet366:" + marketInfo.Player2.ScoreBet366);
                    Debug.WriteLine("ScoreBetfair:" + marketInfo.Player2.ScoreBetfair1);
                    Assert.Fail("No info about score2 one in some markets");
                }
                if (marketInfo.MarketName.Length < 4) Assert.Fail("No info about player1 in some markets");
            }
            Debug.WriteLine("In-plays markets: " + allmarkets.AllMarketsHashSet.Count);

          
        }
    }
}
