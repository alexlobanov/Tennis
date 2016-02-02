using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Joins;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using IOException = System.IO.IOException;
using Random = System.Random;
using Saxon.Api;
using Tennis_Betfair.TO.Bet365;

namespace Tennis_Betfair.DBO.ParserBet365
{
    public class Parse
    {
        public static string BET365_HOME = "https://mobile.bet365.com";
        private static String cook;
        private static String homePage;
        private List<string> hosts = new List<string>();
        private List<string> ports = new List<string>();
        private String clientRn;
        private String clientID;
        public static String USER_AGENT = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:41.0) Gecko/20100101 Firefox/41.0";
        private int connectionAttempts = 0;
        private bool done = false;
       

        private static  String RECORD_DELIM = "\\x01";
        private static  String FIELD_DELIM = "\\x02";
        private static  String[] CHANNELS = {"OVInPlay_1_3" };
        private int serverNum; 

        /*New*/
        static CookieContainer Cookie = new CookieContainer();
        Uri CookieHostname = new Uri(BET365_HOME);

        public int GenerateRandom(int min, int max)
        {
            var seed = Convert.ToInt32(Regex.Match(Guid.NewGuid().ToString(), @"\d+").Value);
            return new Random(seed).Next(min, max);
        }

        public List<Event> ParseAll()
        {
            List<Event> eventsList = new List<Event>();
            try
            {
                Debug.WriteLine("Start parsing Bet365");
                SetConnection();
            }
            catch (Exception e )
            {
                Debug.WriteLine("Ex: " + e.Message + "stack: " + e.StackTrace);
                return default(List<Event>);
            }
            Debug.WriteLine("Connected to Bet365");
            List<String> matches = new List<String>();
            bool success = false;
            try
            {
                foreach (var channel in CHANNELS)
                {
                    Debug.WriteLine("Trying to get events list from " + channel + " channel.");
                    matches = getAvailableMatches(channel);
                    if (matches != null)
                        success = true;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Ex in Chanel Get Mathes: " + e.Message + "stack: " + e.StackTrace);
                return default(List<Event>);
            }
            if (!success) return null;
            try
            {
                foreach (var match in matches)
                {
                    var eventInfo = GetTennisEventInformation(match);
                    if (eventInfo == null)
                    {
                        eventInfo = new Event(null,null,new Team(),new Team());
                        eventInfo.IsClose = true;
                        Debug.WriteLine("Finnishd");
                    }
                    eventsList.Add(eventInfo);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Ex in getTennisEvent Score: " + e.Message + "stack: " + e.StackTrace);
                return default(List<Event>);
            }
            return eventsList;
        }

        

        private void SetConnection()
        {
            getCookies();
            if (Cookie.Count == 0) return;
            var sessionId = getProperty("sessionId");
            if (sessionId == null) return;
            var connectionDetails = getProperty("ConnectionDetails");
            if (connectionDetails == null) return;

            sessionId = sessionId.Remove(0, 12);
            sessionId = sessionId.Remove(sessionId.Length - 1, 1);

            setConnectionDetails(connectionDetails);
            if (hosts.Count != 2 || ports.Count != 2) return;

            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                buffer.Append(GenerateRandom(0,10));
            }
            clientRn = buffer.ToString();

            var headesCollection = new WebHeaderCollection
            {
                {"method", "0"},
                {"topic", "__time,S_" + sessionId},
                {"transporttimeout", "20"},
                {"type", "F"}
            };

            

            String postResponce = powRequest(0, headesCollection);
            String[] temp = postResponce.Split(new char[] { (char)0x02});
            clientID = temp[1];

        }

        public Event GetTennisEventInformation(string id)
        {
            subscribe(id);

            var header = new WebHeaderCollection {{"method", "1"}};
            var requestPow = powRequest(2, header);
            var eventExpandedData = requestPow.Split((char)0x01);
            eventExpandedData = eventExpandedData[eventExpandedData.Length - 1].Split((char)0x7c);

            var resultList = new List<Dictionary<string, string>>();
            var currentRoot = new Dictionary<string, string>();

            resultList.Add(new Dictionary<string, string>());
            resultList.Add(new Dictionary<string, string>());

            int currentTeam;
            var firstItem = true;
            string currentKey = null;
            string competitionType = null;
            string eventName = null;

            string team1Score = null;
            string team2Score = null;

            string name1Player = null;
            string name2Player = null;

            foreach (var anEventExpandedData in eventExpandedData)
            {

                var parsedLine = parameterizeLine(anEventExpandedData);

                if (parsedLine == null)
                    continue;

                if (parsedLine.ContainsKey("EV"))
                {
                    //Event
                    parsedLine.TryGetValue("EV", out currentRoot);
                    Debug.Assert(currentRoot != null, "currentRoot != null");
                    currentRoot?.TryGetValue("CT", out competitionType);
                    currentRoot?.TryGetValue("NA", out eventName);
                }
                else if (parsedLine.ContainsKey("SC"))
                {
                    parsedLine.TryGetValue("SC", out currentRoot);
                    if (firstItem)
                    {
                        currentKey = "name";
                        firstItem = false;
                    }
                    else
                    {
                        Debug.Assert(currentRoot != null, "currentRoot != null");
                        currentRoot.TryGetValue("NA", out currentKey);
                    }
                }
                else if (parsedLine.ContainsKey("TE"))
                {
                    parsedLine.TryGetValue("TE",out currentRoot);
                    var equelsValue = "";
                    currentRoot.TryGetValue("OR", out equelsValue);
                    if (string.Equals(equelsValue, "0"))
                    {
                        currentTeam = 0;
                        currentRoot.TryGetValue("PO",out team1Score);
                        currentRoot.TryGetValue("NA", out name1Player);
                    }
                    else
                    {
                        currentTeam = 1;
                        currentRoot.TryGetValue("NA", out name2Player);
                        currentRoot.TryGetValue("PO", out team2Score);
                    }
                }
            }
            unsubscribe(id);
            if (competitionType == null) return null;
            Team team1 = new Team(name1Player, team1Score);
            Team team2 = new Team(name2Player, team2Score);
            Event eEvent = new Event(id, competitionType, team1, team2);
            return eEvent;

        }

        private String powRequest(int sid, WebHeaderCollection specialHeaders)
        {
            var defaultHeaders = new WebHeaderCollection();

            if (clientID != null)
            {
                defaultHeaders.Add("clientid", clientID);
            }

            if (sid != 0)
            {
                defaultHeaders.Add("s", serverNum.ToString());
                serverNum++;
            }
            var totalHeaders = new WebHeaderCollection { specialHeaders, defaultHeaders };
            String url = hosts.ElementAt(1) + @"/pow/?sid=" + sid + "&rn=" + clientRn;
            return Connection.postRequest(url, totalHeaders);
        }

        private void setConnectionDetails(String connectionDetails)
        {
            String pattern = "Host\":\"(.*?)\"";
            Regex reg = new Regex(pattern);
            var match = reg.Match(connectionDetails);
            var oneMatch = match?.Value.Remove(0, 7);
            var twoMatch = match.NextMatch()?.Value.Remove(0, 7);
            hosts.Add(oneMatch.Remove(oneMatch.Length-1,1) );
            hosts.Add(twoMatch.Remove(twoMatch.Length - 1, 1) );

            pattern = "Port\":(.*?),";
            Regex regPort = new Regex(pattern);
            var matherPort = regPort.Match(connectionDetails);
            var oneMatchPort = matherPort?.Value.Remove(0, 6);
            var twoMatchPort = matherPort.NextMatch()?.Value.Remove(0, 6);
            ports.Add(oneMatchPort.Remove(oneMatchPort.Length - 1, 1));
            ports.Add(twoMatchPort.Remove(twoMatchPort.Length - 1, 1));
        }

        private String getProperty(String property)
        {
            String pattern;

            switch (property)
            {
                case "sessionId":
                    pattern = property + "\":\"(.*?)\"";
                    break;
                case "ConnectionDetails":
                    pattern = property + "\":\\[(.*?)\\]";
                    break;
                default:
                    return null;
            }

            Regex reg = new Regex(pattern);
            Match match = reg.Match(homePage);
            if (match.Success == true)
            {
                return match.Value;
            }
            return null;
        }

        private void getCookies()
        {
            StringBuilder html = new StringBuilder();
            var req = (HttpWebRequest)WebRequest.Create(BET365_HOME);
            req.Timeout = 1000000;
            req.UserAgent = USER_AGENT;
            req.CookieContainer = Cookie;
             
            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            using (HttpWebResponse response = (HttpWebResponse)req.GetResponse())
            {
                var cookies = new CookieCollection();
                cookies = response.Cookies;
                Cookie.Add(CookieHostname, cookies);
                var readStream = new StreamReader(response?.GetResponseStream(), encode);
                html.Append(readStream.ReadToEnd());
                readStream.Close();
                response.Close();
            }
            // var strToSub = Cookie.GetCookieHeader(CookieHostname).ToString();
            //TODO: FIX;
            cook = Cookie.GetCookieHeader(CookieHostname).ToString();
            homePage = html.ToString();


        }

        private List<String> getAvailableMatches(String channel)
        {
            List<String> matches = new List<String>();
            try
            {
                matches = getEvents(channel);
            }
            catch (IOException e)
            {
                Debug.WriteLine(e.StackTrace);
            }

            return matches;
        }

        private List<string> getEvents(string channel)
        {
            subscribe(channel);
            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("method", "1");
            String gameDataRequest = powRequest(2, headers);

            String[] gameData = gameDataRequest.Split(new char[] { (char)0x01 });
            gameData = gameData[gameData.Length - 1].Split(new char[] { (char)0x7c });
            List<string> gameDateList = new List<string>(350);
            gameDateList = gameData.ToList();
            gameDateList.RemoveAt(0); //Remove F
            if (gameDateList.Count == 0) return null;
            var initialCL = parameterizeLine(line: gameDateList[0]);
            var paramsDic = new Dictionary<string, string>();
            if (initialCL != null)
            {
                initialCL.TryGetValue("CL", out paramsDic);
            }
            if (paramsDic == null) return null;

            List<string> Events = new List<string>(5);
            bool isTennis = false;
            bool isFirst = true;
            foreach (var data in gameDateList)
            {
                if (isFirst)
                {
                    isFirst = false;
                    continue;
                }

                var lineData = parameterizeLine(data);
                if (lineData == null)
                    continue;
                if (!isTennis)
                {
                    if (!lineData.ContainsKey("CL"))
                        continue;
                    else
                    {
                        isTennis = true;
                        continue;
                    }
                }
                if (lineData.ContainsKey("CL"))
                {
                    break;
                }
                if (lineData.ContainsKey("CL"))
                {
                    break;
                }
                if (lineData.ContainsKey("EV"))
                {
                    var Id = "";
                    var tmp = new Dictionary<string,string>();
                    lineData.TryGetValue("EV", out tmp);
                    tmp.TryGetValue("ID", out Id);
                    if (Id.Length == 18)
                    {
                        Events.Add(Id);
                    }
                }
            }
            unsubscribe(channel);
            return Events;

        }

        private void subscribe(String channel)
        {
            var headers = new WebHeaderCollection {{"method", "22"}, {"topic", channel}};
            powRequest(2, headers);
        }

        private void unsubscribe(String channel)
        {
            var headers = new WebHeaderCollection {{"method", "23"}, {"topic", channel}};
            powRequest(2, headers);
        }

    /*
    private List<String> getEvents(String channel)
    {
        subscribe(channel);

        //HttpCookie 
       /* List<BasicHeader> headers = new List<BasicHeader>();
        headers.Add(new BasicHeader("method","1"));
        String gameDataRequest = powRequest(2, headers);

        string[] gameData = gameDataRequest.Split(new char[] { (char)0x01 });
        gameData = gameData[gameData.Length - 1].Split('|');
        string[] data = gameData;
        gameData.CopyTo(data,1);
        gameData = data;
        Dictionary<String, Dictionary<String, String>> initialCL = parameterizeLine(gameData[0]);
        Dictionary<String, String> paramsDictionary = new Dictionary<string, string>();

        if (initialCL != null)
        {
            initialCL.TryGetValue("CL", out paramsDictionary);
        }

        if (paramsDictionary == null) return null;

        List<string> events = new List<string>();
        bool isTennis = false;
        for (int i = 1; i < gameData.Length; i++)
        {
            Dictionary<String, Dictionary<String, String>> lineData =
                parameterizeLine(gameData[i]);

            if (lineData == null)
                continue;

            if (!isTennis)
            {
                if (!lineData.ContainsKey("CL"))
                    continue;
                else
                    isTennis = true;
                continue;
            }

            if (lineData.ContainsKey("CL"))
            {
                break;
            }
            // stop if found new category line
            if (lineData.ContainsKey("CL"))
            {
                break;
            }

            if (lineData.ContainsKey("EV"))
            {
                Dictionary<string, string> str1;
                lineData.TryGetValue("EV", out str1);
                string ID = "";
                str1?.TryGetValue("ID",out ID);
                events.Add(ID);
            }

        }
        unsubscribe(channel);

        return events;

    }*/

    private Dictionary<String, Dictionary<String, String>> parameterizeLine(String line)
        {
            String[] chunk = line.Split(';');

            if (chunk.Length == 0)
                return null;

            String cmd = chunk[0];

            // remove cmd element
            //chunk = (String[]) Array.copyOfRange(chunk, 1, chunk.Length);

            Dictionary<String, Dictionary<String, String>> map 
                = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<String,String> paramDictionarys = new Dictionary<string, string>();

            foreach (var pstr in chunk)
            {
                String[] pdata = pstr.Split('=');

                if (pdata.Length == 2)
                {
                    paramDictionarys.Add(pdata[0], pdata[1]);
                }
            }

            map.Add(cmd, paramDictionarys);

            return map;
        }

       /* private void subscribe(String channel)
        {
            var headers = new List<BasicHeader>
            {
               new BasicHeader("method", "22"),
               new BasicHeader("topic", channel)
            };
            powRequest(2, headers);
        }

        private void unsubscribe(String channel)
        {
            var headers = new List<BasicHeader>
            {
               new BasicHeader("method", "23"),
               new BasicHeader("topic", channel)
            };
            powRequest(2, headers);
        }
        */
        /// <summary>
        /// Set connection to bet365
        /// </summary>
      /*  private void SetConnection()
        {
            getCookies();
            if (cookies.isEmpty()) return;
            String sessionID = getProperty("sessionId");
            if (sessionID == null) return;
            String connectionDetails = getProperty("ConnectionDetails");
            if (connectionDetails == null || connectionDetails.isEmpty()) return;
            setConnectionDetails(connectionDetails);
            if (hosts.Count != 2 || ports.Count != 2) return;
            // Generate random Client RN
            String characters = "1234567890";
            StringBuilder buffer = new StringBuilder();

            for (int i = 0; i < 16; i++)
            {
                Random rnd = new Random();
                double index = rnd.NextDouble() * 10;
                buffer.Append(characters.charAt((int)index));
            }
            clientRn = buffer.toString();

            // Send POST request to get clientID
            var headersDictionary = new List<BasicHeader>
            {
                new BasicHeader("method", "0"),
                new BasicHeader("topic", "__time, S_" + sessionID),
                new BasicHeader("transporttimeout", "20"),
                new BasicHeader("type", "F")
            };
            String postResponce = powRequest(0, headersDictionary);
            String[] temp = postResponce.split(FIELD_DELIM);
            clientID = temp[1];
        }
        */

            /*
        private string powRequest(int sid, List<BasicHeader> specialHeaders)
        {
            var headersDictionary = new List<BasicHeader>
            {
                new BasicHeader("Content-Type", "text/plain; charset=UTF-8"),
                new BasicHeader("Referer", BET365_HOME + "/"),
                new BasicHeader("Origin", BET365_HOME),
                new BasicHeader("User-Agent", USER_AGENT)
            };

           
            if (clientID != null)
            {
                headersDictionary.Add(new BasicHeader("clientid", clientID));
            }

            if (sid != 0)
            {
                headersDictionary.Add(new BasicHeader("s", serverNum.toString()));
                serverNum++;
            }
            var totalHeaders = new List<BasicHeader>(headersDictionary);
            totalHeaders.AddRange(specialHeaders);

            String url = hosts.ElementAt(1) + "/pow/?sid=" + sid + "&rn=" + clientRn;

            return Connection.PostRequest(url, totalHeaders);

        }

        private void setConnectionDetails(String connectionDetails)
        {
         /*   String pattern = "Host\":\"(.*?)\"";

            Pattern p = Pattern.compile(pattern);
            Field.Matcher m = p.matcher(connectionDetails);

            while (m.find())
            {
                hosts.Add(m.group(1));
            }

            pattern = "Port\":(.*?),";
            p = Pattern.compile(pattern);
            m = p.matcher(connectionDetails);

            while (m.find())
            {
                ports.Add(m.group(1));
            }*/
    /*    }
    
        private String getProperty(String property)
        {

          /*  String pattern;

            switch (property)
            {
                case "sessionId":
                    pattern = property + "\":\"(.*?)\"";
                    break;
                case "ConnectionDetails":
                    pattern = property + "\":\\[(.*?)\\]";
                    break;
                default:
                    return null;
            }

            Pattern p = Pattern.compile(pattern);
            Field.Matcher m = p.matcher(homePage);

            if (m.find())
            {
                return m.group(1);
            }
            */
    /*        return null;
        }

        private void getCookies()
        {
            CookieStore store = new CustomCookieScore();
            CookiePolicy policy = CookiePolicy.ACCEPT_ALL;
            CookieManager handler = new CookieManager(store, policy);
            CookieHandler.setDefault(handler);
            URL url = new URL(BET365_HOME);
            URLConnection conn = url.openConnection();
            BufferedReader insBufferedReader = new BufferedReader(
               new InputStreamReader(conn.getInputStream()));
            String inputLine;
            StringBuilder html = new StringBuilder();

            while ((inputLine = insBufferedReader.readLine()) != null) {
                html.Append(inputLine);
            }
            insBufferedReader.close();

            homePage = html.toString();

            // set cookies
            String str = store.getCookies().toString();
            cookies = str.substring(1, str.length() - 1);
        }*/
    


    }
}
