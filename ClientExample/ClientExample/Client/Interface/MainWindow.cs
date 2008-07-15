using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FlowLib.Containers;
using System.Threading;

namespace ClientExample.Client.Interface
{
    public partial class MainWindow : Form
    {
        protected MessageWindow msgWindow = new MessageWindow();
        protected delegate void AddingListItem();

        protected List<HubSetting> savedHubs = null;
        protected bool isUpdating = false;
        protected System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public MainWindow()
        {
            InitializeComponent();

            if (Program.Settings.SavedHubs != null)
                savedHubs = new List<HubSetting>(Program.Settings.SavedHubs);
            else
                savedHubs = new List<HubSetting>();
            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 200;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (isUpdating)
                return;
            isUpdating = true;
            if (savedHubs.Count > 0)
            {
                lock (savedHubs)
                {
                    list1.SuspendLayout();
                    HubSetting item = savedHubs[0];
                    savedHubs.RemoveAt(0);

                    ClientExample.Controls.HubControl hub = new ClientExample.Controls.HubControl();
                    msgWindow.AddHub(hub);
                    list1.Items.Add(hub);
                    bool showUserlist = hub.Expanded;       // True
                    if (item.ContainsKey("UserListState") && item.Get("UserListState").Equals("1"))
                    {

                    }
                    else
                        hub.Expanded = !hub.Expanded;
                    hub.Setting = item;
                    list1.ResumeLayout();
                }
            }
            else
            {
                timer.Dispose();
            }
            isUpdating = false;
        }

        protected void OnLoad()
        {
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            toolBtnMessages.Image = new Bitmap(typeof(Program), @"Images.pen.gif");
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
