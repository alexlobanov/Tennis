using System;
using Tennis_Betfair.TO;

namespace Tennis_Betfair.Events
{
    public class PlayerScoreUpdEventArgs : EventArgs
    {
        public PlayerScoreUpdEventArgs(string score, TypeDBO typeEx)
        {
            Score = score;
            TypeEx = typeEx;
        }

        public string Score { get; set; }

        public TypeDBO TypeEx { get; set; }
    }
}