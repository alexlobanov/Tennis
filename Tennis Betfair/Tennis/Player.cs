using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Tennis_Betfair.Events;
using Tennis_Betfair.TO;

namespace Tennis_Betfair
{
    public class Player : INotifyPropertyChanged
    {

        private string name;

        private long _dateTimeBetfair;
        private long _dataTime365;

        private string scoreBetfair;
        private string scoreBet365;

        private string scoreNew;

        private string prevScoreBetfair;
        private string prevScore365;
        private string prevNew;
        private bool isFirst = true;

        private DateTime prevDate365;

        public event PlayerUpdateHandler PlayerHanlder;
        public delegate void PlayerUpdateHandler(PlayerScoreUpdEventArgs scoreUpdEventArgs);

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public string ScoreBetfair1
        {
            get { return scoreBetfair; }
            set
            {
                OnPropertyChanged();
                int integ = 0;
                if (!int.TryParse(value, out integ))
                {
                    scoreBetfair = "Adv";
                    PlayerHanlder?.Invoke(new PlayerScoreUpdEventArgs("Adv", TypeDBO.BetFair));
                    return;
                }
                PlayerHanlder?.Invoke(new PlayerScoreUpdEventArgs(value, TypeDBO.BetFair));
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
                    scoreBet365 = "Adv";
                    PlayerHanlder?.Invoke(new PlayerScoreUpdEventArgs("Adv", TypeDBO.Bet365));
                    return;
                }
                PlayerHanlder?.Invoke(new PlayerScoreUpdEventArgs(value, TypeDBO.Bet365));
                scoreBet365 = value;

            }
        }

        public string ScoreNew
        {
            get { return scoreNew; }
        }

        public static int scoreComparator(string sr1, string sr2)
        {
            if (toIntScore(sr1) > toIntScore(sr2))
                return 1;
            else if (toIntScore(sr1) == toIntScore(sr2))
                return 0;
            else
                return -1;
        }

        public static int toIntScore(string sr1)
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
                    return 0;

            }
        }
        /*
        private void updateScore(string valueScore,int marketNumber)
        {


            switch (marketNumber)
            {
                case 1:
                    if (isFirst)
                    {
                        scoreNew = valueScore;
                        prevNew = valueScore;
                        isFirst = false;
                    }
                    if (!valueScore.Equals(prevNew))
                    {
                        Debug.WriteLine("Update Score: " + valueScore + " prev: " + prevNew + " Betfair");
                        if ((Math.Abs(toIntScore(prevNew) - toIntScore(valueScore)) > 16) 
                            && (toIntScore(valueScore) != 0)) return;

                        prevNew = valueScore;
                        scoreNew = valueScore;
                        
                    }
                    break;
                case 2:
                    if (isFirst)
                    {
                        scoreNew = valueScore;
                        prevNew = valueScore;
                        isFirst = false;
                    }
                    if (!valueScore.Equals(prevNew))
                    {
                        Debug.WriteLine("Update Score: " + valueScore + " prev: " + prevNew + " Score365");
                        /*if ((toIntScore(prevScore365) > toIntScore(valueScore))
                            && ((toIntScore(prevScore365) != 50) && toIntScore(valueScore) != 0))
                            return;*/
          /*              if ((Math.Abs(toIntScore(prevNew) - toIntScore(valueScore)) > 16)
                             && (toIntScore(valueScore) != 0)) return;

                        prevNew = valueScore;
                        scoreNew = valueScore;
                    }
                    break;
            }
            /*if (scoreBet365 != " : ") scoreNew = scoreBet365;
            else scoreNew = scoreBetfair;*/
        /*}
*/
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
