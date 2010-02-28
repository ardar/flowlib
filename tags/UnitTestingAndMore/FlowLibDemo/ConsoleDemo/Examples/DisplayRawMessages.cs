
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

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class DisplayRawMessages
    {
        public DisplayRawMessages()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            Hub hubConnection = new Hub(settings);
            hubConnection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Connect();
        }

        void hubConnection_ProtocolChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.MessageReceived -= Protocol_MessageReceived;
                prot.MessageToSend -= Protocol_MessageToSend;
            }
            hubConnection.Protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FlowLib.Events.FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine(string.Format("OUT: {0}", msg.Raw));
        }

        void Protocol_MessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
               System.Console.WriteLine(string.Format("IN: {0}", msg.Raw));
        }
    }
}
