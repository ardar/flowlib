# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

_[To learn how to actively connect to a user click here.](CodeExamplesActiveDownloadFilelistFromUser.md)_


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
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;
using FlowLib.Utils.Statistics;

namespace ConsoleDemo.Examples
{
    public class CollectTransferedInformationFromFilelistDownload : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        GeneralProtocolDataCollector stats = null;

        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;
        bool sentRequest = false;

        public CollectTransferedInformationFromFilelistDownload()
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

            Hub hubConnection = new Hub(setting, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            // Adds share to hub
            hubConnection.Share = share;
            //hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Me.TagInfo.Slots = 2;
            hubConnection.Connect();


            FlowLib.Utils.Convert.General.BinaryPrefixes bp;
            Console.WriteLine("Press any key to update information");
            do
            {
                Console.ReadKey(true);
                Console.Clear();
                Console.WriteLine("Press any key to update information");
                Console.WriteLine("==================================");
                if (stats != null)
                {
                    Console.WriteLine("Total data sent: " + FlowLib.Utils.Convert.General.FormatBytes(stats.TotalBytesSent, out bp) + bp);
                    Console.WriteLine("Total data received: " + FlowLib.Utils.Convert.General.FormatBytes(stats.TotalBytesReceived, out bp) + bp);
                    Console.WriteLine("current download speed: " + FlowLib.Utils.Convert.General.FormatBytes(stats.CurrentReceiveSpeed, out bp) + bp + "/s");
                    Console.WriteLine("current upload speed: " + FlowLib.Utils.Convert.General.FormatBytes(stats.CurrentSendSpeed, out bp) + bp + "/s");
                    Decimal d = new decimal(stats.MaximumReceiveSpeed);
                    Console.WriteLine("Maximum download speed: " + FlowLib.Utils.Convert.General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                    d = new decimal(stats.MaximumSendSpeed);
                    Console.WriteLine("Maximum upload speed: " + FlowLib.Utils.Convert.General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                    d = new decimal(stats.MinimumReceiveSpeed);
                    Console.WriteLine("Minimum download speed: " + FlowLib.Utils.Convert.General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                    d = new decimal(stats.MinimumSendSpeed);
                    Console.WriteLine("Minimum upload speed: " + FlowLib.Utils.Convert.General.FormatBytes(decimal.ToInt64(d), out bp) + bp + "/s");
                }
                else
                {
                    Console.WriteLine("No transfer has started yet.");
                }
                Console.WriteLine("==================================");
            } while (true);
        }

        void Connection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        // Here we bind transfer to our data collector
                        stats = new GeneralProtocolDataCollector(trans);
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
                        // Here we bind transfer to our data collector
                        stats = new GeneralProtocolDataCollector(trans);
                        transferManager.StartTransfer(trans);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                    }
                    break;
                case Actions.UserOnline:
                    bool hasMe = (hub.GetUserById(hub.Me.ID) != null);
                    if (!sentRequest && hasMe)
                    {
                        User usr = null;
                        if ((usr = hub.GetUserByNick("DCpp706")) != null)
                        {
                            // Adding filelist of unknown type to download manager.
                            // to the user DCpp706
                            ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);
                            info.Set(ContentInfo.STORAGEPATH, currentDir + "Filelists\\" + usr.StoreID + ".filelist");
                            downloadManager.AddDownload(new DownloadItem(info), new Source(hub.RemoteAddress.ToString(), usr.StoreID));
                            // Start transfer to user
                            UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                            sentRequest = true;
                        }
                    }
                    break;
            }
        }

        void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (req != null)
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

```