using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.OffScreen;
using Newtonsoft.Json;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using Tennis_Betfair.TO.NewBet365;
using Tennis_Betfair.TO.NewSkyBet;
using Scores = Tennis_Betfair.TO.NewSkyBet.Scores;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair.DBO
{
    public class NewSkyBet
    {
        private ChromiumWebBrowser browser;
        public static bool isLoad;
        private string prevComp;
        public static event AllMarkets.PlayerChanged PlayerChanged;
        private const string mainURL = "https://www.skybet.com/tennis/";
        private const string scoreUrl = "https://www.skybet.com/tennis/tennis-live/event/";
        private AllMarkets allMarkets;
        public static event MainForm.ChengedMessage MessageChanged;
        public static bool ShouldStop;
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

        public NewSkyBet(AllMarkets marktes)
        {
            var cef = Chrome.Instanse;
            isLoad = false;
            browser = Chrome.InstSkyBet;
            browser.ConsoleMessage += BrowserOnConsoleMessage;
            browser.LoadingStateChanged += BrowserOnLoadingStateChanged;
            browser.BrowserInitialized += BrowserOnBrowserInitialized; 
            allMarkets = marktes;

            State = ThreadState.Running;
            isIgnoredMarket = false;
        }

        private void BrowserOnBrowserInitialized(object sender, EventArgs eventArgs)
        {
            Debug.WriteLine("[SkyBet]Browser init");
        }

        private string prevScoreOne;
        private string prevScoreTwo;

        private void BrowserOnConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            if (isIgnoredMarket)
            {
                Debug.WriteLine("[IGNORED][SkyBet]Console: " + e.Message);
                return;
            }
            Debug.WriteLine("[SkyBet]Console: " + e.Message);
            if (e.Message.Contains("FINISHED"))
            {
                State = ThreadState.Stopped;
            }
            if (e.Message[0] == '1')
            {
                var tmp = e.Message.Split('|');
                var main = tmp[1];
                var scores = JsonConvert.DeserializeObject<List<Scores>>(main);

                var score1 = scores[1].score;
                var score2 = scores[2].score;
                var eventId = scores[0].eventId;
                if ((!string.IsNullOrWhiteSpace(prevScoreOne)) && (!string.IsNullOrWhiteSpace(prevScoreTwo)))
                {
                    if (!((score1 == "0") && (score2 == "0")))
                        if (((score1 == "0") && (score1 != prevScoreOne)) || ((score2 == "0") && (score2 != prevScoreTwo)))
                            return;
                }
                foreach (var currentMarket in allMarkets.ParsingInfo.AllMarketsHashSet)
                {
                    if ((!string.IsNullOrWhiteSpace(currentMarket.SkyBetEventId)) && (currentMarket.SkyBetEventId.Equals(eventId)))
                    {
                        prevScoreOne = score1;
                        prevScoreTwo = score2;
                        currentMarket.Player1.ScoreSkyBet = score1;
                        currentMarket.Player2.ScoreSkyBet = score2;
                        PlayerChanged?.Invoke(new ScoreUpdEventArgs(currentMarket));
                        break;
                    }
                }
            }
            if (e.Message[0] == '2')
            {
                var str = e.Message.Split('|');
                var elem = str[1];
                MessageChanged?.Invoke(new MessagesEventArgs(elem,TypeDBO.SkyBet));
            }
        }

        private void BrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs loadingStateChangedEventArgs)
        {
            var allJs = Properties.Resources.jsAllLoadSkybet;
            var matheses = new List<Mathes>();
            if (loadingStateChangedEventArgs.IsLoading) return;
            Debug.WriteLine("Loaded");
            //getScreenShot("load");
            var all = browser.EvaluateScriptAsync(allJs);
            all.ContinueWith(task2 =>
            {
                isLoad = true;
            });
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

        private void reloadToMainPage()
        {
            if ((browser.IsLoading))
            {
                CheckLoad();
                return;
            }
            if (browser.IsBrowserInitialized)
                browser.Delete();
            isLoad = false;
            browser.Dispose();

            browser = Chrome.InstSkyBet;
            CheckLoad();
        }

        public List<Markets> GetAlllMathes()
        {
            if (!isLoad)
                reloadToMainPage();
            var allMarktes = new List<Markets>();
           // reloadToMainPage();
            var reloadAlljs = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoadSkybet);
            CheckLoad();
            if (reloadAlljs.Result.Message != null)
            {
                GetAlllMathes();
            }

            var mathes = browser.EvaluateScriptAsync("getMarkets()");
            
            mathes.ContinueWith(task1 =>
            {
                CheckLoad();
                if (task1.Result.Result == null)
                {
                    //getScreenShot("ERROR");
                    throw new Exception("Exeption in stateChanged" + task1.Result.Message);
                }
                while (true)
                {
                    if (task1.Result.Result != null)
                        break;
                    reloadToMainPage();
                    Thread.Sleep(100);
                    var reloadAllj1s = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoadSkybet).Result;
                    var reloadSkript = browser.EvaluateScriptAsync("getMarkets()");
                    if (reloadSkript.Result.Result != null)
                    {
                        allMarktes = JsonConvert.DeserializeObject<List<Markets>>(task1.Result.Result.ToString());
                        return allMarktes;
                    }
                }
                if (task1.Result.Result != null)
                    allMarktes = JsonConvert.DeserializeObject<List<Markets>>(task1.Result.Result.ToString());
                Console.WriteLine("getMathes(): " + allMarktes.Count);
                return allMarktes;
            });
            
            var returnElem = mathes.Result.Result.ToString();
            return JsonConvert.DeserializeObject<List<Markets>>(returnElem); ;
        }

        private void CheckLoad()
        {
            var alljs = Properties.Resources.jsAllLoad;
            while (true)
            {
                Application.DoEvents();
                if (!browser.IsLoading)
                {
                    if (browser.IsBrowserInitialized)
                    {
                        var sss = browser.EvaluateScriptAsync(alljs).Result;
                        var result = browser.EvaluateScriptAsync("checkLoaded()").Result;
                        if ((result.Result != null) && ((bool) result.Result == true))
                        {
                            var ss = browser.EvaluateScriptAsync(alljs).Result;
                            isLoad = true;
                            break;
                        }
                    }
                }
                Thread.Sleep(200);
            }
        }

        public void LoadPageSkyBet(string Url)
        {
            browser.ConsoleMessage -= BrowserOnConsoleMessage;
            browser.LoadingStateChanged -= BrowserOnLoadingStateChanged;
            browser.BrowserInitialized -= BrowserOnBrowserInitialized;
            browser.Delete();
            browser.Dispose();
            browser = new ChromiumWebBrowser(Url);
            browser.ConsoleMessage += BrowserOnConsoleMessage;
            browser.LoadingStateChanged += BrowserOnLoadingStateChanged;
            browser.BrowserInitialized += BrowserOnBrowserInitialized;
            isLoad = false;
            //  browser.RegisterAsyncJsObject("Alljs", Properties.Resources.jsAllLoadSkybet);
            var alljs = Properties.Resources.jsAllLoadSkybet;
            var countUpdate = 0;
            while (true)
            {
                Thread.Sleep(200);
                countUpdate++;
                Application.DoEvents();
                if (!browser.IsLoading)
                {
                    if (browser.IsBrowserInitialized)
                    {
                        var sss = browser.EvaluateScriptAsync(alljs).Result;
                        var result = browser.EvaluateScriptAsync("checkLoad()").Result;
                        if ((result.Result != null) && ((bool) result.Result == true))
                        {
                            var ss = browser.EvaluateScriptAsync(alljs).Result;
                            break;
                        }
                    }
                }
                if (countUpdate > 30)
                {
                    if (browser.IsBrowserInitialized)
                        browser.Delete();
                    browser.Dispose();
                    browser = Chrome.InitSkyBet(Url);
                    browser.ConsoleMessage += BrowserOnConsoleMessage;
                    browser.LoadingStateChanged += BrowserOnLoadingStateChanged;
                    browser.BrowserInitialized += BrowserOnBrowserInitialized;
                    isLoad = false;
                    countUpdate = 0;
                }
            }
        }
    

        public List<Scores> GetScoreses(string eventId)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                State = ThreadState.Unstarted;
                return default(List<Scores>);
            }
            State = ThreadState.Running;
            var scores = new List<Scores>();
            isLoad = false;
            LoadPageSkyBet(scoreUrl + eventId);
            var score = getScore();
            if (score.Result.ToString().Contains("null"))
            {
                score = getScore();
            }
            scores = JsonConvert.DeserializeObject<List<Scores>>(score.Result.ToString());
            isLoad = true;
            return scores;
        }

        private JavascriptResponse getScore()
        {
            var alljs = Properties.Resources.jsAllLoadSkybet;
            var ss = browser.EvaluateScriptAsync(alljs,TimeSpan.FromSeconds(1)).Result;
            var result = browser.EvaluateScriptAsync("suscribeEventsScoresSkyBet()", TimeSpan.FromSeconds(1)).Result;
            var ss5 = browser.EvaluateScriptAsync("suscribeEventsScoresSkyBet", TimeSpan.FromSeconds(1)).Result;
            if (ss5.Result.ToString().Length < 20)
            {
                var ss4 = browser.EvaluateScriptAsync(alljs).Result;
                var result5 = browser.EvaluateScriptAsync("suscribeEventsScoresSkyBet()", TimeSpan.FromSeconds(1)).Result;
            }
 
            if (result.Message != null)
            {
                var ss2 = browser.EvaluateScriptAsync(alljs).Result;
                var res = browser.EvaluateScriptAsync("suscribeEventsScoresSkyBet()", TimeSpan.FromSeconds(1)).Result;
            }
            var score = browser.EvaluateScriptAsync("getCurrentScore()", TimeSpan.FromSeconds(1)).Result;
            if (score.Message != null)
            {
                var res = browser.EvaluateScriptAsync(alljs).Result;
                var res2 = browser.EvaluateScriptAsync("suscribeEventsScoresSkyBet()", TimeSpan.FromSeconds(1)).Result;
                score = browser.EvaluateScriptAsync("getCurrentScore()", TimeSpan.FromSeconds(1)).Result;
            }
            //getScreenShot("BEDA");
            return score;
        }
    }
}
