
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
using System.Collections.Generic;
using System.Threading;

namespace ConsoleDemo.Examples
{
    public class MultiConnections : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        System.Timers.Timer timer = new System.Timers.Timer();
        long last = 0;

        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
        bool sentRequest = false;

        Hub hub = null;

        public MultiConnections()
        {
            UpdateBase = new FmdcEventHandler(PassiveConnectToUser_UpdateBase);

            // Creates a empty share
            Share share = new Share("Testing");
            // Port to listen for incomming connections on
            share.Port = 12345;

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();

            // Adds common filelist to share
            AddFilelistsToShare(share);

            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLib";
            setting.Protocol = "Auto";

            hub = new Hub(setting, this);
            hub.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            // Adds share to hub
            hub.Share = share;
            hub.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hub.Connect();


            last = System.DateTime.Now.Ticks;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.AutoReset = true;
            timer.Interval = 1000;
            timer.Start();
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

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (hub != null && !hub.IsDisposed && hub.RegMode >= 0 && !sentRequest)
            {
                if (last > System.DateTime.Now.Subtract(new System.TimeSpan(0, 0, 30)).Ticks)
                {
                    SortedList<string, User> usrs = new SortedList<string, User>(hub.Userlist);
                    foreach (var item in usrs)
                    {
                        if (item.Value.ID == hub.Me.ID || item.Value.ID == "-YnHub-")
                        {
                            continue;
                        }
                        User usr = item.Value;
                        new Thread(
                        delegate()
                        {
                            // Adding filelist of unknown type to download manager.
                            ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);
                            info.Set(ContentInfo.ID, usr.StoreID);
                            info.Set(ContentInfo.STORAGEPATH, currentDir + "Filelists\\" + usr.StoreID + ".filelist");
                            downloadManager.AddDownload(new DownloadItem(info), new Source(hub.RemoteAddress.ToString(), usr.StoreID));
                            // Start transfer to user
                            UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                        }).Start();
                    }
                        sentRequest = true;
                }
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
