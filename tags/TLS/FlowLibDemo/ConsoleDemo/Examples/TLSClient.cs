
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
    public class TLSClient : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public TLSClient()
        {
            UpdateBase = new FmdcEventHandler(OnUpdateBase);

            // Creates a empty share
            Share share = new Share("TLSClient");
            // Adds common filelist to share
            AddFilelistsToShare(share);

            HubSetting setting = new HubSetting();
            //setting.Address = "127.0.0.1";
            //setting.Port = 1337;
            setting.Address = "192.168.1.12";
            setting.Port = 2876;
            setting.DisplayName = "TLSClient";
            setting.Protocol = "Nmdc";

            Hub hubConnection = new Hub(setting, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);

            // Adds share to hub
            hubConnection.Share = share;
            hubConnection.Connect();
        }

        void OnUpdateBase(object sender, FmdcEventArgs e) { }
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
                System.Console.WriteLine(string.Format("[{0}] CLNT SND: {1}", System.DateTime.Now.ToLongTimeString(), msg.Raw));
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine(string.Format("[{0}] CLNT REC: {1}", System.DateTime.Now.ToLongTimeString(), msg.Raw));
        }

        void AddFilelistsToShare(Share s)
        {
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, currentDir + @"MyFileLists\");
        }

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            Hub hub = (Hub)sender;
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        transferManager.StartTransfer(trans);
                        trans.SecureUpdate += new FmdcEventHandler(trans_SecureUpdate);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.ConnectionStatusChange += new FmdcEventHandler(trans_ConnectionStatusChange);
                        trans.ProtocolChange += new FmdcEventHandler(trans_ProtocolChange);
                    }
                    break;
                case Actions.TransferRequest:
                    if (e.Data is TransferRequest)
                    {
                        TransferRequest req = (TransferRequest)e.Data;
                        transferManager.AddTransferReq(req);
                    }
                    break;
                case Actions.UserOnline:
                    UserInfo usrInfo = e.Data as UserInfo;
                    User usr = null;
                    if (usrInfo.DisplayName == "TLSServer")
                        usr = hub.GetUserByNick("TLSServer");
                    //if (usrInfo.DisplayName == "DCDM495")
                    //    usr = hub.GetUserByNick("DCDM495");
                    if (usr != null)
                    {
                        // Adding filelist of unknown type to download manager.
                        // to the user Flow84
                        ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);
                        info.Set(ContentInfo.STORAGEPATH, currentDir + @"Filelists\" + usr.StoreID + ".filelist");
                        downloadManager.AddDownload(new DownloadItem(info), new Source(hub.RemoteAddress.ToString(), usr.StoreID));
                        // Start transfer to user
                        UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                    }
                    break;
            }
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

        void trans_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans != null && e.Action == Transfer.Connected)
                trans.SecureProtocol = System.Security.Authentication.SslProtocols.Default;
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

        void trans_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            IProtocolTransfer prot = e as IProtocolTransfer;
            if (prot != null)
            {
                prot.ChangeDownloadItem -= Protocol_ChangeDownloadItem;
            }

            trans.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            trans.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }


    }
}
