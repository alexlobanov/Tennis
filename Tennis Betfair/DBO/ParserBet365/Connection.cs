using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Tennis_Betfair.DBO.ParserBet365
{
    public static class Connection
    {
        public static string PostRequest(string url, WebHeaderCollection headers)
        {
            var responseFromServer = "";
            ServicePointManager.Expect100Continue = false;
            WebProxy myProxy = new WebProxy();
            myProxy.IsBypassed(new Uri(url));
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.Proxy = myProxy;
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
            request.Timeout = 1500;
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var dataStream = response.GetResponseStream())
                {
                    Debug.Assert(dataStream != null, "dataStream != null");
                    using (BufferedStream buffer = new BufferedStream(dataStream))
                    {
                        using (StreamReader readerStream = new StreamReader(buffer))
                        {
                            responseFromServer = readerStream.ReadToEnd();
                            readerStream.Close();
                        }
                        buffer.Close();
                    }
                    response.Close();
                    dataStream.Close();

                }
            }
           

            return responseFromServer;
        }
    }
}