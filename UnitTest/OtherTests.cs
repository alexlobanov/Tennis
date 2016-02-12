using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tennis_Betfair;
using Tennis_Betfair.TO;

namespace UnitTest
{
    [TestClass]
    public class OtherTests
    {
        [TestMethod]
        public void TestConnection()
        {
            string message;
            if (CheckInternetConenction.CheckConnection(TypeDBO.Bet365, out message) != StatusInternet.NoAvirable)
            {
                Assert.Fail("No connection to bet365. " + message);
            }
            if (CheckInternetConenction.CheckConnection(TypeDBO.BetFair,out message) != StatusInternet.NoAvirable)
            {
                Assert.Fail("No connetion to betfair. " + message);
            }
            if (CheckInternetConenction.CheckConnection(TypeDBO.SkyBet, out message) != StatusInternet.NoAvirable)
            {
                Assert.Fail("No connection to skybet."  + message);
            }
        }
    }
}
