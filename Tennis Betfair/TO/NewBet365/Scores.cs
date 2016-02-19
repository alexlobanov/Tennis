using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Tennis_Betfair.TO.NewBet365
{
    public class Scores
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("score")]
        public string score { get; set; }

    }
}
