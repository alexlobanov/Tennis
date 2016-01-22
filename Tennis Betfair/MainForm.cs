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
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tennis_Betfair
{
    public partial class MainForm : Syncfusion.Windows.Forms.MetroForm
    {
        
        public MainForm()
        {
            InitializeComponent();
        }

        private void buttonAdv1_Click(object sender, EventArgs e)
        {
            var betfair = new Betfair();
            var elem = betfair.GetInPlayAllMarkets();
            var elem2 = betfair.GetScoreEvent(elem.FirstOrDefault().eventId);
        }
    }
}
