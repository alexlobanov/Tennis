using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.TO;

namespace Tennis_Betfair.Events
{
    public class PlayerScoreUpdEventArgs : EventArgs
    {
        private string score;
        private TypeDBO typeEx;

        public string Score
        {
            get { return score; }
            set { score = value; }
        }

        public TypeDBO TypeEx
        {
            get { return typeEx; }
            set { typeEx = value; }
        }

        public PlayerScoreUpdEventArgs(string score, TypeDBO typeEx)
        {
            this.score = score;
            this.typeEx = typeEx;
        }
    }

}
