namespace Tennis_Betfair.TO.Bet365
{
    public class Team
    {
        private readonly string name;
        private readonly string score;

        public Team(string name, string score)
        {
            this.name = name;
            this.score = score;
        }

        public Team()
        {
        }

        public string getName()
        {
            return name;
        }

        public string getScore()
        {
            return score;
        }
    }
}