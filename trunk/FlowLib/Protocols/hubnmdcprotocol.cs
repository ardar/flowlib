
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using System.Text;
using System.Threading;

using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Protocols.HubNmdc;
using FlowLib.Connections;
using FlowLib.Managers;
using FlowLib.Enums;

namespace FlowLib.Protocols
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
        public event FmdcEventHandler Update;
        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
        #endregion
        #region Variables
        protected Hub hub = null;
        protected string recieved = "";
        protected long myInfoLastUpdated = 0;
        protected MyINFO lastMyInfo = null;
        protected Supports hubSupport = null;
        protected Timer updateMyInfoTimer;
        protected bool disposed = false;
        protected Encoding currentEncoding = null;
        #endregion
        #region Properties
        public string Name
        {
            get { return "Nmdc"; }
        }
        public IConMessage KeepAliveCommand
        {
            get { return new HubMessage(hub, Seperator); }
        }

        public IConMessage FirstCommand
        {
            get { return null; }
        }

        public System.Text.Encoding Encoding
        {
            //get { return System.Text.Encoding.Default; }
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
        public HubNmdcProtocol(Hub hub)
        {
            this.hub = hub;
            hub.ConnectionStatusChange += new FmdcEventHandler(hub_ConnectionStatusChange);
            if (hub.Share != null)
                hub.Share.LastModifiedChanged += new FmdcEventHandler(Share_LastModifiedChanged);
            Hub.RegModeUpdated += new FmdcEventHandler(Hub_RegModeUpdated);

            TimerCallback updateCallback = new TimerCallback(OnUpdateMyInfo);
            updateMyInfoTimer = new Timer(updateCallback, this, Timeout.Infinite, Timeout.Infinite);

            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
        }

        public void Dispose()
        {
            if (!disposed)
            {
                hub.ConnectionStatusChange -= hub_ConnectionStatusChange;
                if (hub.Share != null)
                    hub.Share.LastModifiedChanged -= Share_LastModifiedChanged;
                Hub.RegModeUpdated -= Hub_RegModeUpdated;

                hub = null;
                updateMyInfoTimer.Dispose();
                updateMyInfoTimer = null;

                disposed = true;
            }
        }

        void Share_LastModifiedChanged(object sender, FmdcEventArgs e)
        {
            if (!e.Handled)
            {
                UpdateMyInfo();
            }
        }

        void Hub_RegModeUpdated(object sender, FmdcEventArgs e)
        {
            if (sender != hub && !e.Handled && hub.RegMode >= 0)
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
                MyINFO tmp = new MyINFO(hub);
                if (lastMyInfo == null || (tmp.Raw != lastMyInfo.Raw))
                {
                    lastMyInfo = tmp;
                    hub.Send(lastMyInfo);
                    myInfoLastUpdated = System.DateTime.Now.Ticks;
                    updateMyInfoTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            else if (firstTime)
            {
                updateMyInfoTimer.Change(0, 15 * 60 * 1000 + 10);
            }
        }

        void hub_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            HubStatus h;
            switch (e.Action)
            {
                case TcpConnection.Disconnected:
                    if (e.Data is System.Exception)
                        h = new HubStatus(HubStatus.Codes.Disconnected, (System.Exception)e.Data);
                    else
                        h = new HubStatus(HubStatus.Codes.Disconnected);
                    hub.RegMode = -1;
					// Sets MyInfo Interval to 0 when connection is disconnected
					this.myInfoLastUpdated = 0;
					lastMyInfo = null;
					break;
                case TcpConnection.Connected:
                    h = new HubStatus(HubStatus.Codes.Connected);
                    break;
                case TcpConnection.Connecting:
                default:
                    h = new HubStatus(HubStatus.Codes.Connecting);
                    break;
            }
            Update(hub, new FmdcEventArgs(Actions.StatusChange, h));
            hub.Userlist.Clear();
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
                FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(hub, e);
                if (!e.Handled && msg.IsValid)
                    ActOnInMessage(msg);
                pos++;
            }
            // If wrong Protocol type has been set. change it to ADC
            if (hub.RegMode == -1 && raw.StartsWith("ISUP")) 
            {   // Setting hubtype to ADC
                Hub tmpHub = hub;
                hub.Protocol = new AdcProtocol(hub);
                tmpHub.Reconnect();
            }
            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                recieved = raw;
        }

        protected HubMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            HubMessage msg = new HubMessage(hub, raw);
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
                                msg = new Lock(hub, raw); break;
                            case "$supports":
                                msg = new Supports(hub, raw); break;
                            case "$hubname":
                                msg = new HubNmdc.HubName(hub, raw); break;
                            case "$hello":
                                msg = new Hello(hub, raw); break;
                            case "$myinfo":
                                msg = new MyINFO(hub, raw); break;
                            case "$nicklist":
                                msg = new NickList(hub, raw); break;
                            case "$oplist":
                                msg = new OpList(hub, raw); break;
                            case "$to:":
                                msg = new To(hub, raw); break;
                            case "$quit":
                                msg = new Quit(hub, raw); break;
                            case "$getpass":
                                msg = new GetPass(hub, raw); break;
                            case "$logedin":
                                msg = new LogedIn(hub, raw); break;
                            case "$validatedenide":
                                msg = new ValidateDenide(hub, raw); break;
                            case "$forcemove":
                                msg = new ForceMove(hub, raw); break;
                            case "$connecttome":
                                msg = new ConnectToMe(hub, raw); break;
                            case "$revconnecttome":
                                msg = new RevConnectToMe(hub, raw); break;
                            case "$search":
                                msg = new Search(hub, raw); break;
                            case "$sr":
                                msg = new SR(hub, raw); break;
                        }
                        break;
                    default:
                        // No command. Assume MainChat.
                        msg = new MainChat(hub, raw);
                        break;
                }
            }
            return msg;
        }
        #endregion
        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(hub, e);
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
                Update(hub, new FmdcEventArgs(Actions.MainMessage, msg));
            }
            else if (message is To)
            {
                To to = (To)message;
                PrivateMessage pm = new PrivateMessage(to.To, to.From, to.Content);
                Update(hub, new FmdcEventArgs(Actions.PrivateMessage, pm));
            }
            else if (message is SR)
            {
                SR searchResult = (SR)message;
                SearchResultInfo srinfo = new SearchResultInfo(searchResult.Info, searchResult.From);
                Update(hub, new FmdcEventArgs(Actions.SearchResult, srinfo));
            }
            else if (message is Search)
            {
                Search search = (Search)message;
                if (hub.Share == null)
                    return;
                int maxReturns = 5;
                bool active = false;
                if (search.Address != null)
                {
                    maxReturns = 10;
                    active = true;
                }
                System.Collections.Generic.List<ContentInfo> ret = new System.Collections.Generic.List<ContentInfo>(maxReturns);
                // TODO : This lookup can be done nicer
                lock (hub.Share)
                {

                    foreach (System.Collections.Generic.KeyValuePair<string, Containers.ContentInfo> var in hub.Share)
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
                                        if (hub.Share.ContainsContent(ref contentInfo))
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
                        size = int.Parse(search.Info.Get(SearchInfo.SIZE));
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
                        SR sr = new SR(hub, ret[i], (search.Info.ContainsKey(SearchInfo.EXTENTION) ? search.Info.Get(SearchInfo.EXTENTION).Equals("$0") : false), search.From);
                        if (active)
                        {
                            // Send with UDP
                            UdpConnection.Send(sr, search.Address);
                        }
                        else
                        {
                            // Send through hub
                            hub.Send(sr);
                        }
                    }
                }
            }
            else if (message is Lock)
            {
                hub.Send(new Supports(hub));
                hub.Send(new Key(hub, ((Lock)message).Key));
                hub.Send(new ValidateNick(hub));
            }
            else if (message is HubNmdc.HubName)
            {
                HubNmdc.HubName hubname = (HubNmdc.HubName)message;
                Containers.HubName name = null;
                if (hubname.Topic != null)
                    name = new Containers.HubName(hubname.Name, hubname.Topic);
                else
                    name = new Containers.HubName(hubname.Content);
                Update(hub, new FmdcEventArgs(Actions.Name, name));
            }
            else if (message is NickList)
            {
                NickList nicks = (NickList)message;
                foreach (string userid in nicks.List)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.DisplayName = userid;
                    userInfo.Set(UserInfo.STOREID, hub.HubSetting.Address + hub.HubSetting.Port.ToString() + userid);
                    if (hub.GetUserById(userid) == null)
                        Update(hub, new FmdcEventArgs(Actions.UserOnline, userInfo));
                }
            }
            else if (message is OpList)
            {
                OpList ops = (OpList)message;
                foreach (string userid in ops.List)
                {
                    UserInfo userInfo = new UserInfo();
                    userInfo.DisplayName = userid;
                    userInfo.Set(UserInfo.STOREID, hub.HubSetting.Address + hub.HubSetting.Port.ToString() + userid);
                    userInfo.IsOperator = true;
                    User usr = null;
                    if ((usr = hub.GetUserById(userid)) == null)
                        Update(hub, new FmdcEventArgs(Actions.UserOnline, userInfo));
                    else
                    {
                        usr.UserInfo = userInfo;
                        Update(hub, new FmdcEventArgs(Actions.UserInfoChange, usr.UserInfo));
                    }
                }
            }
            else if (message is Quit)
            {
                Quit quit = (Quit)message;
                User usr = null;
                if ((usr = hub.GetUserById(quit.From)) != null)
                    Update(hub, new FmdcEventArgs(Actions.UserOffline, usr.UserInfo));
            }
            else if (message is LogedIn)
                hub.RegMode = 2;
            else if (message is ValidateDenide)
                Update(hub, new FmdcEventArgs(Actions.StatusChange, new HubStatus(HubStatus.Codes.Disconnected)));
            else if (message is GetPass)
            {
                hub.RegMode = 1;
                if (hub.HubSetting.Password.Length == 0)
                    Update(hub, new FmdcEventArgs(Actions.Password, null));
                else
                    hub.Send(new MyPass(hub));
            }
            else if (message is MyINFO)
            {
                MyINFO myinfo = (MyINFO)message;
                User usr = null;
                if ((usr = hub.GetUserById(message.From)) == null)
                    Update(hub, new FmdcEventArgs(Actions.UserOnline, myinfo.UserInfo));
                else
                {
                    bool op = usr.IsOperator;
                    usr.UserInfo = myinfo.UserInfo;
                    usr.UserInfo.IsOperator = op;
                    Update(hub, new FmdcEventArgs(Actions.UserInfoChange, usr.UserInfo));
                }
            }
            else if (message is Hello)
            {
                if (hub.HubSetting.DisplayName.Equals(message.From))
                {
                    hub.Send(new Version(hub));
                    hub.Send(new GetNickList(hub));
                    if (hub.RegMode < 0)
                        hub.RegMode = 0;
                    UpdateMyInfo();
                }
            }
            else if (message is ConnectToMe)
            {
                ConnectToMe conToMe = (ConnectToMe)message;
                Transfer trans = new Transfer(conToMe.Address, conToMe.Port);
                trans.Share = this.hub.Share;
                trans.Me = hub.Me;
                trans.Source = new Source(hub.RemoteAddress.ToString(), null);
                // Protocol has to be set last.
                trans.Protocol = new TransferNmdcProtocol(trans);
#if !COMPACT_FRAMEWORK
                if (conToMe.TLS && hub.Me.ContainsKey(UserInfo.SECURE))
                    trans.SecureProtocol = SecureProtocols.TLS;
#endif
                Update(hub, new FmdcEventArgs(Actions.TransferStarted, trans));
            }
            else if (message is RevConnectToMe)
            {
                RevConnectToMe revConToMe = (RevConnectToMe)message;
                User usr = null;
                usr = hub.GetUserById(revConToMe.From);
                if (hub.Me.Mode == FlowLib.Enums.ConnectionTypes.Passive)
                {
                    if (usr != null)
                    {
                        // If user are not set as passive. Set it as it and respond with a revconnect.
                        if (usr.UserInfo.Mode != FlowLib.Enums.ConnectionTypes.Passive)
                        {
                            usr.UserInfo.Mode = FlowLib.Enums.ConnectionTypes.Passive;
                            hub.Send(new RevConnectToMe(revConToMe.From, hub));
                        }
                    }
                }
                else
                {
                    if (usr != null)
                    {
                        Update(hub, new FmdcEventArgs(Actions.TransferRequest, new TransferRequest(usr.ID, hub, usr.UserInfo)));
#if !COMPACT_FRAMEWORK
                        // Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        if (
                            usr.UserInfo.ContainsKey(UserInfo.SECURE) &&
                            hub.Me.ContainsKey(UserInfo.SECURE) &&
                            !string.IsNullOrEmpty(hub.Me.Get(UserInfo.SECURE))
                            )
                            hub.Send(new ConnectToMe(usr.ID, hub.Share.Port, hub, SecureProtocols.TLS));
                        else
#endif
                            hub.Send(new ConnectToMe(usr.ID, hub.Share.Port, hub));
                    }
                }
            }
            else if (message is ForceMove)
            {
                ForceMove forceMove = (ForceMove)message;
                hub.Disconnect();
                Update(hub, new FmdcEventArgs(Actions.Redirect, new RedirectInfo(forceMove.Address)));
            }
        }

        public void ActOnOutMessage(FmdcEventArgs e)
        {
            if (e.Action.Equals(Actions.MainMessage))
            {
                Containers.MainMessage main = (Containers.MainMessage)e.Data;
                hub.Send(new MainChat(hub, hub.Me.ID, main.Content));
            }
            else if (e.Action.Equals(Actions.PrivateMessage))
            {
                Containers.PrivateMessage pm = (Containers.PrivateMessage)e.Data;
                To to = null;
                hub.Send(to = new To(hub, pm.To, pm.Content));
                this.ParseRaw(to.Raw);
            }
            else if (e.Action.Equals(Actions.Password))
            {
                hub.Send(new MyPass(hub, e.Data.ToString()));
            }
            else if (e.Action.Equals(Actions.Search))
            {
                hub.Send(new Search(hub, (SearchInfo)e.Data));
            }
            else if (e.Action.Equals(Actions.StartTransfer))
            {
                User usr = e.Data as User;
                if (usr == null)
                    return;
                switch (hub.Me.Mode)
                {
                    case ConnectionTypes.Direct:
                    case ConnectionTypes.UPnP:
                    case ConnectionTypes.Forward:
                        Update(hub, new FmdcEventArgs(Actions.TransferRequest, new TransferRequest(usr.ID, hub, usr.UserInfo)));
#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        if (
                            usr.UserInfo.ContainsKey(UserInfo.SECURE) && 
                            hub.Me.ContainsKey(UserInfo.SECURE) &&
                            !string.IsNullOrEmpty( hub.Me.Get(UserInfo.SECURE) )
                            )
                            hub.Send(new ConnectToMe(usr.ID, hub.Share.Port, hub, SecureProtocols.TLS));
                        else
#endif
                            hub.Send(new ConnectToMe(usr.ID, hub.Share.Port, hub));
                        break;
                    case ConnectionTypes.Passive:
                    case ConnectionTypes.Socket5:
                    case ConnectionTypes.Unknown:
                    default:
                        if (usr.UserInfo.Mode == ConnectionTypes.Passive)
                        {
                            break;
                        }
                        hub.Send(new RevConnectToMe(usr.ID, hub));
                        break;
                }
            }
        }
        #endregion
        #region Event(s)
        protected void OnMessageReceived(object sender, FmdcEventArgs e)
        {

        }
        protected void OnMessageToSend(object sender, FmdcEventArgs e)
        {

        }
        #endregion
    }
}
