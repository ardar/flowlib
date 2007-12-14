using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Managers;

using System;
using System.Text;

namespace ConsoleDemo
{
    class UiHub : IBaseUpdater
    {
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        public Hub hub = null;
        public User user = null;
        public DownloadManager downloadManager = new DownloadManager();
        public TransferManager transferManager = new TransferManager();

        public UiHub()
        {
            UpdateBase = new FlowLib.Events.FmdcEventHandler(UHub_UpdateBase);
        }

        void UHub_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) {}
        public void Hub_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            Hub hub = (Hub)sender;
            this.hub = hub;
            string str = string.Empty;

            switch (e.Action)
            {
                case Actions.TransferStarted:
                    if (e.Data is ITransfer)
                    {
                        ITransfer trans = (ITransfer)e.Data;
                        transferManager.StartTransfer(trans);
                    }
                    break;
                case Actions.TransferRequest:
                    if (e.Data is TransferRequest)
                    {
                        TransferRequest req = (TransferRequest)e.Data;
                        transferManager.AddTransferReq(req);
                    }
                    break;
                #region MainMessage
                case Actions.MainMessage:
                    if (e.Data is MainMessage)
                    {
                        MainMessage mainchat = (MainMessage)e.Data;
                        if (mainchat.From == null)
                            str = mainchat.Content;
                        else
                        {
                            User u = hub.GetUserById(mainchat.From);
                            if (u == null)
                                str = "<" + mainchat.From + "> " + mainchat.Content;
                            else
                                str = "<" + u.DisplayName + "> " + mainchat.Content;
                        }
                        // Test : Start
                        user = hub.GetUserById("DC++0.699");
                        //User user = hub.GetUserById("DCDM++0.0495");
                        if (user != null && hub.Share != null)
                        {
                            ContentInfo ci = new ContentInfo(user.ID + ".xml.bz2", FlowLib.Enums.ContentIdTypes.Filelist);
                            ci.SystemPath = @"C:\Private\FMDC\PiP\FlowLibDemo\ConsoleDemo\bin\Debug\FileLists\" + user.ID + ".xml.bz2";
                            DownloadItem di = new DownloadItem(ci);
                            downloadManager.AddDownload(di, new Source(null, user.ID));
                            downloadManager.DownloadCompleted += new FmdcEventHandler(DownloadManager_DownloadCompleted);
                            this.Hub_Update(null, new FmdcEventArgs(Actions.TransferRequest, new TransferRequest(user.ID, hub, user.UserInfo)));
                            //TransferManager.AddTransferReq(user.ID, hub, user.UserInfo);
                            hub.Send(new FlowLib.Protocols.HubNmdc.ConnectToMe(user.ID, hub.Share.Port, hub));
                        }
                        // Test : End
                    }
                    else
                        str = e.Data.ToString();
                    Console.WriteLine(str);
                    break;
                #endregion
                #region PrivateMessage
                case Actions.PrivateMessage:
                    PrivateMessage to = (PrivateMessage)e.Data;
                    Console.WriteLine("*** PM From: " + to.From + ", To: " + to.To + " " + to.Content);
                    break;
                #endregion
                #region RegMode
                case Actions.RegMode:
                    break;
                #endregion
                #region Status
                case Actions.StatusChange:
                    HubStatus status = (HubStatus)e.Data;
                    switch (status.Code)
                    {
                        case HubStatus.Codes.Disconnected:
                            Console.WriteLine("*** Hub Disconnected");
                            if (status.Exception != null)
                            {
#if DEBUG
                                if (status.Exception is System.Net.Sockets.SocketException)
                                {
                                    System.Net.Sockets.SocketException se = (System.Net.Sockets.SocketException)status.Exception;
                                    StringBuilder sb = new StringBuilder();
                                    sb.Append("\r\n");
                                    sb.Append("\tErrorCode: " + se.ErrorCode + "\r\n");
                                    sb.Append("\tMessage: " + se.Message + "\r\n");
                                    sb.Append("\tStackTrace: " + se.StackTrace);
                                    Console.WriteLine(sb.ToString());
                                }
#else
                                Console.WriteLine(" : " + status.Exception.Message);
                                    //richTextBox1.AppendText(status.Exception.StackTrace);
#endif
                            }
                            Console.WriteLine("\r\n");
                            break;
                        case HubStatus.Codes.Connected:
                            Console.WriteLine("*** Hub Connected");
                            break;
                        case HubStatus.Codes.Connecting:
                            Console.WriteLine("*** Hub Connecting");
                            break;
                    }
                    break;
                #endregion
                default:
                        Console.WriteLine(e.Data);
                        break;
            }
        }

        void DownloadManager_DownloadCompleted(object sender, FmdcEventArgs e)
        {
            DownloadItem di = sender as DownloadItem;
            if (di == null)
                return;
            if (di.ContentInfo.IsFilelist)
            {
                byte[] data = System.IO.File.ReadAllBytes(di.ContentInfo.SystemPath);
                bool isBzXmlList = ((di.ContentInfo.IdType | FlowLib.Enums.ContentIdTypes.FilelistXmlBz) == di.ContentInfo.IdType);

                FlowLib.Utils.FileLists.FilelistXmlBz2 filelist = new FlowLib.Utils.FileLists.FilelistXmlBz2(data, isBzXmlList);
                filelist.CreateShare();
                Share userShare = filelist.Share;

                if (user != null)
                {
                    foreach (System.Collections.Generic.KeyValuePair<string, ContentInfo> var in userShare)
                    {
                        var.Value.SystemPath = @"C:\Private\FMDC\PiP\FlowLibDemo\ConsoleDemo\bin\Debug\Download\" + var.Value.SystemPath;
                        DownloadItem di2 = new DownloadItem(var.Value);
                        // Uncomment below if you want to disable segment downloading.
                        //di2.SegmentSize = di2.ContentInfo.Size;
                        downloadManager.AddDownload(di2, new Source(null, user.ID));
                    }
                }
            }
        }
    }
}
