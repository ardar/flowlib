
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using FlowLib.Interfaces;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Managers;
// For user Handling
using System.Collections.Generic;
// For Connection
using System.Net.Sockets;
using System.Text;
// For Connection Keepalive
using System.Threading;
namespace FlowLib.Connections
{
    /// <summary>
    /// Class representing hub in the p2p network Direct Connect
    /// </summary>
    public class Hub : TcpConnection
    {
        #region Events
        /// <summary>
        /// This is so we will have right hubcount in all hubs all the time (For internal use most)
        /// </summary>
        public static event FmdcEventHandler RegModeUpdated;
        /// <summary>
        /// If HubSettings contains a unknown protocol Id. This will be trigged
        /// </summary>
        public event FmdcEventHandler UnknownProtocolId;
        #endregion
        #region Variables
        static protected int regmode_normal = 0;
        static protected int regmode_regged = 0;
        static protected int regmode_operator = 0;

        protected long interval = 120;
        protected User me = null;
        protected SortedList<string, User> userlist = new SortedList<string, User>();
        // For Connection Keepalive
        protected long keepAliveTicks = System.DateTime.Now.Ticks;
        protected Timer keepAliveTimer;
        #region Connection
        protected int regMode = -1;
        #endregion
        protected HubSetting fav = null;
        protected Share share = null;
        #endregion
        #region Properties
        /// <summary>
        /// Interval in seconds between commands sent to ensure connection.
        /// </summary>
        public long KeepAliveInterval
        {
            get { return interval; }
            set {
                interval = (value == 0) ? Timeout.Infinite : (interval * 1000);
                keepAliveTimer.Change(interval, interval);
            }
        }

        public new IProtocolHub Protocol
        {
            get { return (IProtocolHub)protocol; }
            set 
            {
                base.Protocol = value;
                protocol = (IProtocol)value;

            }
        }

        /// <summary>
        /// Share related to this hub
        /// </summary>
        public override Share Share
        {
            get { return share; }
            set
            {
                if (share != null)
                    share.LastModifiedChanged -=share_LastModifiedChanged;
                share = value;
                if (share != null)
                {
                    if (fav != null)
                        fav.ShareId = share.Name;
                    share.LastModifiedChanged += new FmdcEventHandler(share_LastModifiedChanged);
                    UpdateShare();
                }
            }
        }
        /// <summary>
        /// Settings used by this hub for connection.
        /// </summary>
        public HubSetting HubSetting
        {
            get { return fav; }
        }
        /// <summary>
        /// Reg mode in this hub.
        /// -1 = Login Procedure is not done,
        /// 0 = Normal,
        /// 1 = Regged, 
        /// 2 = OP
        /// </summary>
        public int RegMode
        {
            get { return regMode; }
            set
            {
                // remove old count for this hub
                switch (regMode)
                {
                    case 0:
                        regmode_normal--;
                        break;
                    case 1:
                        regmode_regged--;
                        break;
                    case 2:
                        regmode_operator--;
                        break;
                }
                // add new count
                regMode = value;
                switch (regMode)
                {
                    case 0:
                        regmode_normal++;
                        break;
                    case 1:
                        regmode_regged++;
                        break;
                    case 2:
                        regmode_operator++;
                        break;
                }
                RegModeUpdated(this, new FmdcEventArgs(regMode));
            }
        }
        /// <summary>
        /// Represents our user in hub.
        /// </summary>
        public UserInfo Me
        {
            get { return me.UserInfo; }
            set
            {
                me.UserInfo = value;
                UpdateRegMode();
            }
        }
        /// <summary>
        /// Online users in hub
        /// </summary>
        public SortedList<string, User> Userlist
        {
            get { return userlist; }
            set { userlist = value; }
        }
        #endregion

        #region Functions
        protected void UpdateRegMode()
        {
            // Updates user if event havnt been handled already
            me.UserInfo.TagInfo.Normal = regmode_normal;
            me.UserInfo.TagInfo.Regged = regmode_regged;
            me.UserInfo.TagInfo.OP = regmode_operator;
        }

        public Hub(HubSetting infav)
            : this(infav, null) { }

        public Hub(HubSetting infav, IBaseUpdater gui)
            : base(infav.Address, infav.Port)
        {
            fav = infav;    // Sets Favorites.
            me = new User(fav.DisplayName);
            me.UserInfo.Description = fav.UserDescription;
            me.Tag.Version = "FlowLib";

            UpdateRegMode();
            UpdateShare();
            #region Event(s)
            UnknownProtocolId = new FmdcEventHandler(OnUnknownProtocolId);
            RegModeUpdated += new FmdcEventHandler(Hub_RegModeUpdated);
            ProtocolChange += new FmdcEventHandler(Hub_ProtocolChange);
            #endregion
            #region Event(s)
            if (gui != null)
                gui.UpdateBase += new FmdcEventHandler(OnUpdateBase);
            #endregion
            //FireUpdate(Actions.Name, new HubName(HubSetting.Address + ":" + HubSetting.Port.ToString()));
            #region Keep Alive
            // For Connection Keepalive
            TimerCallback timerDelegate = new TimerCallback(OnkeepAliveTimer_Elapsed);
            keepAliveTimer = new System.Threading.Timer(timerDelegate, socket, interval * 1000, interval * 1000);
            #endregion
        }

        void Hub_ProtocolChange(object sender, FmdcEventArgs e)
        {
            if (Protocol != null)
                this.Protocol.Update += new FmdcEventHandler(OnProtocolUpdate);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Hub()
        {
            if (keepAliveTimer != null)
                keepAliveTimer.Dispose();
        }



        protected void UpdateShare()
        {
            if (share != null)
                me.UserInfo.Share = share.HashedSize.ToString();
        }

        /// <summary>
        /// Returns user with the specified id from userlist.
        /// In ADC this means SID.
        /// In NMDC this means Nickname.
        /// </summary>
        /// <param name="id">Id for the user we want to get</param>
        /// <returns>User that have the specified id or null if not found</returns>
        public User GetUserById(string id)
        {
            User usr = new User(id);
            if (id != null && userlist.TryGetValue(id, out usr))
            {
                return usr;
            }
            return null;
        }
        /// <summary>
        /// Returns first user in userlist with the specified nick.
        /// </summary>
        /// <param name="nick">Nickname for the user we want to get</param>
        /// <returns>User that have the specified nick or null if not found</returns>
        public User GetUserByNick(string nick)
        {
            // TODO : Make this more effective.
            foreach (KeyValuePair<string, User> kpair in userlist)
            {
                if (nick.Equals(kpair.Value.DisplayName))
                {
                    return kpair.Value;
                }
            }
            return null;
        }

        #region Connection
        /// <summary>
        /// Tells listener object that hub is will reconnect and then reconnects.
        /// </summary>
        public override void Reconnect()
        {
            base.Reconnect();
        }

        protected override void OnSendData(System.IAsyncResult async)
        {
            base.OnSendData(async);
            // KeepAlive : Update ticks so we are not sending stuff to hub when we dont need to..
            keepAliveTicks = System.DateTime.Now.Ticks;
        }

        /// <summary>
        /// Tries to connect to hub.
        /// </summary>
        public override void Connect()
        {
            // RegMode changed to default
            RegMode = -1;
            if (Protocol == null)
            {
                // Setting right Command Seperator.
                switch (HubSetting.Protocol)
                {
                    case "Nmdc":     // NMDC
                        Protocol = new HubNmdcProtocol(this);
                        break;
                    case "":        // Auto Detect
                    case "Auto":    // Auto Detect
                    case "Adc":     // ADC
                        Protocol = new AdcProtocol(this);
                        break;
                    default:
                        FmdcEventArgs e = new FmdcEventArgs(0);
                        UnknownProtocolId(this, e);
                        if (e.Handled && e.Data is IProtocolHub)
                        {
                            Protocol = (IProtocolHub)e.Data;
                        }
                        else
                        {
                            throw new System.InvalidOperationException("Protocol is not beeing set. Protocol name is not a inbuilt protocol and has not been handled by plugin");
                        }
                        break;
                }
            }
            Thread t = new Thread(new ThreadStart(base.Connect));
            t.IsBackground = true;
            t.Start();
        }

        public override void Disconnect(string msg)
        {
            RegMode = -1;
            base.Disconnect(msg);
        }

        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If no data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        protected override void OnRecievedData(System.IAsyncResult ar)
        {
            base.OnRecievedData(ar);
            // KeepAlive : Update ticks so we are not sending stuff to hub when we dont need to..
            keepAliveTicks = System.DateTime.Now.Ticks;

            //if (socket.Connected && RegMode < 0)    // Change Hub mode to 0
            //    Update(this, new FmdcEventArgs(Actions.RegMode, 0));
        }
        #endregion
        #endregion
        #region OnEvent(s)
        private void Hub_RegModeUpdated(object sender, FmdcEventArgs e)
        {
            Hub hub = sender as Hub;
            if (!e.Handled)
                UpdateRegMode();
        }

        private void share_LastModifiedChanged(object sender, FmdcEventArgs e) 
        {
            UpdateShare();
        }

        protected void OnkeepAliveTimer_Elapsed(System.Object stateInfo)
        {
            long second = 10000000;
            // We are checking against socket != null. This is if we havnt connect.
            if (socket != null && System.DateTime.Now.Ticks > (this.keepAliveTicks + (interval * second)))
            {
                this.keepAliveTicks = System.DateTime.Now.Ticks;

                if (this.RegMode >= 0)
                { // Check if we are still connected.
                    if (Protocol.KeepAliveCommand != null)
                    {
                        IConMessage msg = Protocol.KeepAliveCommand;
                        msg.Connection = this; // This is so plugins know what hub it was triggered in.
                        Send(msg);
                    }
                }
                if (!socket.Connected)
                {
                    Reconnect();
                }
            }
        }

        protected void OnUpdateBase(object sender, FmdcEventArgs e)
        {
            // TODO : We are looking Gui as it is now. Please change this.
            Protocol.ActOnOutMessage(e);
        }

        protected void OnUnknownProtocolId(object sender, FmdcEventArgs e)
        {

        }

        protected void OnProtocolUpdate(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.StatusChange:
                    switch (((HubStatus)e.Data).Code)
                    {
                        case HubStatus.Codes.Disconnected:
                            if (socket != null && socket.Connected)
                            {
                                try
                                {
                                    socket.Shutdown(SocketShutdown.Both);
                                    //socket.Disconnect(true);
                                    socket.Close();
                                }
                                catch (System.Net.Sockets.SocketException) { }
                                catch (System.ObjectDisposedException) { }
                            }
                            break;
                    }
                    break;
                case Actions.RegMode:
                    RegMode = (int)e.Data;
                    break;
                case Actions.UserOnline:
                    UserInfo ui = (UserInfo)e.Data;
                    User u = new User(ui.DisplayName);
                    u.UserInfo = ui;
                    userlist.Add(ui.ID, u);
                    break;
                case Actions.UserOffline:
                    UserInfo userInfo = (UserInfo)e.Data;
                    if (userInfo == null)
                        return;
                    userlist.Remove(userInfo.ID);
                    break;
            }
        }
        #endregion
    }
}
