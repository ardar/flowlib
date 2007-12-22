using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

namespace ConsoleDemo.Examples
{
    public class PassiveEmptySharing
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();

        public PassiveEmptySharing()
        {
            // Creates a empty share
            Share share = new Share("Testing");
            // Adds common filelist to share
            AddFilelistsToShare(share);

            HubSetting setting = new HubSetting();
            setting.Address = "127.0.0.1";
            setting.Port = 411;
            setting.DisplayName = "FlowLib";

            Hub hubConnection = new Hub(setting);
            // Adds share to hub
            hubConnection.Share = share;
            hubConnection.Protocol = new HubNmdcProtocol(hubConnection);
            hubConnection.Update += new FmdcEventHandler(hubConnection_Update);
            hubConnection.Connect();
        }

        void AddFilelistsToShare(Share s)
        {
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, @"C:\Private\FMDC\PiP\FlowLibDemo\ConsoleDemo\bin\Debug\MyFileLists\");
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
                    }
                    break;
            }
        }
    }
}
