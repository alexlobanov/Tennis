using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.Events;

namespace Tennis_Betfair
{
    public class Market
    {
        private string marketName;
        private Player player1;
        private Player player2;

        private string betfairEventId;
        private string bet365EventId;
  
        public string MarketName => marketName;
        public Player Player1 => player1;
        public Player Player2 => player2;

        public static event MarketChangedEventHandler MarketChanged;
        public delegate void MarketChangedEventHandler(MarketUpdEventArgs market);


        public string BetfairEventId
        {
            get { return betfairEventId; }
            set { betfairEventId = value; }
        }

        public string Bet365EventId
        {
            get { return bet365EventId; }
            set { bet365EventId = value; }
        }

        public string GetBetFairScore()
        {
            return player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
        }

        public string GetBet365Score()
        {
            return player1.ScoreBet366 + " : " + player2.ScoreBet366;
        }

        public string GetNewScore()
        {
            return player1.ScoreNew + " : " + player2.ScoreNew;
        }

        public Market(string marketName, Player player1, Player player2, 
            string eventId, bool isBetfair)
        {

            this.marketName = marketName;
            this.player1 = player1;
            this.player2 = player2;
            if (isBetfair)
            {
                this.betfairEventId = eventId;
            }
            else
            {
                this.bet365EventId = eventId;
            }
            MarketChanged?.Invoke(new MarketUpdEventArgs(this));
        }
  
        
        public override string ToString()
        {
            return "Market: " + marketName + "===: " + player1 + " == " +  player2;
        }

       
    }
}
