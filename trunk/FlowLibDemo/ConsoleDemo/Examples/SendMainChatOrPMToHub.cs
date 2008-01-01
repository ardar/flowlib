
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

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class SendMainChatOrPMToHub : IBaseUpdater
    {
        #region IBaseUpdater Members
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        #endregion

        public SendMainChatOrPMToHub()
        {
            UpdateBase = new FlowLib.Events.FmdcEventHandler(SendMainChatOrPMToHub_UpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLibNick";

            Hub hubConnection = new Hub(settings, this);
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);
            hubConnection.Connect();
            // YOU should really listen for regmode change here.
            // Im not doing it here to make example more easy to understand.
            // Wait 10 seconds and hope we are connected.
            System.Threading.Thread.Sleep(10 * 1000);

            // Create mainchat message.
            MainMessage msg = new MainMessage(hubConnection.Me.ID, "Testing");
            // message will here be converted to right format and then be sent.
            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.MainMessage, msg));

            // Create private message.
            PrivateMessage privMsg = new PrivateMessage("DCDM++0.0495", hubConnection.Me.ID, "Testing");

            // message will here be converted to right format and then be sent.
            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.PrivateMessage, privMsg));

        }

        void SendMainChatOrPMToHub_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) {}
    }
}
