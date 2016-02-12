using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.TO;

namespace Tennis_Betfair
{
    public static class CheckInternetConenction
    {
        public static bool CheckConnection(TypeDBO dboType, out string status)
        {
            try
            {
                var result = Check(dboType);
                status = "Ok";
                return result;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Connection problem with: " + dboType + " message" + exception.Message + " Data: " +
                                exception.Data);
                status = exception.Message;
                return false;
            }
        }

        public static bool CheckConnection(TypeDBO dboType)
        {
            try
            {
                var result = Check(dboType);
                return result;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Connection problem with: " + dboType + " message" + exception.Message + " Data: " +
                                exception.Data);
                return false;
            }
        }

        private static bool Check(TypeDBO dboType)
        {
            var address = "";
            var port = "";
            switch (dboType)
            {
                case TypeDBO.Bet365:
                    address = "https://mobile.bet365.com/";
                    port = "80";
                    break;
                case TypeDBO.BetFair:
                    address =
                        "https://www.betfair.com/inplayservice/v1.1/eventsInPlay?regionCode=UK&alt=json&locale=en_GB" +
                        "&channel=WEB&maxResults=100&eventTypeIds=2";
                    port = "80";
                    break;
                case TypeDBO.SkyBet:
                    address = "https://m.skybet.com/tennis";
                    port = "80";
                    break;
            }
            var request = (HttpWebRequest) WebRequest.Create(address);
            request.Timeout = 2000;
            request.UserAgent = Parse.USER_AGENT;
            var response = (HttpWebResponse) request.GetResponse();

            if (response.StatusCode != HttpStatusCode.OK) return false;

            var dataStream = response.GetResponseStream();
            if (dataStream != null)
            {
                var reader = new StreamReader(dataStream);
                var responseFromServer = reader.ReadToEnd();
                if (responseFromServer.Length < 100) return false;
            }
            return true;
        }
    }
}