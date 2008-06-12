
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
using FlowLib.Containers.Security;
using System.Security.Cryptography.X509Certificates;

namespace ConsoleDemo.Examples
{
    public class TLSServer
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;

        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public TLSServer()
        {
            // Creates a empty share
            Share share = new Share("TLSServer");
            // Port to listen for incomming connections on
            share.Port = 12345;
            AddFilelistsToShare(share);

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();
            
            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 1337;
            setting.DisplayName = "TLSServer";
            setting.Protocol = "Auto";

            Hub hubConnection = new Hub(setting);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Share = share;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            //hubConnection.Me.Set(UserInfo.IP, "192.168.1.4");
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
            hubConnection.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine(string.Format("[{0}] SND: {1}", System.DateTime.Now.ToLongTimeString(), msg.Raw));
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine(string.Format("[{0}] REC: {1}", System.DateTime.Now.ToLongTimeString(), msg.Raw));
        }

        void AddFilelistsToShare(Share s)
        {
            General.AddCommonFilelistsToShare(s, currentDir + @"MyFileLists\");
        }

        void Connection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.AdcProtocol(trans);
                            trans.SecureProtocol = System.Security.Authentication.SslProtocols.Default;
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }

                        trans.ConnectionStatusChange += new FmdcEventHandler(trans_ConnectionStatusChange);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        trans.ProtocolChange += new FmdcEventHandler(trans_ProtocolChange);
                        e.Handled = true;
                    }
                    break;
            }
        }

        void trans_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            if (e.Action == TcpConnection.Connected)
                throw new System.NotImplementedException();
        }

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

            trans.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            trans.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
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
                        transferManager.StartTransfer(trans);
                        trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
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
    }
}
