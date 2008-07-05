using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Guide
{
    public partial class UCFinished : UserControl
    {
        public UCFinished()
        {
            InitializeComponent();
        }

        private void UCFinished_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(typeof(Program), @"Images.flowlib-main.jpg");
        }
    }
}
