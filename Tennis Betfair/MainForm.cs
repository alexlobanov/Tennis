#region Copyright Syncfusion Inc. 2001-2016.
// Copyright Syncfusion Inc. 2001-2016. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Tennis_Betfair.DBO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Syncfusion.Windows.Forms.Tools;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;

namespace Tennis_Betfair
{
    public partial class MainForm : Syncfusion.Windows.Forms.MetroForm
    {
        private readonly AllMarkets _allMarkets;
        /*Events*/
        public static event ChangedCheckHandler CheckChange;
        public delegate void ChangedCheckHandler(ChangedCheckEventArgs checkEvent);

        public static event UnCheckHandler UnCheck;
        public delegate void UnCheckHandler(UnCheckEventsArgs unCheckEvent);
        /*End Events*/


        public MainForm()
        {
            InitializeComponent();
            
            _allMarkets = new AllMarkets();
            Market.MarketChanged += OnMarketChangedEvent;
            AllMarkets.playerChanged += OnPlayerChanged;
        }

        private void OnPlayerChanged(ScoreUpdEventArgs marketArgs)
        {
            if (InvokeRequired)
            {
                label1.Invoke(new System.Action(() =>
                {
                    label1.Text = marketArgs.ChangetMarket.GetNewScore();
                    label3.Text = marketArgs.ChangetMarket.GetBet365Score();
                    label4.Text = marketArgs.ChangetMarket.GetBetFairScore();
                }));
            }
        }


        private void OnMarketChangedEvent(MarketUpdEventArgs eventArgs)
        {
            var gameStr = eventArgs.ChangetMarket.Player1.Name +
                       "  :  " + eventArgs.ChangetMarket.Player2.Name;
            var isHaveElem = false;
            for (var j = 0; j < treeViewAdv1.Nodes.Count; j++)
            {
                if (treeViewAdv1.Nodes[j].Text.Equals(gameStr))
                    isHaveElem = true;
            }
            if (!isHaveElem)
            {
                treeViewAdv1.Nodes.Add(new TreeNodeAdv(gameStr));
            }
        }




        private void buttonAdv1_Click(object sender, EventArgs e)
        {
            _allMarkets.StartThreads();
        }
 

        private void ToHashCompetion(HashSet<Market> all)
        {
            HashSet<string> competiontype = new HashSet<string>();
            foreach (var market in all)
            {
                competiontype.Add(market.MarketName);
            }
            foreach (var type in competiontype)
            {
                treeViewAdv1.Nodes.Add(new TreeNodeAdv(type));
            }
        }

        private TreeViewAdvMouseClickEventArgs _prev;

        private void treeViewAdv1_NodeMouseClick(object sender, TreeViewAdvMouseClickEventArgs e)
        {
            if (e.Node.HasChildren) return;
            if ((_prev != null) && (_prev.Node != e.Node))
            {
                UnCheck?.Invoke(new UnCheckEventsArgs(_prev.Node));
            }
            
            var players = e.Node.Text.Split(':');
            var player1Node = players[0].Trim();
            var player2Node = players[1].Trim();
            foreach (var market in _allMarkets.AllMarketsHashSet)
            {
                //*UnCheckElem event*/
                if ((_prev != null) && (!_prev.Node.Equals(e.Node)))
                {
                    var gameStr = market.Player1.Name +
                       "  :  " + market.Player2.Name;
                    if (_prev.Node.Text.Equals(gameStr))
                    {
                        UnCheck?.Invoke(
                            new UnCheckEventsArgs(market.BetfairEventId, market.Bet365EventId)
                            );
                    }    
                }
                //*Check event*/
                if ((market.Player1.Name == player1Node) 
                    || (market.Player2.Name == player2Node))
                {
                    var eventIdBetfair = market.BetfairEventId;
                    var eventId365 = market.Bet365EventId;
                    CheckChange?.Invoke(
                        new ChangedCheckEventArgs(eventIdBetfair, eventId365)
                        );
                }

            }
            _prev = e;
        }

        private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            _allMarkets.StopThreads();
        }

        private void treeViewAdv1_OnNodeReplaced(object sender, TreeNodeAdvOnReplacedArgs e)
        {
            Debug.WriteLine("OKOKOK");
        }
    }
}
