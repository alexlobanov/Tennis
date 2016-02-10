using System;
using System.Collections.Generic;
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
                var html = cell.InnerHtml;

                if (!html.Contains("/tennis-live/event/"))
                    continue;
                var regexMathesEventId = Regex.Match(html, "(\\/)+([0-9]+)");
                var regexNamePlayers = Regex.Match(html, "([>]*.*[v].*.[<])");
                var eventId = regexMathesEventId.Value.Trim(new[] {'/'});
                var players = regexNamePlayers.Value.Trim(new[] {' ', '<'});
                var tmp = Regex.Match(players, "([A-z+-]\\w)+([A-z+-]*\\w)");
                var player1 = "";
                var player2 = "";
                if (players.Contains("/"))
                {
                    //two players in team
                    var team1 = tmp.Value + '/' + tmp.NextMatch().Value;
                    var team2 = tmp.NextMatch().NextMatch().Value + '/' +
                                tmp.NextMatch().NextMatch().NextMatch().Value;
                    player1 = team1;
                    player2 = team2;
                }
                else
                {
                    //one player in team
                    var team1 = tmp.Value + ' ' + tmp.NextMatch().Value;
                    var team2 = tmp.NextMatch().NextMatch().Value + ' ' +
                                tmp.NextMatch().NextMatch().NextMatch().Value;
                    player1 = team1;
                    player2 = team2;
                }             
                inplayMarkets.Add(new MarketInfo(player1 + " : " + player2, player1, player2, eventId));
            }
            markets = inplayMarkets;
            return inplayMarkets;
        }

        public ScoreInfo GetScoreInfo(string eventIdSkyBet)
        {
            //xpath = //*[@id='js-inner-content']/section[1]/table/tbody
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
