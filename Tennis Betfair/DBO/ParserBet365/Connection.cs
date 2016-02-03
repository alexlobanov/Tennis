using System.IO;
using System.Net;

namespace Tennis_Betfair.DBO.ParserBet365
{
    public class Connection
    {
        public static string postRequest(string url, WebHeaderCollection headers)
        {
            var responseFromServer = "";


            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/plain; charset=UTF-8";
            request.Referer = Parse.BET365_HOME + "/";
            request.Headers.Add("Origin", Parse.BET365_HOME);
            request.UserAgent = Parse.USER_AGENT;
            request.Accept = "*/*";
            request.AutomaticDecompression = DecompressionMethods.GZip;
            request.KeepAlive = true;
            request.ContentLength = 0;
            request.Headers.Add(headers);
            //request.ContinueTimeout = 100000;

            var response = request.GetResponse();
            var dataStream = response.GetResponseStream();

            var reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();

            return responseFromServer;
        }
    }
}