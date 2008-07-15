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
            toolBtnMessages.Image = new Bitmap(typeof(Program), @"Images.pen.gif");
            // Hub 1
            ClientExample.Controls.HubControl hub = new ClientExample.Controls.HubControl();
            msgWindow.AddHub(hub);
            list1.Items.Add(hub);
            HubSetting setting = new HubSetting();
            setting.DisplayName = "Xmpl1";
            //setting.Address = "mrmikejj.co.uk";
            //setting.Port = 1669;
            //setting.Protocol = "Nmdc";
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.Protocol = "Nmdc";
            hub.Setting = setting;

            System.Threading.Thread.Sleep(1000);

            // Hub 2
            ClientExample.Controls.HubControl hub2 = new ClientExample.Controls.HubControl();
            msgWindow.AddHub(hub2);
            list1.Items.Add(hub2);
            HubSetting setting2 = new HubSetting();
            setting2.DisplayName = "Xmpl2";
            //setting2.Address = "devpublic.adcportal.com";
            //setting2.Port = 16591;
            //setting2.Protocol = "Adc";
            setting2.Address = "127.0.0.1";
            setting2.Port = 412;
            setting2.Protocol = "Nmdc";
            hub2.Setting = setting2;


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
            msgWindow.Focus();
        }
    }
}
