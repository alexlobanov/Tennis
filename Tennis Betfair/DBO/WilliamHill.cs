using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Security.Policy;
using Newtonsoft.Json;
using Tennis_Betfair.TO.WilliamHill;

namespace Tennis_Betfair.DBO
{
    public class WilliamHill
    {
        private const string AppKey = "x";

        private const string URL_GET_INPLAY_MARKETS =
            "http://sandbox.whapi.com/v1/competitions/events/inplay/";

        public IList<InPlayComp> GetInPlayAllMarkets()
        {
            var request = (HttpWebRequest) WebRequest.Create(URL_GET_INPLAY_MARKETS);
            request.Method = "GET";
            request.ContentType = "application/vnd.who.Sportsbook+json;v=1;charset=utf-8";
            request.Host = "sandbox.whapi.com";
            request.Headers.Add("who-apiKey", AppKey);
            using (Stream stream = ((HttpWebResponse) request.GetResponse()).GetResponseStream())
            {
                Debug.Assert(stream != null, "stream != null");
                using (var reader = new StreamReader(stream, Encoding.Default))
                {
                    var jsonResponse = JsonConvert.DeserializeObject<List<InPlayComp>>(reader.ReadToEnd());
                    return jsonResponse;
                }
            }
        }

    }

}