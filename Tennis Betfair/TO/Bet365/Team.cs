using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO.Bet365
{
    public class Team
    {
        private String name;
        private String score;

        public Team(String name, String score)
        {
            this.name = name;
            this.score = score;
        }

        public Team()
        {
            
        }

        public String getName()
        {
            return name;
        }

        public String getScore()
        {
            return score;
        }
    }
}
