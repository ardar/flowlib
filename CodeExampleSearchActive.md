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

using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Events;
using FlowLib.Interfaces;
using System.Net;

namespace ConsoleDemo.Examples
{
    public class ActiveSearch : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;
        Hub hubConnection = null;
        bool sentRequest = false;

        public ActiveSearch()
        {
            UpdateBase = new FmdcEventHandler(ActiveSearch_UpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            hubConnection = new Hub(settings, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Connect();

            Share share = new Share("Test");
            share.Port = 1000;

            // Telling that we are listening on port 1000 for incomming search results (Any means from everyone)
            UdpConnection udp = new UdpConnection(new IPEndPoint(IPAddress.Any, share.Port));
            udp.Protocol = new FlowLib.Protocols.UdpNmdcProtocol();
            udp.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            // Enable Active searching
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Share = share;
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.MessageReceived -= Protocol_MessageReceived;
                prot.MessageToSend -= Protocol_MessageToSend;
                prot.Update -= hubConnection_Update;
            }
            hubConnection.Protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(Protocol_MessageReceived2);
            hubConnection.Protocol.MessageToSend += new FlowLib.Events.FmdcEventHandler(Protocol_MessageToSend);
            hubConnection.Protocol.Update += new FlowLib.Events.FmdcEventHandler(hubConnection_Update);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine("OUT:" + msg.Raw);
        }

        void Protocol_MessageReceived2(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine("IN:" + msg.Raw);
        }

        void ActiveSearch_UpdateBase(object sender, FmdcEventArgs e) { }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            ConMessage msg = e.Data as ConMessage;
            if (msg != null)
            {
                // We are letting hub take care of incomming messages.
                hubConnection.Protocol.ParseRaw(msg.Bytes, msg.Bytes.Length);
            }
        }

        void hubConnection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            Hub hub = sender as Hub;
            if (hub == null)
                return;
            switch (e.Action)
            {
                case Actions.SearchResult:
                    if (e.Data is SearchResultInfo)
                    {
                        SearchResultInfo srInfo = (SearchResultInfo)e.Data;
                        // This is if we want to enable auto adding of sources to a downloaditem.
                        //FlowLib.Managers.DownloadManager dm = new FlowLib.Managers.DownloadManager();
                        //DownloadItem tmpDownloadItem = new DownloadItem(srInfo.Info);
                        //// Do downloadItem exist?
                        //if (dm.ContainsDownload(tmpDownloadItem))
                        //{
                        //    // Add Source to the existing downloaditem. Note that we first check if it exist.
                        //    dm.AddDownload(tmpDownloadItem, srInfo.Source);
                        //}
                    }
                    break;
                case Actions.UserOnline:
                    bool hasMe = (hub.GetUserById(hub.Me.ID) != null);
                    if (!sentRequest && hasMe)
                    {
                        // Send Search
                        SearchInfo searchInfo = new SearchInfo();
                        searchInfo.Set(SearchInfo.SEARCH, "Ubuntu");
                        //searchInfo.Set(SearchInfo.SIZE, "1000");
                        //searchInfo.Set(SearchInfo.SIZETYPE, "1");
                        UpdateBase(this, new FlowLib.Events.FmdcEventArgs(Actions.Search, searchInfo));
                        sentRequest = true;
                    }
                    break;
            }
        }
    }
}


```