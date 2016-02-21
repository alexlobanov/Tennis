using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;

namespace Tennis_Betfair.Tennis
{
    public class AllMarkets
    {
        public delegate void LoadedEventHandler(LoadedEventArgs loadedEvent);
        public delegate void PlayerChanged(ScoreUpdEventArgs player);

        public bool isStop = false;
        /// <summary>
        /// Евент вызываеммый для отображения полосы загрузки
        /// </summary>
        public static event LoadedEventHandler LoadedEvent;

        //
        private readonly Betfair _betfair;

        private readonly Bet365 _bet365;
        private readonly NewSkyBet _skyBetNew;
        //

        private readonly List<ThreadScore> _threadsScores;
        private int _countContect;
        private readonly ParsingInfo _parsingInfo;


        public AllMarkets()
        {
            _betfair = new Betfair();

            _bet365 = new Bet365(this);
            _skyBetNew = new NewSkyBet(this);

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
            LoadedEvent?.Invoke(new LoadedEventArgs(true, false));
            var thread = new Thread(() =>
            {;
                //Load all markets
                GetAll(TypeDBO.BetFair);
                GetAll(TypeDBO.Bet365);
                GetAll(TypeDBO.SkyBet);
                //End load markets
            });
            thread.Start();
            while (true)
            {
                if (isStop)
                    return;
                if ((NewSkyBet.isLoad) && (Bet365.isLoad))
                    break;
                Application.DoEvents();
                Thread.Sleep(100);
            }
            LoadedEvent?.Invoke(new LoadedEventArgs(false, true));
        }

        private string saveSkyBetId;
        private string saveBet365Id;

        /// <summary>
        /// Приостанавливает потоки которые обрабатывают рынок с идфикатором указанном в id;
        /// </summary>
        /// <param name="eventIdType">Индификатор рынка который следует приостоновить</param>
        public void MarketIgnore(TypeDBO eventIdType)
        {
            switch (eventIdType)
            {
                case TypeDBO.None:
                    _skyBetNew.IsIgnoredMarket = false;
                    if (!string.IsNullOrEmpty(saveSkyBetId))
                        GetScoreMarket(saveSkyBetId, TypeDBO.SkyBet);

                    _bet365.IsIgnoredMarket = false; //new version
                    if (!string.IsNullOrEmpty(saveBet365Id))
                        GetScoreMarket(saveBet365Id, TypeDBO.Bet365);

                    _threadsScores.Last().UnMarketIgnore(eventIdType); //old version
                    break;
                case TypeDBO.BetFair:
                    _threadsScores.Last().MarketIgnore(eventIdType); //old
                    break;
                case TypeDBO.Bet365:
                    _bet365.IsIgnoredMarket = true; //new version
                    break;
                case TypeDBO.SkyBet:
                    _skyBetNew.IsIgnoredMarket = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventIdType), eventIdType, null);
            }
           // _threadsScores.Last().MarketIgnore(eventIdType);
        }

        /// <summary>
        /// Возобновляет работу потоков которые обрабатывают рынок с идфикатором указанном в id;
        /// </summary>
        /// <param name="eventIdType">Индификатор рынка который следует возобновить</param>
        public void UnMarketIngore(TypeDBO evntIDType)
        {
            switch (evntIDType)
            {
                case TypeDBO.None:
                    break;
                case TypeDBO.BetFair:
                    _threadsScores.Last().UnMarketIgnore(evntIDType);
                    break;
                case TypeDBO.Bet365:
                    _bet365.IsIgnoredMarket = false; //new version
                    if (!string.IsNullOrEmpty(saveBet365Id))
                        GetScoreMarket(saveBet365Id, TypeDBO.Bet365);
                    break;
                case TypeDBO.SkyBet:
                    _skyBetNew.IsIgnoredMarket = false;
                    if (!string.IsNullOrEmpty(saveSkyBetId))
                        GetScoreMarket(saveSkyBetId, TypeDBO.SkyBet);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evntIDType), evntIDType, null);
            }
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
                    var threadBetFair = _threadsScores.Last().GetStatus().StateBetfair;
                    var skyBet = _skyBetNew.StateMarket;
                    var bet365 = _bet365.StateMarket;
                    return new ThreadStatus(threadBetFair, bet365, skyBet);
                }
                return null;
            }
            catch (Exception)
            {
                /*ignored*/
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
                    var bet365All = _bet365.GetAlllMathes();
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
                    var skyBetAll = _skyBetNew.GetAlllMathes();
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
                    var bet365 = _bet365.SuscribeToScores(eventId);
                    if (bet365 == MarketStatus.Closed)
                        return false;
                    saveBet365Id = eventId;
                    var currentScore = _bet365.GetCurrentScores();
                    var currentScore2 = _bet365.GetCurrentScores();
                    if ((currentScore2 == null) || (currentScore2.Count == 0))
                    {
                        if ((currentScore != null) && (currentScore.Count != 0))
                        {
                            ParsingInfo.Parse(currentScore);
                            return true;
                        }
                    }
                    else
                    {
                        ParsingInfo.Parse(currentScore2);
                        return true;
                    }
                    return false;
                }
                case TypeDBO.BetFair:
                {
                    var betfairReturn = _betfair.GetScoreEvent(long.Parse(eventId));
                    if (betfairReturn.Count == 0) return false;
                    if (betfairReturn[0].matchStatus == null) return false;
                    ParsingInfo.Parse(betfairReturn);

                    return true;
                }
                case TypeDBO.SkyBet:
                {
                    var newSkyBetReturn = _skyBetNew.GetScoreses(eventId);
                    ParsingInfo.Parse(newSkyBetReturn);
                    saveSkyBetId = eventId;
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