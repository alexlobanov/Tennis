using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO.SkyBet
{
    public class ScoreInfo
    {
        private string player1;
        private string player2;

        private string scoreFirst;
        private string scoreSecond;

        public ScoreInfo(string player1, string player2, string scoreFirst, string scoreSecond)
        {
            this.player1 = player1;
            this.player2 = player2;
            this.scoreFirst = scoreFirst;
            this.scoreSecond = scoreSecond;
        }

        public string Player1
        {
            get { return player1; }
        }

        public string Player2
        {
            get { return player2; }
        }

        public string ScoreFirst
        {
            get { return scoreFirst; }
        }

        public string ScoreSecond
        {
            get { return scoreSecond; }
        }
    }
}
