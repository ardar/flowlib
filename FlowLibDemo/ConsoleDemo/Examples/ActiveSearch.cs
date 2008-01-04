
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

        public ActiveSearch()
        {
            UpdateBase = new FmdcEventHandler(ActiveSearch_UpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "flow84.no-ip.org";
            settings.Port = 2876;
            settings.DisplayName = "FlowLibNick";
            //settings.Protocol = "Nmdc";
            settings.Password = "1";

            hubConnection = new Hub(settings, this);
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);
            hubConnection.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived2);
            hubConnection.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);

            hubConnection.Update += new FlowLib.Events.FmdcEventHandler(hubConnection_Update);
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
            hubConnection.ConnectionStatusChange += new FmdcEventHandler(hubConnection_ConnectionStatusChange);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            HubMessage msg = e.Data as HubMessage;
            if (msg != null)
                System.Console.WriteLine("OUT:" + msg.Raw);
        }

        void Protocol_MessageReceived2(object sender, FmdcEventArgs e)
        {
            HubMessage msg = e.Data as HubMessage;
            if (msg != null)
                System.Console.WriteLine("IN:" + msg.Raw);
        }

        void hubConnection_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            if (e.Action == TcpConnection.Connected)
            {
                // Send Search
                //SearchInfo searchInfo = new SearchInfo();
                ////searchInfo.Set(SearchInfo.SEARCH, "Ecma-334");
                //searchInfo.Set(SearchInfo.SEARCH, "books");
                //searchInfo.Set(SearchInfo.SIZE, "1000");
                //searchInfo.Set(SearchInfo.SIZETYPE, "1");
                //UpdateBase(this, new FlowLib.Events.FmdcEventArgs(Actions.Search, searchInfo));
            }
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
            }
        }
    }
}
