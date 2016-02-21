using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.OffScreen;

namespace Tennis_Betfair.DBO
{
    public class Chrome
    {
        private static Chrome _instanse;
        private static ChromiumWebBrowser bet365ChromiumWebBrowser;
        private static ChromiumWebBrowser skyBetChromiumWebBrowser;

        private const string Bet365URL = "https://mobile.bet365.com/?apptype=&appversion=&cb=1455792521#type=InPlay;key=;ip=1;lng=1";
        private const string SkyBetURL = "https://www.skybet.com/tennis";

        private Chrome()
        {
            var settings = new CefSettings();
            Cef.Initialize(settings, shutdownOnProcessExit: true, performDependencyCheck: false);
        }

        private static ChromiumWebBrowser InitBet365()
        {
            return new ChromiumWebBrowser(Bet365URL);
        }

        private static ChromiumWebBrowser InitSkyBet()
        {
            return new ChromiumWebBrowser(SkyBetURL);
        }

        public static ChromiumWebBrowser InitSkyBet(string URL)
        {
            skyBetChromiumWebBrowser.Dispose();
            skyBetChromiumWebBrowser = new ChromiumWebBrowser(URL);
            return skyBetChromiumWebBrowser;
        }

        public static Chrome Instanse
        {
            get
            {
                if (_instanse == null)
                {
                    _instanse = new Chrome();
                }
                return _instanse;
            }
        }

        public static ChromiumWebBrowser InstSkyBet
        {
            get
            {
                if (skyBetChromiumWebBrowser == null)
                {
                    skyBetChromiumWebBrowser = InitSkyBet();
                }
                return skyBetChromiumWebBrowser;
            }
        }
        public static ChromiumWebBrowser InstBet365
        {
            get
            {
                if (bet365ChromiumWebBrowser == null)
                {
                    bet365ChromiumWebBrowser = InitBet365();
                }
                return bet365ChromiumWebBrowser;
            }
        }

        

    }
}
