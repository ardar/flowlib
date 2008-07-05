using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Client.Interface
{
    public partial class MessageWindow : Form
    {
        public bool Close
        {
            get;
            set;
        }

        public MessageWindow()
        {
            Close = false;
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            //base.OnClosing(e);
            e.Cancel = !Close;
            if (!Close)
                this.Hide();
        }

        private void MessageWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
