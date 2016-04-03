# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_



# Code #

```


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

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;
using FlowLib.Utils.Convert;

namespace ConsoleDemo.Examples
{
    public class RetrievingUserInfoWhenReceivingPrivateMessage : IBaseUpdater
    {
        Hub hubConnection = null;
        #region IBaseUpdater Members
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        #endregion

        public RetrievingUserInfoWhenReceivingPrivateMessage()
        {
            UpdateBase = new FlowLib.Events.FmdcEventHandler(OnUpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Nmdc";

            Hub hubConnection = new Hub(settings, this);
            hubConnection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Connect();
        }

        void hubConnection_ProtocolChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.MessageReceived -= Protocol_MessageReceived;
            }
            hubConnection.Protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(Protocol_MessageReceived);
        }

        void Protocol_MessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            FlowLib.Protocols.HubNmdc.To msg = e.Data as FlowLib.Protocols.HubNmdc.To;
            if (msg != null && !string.IsNullOrEmpty(msg.From))
            {
                User usr = hubConnection.GetUserById(msg.From);
                if (usr != null)
                {
                    // Note that this will always be empty if you dont set it. (As this took me like 1-2 min todo. i havnt added a none static one)
                    usr.UserInfo.Set(UserInfo.IP, "193.0.19.25");   // http://ripe.net

                    System.Text.StringBuilder sb = new System.Text.StringBuilder("\r\n");
                    sb.AppendLine("---------------------------------------------------------------------------------------------------------");
                    sb.AppendFormat("¦ Showing information on: {0}\r\n", usr.DisplayName );
                    sb.AppendLine("---------------------------------------------------------------------------------------------------------");
                    sb.AppendLine("¦ General information:");
                    sb.AppendFormat("¦  User: {0} (Online)\r\n", usr.DisplayName);
                    sb.AppendFormat("¦  Profile: {0}\r\n", usr.UserInfo.Account.ToString());
                    // Note that this will always be empty if you dont set it. (As this took me like 1-2 min todo. i havnt added a none static one)
                    if (usr.UserInfo.ContainsKey(UserInfo.IP))
                    {
                        sb.AppendFormat("¦  IP: {0}\r\n", usr.UserInfo.Get(UserInfo.IP));
                        try{
                            System.Net.IPHostEntry entry = System.Net.Dns.GetHostEntry(usr.UserInfo.Get(UserInfo.IP));
                            sb.AppendFormat("¦  DNS: {0}\r\n", entry.HostName);
                        }catch{}
                    }
                    sb.AppendFormat("¦  Passive: {0}\r\n", (usr.Tag.Mode == FlowLib.Enums.ConnectionTypes.Passive));
                    sb.AppendFormat("¦  Operator: {0}\r\n", usr.IsOperator);
                    sb.AppendFormat("¦  Sharesize: {0} (exact size: {1} B)\r\n", usr.UserInfo.ShareIEEE1541, usr.UserInfo.Share);
                    sb.AppendFormat("¦  Description: {0}\r\n", usr.Description);
                    sb.AppendFormat("¦  Tag: {0}\r\n", usr.Tag.Tag);
                    sb.AppendFormat("¦  Hubs: {0}\r\n", usr.UserInfo.TagInfo.Normal + usr.UserInfo.TagInfo.OP + usr.UserInfo.TagInfo.Regged);
                    sb.AppendFormat("¦  Slots: {0}\r\n", usr.UserInfo.TagInfo.Slots);
                    sb.AppendLine("---------------------------------------------------------------------------------------------------------");

                    // Create mainchat message.
                    MainMessage mm = new MainMessage(hubConnection.Me.ID, sb.ToString());
                    // message will here be converted to right format and then be sent.
                    UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.MainMessage, mm));

                    string str = string.Format("[{1}] IN: {0}", sb.ToString(), System.DateTime.Now.ToLongTimeString());
                    System.Console.WriteLine(str);
                }
            }
        }

        void OnUpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) { }
    }
}


```