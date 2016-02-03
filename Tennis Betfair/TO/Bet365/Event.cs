using System.Collections.Generic;
using System.Text;

namespace Tennis_Betfair.TO.Bet365
{
    public class Event
    {
        public Event(string eventID, string competitionType, Team team1, Team team2)
        {
            EventId = eventID;
            CompetitionType = competitionType;
            Team1 = team1;
            Team2 = team2;
        }

        public bool IsClose { get; set; }

        public string EventId { get; }

        public string CompetitionType { get; }

        public Team Team1 { get; }

        public Team Team2 { get; }

        public override string ToString()
        {
            var result = new StringBuilder();
            result.AppendLine("======Event:" + EventId + "==========");
            result.AppendLine("Compitition name: " + CompetitionType);
            result.AppendLine("Player1: " + Team1.getName() + " Player 2" + Team2.getName());
            result.AppendLine("Score: " + Team1.getScore() + ":" + Team2.getScore());
            return result.ToString();
        }

        public Dictionary<object, object> toJSON()
        {
            var _event = new Dictionary<object, object>();
            _event.Add("eventID", EventId);
            _event.Add("competitionType", CompetitionType);
            _event.Add("competitionName", Team1.getName() + " vs " + Team2.getName());
            _event.Add("currentResult", Team1.getScore() + " : " + Team2.getScore());
            return _event;
        }
    }
}