using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Tennis_Betfair.TO.SkyBet;

namespace Tennis_Betfair.DBO
{
    public class SkyBet
    {
        private static List<MarketInfo> markets; 
        public List<MarketInfo> GetMartches()
        {
            var doc = new HtmlWeb().Load("https://m.skybet.com/tennis");
            var rows = doc?.DocumentNode.SelectNodes("//*[@id='js-inner-content']/ul/li[1]/table/tbody");
            if (rows == null) return null;
            var inplayMarkets = new List<MarketInfo>();
            markets = new List<MarketInfo>();

            var cells = rows[0].SelectNodes("./tr");
            foreach (var cell in cells)
            {
                try
                {
                    var html = cell.InnerHtml;

                    if (!html.Contains("/tennis-live/event/"))
                        continue;
                    var regexMathesEventId = Regex.Match(html, "(\\/)+([0-9]+)");
                    var regexNamePlayers = Regex.Match(html, "([>]*.*[v].*.[<])");
                    var eventId = regexMathesEventId.Value.Trim(new[] {'/'});
                    var players = regexNamePlayers.Value.Trim(new[] {' ', '<'});

                    var playersList = Regex.Split(players, " v ");

                    var tmpOne = Regex.Split(playersList[0], ">");
                    var tmpTwo = Regex.Split(playersList[1], ">");

                    var plr1 = (tmpOne[tmpOne.Length - 1].Trim(' '));
                    var plr2 = (tmpTwo[tmpTwo.Length - 1].Trim(' '));

                    var player1 = "";
                    var player2 = "";
                    var isFirst = false;
                    if (plr1.Contains("*"))
                    {
                        var tmpParse = plr1.Split('*');
                        player1 = tmpParse[0].Length > tmpParse[1].Length ? tmpParse[0] : tmpParse[1];
                        isFirst = true;
                    }
                    if (plr2.Contains("*"))
                    {
                        var tmpParse = plr2.Split('*');
                        player2 = tmpParse[0].Length > tmpParse[1].Length ? tmpParse[0] : tmpParse[1];
                    }
                    if (isFirst)
                    {
                        player2 = RemoveDigit(plr2);
                    }
                    else
                    {
                        player1 = RemoveDigit(plr1);
                    }
                    player1 = player1.Trim();
                    player2 = player2.Trim();
                    inplayMarkets.Add(new MarketInfo(player1 + " : " + player2, player1, player2, eventId));
                }
                catch (Exception)
                {
                    Debug.WriteLine("[SKYBET] Exeption parse in [GetMartches]");
                    continue;
                }
            }
            markets = inplayMarkets;
            return inplayMarkets;
        }

        private string RemoveDigit(string strToClear)
        {
            string str = "";
            for (int i = 0; i < strToClear.Length; i++)
            {
                if (!char.IsDigit(strToClear[i]))
                {
                    str += strToClear[i];
                }
            }
            return str;
        }

        public ScoreInfo GetScoreInfo(string eventIdSkyBet)
        {
            var doc = new HtmlWeb().Load("https://m.skybet.com/tennis/tennis-live/event/" + eventIdSkyBet);
            var rows = doc?.DocumentNode.SelectSingleNode("//*[@id='js-inner-content']/section[1]/table/tbody");
            if (rows == null) return null;
            var html = rows.InnerHtml;
            var regexScore = Regex.Matches(html, "(points..[0-90+Ad+Adv+A]*)");

            var scoreFirstPlayer = regexScore[0].Value.Remove(0,8);
            var scoreSecondPlayer = regexScore[1].Value.Remove(0, 8);
            var player1 = "";
            var player2 = "";

            foreach (var marketInfo in markets)
            {
                if (marketInfo.EventId == eventIdSkyBet)
                {
                    player1 = marketInfo.Player1;
                    player2 = marketInfo.Player2;
                    break;
                }
            }

            return new ScoreInfo(player1,player2,scoreFirstPlayer,scoreSecondPlayer, eventIdSkyBet);
        } 
    }
}
