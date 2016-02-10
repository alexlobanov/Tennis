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

        /// <summary>
        /// Получает строку со счётом с betfair
        /// </summary>
        /// <returns>Счёт с betfair</returns>
        public string GetBetFairScore()
        {
            var newScore = Player1.ScoreBetfair1 + " : " + Player2.ScoreBetfair1;
            if ((_prevBetfairScore == null) || (!_prevBetfairScore.Equals(newScore)) || _prevBetfairScore == " : ")
            {
                Debug.WriteLine("[Betfair] " + newScore);
                if ((!(DateTime.Now.CompareTo(_betfairSpan.AddSeconds(2)) <= 0)) || _betfairSpan == DateTime.MinValue)
                {
                  
                    _betfairSpan = DateTime.Now;
                    _prevBetfairScore = Player1.ScoreBetfair1 + " : " + Player2.ScoreBetfair1;
                }
            }
            return Player1.ScoreBetfair1 + " : " + Player2.ScoreBetfair1;
        }

     

        /// <summary>
        /// Получает строку со счётом с bet365
        /// </summary>
        /// <returns>Счёт</returns>
        public string GetBet365Score()
        {
            var newScore = Player1.ScoreBet366 + " : " + Player2.ScoreBet366;
            if ((_prevBet365Score == null) || (!_prevBet365Score.Equals(newScore)) || _prevBet365Score == " : ")
            {
                Debug.WriteLine("[Bet365] " + newScore);
                if ((!(DateTime.Now.CompareTo(_bet365Span.AddSeconds(2)) <= 0))
                    || (_bet365Span == DateTime.MinValue))
                {
                    _bet365Span = DateTime.Now;
                    _prevBet365Score = Player1.ScoreBet366 + " : " + Player2.ScoreBet366;
                }
            }
            return Player1.ScoreBet366 + " : " + Player2.ScoreBet366;
        }

        /// <summary>
        /// Получает строку со счётом с skybet
        /// </summary>
        /// <returns>Счёт</returns>
        public string GetSkyBetScore()
        {
            var newScore = Player1.ScoreSkyBet + " : " + Player2.ScoreSkyBet;
            if ((_prevSkybetScore == null) || (!_prevSkybetScore.Equals(newScore)) || _prevSkybetScore == " : ")
            {
                Debug.WriteLine("[Skybet] " + newScore);
                if ((!(DateTime.Now.CompareTo(_skyBetSpan.AddSeconds(2)) <= 0))
                    || (_skyBetSpan == DateTime.MinValue))
                {
                    _skyBetSpan = DateTime.Now;
                    _prevSkybetScore = Player1.ScoreSkyBet + " : " + Player2.ScoreSkyBet;
                }
            }
            return Player1.ScoreSkyBet + " : " + Player2.ScoreSkyBet;
        }

        /// <summary>
        /// Итоговый счёт со всех бирж
        /// </summary>
        /// <returns>Счёт со всех бирж</returns>

        public string GetNewScore()
        {
            var result = "";
            if ((_bet365Span.CompareTo(_betfairSpan) > 0))
            {
                result = _prevBet365Score;
                _prevNewScore = _prevBet365Score;
            }
            else if ((_bet365Span.CompareTo(_betfairSpan) < 0))
            {
                result = _prevBetfairScore;
                _prevNewScore = _prevBetfairScore;
            }
            else if (_prevNewScore != null)
            {
                result = _prevNewScore;
            }
            else result = "0 : 0";
            var scores = result.Split();
            if (scores.Count() == 3)
            {
                ScoreNewOne = scores[0].Trim();
                ScoreNewTwo = scores[2].Trim();
            }
            if (result == " : ")
            {
                var elem1 = GetBet365Score();
                var elem2 = GetBetFairScore();
                if (elem1 != " : ")
                    return elem1;
                return elem2;
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