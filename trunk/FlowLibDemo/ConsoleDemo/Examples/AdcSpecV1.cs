
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
using FlowLib.Utils.Hash;
using FlowLib.Utils.Convert;

namespace ConsoleDemo.Examples
{
    public class AdcSpecV1
    {
        Share share = new Share("AdcSpecV1");
        string dir = @"C:\Private\Code\FlowLib\trunk\FlowLibDemo\ConsoleDemo\bin\Debug\";

        public AdcSpecV1()
        {
            share.Load(dir);
            if (share.VirtualDirs.Count == 0)
            {
                share.AddVirtualDir(@"Books\", @"C:\Private\Books\");
                share.AddVirtualDir(@"Images\", @"C:\Private\Download\BIGFILE\");
                share.HashingCompleted += new System.EventHandler<System.EventArgs>(share_HashingCompleted);
            }

            HubSetting settings = new HubSetting();
            settings.Address = "vidfamne.myftp.org";
            settings.Port = 12345;
            settings.DisplayName = "FlowLibNick";
            settings.UserDescription = "FlowLib";
            // TODO : Do this work
            //settings.Protocol = "Nmdc";

            Tiger tiger = new Tiger();
            byte[] data = tiger.ComputeHash(System.Text.Encoding.UTF8.GetBytes("FlowLib"));
            string pid = Base32.Encode(data);
            data = tiger.ComputeHash(Base32.Decode(pid));
            string cid = Base32.Encode(data);

            Hub hubConnection = new Hub(settings);

            hubConnection.Me.Set(UserInfo.PID, pid);
            hubConnection.Me.Set(UserInfo.CID, cid);
            hubConnection.Share = share;

            hubConnection.Update += new FlowLib.Events.FmdcEventHandler(hubConnection_Update);

            hubConnection.Protocol = new FlowLib.Protocols.HubAdcProtocol(hubConnection);
            hubConnection.Protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FlowLib.Events.FmdcEventHandler(Protocol_MessageToSend);

            hubConnection.Connect();
        }

        void hubConnection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case FlowLib.Events.Actions.MainMessage:
                    MainMessage msg = e.Data as MainMessage;
                    if (msg == null)
                        return;
                    System.Console.WriteLine(string.Format("<{0}> {1}", msg.From, msg.Content));
                    break;
                default:
                    break;
            }
        }

        void Protocol_MessageToSend(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            HubMessage msg = e.Data as HubMessage;
            if (msg != null && msg.Raw.Length >= 4)
            {
                switch (msg.Raw.Substring(0, 4))
                {
                    default:
                        System.Console.WriteLine("OUT: " + msg.Raw);
                        break;
                }
            }
        }

        void Protocol_MessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            HubMessage msg = e.Data as HubMessage;
            if (msg != null && msg.Raw.Length >= 4)
            {
                switch (msg.Raw.Substring(0, 4))
                {
                    //case "BINF":
                    //case "BSCH":
                    //case "IMSG":
                        //break;
                    default:
                        System.Console.WriteLine("IN: " + msg.Raw);
                        break;
                }
            }
        }

        void share_HashingCompleted(object sender, System.EventArgs e)
        {
            share.Save(dir);
            System.Console.WriteLine("Saved");
        }
    }
}
