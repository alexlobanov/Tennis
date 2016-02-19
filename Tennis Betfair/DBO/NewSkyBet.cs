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
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO.NewBet365;
using Tennis_Betfair.TO.NewSkyBet;
using Scores = Tennis_Betfair.TO.NewSkyBet.Scores;

namespace Tennis_Betfair.DBO
{
    public class NewSkyBet
    {
        private ChromiumWebBrowser browser;
        private bool isLoad;
        private string prevComp;
        public static event AllMarkets.PlayerChanged PlayerChanged;
        private const string mainURL = "https://www.skybet.com/tennis/";
        private const string scoreUrl = "https://www.skybet.com/tennis/tennis-live/event/";

        public NewSkyBet()
        {
            var cef = Chrome.Instanse;
            isLoad = false;
            browser = Chrome.InstSkyBet;
            browser.ConsoleMessage += BrowserOnConsoleMessage;
            browser.LoadingStateChanged += BrowserOnLoadingStateChanged;
        }

        private void BrowserOnConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            
        }

        private void BrowserOnLoadingStateChanged(object sender, LoadingStateChangedEventArgs loadingStateChangedEventArgs)
        {
            var allJs = Properties.Resources.jsAllLoadSkybet;
            var matheses = new List<Mathes>();
            if (loadingStateChangedEventArgs.IsLoading) return;
            if (!isLoad)
                return;
            getScreenShot("Skybet");
            Thread.Sleep(50);
            var all = browser.EvaluateScriptAsync(allJs);
            all.ContinueWith(task2 =>
            {
                Thread.Sleep(100);
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
            isLoad = false;
            browser.Reload();
        }

        public List<Markets> GetAlllMathes()
        {
            var allMarktes = new List<Markets>();
           // reloadToMainPage();
            var reloadAlljs = browser.EvaluateScriptAsync(Properties.Resources.jsAllLoadSkybet);
            Task<JavascriptResponse> mathes = null;
            reloadAlljs.ContinueWith(task =>
            {
                mathes = browser.EvaluateScriptAsync("getMarkets()");
                mathes.ContinueWith(task1 =>
                {
                    if (task1.Result.Result == null)
                    {
                        getScreenShot("ERROR");
                        throw new Exception("Exeption in stateChanged" + task1.Result.Message);
                    }
                    if (task1.Result.Result != null)
                        allMarktes = JsonConvert.DeserializeObject<List<Markets>>(task1.Result.Result.ToString());
                    Console.WriteLine("getMathes(): " + allMarktes.Count);
                    return allMarktes;
                });
            });
            var returnElem = mathes.Result.Result.ToString();
            return JsonConvert.DeserializeObject<List<Markets>>(returnElem); ;
        }

        public List<Scores> GetScoreses(string eventId)
        {
            var scores = new List<Scores>();
            var alljs = Properties.Resources.jsAllLoadSkybet;
            browser.Load(scoreUrl + eventId);
            while (true)
            {
                if (!isLoad)
                {
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                else
                {
                    break;
                }
            }
            var ss = browser.EvaluateScriptAsync(alljs);
            if (!ss.Result.Message.Equals(""))
                GetScoreses(eventId);
            var result = browser.EvaluateScriptAsync("suscribeEventsScores()");
            if (!result.Result.Message.Equals(""))
            {
                GetScoreses(eventId);
            }
            var score = browser.EvaluateScriptAsync("getCurrentScore()");
            if (score.Result != null)
                scores = JsonConvert.DeserializeObject<List<Scores>>(score.Result.Result.ToString());
            return scores;
        } 


    }
}
