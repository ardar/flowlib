
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using System.Text;
using System.Threading;
using Flowertwig.Utils.Connections;
using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Entities;
using Flowertwig.Utils.Enums;
using Flowertwig.Utils.Events;
using FlowLib.Connections.Entities;
using FlowLib.Connections.Interfaces;
using FlowLib.Connections.Protocols.Nmdc.Commands;
using FlowLib.Entities;
using HubName = FlowLib.Connections.Protocols.Nmdc.Commands.HubName;

namespace FlowLib.Connections.Protocols.Nmdc
{
    /// <summary>
    /// Hub NMDC Protocol
    /// </summary>
    public class HubNmdcProtocol : IProtocolHub
    {
        #region Events
        /// <summary>
        /// Events have happen in this hub and we want to tell others.
        /// Mainmessages, private messages and more.
        /// </summary>
        public event EventHandler Update;
        public event EventHandler MessageReceived;
        public event EventHandler MessageToSend;
        #endregion
        #region Variables
        protected Client client = null;
        protected string recieved = "";
        protected long myInfoLastUpdated = 0;
        protected MyINFO lastMyInfo = null;
        protected Supports hubSupport = null;
        protected Timer updateMyInfoTimer;
        protected bool disposed = false;
        protected Encoding currentEncoding = null;
        #endregion
        #region Properties
        protected string _name = "Nmdc";
        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }
        protected bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                Update(client, new DefaultEventArgs(BaseTransferProtocol.IsReady, value));
            }
        }

        public IConMessage KeepAliveCommand
        {
            get { return new HubMessage(client, Seperator); }
        }

        public IConMessage FirstCommand
        {
            get { return null; }
        }

        public System.Text.Encoding Encoding
        {
            get
			{
                if (currentEncoding == null)
                {
				    try {
					    currentEncoding = System.Text.Encoding.GetEncoding(1252);
				    } catch (System.Exception e) {
                        throw new System.NotSupportedException("Exception throwed when trying to retrieve Encoding page: 1252.", e);
				    }
                }
                return currentEncoding;
            }
            set
            {
                currentEncoding = value;
            }
        }
        public string Seperator
        {
            get { return "|"; }
        }
        public bool IsDisposed
        {
            get { return disposed; }
        }
        #endregion
        #region Constructor(s)
        public HubNmdcProtocol(Client client)
        {
            this.client = client;
            client.ConnectionStatusChange += new EventHandler(hub_ConnectionStatusChange);
            if (client.Share != null)
                client.Share.LastModifiedChanged += new EventHandler(Share_LastModifiedChanged);
            Client.RegModeUpdated += new EventHandler(Hub_RegModeUpdated);

            TimerCallback updateCallback = new TimerCallback(OnUpdateMyInfo);
            updateMyInfoTimer = new Timer(updateCallback, this, Timeout.Infinite, Timeout.Infinite);

            MessageReceived = new EventHandler(OnMessageReceived);
            MessageToSend = new EventHandler(OnMessageToSend);
        }

        public HubNmdcProtocol(Client client, bool isSecure)
            : this(client)
        {
            // We are doing this to keep a good HubSettings
            if (isSecure)
                Name = Name + "Secure";
        }

        public void Dispose()
        {
            if (!disposed)
            {
                client.ConnectionStatusChange -= hub_ConnectionStatusChange;
                if (client.Share != null)
                    client.Share.LastModifiedChanged -= Share_LastModifiedChanged;
                Client.RegModeUpdated -= Hub_RegModeUpdated;

                client = null;
                updateMyInfoTimer.Dispose();
                updateMyInfoTimer = null;

                disposed = true;
            }
        }

        void Share_LastModifiedChanged(object sender, DefaultEventArgs e)
        {
            if (!e.Handled)
            {
                UpdateMyInfo();
            }
        }

        void Hub_RegModeUpdated(object sender, DefaultEventArgs e)
        {
            if (sender != client && !e.Handled && client.RegMode >= 0)
            {
                UpdateMyInfo();
            }
        }

        protected void OnUpdateMyInfo(System.Object stateInfo)
        {
            UpdateMyInfo(false);
        }

        protected void UpdateMyInfo() { UpdateMyInfo(true); }
        protected virtual void UpdateMyInfo(bool firstTime)
        {
            if (new System.DateTime(myInfoLastUpdated).AddMinutes(15) < System.DateTime.Now)
            {
                MyINFO tmp = new MyINFO(client);
                if (lastMyInfo == null || (tmp.Raw != lastMyInfo.Raw))
                {
                    lastMyInfo = tmp;
                    client.Send(lastMyInfo);
                    myInfoLastUpdated = System.DateTime.Now.Ticks;
                    updateMyInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            else if (firstTime)
            {
                updateMyInfoTimer.Change(0, 15 * 60 * 1000 + 10);
            }
        }

        void hub_ConnectionStatusChange(object sender, DefaultEventArgs e)
        {
            HubStatus h;
            switch (e.Action)
            {
                case TcpConnection.Disconnected:
                    if (e.Data is System.Exception)
                        h = new HubStatus(HubStatus.Codes.Disconnected, (System.Exception)e.Data);
                    else
                        h = new HubStatus(HubStatus.Codes.Disconnected);
                    client.RegMode = -1;
					// Sets MyInfo Interval to 0 when connection is disconnected
					this.myInfoLastUpdated = 0;
					lastMyInfo = null;
                    IsReady = false;
					break;
                case TcpConnection.Connected:
                    h = new HubStatus(HubStatus.Codes.Connected);
                    break;
                case TcpConnection.Connecting:
                default:
                    h = new HubStatus(HubStatus.Codes.Connecting);
                    break;
            }
            Update(client, new DefaultEventArgs(Actions.StatusChange, h));
            client.Userlist.Clear();
        }
        #endregion

        #region Parse
        public void ParseRaw(byte[] b, int length)
        {
            ParseRaw(this.Encoding.GetString(b, 0, length));
        }

        public void ParseRaw(string raw)
        {
            // If raw lenght is 0. Ignore
            if (raw.Length == 0)
                return;

            // Should we read buffer?
            if (recieved.Length > 0)
            {
                raw = recieved + raw;
                recieved = string.Empty;
            }
            int pos;
            // Loop through Commands.
            while ((pos = raw.IndexOf(Seperator)) != -1)
            {
                pos++;
                HubMessage msg = ParseMessage(raw.Substring(0, pos));
                raw = raw.Remove(0, pos);
                // Plugin handling here
                DefaultEventArgs e = new DefaultEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(client, e);
                if (!e.Handled && msg.IsValid)
                    ActOnInMessage(msg);
                pos++;
            }
            // If wrong Protocol type has been set. change it to ADC
            if (client.RegMode == -1 && raw.StartsWith("ISUP")) 
            {   // Setting hubtype to ADC
                Client tmpClient = client;
                client.Protocol = new AdcProtocol(client);
                tmpClient.Reconnect();
            }
            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                recieved = raw;
        }

        protected HubMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            HubMessage msg = new HubMessage(client, raw);
            if (!string.IsNullOrEmpty(raw))
            {
                switch (raw[0])
                {
                    case '$':
                        int pos;
                        string cmd = null;
                        if ((pos = raw.IndexOf(' ')) != -1)
                            cmd = raw.Substring(0, pos).ToLower();
                        else
                        {
                            if (raw.Length >= 10)
                                break;
                            cmd = raw.ToLower();
                        }
                        if (cmd == null || cmd.Equals(string.Empty))
                            break;
                        switch (cmd)
                        {
                            case "$lock":
                                msg = new Lock(client, raw); break;
                            case "$supports":
                                msg = new Supports(client, raw); break;
                            case "$hubname":
                                msg = new HubName(client, raw); break;
                            case "$hello":
                                msg = new Hello(client, raw); break;
                            case "$myinfo":
                                msg = new MyINFO(client, raw); break;
                            case "$nicklist":
                                msg = new NickList(client, raw); break;
                            case "$oplist":
                                msg = new OpList(client, raw); break;
                            case "$to:":
                                msg = new To(client, raw); break;
                            case "$quit":
                                msg = new Quit(client, raw); break;
                            case "$getpass":
                                msg = new GetPass(client, raw); break;
                            case "$logedin":
                                msg = new LogedIn(client, raw); break;
                            case "$validatedenide":
                                msg = new ValidateDenide(client, raw); break;
                            case "$forcemove":
                                msg = new ForceMove(client, raw); break;
                            case "$connecttome":
                                msg = new ConnectToMe(client, raw); break;
                            case "$revconnecttome":
                                msg = new RevConnectToMe(client, raw); break;
                            case "$search":
                                msg = new Search(client, raw); break;
                            case "$sr":
                                msg = new SR(client, raw); break;
                        }
                        break;
                    default:
                        // No command. Assume MainChat.
                        msg = new MainChat(client, raw);
                        break;
                }
            }
            return msg;
        }
        #endregion
        public bool OnSend(IConMessage msg)
        {
            DefaultEventArgs e = new DefaultEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(client, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        #region Act On
        public void ActOnInMessage(IConMessage comMsg)
        {
            HubMessage message = (HubMessage)comMsg;
            if (message is MainChat)
            {
                MainChat main = (MainChat)message;
                MainMessage msg = new MainMessage(main.From, main.Content);
                Update(client, new DefaultEventArgs(Actions.MainMessage, msg));
            }
            else if (message is To)
            {
                To to = (To)message;
                PrivateMessage pm = new PrivateMessage(to.To, to.From, to.Content);
                Update(client, new DefaultEventArgs(Actions.PrivateMessage, pm));
            }
            else if (message is SR)
            {
                SR searchResult = (SR)message;
                SearchResultInfo srinfo = new SearchResultInfo(searchResult.Info, searchResult.From);
                Update(client, new DefaultEventArgs(Actions.SearchResult, srinfo));
            }
            else if (message is Search)
            {
                Search search = (Search)message;
                if (client.Share == null)
                    return;
                int maxReturns = 5;
                bool active = false;
                if (search.Address != null)
                {
                    maxReturns = 10;
                    active = true;
                }
                var ret = new List<ContentInfo>(maxReturns);
                // TODO : This lookup can be done nicer
                lock (client.Share)
                {

                    foreach (KeyValuePair<string, ContentInfo> var in client.Share)
                    {
                        if (var.Value == null)
                            continue;
                        bool foundEnough = false;
                        string ext = search.Info.Get(SearchInfo.EXTENTION);
                        string sch = search.Info.Get(SearchInfo.SEARCH);
                        if (ext != null && sch != null)
                        {
                            ContentInfo contentInfo = new ContentInfo();
                            if (search.Info.ContainsKey(SearchInfo.TYPE))
                            {
                                switch (search.Info.Get(SearchInfo.TYPE))
                                {
                                    case "2":

                                        contentInfo.Set(ContentInfo.TTH, search.Info.Get(SearchInfo.SEARCH));
                                        if (client.Share.ContainsContent(ref contentInfo))
                                        {
                                            ret.Add(contentInfo);
                                        }
                                        // We are looking through whole share here.
                                        // If no TTH matching. Ignore.
                                        foundEnough = true;
                                        break;
                                    case "1":
                                    default:
                                        if (var.Value.ContainsKey(ContentInfo.VIRTUAL) && (System.IO.Path.GetDirectoryName(var.Value.Get(ContentInfo.VIRTUAL)).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1))
                                            ret.Add(var.Value);
                                        break;
                                }
                            }

                            if (!foundEnough)
                            {
                                string infoExt = System.IO.Path.GetExtension(var.Value.Get(ContentInfo.VIRTUAL)).TrimStart('.');
                                if (
                                        var.Value.ContainsKey(ContentInfo.VIRTUAL)
                                        && (var.Value.Get(ContentInfo.VIRTUAL).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1)
                                        && (ext.Length == 0 || ext.Contains(infoExt))
                                    )
                                    ret.Add(var.Value);
                            }
                        }
                        if (foundEnough || ret.Count >= maxReturns)
                            break;
                    }
                }
                // Test against size restrictions
                for (int i = 0; i < ret.Count; i++)
                {
                    bool send = true;
                    long size = -1;
                    try
                    {
                        size = long.Parse(search.Info.Get(SearchInfo.SIZE));
                    }
                    catch { }
                    if (search.Info.ContainsKey(SearchInfo.SIZETYPE) && size != -1)
                    {
                        switch (search.Info.Get(SearchInfo.SIZETYPE))
                        {
                            case "1":       // Min Size
                                send = (size <= ret[i].Size);
                                break;
                            case "2":       // Max Size
                                send = (size >= ret[i].Size);
                                break;
                            case "3":       // Equal Size
                                send = (size == ret[i].Size);
                                break;
                        }
                    }
                    // Should this be sent?
                    if (send)
                    {
                        SR sr = new SR(client, ret[i], (search.Info.ContainsKey(SearchInfo.EXTENTION) ? search.Info.Get(SearchInfo.EXTENTION).Equals("$0") : false), search.From);
                        if (active)
                        {
                            // Send with UDP
                            UdpConnection.Send(sr, search.Address);
                        }
                        else
                        {
                            // Send through hub
                            client.Send(sr);
                        }
                    }
                }
            }
            else if (message is Lock)
            {
                client.Send(new Supports(client));
                client.Send(new Key(client, ((Lock)message).Key));
                client.Send(new ValidateNick(client));
            }
            else if (message is HubName)
            {
                HubName hubname = (HubName)message;
                FlowLib.Entities.HubName name = null;
                if (hubname.Topic != null)
                    name = new FlowLib.Entities.HubName(hubname.Name, hubname.Topic);
                else
                    name = new FlowLib.Entities.HubName(hubname.Content);
                Update(client, new DefaultEventArgs(Actions.Name, name));
            }
            else if (message is NickList)
            {
                var nicks = (NickList)message;
                foreach (string userid in nicks.List)
                {
                    var userInfo = new UserInfo();
                    userInfo.DisplayName = userid;
                    userInfo.Set(UserInfo.STOREID, client.StoreId + userid);
                    if (client.GetUserById(userid) == null)
                        Update(client, new DefaultEventArgs(Actions.UserOnline, userInfo));
                }
            }
            else if (message is OpList)
            {
                OpList ops = (OpList)message;
                foreach (string userid in ops.List)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.DisplayName = userid;
                    userInfo.Set(UserInfo.STOREID, client.StoreId + userid);
                    userInfo.IsOperator = true;
                    User usr = null;
                    if ((usr = client.GetUserById(userid)) == null)
                        Update(client, new DefaultEventArgs(Actions.UserOnline, userInfo));
                    else
                    {
                        usr.UserInfo = userInfo;
                        Update(client, new DefaultEventArgs(Actions.UserInfoChange, usr.UserInfo));
                    }
                }
            }
            else if (message is Quit)
            {
                Quit quit = (Quit)message;
                User usr = null;
                if ((usr = client.GetUserById(quit.From)) != null)
                    Update(client, new DefaultEventArgs(Actions.UserOffline, usr.UserInfo));
            }
            else if (message is LogedIn)
                client.RegMode = 2;
            else if (message is ValidateDenide)
                Update(client, new DefaultEventArgs(Actions.StatusChange, new HubStatus(HubStatus.Codes.Disconnected)));
            else if (message is GetPass)
            {
                client.RegMode = 1;
                if (client.HubSetting.Password.Length == 0)
                    Update(client, new DefaultEventArgs(Actions.Password, null));
                else
                    client.Send(new MyPass(client));
            }
            else if (message is MyINFO)
            {
                MyINFO myinfo = (MyINFO)message;
                User usr = null;
                if ((usr = client.GetUserById(message.From)) == null)
                    Update(client, new DefaultEventArgs(Actions.UserOnline, myinfo.UserInfo));
                else
                {
                    bool op = usr.IsOperator;
                    usr.UserInfo = myinfo.UserInfo;
                    usr.UserInfo.IsOperator = op;
                    Update(client, new DefaultEventArgs(Actions.UserInfoChange, usr.UserInfo));
                }

                if (client.RegMode >= 0 && string.Equals(myinfo.From, client.Me.ID))
                {
                    IsReady = true;
                }
            }
            else if (message is Hello)
            {
                if (client.HubSetting.DisplayName.Equals(message.From))
                {
                    client.Send(new Version(client));
                    client.Send(new GetNickList(client));
                    if (client.RegMode < 0)
                        client.RegMode = 0;
                    UpdateMyInfo();
                }
            }
            else if (message is ConnectToMe)
            {
                ConnectToMe conToMe = (ConnectToMe)message;
                Transfer trans = new Transfer(conToMe.Address, conToMe.Port);
                trans.Share = this.client.Share;
                trans.Me = client.Me;
                trans.Source = new Source(client.StoreId, null);
                // Protocol has to be set last.
                trans.Protocol = new TransferNmdcProtocol(trans);
#if !COMPACT_FRAMEWORK
                if (conToMe.TLS && client.Me.ContainsKey(UserInfo.SECURE))
                    trans.SecureProtocol = SecurityProtocols.TLS;
#endif
                Update(client, new DefaultEventArgs(Actions.TransferStarted, trans));
            }
            else if (message is RevConnectToMe)
            {
                RevConnectToMe revConToMe = (RevConnectToMe)message;
                User usr = null;
                usr = client.GetUserById(revConToMe.From);
                if (client.Me.Mode == ConnectionTypes.Passive)
                {
                    if (usr != null)
                    {
                        // If user are not set as passive. Set it as it and respond with a revconnect.
                        if (usr.UserInfo.Mode != ConnectionTypes.Passive)
                        {
                            usr.UserInfo.Mode = ConnectionTypes.Passive;
                            client.Send(new RevConnectToMe(revConToMe.From, client));
                        }
                    }
                }
                else
                {
                    if (usr != null)
                    {
                        Update(client, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(usr.ID, client, usr.UserInfo)));
#if !COMPACT_FRAMEWORK
                        // Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        if (
                            usr.UserInfo.ContainsKey(UserInfo.SECURE) &&
                            client.Me.ContainsKey(UserInfo.SECURE) &&
                            !string.IsNullOrEmpty(client.Me.Get(UserInfo.SECURE))
                            )
                            client.Send(new ConnectToMe(usr.ID, client.Share.Port, client, SecurityProtocols.TLS));
                        else
#endif
                            client.Send(new ConnectToMe(usr.ID, client.Share.Port, client));
                    }
                }
            }
            else if (message is ForceMove)
            {
                ForceMove forceMove = (ForceMove)message;
                client.Disconnect();
                Update(client, new DefaultEventArgs(Actions.Redirect, new RedirectInfo(forceMove.Address)));
            }
        }

        public void ActOnOutMessage(DefaultEventArgs e)
        {
            if (e.Action.Equals(Actions.MainMessage))
            {
                MainMessage main = (MainMessage)e.Data;
                client.Send(new MainChat(client, client.Me.ID, main.Content));
            }
            else if (e.Action.Equals(Actions.PrivateMessage))
            {
                PrivateMessage pm = (PrivateMessage)e.Data;
                To to = null;
                client.Send(to = new To(client, pm.To, pm.Content));
                this.ParseRaw(to.Raw);
            }
            else if (e.Action.Equals(Actions.Password))
            {
                client.Send(new MyPass(client, e.Data.ToString()));
            }
            else if (e.Action.Equals(Actions.Search))
            {
                client.Send(new Search(client, (SearchInfo)e.Data));
            }
            else if (e.Action.Equals(Actions.StartTransfer))
            {
                UserInfo usrInfo;
                User usr = e.Data as User;
                if (usr == null)
                {
                    usrInfo = e.Data as UserInfo;
                    if (usrInfo == null)
                    {
                        return;
                    }
                }
                else
                {
                    usrInfo = usr.UserInfo;
                }

                switch (client.Me.Mode)
                {
                    case ConnectionTypes.Direct:
                    case ConnectionTypes.UPnP:
                    case ConnectionTypes.Forward:
                        Update(client, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(usrInfo.ID, client, usrInfo)));
#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        if (
                            usrInfo.ContainsKey(UserInfo.SECURE) && 
                            client.Me.ContainsKey(UserInfo.SECURE) &&
                            !string.IsNullOrEmpty( client.Me.Get(UserInfo.SECURE) )
                            )
                            client.Send(new ConnectToMe(usrInfo.ID, client.Share.Port, client, SecurityProtocols.TLS));
                        else
#endif
                            client.Send(new ConnectToMe(usrInfo.ID, client.Share.Port, client));
                        break;
                    case ConnectionTypes.Passive:
                    case ConnectionTypes.Socket5:
                    case ConnectionTypes.Unknown:
                    default:
                        if (usrInfo.Mode == ConnectionTypes.Passive)
                        {
                            break;
                        }
                        client.Send(new RevConnectToMe(usrInfo.ID, client));
                        break;
                }
            }
        }
        #endregion
        #region Event(s)
        protected void OnMessageReceived(object sender, DefaultEventArgs e)
        {

        }
        protected void OnMessageToSend(object sender, DefaultEventArgs e)
        {

        }
        #endregion
    }
}
