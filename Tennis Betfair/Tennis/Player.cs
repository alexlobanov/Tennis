using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.Events;

namespace Tennis_Betfair
{
    public class Player : INotifyPropertyChanged
    {

        private readonly string name;

        private long _dateTimeBetfair;
        private long _dataTime365;

        private string scoreBetfair;
        private string scoreBet365;

        private string scoreNew;

        public string Name => name;


        public string ScoreBetfair1
        {
            get { return scoreBetfair; }
            set
            {
                OnPropertyChanged();
                int integ = 0;
                if (!int.TryParse(value, out integ))
                {
                    updateScore("Adv");
                    scoreBet365 = "Adv";
                    return;
                }
                updateScore(value);
                scoreBetfair = value;
               
            }
        }

        public string ScoreBet366
        {
            get { return scoreBet365; }
            set
            {
                OnPropertyChanged();
                int integ = 0;
                if (!int.TryParse(value,out integ))
                {
                    updateScore("Adv");
                    scoreBet365 = "Adv";
                    return;
                }
                updateScore(value);
                scoreBet365 = value;

            }
        }

        public string ScoreNew
        {
            get { return scoreNew; }
        }

        private int scoreComparator(string sr1, string sr2)
        {
            if (toIntScore(sr1) > toIntScore(sr2))
                return 1;
            else if (toIntScore(sr1) == toIntScore(sr2))
                return 0;
            else
                return -1;
        }

        private int toIntScore(string sr1)
        {
            switch (sr1)
            {
                case "0":
                    return 0;
                case "15":
                    return 15;
                case "30":
                    return 30;
                case "40":
                    return 40;
                case "Adv":
                    return 50;
                default:
                    return -1;

            }
        }

        private void updateScore(string valueScore)
        {
            scoreNew = scoreComparator(scoreBet365, ScoreBetfair1) > 0
                    ? scoreBet365 : ScoreBetfair1;
        }

        public Player(string name, string score, bool isBetfair)
        {
            this.name = name;
            if (isBetfair)
            {
                this.scoreBetfair = score;
            }
            else
            {
                this.scoreBet365 = score;
            }
        }

        public override string ToString()
        {
            return "Player: " + name + " Score(betfair): " + scoreBetfair + " Score(365): " + scoreBet365;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
