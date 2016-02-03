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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Tennis_Betfair.DBO.ParserBet365;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using LoadedEventArgs = Tennis_Betfair.Events.LoadedEventArgs;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair
{
    public partial class MainForm : Syncfusion.Windows.Forms.MetroForm
    {
        private readonly AllMarkets _allMarkets;
        /*Events*/
        public static event ChangedCheckHandler CheckChange;
        public delegate void ChangedCheckHandler(ChangedCheckEventArgs checkEvent);
        /*End Events*/

        private volatile int toClickIndex;

        private int prevClickScore;
        private int prevPlayerOneScore;
        private int prevPlayerTwoScore;

        private bool isChengedScoreOne;
        private bool isChengedScoreTwo;
        private bool isStop;
        private int playerChecked;

        private TreeViewAdvMouseClickEventArgs e_prev;

        private Thread UiThread;
        public MainForm()
        {
            InitializeComponent();
            
            _allMarkets = new AllMarkets();
            Market.MarketChanged += OnMarketChangedEvent;
            AllMarkets.playerChanged += OnPlayerChanged;
            AllMarkets.LoadedEvent += OnLoadedEvent;

            UiThread = new Thread(Start);
            UiThread.Name = "UiThread";
            UiThread.Start();
            isStop = false;
         
        }

        private void OnLoadedEvent(LoadedEventArgs loadedEvent)
        {
            var thread = new Thread(() =>
            {
                if (loadedEvent.LoadedStarted)
                {
                    if (InvokeRequired)
                        panel1.Invoke(new System.Action(() => { panel1.Visible = false; }));
                    else
                        panel1.Visible = false;
                    LoadingAnimator.Wire(panel2);
                }
                if (loadedEvent.LoadedEnded)
                {
                    if (InvokeRequired)
                        panel1.Invoke(new System.Action(() => { panel1.Visible = true; }));
                    else
                        panel1.Visible = true;
                    LoadingAnimator.UnWire(panel2);
                }
            });
            thread.Start();
        }

        private void Start()
        {
            try
            {
                while (true)
                {
                    if (isStop) return;
                    var elem = _allMarkets.GetStatus();
                    if (elem == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    label11.Invoke(new System.Action(() =>
                    {
                        label11.Text = Closed.Nodes.Count.ToString();
                    }));
                    textBoxExt5.Invoke(new System.Action(() =>
                    {
                       getStateThread(elem);
                    }));
                    if ((elem?.State365 == ThreadState.Stopped) && (elem?.StateBetfair == ThreadState.Stopped))
                    {
                        radioButtonAdv1.Invoke(new System.Action(() =>
                        {
                         
                            var parse = e_prev.Node.Text.Split(':');

                            if (parse.Count() < 2)
                            {
                                Thread.Sleep(1000);
                                return;
                            }
                            radioButtonAdv1.Text = parse[0].Trim();
                            radioButtonAdv2.Text = parse[1].Trim();
                            textBoxExt7.Text = parse[0].Trim();
                            textBoxExt8.Text = parse[1].Trim();
                            digitalGauge1.Value = "Finished";
                            textBoxExt4.Text = "Finished";
                            textBoxExt4.BackColor = Color.Tomato;
                            textBoxExt1.Text = "END";
                            textBoxExt2.Text = "END";
                        }));

                    }
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        private void OnPlayerChanged(ScoreUpdEventArgs marketArgs)
        {
            
                textBoxExt1.Invoke(new System.Action(() =>
                {
                    radioButtonAdv1.Text = marketArgs.ChangetMarket.Player1.Name;
                    radioButtonAdv2.Text = marketArgs.ChangetMarket.Player2.Name;

                    textBoxExt7.Text = marketArgs.ChangetMarket.Player1.Name;
                    textBoxExt8.Text = marketArgs.ChangetMarket.Player2.Name;
                    var one = marketArgs.ChangetMarket.GetBetFairScore();
                    var two = marketArgs.ChangetMarket.GetBet365Score();
                    digitalGauge1.Value = marketArgs.ChangetMarket.GetNewScore();
                    textBoxExt3.Text = marketArgs.ChangetMarket.MarketName;

                   

                    switch (one)
                    {
                        case " : ":
                            one = "No score";
                            label5.Visible = true;
                            break;
                        default:
                            label5.Visible = false;
                            break;
                    }
                    switch (two)
                    {
                        case " : ":
                            two = "No score";
                            label4.Visible = true;
                            break;
                        default:
                            label4.Visible = false;
                            break;
                    }
                    if (marketArgs.ChangetMarket.IsClose)
                    {
                        one = "END";
                        two = "END";
                        digitalGauge1.Value = "Finished";
                        textBoxExt4.Text = "Finished";
                        textBoxExt4.BackColor = Color.Tomato;
                    }
                    else
                    {
                        textBoxExt4.Text = "In-Play";
                        textBoxExt4.BackColor = Color.SpringGreen;
                    }
                    textBoxExt1.Text = one;
                    textBoxExt2.Text = two;

                    ClickPlayer(marketArgs);

                    prevPlayerOneScore = Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne);
                    prevPlayerTwoScore = Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo);
                }));          
        }

        private void ClickPlayer(ScoreUpdEventArgs marketArgs)
        {
            switch (toClickIndex)
            {
                case 0:
                    break;
                case 15:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 15)
                            && (prevPlayerOneScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 15;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 15)
                            && (prevPlayerTwoScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 15;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 30:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 30)
                            && (prevPlayerOneScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 30;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 30)
                            && (prevPlayerTwoScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 30;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 40:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 40)
                            && (prevPlayerOneScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 40;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 40)
                            && (prevPlayerTwoScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 40;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 50:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 50)
                            && (prevPlayerOneScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 50;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 50)
                            && (prevPlayerTwoScore
                                != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 50;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
            }
        }



        private void OnMarketChangedEvent(MarketUpdEventArgs eventArgs)
        {
            var gameStr = eventArgs.ChangetMarket.Player1.Name +
                    "  :  " + eventArgs.ChangetMarket.Player2.Name;
            var isHaveElem = false;
            if (eventArgs.ChangetMarket.Player1.Name == null) return;
            for (var j = 0; j < Closed.Nodes.Count; j++)
            {
                if (Closed.Nodes[j].Text.Equals(gameStr))
                    isHaveElem = true;
            }
            if (!isHaveElem)
            {
                Closed.Nodes?.Add(new TreeNodeAdv(gameStr));
              
            }
        }

  



        private void buttonAdv1_Click(object sender, EventArgs e)
        {
          
            if ((_allMarkets?.GetStatus() != null))
            {
                _allMarkets.StopThreads();
                _allMarkets.AbortThreads();
                
            }
            // LoadingAnimator.Wire(treeViewAdv1);
            _allMarkets?.StartThreads();
            if ((_allMarkets?.GetStatus() != null))
            {
                treeViewAdv1_NodeMouseClick(null, e_prev);
            }
            //LoadingAnimator.UnWire(treeViewAdv1);
           
        }
 

    
        private ChangedCheckEventArgs _prev;

        private void treeViewAdv1_NodeMouseClick(object sender, TreeViewAdvMouseClickEventArgs e)
        {
            if (e.Node.HasChildren)
            {
                return;
            }
            panel1.Visible = false;
            LoadingAnimator.Wire(panel2);
            e_prev = e;           
            var players = e.Node.Text.Split(':');
            var player1Node = players[0].Trim();
            var player2Node = players[1].Trim();
            foreach (var market in _allMarkets.AllMarketsHashSet)
            {
                //*Check event*/
                if ((market.Player1.Name != player1Node) 
                    && (market.Player2.Name != player2Node)) continue;
                var eventIdBetfair = market.BetfairEventId;
                var eventId365 = market.Bet365EventId;
                if ((eventIdBetfair == null) && (eventId365 == null)) _allMarkets.AllMarketsHashSet.Remove(market);

                Debug.WriteLine("Event: " + eventId365 + " : " + eventIdBetfair);
                 
                CheckChange?.Invoke(
                    new ChangedCheckEventArgs(eventIdBetfair, eventId365)
                    );
                    
                Debug.WriteLine("Ok-Invoke");                   
                break;
            }
            LoadingAnimator.UnWire(panel2,1700);
            panel1.Visible = true;
        }

        private void MainForm_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            _allMarkets.StopThreads();
            isStop = true;
        }

        private void treeViewAdv1_OnNodeReplaced(object sender, TreeNodeAdvOnReplacedArgs e)
        {
            Debug.WriteLine("OKOKOK");
        }

        private void digitalGauge1_Click(object sender, EventArgs e)
        {


        }

        private void radioButtonAdv9_CheckChanged(object sender, EventArgs e)
        {
            _allMarkets.MarketIgnore(2);
            var elem = _allMarkets.GetStatus();
            getStateThread(elem);
            //bet365
        }

        private void radioButtonAdv8_CheckChanged(object sender, EventArgs e)
        {
            _allMarkets.MarketIgnore(1);//
            var elem = _allMarkets.GetStatus();
            getStateThread(elem);

        }

        private void radioButtonAdv10_CheckChanged(object sender, EventArgs e)
        {
            _allMarkets.MarketIgnore(0);
            var elem = _allMarkets.GetStatus();

            getStateThread(elem);
            //not ignore
        }

        private void getStateThread(ThreadStatus elem)
        {
            switch (elem.StateBetfair)
            {
                case ThreadState.Running:
                    textBoxExt5.Text = "alive";
                    textBoxExt5.BackColor = Color.Aquamarine;
                    break;
                case ThreadState.Aborted:
                    textBoxExt5.Text = "aborted";
                    textBoxExt5.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Stopped:
                    textBoxExt5.Text = "stopped";
                    textBoxExt5.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Suspended:
                    textBoxExt5.Text = "ignore";
                    textBoxExt5.BackColor = Color.Yellow;
                    break;
                case ThreadState.SuspendRequested:
                    textBoxExt5.Text = "wait for ignore";
                    textBoxExt5.BackColor = Color.Yellow;
                    break;
            }
            switch (elem.State365)
            {
                case ThreadState.Running:
                    textBoxExt6.Text = "alive";
                    textBoxExt6.BackColor = Color.Aquamarine;
                    break;
                case ThreadState.Aborted:
                    textBoxExt6.Text = "aborted";
                    textBoxExt6.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Stopped:
                    textBoxExt6.Text = "stopped";
                    textBoxExt6.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Suspended:
                    textBoxExt6.Text = "ignore";
                    textBoxExt6.BackColor = Color.Yellow;
                    break;
                case ThreadState.SuspendRequested:
                    textBoxExt6.Text = "wait for ignore";
                    textBoxExt6.BackColor = Color.Yellow;
                    break;
            }
        }

        private void buttonAdv2_Click(object sender, EventArgs e)
        {

        }

        private void radioButtonAdv3_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 0;
            prevClickScore = -1;
            //0
        }

        private void radioButtonAdv4_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 15;
            prevClickScore = -1;
            //15
        }

        private void radioButtonAdv5_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 30;
            prevClickScore = -1;
            //30
        }

        private void radioButtonAdv6_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 40;
            prevClickScore = -1;
            //40
        }

        private void radioButtonAdv7_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 50;
            prevClickScore = -1;

            //Adv
        }

        private void radioButtonAdv11_CheckChanged(object sender, EventArgs e)
        {
            toClickIndex = 0;
            prevClickScore = -1;

            //No
        }

        private void treeViewAdv1_Click(object sender, EventArgs e)
        {

        }

        private void radioButtonAdv1_CheckChanged(object sender, EventArgs e)
        {
            playerChecked = 1;
        }

        private void radioButtonAdv2_CheckChanged(object sender, EventArgs e)
        {
            playerChecked = 2;
        }
    }
}
