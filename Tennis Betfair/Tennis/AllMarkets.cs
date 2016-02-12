using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;

namespace Tennis_Betfair.Tennis
{
    public class AllMarkets
    {
        public delegate void LoadedEventHandler(LoadedEventArgs loadedEvent);
        public delegate void PlayerChanged(ScoreUpdEventArgs player);
       
        /// <summary>
        /// Евент вызываеммый для отображения полосы загрузки
        /// </summary>
        public static event LoadedEventHandler LoadedEvent;

        //
        private readonly Bet365Class _bet365Class;
        private readonly Betfair _betfair;
        private readonly SkyBet _skyBet;
        //

        private readonly List<ThreadScore> _threadsScores;
        private int _countContect;
        private readonly ParsingInfo _parsingInfo;


        public AllMarkets()
        {
            _betfair = new Betfair();
            _bet365Class = new Bet365Class();
            _skyBet = new SkyBet();
            _threadsScores = new List<ThreadScore>(2);
            _parsingInfo = new ParsingInfo();
            MainForm.CheckChange += MainFormOnCheckChange;
            
        }

        private void MainFormOnCheckChange(ChangedCheckEventArgs checkEvent)
        {
            StopThreads();
            var startThreads = new ThreadScore(checkEvent.BetfairId, checkEvent.Bet365Id, checkEvent.SkyBetId, this);
            startThreads.StartThreads();
            _threadsScores.Add(startThreads);
        }

        public ParsingInfo ParsingInfo
        {
            get { return _parsingInfo; }
        }


        /// <summary>
        /// Запускает потоки отслеживающие изменения счёты рынков. Так же метод вызывает запуск события, для отображения загрузки.
        /// </summary>
        public void StartThreads()
        {
            var thread = new Thread(() =>
            {
                LoadedEvent?.Invoke(new LoadedEventArgs(true, false));
                //Load all markets
                GetAll(TypeDBO.BetFair);
                GetAll(TypeDBO.Bet365);
                GetAll(TypeDBO.SkyBet);
                //End load markets
                LoadedEvent?.Invoke(new LoadedEventArgs(false, true));
            });
            thread.Start();
        }


        /// <summary>
        /// Приостанавливает потоки которые обрабатывают рынок с идфикатором указанном в id;
        /// </summary>
        /// <param name="eventIdType">Индификатор рынка который следует приостоновить</param>
        public void MarketIgnore(TypeDBO eventIdType)
        {
            _threadsScores.Last().MarketIgnore(eventIdType);
        }

        /// <summary>
        /// Возобновляет работу потоков которые обрабатывают рынок с идфикатором указанном в id;
        /// </summary>
        /// <param name="eventIdType">Индификатор рынка который следует возобновить</param>
        public void UnMarketIngore(TypeDBO evntIDType)
        {
            _threadsScores.Last().UnMarketIgnore(evntIDType);
        }

        /// <summary>
        /// Получает статус потоков обрабатывающих рынки
        /// </summary>
        /// <returns></returns>
        public ThreadStatus GetStatus()
        {
            try
            {
                if (_threadsScores.Count > 0)
                {
                    return _threadsScores.Last().GetStatus();
                }
                return null;
            }
            catch (Exception)
            {
                /**/
                return null;
            }
        }

        /// <summary>
        /// Останавливает потоки обрабатывающие счёт рынков
        /// </summary>
        public void StopThreads()
        {
            foreach (var threadsScore in _threadsScores)
            {
                threadsScore.StopThreads();
            }
        }

        /// <summary>
        /// Принудительно завершает потоки обрабатывающие счёт рынков
        /// </summary>
        public void AbortThreads()
        {
            foreach (var threadsScore in _threadsScores)
            {
                threadsScore.AbortThreads();
            }
        }

        /// <summary>
        /// Получить все маркеты с рынков
        /// </summary>
        /// <param name="typeMarket">Тип маркета</param>
        /// <returns>True - если успешно, else - если неудача</returns>
        public bool GetAllMarkets(TypeDBO typeMarket)
        {
            switch (typeMarket)
            {
                case TypeDBO.Bet365:
                    {
                        var bet365All = _bet365Class.GetInPlayAllMarkets();
                        if (bet365All == null) return false;
                        ParsingInfo.Parse(bet365All);
                        return true;
                    }
                case TypeDBO.BetFair:
                    {
                        var betfairAll = _betfair.GetInPlayAllMarkets();
                        if (betfairAll == null) return false;
                        ParsingInfo.Parse(betfairAll);
                        return true;
                    }
                case TypeDBO.SkyBet:
                    {
                        var skyBetAll = _skyBet.GetMartches();
                        if (skyBetAll == null) return false;
                        ParsingInfo.Parse(skyBetAll);
                        return true;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Метод получения счёта с рынка с индификатором.
        /// </summary>
        /// <param name="eventId">Id рынка</param>
        /// <param name="marketTypeDbo">Тип рынка</param>
        /// <returns>True - если успешно, else - иначе</returns>
        public bool GetScoreMarket(string eventId, TypeDBO marketTypeDbo)
        {
            switch (marketTypeDbo)
            {
                case TypeDBO.Bet365:
                {
                    var bet365Return = _bet365Class.GetScoreEvent(eventId);
                    if (bet365Return == null) return false;
                    ParsingInfo.Parse(bet365Return);
                    return true; 
                }
                case TypeDBO.BetFair:
                {
                    var betfairReturn =
                        _betfair.GetScoreEvent(long.Parse(eventId));
                    if (betfairReturn.Count == 0) return false;
                    if (betfairReturn[0].matchStatus == null) return false;
                    ParsingInfo.Parse(betfairReturn);
                    return true;
                }
                case TypeDBO.SkyBet:
                {
                    var skyBetReturn =
                    _skyBet.GetScoreInfo(eventId);
                    if (skyBetReturn?.EventId == null) return false;
                    if ((skyBetReturn.Player1 == null) && (skyBetReturn.Player2 == null)) return false;
                    ParsingInfo.Parse(skyBetReturn);
                    return true;
                }
                default:
                    return false;
            }
        }

        private void GetAll(TypeDBO typeDbo)
        {
            while (true)
            {
                var good = GetAllMarkets(typeDbo);
                if (_countContect > 20)
                {
                    Debug.WriteLine("No connetion to ex: " + typeDbo);
                    _countContect = 0;
                    break;
                }
                if (!good)
                {
                    _countContect++;
                    Thread.Sleep(200);
                    continue;
                }
                _countContect = 0;
                break;
            }
        }


        /*End methods*/

        
    }
}