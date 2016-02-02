using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO
{
    public class ThreadStatus
    {
        private ThreadState stateBetfair;
        private ThreadState state365;

        public ThreadStatus(ThreadState stateBetfair, ThreadState state365)
        {
            this.stateBetfair = stateBetfair;
            this.state365 = state365;
        }

        public ThreadState StateBetfair
        {
            get { return stateBetfair; }
            set { stateBetfair = value; }
        }

        public ThreadState State365
        {
            get { return state365; }
            set { state365 = value; }
        }
    }
}
