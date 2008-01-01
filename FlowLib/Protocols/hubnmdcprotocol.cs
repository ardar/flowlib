
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
        #region Variables
        protected Hub hub = null;
        protected string recieved = "";

        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
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
            get { return System.Text.Encoding.Default; }
        }
        public string Seperator
        {
            get { return "|"; }
        }
        #endregion
        #region Constructor(s)
        public HubNmdcProtocol(Hub hub)
        {
            this.hub = hub;
            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
        }
        #endregion

        public bool OnStartTransfer(User usr)
        {
            switch (hub.Me.Mode)
            {
                case ConnectionTypes.Direct:
                case ConnectionTypes.UPnP:
                case ConnectionTypes.Forward:
                    hub.FireUpdate(Actions.TransferRequest, new TransferRequest(usr.ID, hub, usr.UserInfo));
                    hub.Send(new ConnectToMe(usr.ID, hub.Share.Port, hub));
                    return true;
                case ConnectionTypes.Passive:
                case ConnectionTypes.Socket5:
                case ConnectionTypes.Unknown:
                default:
                    if (usr.UserInfo.Mode == ConnectionTypes.Passive)
                    {
                        return false;
                    }
                    hub.Send(new RevConnectToMe(usr.ID, hub));
                    return true;
            }
        }

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
                hub.Protocol = new HubAdcProtocol(hub);
                hub.HubSetting.Protocol = hub.Protocol.Name;
                hub.Reconnect();
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
                hub.FireUpdate(Actions.MainMessage, msg);
            }
            else if (message is To)
            {
                To to = (To)message;
                PrivateMessage pm = new PrivateMessage(to.To, to.From, to.Content);
                hub.FireUpdate(Actions.PrivateMessage, pm);
            }
            else if (message is SR)
            {
                SR searchResult = (SR)message;
                SearchResultInfo srinfo = new SearchResultInfo(searchResult.Info, searchResult.From);
                hub.FireUpdate(Actions.SearchResult, srinfo);
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
                            switch (search.Info.Get(SearchInfo.EXTENTION))
                            {
                                case "$0":
                                    if (var.Value.ContainsKey(ContentInfo.VIRTUAL) && (System.IO.Path.GetDirectoryName(var.Value.Get(ContentInfo.VIRTUAL)).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1))
                                        ret.Add(var.Value);
                                    break;
                                case "$1":
                                    contentInfo.Set(ContentInfo.TTH, search.Info.Get(SearchInfo.SEARCH));
                                    if (hub.Share.ContainsContent(ref contentInfo))
                                    {
                                        ret.Add(contentInfo);
                                    }
                                    // We are looking through whole share here.
                                    // If no TTH matching. Ignore.
                                    foundEnough = true;
                                    break;
                                default:
                                    string infoExt = System.IO.Path.GetExtension(var.Value.Get(ContentInfo.VIRTUAL)).TrimStart('.');
                                    if (
                                            var.Value.ContainsKey(ContentInfo.VIRTUAL)
                                            && (var.Value.Get(ContentInfo.VIRTUAL).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1)
                                            && (ext.Length == 0 || ext.Contains(infoExt))
                                        )
                                        ret.Add(var.Value);
                                    break;
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
                    if (search.Info.ContainsKey(SearchInfo.SIZETYPE) && long.TryParse(search.Info.Get(SearchInfo.SIZE), out size))
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
                hub.FireUpdate(Actions.Name, name);
            }
            else if (message is NickList)
            {
                NickList nicks = (NickList)message;
                hub.FireUpdate(Actions.UsersOnline, nicks.List);
            }
            else if (message is OpList)
            {
                OpList ops = (OpList)message;
                hub.FireUpdate(Actions.OpUsers, ops.List);
            }
            else if (message is Quit)
            {
                Quit quit = (Quit)message;
                hub.FireUpdate(Actions.UserOffline, quit.From);
            }
            else if (message is LogedIn)
                hub.RegMode = 2;
            else if (message is ValidateDenide)
                hub.FireUpdate(Actions.StatusChange, new HubStatus(HubStatus.Codes.Disconnected));
            else if (message is GetPass)
            {
                hub.RegMode = 1;
                if (hub.HubSetting.Password.Length == 0)
                    hub.FireUpdate(Actions.Password, message);
                else
                    hub.Send(new MyPass(hub));
            }
            else if (message is MyINFO)
            {
                MyINFO myinfo = (MyINFO)message;
                if (hub.GetUserById(message.From) == null)
                    hub.FireUpdate(Actions.UserOnline, myinfo.UserInfo);
                else
                    hub.FireUpdate(Actions.UserInfoChange, myinfo.UserInfo);
            }
            else if (message is Hello)
            {
                if (hub.HubSetting.DisplayName.Equals(message.From))
                {
                    hub.Send(new Version(hub));
                    hub.Send(new GetNickList(hub));
                    hub.Send(new MyINFO(hub));
                }
            }
            else if (message is ConnectToMe)
            {
                ConnectToMe conToMe = (ConnectToMe)message;
                Transfer trans = new Transfer(conToMe.Address, conToMe.Port);
                trans.Share = this.hub.Share;
                trans.Me = hub.Me;
                // Protocol has to be set last.
                trans.Protocol = new TransferNmdcProtocol(trans);

                hub.FireUpdate(Actions.TransferStarted, trans);

                //TransferManager.StartTransfer(trans);
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
                        hub.FireUpdate(Actions.TransferRequest, new TransferRequest(revConToMe.From, hub, usr.UserInfo));

                        //TransferManager.AddTransferReq(revConToMe.From, hub, usr.UserInfo);
                        hub.Send(new ConnectToMe(revConToMe.From, hub.Share.Port, hub));
                    }
                }
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
                hub.Send(new To(hub, pm.To, pm.Content));
            }
            else if (e.Action.Equals(Actions.Password))
            {
                hub.Send(new MyPass(hub, (string)e.Data));
            }
            else if (e.Action.Equals(Actions.Search))
            {
                hub.Send(new Search(hub, (SearchInfo)e.Data));
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
