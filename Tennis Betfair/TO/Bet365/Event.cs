using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tennis_Betfair.TO.Bet365
{
    public class Event
    {
        private  String eventID;
        private  String competitionType;
        private  Team team1;
        private  Team team2;
        private bool isClose;

        public bool IsClose
        {
            get { return isClose; }
            set { isClose = value; }
        }

        public Event(String eventID, String competitionType, Team team1, Team team2)
        {
            this.eventID = eventID;
            this.competitionType = competitionType;
            this.team1 = team1;
            this.team2 = team2;
        }

        public string EventId
        {
            get { return eventID; }
        }

        public string CompetitionType
        {
            get { return competitionType; }
        }

        public Team Team1
        {
            get { return team1; }
        }

        public Team Team2
        {
            get { return team2; }
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine("======Event:" + eventID + "==========");
            result.AppendLine("Compitition name: " + competitionType);
            result.AppendLine("Player1: " + team1.getName() + " Player 2" + team2.getName());
            result.AppendLine("Score: " + team1.getScore() + ":" + team2.getScore());
            return result.ToString();
        }

        public Dictionary<Object, Object> toJSON()
        {
            Dictionary <object, object> _event = new Dictionary<object, object>();
            _event.Add("eventID",eventID);
            _event.Add("competitionType", competitionType);
            _event.Add("competitionName", team1.getName() + " vs " + team2.getName());
            _event.Add("currentResult", team1.getScore() + " : " + team2.getScore());
            return _event;
        }
    }
}
