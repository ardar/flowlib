using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Guide
{
    public partial class UCInstallationMode : UserControl
    {
        int intMode = 0;

        public int Mode
        {
            get { return intMode; }
        }

        public UCInstallationMode()
        {
            InitializeComponent();
        }

        private void UCInstallationMode_Load(object sender, EventArgs e)
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
    }
}
