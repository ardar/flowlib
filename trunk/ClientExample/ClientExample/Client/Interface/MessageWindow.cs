using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;

using ClientExample.Controls;
using ClientExample.Containers;
using FlowLib.Events;

namespace ClientExample.Client.Interface
{
    public partial class MessageWindow : System.Windows.Forms.Form
    {
        protected Queue<Message> messages = new Queue<Message>();
        protected System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        protected bool isUpdating = false;

        public bool Close
        {
            get;
            set;
        }

        public MessageWindow()
        {
            Close = false;
            InitializeComponent();

            timer.Tick += new EventHandler(timer_Tick);
            timer.Interval = 100;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (isUpdating)
                return;
            isUpdating = true;

            if (messages.Count > 0)
            {
                MessagePanel panel = null;
                //System.Windows.Forms.TabPage tab = null;
                Message msg = messages.Dequeue();
                lock (tabControl1)
                {
                    if (tabControl1.TabPages.ContainsKey(msg.GroupId))
                    {
                        System.Windows.Forms.TabPage tab = tabControl1.TabPages[msg.GroupId];
                        panel = tab.Tag as MessagePanel;
                    }
                    else
                    {
                        System.Windows.Forms.TabPage tab = new System.Windows.Forms.TabPage();
                        tab.Name = msg.GroupId;
                        tab.Text = msg.GroupName;
                        tabControl1.TabPages.Add(tab);
                        tab.Controls.Add(panel = new MessagePanel());
                        panel.Dock = System.Windows.Forms.DockStyle.Fill;
                        tab.Tag = panel;
                    }
                }

                string str = string.Format("[{0}] <{1}> {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg.From, msg.Content);
                panel.AddMessage(str);
            }

            isUpdating = false;
        }

        public void AddHub(HubControl hub)
        {
            hub.HubUpdate += new FlowLib.Events.FmdcEventHandler(hub_HubUpdate);
        }

        void hub_HubUpdate(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.MainMessage:
                case Actions.PrivateMessage:
                    Message msg = e.Data as Message;
                    if (msg == null)
                        return;
                    messages.Enqueue(msg);
                    break;
            }
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
