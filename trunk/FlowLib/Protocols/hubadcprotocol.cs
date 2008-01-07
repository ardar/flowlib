
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
    public class HubAdcProtocol : IProtocolHub
    {
        #region Variables
        // Variables to remember
        protected SUP hubsupports = null;       // What current hub support
        protected User info = new User("");     // Hub Info (Name and description and so on).
        protected Hub hub = null;               // Current hub where this protocol is used
        protected string recieved = "";
        protected string randomData = "";       // If hub wants password. We will save randomData here.

        
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
            get { return new HubMessage(hub, Seperator); }
        }

        public IConMessage FirstCommand
        {
            get {
                return new SUP(hub);
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
        public HubAdcProtocol(Hub hub)
        {
            this.hub = hub;
            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
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
            MessageToSend(hub, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
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
            while ((pos = raw.IndexOf(Seperator)) > 0)
            {
                pos++;
                HubMessage msg = ParseMessage(raw.Substring(0, pos));
                raw = raw.Remove(0, pos);
                // Plugin handling here
                FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(hub, e);
                if (!e.Handled)
                    ActOnInMessage(msg);
            }
            // If wrong Protocol type has been set. change it to Nmdc
            if (hub.RegMode == -1 && raw.Length > 5 && raw.StartsWith("$"))
            {   // Setting hubtype to NMDC
                hub.Protocol = new HubNmdcProtocol(hub);
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
            AdcBaseMessage msg = new AdcBaseMessage(hub, raw);
            switch (msg.Action)
            {
                case "SUP":
                    msg = new SUP(hub, raw); break;
                case "SID":
                    msg = new SID(hub, raw); break;
                case "MSG":
                    msg = new MSG(hub, raw); break;
                case "INF":
                    msg = new INF(hub, raw); break;
                case "STA":
                    msg = new STA(hub, raw); break;
                case "QUI":
                    msg = new QUI(hub, raw); break;
                case "GPA":
                    msg = new GPA(hub, raw); break;
                case "CTM":
                    msg = new CTM(hub, raw); break;
                case "SND":
                    msg = new SND(hub, raw); break;
                case "GFI":
                    msg = new GFI(hub, raw); break;
                case "GET":
                    msg = new GET(hub, raw); break;
                case "RCM":
                    msg = new RCM(hub, raw); break;
                case "SCH":
                    msg = new SCH(hub, raw); break;
                case "RES":
                    msg = new RES(hub, raw); break;
                case "PAS":
                    msg = new PAS(hub, raw); break;
            }
            return msg;
        }
        #endregion
        #region Act On
        public void ActOnInMessage(IConMessage conMsg)
        {
            HubMessage message = (HubMessage)conMsg;
            if (message is INF)
            {
                INF inf = (INF)message;
                if (inf.Type.Equals("I"))
                {
                    //if (inf.Hub.RegMode == -1)    // TODO : We shouldnt have RegMode == 0 here. Fix it.
                    hub.Send(new INF(hub));
                    Info = inf.UserInfo;
                    if (Info.Description == null)
                        hub.FireUpdate(Actions.Name, new Containers.HubName(Info.DisplayName));
                    else
                        hub.FireUpdate(Actions.Name, new Containers.HubName(Info.DisplayName, Info.Description));
                }
                else
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
            else if (message is MSG)
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
            else if (message is SID)
            {
                SID sid = (SID)message;
                sid.Hub.Me.Set(UserInfo.SID, sid.Id);
            }
            else if (message is STA)
            {
                STA sta = (STA)message;
                MainMessage main = new MainMessage(info.ID, sta.Content);
                hub.FireUpdate(Actions.MainMessage, main);
            }
            else if (message is GPA)
            {
                GPA gpa = (GPA)message;
                if (hub.HubSetting.Password.Length == 0)
                {
                    randomData = gpa.RandomData;
                    hub.FireUpdate(Actions.Password, gpa);
                }
                else
                    hub.Send(new PAS(hub, gpa.RandomData, hub.HubSetting.Password));
            }
            else if (message is QUI)
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
                hubsupports = (SUP)message;
                // TODO : We should really care about what hub support.
                if (!hubsupports.Param.Contains("ADBASE")/* || !hubsupports.Param.Contains("ADTIGR")*/)
                {
                    // We will just simply disconnect if hub doesnt support this right now
                    hub.Disconnect("Hub doesnt support BASE or TIGR");
                }
            }
            else if (message is RES)
            {
                RES res = (RES)message;
                SearchResultInfo srinfo = new SearchResultInfo(res.Info, res.Id);
                hub.FireUpdate(Actions.SearchResult, srinfo);
            }
            else if (message is SCH)
            {
                SCH sch = (SCH)message;
                SendRES(sch.Info, sch.Id);
            }
            else if (message is CTM)
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
            else if (message is RCM)
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

        protected void SendRES(SearchInfo info, string userId)
        {
            if (hub.Share == null)
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
                    RES res = new RES(hub, ret[i], token, userId);
                    if (res.Address != null)
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
            if (e.Action.Equals(Actions.MainMessage))
            {
                Containers.MainMessage main = (Containers.MainMessage)e.Data;
                hub.Send(new MSG(hub, main.ShowAsMe, main.Content));
            }
            else if (e.Action.Equals(Actions.PrivateMessage))
            {
                Containers.PrivateMessage pm = (Containers.PrivateMessage)e.Data;
                hub.Send(new MSG(hub, pm.ShowAsMe, pm.Content, pm.To, pm.Group));
            }
            else if (e.Action.Equals(Actions.Password))
            {
                hub.Send(new PAS(hub, this.randomData, hub.HubSetting.Password));
            }
            else if (e.Action.Equals(Actions.StartTransfer))
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
        protected void OnMessageReceived(object sender, FmdcEventArgs e)
        {

        }
        protected void OnMessageToSend(object sender, FmdcEventArgs e)
        {

        }
        #endregion
    }
}
