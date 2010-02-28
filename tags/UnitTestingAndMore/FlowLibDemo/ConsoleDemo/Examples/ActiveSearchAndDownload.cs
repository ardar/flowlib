
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
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;
using System.Net;
using System.Collections.Generic;

namespace ConsoleDemo.Examples
{
    public class ActiveSearchAndDownload : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        Hub hubConnection;
        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

        bool sentRequest = false;
        System.Timers.Timer timer = new System.Timers.Timer();
        long last = 0;

        bool downloadManagerIsDirty = false;

        public ActiveSearchAndDownload()
        {
            UpdateBase = new FmdcEventHandler(PassiveConnectToUser_UpdateBase);

            //downloadManager.Load();

            if (downloadManager.DownloadItems.Count == 0)
            {
                // We will illustrate that we have a DownloadItem in DownloadManager that has no sources.
                // This is so we can later search for this file and add sources :)
                ContentInfo ci = new ContentInfo(ContentInfo.TTH, "H6EJQ4LYVVYRGIZYX4RXFLOFSFL7KULSTOO3PRA");
                ci.Set(ContentInfo.STORAGEPATH, currentDir + "video.tmp");
                ci.Size = 4677654528L;
                DownloadItem di = new DownloadItem(ci);
                //di.SegmentSize = 1024;
                downloadManager.AddDownload(di, new Source[] { null });
            }
            downloadManager.SegmentCompleted += new FmdcEventHandler(downloadManager_SegmentCompleted);
            downloadManager.SegmentCanceled += new FmdcEventHandler(downloadManager_SegmentCanceled);

            // Creates a empty share
            Share share = new Share("Testing");
            // Port to listen for incomming connections on
            share.Port = 12345;

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();
            // Telling that we are listening on port 1000 for incomming search results (Any means from everyone)
            UdpConnection udp = new UdpConnection(new IPEndPoint(IPAddress.Any, share.Port));
            udp.Protocol = new FlowLib.Protocols.UdpNmdcProtocol();
            udp.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);

            // Adds common filelist to share
            AddFilelistsToShare(share);

            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLib";
            setting.Protocol = "Auto";

            hubConnection = new Hub(setting, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);

            // Adds share to hub
            hubConnection.Share = share;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Connect();

            last = System.DateTime.Now.Ticks;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Start();
        }

        void downloadManager_SegmentCanceled(object sender, FmdcEventArgs e)
        {
            System.Console.WriteLine("Segment canceled.");
        }

        void downloadManager_SegmentCompleted(object sender, FmdcEventArgs e)
        {
            downloadManagerIsDirty = true;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (downloadManagerIsDirty)
            {
                lock (this)
                {
                    //downloadManager.Save();
                }
            }

            if (hubConnection != null && !hubConnection.IsDisposed && hubConnection.RegMode >= 0 && !sentRequest)
            {
                if (last > System.DateTime.Now.Subtract(new System.TimeSpan(0, 0, 30)).Ticks)
                {
                    sentRequest = true;
                    timer.Stop();

                    // Send Search
                    // Please note that we will only do this once.
                    // You shouldnt do it for every downloaditem (Doing it would result in spaming hub and would probably result in ban).
                    IList<DownloadItem> downloads = downloadManager.DownloadItems;
                    foreach (DownloadItem dwnItem in downloads)
                    {
                        if (dwnItem.ContentInfo.IsTth)
                        {
                            SearchInfo searchInfo = new SearchInfo();
                            searchInfo.Set(SearchInfo.SEARCH, dwnItem.ContentInfo.Get(ContentInfo.TTH));
                            searchInfo.Set(SearchInfo.TYPE, "2");
                            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(Actions.Search, searchInfo));
                            break;  
                        }
                    }
                }
            }
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            ConMessage msg = e.Data as ConMessage;
            if (msg != null && hubConnection != null)
            {
                // We are letting hub take care of incomming messages.
                hubConnection.Protocol.ParseRaw(msg.Bytes, msg.Bytes.Length);
            }
        }

        void Connection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.AdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }

                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        trans.ProtocolChange += new FmdcEventHandler(trans_ProtocolChange);
                        e.Handled = true;
                    }
                    break;
            }
        }

        void trans_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            IProtocolTransfer prot = e as IProtocolTransfer;
            if (prot != null)
            {
                prot.ChangeDownloadItem -= Protocol_ChangeDownloadItem;
                prot.RequestTransfer -= Protocol_RequestTransfer;
            }
            trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
            trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
        }

        void PassiveConnectToUser_UpdateBase(object sender, FmdcEventArgs e) { }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.Update -= hubConnection_Update;
            }
            hubConnection.Protocol.Update += new FmdcEventHandler(hubConnection_Update);
        }

        void AddFilelistsToShare(Share s)
        {
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, currentDir + "MyFileLists\\");
        }

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            Hub hub = (Hub)sender;
            switch (e.Action)
            {
                case Actions.SearchResult:
                    if (e.Data is SearchResultInfo)
                    {
                        SearchResultInfo srInfo = (SearchResultInfo)e.Data;
                        // TODO: Do some check to se if this result is anything we want to download
                        // This is if we want to enable auto adding of sources to a downloaditem.
                        DownloadItem tmpDownloadItem = new DownloadItem(srInfo.Info);
                        // Do downloadItem exist?
                        if (downloadManager.ContainsDownload(tmpDownloadItem))
                        {
                            User usr = hubConnection.GetUserById(srInfo.UserId);
                            if (usr != null)
                            {
                                Source src = new Source(hub.RemoteAddress.ToString(), usr.StoreID);
                                // Add Source to the existing downloaditem. Note that we first check if it exist.
                                downloadManager.AddSource(src, tmpDownloadItem);
                                // Start transfer to user
                                UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                            }
                        }
                    }
                    break;
                case Actions.TransferRequest:
                    if (e.Data is TransferRequest)
                    {
                        TransferRequest req = (TransferRequest)e.Data;
                        if (transferManager.GetTransferReq(req.Key) == null)
                            transferManager.AddTransferReq(req);
                    }
                    break;
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        transferManager.StartTransfer(trans);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                    }
                    break;
                case Actions.UserOnline:
                    last = System.DateTime.Now.Ticks;
                    break;
            }
        }

        void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (trans != null && req != null)
            {
                e.Handled = true;
                e.Data = req;
                transferManager.RemoveTransferReq(req.Key);
            }
        }

        void Protocol_ChangeDownloadItem(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            DownloadItem dwnItem = null;
            if (downloadManager.TryGetDownload(trans.Source, out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }
    }
}
