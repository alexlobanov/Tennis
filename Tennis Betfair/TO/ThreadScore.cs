using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tennis_Betfair.Tennis;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair.TO
{
    public class ThreadScore
    {
        private string betfairId;
        private string bet365Id;

        private bool isStop;

        private Thread threadScore365;
        private Thread threadBetfair;

        private ThreadStatus status;

        private AllMarkets allMarkets;

        private int countUpdate;

        private bool isPosibleStopBet;
        private bool isPosibleStop365;
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
            int countErrors = 0;
            try
            {
                HashSet<Market> hashset = new HashSet<Market>();
                hashset = allMarkets.AllMarketsHashSet;
                foreach (var market in hashset)
                {
                    if ((market.Bet365EventId == this.bet365Id)
                        || (market.BetfairEventId == this.betfairId))
                    {
                        if (market.IsClose)
                        {
                            countUpdate++;
                            if (countUpdate == 3)
                            {
                                isStop = true;
                            }
                        }
                        if (this.bet365Id == null)
                            this.bet365Id = market.Bet365EventId;
                        if (this.betfairId == null)
                            this.betfairId = market.BetfairEventId;
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
            int count = 0;
            while (true)
            {
                bool result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string)eventId != null)
                    result = allMarkets.GetScoreMarket((string)eventId, true);
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
            int count = 0;
            while (true)
            {
                bool result = false;
                UpdateEventId();
                if (isStop) return;
                if ((string)eventId != null)
                    result = allMarkets.GetScoreMarket((string)eventId, false);
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
