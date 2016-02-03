using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;

namespace Tennis_Betfair
{
    public class Market
    {
        private string marketName;
        private Player player1;
        private Player player2;

        private string betfairEventId;
        private string bet365EventId;

        private bool isClose = false;

        private string scoreNewOne;
        private string scoreNewTwo;
        private string scoreNewPrevOne;
        private string scoreNewPrevTwo;

        private string scoreBetOne;
        private string scoreBetTwo;

        private string score365One;
        private string score365two;
        private bool isFirstUpdScoreOne;
        private bool isFirstUpdScoreTwo;

        private DateTime betfairSpan;
        private DateTime bet365Span;
        private string prevBetfairScore;
        private string prevBet365Score;
        private string prevNewScore;


        public static event LoadedEventHandler LoadedEvent;
        public delegate void LoadedEventHandler(LoadedEventArgs loadedEvent);
        public string ScoreBetOne
        {
            get
            {
                return Player1.ScoreBetfair1;
            }
        }

        public string ScoreBetTwo
        {
            get { return Player2.ScoreBetfair1; }
        }

        public string Score365One
        {
            get { return Player1.ScoreBet366; }
        }

        public string Score365Two
        {
            get { return Player2.ScoreBet366; }
        }



        public string ScoreNewOne
        {
            get
            {
                return scoreNewOne;
            }
        }

        public string ScoreNewTwo
        {
            get
            {
                return scoreNewTwo;
            }
        }

        public bool IsClose
        {
            get { return isClose; }
            set { isClose = value; }
        }

        public string MarketName
        {
            get { return marketName; }
            set { marketName = value; }
        }

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
            var newScore = player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
            if ((prevBetfairScore == null) || (!prevBetfairScore.Equals(newScore)) || prevBetfairScore == " : ")
            {
                if (((DateTime.Now.CompareTo(betfairSpan.AddSeconds(2)) <= 0))|| betfairSpan == DateTime.MinValue)
                {
                    Debug.WriteLine("[Betfair] " + newScore);
                    betfairSpan = DateTime.Now;
                    prevBetfairScore = player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
                    isFirstUpdScoreTwo = false;
                }
            }
            return player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
        }

        public string GetBet365Score()
        {
            var newScore = player1.ScoreBet366 + " : " + player2.ScoreBet366;
            if ((prevBet365Score == null) || (!prevBet365Score.Equals(newScore)) || prevBet365Score == " : ")
            {
                if ((!(DateTime.Now.CompareTo(bet365Span.AddSeconds(2)) <= 0)) 
                    || (bet365Span == DateTime.MinValue))
                {
                    Debug.WriteLine("[Bet365] " + newScore);
                    bet365Span = DateTime.Now;
                    prevBet365Score = player1.ScoreBet366 + " : " + player2.ScoreBet366;
                    isFirstUpdScoreOne = false;
                }
            }
            return player1.ScoreBet366 + " : " + player2.ScoreBet366;
        }

        public string GetNewScore()
        {
            string result = "";
            if ((bet365Span.CompareTo(betfairSpan) > 0))
            {
                result = prevBet365Score;
                prevNewScore = prevBet365Score;
            }
            else if ((bet365Span.CompareTo(betfairSpan) < 0))
            {
                result = prevBetfairScore;
                prevNewScore = prevBetfairScore;
            }
            else if (prevNewScore != null)
            {
                result = prevNewScore;
            }
            else result = "0 : 0";
            var scores = result.Split();
            if (scores.Count() == 3)
            {
                scoreNewOne = scores[0].Trim();
                scoreNewTwo = scores[2].Trim();
            }
            if (result == " : ")
            {
               var elem1 = GetBet365Score();
               var elem2 = GetBetFairScore();
                if (elem1 != " : ")
                    return elem1;
                else return elem2;
            }
            return result;
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
            Player1.PlayerHanlder += Player1OnPlayerHanlder;
            Player2.PlayerHanlder += Player2OnPlayerHanlder;
            isFirstUpdScoreOne = true;
            isFirstUpdScoreTwo = true;
            MarketChanged?.Invoke(new MarketUpdEventArgs(this));
        }


        private int countUpdate = 0;
        private void Player2OnPlayerHanlder(PlayerScoreUpdEventArgs scoreUpdEventArgs)
        {
            switch (scoreUpdEventArgs.TypeEx)
            {
                case TypeDBO.BetFair:
                    UpdateScore(scoreUpdEventArgs.Score, 2);
                break;
                case TypeDBO.Bet365:
                    UpdateScore(scoreUpdEventArgs.Score, 2);
                break;
            }
        }

        private void Player1OnPlayerHanlder(PlayerScoreUpdEventArgs scoreUpdEventArgs)
        {
            switch (scoreUpdEventArgs.TypeEx)
            {
                case TypeDBO.BetFair:
                    UpdateScore(scoreUpdEventArgs.Score, 1);
                    break;
                case TypeDBO.Bet365:
                   UpdateScore(scoreUpdEventArgs.Score, 1);
                    break;
            }

        }



        public void updateFirstScore()
        {
            isFirstUpdScoreTwo = true;
            isFirstUpdScoreTwo = true;
        }
        private void UpdateScore(string score, int player)
        {
          
        }


        public override string ToString()
        {
            return player1 +" : " + player2 + " market:" + marketName;
        }

       
    }
}
