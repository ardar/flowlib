# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

# Code #

```

/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Enums;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;

namespace FlowLibApp
{
    public class SearchAndDownload : IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        TransferManager _transferManager = new TransferManager();
        DownloadManager _downloadManager = new DownloadManager();
        Hub _hubConnection;
        string _currentDir = System.AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar;
        HubSetting _setting;

        public SearchAndDownload()
        {
            UpdateBase += new FmdcEventHandler(OnUpdateBase);

            _setting = new HubSetting();
            _setting.Address = "127.0.0.1";
            _setting.Port = 441;
            _setting.DisplayName = "SearchAndDownload";
            _setting.Protocol = "Adc";

            _hubConnection = new Hub(_setting, this);
            _hubConnection.ProtocolChange += new FmdcEventHandler(_hubConnection_ProtocolChange);

            // We need to have a share
            _hubConnection.Share = new Share("temp");

            _hubConnection.Me.TagInfo.Mode = ConnectionTypes.Passive;
            _hubConnection.Me.TagInfo.Slots = 2;

            _hubConnection.Connect();

            while (true)
            {
                Thread.Sleep(100);
            }
        }

        protected void OnUpdateBase(object sender, FmdcEventArgs e) { }

        void _hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(Protocol_Update);
            }
        }

        void Protocol_Update(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                switch (e.Action)
                {
                    case Actions.IsReady:
                        bool isReady = (bool)e.Data;
                        if (isReady)
                        {
                            SearchInfo searchInfo = new SearchInfo();
                            searchInfo.Set(SearchInfo.SEARCH, "Ubuntu");
                            searchInfo.Set(SearchInfo.TYPE, "0");
                            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(Actions.Search, searchInfo));
                        }
                        break;
                    case Actions.TransferRequest:
                        if (e.Data is TransferRequest)
                        {
                            TransferRequest req = (TransferRequest)e.Data;
                            if (_transferManager.GetTransferReq(req.Key) == null)
                                _transferManager.AddTransferReq(req);
                        }
                        break;
                    case Actions.TransferStarted:
                        Transfer trans = e.Data as Transfer;
                        if (trans != null)
                        {
                            // We could add a TransferRequest here if we wanted.
                            _transferManager.StartTransfer(trans);
                            trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                            trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        }
                        break;
                    case Actions.SearchResult:
                        if (e.Data is SearchResultInfo)
                        {
                            SearchResultInfo srInfo = e.Data as SearchResultInfo;
                            if (srInfo != null)
                            {
                                ContentInfo info = srInfo.Info;
                                System.IO.FileInfo fi = new FileInfo(info.Get(ContentInfo.VIRTUAL));

                                string fileName = _currentDir + "SearchAndDownload" +
                                                  System.IO.Path.DirectorySeparatorChar + fi.Name;

                                // You should realy validate the "info.Get(ContentInfo.VIRTUAL)" before using it as it could be used to harm your computer.
                                // We are not doing it here as that would make the example bigger.
                                info.Set(ContentInfo.STORAGEPATH, fileName);

                                DownloadItem downloadItem = new DownloadItem(info);
                                // We can do this as we are only in one hub.
                                var usr = _hubConnection.GetUserById(srInfo.UserId);
                                if (usr != null)
                                {
                                    _downloadManager.AddDownload(downloadItem,
                                                                 new Source(_hubConnection.StoreId, usr.StoreID));
                                    AutoDownloadNewStuff();
                                }
                            }
                        }
                        break;
                }
            }
        }

        void AutoDownloadNewStuff()
        {
            if (_hubConnection == null)
            {
                return;
            }

            Source[] sourcesToIgnore = _transferManager.Transfers.Select(f => f.Value.Source).Distinct().ToArray();
            List<Source> sourcesWithDownloadItems = _downloadManager.SourceItems.Distinct().ToList();

            if (sourcesWithDownloadItems.Count > 0 && sourcesToIgnore.Length > 0)
            {
                return;
            }

            // We are already having a connection to this user, dont start a new one.
            foreach (Source src in sourcesToIgnore)
            {
                sourcesWithDownloadItems.Remove(src);
            }
            Source[] sourcesToStartDownloadingFrom = sourcesWithDownloadItems.ToArray();
            foreach (Source src in sourcesToStartDownloadingFrom)
            {
                // This should never happen as we are in 1 hub only
                if (!string.Equals(src.ConnectionId, _hubConnection.StoreId))
                    continue;

                User usr = _hubConnection.GetUserByStoredId(src.UserId);
                // Check to see if user is in hub or not.
                if (usr == null)
                    continue;

                // Start transfer to user
                UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
            }
        }

        void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            TransferRequest req = e.Data as TransferRequest;
            req = _transferManager.GetTransferReq(req.Key);
            if (trans != null && req != null)
            {
                e.Handled = true;
                e.Data = req;
                _transferManager.RemoveTransferReq(req.Key);
            }
        }

        void Protocol_ChangeDownloadItem(object sender, FmdcEventArgs e)
        {
            Transfer trans = sender as Transfer;
            if (trans == null)
                return;
            DownloadItem dwnItem = null;

            if (_downloadManager.TryGetDownload(trans.Source, out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }
    }
}

```