using System;
using System.Collections.Generic;
using System.Threading;
using Tennis_Betfair.Tennis;

namespace Tennis_Betfair.TO
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

        private bool isPosibleStop365;
        private bool isPosibleStopBet;
        private bool isPosibleStopSkyBet;

        private bool isStop;

        private ThreadStatus status;

        public ThreadScore(string betfairId, string bet365Id, string skyBet, ref AllMarkets allMarkets)
        {
            this.betfairId = betfairId;
            this.bet365Id = bet365Id;
            this.skyBetId = skyBet;

            this.allMarkets = allMarkets;

            threadScore365 = new Thread(GetScore365);
            threadBetfair = new Thread(GetScoreBetfair);
            threadSkyBet = new Thread(GetScoreSkyBet);

            threadScore365.Name = "BetScore365 " + bet365Id;
            threadBetfair.Name = "BetFair " + betfairId;
            threadSkyBet.Name = "SkyBet " + skyBetId;
        }

    

        public ThreadStatus GetStatus()
        {
            return new ThreadStatus(threadBetfair.ThreadState, threadScore365.ThreadState, threadSkyBet.ThreadState);
        }


        public void MarketIgnore(TypeDBO marketType)
        {
            switch (marketType)
            {
                case TypeDBO.BetFair:
                    threadBetfair.Suspend();
                    if (threadScore365.ThreadState != ThreadState.Running)
                    {
                        threadScore365.Resume();
                    }
                    if (threadSkyBet.ThreadState != ThreadState.Running)
                    {
                        threadSkyBet.Resume();
                    }
                    break;
                case TypeDBO.Bet365:
                    threadScore365.Suspend();
                    if (threadBetfair.ThreadState != ThreadState.Running)
                    {
                        threadBetfair.Resume();
                    }
                    if (threadSkyBet.ThreadState != ThreadState.Running)
                    {
                        threadSkyBet.Resume();
                    }
                    break;
                case TypeDBO.SkyBet:
                    threadSkyBet.Suspend();
                    if (threadBetfair.ThreadState != ThreadState.Running)
                    {
                        threadBetfair.Resume();
                    }
                    if (threadScore365.ThreadState != ThreadState.Running)
                    {
                        threadScore365.Resume();
                    }
                    break;
                case TypeDBO.None:
                    if (threadBetfair.ThreadState != ThreadState.Running)
                        threadBetfair.Resume();
                    if (threadScore365.ThreadState != ThreadState.Running)
                        threadScore365.Resume();
                    if (threadSkyBet.ThreadState != ThreadState.Running)
                        threadSkyBet.Resume();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(marketType), marketType, null);
            }
        }

        public void StartThreads()
        {
            threadScore365.Start(bet365Id);
            threadBetfair.Start(betfairId);
            threadSkyBet.Start(skyBetId);
        }

        public void StopThreads()
        {
            isStop = true;
        }

        public void AbortThreads()
        {
            threadBetfair.Abort();
            threadScore365.Abort();
            threadSkyBet.Abort();
        }


        public void UpdateEventId()
        {
            var countErrors = 0;
            try
            {
                var hashset = new HashSet<Market>();
                hashset = allMarkets.AllMarketsHashSet;
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
                        if (bet365Id == null)
                            bet365Id = market.Bet365EventId;
                        if (betfairId == null)
                            betfairId = market.BetfairEventId;
                        if (skyBetId == null)
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

        private void GetScoreSkyBet(object eventId)
        {
            var count = 0;
            while (true)
            {
                var result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string) eventId != null)
                    result = allMarkets.GetScoreMarket((string) eventId, TypeDBO.SkyBet);
                if (result)
                {
                    Thread.Sleep(350);
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count < 15) continue;
                isPosibleStopSkyBet = true;
                if ((isPosibleStop365) && (isPosibleStopBet) && (isPosibleStopSkyBet))
                    isStop = true;
            }
        }

        private void GetScoreBetfair(object eventId)
        {
            var count = 0;
            while (true)
            {
                var result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string) eventId != null)
                    result = allMarkets.GetScoreMarket((string) eventId, TypeDBO.BetFair);
                if (result)
                {
                    Thread.Sleep(350);
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count < 15) continue;
                isPosibleStopBet = true;
                if ((isPosibleStop365) && (isPosibleStopBet) && (isPosibleStopSkyBet))
                    isStop = true;
            }
        }

        private void GetScore365(object eventId)
        {
            var count = 0;
            while (true)
            {
                var result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string) eventId != null)
                    result = allMarkets.GetScoreMarket((string) eventId, TypeDBO.Bet365);
                if (result)
                {
                    Thread.Sleep(350);
                    count = 0;
                }
                else
                {
                    count++;
                }
                if (count < 15) continue;
                isPosibleStop365 = true;
                if ((isPosibleStop365) && (isPosibleStopBet) && (isPosibleStopSkyBet))
                    isStop = true;
            }
        }
    }
}