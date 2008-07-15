using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
//using System.Windows.Forms;

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Utils.Convert;
using FlowLib.Utils.Hash;

using ClientExample.Containers;

namespace ClientExample.Controls
{
    public partial class HubControl : ExpandablePanel, IBaseUpdater
    {
        public event FmdcEventHandler HubUpdate;
        public event FmdcEventHandler UpdateBase;

        protected bool isUpdating = false;
        protected System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        protected Queue<FmdcEventArgs> updateQueue = new Queue<FmdcEventArgs>();
        protected bool clearList = false;

        protected Tiger tiger = new Tiger();
        protected Hub connection = null;
        protected HubSetting setting = null;

        public string Id
        {
            get;
            protected set;
        }

        public string Name
        {
            get;
            protected set;
        }

        public HubSetting Setting
        {
            get { return setting; }
            set
            {
                setting = value;
                if (connection == null)
                {
                    Id = string.Format("{0}:{1}", setting.Address, setting.Port);
                    Name = (setting.Name.Length > 0 ? setting.Name : Id);
                    try
                    {
                        connection = new Hub(setting, this);
                        connection.Me.TagInfo.Version = "Xmple V:20080715";
                        connection.Me.TagInfo.Slots = 2;
                        connection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(connection_ProtocolChange);
                        connection.ConnectionStatusChange += new FmdcEventHandler(connection_ConnectionStatusChange);
                        if (setting.ContainsKey("Connect"))
                        {
                            switch (setting.Get("Connect").ToLower())
                            {
                                case "1":   // Auto connect to hub
                                    connection.Connect();
                                    break;
                            }
                        }
                    }
                    catch { /* Invalid addy */ }
                    this.TopLabel.Text = Name;
                }
                else
                    throw new InvalidOperationException("HubSetting can only be set once");
            }
        }

        void connection_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            HubUpdate(this, new FmdcEventArgs(1000, new OnlineStatus(Id, null, e.Action)));
            switch (e.Action)
            {
                case TcpConnection.Connecting:
                    TopLabel.ForeColor = Color.Orange;
                    break;
                case TcpConnection.Connected:
                    TopLabel.ForeColor = Color.Green;
                    break;
                case TcpConnection.Disconnected:
                    TopLabel.ForeColor = Color.Gray;
                    clearList = true;
                    break;
                default:
                    break;
            }
        }

        void connection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.Update -= Protocol_Update;
                //prot.MessageReceived -= Protocol_MessageReceived;
                //prot.MessageToSend -= Protocol_MessageToSend;
            }

            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(Protocol_Update);
                //hubConnection.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
                //hubConnection.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
            }
        }

        //void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        //{
        //    StrMessage raw = e.Data as StrMessage;
        //    if (raw != null)
        //    {
        //        Message msg = new Message();
        //        msg.From = "OUT";
        //        msg.Content = raw.Raw;
        //        msg.GroupId = Id;
        //        msg.GroupName = this.TopLabel.Text;
        //        HubUpdate(this, new FmdcEventArgs(Actions.MainMessage, msg));
        //    }
        //}

        //void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        //{
        //    StrMessage raw = e.Data as StrMessage;
        //    if (raw != null)
        //    {
        //        Message msg = new Message();
        //        msg.From = "IN";
        //        msg.Content = raw.Raw;
        //        msg.GroupId = Id;
        //        msg.GroupName = this.TopLabel.Text;
        //        HubUpdate(this, new FmdcEventArgs(Actions.MainMessage, msg));
        //    }
        //}

        void Protocol_Update(object sender, FmdcEventArgs e)
        {
            Message msg = null;
            Hub hubConnection = sender as Hub;
            byte[] bytes = null;
            User usr = null;

            if (hubConnection == null)
                return;
            switch (e.Action)
            {
                case Actions.MainMessage:
                    MainMessage msgMain = e.Data as MainMessage;
                    if (msgMain == null)
                        return;
                    msg = new Message();
                    if (!string.IsNullOrEmpty(msgMain.From))
                    {
                        usr = hubConnection.GetUserById(msgMain.From);
                        if (usr != null)
                            msg.From = usr.DisplayName;
                        else
                            msg.From = msgMain.From;
                    }
                    msg.Content = msgMain.Content;

                    msg.GroupId = Id;
                    // TODO : Add 
                    msg.GroupName = this.TopLabel.Text;
                    break;
                case Actions.PrivateMessage:
                    PrivateMessage msgPM = e.Data as PrivateMessage;
                    if (msgPM == null)
                        return;
                    msg = new Message();
                    usr = hubConnection.GetUserById(msgPM.From);
                    msg.From = usr.DisplayName;
                    msg.To = msgPM.To;
                    if (msgPM.Content.StartsWith("<" + msgPM.From + "> "))
                        msg.Content = msgPM.Content.Substring(msgPM.From.Length + 3);
                    else
                        msg.Content = msgPM.Content;

                    if (!string.IsNullOrEmpty(msgPM.Group))
                        usr = hubConnection.GetUserById(msgPM.Group);
                    string groupName = usr.DisplayName;

                    msg.GroupId = Id + groupName;
                    msg.GroupName = groupName;
                    break;
                case Actions.UserOnline:
                case Actions.UserOffline:
                    updateQueue.Enqueue(e);
                    UserInfo infoOnline = e.Data as UserInfo;
                    if (infoOnline != null)
                    {
                        int status = (e.Action == Actions.UserOnline ? TcpConnection.Connected : TcpConnection.Disconnected);
                        HubUpdate(this, new FmdcEventArgs(1000, new OnlineStatus(Id, infoOnline.ID, status)));
                    }
                    break;
            }

            if (msg != null)
            {
                    HubUpdate(this, new FmdcEventArgs(e.Action, msg));
            }
        }

        public HubControl()
        {
            UpdateBase = new FmdcEventHandler(HubControl_UpdateBase);
            HubUpdate = new FmdcEventHandler(HubControl_Update);
            InitializeComponent();

            label1.Width = 180;

            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Interval = 50;
            updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs eTimer)
        {
            if (isUpdating)
                return;
            isUpdating = true;

            if (clearList)
            {
                listView1.Items.Clear();
                clearList = false;
            }

            if (updateQueue.Count > 0)
            {
                try
                {
                    listView1.SuspendLayout();
                    FmdcEventArgs e = updateQueue.Dequeue();
                    UserInfo usr = null;
                    byte[] bytes = null;
                    switch (e.Action)
                    {
                        case Actions.UserOnline:

                            //case Actions.UserInfoChange:
                            usr = e.Data as UserInfo;
                            if (usr == null)
                                return;
                            System.Windows.Forms.ListViewItem item = new System.Windows.Forms.ListViewItem(usr.DisplayName);
                            item.Name = usr.ID;
                            listView1.Items.Add(item);

                            //Rectangle rec = listView1.GetItemRect(0, System.Windows.Forms.ItemBoundsPortion.Entire);
                            //this.Height = rec.Height * listView1.Items.Count;
                            //this.Invalidate();
                            break;
                        case Actions.UserOffline:
                            usr = e.Data as UserInfo;
                            if (usr == null)
                                return;
                            listView1.Items.RemoveByKey(usr.ID);
                            break;
                    }
                    listView1.ResumeLayout();
                }
                catch { }
            }

            isUpdating = false;
        }

        protected void HubControl_Update(object sender, FmdcEventArgs e) { }
        protected void HubControl_UpdateBase(object sender, FmdcEventArgs e) { }
        protected void HubControl_Load(object sender, EventArgs e) { }
    }
}
