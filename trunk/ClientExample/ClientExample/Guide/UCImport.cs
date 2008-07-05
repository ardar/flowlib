using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using FlowLib.Containers;

namespace ClientExample.Guide
{
    public partial class UCImport : UserControl
    {
        protected int intMode = 0;
        protected List<HubSetting> hubSettings = new List<HubSetting>();

        public int Mode
        {
            get { return intMode; }
            set { intMode = value; }
        }

        public List<HubSetting> HubSettings
        {
            get { return hubSettings; }
        }

        public UCImport()
        {
            InitializeComponent();
        }

        private void UCImport_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(typeof(Program), @"Images.flowlib-main.jpg");
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            intMode = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            intMode = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            intMode = 2;
        }
    }
}
