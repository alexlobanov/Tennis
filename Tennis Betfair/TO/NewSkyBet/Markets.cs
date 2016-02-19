using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tennis_Betfair.TO.NewSkyBet
{
    public class Markets
    {
        [JsonProperty("player1Name")]
        public string player1Name { get; set; }
        [JsonProperty("player2Name")]
        public string player2Name { get; set; }
        [JsonProperty("eventId")]
        public string eventId { get; set; }
    }

}
