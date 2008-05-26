using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

namespace ConsoleDemo
{
    class WorkingConnections
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;
        Share share = new Share("PIP");

        public WorkingConnections()
        {
            share.HashThreadSleep = 1;
            share.Load(@"C:\Private\Code\FlowLib\trunk\FlowLibDemo\ConsoleDemo\bin\Debug\");
            share.Reload();
            share.Port = 1000;

            if (share.VirtualDirs.Count == 0)
            {
                share.AddVirtualDir(@"YnHub\", @"C:\Private\YnHub\");
                share.HashAutoSaveCount = 1;
                share.HashAllowDuplicate = true;
                share.LastModifiedChanged += new FmdcEventHandler(share_LastModifiedChanged);
                return;
            }
            AddFilelistsToShare(share);

            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update);
            incomingConnectionListener.Start();

            HubSetting setting = new HubSetting();
            //setting.Address = "mrmikejj.co.uk";
            //setting.Port = 1669;
            setting.Address = "flow84.no-ip.org";
            setting.Port = 2876;
            //setting.Address = "127.0.0.1";
            //setting.Port = 411;
            setting.Password = "1";

            setting.DisplayName = "FlowLibNick";
            setting.Password = "1";
            setting.Protocol = "Nmdc";

            Hub hubConnection = new Hub(setting);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.ConnectionStatusChange += new FmdcEventHandler(hubConnection_ConnectionStatusChange);

            hubConnection.Share = share;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Me.Set(UserInfo.IP, "82.182.95.201");
            //hubConnection.Me.Set(UserInfo.IP, "127.0.0.1");

            hubConnection.Connect();
        }

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
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);
                            trans.Listen();
                        }

                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        e.Handled = true;
                    }
                    break;
            }
        }

        void share_LastModifiedChanged(object sender, FmdcEventArgs e)
        {
            Share s = sender as Share;
            if (s != null)
                AddFilelistsToShare(s);
        }

        void hubConnection_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            System.Console.WriteLine(string.Format("{0}:{1} -> {2}", sender, e.Action, e.Data));
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol protocol = e.Data as IProtocol;
            if (protocol != null)
                protocol.Update -= hubConnection_Update;
            hubConnection.Protocol.Update += new FmdcEventHandler(hubConnection_Update);
            hubConnection.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            System.Console.WriteLine(string.Format("OUT:{0}", ((StrMessage)e.Data).Raw));
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            System.Console.WriteLine(string.Format("IN:{0}", ((StrMessage)e.Data).Raw));
        }

        void hubConnection_Update(object sender, FmdcEventArgs e)
        {
            //UserInfo usrInfo = null;
            switch (e.Action)
            {
                case Actions.MainMessage:
                    MainMessage main = e.Data as MainMessage;
                    if (main != null)
                        System.Console.WriteLine(string.Format("<{0}> {1}", main.From, main.Content));
                    break;
                //    case Actions.UserOnline:
                //        usrInfo = e.Data as UserInfo;
                //        if (usrInfo != null)
                //            System.Console.WriteLine(string.Format("ONLINE User: {0} is Operator: {1}", usrInfo.ID, usrInfo.IsOperator));
                //        break;
                //    case Actions.UserInfoChange:
                //        usrInfo = e.Data as UserInfo;
                //        if (usrInfo != null)
                //            System.Console.WriteLine(string.Format("CHANGE User: {0} is Operator: {1}", usrInfo.ID, usrInfo.IsOperator));
                //        break;
                //    default:
                //        break;
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
            }
        }

        void Protocol_ChangeDownloadItem(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            //DownloadItem dwnItem = null;
            //if (downloadManager.TryGetDownload(new Source(null, trans.User.ID), out dwnItem))
            //{
            //    e.Data = dwnItem;
            //    e.Handled = true;
            //}
        }

    }
}
