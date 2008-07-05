using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Guide
{
    public partial class UCConnection : UserControl
    {
        public int Mode
        {
            get;
            protected set;
        }

        public UCConnection()
        {
            InitializeComponent();
        }

        private void UCConnection_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(typeof(Program), @"Images.flowlib-main.jpg");
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Mode = 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Mode = 1;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Mode = 2;
        }
    }
}
