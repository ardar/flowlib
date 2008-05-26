
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

namespace ConsoleDemo
{
    class Testing : IBaseUpdater
    {
        #region IBaseUpdater Members
        public event FmdcEventHandler UpdateBase;
        #endregion


        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        Share share = new Share("PIP");

        void OnReload()
        {
            share.Reload();
        }

        public Testing()
        {
            UpdateBase = new FmdcEventHandler(Testing_UpdateBase);

            share.HashThreadSleep = 10;
            share.Load(@"C:\Private\Code\FlowLib\trunk\FlowLibDemo\ConsoleDemo\bin\Debug\");
            share.HashAutoSaveCount = 1;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(OnReload));
            t.IsBackground = true;
            t.Start();
            //share.Reload();
            share.Port = 1000;

            if (share.VirtualDirs.Count < 3)
            {
                share.AddVirtualDir(@"YnHub\", @"C:\Private\YnHub\");
                share.AddVirtualDir(@"IMG\", @"C:\Private\Download\BIGFILE\");
                share.AddVirtualDir(@"Books\", @"C:\Private\Books\");
                share.HashAutoSaveCount = 1;
                share.HashAllowDuplicate = true;
                share.LastModifiedChanged += new FmdcEventHandler(share_LastModifiedChanged);
                //return;
            }
            AddFilelistsToShare(share);

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();

            HubSetting setting = new HubSetting();
            //setting.Address = "mrmikejj.co.uk";
            //setting.Port = 1669;
            //setting.Address = "flow84.no-ip.org";
            //setting.Port = 2876;
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.Password = "1";
            setting.DisplayName = "FlowLibNick";
            setting.Protocol = "Nmdc";

            setting.Address = "vidfamne.myftp.org";
            setting.Port = 12345;
            setting.DisplayName = "FlowLib";
            setting.UserDescription = "FlowLib";
            setting.Password = "hello";
            setting.Protocol = "Adc";

            Hub hubConnection = new Hub(setting, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.ConnectionStatusChange += new FmdcEventHandler(hubConnection_ConnectionStatusChange);

            hubConnection.Share = share;
            //hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Passive;
            //hubConnection.Me.Set(UserInfo.IP, "82.182.95.201");

            hubConnection.Me.Set(UserInfo.PID, "XMKEULTT4HHAHRNRUJNQMSRWXEVAXXOYXFWIUIQ");
            hubConnection.Me.TagInfo.Slots = 1;
            //hubConnection.Me.Set(UserInfo.IP, "127.0.0.1");
            
            hubConnection.Connect();

            //for (int i = 1; i < 10; i++)
            //{
            //    System.Threading.Thread.Sleep(10 * 1000);
            //    setting.DisplayName = "FlowLibNick" + i.ToString();
            //    Hub hubTmp = new Hub(setting, this);
            //    hubTmp.Share = share;
            //    hubTmp.Me.TagInfo.Slots = 1;
            //    hubTmp.Connect();
                
            //}

            //User usr = hubConnection.GetUserByNick("Flow84@Laptop");
            //User usr = hubConnection.GetUserByNick("VidFamne");
            //User usr = hubConnection.GetUserByNick("PipL2");
            User usr = hubConnection.GetUserByNick("FlowLibNick");
            //User usr = null;
            if (usr != null)
            {
                System.Console.WriteLine(string.Format("REGMODE{0}, ME:{1}, USR:{2}", hubConnection.RegMode, hubConnection.Me.TagInfo.Normal, usr.Tag.Normal));

                // Adding filelist of unknown type to download manager.
                // to the user Flow84
                //ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);
                //info.Set(ContentInfo.STORAGEPATH, @"C:\Private\Code\FlowLib\trunk\FlowLib\bin\Debug\" + usr.ID + ".filelist");
                //downloadManager.AddDownload(new DownloadItem(info), new Source(null, usr.ID));
                //// Start transfer to user
                //UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
            }
        }

        void Testing_UpdateBase(object sender, FmdcEventArgs e) { }

        void AddFilelistsToShare(Share s)
        {
            General.AddCommonFilelistsToShare(s, System.AppDomain.CurrentDomain.BaseDirectory + @"MyFileLists\");
        }

        void Connection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        // TODO : This should be changed. we should not set protocol here.
                        // Should be set in TransferRequest
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.AdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }

                        trans.ProtocolChange += new FmdcEventHandler(trans_ProtocolChange);
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        trans.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
                        trans.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
                        e.Handled = true;
                    }
                    break;
            }
        }

        void trans_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
            trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
            trans.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            trans.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }
        
        void share_LastModifiedChanged(object sender, FmdcEventArgs e)
        {
            Share s = sender as Share;
            if (s != null)
                AddFilelistsToShare(s);
        }

        void hubConnection_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            System.Console.WriteLine( string.Format("{0}:{1} -> {2}", sender, e.Action, e.Data));
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection == null)
                return;
            IProtocol protocol = e.Data as IProtocol;
            if (protocol != null)
                protocol.Update -= hubConnection_Update;
            hubConnection.Protocol.Update += new FmdcEventHandler(hubConnection_Update);
            hubConnection.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
            {
                if (msg is FlowLib.Protocols.Adc.AdcBaseMessage)
                {
                    FlowLib.Protocols.Adc.AdcBaseMessage adc = msg as FlowLib.Protocols.Adc.AdcBaseMessage;
                    switch (adc.Action)
                    {
                        default:
                            System.Console.WriteLine(string.Format("OUT:{0}", msg.Raw.TrimEnd(msg.Connection.Protocol.Seperator[0])));
                            break;
                    }
                }
                else
                    System.Console.WriteLine(string.Format("OUT:{0}", msg.Raw.TrimEnd(msg.Connection.Protocol.Seperator[0])));
            }
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
            {
                if (msg is FlowLib.Protocols.Adc.AdcBaseMessage)
                {
                    FlowLib.Protocols.Adc.AdcBaseMessage adc = msg as FlowLib.Protocols.Adc.AdcBaseMessage;
                    switch (adc.Type)
                    {
                        case "B":
                        case "F":
                            break;
                        default:
                            if (msg.IsValid)
                                System.Console.WriteLine(string.Format("IN:{0}", msg.Raw));
                            else
                                System.Console.WriteLine(string.Format("IN:{0}", msg.Raw));
                            break;
                    }
                }
                else
                    System.Console.WriteLine(string.Format("IN:{0}", msg.Raw));
            }
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
