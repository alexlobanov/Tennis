using System;

namespace Tennis_Betfair.Events
{
    public class LoadedEventArgs : EventArgs
    {
        public LoadedEventArgs(bool loadedStarted, bool loadedEnded)
        {
            LoadedStarted = loadedStarted;
            LoadedEnded = loadedEnded;
        }

        public LoadedEventArgs(bool loadedStarted, bool loadedEnded, bool isNodeClick)
        {
            LoadedStarted = loadedStarted;
            LoadedEnded = loadedEnded;
            IsNodeClick = isNodeClick;
        }

        public bool IsNodeClick { get; set; }

        public bool LoadedStarted { get; set; }

        public bool LoadedEnded { get; set; }
    }
}