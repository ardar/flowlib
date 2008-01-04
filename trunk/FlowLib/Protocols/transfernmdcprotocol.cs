
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

using System.Threading;

using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Protocols.TransferNmdc;
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Managers;
using FlowLib.Enums;

namespace FlowLib.Protocols
{
    /// <summary>
    /// Transfer NMDC Protocol
    /// </summary>
    public class TransferNmdcProtocol : IProtocolTransfer
    {
        #region Variables
        protected ITransfer trans = null;
        protected string received = string.Empty;
        protected bool rawData = false;
        protected bool compressedZLib = false;
        protected bool download = true;
        protected Direction userDir = null;
        protected int downloadRandom = -1;
        protected byte[] bytesToSend = null;
        protected byte[] bytesReceived = null;
        protected int connectionStatus = -1;
        protected int protocolStatus = -1;
        protected Supports mySupport = null;
        protected Timer timer = null;

        string directory = string.Empty;
        /// <summary>
        /// Internal identification for Supports
        /// 
        /// $UGetZBlock needs both SupportGetZBlock and SupportXmlBZList.
        /// $UGetBlock needs SupportXmlBZList.
        /// $GetZBlock needs SupportGetZBlock.
        /// $ADCGET needs SupportADCGet.
        /// </summary>
        Supports userSupport = null;

        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
        public event FmdcEventHandler ChangeDownloadItem;
        public event FmdcEventHandler RequestTransfer;
        public event FmdcEventHandler Error;
        #endregion

        #region Properties
        public string Name
        {
            get { return "Nmdc"; }
        }
        public IConMessage KeepAliveCommand
        {
            get { return null; }
        }
        public IConMessage FirstCommand
        {
            get {
                // Yes i know that this is the second message.
                // Need todo this and then send MyNick from OnMessageToServer.
                if (trans != null && trans.Me != null)
                    return new Lock(trans);
                return null;
            }
        }
        public System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.Default; }
        }
        public string Seperator
        {
            get { return "|"; }
        }
        #endregion
        #region Constructor(s)
        public TransferNmdcProtocol(ITransfer trans)
            :this(trans, System.AppDomain.CurrentDomain.BaseDirectory)
        {

        }

        public TransferNmdcProtocol(ITransfer trans, string dir)
        {
            directory = dir;
            this.trans = trans;
            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
            ChangeDownloadItem = new FmdcEventHandler(OnChangeDownloadItem);
            RequestTransfer = new FmdcEventHandler(OnRequestTransfer);
            Error = new FmdcEventHandler(OnError);

            TimerCallback timerDelegate = new TimerCallback(OnTimer);
            long interval = 10 * 1000; // 10 seconds
            timer = new System.Threading.Timer(timerDelegate, trans, interval, interval);


            trans.ConnectionStatusChange += new FmdcEventHandler(trans_ConnectionStatusChange);
        }

        void OnError(object sender, FmdcEventArgs e) { }
        void OnRequestTransfer(object sender, FmdcEventArgs e) { }
        void OnChangeDownloadItem(object sender, FmdcEventArgs e) { }

        void OnTimer(object stateInfo)
        {
            //long interval = 30 * 1000 + trans.LastEventTimeStamp;  // 30 seconds
            long interval = 30 * 1000 * 10000 + trans.LastEventTimeStamp;  // 30 seconds
            // We are checking against socket != null. This is if we havnt connect.
            if (connectionStatus != TcpConnection.Disconnected && System.DateTime.Now.Ticks > interval)
            {
                FmdcEventArgs e = new FmdcEventArgs((int)TransferErrors.INACTIVITY);
                Error(this, e);
                if (!e.Handled)
                    trans.Disconnect("Inactivity");
            }
        }

        void trans_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            connectionStatus = e.Action;
            // TODO : Change status message for this connection here.
            switch (connectionStatus)
            {
                case TcpConnection.Connected: break;
                case TcpConnection.Connecting: break;
                case TcpConnection.Disconnected:
                    if (trans.DownloadItem != null && trans.CurrentSegment !=null)
                    {
                        // Clean up here please :)
                        trans.DownloadItem.Cancel(trans.CurrentSegment.Index);
                    }

                    if (e.Data is Utils.FmdcException)
                    {
                        // TODO : Send out error message here.
                    }
                    break;
            }
        }
        #endregion
        #region Parse
        public void ParseRaw(byte[] b, int length)
        {
            if (rawData)
            {
                if (length < 0)
                    throw new System.ArgumentOutOfRangeException("length has to be above zero");

                // Do we have content left that we need to convert?
                if (this.received.Length > 0)
                {
                    byte[] old = this.Encoding.GetBytes(this.received);
                    long size = (long)length + old.LongLength;

                    byte[] tmp = new byte[size];
                    System.Array.Copy(old, 0, tmp, 0, old.LongLength);
                    if (b != null)
                        System.Array.Copy(b, 0, tmp, old.LongLength, (long)length);
                    b = tmp;
                    length += old.Length;
                    received = string.Empty;
                }

                // Do we have a working byte array?
                if (b != null && length != 0)
                {
                    BinaryMessage conMsg = new BinaryMessage(trans, b, length);
                    // Plugin handling here
                    FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, conMsg);
                    MessageReceived(trans, e);
                    if (!e.Handled)
                    {
                        if (this.download)
                        {
                            if (trans.DownloadItem != null && trans.CurrentSegment.Index != -1)
                            {
                                if (trans.CurrentSegment.Length < length)
                                {
                                    trans.Disconnect("You are sending more then i want.. Why?!");
                                    return;
                                }

                                if (trans.CurrentSegment.Position == 0 && !Utils.FileOperations.PathExists(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH)))
                                {
                                    Utils.FileOperations.AllocateFile(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH), trans.DownloadItem.ContentInfo.Size);
                                }

                                // Create the file.
                                using (System.IO.FileStream fs = System.IO.File.OpenWrite(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH)))
                                {
                                    try
                                    {
                                        // Lock this segment of file
                                        fs.Lock(trans.CurrentSegment.Start, trans.CurrentSegment.Length);
                                        // Set position
                                        fs.Position = trans.CurrentSegment.Start + trans.CurrentSegment.Position;
                                        // Write this byte array to file
                                        fs.Write(b, 0, length);
                                        trans.CurrentSegment.Position += length;
                                    }
                                    catch (System.Exception exp)
                                    {
                                        System.Console.WriteLine("E:" + exp);
                                        trans.DownloadItem.Cancel(trans.CurrentSegment.Index);
                                    }
                                    finally
                                    {
                                        // Saves and unlocks file
                                        fs.Flush();
                                        fs.Unlock(trans.CurrentSegment.Start, trans.CurrentSegment.Length);
                                        fs.Dispose();
                                        fs.Close();
                                    }
                                    if (trans.CurrentSegment.Position >= trans.CurrentSegment.Length)
                                    {
                                        trans.DownloadItem.Finished(trans.CurrentSegment.Index);
                                        //// Searches for a download item and a segment id
                                        GetDownloadItem();
                                        // Request new segment from user. IF we have found one. ELSE disconnect.
                                        if (trans.DownloadItem != null && (trans.CurrentSegment = trans.DownloadItem.GetAvailable()).Index != -1)
                                        {
                                            OnDownload();
                                        }
                                        else
                                            trans.Disconnect("All content downloaded");
                                    }
                                }
                            }
                        }
                        else
                        {
                            trans.Disconnect("I dont want to download from you. Fuck off!");
                        }
                    }
                }
            }
            else
            {
                ParseRaw(this.Encoding.GetString(b, 0, length));
            }
        }
        public void ParseRaw(string raw)
        {
            // If raw lenght is 0. Ignore
            if (raw.Length == 0)
                return;

            // Should we read buffer?
            if (received.Length > 0)
            {
                raw = received + raw;
                received = string.Empty;
            }
            int pos = 0;
            // Loop through Commands.
            while ((pos = raw.IndexOf(Seperator)) > 0)
            {
                // We have received a command that tells us to read binary
                if (rawData)
                    break;

                pos++;
                TransferMessage msg = ParseMessage(raw.Substring(0, pos));
                raw = raw.Remove(0, pos);
                // Plugin handling here
                FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(trans, e);
                if (!e.Handled)
                    ActOnInMessage(msg);
                pos++;
            }
            // Right now we are not supporting changing of protocol directly. Else it should have been here.

            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                received = raw;
            if (rawData)
                ParseRaw(null, 0);

        }
        protected TransferMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            TransferMessage msg = new TransferMessage(trans, raw);
            switch (raw[0])
            {
                case '$':
                    int pos; 
                    string cmd = null;
                    if ((pos = raw.IndexOf(' ')) != -1)
                        cmd = raw.Substring(0, pos).ToLower();
                    else
                    {
                        if (raw.Length >= 10)
                            break;
                        cmd = raw.ToLower();
                    }
                    if (cmd == null || cmd.Equals(string.Empty))
                        break;
                    switch (cmd)
                    {
                        case "$mynick": msg = new MyNick(trans,raw); break;
                        case "$lock": msg = new Lock(trans, raw); break;
                        case "$supports": msg = new Supports(trans, raw); break;
                        case "$send": msg = new Send(trans, raw); break;
                        case "$key": msg = new Key(trans, raw); break;
                        case "$direction": msg = new Direction(trans, raw); break;
                        case "$get": msg = new Get(trans, raw); break;
                        case "$ugetblock": msg = new UGetBlock(trans, raw); break;
                        case "$ugetzblock": msg = new UGetZBlock(trans, raw); break;
                        case "$adcget": msg = new ADCGET(trans, raw); break;
                        case "$adcsnd": msg = new ADCSND(trans, raw); break;
                        case "$filelength": msg = new FileLength(trans, raw); break;
                        case "$sending": msg = new Sending(trans, raw); break;
                        case "$error": msg = new Error(trans, raw); break;
                        case "$getlistlen": msg = new GetListLen(trans, raw); break;
                        case "$maxedout": msg = new MaxedOut(trans, raw); break;
                        case "$failed": msg = new Failed(trans, raw); break;
                    }
                    break;
                default:
                    break;
                    // No command. Ignore.
            }
            return msg;
        }
        #endregion

        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(trans, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        public void GetDownloadItem()
        {
            // Get content
            trans.DownloadItem = null;
            DownloadItem dwnItem = null;
            UserInfo usrInfo = trans.User;
            if (usrInfo != null)
            {
                FmdcEventArgs eArgs = new FmdcEventArgs(0, dwnItem);
                ChangeDownloadItem(trans, eArgs);

                trans.DownloadItem = eArgs.Data as DownloadItem;
                if (trans.DownloadItem != null && (trans.CurrentSegment = trans.DownloadItem.GetAvailable()).Index != -1)
                    download = true;
                else
                    download = false;
            }
        }

        public void OnDownload()
        {
            // We havnt received direction yet.
            if (userDir == null)
                return;
            // We havnt sent direction yet.
            if (this.downloadRandom == -1)
                return;

            if (this.download)  // If we want to download. Check stuff.
            {
                if (userDir.Download && this.downloadRandom == userDir.Number)
                {
                    // Same value. Disconnect.
                    trans.Disconnect();
                }
                else if (userDir.Download && this.downloadRandom < userDir.Number)
                {
                    // Other client won battle. Wait for commands.
                    this.download = false;
                }
                else
                {
                    // We won battle. Start download.
                    if (trans.DownloadItem != null && trans.CurrentSegment.Index != -1)
                    {
                        // Set right content string
                        trans.Content = new ContentInfo(ContentInfo.REQUEST, trans.DownloadItem.ContentInfo.Get(ContentInfo.VIRTUAL));
                        if (trans.DownloadItem.ContentInfo.IsFilelist)
                        {
                            if (userSupport != null && userSupport.XmlBZList && mySupport.XmlBZList)
                            {
                                trans.Content.Set(ContentInfo.REQUEST, "files.xml.bz2");
                                trans.DownloadItem.ContentInfo.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.XMLBZ);
                            }
                            else if (userSupport != null && userSupport.BZList && mySupport.BZList)
                            {
                                trans.Content.Set(ContentInfo.REQUEST, "MyList.bz2");
                                trans.DownloadItem.ContentInfo.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.BZ);
                            }
                            else
                            {
                                trans.Content.Set(ContentInfo.REQUEST, "MyList.DcLst");
                                trans.DownloadItem.ContentInfo.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.HUFFMAN);
                            }
                        }

                        // Set that we are actually downloading stuff
                        if (trans.CurrentSegment.Index >= 0)
                        {
                            trans.DownloadItem.Start(trans.CurrentSegment.Index);
                        }
                        rawData = false;
                        compressedZLib = false;

                        /// $UGetZBlock needs both SupportGetZBlock and SupportXmlBZList.
                        /// $UGetBlock needs SupportXmlBZList.
                        /// $GetZBlock needs SupportGetZBlock.
                        /// $ADCGET needs SupportADCGet.
                        /// $Get needs nothing
                        if (userSupport != null && userSupport.ADCGet && mySupport.ADCGet)
                        {
                            if (userSupport.TTHF && mySupport.TTHF && trans.DownloadItem.ContentInfo.ContainsKey(ContentInfo.TTH))
                                trans.Content.Set(ContentInfo.REQUEST, "TTH/" + trans.DownloadItem.ContentInfo.Get(ContentInfo.TTH));

                            trans.Send(new ADCGET(trans, "file", trans.Content.Get(ContentInfo.REQUEST), trans.CurrentSegment.Start, trans.CurrentSegment.Length, compressedZLib));
                        }
                        else if (
                            (userSupport != null && userSupport.GetZBlock || userSupport.XmlBZList)
                            && (mySupport.GetZBlock || mySupport.XmlBZList))
                        {
                            if ((userSupport.GetZBlock && userSupport.XmlBZList) && (mySupport.GetZBlock && mySupport.XmlBZList))
                            {
                                trans.Send(new UGetZBlock(trans, trans.Content.Get(ContentInfo.REQUEST), trans.CurrentSegment.Start, trans.CurrentSegment.Length));
                                compressedZLib = true;
                            }
                            else if (userSupport.XmlBZList && mySupport.XmlBZList)
                            {
                                trans.Send(new UGetBlock(trans, trans.Content.Get(ContentInfo.REQUEST), trans.CurrentSegment.Start, trans.CurrentSegment.Length));
                            }
                            else
                            {
                                trans.Send(new GetZBlock(trans, trans.Content.Get(ContentInfo.REQUEST), trans.CurrentSegment.Start, trans.CurrentSegment.Length));
                                compressedZLib = true;
                            }
                        }
                        else
                        {
                            trans.Send(new Get(trans, trans.Content.Get(ContentInfo.REQUEST), trans.CurrentSegment.Start));
                        }
                    }
                }
            }
            else if (!userDir.Download)
            {
                // Do other user also want to upload?
                trans.Disconnect();
            }
            // We dont want to download. wait for commands.
        }

        public void ActOnInMessage(IConMessage conMsg)
        {
            TransferMessage message = (TransferMessage)conMsg;
            if (message is Lock)
            {
                Lock lk = (Lock)message;
                if (lk.Extended)
                    trans.Send(mySupport = new Supports(trans));

                GetDownloadItem();

                Direction dir = new Direction(trans, this.download);
                this.downloadRandom = dir.Number;
                trans.Send(dir);

                trans.Send(new Key(lk.Key, trans));
            }
            else if (message is Key)
            {
                OnDownload();
            }
            else if (message is MyNick)
            {
                MyNick myNick = (MyNick)message;
                if (trans.Me == null || trans.Share == null)
                {
                    this.trans.User = myNick.Info;
                    TransferRequest req = new TransferRequest(myNick.Info.ID, null, null);

                    FmdcEventArgs eArgs = new FmdcEventArgs(0, req);
                    RequestTransfer(this, eArgs);
                    req = eArgs.Data as TransferRequest;
                    if (!eArgs.Handled || req == null)
                    {
                        // Can't see user on my allow list
                        trans.Disconnect("No match for Request");
                        return;
                    }
                    trans.Me = req.Me;
                    trans.User = req.User;
                    trans.Share = req.Share;
                }
                else
                {
                    trans.User = myNick.Info;
                }
                trans.Send(new Lock(trans));
            }
            else if (message is Supports)
            {
                // Sets Supports for protocol.
                Supports sup = (Supports)message;
                userSupport = sup;
            }
            else if (message is Direction)
            {
                userDir = (Direction)message;
            }
            else if (message is Sending)
            {
                Sending sending = (Sending)message;
                if (trans.DownloadItem.ContentInfo.Size == -1)
                {
                    trans.DownloadItem.ContentInfo.Size = sending.Length;
                    trans.DownloadItem.SegmentSize = sending.Length;
                    trans.CurrentSegment = trans.DownloadItem.GetAvailable();
                }
                else if (trans.CurrentSegment.Length != sending.Length)
                {
                    trans.Disconnect("Why would i want to get a diffrent length of bytes then i asked for?");
                    return;
                }
                this.rawData = true;
            }
            else if (message is FileLength)
            {
                FileLength fileLength = (FileLength)message;
                if (trans.DownloadItem.ContentInfo.Size == -1)
                {
                    trans.DownloadItem.ContentInfo.Size = fileLength.Length;
                    trans.DownloadItem.SegmentSize = fileLength.Length;
                    trans.CurrentSegment = trans.DownloadItem.GetAvailable();
                }
                else if (trans.CurrentSegment.Length != fileLength.Length)
                {
                    trans.Disconnect("Why would i want to get a diffrent length of bytes then i asked for?");
                    return;
                }
                this.rawData = true;
            }
            else if (message is ADCSND)
            {
                ADCSND adcsnd = (ADCSND)message;
                if (!trans.Content.Get(ContentInfo.REQUEST).Equals(adcsnd.Content))
                {
                    trans.Disconnect("I want my bytes..");
                    return;
                }
                if (trans.DownloadItem.ContentInfo.Size == -1)
                {
                    trans.DownloadItem.ContentInfo.Size = adcsnd.Length;
                    trans.DownloadItem.SegmentSize = adcsnd.Length;
                    trans.CurrentSegment = trans.DownloadItem.GetAvailable();
                }
                else if (trans.CurrentSegment.Length != adcsnd.Length)
                {
                    trans.Disconnect("Why would i want to get a diffrent length of bytes then i asked for?");
                    return;
                }
                this.rawData = true;
            }
            else if (message is Send)
            {
                if (this.download || bytesToSend == null)
                {
                    trans.Disconnect();
                    return;
                }
                trans.CurrentSegment.Position = bytesToSend.Length;
                long length = trans.Content.Size;
                do
                {
                    trans.Send(new ConMessage(trans, bytesToSend));
                    trans.CurrentSegment.Position += bytesToSend.Length;
                } while (connectionStatus != TcpConnection.Disconnected && (bytesToSend = this.GetContent(System.Text.Encoding.ASCII, trans.CurrentSegment.Position, trans.CurrentSegment.Length - trans.CurrentSegment.Position)) != null);
                trans.Disconnect();
            }
            else if (message is Get)
            {
                // If we are supposed to download and other client tries to download. Disconnect.
                if (this.download)
                {
                    trans.Disconnect();
                    return;
                }
                Get get = (Get)message;

                trans.Content = new ContentInfo(ContentInfo.REQUEST, get.File);
                trans.CurrentSegment = new SegmentInfo(-1, get.Start, -1);
                if (get.File.Equals("files.xml.bz2"))
                {
                    //trans.Content.VirtualName = System.Text.Encoding.ASCII.WebName + get.File;
                    //trans.Content.IdType = ContentIdTypes.Filelist;
                    trans.Content.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.XMLBZ);
                    trans.Content.Set(ContentInfo.VIRTUAL, System.Text.Encoding.ASCII.WebName + get.File);
                }
                else
                {
                    //trans.Content.VirtualName = get.File;
                    trans.Content.Set(ContentInfo.VIRTUAL, get.File);
                }
                bytesToSend = this.GetContent(System.Text.Encoding.ASCII, trans.CurrentSegment.Position, trans.CurrentSegment.Length - -trans.CurrentSegment.Position);

                // Do file exist?
                if (trans.Content.Size > -1)
                {
                    trans.Send(new FileLength(trans, trans.Content.Size));
                }
                else
                {
                    trans.Send(new Error("File Not Available", trans));
                }
            }
            else if (message is GetBlocks)
            {
                // If we are supposed to download and other client tries to download. Disconnect.
                if (this.download)
                {
                    trans.Disconnect();
                    return;
                }
                GetBlocks getblocks = (GetBlocks)message;
                trans.Content = new Containers.ContentInfo(ContentInfo.REQUEST, getblocks.FileName);
                if (getblocks.FileName.Equals("files.xml.bz2"))
                {
                    //trans.Content.VirtualName = System.Text.Encoding.UTF8.WebName + getblocks.FileName;
                    //trans.Content.IdType = ContentIdTypes.Filelist;
                    trans.Content.Set(ContentInfo.VIRTUAL, System.Text.Encoding.UTF8.WebName + getblocks.FileName);
                    trans.Content.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.XMLBZ);
                }
                else
                {
                    trans.Content.Set(ContentInfo.VIRTUAL, getblocks.FileName);
                }

                trans.CurrentSegment = new SegmentInfo(-1, getblocks.Start, getblocks.Length);
                bool firstTime = true;
                while (connectionStatus != TcpConnection.Disconnected && (bytesToSend = GetContent(System.Text.Encoding.UTF8, trans.CurrentSegment.Position, trans.CurrentSegment.Length - trans.CurrentSegment.Position)) != null)
                {
                    if (firstTime)
                    {
                        trans.Send(new Sending(trans));
                        firstTime = false;
                    }
                    trans.Send(new ConMessage(trans, bytesToSend));
                    trans.CurrentSegment.Position += bytesToSend.Length;
                }
                if (firstTime)
                    trans.Send(new Failed("File Not Available", trans));
            }
            else if (message is ADCGET)
            {
                // If we are supposed to download and other client tries to download. Disconnect.
                if (this.download)
                {
                    trans.Disconnect();
                    return;
                }
                ADCGET adcget = (ADCGET)message;
                trans.Content = new ContentInfo(ContentInfo.REQUEST, adcget.Content);
                switch (adcget.Type.ToLower())
                {
                    case "file":
                        if (adcget.Content.StartsWith("TTH/"))
                        {
                            //trans.Content.Id = adcget.Content.Substring(4);
                            //trans.Content.IdType = ContentIdTypes.TTH | ContentIdTypes.Hash;
                            trans.Content.Set(ContentInfo.TTH, adcget.Content.Substring(4));
                        }
                        else
                        {
                            if (adcget.Content.Equals("files.xml.bz2"))
                            {
                                //trans.Content.VirtualName = System.Text.Encoding.UTF8.WebName + adcget.Content;
                                //trans.Content.IdType = ContentIdTypes.Filelist;
                                trans.Content.Set(ContentInfo.VIRTUAL, System.Text.Encoding.UTF8.WebName + adcget.Content);
                                trans.Content.Set(ContentInfo.FILELIST, Utils.FileLists.BaseFilelist.XMLBZ);
                            }
                            else
                            {
                                trans.Content.Set(ContentInfo.VIRTUAL, adcget.Content);
                            }
                        }
                        break;
                }
                try
                {
                    trans.CurrentSegment = new SegmentInfo(-1, adcget.Start, adcget.Length);
                    bool firstTime = true;
                    // TODO : ZLib compression here doesnt work as we want. It takes much memory and much cpu
                    //Util.Compression.ZLib zlib = null;
                    //if (adcget.ZL1)
                    //    zlib = new Fmdc.Util.Compression.ZLib();
                    while (connectionStatus != TcpConnection.Disconnected && (bytesToSend = GetContent(System.Text.Encoding.UTF8, trans.CurrentSegment.Position, trans.CurrentSegment.Length - -trans.CurrentSegment.Position)) != null)
                    {
                        if (firstTime)
                        {
                            ADCSND adcsend = new ADCSND(trans);
                            adcsend.Type = adcget.Type;
                            adcsend.Content = adcget.Content;
                            adcsend.Start = adcget.Start;
                            adcsend.Length = trans.Content.Size;
                            adcsend.ZL1 = adcget.ZL1;
                            trans.Send(adcsend);

                            firstTime = false;
                        }

                        trans.CurrentSegment.Position += bytesToSend.Length;
                        // We want to compress content with ZLib
                        //if (zlib != null)
                        //{
                        //    zlib.Compress2(bytesToSend);
                        //    bytesToSend = zlib.Read();
                        //}
                        trans.Send(new ConMessage(trans, bytesToSend));
                        bytesToSend = null;

                    }

                    // If we compressing data with zlib. We need to send ending bytes too.
                    //if (zlib != null && connectionStatus != Connection.Disconnected)
                    //    trans.Send(new ConMessage(trans, zlib.close()));
                    if (firstTime)
                    {
                        // We should not get here if file is in share.
                        trans.Send(new Error("File Not Available", trans));
                        trans.Disconnect();
                    }
                }
                catch (System.Exception e) { System.Console.WriteLine("ERROR:" + e); }
            }
            else if (message is MaxedOut)
            {
                FmdcEventArgs e = new FmdcEventArgs((int)TransferErrors.NO_FREE_SLOTS);
                Error(this, e);
                if (!e.Handled)
                    trans.Disconnect();
            }
            else if (message is Error)
            {
                TransferNmdc.Error error = (TransferNmdc.Error)message;
                FmdcEventArgs e = null;
                // TODO : Add more error messages here.
                switch (error.Message)
                {
                    case "File Not Available":
                        e = new FmdcEventArgs((int)TransferErrors.FILE_NOT_AVAILABLE);
                        break;
                    default:
                        e = new FmdcEventArgs((int)TransferErrors.UNKNOWN, error.Message);
                        break;
                }
                Error(this, e);
                if (!e.Handled)
                    trans.Disconnect();
            }
            else if (message is Failed)
            {
                Failed failed = (Failed)message;
                FmdcEventArgs e = null;
                // TODO : Add more error messages here.
                switch (failed.Message)
                {
                    case "File Not Available":
                        e = new FmdcEventArgs((int)TransferErrors.FILE_NOT_AVAILABLE);
                        break;
                    default:
                        e = new FmdcEventArgs((int)TransferErrors.UNKNOWN, failed.Message);
                        break;
                }
                Error(this, e);
                if (!e.Handled)
                    trans.Disconnect();
            }
        }

        protected byte[] GetContent(System.Text.Encoding encoding, long start, long length)
        {
            Share share = trans.Share;
            Containers.ContentInfo info = trans.Content;
            if (share != null && share.ContainsContent(ref info))
            {
                trans.Content = info;
                if (length == -1 && start == 0)
                    length = trans.Content.Size;
                return share.GetContent(trans.Content, start, length);
            }
            return null;
        }

        public void ActOnOutMessage(FmdcEventArgs e)
        {

        }
        #region Event(s)
        protected void OnMessageReceived(object sender, FmdcEventArgs e)
        {
            trans.LastEventTimeStamp = System.DateTime.Now.Ticks;
        }
        protected void OnMessageToSend(object sender, FmdcEventArgs e)
        {
            trans.LastEventTimeStamp = System.DateTime.Now.Ticks;
            if (e.Data is TransferMessage)
            {
                TransferMessage msg = (TransferMessage)e.Data;
                if (e.Data is Lock)
                {
                    // As we can just set one cmd to be sent as first command,
                    // we need to send the second one to buffer from here.
                    trans.Send(new MyNick(trans, trans.Me));
                }
            }
        }
        #endregion
    }
}
