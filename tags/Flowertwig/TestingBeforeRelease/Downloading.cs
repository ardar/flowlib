using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLib.Interfaces;
using FlowLib.Managers;
using TestingBeforeRelease.Utils;
using FlowLib.Connections;
using System.Threading;
using FlowLib.Entities;
using Flowertwig.Utils.Events;
using Flowertwig.Utils.Entities;
using FlowLib.Filelists;
using FlowLib.Connections.Entities;
using FlowLib.Connections.Interfaces;

namespace TestingBeforeRelease
{
    [TestClass]
    public class Downloading : IBaseUpdater
    {
        private const long BIGSIZE = 524288000;
        private const long SMALLSIZE = 104857600;

        public event Flowertwig.Utils.Events.EventHandler UpdateBase;

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
        private bool _gotBigContent;
        private bool _gotSmallContent;
        private bool _hasValidRequestTransfer;
		private Client _clientConnection;

        [TestMethod]
        public void Downloading_In_ActiveMode_Nmdc()
        {
            HubTest("Nmdc", ConnectionTypes.Direct);

			// Did we find user?
			if (!_hasFoundUser)
				throw new AssertFailedException("Unable to find a user to download from");
			// Did we get filelist?
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
				if (!_hasBigContent && !_hasSmallContent)
				{
					throw new AssertFailedException("No matching files found in filelist");
				}
				else
				{
					if (!_hasBigContent)
						throw new AssertFailedException("No big file (Over " + Downloading.BIGSIZE + " bytes) found in filelist");
					if (!_hasSmallContent)
						throw new AssertFailedException("No small file (Less then " + Downloading.SMALLSIZE + " bytes) found in filelist");
				}

				// TODO: Why where we unable to get any files?

				if (downloadManager.SourceItems.Count == 0)
					throw new AssertFailedException("Unable to get any files, we have no sources in DownloadManager");
				else
				{
					Source firstSrc = downloadManager.SourceItems[0];
					if (firstSrc == null)
						throw new AssertFailedException("Unable to get any files, a source in DownloadManager is null");
					else if (firstSrc.ConnectionId == null)
					{
						throw new AssertFailedException("Unable to get any files, connectionId in first Source is null");
					}
					else if (firstSrc.UserId == null)
					{
						throw new AssertFailedException("Unable to get any files, userStoreId in first Source is null");
					}
				}

				int transferCount = transferManager.Transfers.Count;
				if (transferCount == 1)
				{
					throw new AssertFailedException("Unable to get any files, a transfer is still open");
				}

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
		public void Downloading_In_PassiveMode_Nmdc()
		{
			HubTest("Nmdc", ConnectionTypes.Passive);
			// Did we find user?
			if (!_hasFoundUser)
				throw new AssertFailedException("Unable to find a active user to download from");
			// Did we get filelist?
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
				if (!_hasBigContent && !_hasSmallContent)
				{
					throw new AssertFailedException("No matching files found in filelist");
				}
				else
				{
					if (!_hasBigContent)
						throw new AssertFailedException("No big file (Over " + Downloading.BIGSIZE + " bytes) found in filelist");
					if (!_hasSmallContent)
						throw new AssertFailedException("No small file (Less then " + Downloading.SMALLSIZE + " bytes) found in filelist");
				}

				// TODO: Why where we unable to get any files?

				if (downloadManager.SourceItems.Count == 0)
					throw new AssertFailedException("Unable to get any files, we have no sources in DownloadManager");
				else
				{
					Source firstSrc = downloadManager.SourceItems[0];
					if (firstSrc == null)
						throw new AssertFailedException("Unable to get any files, a source in DownloadManager is null");
					else if (firstSrc.ConnectionId == null)
					{
						throw new AssertFailedException("Unable to get any files, connectionId in first Source is null");
					}
					else if (firstSrc.UserId == null)
					{
						throw new AssertFailedException("Unable to get any files, userStoreId in first Source is null");
					}
				}

				int transferCount = transferManager.Transfers.Count;
				if (transferCount == 1)
				{
					throw new AssertFailedException("Unable to get any files, a transfer is still open");
				}

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

			// Did we find user?
			if (!_hasFoundUser)
				throw new AssertFailedException("Unable to find a active user to download from");
			// Did we get filelist?
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
				if (!_hasBigContent && !_hasSmallContent)
				{
					throw new AssertFailedException("No matching files found in filelist");
				}
				else
				{
					if (!_hasBigContent)
						throw new AssertFailedException("No big file (Over " + Downloading.BIGSIZE + " bytes) found in filelist");
					if (!_hasSmallContent)
						throw new AssertFailedException("No small file (Less then " + Downloading.SMALLSIZE + " bytes) found in filelist");
				}

				// TODO: Why where we unable to get any files?

				if (downloadManager.SourceItems.Count == 0)
					throw new AssertFailedException("Unable to get any files, we have no sources in DownloadManager");
				else
				{
					Source firstSrc = downloadManager.SourceItems[0];
					if (firstSrc == null)
						throw new AssertFailedException("Unable to get any files, a source in DownloadManager is null");
					else if (firstSrc.ConnectionId == null)
					{
						throw new AssertFailedException("Unable to get any files, connectionId in first Source is null");
					}
					else if (firstSrc.UserId == null)
					{
						throw new AssertFailedException("Unable to get any files, userStoreId in first Source is null");
					}
				}

				int transferCount = transferManager.Transfers.Count;
				if (transferCount == 1)
				{
					throw new AssertFailedException("Unable to get any files, a transfer is still open");
				}

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
        public void Downloading_In_PassiveMode_Adcs()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2781;
            HubTest("AdcSecure", ConnectionTypes.Passive);

			// Did we find user?
			if (!_hasFoundUser)
				throw new AssertFailedException("Unable to find a active user to download from");
			// Did we get filelist?
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
				if (!_hasBigContent && !_hasSmallContent)
				{
					throw new AssertFailedException("No matching files found in filelist");
				}
				else
				{
					if (!_hasBigContent)
						throw new AssertFailedException("No big file (Over " + Downloading.BIGSIZE + " bytes) found in filelist");
					if (!_hasSmallContent)
						throw new AssertFailedException("No small file (Less then " + Downloading.SMALLSIZE + " bytes) found in filelist");
				}

				// TODO: Why where we unable to get any files?

				if (downloadManager.SourceItems.Count == 0)
					throw new AssertFailedException("Unable to get any files, we have no sources in DownloadManager");
				else
				{
					Source firstSrc = downloadManager.SourceItems[0];
					if (firstSrc == null)
						throw new AssertFailedException("Unable to get any files, a source in DownloadManager is null");
					else if (firstSrc.ConnectionId == null)
					{
						throw new AssertFailedException("Unable to get any files, connectionId in first Source is null");
					}
					else if (firstSrc.UserId == null)
					{
						throw new AssertFailedException("Unable to get any files, userStoreId in first Source is null");
					}
				}

				int transferCount = transferManager.Transfers.Count;
				if (transferCount == 1)
				{
					throw new AssertFailedException("Unable to get any files, a transfer is still open");
				}

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

        private void HubTest(string protocol, ConnectionTypes connectionType)
        {
            _settings.Protocol = protocol;

            _clientConnection = new Client(_settings, this);
            _clientConnection.ProtocolChange += new Flowertwig.Utils.Events.EventHandler(hubConnection_ProtocolChange);

            // We need to have a share
			_clientConnection.Share = new Share("temp");

			_clientConnection.Me.TagInfo.Mode = connectionType;

			_clientConnection.Connect();

			//Thread thread = new Thread(new ThreadStart(AutoDownloadNewStuff));
			//thread.IsBackground = true;
			//thread.Start();

            int i = 0;
            while (!_isFinished && i++ < _testTimeoutLength)
            {
                Thread.Sleep(500);
            }

            // Close all open threads
			//thread.Abort();
            _clientConnection.Disconnect("Test time exceeded");
            _clientConnection.Dispose();
        }

        void AutoDownloadNewStuff()
        {
			if (_clientConnection == null)
				return;

			Source[] sourcesToIgnore = transferManager.Transfers.Select(f => f.Value.Source).Distinct().ToArray();
			List<Source> sourcesWithDownloadItems = downloadManager.SourceItems.Distinct().ToList();

			if (sourcesWithDownloadItems.Count > 0 && sourcesToIgnore.Length > 0)
			{
				return;
			}

			foreach (Source src in sourcesToIgnore)
			{
				sourcesWithDownloadItems.Remove(src);
			}
			// This doesn't seem todo what we want.
			//Source[] sourcesToStartDownloadingFrom = sourcesWithDownloadItems.Except(sourcesToIgnore).ToArray();
			Source[] sourcesToStartDownloadingFrom = sourcesWithDownloadItems.ToArray();
			foreach (Source src in sourcesToStartDownloadingFrom)
			{
				// This should never happen as we are in 1 hub only
				if (!string.Equals(src.ConnectionId, _clientConnection.StoreId))
					continue;

				User usr = _clientConnection.GetUserByStoredId(src.UserId);
				// Check to see if user is in hub or not.
				if (usr == null)
					continue;

				_testTimeoutLength += 60;
				// Start transfer to user
                UpdateBase(this, new DefaultEventArgs(Actions.StartTransfer, usr));
			}
        }

        void downloadManager_DownloadCompleted(object sender, DefaultEventArgs e)
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
				downloadManager.AddDownload(new DownloadItem(bigContentItem), new Source(dwnItem.ContentInfo.Get("HubStoreId"), dwnItem.ContentInfo.Get(UserInfo.STOREID)));
                _hasBigContent = true;

                if (smallContentItem == null)
                    return;
                // Add SMALL file for a user
                smallContentItem.Set(ContentInfo.STORAGEPATH, currentDir + "Download" + System.IO.Path.DirectorySeparatorChar + "small" + ".file");
				downloadManager.AddDownload(new DownloadItem(smallContentItem), new Source(dwnItem.ContentInfo.Get("HubStoreId"), dwnItem.ContentInfo.Get(UserInfo.STOREID)));
                _hasSmallContent = true;

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

			if (_gotFilelist && _gotBigContent && _gotSmallContent)
				_isFinished = true;
        }

        void hubConnection_ProtocolChange(object sender, DefaultEventArgs e)
        {
            Client clientConnection = sender as Client;
            if (clientConnection != null)
            {
                clientConnection.Protocol.Update += new Flowertwig.Utils.Events.EventHandler(prot_Update);
            }
        }

        void prot_Update(object sender, DefaultEventArgs e)
        {
            Client clientConnection = sender as Client;
            if (clientConnection != null)
            {
                switch (e.Action)
                {
                    case FlowLib.Connections.Protocols.BaseTransferProtocol.IsReady:
                        bool isReady = (bool)e.Data;
                        if (isReady)
                        {
                            // TODO: Start testing
                            var usrlst = clientConnection.Userlist;
                            User usr = null;
                            // Find usable user.
                            foreach (KeyValuePair<string, User> item in usrlst)
                            {
								UserInfo me = clientConnection.Me;
								usr = item.Value;

								// Check so we are not trying to download from our self.
								if (me.ID == usr.ID)
									continue;

                                if (me.TagInfo.Mode == ConnectionTypes.Passive)
                                {
                                    if (usr.Tag.Mode == ConnectionTypes.Direct)
                                    {
                                        _hasFoundUser = true;
                                        break;
                                    }
                                }
                                else
                                {
                                    if (usr.Tag.Mode == ConnectionTypes.Passive)
                                    {
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
                                info.Set("HubStoreId", clientConnection.StoreId);
                                info.Set(UserInfo.STOREID, usr.StoreID);

                                downloadManager.AddDownload(new DownloadItem(info), new Source(clientConnection.StoreId, usr.StoreID));

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
                            trans.Protocol.ChangeDownloadItem += new Flowertwig.Utils.Events.EventHandler(Protocol_ChangeDownloadItem);
                            trans.Protocol.RequestTransfer += new Flowertwig.Utils.Events.EventHandler(Protocol_RequestTransfer);
                        }
                        break;
                }
            }
        }


        void Protocol_RequestTransfer(object sender, DefaultEventArgs e)
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

        void Protocol_ChangeDownloadItem(object sender, DefaultEventArgs e)
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

        void downloadManager_SourceAdded(object sender, DefaultEventArgs e)
		{
			// New Source added. Make sure we will try to start new transfer(s) to all none used soures.
			AutoDownloadNewStuff();
		}

        void downloadManager_DownloadAdded(object sender, DefaultEventArgs e)
		{
			// New download added. Make sure we will try to start new transfer(s) to all none used soures.
			AutoDownloadNewStuff();
		}

        void Downloading_In_PassiveMode_UpdateBase(object sender, DefaultEventArgs e) { }

        [TestInitialize()]
        public void Init()
        {
            Application.InitilizeAll();

            _testTimeoutLength = 20;

            UpdateBase = new Flowertwig.Utils.Events.EventHandler(Downloading_In_PassiveMode_UpdateBase);

            downloadManager.DownloadCompleted += new Flowertwig.Utils.Events.EventHandler(downloadManager_DownloadCompleted);
			downloadManager.DownloadAdded += new Flowertwig.Utils.Events.EventHandler(downloadManager_DownloadAdded);
			downloadManager.SourceAdded += new Flowertwig.Utils.Events.EventHandler(downloadManager_SourceAdded);
			downloadManager.SegmentCompleted += new Flowertwig.Utils.Events.EventHandler(downloadManager_SegmentCompleted);

            _settings = new HubSetting();
            _settings.Address = "127.0.0.1";
            _settings.Port = 411;
            _settings.DisplayName = "FlowLib";
            _settings.Password = "Password";
        }

        void downloadManager_SegmentCompleted(object sender, DefaultEventArgs e)
		{
			//DownloadItem di = sender as DownloadItem;
			//if (di == null)
			//	return;

			//Console.WriteLine(string.Format("SEG: {0}", di.ContentInfo.Get(ContentInfo.STORAGEPATH)));
			_testTimeoutLength += 4;
		}

        [TestCleanup()]
        public void CleanUp()
        {
            _isFinished = false;
            _hasFoundUser = false;
            _gotFilelist = false;

			downloadManager.Clear();
			transferManager.Clear();
			
			downloadManager.DownloadCompleted -= downloadManager_DownloadCompleted;
			downloadManager.DownloadAdded -= downloadManager_DownloadAdded;
			downloadManager.SourceAdded -= downloadManager_SourceAdded;
			downloadManager.SegmentCompleted -= downloadManager_SegmentCompleted;

            // Remove filelist from filesystem
            //if (System.IO.File.Exists(_filelistPath))
            //{
            //    System.IO.File.Delete(_filelistPath);
            //    _filelistPath = null;
            //}
        }
    }
}
