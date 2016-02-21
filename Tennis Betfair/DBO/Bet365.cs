using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.TO.SkyBet;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.Bet365;
using Tennis_Betfair.TO.NewBet365;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair.DBO
{
    public class Bet365
    {
        private ChromiumWebBrowser browser;
        public static bool isLoad;
        private string prevComp;
        public static event AllMarkets.PlayerChanged PlayerChanged;
        private AllMarkets allMarkets;


        public System.Threading.ThreadState State;
        public bool isIgnoredMarket;

        public ThreadState StateMarket
        {
            get { return State; }
            set { State = value; }
        }

        public bool IsIgnoredMarket
        {
            get { return isIgnoredMarket; }
            set
            {
                State = value ? ThreadState.Suspended : ThreadState.Running;
                isIgnoredMarket = value;
            }
        }

        public static event MainForm.ChengedMessage MessageChanged;
        public Bet365(AllMarkets markteMarkets)
        {
            Debug.WriteLine("Start browser2");
            var cef = Chrome.Instanse;
            isLoad = false;
            isIgnoredMarket = false;
            browser = Chrome.InstBet365;
            browser.ConsoleMessage += BrowserOnConsoleMessage;
            allMarkets = markteMarkets;
        }

        private string prevScoreOne = "0";
        private string prevScoreTwo = "0";

        private void BrowserOnConsoleMessage(object sender, ConsoleMessageEventArgs consoleMessageEventArgs)
        {     
            if (isIgnoredMarket)
            {
                Console.WriteLine("[IGNORED][Bet365]Console: " + consoleMessageEventArgs.Message);
                return;
            }
            Console.WriteLine("[Bet365]Console: " + consoleMessageEventArgs.Message);
            if (consoleMessageEventArgs.Message.Contains("Mixed Content"))
            {
                isLoad = true;
                browser.LoadingStateChanged += BrowserOnLoadingStateChanged;
                browser.EvaluateScriptAsync("clickToTennis()");
                BrowserOnLoadingStateChanged(sender,new LoadingStateChangedEventArgs(null, false, false, false));
            }
            if (consoleMessageEventArgs.Message[0] == '1')
            {
                var tmp = consoleMessageEventArgs.Message.Split('|');
                var elem = tmp[1];
                var scores = JsonConvert.DeserializeObject<List<Scores>>(elem);
                var player1 = scores[0];
                var player2 = scores[1];
                var eventId = player1.name + "|" + player2.name;
                if ((!string.IsNullOrWhiteSpace(prevScoreOne)) && (!string.IsNullOrWhiteSpace(prevScoreTwo)))
                {
                    var score1 = player1.score;
                    var score2 = player2.score;
                    if (!((score1 == "0") && (score2 == "0")))
                        if (((score1 == "0") && (score1 != prevScoreOne)) || ((score2 == "0") && (score2 != prevScoreTwo)))
                            return;
                }
                foreach (var currentMarket in allMarkets.ParsingInfo.AllMarketsHashSet)
                {
                    if (!(string.IsNullOrEmpty(currentMarket.Bet365EventId)) &&(currentMarket.Bet365EventId.Equals(eventId)))
                    {
                        prevScoreOne = player1.score;
                        prevScoreTwo = player2.score;
                        currentMarket.Player1.ScoreBet366 = player1.score;
                        currentMarket.Player2.ScoreBet366 = player2.score;
                        PlayerChanged?.Invoke(new ScoreUpdEventArgs(currentMarket));
                        break;
                    }
                }
            }

            if (consoleMessageEventArgs.Message[0] == '2')
            {
                var elem = consoleMessageEventArgs.Message.Split('/');
                var tmp = elem[1].Split('|');
                if (tmp.Length == 2)
                {
                    var player1Message = tmp[0];
                    var player2Message = tmp[1];
                    MessageChanged?.Invoke(new MessagesEventArgs(TypeDBO.Bet365, player1Message, player2Message));
                    
                }

            }
        }


        private void BrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs loadingStateChangedEventArgs)
        {
            var allJs = Properties.Resources.jsAllLoad;
            var matheses = new List<Mathes>();

            if (loadingStateChangedEventArgs.IsLoading) return;
            if (!isLoad)
                return;
            var all = browser.EvaluateScriptAsync(allJs);
            all.ContinueWith(task2 =>
            {
                CheckLoaded();
                var click = browser.EvaluateScriptAsync("clickToTennis()");
                if (click.Result.Success != true)
                {
#if DEBUG   
                   // getScreenShot("two");
#endif
                    throw new Exception("Exeption in stateChanged" + click.Result.Message);
                }
                CheckLoaded();
                Debug.WriteLine("Click");
                var suscribe = browser.EvaluateScriptAsync("suscribeEventsScores()");
                if (!string.IsNullOrEmpty(prevComp))
                {
                    SuscribeToScores(prevComp);
                }
                if (suscribe.Result.Success == true) return;
#if DEBUG
               // getScreenShot("three");
#endif
                throw new Exception("Exeption in stateChanged" + suscribe.Result.Message);
            });
        }

        private void CheckLoaded()
        {
            var allJs = Properties.Resources.jsAllLoad;
            while (true)
            {
                if (!browser.IsBrowserInitialized) continue;
                if (!browser.IsLoading)
                {
                    try
                    {
                        var sss = browser.EvaluateScriptAsync(allJs,TimeSpan.FromSeconds(2)).Result;
                        var clickTennis = browser.EvaluateScriptAsync("clickToTennis()").Result;
                        var tt = browser.EvaluateScriptAsync("$('.ipo-Classification.sport_13').length == 1");
                        if (!(bool)tt.Result.Result)
                            return;
                        var result = browser.EvaluateScriptAsync("checkLoad()",TimeSpan.FromSeconds(2)).Result;
                        if ((result.Result != null) && ((bool) result.Result == true))
                        {
                            var ss = browser.EvaluateScriptAsync(allJs).Result;
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("EXEPTION: " + ex.Message + "Stack : " + ex.StackTrace);
                    }
                }
                Thread.Sleep(200);
            }
        }

        private void getScreenShot(string str)
        {
            var task = browser.ScreenshotAsync();
            task.ContinueWith(x =>
            {
                // Make a file to save it to (e.g. C:\Users\jan\Desktop\CefSharp screenshot.png)
                var screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"CefSharp screenshot{str}.png");

                Console.WriteLine();
                Console.WriteLine("Screenshot ready. Saving to {0}", screenshotPath);

                // Save the Bitmap to the path.
                // The image type is auto-detected via the ".png" extension.
                task.Result.Save(screenshotPath);

                // We no longer need the Bitmap.
                // Dispose it to avoid keeping the memory alive.  Especially important in 32-bit applications.
                task.Result.Dispose();

                Console.WriteLine("Screenshot saved.  Launching your default image viewer...");

                // Tell Windows to launch the saved image.
                Process.Start(screenshotPath);

                Console.WriteLine("Image viewer launched.  Press any key to exit.");
            });
        }


        public MarketStatus SuscribeToScores(string comp)
        {
            prevComp = comp;
            var mathes = GetAlllMathes();
            var mathesMarket = new Mathes();
            var players = comp.Split('|');

            mathesMarket.player1Name = players[0];
            mathesMarket.player2Name = players[1];

            var index = -1;
            if (mathes == null)
            {
                State = ThreadState.Stopped;
                return MarketStatus.Closed;
            }
            foreach (var mathese in mathes)
            {
                if ((mathesMarket.player2Name.Equals(mathese.player2Name)) && (mathesMarket.player1Name.Equals(mathese.player1Name)))
                {
                    index = mathese.indexToClick;
                    break;
                }
            }
            var clickTennis = browser.EvaluateScriptAsync("clickToTennis()");
            if (index == -1)
            {
                State = ThreadState.Stopped;
                return MarketStatus.Closed;
            }
            CheckLoaded();
            var result = ClickToMarket(index);
            Thread.Sleep(100);
            if (result != true)
            {
                State = ThreadState.Stopped;
                return MarketStatus.Closed;
            }
            var suscribe = browser.EvaluateScriptAsync("suscribeEventsScores()");
            if (suscribe.Result.Result != null)
            {
                while (true)
                {
                    var loadJs = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoad).Result;
                    var clickTennis1 = browser.EvaluateScriptAsync("clickToTennis()").Result;
                    CheckLoaded();
                    var result1 = ClickToMarket(index);
                    Thread.Sleep(100);
                    var res = browser.EvaluateScriptAsync("suscribeEventsScores()").Result;
                    if (res.Result == null)
                        break;
                }
            }
            return MarketStatus.Open;
        }

        public bool ClickToMarket(int index)
        {
            var ret =  browser.EvaluateScriptAsync($"clickToMathes({index})").Result;
            Debug.WriteLine("Index to click: " + index + " Message: " + ret.Message + " Result: " + ret.Result);
            return ret.Success;
        }

        public List<Mathes> GetAlllMathes()
        {
            var allMarktes = new List<Mathes>();
            CheckLoaded();
            var loadAllJs = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoad).Result;

            var tt = browser.EvaluateScriptAsync("$('.ipo-Classification.sport_13').length == 1");
            if (!(bool)tt.Result.Result)
                return default (List<Mathes>);

            var sst = browser.EvaluateScriptAsync("clickToTennis()").Result;
            CheckLoaded();
            var mathes = browser.EvaluateScriptAsync("getMatches()");
            mathes.ContinueWith(task1 =>
            {
                if (task1.Result.Result == null)
                {
                    throw new Exception("Exeption in stateChanged" + task1.Result.Message);
                }
                if (task1.Result.Result != null)
                    allMarktes = JsonConvert.DeserializeObject<List<Mathes>>(task1.Result.Result.ToString());
                Console.WriteLine("getMathes(): " + allMarktes.Count);
                return allMarktes;
            });
            return JsonConvert.DeserializeObject<List<Mathes>>(mathes.Result.Result.ToString()); ;
        }

        public List<Scores> GetCurrentScores()
        {
            var errmsg = "";
            CheckLoaded();
            var result = browser.EvaluateScriptAsync("getScore()");
            var score = new List<Scores>();
            var res = result.Result;
            if (res.Result == null)
            {
                errmsg = res.Message;
                Debug.WriteLine("Exeption 'GetCurrentScores()': " + errmsg);
                int countUpdate = 0;
                while (true)
                {
                    res = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoad).Result;
                    result = browser.EvaluateScriptAsync("getScore()");
                    Thread.Sleep(100);
                    if (result.Result.Result != null)
                    {
                        res = result.Result;
                        break;
                    }
                    countUpdate++;
                    if (countUpdate > 25)
                    {
                        browser.Reload();
                        CheckLoaded();
                        countUpdate = 0;
                    }
                }
                #if DEBUG
               //   getScreenShot("six");
                #endif
                //throw new Exception("Ex in getCurrentScore: " + errmsg);
            }
            score = JsonConvert.DeserializeObject<List<Scores>>(res.Result.ToString());
            return score;
        }

        
    }
}
