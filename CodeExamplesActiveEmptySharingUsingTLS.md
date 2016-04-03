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
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
using FlowLib.Containers.Security;
using System.Security.Cryptography.X509Certificates;
#endif

namespace ConsoleDemo.Examples
{
    public class ActiveEmptySharingUsingTLS
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        TcpConnectionListener incomingConnectionListenerTLS = null;
        int tlsport = 54321;
        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public ActiveEmptySharingUsingTLS()
        {
            // Creates a empty share
            Share share = new Share("Testing");
            // Port to listen for incomming connections on
            share.Port = 12345;
            AddFilelistsToShare(share);

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();

            // TLS listener
            incomingConnectionListenerTLS = new TcpConnectionListener(tlsport);
            incomingConnectionListenerTLS.Update += new FmdcEventHandler(Connection_UpdateTLS);
            incomingConnectionListenerTLS.Start();

            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLibActiveTLS";
            setting.Protocol = "Auto";

            Hub hubConnection = new Hub(setting);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Share = share;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
            hubConnection.Me.Set(UserInfo.SECURE, tlsport.ToString());
#endif
            hubConnection.Connect();
        }

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
            General.AddCommonFilelistsToShare(s, currentDir + "MyFileLists\\");
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

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferRequest:
                    if (e.Data is TransferRequest)
                    {
                        TransferRequest req = (TransferRequest)e.Data;
                        transferManager.AddTransferReq(req);
                    }
                    break;
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
#if !COMPACT_FRAMEWORK
                        // Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
                        trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
#endif
                        transferManager.StartTransfer(trans);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
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
            if (downloadManager.TryGetDownload(new Source(null, trans.User.ID), out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }

#if !COMPACT_FRAMEWORK
// Security, Windows Mobile doesnt support SSLStream so we disable this feature for it.
        void trans_SecureUpdate(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.SecuritySelectLocalCertificate:
                    LocalCertificationSelectionInfo lc = e.Data as LocalCertificationSelectionInfo;
                    if (lc != null)
                    {
                        string file = System.AppDomain.CurrentDomain.BaseDirectory + "FlowLib.cer";
                        lc.SelectedCertificate = X509Certificate.CreateFromCertFile(file);
                        e.Data = lc;
                    }

                    break;
                case Actions.SecurityValidateRemoteCertificate:
                    CertificateValidationInfo ct = e.Data as CertificateValidationInfo;
                    if (ct != null)
                    {
                        ct.Accepted = true;
                        e.Data = ct;
                        e.Handled = true;
                    }
                    break;
            }
        }

        void Connection_UpdateTLS(object sender, FlowLib.Events.FmdcEventArgs e)
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
                            trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
                            trans.SecureProtocol = FlowLib.Enums.SecureProtocols.TLS;
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
#endif
    }
}


```