# Description #

**This example shows how much data is transfered in both ways when connected to hub.**

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_



# Code #

```

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

using System;
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;
using FlowLib.Utils.Statistics;
using FlowLib.Utils.Convert;
using FlowLib.Events;

namespace ConsoleDemo.Examples
{
    public class CollectTransferedInformationForHub
    {
        GeneralProtocolDataCollector stats = null;

        public CollectTransferedInformationForHub()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";   // Here we tell it we dont care what protocol it uses (Adc or Nmdc). Just try to connect.

            Hub hubConnection = new Hub(settings);
            // Here we bind hub to our data collector
            stats = new GeneralProtocolDataCollector(hubConnection);
            hubConnection.Connect();
            General.BinaryPrefixes bp;
            Console.WriteLine("Press any key to update information");
            do
            {
                Console.ReadKey(true);
                Console.Clear();
                Console.WriteLine("Press any key to update information");
                Console.WriteLine("==================================");
                Console.WriteLine("Total data sent: " + General.FormatBytes(stats.TotalBytesSent, out bp) + bp);
                Console.WriteLine("Total data received: " + General.FormatBytes(stats.TotalBytesReceived, out bp) + bp);
                Console.WriteLine("current download speed: " + General.FormatBytes(stats.CurrentReceiveSpeed, out bp) + bp + "/s");
                Console.WriteLine("current upload speed: " + General.FormatBytes(stats.CurrentSendSpeed, out bp) + bp + "/s");
                Decimal d = new decimal(stats.MaximumReceiveSpeed);
                Console.WriteLine("Maximum download speed: " + General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                d = new decimal(stats.MaximumSendSpeed);
                Console.WriteLine("Maximum upload speed: " + General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                d = new decimal(stats.MinimumReceiveSpeed);
                Console.WriteLine("Minimum download speed: " + General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                d = new decimal(stats.MinimumSendSpeed);
                Console.WriteLine("Minimum upload speed: " + General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                Console.WriteLine("==================================");
            } while (true);
        }
    }
}

```