using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

namespace ConsoleDemo
{
    class Testing
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        TcpConnectionListener incomingConnectionListener = null;

        public Testing()
        {
            Share share = new Share("Testing");
            share.Port = 6500;
            AddFilelistsToShare(share);
            
            incomingConnectionListener = new TcpConnectionListener(share.Port);
            incomingConnectionListener.Update += new FmdcEventHandler(Connection_Update2);
            incomingConnectionListener.Start();
            
            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLib";

            Hub hubConnection = new Hub(setting);
            hubConnection.Update += new FlowLib.Events.FmdcEventHandler(Connection_Update);
            hubConnection.Protocol = new HubNmdcProtocol(hubConnection);
            hubConnection.Share = share;
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            hubConnection.Connect();
        }

        void AddFilelistsToShare(Share s)
        {
            General.AddCommonFilelistsToShare(s, @"C:\Private\FMDC\PiP\FlowLibDemo\ConsoleDemo\bin\Debug\MyFileLists\");
        }

        void Connection_Update2(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        // TODO : This should be changed. we should not set protocol here.
                        if (trans.Protocol == null)
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);

                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                    }
                    break;
            }
        }

        void Connection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            Hub hub = sender as Hub;
            if (hub == null)
                return;
            switch (e.Action)
            {
                case Actions.UserOnline:
                    UserInfo userInfo = e.Data as UserInfo;
                    if (userInfo == null)
                        return;
                    if (!userInfo.IsOperator && !userInfo.ID.Equals(hub.Me.ID))
                    {
                        transferManager.AddTransferReq(userInfo.ID, hub, userInfo);
                        ContentInfo content = new ContentInfo(userInfo.ID, FlowLib.Enums.ContentIdTypes.Filelist);
                        content.SystemPath =  System.AppDomain.CurrentDomain.BaseDirectory + @"\FileLists\" + userInfo.ID + ".filelist";
                        downloadManager.AddDownload(new DownloadItem(content), new Source(null, userInfo.ID));
                        hub.Send(new FlowLib.Protocols.HubNmdc.ConnectToMe(userInfo.ID, hub.Share.Port, hub));
                    }
                    break;
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
                        trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
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
            DownloadItem dwnItem = null;
            if (downloadManager.TryGetDownload(new Source(null, trans.User.ID), out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }
    }
}
