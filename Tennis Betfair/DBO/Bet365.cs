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

namespace Tennis_Betfair.DBO
{
    public class Bet365
    {
        private ChromiumWebBrowser browser;
        private bool isLoad;
        private string prevComp;
        public static event AllMarkets.PlayerChanged PlayerChanged;

        public Bet365()
        {
            Debug.WriteLine("Start browser");
            var cef = Chrome.Instanse;
            isLoad = false;
            browser = Chrome.InstBet365;
            browser.ConsoleMessage += BrowserOnConsoleMessage;

        }

        private void BrowserOnConsoleMessage(object sender, ConsoleMessageEventArgs consoleMessageEventArgs)
        {     
            Console.WriteLine("Console: " + consoleMessageEventArgs.Message);
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

                var player1 = new Player(scores[0].name,scores[0].score,TypeDBO.Bet365);
                var player2 = new Player(scores[1].name, scores[1].score, TypeDBO.Bet365);
                var eventId = player1.Name + "|" + player2.Name;
                var market = new Market(null, player1, player2, eventId, TypeDBO.Bet365);
                PlayerChanged?.Invoke(new ScoreUpdEventArgs(market));
            }
        }


        private void BrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs loadingStateChangedEventArgs)
        {
            var allJs = Properties.Resources.jsAllLoad;
            var matheses = new List<Mathes>();

            if (loadingStateChangedEventArgs.IsLoading) return;
            if (!isLoad)
                return;
            getScreenShot("one");
            Thread.Sleep(50);
            var all = browser.EvaluateScriptAsync(allJs);
            all.ContinueWith(task2 =>
            {
                Thread.Sleep(100);
                var click = browser.EvaluateScriptAsync("clickToTennis()");
                Thread.Sleep(50);
                if (click.Result.Success != true)
                {
#if DEBUG
                   // getScreenShot("two");
#endif
                    throw new Exception("Exeption in stateChanged" + click.Result.Message);
                }

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
                return MarketStatus.Closed;
            var result = ClickToMarket(index);
            Thread.Sleep(200);
            if (result != true)
                return MarketStatus.Closed;
            var suscribe = browser.EvaluateScriptAsync("suscribeEventsScores()");
            if (suscribe.Result.Success)
            {
                getScreenShot("END");
                return MarketStatus.Open;
            }
            return MarketStatus.Closed;
        }

        public bool ClickToMarket(int index)
        {
            Thread.Sleep(50);
            var ret =  browser.EvaluateScriptAsync($"clickToMathes({index})").Result.Success;
            Thread.Sleep(50);
            Debug.WriteLine("Index to click: " + index);
            return ret;
        }

        public List<Mathes> GetAlllMathes()
        {
            var allMarktes = new List<Mathes>();
            browser.EvaluateScriptAsync("clickToTennis()");
            var mathes = browser.EvaluateScriptAsync("getMathes()");
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
            var result = browser.EvaluateScriptAsync("getScore()");
            var score = new List<Scores>();
            Thread.Sleep(50);
            var res = result.Result;
            if (res.Result == null)
            {
                errmsg = res.Message;
                Debug.WriteLine("Exeption 'GetCurrentScores()': " + errmsg);
                //var cc = browser.EvaluateScriptAsync("clickToTennis()").Result;
                //var vv = browser.EvaluateScriptAsync("clickToMathes(0)").Result;
                int countUpdate = 0;
                while (true)
                {
                    result = browser.EvaluateScriptAsync("getScore()");
                    Thread.Sleep(100);
                    if (result.Result.Result != null)
                    {
                        res = result.Result;
                        break;
                    }
                    if (countUpdate > 25)
                    {
                        browser.Reload();
                        return null;
                    }
                }
                #if DEBUG
                  getScreenShot("six");
                #endif
                //throw new Exception("Ex in getCurrentScore: " + errmsg);
            }
            score = JsonConvert.DeserializeObject<List<Scores>>(res.Result.ToString());
            return score;
        }

        
    }
}
