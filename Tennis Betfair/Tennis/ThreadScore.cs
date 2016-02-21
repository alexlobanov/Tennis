using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair.Tennis
{
    public class ThreadScore
    {
        private readonly AllMarkets allMarkets;

        private readonly Thread threadBetfair;
        private readonly Thread threadScore365;
        private readonly Thread threadSkyBet;

        private string bet365Id;
        private string betfairId;
        private string skyBetId;

        private int countUpdate;

        public bool isPosibleStop365;
        public bool isPosibleStopBet;
        public bool isPosibleStopSkyBet;

        private bool isStop;

        private ThreadStatus status;

        public ThreadScore(string betfairId, string bet365Id, string skyBet, AllMarkets allMarkets)
        {
            this.betfairId = betfairId;
            this.bet365Id = bet365Id;
            this.skyBetId = skyBet;

            this.allMarkets = allMarkets;

            allMarkets.GetScoreMarket(this.bet365Id, TypeDBO.Bet365);
            allMarkets.GetScoreMarket(this.skyBetId, TypeDBO.SkyBet);

            //threadScore365 = new Thread(GetScore);
            threadBetfair = new Thread(GetScore);
            //threadSkyBet = new Thread(GetScore);

            threadBetfair.Name = "BetFair " + betfairId;
       /*     threadScore365.Name = "BetScore365 " + bet365Id;
            threadScore365.IsBackground = true;
            threadSkyBet.Name = "SkyBet " + skyBetId;
            threadSkyBet.IsBackground = true;*/
        }

    

        public ThreadStatus GetStatus()
        {
            return new ThreadStatus(threadBetfair.ThreadState);
        }


        public void MarketIgnore(TypeDBO marketType)
        {
            switch (marketType)
            {
                case TypeDBO.BetFair:
                    threadBetfair.Suspend();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(marketType), marketType, null);
            }
        }

        public void UnMarketIgnore(TypeDBO marketTypeDbo)
        {
            switch (marketTypeDbo)
            {
                case TypeDBO.BetFair:
                    threadBetfair.Resume();
                    break;
            }
        }

        public void StartThreads()
        {
           // threadScore365.Start(new GetScoreStruct(TypeDBO.Bet365, bet365Id));
            threadBetfair.Start(new GetScoreStruct(TypeDBO.BetFair, betfairId));
           // threadSkyBet.Start(new GetScoreStruct(TypeDBO.SkyBet, skyBetId));
        }

        public void StopThreads()
        {
            isStop = true;
        }

        public void AbortThreads()
        {
            try
            {
                threadBetfair.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ThreadScore Exeption]: " + ex.Message);
            }
            try
            {
                threadScore365.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ThreadScore Exeption]: " + ex.Message);
            }
            try
            {
                threadSkyBet.Abort();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[ThreadScore Exeption]: " + ex.Message);
            }
        }


        public void UpdateEventId()
        {
            var countErrors = 0;
            try
            {
                var hashset = new HashSet<Market>();
                hashset = allMarkets.ParsingInfo.AllMarketsHashSet;
                foreach (var market in hashset)
                {
                    if ((market.Bet365EventId == bet365Id) || (market.BetfairEventId == betfairId) || (market.SkyBetEventId == skyBetId))
                    {
                        if (market.IsClose)
                        {
                            countUpdate++;
                            if (countUpdate == 3)
                            {
                                isStop = true;
                            }
                        }
                        if (string.IsNullOrEmpty(bet365Id))
                            bet365Id = market.Bet365EventId;
                        if (string.IsNullOrEmpty(betfairId))
                            betfairId = market.BetfairEventId;
                        if (string.IsNullOrEmpty(skyBetId))
                            skyBetId = market.SkyBetEventId;
                    }
                }
                countErrors = 0;
            }
            catch (Exception)
            {
                countErrors++;
                if (countErrors >= 20)
                    throw new Exception("Update market's id Error");
            }
        }

        private void GetScore(object info)
        {
            var information = (GetScoreStruct) info;
            var count = 0;
            var isFirst = true; 
            while (true)
            {
                var result = true;
                UpdateEventId();
                if (isStop) return;
                if ((string) information.EventId != null)
                    if (isFirst)
                    {
                        result = allMarkets.GetScoreMarket((string) information.EventId, information.TypeDbo);
                        isFirst = false;
                    }
                if ((information.TypeDbo == TypeDBO.BetFair))
                {
                    isFirst = true;
                }
                if (result)
                {
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count < 20) continue;
                switch (information.TypeDbo)
                {
                    case TypeDBO.BetFair:
                        isPosibleStopBet = true;
                        break;
                }
                if ((isPosibleStop365) && (isPosibleStopBet) && (isPosibleStopSkyBet))
                    isStop = true;
            }
        }
    }
}