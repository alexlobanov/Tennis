using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.DBO;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO.Bet365;
using Tennis_Betfair.TO.BetFair.GetMarkets;
using Tennis_Betfair.TO.BetFair.GetScore;

namespace Tennis_Betfair.Tennis
{
    public class AllMarkets
    {
        private Betfair betfair;
        private Bet365Class bet365Class;

        private HashSet<Market> allMarkets;

        public HashSet<Market> AllMarketsHashSet => allMarkets;

        public ThreadControl threadControl;


        public static event PlayerChanged playerChanged;
        public delegate void PlayerChanged(ScoreUpdEventArgs player);

        public AllMarkets()
        {
            betfair = new Betfair();
            bet365Class = new Bet365Class();

            var comparator = new MarketEqualityComparer();

            allMarkets = new HashSet<Market>(comparator);
            threadControl = new ThreadControl(this);
        }

        public void StartThreads()
        {
            threadControl.StartThreads();
        }

        public void StopThreads()
        {
            threadControl.StopThread();
        }

        public void ChangeId(string eventId, bool isBetfair)
        {
            threadControl.ChangeEventId(eventId, isBetfair);            
        }

        public void ChangeId(string betFairEventId, string bet365Id)
        {
            threadControl.ChangeEventId(betFairEventId, bet365Id);
        }

        public bool GetAllMarkets(bool isBetfair)
        {
            if (isBetfair)
            {
                var betfairAll = betfair.GetInPlayAllMarkets();
                if (betfairAll == null) return false;
                ParseDate(betfairAll);
                return true;
            }
            else
            {
                var bet365All = bet365Class.GetInPlayAllMarkets();
                if (bet365All == null) return false;
                ParseDate(bet365All);
                return true;
            }
        }

        public void GetAllMarkets()
        {
            var bet365All = bet365Class.GetInPlayAllMarkets();
            var betfairAll = betfair.GetInPlayAllMarkets();

            ParseDate(bet365All);
            ParseDate(betfairAll);
        }

        public void GetScoreMarket(string eventId, bool isBetfair)
        {
            if (isBetfair)
            {
                var betfairReturn =
                    betfair.GetScoreEvent(long.Parse(eventId));
                ParseDate(betfairReturn);
            }
            else
            {
                var bet365Return =
                    bet365Class.GetScoreEvent(eventId);
                ParseDate(bet365Return);
            }
        }

        public void GetScoreMarket(string eventIdBetfair, string eventId365)
        {
            if (eventIdBetfair != null)
            {
                var betfairReturn =
                    betfair.GetScoreEvent(long.Parse(eventIdBetfair));
                ParseDate(betfairReturn);
            }
            if (eventId365 != null)
            {
                var bet365Return = bet365Class.GetScoreEvent(eventId365);
                ParseDate(bet365Return);
            }
        }

        private void ParseDate(object objectToParse)
        {
            lock (threadControl._lockAll)
            {
                if (objectToParse is List<Event>)
                {
                    ParseAllData((List<Event>)objectToParse);
                }
                if (objectToParse is List<GetMarketData>)
                {
                    ParseAllData((List<GetMarketData>)objectToParse);
                }
                if (objectToParse is List<GetScore>)
                {
                    ParseAllData((List<GetScore>)objectToParse);
                }
                if (objectToParse is Event)
                {
                    ParseAllData((Event)objectToParse);
                }
            }
        }

        /// <summary>
        /// Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="betfairAllDatas"></param>
        private void ParseAllData(List<GetMarketData> betfairAllDatas)
        {
            if (betfairAllDatas == null) throw new ArgumentNullException(nameof(betfairAllDatas));
            foreach (var betfairAllData in betfairAllDatas)
            {
                try
                {
                    var name = betfairAllData.competitionName;
                    var player1 = betfairAllData.runners.runner1Name;
                    var player2 = betfairAllData.runners.runner2Name;
                    var marketIdBetfair = betfairAllData.eventId.ToString();
                    allMarkets.Add(
                        new Market(name,
                            new Player(player1, null, true),
                            new Player(player2, null, true),marketIdBetfair,true)
                        );
                }
                catch (Exception ex )
                {
                    Debug.WriteLine("Exeption in parse: " + ex.Message);
                    /*ignored*/
                }
            }
        }

        private void ParseAllData(List<Event> b365AllData)
        {
            if (b365AllData == null) throw new ArgumentNullException(nameof(b365AllData));
            foreach (var b365Data in b365AllData)
            {
                var name = b365Data.CompetitionType;
                var player1 = b365Data.Team1.getName();
                var player2 = b365Data.Team2.getName();
                var score1 = b365Data.Team1.getScore();
                var score2 = b365Data.Team2.getScore();
                var eventId = b365Data.EventId;
                allMarkets.Add(
                    new Market(name,
                        new Player(player1, score1, false),
                        new Player(player2, score2, false), eventId, false)
                    );
            }
        }

        private void ParseAllData(List<GetScore> betfairSingleData)
        {
            if (betfairSingleData == null) throw new ArgumentNullException(nameof(betfairSingleData));
            if (betfairSingleData.Count == 0) return;
            var eventId = betfairSingleData[0].eventId;
            foreach (var allMarket in allMarkets)
            {
                if (allMarket.BetfairEventId != null && eventId == int.Parse(allMarket.BetfairEventId))
                {
                    allMarket.Player1.ScoreBetfair1 = 
                        betfairSingleData[0].score.home.score;
                    allMarket.Player2.ScoreBetfair1 =
                       betfairSingleData[0].score.away.score;
                    playerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
                }
            }
        }

        private void ParseAllData(Event b365SingleData)
        {
            var eventId = b365SingleData.EventId;
            foreach (var allMarket in allMarkets)
            {
                if (allMarket.Bet365EventId != null 
                    && allMarket.Bet365EventId.Equals(eventId))
                {
                    allMarket.Player1.ScoreBet366 = 
                        b365SingleData.Team1.getScore();
                    allMarket.Player2.ScoreBet366 = 
                        b365SingleData.Team2.getScore();
                    playerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
                }
            }
        }

        private class MarketEqualityComparer : IEqualityComparer<Market>
        {
            public bool Equals(Market b1, Market b2)
            {

                if (b2 == null && b1 == null)
                    return true;
                else if (b1 == null | b2 == null)
                    return false;
                else if (b1.Player1.Name.Equals(b2.Player1.Name) 
                    || (b2.Player2.Name.Equals(b1.Player2.Name)))
                {
                    if (b1.Bet365EventId == null)
                        b1.Bet365EventId = b2.Bet365EventId;
                    if (b1.BetfairEventId == null)
                        b1.BetfairEventId = b2.BetfairEventId;
                    return true;
                }
                else
                    return false;
            }

            public int GetHashCode(Market obj)
            {
                int hCode = 31 ^ obj.Player1.Name[0]*4 ^ obj.Player2.Name[0]*5 ^
                            obj.Player1.Name[0]*2 ^ obj.Player2.Name[0]*3;
                return hCode;
            }
        }
    }
}
