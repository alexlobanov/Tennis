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
        private string bet365Id;
        private string betfairId;

        private int countUpdate;
        private bool isPosibleStop365;

        private bool isPosibleStopBet;

        private bool isStop;

        private ThreadStatus status;

        public ThreadScore(string betfairId, string bet365Id, ref AllMarkets allMarkets)
        {
            this.betfairId = betfairId;
            this.bet365Id = bet365Id;

            this.allMarkets = allMarkets;

            threadScore365 = new Thread(GetScore365);
            threadBetfair = new Thread(GetScoreBetfair);

            threadScore365.Name = "BetScore365 " + bet365Id;
            threadBetfair.Name = "BetFair " + betfairId;
        }

        public ThreadStatus GetStatus()
        {
            return new ThreadStatus(threadBetfair.ThreadState, threadScore365.ThreadState);
        }


        public void MarketIgnore(int market)
        {
            switch (market)
            {
                case 1:
                    threadBetfair.Suspend();
                    if (threadScore365.ThreadState == ThreadState.Suspended)
                        threadScore365.Resume();
                    break;
                case 2:
                    threadScore365.Suspend();
                    if (threadBetfair.ThreadState == ThreadState.Suspended)
                        threadBetfair.Resume();
                    break;
                case 0:
                    if (threadBetfair.ThreadState == ThreadState.Suspended)
                        threadBetfair.Resume();
                    if (threadScore365.ThreadState == ThreadState.Suspended)
                        threadScore365.Resume();
                    break;
            }
        }

        public void StartThreads()
        {
            threadScore365.Start(bet365Id);
            threadBetfair.Start(betfairId);
        }

        public void StopThreads()
        {
            isStop = true;
        }

        public void AbortThreads()
        {
            threadBetfair.Abort();
            threadScore365.Abort();
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
                    if ((market.Bet365EventId == bet365Id)
                        || (market.BetfairEventId == betfairId))
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


        private void GetScoreBetfair(object eventId)
        {
            var count = 0;
            while (true)
            {
                var result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string) eventId != null)
                    result = allMarkets.GetScoreMarket((string) eventId, true);
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
                if ((isPosibleStop365) && (isPosibleStopBet))
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
                    result = allMarkets.GetScoreMarket((string) eventId, false);
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
                if ((isPosibleStop365) && (isPosibleStopBet))
                    isStop = true;
            }
        }
    }
}