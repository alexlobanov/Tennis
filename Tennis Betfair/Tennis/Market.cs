using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.Events;
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
            if ((prevBetfairScore == null) || (!prevBetfairScore.Equals(newScore)))
            {
                if (!(DateTime.Now.CompareTo(betfairSpan.AddSeconds(2)) <= 0))
                {
                    Debug.WriteLine("[Betfair] " + newScore);
                    betfairSpan = DateTime.Now;
                    prevBetfairScore = player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
                }
            }
            return player1.ScoreBetfair1 + " : " + player2.ScoreBetfair1;
        }

        public string GetBet365Score()
        {
            var newScore = player1.ScoreBet366 + " : " + player2.ScoreBet366;
            if ((prevBet365Score == null) || (!prevBet365Score.Equals(newScore)))
            {
                if (!(DateTime.Now.CompareTo(bet365Span.AddSeconds(2)) <= 0))
                {
                    Debug.WriteLine("[Bet365] " + newScore);
                    bet365Span = DateTime.Now;
                    prevBet365Score = player1.ScoreBet366 + " : " + player2.ScoreBet366;
                }
            }
            return player1.ScoreBet366 + " : " + player2.ScoreBet366;
        }

        public string GetNewScore()
        {
            string result = "";

            // if ((bet365Span.CompareTo(betfairSpan) > 0) && (validate(prevNewScore,prevBet365Score)))
            if ((bet365Span.CompareTo(betfairSpan) > 0))
            {
                result = prevBet365Score;
                prevNewScore = prevBet365Score;
            }
            // else if ((bet365Span.CompareTo(betfairSpan) < 0) && (validate(prevNewScore, prevBetfairScore)))
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

        private string oneBetfait;
        private string twoBetfair;
        private string one365;
        private string two365;
        private int countUpdate = 0;
        private void Player2OnPlayerHanlder(PlayerScoreUpdEventArgs scoreUpdEventArgs)
        {
            switch (scoreUpdEventArgs.TypeEx)
            {
                case TypeDBO.BetFair:
                    twoBetfair = scoreUpdEventArgs.Score;
                    UpdateScore(scoreUpdEventArgs.Score, 2);
                break;
                case TypeDBO.Bet365:
                    two365 = twoBetfair = scoreUpdEventArgs.Score;
                    UpdateScore(scoreUpdEventArgs.Score, 2);
                break;
            }
        }

        private void Player1OnPlayerHanlder(PlayerScoreUpdEventArgs scoreUpdEventArgs)
        {
            switch (scoreUpdEventArgs.TypeEx)
            {
                case TypeDBO.BetFair:
                    oneBetfait = twoBetfair = scoreUpdEventArgs.Score;
                    UpdateScore(scoreUpdEventArgs.Score, 1);
                    break;
                case TypeDBO.Bet365:
                    one365 = twoBetfair = scoreUpdEventArgs.Score;;
                    UpdateScore(scoreUpdEventArgs.Score, 1);
                    break;
            }

        }



       /* private bool validate(string prevScore, string newScore)
        {
            if (prevScore == null) return false;
            if ((newScore == "0:0") && (prevScore == "0:Adv")) return true;
            if ((newScore == "0:0") && (prevScore == "15:Adv")) return true;
            if ((newScore == "0:0") && (prevScore == "30:Adv")) return true;
            if ((newScore == "0:0") && (prevScore == "40:Adv")) return true;

            if ((newScore == "0:0") && (prevScore == "Adv:0")) return true;
            if ((newScore == "0:0") && (prevScore == "Adv:15")) return true;
            if ((newScore == "0:0") && (prevScore == "Adv:30")) return true;
            if ((newScore == "0:0") && (prevScore == "Adv:40")) return true;


            if ((newScore == "15:0") && (prevScore == "0:0")) return true;
            if ((newScore == "30:0") && (prevScore == "15:0")) return true;
            if ((newScore == "40:0") && (prevScore == "30:0")) return true;
            if ((newScore == "Adv:0") && (prevScore == "40:0")) return true;
            if ((newScore == "40:0") && (prevScore == "Adv:0")) return true;

            if ((newScore == "0:15") && (prevScore == "0:0")) return true;
            if ((newScore == "0:30") && (prevScore == "0:15")) return true;
            if ((newScore == "0:40") && (prevScore == "0:40")) return true;
            if ((newScore == "0:40") && (prevScore == "0:Adv")) return true;
            if ((newScore == "0:Adv") && (prevScore == "0:40")) return true;

            if ((newScore == "15:15") && (prevScore == "0:0")) return true;
            if ((newScore == "15:30") && (prevScore == "0:15")) return true;
            if ((newScore == "15:40") && (prevScore == "0:30")) return true;
            if ((newScore == "15:Adv") && (prevScore == "0:40")) return true;
            if ((newScore == "15:40") && (prevScore == "0:Adv")) return true;

            if ((newScore == "30:15") && (prevScore == "15:0")) return true;
            if ((newScore == "30:30") && (prevScore == "15:15")) return true;
            if ((newScore == "30:40") && (prevScore == "15:30")) return true;
            if ((newScore == "30:Adv") && (prevScore == "15:40")) return true;
            if ((newScore == "30:40") && (prevScore == "15:Adv")) return true;

            if ((newScore == "40:15") && (prevScore == "30:0")) return true;
            if ((newScore == "40:30") && (prevScore == "30:15")) return true;
            if ((newScore == "40:40") && (prevScore == "30:30")) return true;
            if ((newScore == "40:Adv") && (prevScore == "30:40")) return true;
            if ((newScore == "40:40") && (prevScore == "30:Adv")) return true;

            if ((newScore == "40:15") && (prevScore == "30:0")) return true;
            if ((newScore == "40:30") && (prevScore == "30:15")) return true;
            if ((newScore == "40:40") && (prevScore == "30:30")) return true;
            if ((newScore == "40:Adv") && (prevScore == "30:40")) return true;
            if ((newScore == "40:40") && (prevScore == "30:Adv")) return true;
            return false;
        }*/

        public void updateFirstScore()
        {
            isFirstUpdScoreTwo = true;
            isFirstUpdScoreTwo = true;
        }
        private void UpdateScore(string score, int player)
        {
           /* switch (player)
            {
                    
            }
            if (betfairSpan.CompareTo(bet365Span) > 0)
            {
                scoreNewOne    
            }*/

            /*
            switch (player)
            {
                case 1:

                    if (isFirstUpdScoreOne)
                    {
                        isFirstUpdScoreOne = false;
                        scoreNewPrevOne = score;
                        scoreNewOne = score;
                        return;
                    }
                    if (!score.Equals(scoreNewPrevOne))
                    {
                        if (!validate(scoreNewPrevOne, score)) return;
                        Debug.WriteLine("Date:" + DateTime.Now.ToShortTimeString() + "[1Player] score: " + score);
                        scoreNewOne = score;
                        scoreNewPrevOne = score;
                    }
                    break;
                case 2:

                    if (isFirstUpdScoreTwo)
                    {
                        isFirstUpdScoreTwo = false;
                        scoreNewPrevTwo = score;
                        scoreNewTwo = score;
                        return;
                    }
                    if (!score.Equals(scoreNewPrevTwo))
                    {
                        if (!validate(scoreNewPrevOne, score)) return;
                        Debug.WriteLine("Date:" + DateTime.Now.ToShortTimeString() + "[2Player] score: " + score);
                        scoreNewPrevTwo = score;
                        scoreNewTwo = score;
                    }
                    break;
            }*/
        }

        /*
        private bool validateScore(string prevScore,string newScoreOnet, string newScoreTwo, int player)
        {
            switch ()
            {
                case "0":
                    break;
                case "15":
                    break;
                case "30":

                    break;
                case "40":

                    break;
                case "Adv":

                    break;
            }
            switch (player)
            {
                case 1:
                    
                    break;
                case 2:

                    break;
            }
        }*/


        public override string ToString()
        {
            return player1 +" : " + player2 + " market:" + marketName;
        }

       
    }
}
