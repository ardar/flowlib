
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

namespace ConsoleDemo.Examples
{
    public class ConnectToHub
    {
        public ConnectToHub()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            // The below is one way to say what protocol we should use when connecting to hub.
            //settings.Protocol = "Nmdc";   // Here we are saying we know it is a Nmdc hub
            //settings.Protocol = "Adc";   // Here we are saying we know it is a Adc hub
            //settings.Protocol = "Auto";   // Here we tell it we dont care what protocol it uses (Adc or Nmdc). Just try to connect.

            Hub hubConnection = new Hub(settings);
            // This is a other way to say what protocol we should use when connecting
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);

            hubConnection.Connect();
        }
    }
}
