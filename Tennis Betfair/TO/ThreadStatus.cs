using System.Threading;

namespace Tennis_Betfair.TO
{
    public class ThreadStatus
    {
        public ThreadStatus(ThreadState stateBetfair, ThreadState state365)
        {
            StateBetfair = stateBetfair;
            State365 = state365;
        }

        public ThreadState StateBetfair { get; set; }

        public ThreadState State365 { get; set; }
    }
}