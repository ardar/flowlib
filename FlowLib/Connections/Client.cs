
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, flowlib at flowertwig dot org
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

using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Flowertwig.Utils;
using Flowertwig.Utils.Connections;
using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Enums;
using Flowertwig.Utils.Events;
using FlowLib.Connections.Interfaces;
using FlowLib.Connections.Protocols;
using FlowLib.Connections.Protocols.Nmdc;
using FlowLib.Entities;
using FlowLib.Interfaces;

// For user Handling
// For Connection
// For Connection Keepalive

namespace FlowLib.Connections
{
    /// <summary>
    /// Class representing hub in the p2p network Direct Connect
    /// </summary>
    public class Client : TcpConnection, IClient
    {
        #region Events
        /// <summary>
        /// This is so we will have right hubcount in all hubs all the time (For internal use most)
        /// </summary>
        public static event EventHandler RegModeUpdated;
        /// <summary>
        /// If HubSettings contains a unknown protocol Id. This will be trigged
        /// </summary>
        public event EventHandler UnknownProtocolId;
        #endregion
        #region Variables
        static protected int regmode_normal = 0;
        static protected int regmode_regged = 0;
        static protected int regmode_operator = 0;

        protected long interval = 120;
        protected User me = null;
        protected SortedList<string, User> userlist = new SortedList<string, User>();
        protected IBaseUpdater baseUpdater = null;
        // For Connection Keepalive
        protected long keepAliveTicks = System.DateTime.Now.Ticks;
        protected Timer keepAliveTimer;
        protected Timer updateInfoTimer;
        protected Thread worker = null;
        #region Connection
        protected int regMode = -1;
        #endregion
        protected HubSetting fav = null;
        protected IShare share = null;
        #endregion
        #region Properties
        /// <summary>
        /// Interval in seconds between commands sent to ensure connection.
        /// </summary>
        public long KeepAliveInterval
        {
            get { return interval; }
            set {
                interval = value;
                long tmp = ((interval == 0) ? Timeout.Infinite : (interval * 1000));
                keepAliveTimer.Change(tmp, tmp);
            }
        }

        public new IProtocolHub Protocol
        {
            get { return (IProtocolHub)protocol; }
            set 
            {
                base.Protocol = value;
                this.HubSetting.Protocol = value.Name;
                protocol = (IProtocol)value;

            }
        }

        /// <summary>
        /// Share related to this hub
        /// </summary>
        public IShare Share
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
                    share.LastModifiedChanged += new EventHandler(share_LastModifiedChanged);
                    UpdateShare();
                }
            }
        }
        public string StoreId
        {
            get
            {
                if (HubSetting == null)
                    return null;
                return HubSetting.Address + HubSetting.Port.ToString();
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
                RegModeUpdated(this, new DefaultEventArgs(regMode));
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
            get 
            {
                lock (userlist)
                {
                    return userlist;
                }
            }
            set 
            {
                lock (userlist)
                {
                    userlist = value;
                }
            }
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

        public Client(HubSetting infav)
            : this(infav, null) { }

        public Client(HubSetting infav, IBaseUpdater gui)
            : base(infav.Address, infav.Port)
        {
            fav = infav;    // Sets Favorites.
            me = new User(fav.DisplayName);
            me.UserInfo.Description = fav.UserDescription;
            me.Tag.Version = FlowLibInfo.GetRunningVersionString();

            UpdateRegMode();
            UpdateShare();
            #region Event(s)
            UnknownProtocolId = new EventHandler(OnUnknownProtocolId);
            RegModeUpdated += new EventHandler(Hub_RegModeUpdated);
            ProtocolChange += new EventHandler(Hub_ProtocolChange);
            #endregion
            #region Event(s)
            if (gui != null)
            {
                gui.UpdateBase += new EventHandler(OnUpdateBase);
                baseUpdater = gui;
            }
            #endregion
            //FireUpdate(Actions.Name, new HubName(HubSetting.Address + ":" + HubSetting.Port.ToString()));
            #region Keep Alive
            // For Connection Keepalive
            TimerCallback timerDelegate = new TimerCallback(OnkeepAliveTimer_Elapsed);
            keepAliveTimer = new System.Threading.Timer(timerDelegate, socket, interval * 1000, interval * 1000);
            #endregion
        }

        void Hub_ProtocolChange(object sender, DefaultEventArgs e)
        {
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
                prot.Update -= OnProtocolUpdate;
            if (Protocol != null)
                this.Protocol.Update += new EventHandler(OnProtocolUpdate);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~Client()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (!disposed)
            {
                if (baseUpdater != null)
                    baseUpdater.UpdateBase -= OnUpdateBase;
                base.Dispose();
                Client.RegModeUpdated -= Hub_RegModeUpdated;
                UnknownProtocolId -= OnUnknownProtocolId;
                ProtocolChange -= Hub_ProtocolChange;
                if (updateInfoTimer != null)
                {
                    updateInfoTimer.Dispose();
                    updateInfoTimer = null;
                }
                if (keepAliveTimer != null)
                {
                    keepAliveTimer.Dispose();
                    keepAliveTimer = null;
                }
                if (worker != null)
                {
                    worker.Abort();
                    worker = null;
                }
                lock (userlist)
                {
                    if (userlist != null)
                    {
                        userlist.Clear();
                        userlist = null;
                    }
                }
                this.me = null;
                if (share != null)
                {
                    share.LastModifiedChanged -= share_LastModifiedChanged;
                    this.share = null;
                }
                this.fav = null;
            }
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
            lock (userlist)
            {
                if (id != null && userlist.TryGetValue(id, out usr))
                {
                    return usr;
                }
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
            lock (userlist)
            {
                foreach (KeyValuePair<string, User> kpair in userlist)
                {
                    if (nick.Equals(kpair.Value.DisplayName))
                    {
                        return kpair.Value;
                    }
                }
            }
            return null;
        }

		/// <summary>
		/// Returns first user in userlist with the specified storedId.
		/// </summary>
		/// <param name="storedId">StoredId for the user we want to get</param>
		/// <returns>User that have the specified storedId or null if not found</returns>
		public User GetUserByStoredId(string storedId)
		{
			// TODO : Make this more effective.
			lock (userlist)
			{
				foreach (KeyValuePair<string, User> kpair in userlist)
				{
					if (storedId.Equals(kpair.Value.StoreID))
					{
						return kpair.Value;
					}
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
#if !COMPACT_FRAMEWORK
// Security
                    case "Adcs":
                    case "AdcSecure":
                        Protocol = new AdcProtocol(this, true);
                        SecureProtocol = SecurityProtocols.TLS;
                        break;
                    case "NmdcSecure":
                        Protocol = new HubNmdcProtocol(this, true);
                        SecureProtocol = SecurityProtocols.TLS;
                        break;
#endif
                    default:
                        DefaultEventArgs e = new DefaultEventArgs(0);
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
            worker = new Thread(new ThreadStart(base.Connect));
            worker.Name = "HubWorker";
            worker.IsBackground = true;
            worker.Start();
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
            //    Update(this, new DefaultEventArgs(Actions.RegMode, 0));
        }
        #endregion
        #endregion
        #region OnEvent(s)
        private void Hub_RegModeUpdated(object sender, DefaultEventArgs e)
        {
            Client client = sender as Client;
            if (!e.Handled)
                UpdateRegMode();
        }

        private void share_LastModifiedChanged(object sender, DefaultEventArgs e) 
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

        protected void OnUpdateBase(object sender, DefaultEventArgs e)
        {
            // TODO : We are looking Gui as it is now. Please change this.
            Protocol.ActOnOutMessage(e);
        }

        protected void OnUnknownProtocolId(object sender, DefaultEventArgs e)
        {

        }

        protected void OnProtocolUpdate(object sender, DefaultEventArgs e)
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
                    lock (userlist)
                    {
                        userlist.Add(ui.ID, u);
                    }
                    break;
                case Actions.UserOffline:
                    UserInfo userInfo = (UserInfo)e.Data;
                    if (userInfo == null)
                        return;
                    lock (userlist)
                    {
                        userlist.Remove(userInfo.ID);
                    }
                    break;
            }
        }
        #endregion
    }
}
