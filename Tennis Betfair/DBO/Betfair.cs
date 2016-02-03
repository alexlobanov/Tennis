using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Services.Protocols;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Tennis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tennis_Betfair.TO.BetFair.GetMarkets;
using Tennis_Betfair.TO.BetFair.GetScore;

namespace Tennis_Betfair.DBO
{
    public class Betfair
    {
        private const string URL_GET_ALL_MARKETS =
            "https://www.betfair.com/inplayservice/v1.1/eventsInPlay?regionCode=UK&alt=json&locale=en_GB&channel=WEB&maxResults=100&eventTypeIds=2";

        private const string URL_GET_MARKET_SCORE_ONLY =
            "https://www.betfair.com/inplayservice/v1.1/scores?regionCode=UK&alt=json&locale=en_GB&eventIds=";

        private long GetTimeForTs()
        {
            long retval = 0;
            var st = new DateTime(1970, 1, 1);
            var t = (DateTime.Now.ToUniversalTime() - st);
            retval = (long)(t.TotalMilliseconds + 0.5);
            return retval;
        }

        private string AddTSToUrl(string basic)
        {
            return basic + "&ts=" + GetTimeForTs();
        }

        private string AddTSToUrl(string basic, long eventId)
        {
            return basic + eventId + "&ts=" + GetTimeForTs();
        }

        public List<GetMarketData> GetInPlayAllMarkets()
        {
            List<GetMarketData> gata = new List<GetMarketData>(5);
            Stream stream;
            using (WebClient client = new WebClient())
            {
                stream = client.OpenRead(AddTSToUrl(URL_GET_ALL_MARKETS));
            }
            if (stream != null)
            {
                var reader = new StreamReader(stream);
                gata =
                    JsonConvert.DeserializeObject<List<GetMarketData>>(reader.ReadLine());
            }
            else
            {
                return default(List<GetMarketData>);
            }
            return gata;
        }

        public List<GetScore> GetScoreEvent(long eventId)
        {
            List<GetScore> gata = new List<GetScore>();
            Stream stream;
            using (WebClient client = new WebClient())
            {
                stream = client.OpenRead(AddTSToUrl(URL_GET_MARKET_SCORE_ONLY,eventId));
            }
            if (stream != null)
            {
                var reader = new StreamReader(stream);
                gata =
                    JsonConvert.DeserializeObject<List<GetScore>>(reader.ReadLine());
                
            }
            else
            {
                return default(List<GetScore>);
            }
            return gata;
        }    

    }
}
