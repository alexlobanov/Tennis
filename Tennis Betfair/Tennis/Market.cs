using System;
using System.Diagnostics;
using System.Linq;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;

namespace Tennis_Betfair
{
    public class Market
    {
        public delegate void LoadedEventHandler(LoadedEventArgs loadedEvent);

        public delegate void MarketChangedEventHandler(MarketUpdEventArgs market);

        private DateTime _bet365Span;
        private DateTime _betfairSpan;
        private DateTime _skyBetSpan;

        private string _prevSkybetScore;
        private string _prevBet365Score;
        private string _prevBetfairScore;

        private string _prevNewScore;

        public Market(string marketName, Player player1, Player player2,
            string eventId, TypeDBO typeDbo)
        {
            MarketName = marketName;
            Player1 = player1;
            Player2 = player2;
            switch (typeDbo)
            {
                    case TypeDBO.SkyBet:
                        SkyBetEventId = eventId;
                    break;
                    case TypeDBO.Bet365:
                        Bet365EventId = eventId;
                    break;
                    case TypeDBO.BetFair:
                        BetfairEventId = eventId;
                    break;
            }     
            Player1.PlayerHanlder += Player1OnPlayerHanlder;
            Player2.PlayerHanlder += Player2OnPlayerHanlder;

            MarketChanged?.Invoke(new MarketUpdEventArgs(this));
        }


        public string ScoreNewOne { get; private set; }

        public string ScoreNewTwo { get; private set; }

        public bool IsClose { get; set; } = false;

        public string MarketName { get; set; }

        public Player Player1 { get; }

        public Player Player2 { get; }

        /*Event id*/
        public string BetfairEventId { get; set; }

        public string Bet365EventId { get; set; }

        public string SkyBetEventId { get; set; }
        /*End Event id*/

        public static event LoadedEventHandler LoadedEvent;

        public static event MarketChangedEventHandler MarketChanged;
        private DateTime betfairScoreTime;
        private DateTime skyBetScoreTime;
        private DateTime bet365ScoreTime;

        private string prevScoreBetfair;
        private string prevScore365;
        private string prevScoreSkyBet;

        private static readonly string NO_SCORE = "No score";

        /// <summary>
        /// Получает строку со счётом с betfair
        /// </summary>
        /// <returns>Счёт</returns>
        public string GetBetfairS()
        {
            var newScore = Player1.ScoreBetfair1 + " : " + Player2.ScoreBetfair1;
            if ((prevScoreBetfair == null) || (!prevScoreBetfair.Equals(newScore)) || (prevScoreBetfair == " : "))
            {
                if ((betfairScoreTime.AddSeconds(2).CompareTo(DateTime.Now) <= 0) || (betfairScoreTime == DateTime.MinValue))
                {
                    Debug.WriteLine("[BetFair]: " + newScore);
                    betfairScoreTime = DateTime.Now;
                    prevScoreBetfair = newScore;
                    return prevScoreBetfair;
                }
            }
            return string.IsNullOrEmpty(prevScoreBetfair) ? NO_SCORE : prevScoreBetfair;
        }

        /// <summary>
        /// Получает строку со счётом с skybet
        /// </summary>
        /// <returns>Счёт</returns>
        public string GetSkyBetS()
        {
            var newScore = Player1.ScoreSkyBet + " : " + Player2.ScoreSkyBet;
            if ((prevScoreSkyBet == null) || (!prevScoreSkyBet.Equals(newScore)) || (prevScoreSkyBet == " : "))
            {
                if ((skyBetScoreTime.AddSeconds(2).CompareTo(DateTime.Now) <= 0) || (skyBetScoreTime == DateTime.MinValue))
                {
                    Debug.WriteLine("[SkyBet]: " + newScore);
                    skyBetScoreTime = DateTime.Now;
                    prevScoreSkyBet = newScore;
                    return prevScoreSkyBet;
                }
            }
            return string.IsNullOrEmpty(prevScoreSkyBet) ? NO_SCORE : prevScoreSkyBet;
        }
           /// <summary>
        /// Получает строку со счётом с 365
        /// </summary>
        /// <returns>Счёт</returns>
        public string Get365S()
        {
            var newScore = Player1.ScoreBet366 + " : " + Player2.ScoreBet366;
            if ((prevScore365 == null) || (!prevScore365.Equals(newScore)) || (prevScore365 == " : "))
            {
                if ((bet365ScoreTime.AddSeconds(2).CompareTo(DateTime.Now) <= 0) || (bet365ScoreTime == DateTime.MinValue))
                {
                    Debug.WriteLine("[Get365]: " + newScore);
                    bet365ScoreTime = DateTime.Now;
                    prevScore365 = newScore;
                    return prevScore365;
                }
            }
            return string.IsNullOrEmpty(prevScore365) ? NO_SCORE : prevScore365;
        }

        /// <summary>
        /// Итоговый счёт со всех бирж
        /// </summary>
        /// <returns>Счёт со всех бирж</returns>
        public string GetNewS()
        {
            var max = bet365ScoreTime;
            TypeDBO typeMax = TypeDBO.BetFair;
            if (max.CompareTo(bet365ScoreTime) < 0)
            {
                max = bet365ScoreTime;
                typeMax = TypeDBO.Bet365;
            }
            if (max.CompareTo(skyBetScoreTime) < 0)
            {
                max = skyBetScoreTime;
                typeMax = TypeDBO.SkyBet;
            }
            var result = "";
            switch (typeMax)
            {
                case TypeDBO.Bet365:
                    result = Get365S();
                break;
                case TypeDBO.BetFair:
                    result = GetBetfairS();
                break;
                case TypeDBO.SkyBet:
                    result = GetSkyBetS();
                break;
            }
            return result;
        }

      
    
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

        private void UpdateScore(string score, int player)
        {
        }


        public override string ToString()
        {
            return Player1 + " : " + Player2 + " market:" + MarketName;
        }
    }
}