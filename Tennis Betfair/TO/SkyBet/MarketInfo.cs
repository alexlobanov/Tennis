using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO.SkyBet
{
    public class MarketInfo
    {
        private string marketName;
        private string player1;
        private string player2;
        private string eventId;

        public MarketInfo(string marketName, string player1, string player2,string eventId)
        {
            this.marketName = marketName;
            this.player1 = player1;
            this.player2 = player2;
            this.eventId = eventId;
        }

        public string EventId
        {
            get { return eventId; }
        }

        public MarketInfo()
        {
            //ignored;
        }

        public string MarketName
        {
            get { return marketName; }
        }

        public string Player1
        {
            get { return player1; }
        }

        public string Player2
        {
            get { return player2; }
        }
    }
}
