using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
namespace Tennis_Betfair.DBO.ParserBet365
{
    public class Connection
    {
        public static String postRequest(String url, WebHeaderCollection headers)
        {

            string responseFromServer = "";
            /*using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(url);

                HttpContent http = new ByteArrayContent(headers.ToByteArray());
              //  client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(Parse.USER_AGENT));
               // client.DefaultRequestHeaders.Referrer = new Uri(Parse.BET365_HOME + "/");

                var result = client.PostAsync(url, http);
                var dataStreams = result.Result.Content.ReadAsStreamAsync();
                if (dataStreams != null)
                {

                    StreamReader reader = new StreamReader(dataStreams.Result);
                    responseFromServer = reader.ReadToEnd();
                }

            }*/
           
         

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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

            WebResponse response = request.GetResponse();
            var dataStream = response.GetResponseStream();
   
            StreamReader reader = new StreamReader(dataStream);
            responseFromServer = reader.ReadToEnd();
            
            return responseFromServer;
        }
    }
}
