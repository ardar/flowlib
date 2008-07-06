using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FlowLib.Containers;

namespace ClientExample.Client.Interface
{
    public partial class MainWindow : Form
    {
        protected MessageWindow msgWindow = new MessageWindow();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            toolBtnUsers.Image = new Bitmap(typeof(Program), @"Images.user.gif");
            toolBtnMessages.Image = new Bitmap(typeof(Program), @"Images.pen.gif");

            ClientExample.Controls.HubControl hub = new ClientExample.Controls.HubControl();
            msgWindow.AddHub(hub);
            hub.HubUpdate += new FlowLib.Events.FmdcEventHandler(hub_Update);
            list1.Items.Add(hub);

            HubSetting setting = new HubSetting();
            setting.DisplayName = "Xmpl";
            setting.Address = "mrmikejj.co.uk";
            setting.Port = 1669;
            setting.Protocol = "Nmdc";
            hub.Setting = setting;
        }

        void hub_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {

        }

        private void toolBtnUsers_Click(object sender, EventArgs e)
        {

        }

        private void toolBtnMessages_Click(object sender, EventArgs e)
        {
            Screen screen = Screen.FromControl(this);
            int pos = this.Left + this.Width + msgWindow.Width;
            msgWindow.Show();
            msgWindow.WindowState = FormWindowState.Normal;
            if (pos < screen.WorkingArea.Left + screen.WorkingArea.Width)
                msgWindow.Location = new Point(this.Left + this.Width, this.Top);
            else
                msgWindow.Location = new Point(this.Left - msgWindow.Width, this.Top);
        }
    }
}
