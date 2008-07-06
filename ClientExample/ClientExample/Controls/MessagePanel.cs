using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Controls
{
    public partial class MessagePanel : UserControl
    {
        public MessagePanel()
        {
            InitializeComponent();
        }

        public void AddMessage(string msg)
        {
            txtOutput.AppendText(msg + "\r\n");
        }

        private void MessagePanel_Load(object sender, EventArgs e)
        {
            toolBtnIgnore.Image = new Bitmap(typeof(Program), @"Images.ignore.gif");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}
