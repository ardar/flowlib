
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
using FlowLib.Protocols.Adc;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Connections;

namespace FlowLib.Protocols
{
    /// <summary>
    /// ADC Protocol
    /// </summary>
    public class HubAdcProtocol : IProtocolHub
    {
        #region Variables
        // Variables to remember
        protected string gpaString = "";        // GPA Random data
        protected string hubsupports = null;    // What current hub support
        protected User info = new User("");  // Hub Info (Name and description and so on).
        protected Hub hub = null;              // Current hub where this protocol is used
        protected string recieved = "";

        
        protected static string yoursupports = "ADBASE ADTIGR BZIP";

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
            // If wrong Protocol type has been set. change it to ADC
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
                    msg = new SUP(hub, raw);
                    break;
                case "SID":
                    msg = new SID(hub, raw);
                    break;
                case "MSG":
                    msg = new MSG(hub, raw);
                    break;
                case "INF":
                    msg = new INF(hub, raw);
                    break;
                case "STA":
                    msg = new STA(hub, raw);
                    break;
                case "QUI":
                    msg = new QUI(hub, raw);
                    break;
                case "GPA":
                    msg = new GPA(hub, raw);
                    break;
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
                    if (hub.GetUserById(inf.From) == null)
                        hub.FireUpdate(Actions.UserOnline, inf.UserInfo);
                    else
                        hub.FireUpdate(Actions.UserInfoChange, inf.UserInfo);
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
                sid.Hub.Me.SID = sid.Param;
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
                hub.Send(new PAS(hub, this.gpaString, (string)e.Data));
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

        #region IProtocolHub Members
        public bool OnStartTransfer(User usr)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}
