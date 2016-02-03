using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.Bet365;
using Tennis_Betfair.TO.BetFair.GetMarkets;
using Tennis_Betfair.TO.BetFair.GetScore;

namespace Tennis_Betfair.Tennis
{
    public class AllMarkets
    {
        public delegate void LoadedEventHandler(LoadedEventArgs loadedEvent);

        public delegate void PlayerChanged(ScoreUpdEventArgs player);

        private static readonly object ObjectTolock = new object();
        private static readonly object LockMarkets = new object();
        private readonly HashSet<Market> _allMarkets;
        private readonly Bet365Class _bet365Class;
        private readonly Betfair _betfair;

        private readonly ThreadControl threadControl;

        /// <summary>
        ///     Events end;
        /// </summary>
        public AllMarkets()
        {
            _betfair = new Betfair();
            _bet365Class = new Bet365Class();

            var comparator = new MarketEqualityComparer();

            _allMarkets = new HashSet<Market>(comparator);
            threadControl = new ThreadControl(this);
        }

        public HashSet<Market> AllMarketsHashSet
        {
            get
            {
                lock (LockMarkets)
                {
                    return _allMarkets;
                }
            }
        }

        /// <summary>
        ///     Events start
        /// </summary>
        public static event PlayerChanged playerChanged;

        public static event LoadedEventHandler LoadedEvent;

        public void StartThreads()
        {
            var thread = new Thread(() =>
            {
                LoadedEvent?.Invoke(new LoadedEventArgs(true, false));
                threadControl.Get365All();
                threadControl.GetBetfairAll();
                LoadedEvent?.Invoke(new LoadedEventArgs(false, true));
            });
            thread.Start();
        }

        public void AbortThreads()
        {
            threadControl.AbortThreads();
        }

        public void MarketIgnore(int eventId)
        {
            threadControl.MarketIgnore(eventId);
        }


        public ThreadStatus GetStatus()
        {
            try
            {
                return threadControl.GetStatus();
            }
            catch (Exception)
            {
                /**/
                return null;
            }
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
                var betfairAll = _betfair.GetInPlayAllMarkets();
                if (betfairAll == null) return false;
                ParseDate(betfairAll);
                return true;
            }
            var bet365All = _bet365Class.GetInPlayAllMarkets();
            if (bet365All == null) return false;
            ParseDate(bet365All);
            return true;
        }


        public bool GetScoreMarket(string eventId, bool isBetfair)
        {
            if (isBetfair)
            {
                var betfairReturn =
                    _betfair.GetScoreEvent(long.Parse(eventId));
                if (betfairReturn.Count == 0) return false;
                if (betfairReturn[0].matchStatus == null) return false;
                ParseDate(betfairReturn);
                return true;
            }
            var bet365Return =
                _bet365Class.GetScoreEvent(eventId);
            if (bet365Return == null) return false;
            ParseDate(bet365Return);
            return true;
        }

        public void GetScoreMarket(string eventIdBetfair, string eventId365)
        {
            if (eventIdBetfair != null)
            {
                var betfairReturn =
                    _betfair.GetScoreEvent(long.Parse(eventIdBetfair));
                ParseDate(betfairReturn);
            }
            if (eventId365 != null)
            {
                var bet365Return = _bet365Class.GetScoreEvent(eventId365);
                ParseDate(bet365Return);
            }
        }

        private void ParseDate(object objectToParse)
        {
            if (objectToParse is List<Event>)
            {
                ParseAllData((List<Event>) objectToParse);
            }
            if (objectToParse is List<GetMarketData>)
            {
                ParseAllData((List<GetMarketData>) objectToParse);
            }
            if (objectToParse is List<GetScore>)
            {
                lock (ObjectTolock)
                {
                    ParseAllData((List<GetScore>) objectToParse);
                }
            }
            if (objectToParse is Event)
            {
                lock (ObjectTolock)
                {
                    ParseAllData((Event) objectToParse);
                }
            }
        }

        /// <summary>
        ///     Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="betfairAllDatas"></param>
        private void ParseAllData(List<GetMarketData> betfairAllDatas)
        {
            var count = 0;
            try
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
                        var flag = false;
                        foreach (var allMarket in AllMarketsHashSet.Where(allMarket
                            =>
                        {
                            return allMarket.Player2.Name != null &&
                                   (allMarket.Player1.Name != null && ((allMarket.Player1.Name.Equals(player1))
                                                                       || (allMarket.Player2.Name.Equals(player2))));
                        }))
                        {
                            flag = true;
                            allMarket.MarketName = name;
                            allMarket.Player1.Name = player1;
                            allMarket.Player2.Name = player2;
                            allMarket.BetfairEventId = marketIdBetfair;
                           
                        }
                        if (!flag)
                        {
                            AllMarketsHashSet.Add(
                                new Market(name,
                                    new Player(player1, null, true),
                                    new Player(player2, null, true), marketIdBetfair, true)
                                );
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Exeption in parse: " + ex.Message);
                        /*ignored*/
                    }
                }
                count = 0;
            }
            catch (Exception)
            {
                count++;
                if (count >= 20)
                {
                    throw new Exception("Ошибка в обработке маркетов");
                }
            }
        }

        private void ParseAllData(List<Event> b365AllData)
        {
            if (b365AllData == null) throw new ArgumentNullException(nameof(b365AllData));
            var count = 0;
            try
            {
                foreach (var b365Data in b365AllData)
                {
                    var name = b365Data.CompetitionType;
                    var player1 = b365Data.Team1.getName();
                    var player2 = b365Data.Team2.getName();
                    var score1 = b365Data.Team1.getScore();
                    var score2 = b365Data.Team2.getScore();
                    var eventId = b365Data.EventId;
                    var flag = false;
                    foreach (var allMarket in AllMarketsHashSet.Where(allMarket
                        => allMarket.Player2.Name != null &&
                           (allMarket.Player1.Name != null &&
                            ((allMarket.Player1.Name.Equals(player1))
                             || (allMarket.Player2.Name.Equals(player2))))))
                    {
                        flag = true;
                        allMarket.MarketName = name;
                        allMarket.Player1.Name = player1;
                        allMarket.Player2.Name = player2;
                        allMarket.Bet365EventId = eventId;
                       
                    }
                    if (!flag)
                    {
                        AllMarketsHashSet.Add(
                            new Market(name,
                                new Player(player1, score1, false),
                                new Player(player2, score2, false), eventId, false)
                            );
                    }
                }
                count = 0;
            }
            catch (Exception)
            {
                count++;
                if (count >= 20)
                {
                    throw new Exception("Ошибка в обработке маркетов");
                }
            }
        }

        private void ParseAllData(List<GetScore> betfairSingleData)
        {
            var count = 0;
            try
            {
                if (betfairSingleData == null) throw new ArgumentNullException(nameof(betfairSingleData));
                if (betfairSingleData.Count == 0) return;
                var eventId = betfairSingleData[0].eventId;
                foreach (var allMarket in AllMarketsHashSet)
                {
                    if (allMarket.BetfairEventId != null && eventId == int.Parse(allMarket.BetfairEventId))
                    {
                        allMarket.Player1.ScoreBetfair1 =
                            betfairSingleData[0].score.home.score;
                        allMarket.Player2.ScoreBetfair1 =
                            betfairSingleData[0].score.away.score;
                        allMarket.Player2.Name =
                            betfairSingleData[0].score.away.name;
                        allMarket.Player1.Name =
                            betfairSingleData[0].score.home.name;


                        if (betfairSingleData[0].matchStatus.ToLower() == "finished")
                            allMarket.IsClose = true;
                        playerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
                    }
                }
                count = 0;
            }
            catch (Exception)
            {
                count++;
                if (count >= 20)
                {
                    throw new Exception("Ошибка в обработке маркетов");
                }
            }
        }

        private void ParseAllData(Event b365SingleData)
        {
            var count = 0;
            try
            {
                var eventId = b365SingleData.EventId;
                foreach (var allMarket in AllMarketsHashSet)
                {
                    if (allMarket.Bet365EventId != null
                        && allMarket.Bet365EventId.Equals(eventId))
                    {
                        allMarket.Player1.ScoreBet366 =
                            b365SingleData.Team1.getScore();
                        allMarket.Player2.ScoreBet366 =
                            b365SingleData.Team2.getScore();
                        allMarket.MarketName = b365SingleData.CompetitionType;

                        playerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
                    }
                }
                count = 0;
            }
            catch (Exception)
            {
                count++;
                if (count >= 20)
                {
                    throw new Exception("Ошибка в обработке маркетов");
                }
            }
        }

        private class MarketEqualityComparer : IEqualityComparer<Market>
        {
            public bool Equals(Market b1, Market b2)
            {
                if ((b1.Player1.Name == null) && (b2.Player1.Name == null))
                    return true;
                if (b1 == null | b2 == null)
                    return false;
                if (b1.Player1.Name == null | b2.Player1.Name == null)
                    return false;
                if ((b1.Player1.Name.Equals(b2.Player1.Name)) || (b1.Player2.Name.Equals(b2.Player2.Name)))
                {
                    if (b1.Bet365EventId == null)
                        b1.Bet365EventId = b2.Bet365EventId;
                    if (b1.BetfairEventId == null)
                        b1.BetfairEventId = b2.BetfairEventId;
                    Debug.WriteLine("Одинаковые: " + b1.Player1.Name + " : " + b1.Player2.Name + " Idb: " +
                                    b1.BetfairEventId + " 2: " + b1.Bet365EventId);
                    return true;
                }
                return false;
            }

            public int GetHashCode(Market obj)
            {
                if (obj.MarketName == null) return 1;
                var hCode = 31 ^ obj.MarketName[0]*4 ^ obj.MarketName[1]*5;
                if (obj.Player1.Name != null)
                {
                    hCode += obj.Player1.Name[0]*obj.Player1.Name[1]*obj.Player2.Name[2];
                }
                if (obj.Player2.Name != null)
                {
                    hCode += obj.Player2.Name[0]*obj.Player2.Name[1]*obj.Player2.Name[2];
                }
                return hCode;
            }
        }
    }
}