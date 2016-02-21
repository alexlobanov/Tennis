#region Copyright Syncfusion Inc. 2001-2016.

// Copyright Syncfusion Inc. 2001-2016. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 

#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using CefSharp;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Tennis_Betfair.DBO;
using Tennis_Betfair.Events;
using Tennis_Betfair.Tennis;
using Tennis_Betfair.TO;
using Action = System.Action;
using ThreadState = System.Threading.ThreadState;

namespace Tennis_Betfair
{
    public partial class MainForm : MetroForm
    {
        public delegate void ChangedCheckHandler(ChangedCheckEventArgs checkEvent);

        public delegate void ChengedMessage(MessagesEventArgs messagesEvent);

        
        private readonly AllMarkets _allMarkets;

        private readonly Thread UiThread;
        private readonly Thread CheckConnetionThread;

        private TreeViewAdvMouseClickEventArgs e_prev;

        private bool isChengedScoreOne;
        private bool isChengedScoreTwo;
        private bool isStop;
        private int playerChecked;

        private int prevClickScore;
        private int prevPlayerOneScore;
        private int prevPlayerTwoScore;
        /*End Events*/

        private volatile int toClickIndex;

        public MainForm()
        {
            InitializeComponent();

            _allMarkets = new AllMarkets();
            Market.MarketChanged += OnMarketChangedEvent;
            ParsingInfo.PlayerChanged += OnPlayerChanged;

            Bet365.PlayerChanged += OnPlayerChanged;
            NewSkyBet.PlayerChanged += OnPlayerChanged;

            Bet365.MessageChanged += OnMessageChanged;
            NewSkyBet.MessageChanged += OnMessageChanged;

            AllMarkets.LoadedEvent += OnLoadedEvent;

            

            UiThread = new Thread(Start) {Name = "UiThread"};
            UiThread.Start();

            CheckConnetionThread = new Thread(CheckConnetion) {Name = "CheckStatus"};
            CheckConnetionThread.Start();
            isStop = false;
        }

        private void OnMessageChanged(MessagesEventArgs messagesEvent)
        {
             var stringToInsert = "";
            switch (messagesEvent.TypeDbo)
            {
                case TypeDBO.None:
                    break;
                case TypeDBO.BetFair:
                    break;
                case TypeDBO.Bet365:
                    stringToInsert = "Player1: " + messagesEvent.MessagePlayerOne + "              Player2: " + messagesEvent.MessagePlayerTwo;
                    labelBet365Info.Invoke(new System.Action(() =>
                    {
                        labelBet365Info.Text = stringToInsert;
                    }));
                    break;
                case TypeDBO.SkyBet:
                    stringToInsert = messagesEvent.Message;
                    labelSkyInfo.Invoke(new System.Action(() =>
                    {
                        labelSkyInfo.Text = stringToInsert;
                    }));
                    break;
            }
        }

        private void CheckConnetion()
        {
            while (true)
            {
                if (isStop) return;
                if (CheckInternetConenction.CheckGoogle() != StatusInternet.Ok)
                {
                    ChangeImageConnection(pictureBox365, CheckInternetConenction.CheckConnection(TypeDBO.Bet365));
                    ChangeImageConnection(pictureBoxBF, CheckInternetConenction.CheckConnection(TypeDBO.BetFair));
                    ChangeImageConnection(pictureBoxSB, CheckInternetConenction.CheckConnection(TypeDBO.SkyBet));
                    Thread.Sleep(10000);
                }
                else
                {
                    ChangeImageConnection(pictureBox365, CheckInternetConenction.CheckConnection(TypeDBO.Bet365));
                    ChangeImageConnection(pictureBoxBF, CheckInternetConenction.CheckConnection(TypeDBO.BetFair));
                    ChangeImageConnection(pictureBoxSB, CheckInternetConenction.CheckConnection(TypeDBO.SkyBet));
                    Thread.Sleep(6000);
                }
            }
        }

        private void ChangeImageConnection(PictureBox picture, StatusInternet status)
        {
            switch (status)
            {
                case StatusInternet.Ok:
                    picture.Image = Properties.Resources.green;
                    break;
                case StatusInternet.BigDelay:
                    picture.Image = Properties.Resources.yellow;
                    break;
                case StatusInternet.NoAvirable:
                    picture.Image = Properties.Resources.red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        /*Events*/
        public static event ChangedCheckHandler CheckChange;

        private void OnLoadedEvent(LoadedEventArgs loadedEvent)
        {
            var thread = new Thread(() =>
            {
                if (loadedEvent.LoadedStarted)
                {
                    if (InvokeRequired)
                        panel1.Invoke(new Action(() => { panel1.Visible = false; }));
                    else
                        panel1.Visible = false;
                    LoadingAnimator.Wire(panel2);
                }
                if (loadedEvent.LoadedEnded)
                {
                    if (InvokeRequired)
                        panel1.Invoke(new Action(() => { panel1.Visible = true; }));
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
                    label11.Invoke(new Action(() => { label11.Text = Closed.Nodes.Count.ToString(); }));
                    textBoxExt5.Invoke(new Action(() => { getStateThread(elem); }));
                    if ((elem?.State365 == ThreadState.Stopped) && (elem?.StateBetfair == ThreadState.Stopped) && (elem.StateSky == ThreadState.Stopped))
                    {
                        radioButtonAdv1.Invoke(new Action(() =>
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
                            textBoxStatus.Text = "Finished";
                            textBoxStatus.BackColor = Color.Tomato;
                            textBoxScoreBetfair.Text = "END";
                            textBoxScoreBet365.Text = "END";
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
            textBoxScoreBetfair.Invoke(new Action(() =>
            {
                radioButtonAdv1.Text = marketArgs.ChangetMarket.Player1.Name;
                radioButtonAdv2.Text = marketArgs.ChangetMarket.Player2.Name;

                textBoxExt7.Text = marketArgs.ChangetMarket.Player1.Name;
                textBoxExt8.Text = marketArgs.ChangetMarket.Player2.Name;

                var betFairScore = marketArgs.ChangetMarket.GetBetfairS();
                var bet365Score = marketArgs.ChangetMarket.Get365S();
                var skybetScore = marketArgs.ChangetMarket.GetSkyBetS();

                digitalGauge1.Value = marketArgs.ChangetMarket.GetNewS();

                betFairScore = CheckForNullScore(betFairScore, labelBetfairInfo, TypeDBO.BetFair);
                bet365Score = CheckForNullScore(bet365Score, labelBet365Info, TypeDBO.Bet365);
                skybetScore = CheckForNullScore(skybetScore, labelSkyInfo, TypeDBO.SkyBet);

                if (marketArgs.ChangetMarket.IsClose)
                {
                    betFairScore = "END";
                    bet365Score = "END";
                    skybetScore = "END";
                    digitalGauge1.Value = "Finished";
                    textBoxStatus.Text = "Finished";
                    textBoxStatus.BackColor = Color.Tomato;
                }
                else
                {
                    textBoxStatus.Text = "In-Play";
                    textBoxStatus.BackColor = Color.SpringGreen;
                }
                textBoxScoreBetfair.Text = betFairScore;
                textBoxScoreBet365.Text = bet365Score;
                textBoxScoreSky.Text = skybetScore;

                ClickPlayer(marketArgs);

                prevPlayerOneScore = Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne);
                prevPlayerTwoScore = Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo);
            }));
        }

        private string CheckForNullScore(string score, Label labelToView,TypeDBO type)
        {
            switch (type)
            {
                case TypeDBO.BetFair:
                    if (score.Equals(" : "))
                    {
                        score = "No score";
                        labelToView.Text = "No game information on Betfair. It can be tried to update market.";
                    }
                    break;
                case TypeDBO.Bet365:
                    if (score.Equals(" : "))
                    {
                        score = "No score";
                        labelToView.Text = "No game information on Bet365. It can be tried to update market.";
                    }
                    break;
                case TypeDBO.SkyBet:
                    if (score.Equals(" : "))
                    {
                        score = "No score";
                        labelToView.Text = "No game information on SkyBet. It can be tried to update market.";
                    }
                    break;
            }
            return score;
        }

        private void ClickPlayer(ScoreUpdEventArgs marketArgs)
        {
            switch (toClickIndex)
            {
                case 0:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 0) && (prevPlayerOneScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 0;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 0) && (prevPlayerTwoScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 0;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 15:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 15) && (prevPlayerOneScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 15;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 15) && (prevPlayerTwoScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 15;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 30:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 30) && (prevPlayerOneScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 30;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 30) && (prevPlayerTwoScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 30;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 40:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 40) && (prevPlayerOneScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 40;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 40) && (prevPlayerTwoScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 40;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
                case 50:
                    if (radioButtonAdv1.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne) == 50) && (prevPlayerOneScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewOne)))
                        {
                            prevClickScore = 50;
                            SimulateMouseClick.DoMouseClick();
                        }
                    if (radioButtonAdv2.Checked)
                        if ((Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo) == 50) && (prevPlayerTwoScore != Player.toIntScore(marketArgs.ChangetMarket.ScoreNewTwo)))
                        {
                            prevClickScore = 50;
                            SimulateMouseClick.DoMouseClick();
                        }
                    break;
            }
        }


        private void OnMarketChangedEvent(MarketUpdEventArgs eventArgs)
        {
            var gameStr = eventArgs.ChangetMarket.Player1.Name + "  :  " + eventArgs.ChangetMarket.Player2.Name;
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
            }
            _allMarkets?.StartThreads();
            if ((_allMarkets?.GetStatus() != null))
            {
                treeViewAdv1_NodeMouseClick(null, e_prev);
            }
        }

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
            foreach (var market in _allMarkets.ParsingInfo.AllMarketsHashSet)
            {
                //*Check event*/
                if ((!(market.Player1.Name.Equals(player1Node))) && (!(market.Player2.Name.Equals(player2Node))))
                    continue;

                var eventIdBetfair = market.BetfairEventId;
                var eventId365 = player1Node + "|" + player2Node;
                var eventIdSky = market.SkyBetEventId;

                if ((eventIdBetfair == null) && (string.IsNullOrEmpty(eventId365)) && (eventIdSky == null)) _allMarkets.ParsingInfo.AllMarketsHashSet.Remove(market);

                Debug.WriteLine("Event: " + eventId365 + " : " + eventIdBetfair);

                CheckChange?.Invoke(new ChangedCheckEventArgs(eventIdBetfair, eventId365, eventIdSky));

                Debug.WriteLine("Ok-Invoke");
                break;
            }
            LoadingAnimator.UnWire(panel2, 1700);
            panel1.Visible = true;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Cef.Shutdown();
            _allMarkets.StopThreads();
            isStop = true;
            _allMarkets.isStop = true;
            
        }

        private void treeViewAdv1_OnNodeReplaced(object sender, TreeNodeAdvOnReplacedArgs e)
        {
            Debug.WriteLine("OKOKOK");
        }

        private void digitalGauge1_Click(object sender, EventArgs e)
        {
        }


        private void updateTextBoxWithState(TextBoxExt textBox, ThreadState state)
        {
            switch (state)
            {
                case ThreadState.Running:
                    textBox.Text = "alive";
                    textBox.BackColor = Color.Aquamarine;
                    break;
                case ThreadState.Aborted:
                    textBox.Text = "aborted";
                    textBox.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Stopped:
                    textBox.Text = "stopped";
                    textBox.BackColor = Color.LightSalmon;
                    break;
                case ThreadState.Suspended:
                    textBox.Text = "ignore";
                    textBox.BackColor = Color.Yellow;
                    break;
                case ThreadState.SuspendRequested:
                    textBox.Text = "wait for ignore";
                    textBox.BackColor = Color.Yellow;
                    break;
            }
        }

        private void getStateThread(ThreadStatus elem)
        {
            updateTextBoxWithState(textBoxExt9, elem.StateSky);
            updateTextBoxWithState(textBoxExt5, elem.StateBetfair);
            updateTextBoxWithState(textBoxExt6, elem.State365);
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

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void checkBoxNotIgnore_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxBetfair.Checked)
                _allMarkets.MarketIgnore(TypeDBO.BetFair);
            if (checkBoxBet365.Checked)
                _allMarkets.MarketIgnore(TypeDBO.Bet365);
            if (checkBoxSky.Checked)
                _allMarkets.MarketIgnore(TypeDBO.SkyBet);
        }

        private void checkBoxBetfair_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxBetfair.Checked)
            {
                _allMarkets.MarketIgnore(TypeDBO.BetFair);
            }
            else
            {
                _allMarkets.UnMarketIngore(TypeDBO.BetFair);
            }
            var elem = _allMarkets.GetStatus();
            getStateThread(elem);
        }

        private void checkBoxBet365_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxBet365.Checked)
            {
                _allMarkets.MarketIgnore(TypeDBO.Bet365);
            }
            else
            {
                _allMarkets.UnMarketIngore(TypeDBO.Bet365);
            }
            var elem = _allMarkets.GetStatus();
            getStateThread(elem);
        }

        private void checkBoxSky_CheckStateChanged(object sender, EventArgs e)
        {
            if (checkBoxSky.Checked)
            {
                _allMarkets.MarketIgnore(TypeDBO.SkyBet);
            }
            else
            {
                _allMarkets.UnMarketIngore(TypeDBO.SkyBet);
            }
            var elem = _allMarkets.GetStatus();
            getStateThread(elem);
        }

        private void buttonAdv2_Click(object sender, EventArgs e)
        {
        }

        private void checkBoxBetfair_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            _allMarkets.AbortThreads();
            CheckConnetionThread.Abort();
        }


        private void button1_Click(object sender, EventArgs e)
        {
        }

        private void button2_Click(object sender, EventArgs e)
        {
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}