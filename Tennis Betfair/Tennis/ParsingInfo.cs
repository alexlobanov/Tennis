using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.Bet365;
using Tennis_Betfair.TO.BetFair.GetMarkets;
using Tennis_Betfair.TO.BetFair.GetScore;
using Tennis_Betfair.TO.NewBet365;
using Tennis_Betfair.TO.NewSkyBet;
using Tennis_Betfair.TO.SkyBet;
using Scores = Tennis_Betfair.TO.NewBet365.Scores;

namespace Tennis_Betfair.Tennis
{
    public class ParsingInfo
    {
        private static readonly object ObjectTolock = new object();
        private static readonly object LockMarkets = new object();
        private readonly HashSet<Market> _allMarkets;

        public ParsingInfo()
        {
            var comparator = new MarketEqualityComparer();
            _allMarkets = new HashSet<Market>(comparator);
        }

        /// <summary>
        /// Предоставляет информацию о всех доступных рынках.
        /// </summary>
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
        /// Евент который отображает изменение атрибутов игрока
        /// </summary>
        public static event AllMarkets.PlayerChanged PlayerChanged;

        /// <summary>
        /// Метод который парсит все данные с различных рынков в единое целое.
        /// </summary>
        /// <param name="objectToParse">Может содержать стуктуру любого из рынков</param>
        public void Parse(object objectToParse)
        {
            if (objectToParse is List<Mathes>)
            {
                ParseAllData(b365AllData: (List<Mathes>)objectToParse);
            }
            if (objectToParse is List<GetMarketData>)
            {
                ParseAllData(betfairAllDatas: (List<GetMarketData>) objectToParse);
            }
            if (objectToParse is List<Markets>)
            {
                ParseAllData(skyBetAllInfo: (List<Markets>)objectToParse);
            }

            if (objectToParse is List<GetScore>)
            {
                lock (ObjectTolock)
                {
                    ParseAllData(betfairSingleData: (List<GetScore>) objectToParse);
                }
            }
            if (objectToParse is List<Scores>)
            {
                lock (ObjectTolock)
                {
                    ParseAllData(b365SingleData: (List<Scores>) objectToParse);
                }
            }
            if (objectToParse is List<TO.NewSkyBet.Scores>)
            {
                lock (ObjectTolock)
                {
                    ParseAllData(skyBetSingleData: (List<TO.NewSkyBet.Scores>) objectToParse);
                }
            }
            
        }

        /// <summary>
        /// [betfair.com][AllData] Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="betfairAllDatas"> Вся информация с рынка betfair</param>
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
                                    new Player(player1, null, TypeDBO.BetFair),
                                    new Player(player2, null, TypeDBO.BetFair), marketIdBetfair, TypeDBO.BetFair)
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

        /// <summary>
        /// [bet365.com][AllData] Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="b365AllData"> Вся информация с рынка bet365</param>
        private void ParseAllData(List<Mathes> b365AllData)
        {
            if (b365AllData == null) throw new ArgumentNullException(nameof(b365AllData));
            var count = 0;
            try
            {
                foreach (var b365Data in b365AllData)
                {
                    var name = "";
                    var player1 = b365Data.player1Name;
                    var player2 = b365Data.player2Name;
                    var eventId = player1 + "|" + player2;
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
                                new Player(player1, null, TypeDBO.Bet365),
                                new Player(player2, null, TypeDBO.Bet365), eventId.ToString(), TypeDBO.Bet365)
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

        /// <summary>
        /// [skybet.com][AllData] Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="skyBetAllInfo"> Вся информация с рынка skybet</param>
        private void ParseAllData(List<Markets> skyBetAllInfo)
        {
            if (skyBetAllInfo == null) throw new ArgumentNullException(nameof(skyBetAllInfo));
            var count = 0;
            try
            {
                foreach (var skyBetResponse in skyBetAllInfo)
                {
                    var name = "";
                    var player1 = skyBetResponse.player1Name;
                    var player2 = skyBetResponse.player2Name;
                    var eventId = skyBetResponse.eventId;
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
                        allMarket.SkyBetEventId = eventId;

                    }
                    if (!flag)
                    {
                        AllMarketsHashSet.Add(
                            new Market(name,
                                new Player(player1, null, TypeDBO.SkyBet),
                                new Player(player2, null, TypeDBO.SkyBet), eventId.ToString(), TypeDBO.SkyBet)
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

        /// <summary>
        ///  [betfair.com][Score] Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="betfairSingleData"> Вся информация с рынка betfair</param>
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
                        PlayerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
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

        /// <summary>
        /// [Bet365.com][Score] Методы для обработки информации с рынков в единое целое.
        /// </summary>
        /// <param name="b365SingleData"> Вся информация с рынка betfair</param>
        private void ParseAllData(List<Scores> b365SingleData)
        {
            var count = 0;
            try
            {
                var eventId = b365SingleData[0].name + "|" + b365SingleData[1].name;
                foreach (var allMarket in AllMarketsHashSet)
                {
                    if (allMarket.Bet365EventId != null
                        && allMarket.Bet365EventId.Equals(eventId))
                    {
                        allMarket.Player1.ScoreBet366 =
                            b365SingleData[0].score;
                        allMarket.Player2.ScoreBet366 =
                            b365SingleData[1].score;
                        //allMarket.MarketName = b365SingleData.CompetitionType;

                        PlayerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
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

        /// <summary>
        /// [SkyBet.com][Score] Метод для обработки информации с рынков в единое целлое
        /// </summary>
        /// <param name="skyBetSingleData">Вся полученная информация с рынка skyBet</param>
        private void ParseAllData(List<TO.NewSkyBet.Scores> skyBetSingleData)
        {
           /* var count = 0;
            try
            {
                var eventId = skyBetSingleData;
                foreach (var allMarket in AllMarketsHashSet)
                {
                    if (allMarket.SkyBetEventId != null
                        && allMarket.SkyBetEventId.Equals(eventId))
                    {
                        allMarket.Player1.ScoreSkyBet =
                            skyBetSingleData.ScoreFirst;
                        allMarket.Player2.ScoreSkyBet =
                            skyBetSingleData.ScoreSecond;
                        allMarket.SkyBetEventId = eventId;
                        PlayerChanged?.Invoke(new ScoreUpdEventArgs(allMarket));
                        break;
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
            }*/
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
                    if (b1.SkyBetEventId == null)
                        b1.SkyBetEventId = b2.SkyBetEventId;
                    Debug.WriteLine("Одинаковые: " + b1.Player1.Name + " : " + b1.Player2.Name + " Idb: " +
                                    b1.BetfairEventId + " 2: " + b1.Bet365EventId);
                    return true;
                }
                return false;
            }

            public int GetHashCode(Market obj)
            {
                if (string.IsNullOrEmpty(obj.MarketName))
                {
                    int pCode = 0;
                    if (!string.IsNullOrEmpty(obj.Player1.Name))
                    {
                        pCode += obj.Player1.Name[0] * obj.Player1.Name[1] * obj.Player2.Name[2]^7;
                    }
                    if (!string.IsNullOrEmpty(obj.Player2.Name))
                    {
                        pCode += obj.Player2.Name[0] * obj.Player2.Name[1] * obj.Player2.Name[2]^3;
                    }
                    return pCode;
                }

                var hCode = 31 ^ obj.MarketName[0] * 4 ^ obj.MarketName[1] * 5 ^ obj.MarketName[2] * 3;
                if (obj.Player1.Name != null)
                {
                    hCode += obj.Player1.Name[0] * obj.Player1.Name[1] * obj.Player2.Name[2];
                }
                if (obj.Player2.Name != null)
                {
                    hCode += obj.Player2.Name[0] * obj.Player2.Name[1] * obj.Player2.Name[2];
                }
                return hCode;
            }
        }
    }
}