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

        protected Tiger tiger = new Tiger();
        protected Hub connection = null;
        protected HubSetting setting = null;

        public HubSetting Setting
        {
            get { return setting; }
            set
            {
                setting = value;
                if (connection == null)
                {
                    connection = new Hub(setting, this);
                    connection.Me.TagInfo.Version = "Xmple V:20080705";
                    connection.Me.TagInfo.Slots = 2;
                    connection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(connection_ProtocolChange);
                    connection.ConnectionStatusChange += new FmdcEventHandler(connection_ConnectionStatusChange);
                    connection.Connect();
                    this.TopLabel.Text = string.Format("{0}:{1}", setting.Address, setting.Port);
                }
                else
                    throw new InvalidOperationException("HubSetting can only be set once");
            }
        }

        void connection_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case TcpConnection.Connecting:
                    TopLabel.ForeColor = Color.Orange;
                    break;
                case TcpConnection.Connected:
                    TopLabel.ForeColor = Color.Green;
                    break;
                case TcpConnection.Disconnected:
                    TopLabel.ForeColor = Color.Red;
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
                prot.Update -= Protocol_Update;

            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(Protocol_Update);
            }
        }

        void Protocol_Update(object sender, FmdcEventArgs e)
        {
            Message msg = null;
            Hub hubConnection = sender as Hub;
            byte[] bytes = null;
            UserInfo usr = null;

            if (hubConnection == null)
                return;
            switch (e.Action)
            {
                case Actions.MainMessage:
                    MainMessage msgMain = e.Data as MainMessage;
                    if (msgMain == null)
                        return;
                    msg = new Message();
                    msg.From = msgMain.From;
                    msg.Content = msgMain.Content;

                    bytes = tiger.ComputeHash( System.Text.Encoding.UTF32.GetBytes(hubConnection.RemoteAddress.ToString()) );
                    msg.GroupId = Base32.Encode(bytes);
                    // TODO : Add 
                    msg.GroupName = this.TopLabel.Text;
                    break;
                case Actions.PrivateMessage:
                    PrivateMessage msgPM = e.Data as PrivateMessage;
                    if (msgPM == null)
                        return;
                    msg = new Message();
                    msg.From = msgPM.From;
                    msg.To = msgPM.To;
                    msg.Content = msgPM.Content;

                    string groupName = (!string.IsNullOrEmpty(msgPM.Group) ? msgPM.Group : msgPM.From);

                    bytes = tiger.ComputeHash( System.Text.Encoding.UTF32.GetBytes(hubConnection.RemoteAddress.ToString() + groupName) );
                    msg.GroupId = Base32.Encode(bytes);
                    msg.GroupName = groupName;
                    break;
                case Actions.UserOnline:
                case Actions.UserOffline:
                    updateQueue.Enqueue(e);
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

            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Interval = 50;
            updateTimer.Start();
        }

        void updateTimer_Tick(object sender, EventArgs eTimer)
        {
            if (isUpdating)
                return;
            isUpdating = true;

            if (updateQueue.Count > 0)
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
                        bytes = tiger.ComputeHash(System.Text.Encoding.UTF32.GetBytes(usr.ID));
                        item.Name = Base32.Encode(bytes);
                        listView1.Items.Add(item);
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

            isUpdating = false;
        }

        protected void HubControl_Update(object sender, FmdcEventArgs e) { }
        protected void HubControl_UpdateBase(object sender, FmdcEventArgs e) { }
        protected void HubControl_Load(object sender, EventArgs e) { }
    }
}
