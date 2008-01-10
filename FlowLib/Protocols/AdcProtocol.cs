
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

using System.Text;

using FlowLib.Interfaces;
using FlowLib.Protocols.Adc;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Enums;

namespace FlowLib.Protocols
{
    /// <summary>
    /// ADC Protocol
    /// </summary>
    public class AdcProtocol : IProtocolHub //, IProtocolTransfer
    {
        #region Variables
        // Variables to remember
        protected string gpaString = "";        // GPA Random data
        protected SUP supports = null;          // Current support
        protected User info = new User("");     // Hub/User Info (Name and description and so on).
        protected IConnection con = null;       // Current Connection where this protocol is used
        protected Hub hub = null;               // Current hub where this protocol is used
        protected Transfer trans = null;        // Current transfer where this protocol is used
        protected string recieved = "";


        protected static string yoursupports = "ADBASE ADTIGR";

        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
        #endregion
        #region Properties
        public string Name
        {
            get { return "Adc"; }
        }
        public Containers.UserInfo Info
        {
            get { return info.UserInfo; }
            set { info.UserInfo = value; }
        }
        public static string Support
        {
            get { return yoursupports; }
        }
        public IConMessage KeepAliveCommand
        {
            get { return new StrMessage(con, Seperator); }
        }
        public IConMessage FirstCommand
        {
            get
            {
                return new SUP(con);
            }
        }
        public System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
        public string Seperator
        {
            get { return "\n"; }
        }
        #endregion
        #region Constructor(s)
        public AdcProtocol()
        {
            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
        }
        protected AdcProtocol(IConnection con)
            :this()
        {
            this.con = con;
        }

        public AdcProtocol(Hub hub)
            : this((IConnection)hub)
        {
            this.hub = hub;
        }
        public AdcProtocol(Transfer trans)
            : this((ITransfer)trans)
        {
            this.trans = trans;
        }
        #endregion
        #region Functions
        #region Convert
        /// <summary>
        /// Replaces "\\s" with " " and so on
        /// </summary>
        /// <param name="content">String to convert to right format</param>
        /// <returns>coverted string</returns>
        public static string ConvertIncomming(string content)
        {
            content = content.Replace("\\\\", "\\");
            content = content.Replace("\\s", " ");
            content = content.Replace("\\n", "\n");
            return content;
        }
        /// <summary>
        /// Replaces " " with "\\s" and so on
        /// </summary>
        /// <param name="content">String to convert to right format</param>
        /// <returns>coverted string</returns>
        public static string ConvertOutgoing(string content)
        {
            content = content.Replace("\\", "\\\\");
            content = content.Replace(" ", "\\s");
            content = content.Replace("\n", "\\n");
            return content;
        }
        #endregion
        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(con, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        #region Parse
        public void ParseRaw(byte[] b, int length)
        {
            ParseRaw(Encoding.GetString(b, 0, length));
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
            while ((pos = raw.IndexOf(Seperator)) > 0)
            {
                pos++;
                StrMessage msg = ParseMessage(raw.Substring(0, pos));
                raw = raw.Remove(0, pos);
                // Plugin handling here
                FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(con, e);
                if (!e.Handled)
                    ActOnInMessage(msg);
            }
            // If wrong Protocol type has been set. change it to Nmdc
            if (hub != null && hub.RegMode == -1 && raw.Length > 5 && raw.StartsWith("$"))
            {   // Setting hubtype to NMDC
                hub.Protocol = new HubNmdcProtocol(hub);
                hub.HubSetting.Protocol = hub.Protocol.Name;
                hub.Reconnect();
            }
            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                recieved = raw;
        }
        protected StrMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            AdcBaseMessage msg = new AdcBaseMessage(con, raw);
            switch (msg.Action)
            {
                case "SUP":
                    msg = new SUP(con, raw); break;
                case "SID":
                    msg = new SID(con, raw); break;
                case "MSG":
                    msg = new MSG(con, raw); break;
                case "INF":
                    msg = new INF(con, raw); break;
                case "STA":
                    msg = new STA(con, raw); break;
                case "QUI":
                    msg = new QUI(con, raw); break;
                case "GPA":
                    msg = new GPA(con, raw); break;
                case "CTM":
                    msg = new CTM(con, raw); break;
                case "SND":
                    msg = new SND(con, raw); break;
                case "GFI":
                    msg = new GFI(con, raw); break;
                case "GET":
                    msg = new GET(con, raw); break;
                case "RCM":
                    msg = new RCM(con, raw); break;
                case "SCH":
                    msg = new SCH(con, raw); break;
                case "RES":
                    msg = new RES(con, raw); break;
                case "PAS":
                    msg = new PAS(con, raw); break;
            }
            return msg;
        }
        #endregion
        #region Act On
        public void ActOnInMessage(IConMessage conMsg)
        {
            StrMessage message = (StrMessage)conMsg;
            if (message is INF)
            {
                INF inf = (INF)message;
                if (hub != null && inf.Type.Equals("I"))
                {
                    //if (inf.Hub.RegMode == -1)    // TODO : We shouldnt have RegMode == 0 here. Fix it.
                    con.Send(new INF(con, hub.Me));
                    Info = inf.UserInfo;
                    if (hub != null && Info.Description == null)
                        hub.FireUpdate(Actions.Name, new Containers.HubName(Info.DisplayName));
                    else if (hub != null)
                        hub.FireUpdate(Actions.Name, new Containers.HubName(Info.DisplayName, Info.Description));
                }
                else if (trans != null && inf.Type.Equals("C"))
                {
                    //User usr = null;
                    //if (trans.User 
                }
                else if (hub != null)
                {
                    User usr = null;
                    if ((usr = hub.GetUserById(inf.Id)) == null)
                        hub.FireUpdate(Actions.UserOnline, inf.UserInfo);
                    else
                    {
                        usr.UserInfo = inf.UserInfo;
                        hub.FireUpdate(Actions.UserInfoChange, usr.UserInfo);
                    }
                    if (hub.RegMode < 0 && hub.Me.ID == inf.Id)
                        hub.FireUpdate(Actions.RegMode, 0);
                }
            }
            else if (message is MSG && hub != null)
            {
                MSG msg = (MSG)message;
                if (msg.To == null)
                {
                    MainMessage main = new MainMessage(msg.From, msg.Content);
                    hub.FireUpdate(Actions.MainMessage, main);
                }
                else
                {
                    PrivateMessage pm = new PrivateMessage(msg.To, msg.From, msg.Content, msg.PmGroup);
                    hub.FireUpdate(Actions.PrivateMessage, pm);
                }
            }
            else if (message is SID && hub != null)
            {
                SID sid = (SID)message;
                hub.Me.Set(UserInfo.SID, sid.Id);
            }
            else if (message is STA)
            {
                STA sta = (STA)message;
                if (hub != null)
                {
                    MainMessage main = new MainMessage(info.ID, sta.Content);
                    hub.FireUpdate(Actions.MainMessage, main);
                }
            }
            else if (message is GPA && hub != null)
            {
                GPA gpa = (GPA)message;
                hub.Send(new PAS(hub, gpa.RandomData, hub.HubSetting.Password));
            }
            else if (message is QUI && hub != null)
            {
                QUI qui = (QUI)message;
                User usr = null;
                if ((usr = hub.GetUserById(qui.Id)) != null)
                {
                    hub.FireUpdate(Actions.UserOffline, usr.UserInfo);
                    if (usr.ID == hub.Me.ID)
                    {
                        // TODO : Banning and redirect handling
                        hub.Disconnect();
                        // Redirect
                        if (!string.IsNullOrEmpty(qui.Address))
                            hub.FireUpdate(Actions.Redirect, new RedirectInfo(qui.Address, qui.Message, qui.DisconnectedBy));
                        // Banned
                        else
                        {
                            if (qui.Time != -1)
                                // Sets reconnect attempt to infinite
                                hub.KeepAliveInterval = 0;
                            else
                                hub.KeepAliveInterval = qui.Time;
                            hub.FireUpdate(Actions.Banned, new BannedInfo(qui.Time, qui.Message, qui.DisconnectedBy));
                        }
                    }
                }
            }
            else if (message is SUP)
            {
                supports = (SUP)message;
                // TODO : We should really care about what hub support.
                if (!supports.Param.Contains("ADBASE") && !supports.Param.Contains("ADBAS0") /* !hubsupports.Param.Contains("ADTIGR")*/)
                {
                    // We will just simply disconnect if hub doesnt support this right now
                    con.Disconnect("Connection doesnt support BASE or BAS0");
                }
            }
            else if (message is RES)
            {
                RES res = (RES)message;
                SearchResultInfo srinfo = new SearchResultInfo(res.Info, res.Id);
                if (hub != null)
                    hub.FireUpdate(Actions.SearchResult, srinfo);
            }
            else if (message is SCH)
            {
                SCH sch = (SCH)message;
                UserInfo usr = null;
                if (hub != null)
                {
                    User u = hub.GetUserById(sch.Id);
                    if (u != null)
                        usr = u.UserInfo;
                }
                else if (trans != null)
                    usr = trans.User;

                SendRES(sch.Info, usr);
            }
            else if (message is CTM && hub != null)
            {
                CTM ctm = (CTM)message;
                User usr = null;
                string addr = null;

                // Do we support same protocol?
                double version = 0.0;
                if (ctm.Protocol != null && ctm.Protocol.StartsWith("ADC/"))
                {
                    try
                    {
                        version = double.Parse(ctm.Protocol.Substring(4).Replace(".", ","));
                    }
                    catch { }
                }
                if (version > 1.0)
                {
                    hub.Send(new STA(hub, "141", "Protocol is not supported. I only support ADC 1.0", "TO" + ctm.Token + " PR" + ctm.Protocol));
                    return;
                }

                if ((usr = hub.GetUserById(ctm.Id)) != null && usr.UserInfo.ContainsKey(UserInfo.IP))
                {
                    addr = usr.UserInfo.Get(UserInfo.IP);
                    Transfer trans = new Transfer(addr, ctm.Port);
                    trans.Share = hub.Share;
                    User me = hub.GetUserById(hub.Me.ID);
                    // We are doing this because we want to filter out PID and so on.
                    trans.Me = me.UserInfo;
                    trans.Protocol = new TransferAdcProtocol(trans);
                    hub.FireUpdate(Actions.TransferRequest, new TransferRequest(ctm.Token, hub, usr.UserInfo));
                    hub.FireUpdate(Actions.TransferStarted, trans);
                }
            }
            else if (message is RCM && hub != null)
            {
                RCM rcm = (RCM)message;
                if (hub.Me.Mode != FlowLib.Enums.ConnectionTypes.Passive && hub.Share != null)
                {
                    User usr = null;
                    if ((usr = hub.GetUserById(rcm.Id)) != null)
                    {
                        hub.FireUpdate(Actions.TransferRequest, new TransferRequest(rcm.Token, hub, usr.UserInfo));
                        hub.Send(new CTM(hub, hub.Share.Port, rcm.Token));
                    }
                }
                else
                {
                    // TODO : we should probably return a STA message.
                }
            }
        }

        protected void SendRES(SearchInfo info, UserInfo usr)
        {
            if (hub.Share == null || usr == null)
                return;

            int maxReturns = 10;
            string token = null;
            if (info.ContainsKey(SearchInfo.TOKEN))
                token = info.Get(SearchInfo.TOKEN);

            System.Collections.Generic.List<ContentInfo> ret = new System.Collections.Generic.List<ContentInfo>(maxReturns);
            // TODO : This lookup can be done nicer
            lock (hub.Share)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, Containers.ContentInfo> var in hub.Share)
                {
                    if (var.Value == null)
                        continue;
                    bool foundEnough = false;
                    string ext = info.Get(SearchInfo.EXTENTION);
                    string sch = info.Get(SearchInfo.SEARCH);
                    if (ext != null && sch != null)
                    {
                        ContentInfo contentInfo = new ContentInfo();
                        if (info.ContainsKey(SearchInfo.TYPE))
                        {
                            switch (info.Get(SearchInfo.TYPE))
                            {
                                case "2":

                                    contentInfo.Set(ContentInfo.TTH, info.Get(SearchInfo.SEARCH));
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

            int x = 0;

            // Test against size restrictions
            for (int i = 0; i < ret.Count; i++)
            {
                bool send = true;
                long size = -1;
                try
                {
                    size = int.Parse(info.Get(SearchInfo.SIZE));
                }
                catch { }
                if (info.ContainsKey(SearchInfo.SIZETYPE) && size != -1)
                {
                    switch (info.Get(SearchInfo.SIZETYPE))
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
                    RES res = new RES(con, ret[i], token, usr);
                    if (res.Address != null && hub != null)
                    {
                        if (10 > x++)
                        {
                            // Send with UDP
                            try
                            {
                                UdpConnection.Send(res, res.Address);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        if (5 > x++)
                        {
                            // Send through hub
                            hub.Send(res);
                        }
                    }
                }
            }
        }

        public void ActOnOutMessage(FmdcEventArgs e)
        {
            if (e.Action.Equals(Actions.MainMessage) && hub != null)
            {
                Containers.MainMessage main = (Containers.MainMessage)e.Data;
                hub.Send(new MSG(hub, hub.Me, main.ShowAsMe, main.Content));
            }
            else if (e.Action.Equals(Actions.PrivateMessage) && hub != null)
            {
                Containers.PrivateMessage pm = (Containers.PrivateMessage)e.Data;
                hub.Send(new MSG(hub, hub.Me, pm.ShowAsMe, pm.Content, pm.To, pm.Group));
            }
            else if (e.Action.Equals(Actions.Password))
            {
                hub.Send(new PAS(hub, this.gpaString, e.Data.ToString()));
            }
            else if (e.Action.Equals(Actions.StartTransfer) && hub != null)
            {
                User usr = e.Data as User;
                if (usr == null)
                    return;
                // Do user support connecting?
                if (
                    usr.UserInfo.ContainsKey(UserInfo.UDPPORT)
                    && usr.UserInfo.ContainsKey("SU")
                    && usr.UserInfo.ContainsKey(UserInfo.IP)
                    && (usr.UserInfo.Get("SU").Contains("UDP4") || usr.UserInfo.Get("SU").Contains("UDP6"))
                    )
                {
                    switch (hub.Me.Mode)
                    {
                        case ConnectionTypes.Direct:
                        case ConnectionTypes.UPnP:
                        case ConnectionTypes.Forward:
                            hub.FireUpdate(Actions.TransferRequest, new TransferRequest(usr.ID, hub, usr.UserInfo));
                            hub.Send(new CTM(hub, hub.Share.Port, usr.ID));
                            break;
                        case ConnectionTypes.Passive:
                        case ConnectionTypes.Socket5:
                        case ConnectionTypes.Unknown:
                        default:
                            if (usr.UserInfo.Mode == ConnectionTypes.Passive)
                            {
                                break;
                            }
                            hub.Send(new RCM(usr.ID, hub));
                            break;
                    }
                }
                else
                {
                    return;
                }
            }
        }
        #endregion
        #endregion
        #region Event(s)
        protected void OnMessageReceived(object sender, FmdcEventArgs e) {}
        protected void OnMessageToSend(object sender, FmdcEventArgs e) { }
        #endregion
    }
}
