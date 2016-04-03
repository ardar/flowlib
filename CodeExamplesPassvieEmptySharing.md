# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

First of all we need to create a Share.
Yes, this needs to be done even if we are not sharing anything.
Secondly we need to add filelist(s) to share.
If we are not doing this everyone will get a message saying that our filelist is not there.
Thirdly we need to add this newly created share to our hub.
Last thing that needs to be done is letting us know when a transfer is beeing created so we can start it and store it somewhere.
We are doing this by listening to the update event in hub.

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

namespace ConsoleDemo.Examples
{
    public class PassiveEmptySharing
    {
        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();
        //string currentDir = @"C:\Temp\";
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

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
            setting.Protocol = "Auto";

            Hub hubConnection = new Hub(setting);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);
            // Adds share to hub
            hubConnection.Share = share;
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
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, currentDir + "MyFileLists\\");
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
```