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
using FlowLib.Events;

namespace ConsoleDemo.Examples
{
    public class ReceiveMainChatOrPMFromHub
    {
        public ReceiveMainChatOrPMFromHub()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            Hub hubConnection = new Hub(settings);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Connect();
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(prot_Update);
            }
        }

        void prot_Update(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.MainMessage:
                    MainMessage msgMain = e.Data as MainMessage;
                    System.Console.Write(string.Format("[{0}] <{1}> {2}\r\n",
                        System.DateTime.Now.ToLongTimeString(),
                        msgMain.From,
                        msgMain.Content));
                    break;

                case Actions.PrivateMessage:
                    PrivateMessage msgPriv = e.Data as PrivateMessage;
                    System.Console.Write(string.Format("[{0}] PM:{1}\r\n",
                        System.DateTime.Now.ToLongTimeString(),
                        msgPriv.Content));
                    break;
            }
        
        
        }
    }
}


```