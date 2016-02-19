using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tennis_Betfair.TO.NewSkyBet
{
    public class Scores
    {
        [JsonProperty("eventId")]
        public string eventId { get; set; }
        [JsonProperty("playerName")]
        public string playerName { get; set; }
        [JsonProperty("score")]
        public string score { get; set; }
    }
}
