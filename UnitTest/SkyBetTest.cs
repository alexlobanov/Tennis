using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair.DBO;
using Tennis_Betfair.TO.SkyBet;

namespace UnitTest
{
    [TestClass]
    public class SkyBetTest
    {

        [TestMethod]
        public void TestScrapeTennisPage()
        {
            var skyBet = new SkyBet();
            var result = skyBet.GetMartches();
            if (result == null) Assert.Fail("No info [market == null]");
            if (result.Count == 0) Assert.Fail("No elems from parse. Maybee no inplay games?");
            foreach (var marketInfo in result)
            {
                if (marketInfo.Player1 == null) Assert.Fail("No info about player1 in some markets");
                if (marketInfo.Player2 == null) Assert.Fail("No info about player2 in some markets");
                if (marketInfo.MarketName.Length < 4) Assert.Fail("No info about player1 in some markets");
            }
        }



        [TestMethod]
        public void TestScrapeScorePage()
        {
            var skybet = new SkyBet();
            var listMarkets = skybet.GetMartches();
            foreach (var listMarket in listMarkets)
            {
                var result = skybet.GetScoreInfo(listMarket.EventId);
                if (result.Player1 == null) Assert.Fail("No info about player1 in some markets");
                if (result.Player2 == null) Assert.Fail("No info about player2 in some markets");
                if ((result.ScoreFirst == null) || (result.ScoreFirst == ""))
                    Assert.Fail("No info about score one in some markets");
                if ((result.ScoreSecond == null) || (result.ScoreSecond == ""))
                    Assert.Fail("No info about score one in some markets");

            }
        }
    }
}
