using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLib.Containers;
using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Managers;
using TestingBeforeRelease.Utils;
using FlowLib.Connections;
using System.Threading;
using FlowLib.Containers.Security;
using FlowLib.Enums;
using FlowLib.Utils.FileLists;

namespace TestingBeforeRelease
{
    [TestClass]
    public class Downloading : IBaseUpdater
    {
        private const long BIGSIZE = 524288000;
        private const long SMALLSIZE = 104857600;

        public event FmdcEventHandler UpdateBase;

        TransferManager transferManager = new TransferManager();
        DownloadManager downloadManager = new DownloadManager();

        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory + System.IO.Path.DirectorySeparatorChar;

        private bool _isFinished;
        private HubSetting _settings;
        private bool _hasFoundUser;
        private string _filelistPath;
        private int _testTimeoutLength = 20;
        private bool _gotFilelist;
        private bool _hasTransferRequest;
        private bool _hasTransferStarted;
        private bool _hasRequestTransfer;
        private bool _hasChangeDownloadItem;
        private bool _hasBigContent;
        private bool _hasSmallContent;
        private bool _allowNewDownload = true;
        private bool _gotBigContent;
        private bool _gotSmallContent;
        private bool _hasValidRequestTransfer;

        [TestMethod]
        public void Downloading_In_PassiveMode_Nmdc()
        {
            HubTest("Nmdc", ConnectionTypes.Passive);

            if (!_hasFoundUser)
                throw new AssertFailedException("Unable to find a active user to download from");

            if (!_gotFilelist)
            {
                if (!_hasTransferStarted)
                {
                    throw new AssertFailedException("Unable to start connection to user");
                }
                if (!_hasRequestTransfer)
                {
                    throw new AssertFailedException("User connection failed before validating user");
                }
                // Not valid for old style in PASSIVE mode.
                //if (!_hasValidRequestTransfer)
                //{
                //    throw new AssertFailedException("Unable to validate user.");
                //}
                throw new AssertFailedException("Unable to get filelist");
            }

            if (!_gotBigContent && !_gotSmallContent)
            {
                // TODO: 
                throw new AssertFailedException("Unable to get any files");
            }
            else
            {
                if (!_gotBigContent)
                    throw new AssertFailedException("Unable to get BIG file");
                if (!_gotSmallContent)
                    throw new AssertFailedException("Unable to get SMALL file");
            }
        }

        [TestMethod]
        public void Downloading_In_PassiveMode_Adc()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2780;
            HubTest("Adc", ConnectionTypes.Passive);

            if (!_hasFoundUser)
                throw new AssertFailedException("Unable to find a active user to download from");

            if (!_gotFilelist)
            {
                if (!_hasTransferStarted)
                {
                    throw new AssertFailedException("Unable to start connection to user");
                }
                if (!_hasRequestTransfer)
                {
                    throw new AssertFailedException("User connection failed before validating user");
                }
                // Not valid for old style in PASSIVE mode.
                //if (!_hasValidRequestTransfer)
                //{
                //    throw new AssertFailedException("Unable to validate user.");
                //}
                throw new AssertFailedException("Unable to get filelist");
            }

            if (!_hasSmallContent && !_hasBigContent)
            {
                // TODO: 
                throw new AssertFailedException("Unable to get any files");
            }
            else
            {
                if (!_hasBigContent)
                    throw new AssertFailedException("Unable to get BIG file");
                if (!_hasSmallContent)
                    throw new AssertFailedException("Unable to get SMALL file");
            }
        }

        [TestMethod]
        public void Downloading_In_PassiveMode_Adcs()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2781;
            HubTest("AdcSecure", ConnectionTypes.Passive);

            if (!_hasFoundUser)
                throw new AssertFailedException("Unable to find a active user to download from");

            if (!_gotFilelist)
            {
                if (!_hasTransferStarted)
                {
                    throw new AssertFailedException("Unable to start connection to user");
                }
                if (!_hasRequestTransfer)
                {
                    throw new AssertFailedException("User connection failed before validating user");
                }
                // Not valid for old style in PASSIVE mode.
                //if (!_hasValidRequestTransfer)
                //{
                //    throw new AssertFailedException("Unable to validate user.");
                //}
                throw new AssertFailedException("Unable to get filelist");
            }

            if (!_hasSmallContent && !_hasBigContent)
            {
                throw new AssertFailedException("Unable to get any files");
            }
            else
            {
                if (!_hasBigContent)
                    throw new AssertFailedException("Unable to get BIG file");
                if (!_hasSmallContent)
                    throw new AssertFailedException("Unable to get SMALL file");
            }
        }

        private void HubTest(string protocol, ConnectionTypes connectionType)
        {
            _settings.Protocol = protocol;

            Hub hubConnection = new Hub(_settings, this);
            hubConnection.ProtocolChange += new FmdcEventHandler(hubConnection_ProtocolChange);

            // We need to have a share
            hubConnection.Share = new Share("temp");

            hubConnection.Me.TagInfo.Mode = connectionType;

            hubConnection.Connect();

            Thread thread = new Thread(new ParameterizedThreadStart(AutoDownloadNewStuff));
            thread.IsBackground = true;
            thread.Start(hubConnection);

            int i = 0;
            while (!_isFinished && i++ < _testTimeoutLength)
            {
                Thread.Sleep(500);
            }

            // Close all open threads
            thread.Abort();
            hubConnection.Disconnect("Test time exceeded");
            hubConnection.Dispose();
        }

        void AutoDownloadNewStuff(object obj)
        {
            try
            {
                Hub hubConnection = obj as Hub;
                if (hubConnection == null)
                    return;

                while (true)
                {
                    if (_allowNewDownload)
                    {
                        IList<Source> sources = downloadManager.SourceItems;
                        var users = hubConnection.Userlist;
                        User usr;
                        foreach (Source src in sources)
                    	{
                            // Make sure source is for this hub.
                            if (string.Equals(hubConnection.StoreId, src.ConnectionId))
                            {
                                usr = users.FirstOrDefault(f => string.Equals(f.Value.StoreID, src.UserId)).Value;
                                _testTimeoutLength += 60;
                                // Start transfer to user
                                UpdateBase(this, new FmdcEventArgs(Actions.StartTransfer, usr));
                                _allowNewDownload = false;
                                break;
                            }
                    	}
                    }
                    Thread.Sleep(100);
                }
            }
            catch (ThreadStateException tse)
            {
                // This test has been finished
            }
        }

        void downloadManager_DownloadCompleted(object sender, FmdcEventArgs e)
        {
            DownloadItem dwnItem = sender as DownloadItem;
            if (dwnItem == null)
                return;

            if (dwnItem.ContentInfo.IsFilelist)
            {
                _gotFilelist = true;
                // Read filelist
                byte[] data = System.IO.File.ReadAllBytes(dwnItem.ContentInfo.Get(ContentInfo.STORAGEPATH));
                BaseFilelist filelist = null;
                switch (dwnItem.ContentInfo.Get(ContentInfo.FILELIST))
                {
                    case BaseFilelist.XMLBZ:
                        filelist = new FilelistXmlBz2(data, true);
                        break;
                    case BaseFilelist.XML:
                        filelist = new FilelistXmlBz2(data, false);
                        break;
                }
                _testTimeoutLength += 10;
                filelist.CreateShare();
                IShare share = filelist.Share;

                // Select one file that is more then 500 MiB of size
                var bigContentItem = share.Where(f => f.Value.Size > BIGSIZE).Select(k => k.Value).FirstOrDefault();
                // Select one file that is less then 100 MiB of size
                var smallContentItem = share.Where(f => f.Value.Size < SMALLSIZE).Select(k => k.Value).FirstOrDefault();

                if (bigContentItem == null)
                    return;
                // Add BIG file for a user
                bigContentItem.Set(ContentInfo.STORAGEPATH, currentDir + "Download" + System.IO.Path.DirectorySeparatorChar + "big" + ".file");
                downloadManager.AddDownload(new DownloadItem(bigContentItem), new Source(bigContentItem.Get("HubStoreId"), bigContentItem.Get(UserInfo.STOREID)));
                _hasBigContent = true;

                if (smallContentItem == null)
                    return;
                // Add SMALL file for a user
                smallContentItem.Set(ContentInfo.STORAGEPATH, currentDir + "Download" + System.IO.Path.DirectorySeparatorChar + "small" + ".file");
                downloadManager.AddDownload(new DownloadItem(smallContentItem), new Source(smallContentItem.Get("HubStoreId"), bigContentItem.Get(UserInfo.STOREID)));
                _hasSmallContent = true;

                _allowNewDownload = true;
                _testTimeoutLength += 30;
            }
            else
            {
                // Handle file
                if (dwnItem.ContentInfo.Size > BIGSIZE)
                {
                    _gotBigContent = true;
                }
                else if (dwnItem.ContentInfo.Size < SMALLSIZE)
                {
                    _gotSmallContent = true;
                }
            }
        }

        void hubConnection_ProtocolChange(object sender, FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            if (hubConnection != null)
            {
                hubConnection.Protocol.Update += new FmdcEventHandler(prot_Update);
            }
        }

        void prot_Update(object sender, FmdcEventArgs e)
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
                            // TODO: Start testing
                            var usrlst = hubConnection.Userlist;
                            User usr = null;
                            // Find usable user.
                            foreach (KeyValuePair<string, User> item in usrlst)
                            {
                                if (hubConnection.Me.TagInfo.Mode == FlowLib.Enums.ConnectionTypes.Passive)
                                {
                                    if (item.Value.Tag.Mode == ConnectionTypes.Direct)
                                    {
                                        usr = item.Value;
                                        _hasFoundUser = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (item.Value.Tag.Mode == ConnectionTypes.Passive)
                                    {
                                        usr = item.Value;
                                        _hasFoundUser = true;
                                        break;
                                    }
                                }
                            }

                            if (_hasFoundUser)
                            {
                                // Add filelist for a user
                                ContentInfo info = new ContentInfo(ContentInfo.FILELIST, BaseFilelist.UNKNOWN);

                                _filelistPath = currentDir + "Filelists" + System.IO.Path.DirectorySeparatorChar + "downloading" + ".filelist";
                                info.Set(ContentInfo.STORAGEPATH, _filelistPath);
                                info.Set("HubStoreId", hubConnection.StoreId);
                                info.Set(UserInfo.STOREID, usr.StoreID);

                                downloadManager.AddDownload(new DownloadItem(info), new Source(hubConnection.StoreId, usr.StoreID));

                                _testTimeoutLength += 10;
                            }
                        }
                        break;
                    case Actions.TransferRequest:
                        if (e.Data is TransferRequest)
                        {
                            _hasTransferRequest = true;
                            TransferRequest req = (TransferRequest)e.Data;
                            if (transferManager.GetTransferReq(req.Key) == null)
                                transferManager.AddTransferReq(req);
                        }
                        break;
                    case Actions.TransferStarted:
                        Transfer trans = e.Data as Transfer;
                        _hasTransferStarted = true;
                        if (trans != null)
                        {
                            // We could add a TransferRequest here if we wanted.
                            transferManager.StartTransfer(trans);
                            trans.Protocol.ChangeDownloadItem += new FmdcEventHandler(Protocol_ChangeDownloadItem);
                            trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        }
                        break;
                }
            }
        }

        void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            TransferRequest req = e.Data as TransferRequest;
            _hasRequestTransfer = true;
            req = transferManager.GetTransferReq(req.Key);
            if (trans != null && req != null)
            {
                _hasValidRequestTransfer = true;
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
            _hasChangeDownloadItem = true;

            if (downloadManager.TryGetDownload(trans.Source, out dwnItem))
            {
                e.Data = dwnItem;
                e.Handled = true;
            }
        }

        void Downloading_In_PassiveMode_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) { }

        [TestInitialize()]
        public void Init()
        {
            Application.InitilizeAll();

            _testTimeoutLength = 20;

            UpdateBase = new FlowLib.Events.FmdcEventHandler(Downloading_In_PassiveMode_UpdateBase);

            downloadManager.DownloadCompleted += new FmdcEventHandler(downloadManager_DownloadCompleted);

            _settings = new HubSetting();
            _settings.Address = "127.0.0.1";
            _settings.Port = 411;
            _settings.DisplayName = "FlowLib";
            _settings.Password = "Password";
        }

        [TestCleanup()]
        public void CleanUp()
        {
            _isFinished = false;
            _hasFoundUser = false;
            _gotFilelist = false;

            downloadManager.DownloadCompleted -= downloadManager_DownloadCompleted;

            // Remove filelist from filesystem
            if (System.IO.File.Exists(_filelistPath))
            {
                System.IO.File.Delete(_filelistPath);
                _filelistPath = null;
            }
        }
    }
}
