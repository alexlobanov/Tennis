using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.Events
{
    public class LoadedEventArgs : EventArgs
    {
        private bool loadedStarted;
        private bool loadedEnded;
        private bool isNodeClick;


        public LoadedEventArgs(bool loadedStarted, bool loadedEnded)
        {
            this.loadedStarted = loadedStarted;
            this.loadedEnded = loadedEnded;
        }

        public LoadedEventArgs(bool loadedStarted, bool loadedEnded, bool isNodeClick)
        {
            this.loadedStarted = loadedStarted;
            this.loadedEnded = loadedEnded;
            this.isNodeClick = isNodeClick;
        }

        public bool IsNodeClick
        {
            get { return isNodeClick; }
            set { isNodeClick = value; }
        }

        public bool LoadedStarted
        {
            get { return loadedStarted; }
            set { loadedStarted = value; }
        }

        public bool LoadedEnded
        {
            get { return loadedEnded; }
            set { loadedEnded = value; }
        }
    }
}
