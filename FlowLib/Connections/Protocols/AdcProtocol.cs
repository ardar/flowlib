using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using Flowertwig.Utils.Connections;
using Flowertwig.Utils.Connections.Entities;
using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Entities;
using Flowertwig.Utils.Enums;
using Flowertwig.Utils.Events;
using Flowertwig.Utils.Exceptions;
using Flowertwig.Utils.Hashing;
using Flowertwig.Utils.IO;
using FlowLib.Connections.Entities;
using FlowLib.Connections.Interfaces;
using FlowLib.Connections.Protocols.Adc.Commands;
using FlowLib.Connections.Protocols.Nmdc;
using FlowLib.Entities;
using FlowLib.Filelists;
using FlowLib.Interfaces;

namespace FlowLib.Connections.Protocols
{
    /// <summary>
    /// ADC Protocol
    /// </summary>
    public class AdcProtocol : BaseTransferProtocol, IProtocolHub, IProtocolTransfer
    {
        protected enum ParsningActions: int
        {
            None = 0,
            ChangeToBinary,
            ChangeToNmdc
        }

        #region Variables
        // Variables to remember
        protected string gpaString = "";        // GPA Random data
        protected SUP supports;          // Current support
        protected bool hasSentSUP = false;      // have we sent SUP?
        protected UserInfo info = new UserInfo();     // Hub/User Info (Name and description and so on).
        protected IDirectConnectConnection con;       // Current Connection where this protocol is used
        protected Hub hub;               // Current hub where this protocol is used
        protected string received = "";
        protected bool download = true;
        protected int connectionStatus = TcpConnection.Disconnected;
        protected bool firstMsg = true;
        //protected bool rawData = false;
        protected bool disposed = false;
        protected Encoding currentEncoding;

        // INF updating
        protected long infLastUpdated = 0;
        protected INF lastInf = null;
        protected Timer updateInfTimer;

        protected static string yourtranssupports = "ADBAS0 ADBASE ADBZIP";
        protected static string yoursupports = "ADBAS0 ADBASE ADTIGR";
        //protected static string yoursupports = "ADBASE RMBAS0 ADTIGR";

        public event EventHandler ChangeDownloadItem;
        public event EventHandler RequestTransfer;
        public event EventHandler Error;

        public event EventHandler MessageReceived;
        public event EventHandler MessageToSend;
        public event EventHandler Update;
        #endregion
        #region Properties
        protected bool _rawData = false;
        public bool IsRawData
        {
            get
            {
                //System.Console.WriteLine("GET: " + _rawData.ToString());
                return _rawData;
            }
            set
            {
                if (_rawData != value)
                {
                    System.Console.WriteLine("IsRawData: " + value.ToString());
                }
                _rawData = value;
            }
        }
        protected string _name = "Adc";
        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }
        public UserInfo Info
        {
            get { return info; }
            set { info = value; }
        }

        protected bool _isReady;
        public bool IsReady
        {
            get { return _isReady; }
            set
            {
                _isReady = value;
                Update(con, new DefaultEventArgs(BaseTransferProtocol.IsReady, value));
            }
        }

        public static string TransferSupport
        {
            get { return yourtranssupports; }
        }

        public static string Support
        {
            get { return yoursupports; }
        }
        public IConMessage KeepAliveCommand
        {
            get { return new StrMessage(con, Seperator); }
        }
        public IConMessage FirstCommand
        {
            get
            {
                return new SUP(con);
            }
        }
        public System.Text.Encoding Encoding
        {
            get
            {
                if (currentEncoding == null)
                {
                    currentEncoding = System.Text.Encoding.UTF8;
                }
                return currentEncoding;
            }
            set
            {
                currentEncoding = value;
            }
        }
        public string Seperator
        {
            get { return "\n"; }
        }

        public bool IsDisposed
        {
            get { return disposed; }
        }
        #endregion
        #region Constructor(s)
        public AdcProtocol()
        {
            MessageReceived = new EventHandler(OnMessageReceived);
            MessageToSend = new EventHandler(OnMessageToSend);
            Update = new EventHandler(OnUpdate);
            ChangeDownloadItem = new EventHandler(AdcProtocol_ChangeDownloadItem);
            RequestTransfer = new EventHandler(AdcProtocol_RequestTransfer);
            Error = new EventHandler(AdcProtocol_Error);

            TimerCallback updateCallback = OnUpdateInf;
            updateInfTimer = new Timer(updateCallback, this, Timeout.Infinite, Timeout.Infinite);
        }

        void AdcProtocol_Error(object sender, DefaultEventArgs e) { }
        void AdcProtocol_RequestTransfer(object sender, DefaultEventArgs e) { }
        void AdcProtocol_ChangeDownloadItem(object sender, DefaultEventArgs e) { }

        protected AdcProtocol(IDirectConnectConnection con)
            :this()
        {
            this.con = con;

            //con.ShouldBlockOnSend = true;
        }

        public AdcProtocol(Transfer trans)
            : this((ITransfer)trans)
        {
            this.trans = trans;
            this.trans.ConnectionStatusChange += new EventHandler(trans_ConnectionStatusChange);
        }

        public AdcProtocol(Hub hub)
            : this((IDirectConnectConnection)hub)
        {
            this.hub = hub;
            this.hub.ConnectionStatusChange += new EventHandler(hub_ConnectionStatusChange);
            if (hub.Share != null)
                hub.Share.LastModifiedChanged += new EventHandler(Share_LastModifiedChanged);
            Hub.RegModeUpdated += new EventHandler(Hub_RegModeUpdated);
        }

        public AdcProtocol(Hub hub, bool isSecure)
            : this(hub)
        {
            // We are doing this to keep a good HubSettings
            if (isSecure)
                Name = Name + "Secure";
        }
        #endregion

        public void Dispose()
        {
            if (!disposed)
            {
                updateInfTimer.Dispose();
                updateInfTimer = null;
                if (trans != null)
                {
                    trans.ConnectionStatusChange -= trans_ConnectionStatusChange;
                }
                trans = null;
                if (hub != null)
                {
                    hub.ConnectionStatusChange -= hub_ConnectionStatusChange;
                    if (hub.Share != null)
                        hub.Share.LastModifiedChanged -= Share_LastModifiedChanged;
                    Hub.RegModeUpdated -= Hub_RegModeUpdated;
                    hub = null;
                }
                con = null;
                disposed = true;
            }
        }

        void Share_LastModifiedChanged(object sender, DefaultEventArgs e)
        {
            if (!e.Handled)
            {
                UpdateInf();
            }
        }

        void Hub_RegModeUpdated(object sender, DefaultEventArgs e)
        {
            if (sender != hub && !e.Handled && hub.RegMode >= 0)
            {
                UpdateInf();
            }
        }

        protected void OnUpdateInf(System.Object stateInfo)
        {
            UpdateInf(false);
        }

        protected void UpdateInf() { UpdateInf(true); }
        protected virtual void UpdateInf(bool firstTime)
        {
            if (new System.DateTime(infLastUpdated).AddMinutes(5) < System.DateTime.Now)
            {
                INF tmp = new INF(hub, hub.Me);
                if (lastInf == null || (tmp.Raw != lastInf.Raw))
                {
                    INF computed = INF.MakeInfFromDifference(tmp, lastInf);
                    lastInf = tmp;
                    hub.Send(computed);
                    infLastUpdated = System.DateTime.Now.Ticks;
                    updateInfTimer.Change(Timeout.Infinite, Timeout.Infinite);
                }
            }
            else if (firstTime)
            {
                updateInfTimer.Change(0, 15 * 60 * 1000 + 10);
            }
        }

        void trans_ConnectionStatusChange(object sender, DefaultEventArgs e)
        {
            connectionStatus = e.Action;
            // TODO : Change status message for this connection here.
            switch (connectionStatus)
            {
                case TcpConnection.Connected: break;
                case TcpConnection.Connecting: break;
                case TcpConnection.Disconnected:
                    System.Console.WriteLine("OnDisconnected");
                    EnsureCurrentSegmentCancelation();

                    if (e.Data is FlowertwigException)
                    {
                        // TODO : Send out error message here.
                    }
                    break;
            }
        }

        void hub_ConnectionStatusChange(object sender, DefaultEventArgs e)
        {
            HubStatus h;
            switch (e.Action)
            {
                case TcpConnection.Disconnected:
                    if (e.Data is System.Exception)
                        h = new HubStatus(HubStatus.Codes.Disconnected, (System.Exception)e.Data);
                    else
                        h = new HubStatus(HubStatus.Codes.Disconnected);
                    hub.RegMode = -1;
                    // Sets Inf Interval to 0 when connection is disconnected
                    this.infLastUpdated = 0;
                    lastInf = null;
                    break;
                case TcpConnection.Connected:
                    h = new HubStatus(HubStatus.Codes.Connected);
                    break;
                case TcpConnection.Connecting:
                default:
                    h = new HubStatus(HubStatus.Codes.Connecting);
                    break;
            }
            Update(con, new DefaultEventArgs(Actions.StatusChange, h));
            hub.Userlist.Clear();
        }
        #region Functions
        #region Convert
        /// <summary>
        /// Replaces "\\s" with " " and so on
        /// </summary>
        /// <param name="content">String to convert to right format</param>
        /// <returns>coverted string</returns>
        public static string ConvertIncomming(string content)
        {
            content = content.Replace("\\\\", "\\");
            content = content.Replace("\\s", " ");
            content = content.Replace("\\n", "\n");
            return content;
        }
        /// <summary>
        /// Replaces " " with "\\s" and so on
        /// </summary>
        /// <param name="content">String to convert to right format</param>
        /// <returns>coverted string</returns>
        public static string ConvertOutgoing(string content)
        {
            content = content.Replace("\\", "\\\\");
            content = content.Replace(" ", "\\s");
            content = content.Replace("\n", "\\n");
            return content;
        }
        #endregion
        public bool OnSend(IConMessage msg)
        {
            System.Console.WriteLine("OUT:" + msg.ToString());
            DefaultEventArgs e = new DefaultEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(con, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        #region Parse
        public void ParseRaw(byte[] b, int length)
        {
            // We may have data on our stack even when we have disconnected the transfer.
            if (this.connectionStatus == TcpConnection.Disconnected)
            {
                System.Console.WriteLine("ParseRaw:disconnected");
                return;
            }

            if (IsRawData)
            {
                System.Console.WriteLine("Bytes:" + length);


                if (length < 0)
                    throw new System.ArgumentOutOfRangeException("length has to be above zero");

                // Do we have content left that we need to convert?
                if (this.received.Length > 0)
                {
                    System.Console.WriteLine("Length:" + this.received.Length);

                    byte[] old = this.Encoding.GetBytes(this.received);
                    this.received = string.Empty;
#if !COMPACT_FRAMEWORK
                    long size = (long)length + old.LongLength;

                    byte[] tmp = new byte[size];
                    System.Console.WriteLine("add old to extended array");
                    System.Array.Copy(old, 0, tmp, 0, old.LongLength);
                    if (b != null)
                    {
                        System.Console.WriteLine("add new to extended array");
                        System.Array.Copy(b, 0, tmp, old.LongLength, (long) length);
                    }
#else
                    int size = length + old.Length;

                    byte[] tmp = new byte[size];
                    System.Array.Copy(old, 0, tmp, 0, old.Length);
                    if (b != null)
                        System.Array.Copy(b, 0, tmp, old.Length, length);
#endif
                    b = tmp;
                    length = tmp.Length;

                    tmp = null;
                    old = null;
                }

                // Do we have a working byte array?
                if (b != null && length != 0)
                {
                    BinaryMessage conMsg = new BinaryMessage(con, b, length);
                    // Plugin handling here

                    DefaultEventArgs e = new DefaultEventArgs(Actions.CommandIncomming, conMsg);
                    MessageReceived(con, e);
                    if (!e.Handled)
                    {
                        if (this.download && trans != null)
                        {
                            if (trans.DownloadItem != null && trans.CurrentSegment != null && trans.CurrentSegment.Index != -1)
                            {
                                if (trans.CurrentSegment.Length < length)
                                {
                                    trans.Disconnect("You are sending more then i want.. Why?!");
                                    return;
                                }

                                if (trans.CurrentSegment.Position == 0 && !FileOperations.PathExists(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH)))
                                {
                                    FileOperations.AllocateFile(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH), trans.DownloadItem.ContentInfo.Size);
                                }

                                // Create the file.
                                SegmentInfo curInfo = trans.CurrentSegment;
                                try
                                {
                                    FileOperations.WriteContent(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH), ref curInfo, b, length);
                                    trans.CurrentSegment.Position += length;
                                }
                                catch (System.Exception exp)
                                {
                                    //trans.DownloadItem.Cancel(trans.CurrentSegment.Index, trans.Source);
                                    trans.Disconnect("Exception thrown when trying to write to file: " + exp.ToString());
                                    return;
                                }
                                curInfo = null;

                                if (trans.CurrentSegment.Position >= trans.CurrentSegment.Length)
                                {
                                    // Set raw to false as we have now got all our bytes
                                    IsRawData = false;

                                    EnsureCurrentSegmentFinishing();
                                    //// Searches for a download item and a segment id
                                    // Request new segment from user. IF we have found one. ELSE disconnect.
                                    if (GetSegment(true))
                                    {
                                        OnDownload();
                                    }
                                    else
                                        trans.Disconnect("All content downloaded");
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
                ParsningActions value = ParseRaw(Encoding.GetString(b, 0, length));
                switch (value)
                {
                    case ParsningActions.None:
                        break;
                    case ParsningActions.ChangeToBinary:
                        ParseRaw(null, 0);
                        break;
                    case ParsningActions.ChangeToNmdc:
                        if (trans != null)
                        {
                            ITransfer tmpTrans = trans;
                            trans.Protocol = new TransferNmdcProtocol(trans);
                            tmpTrans.Protocol.ParseRaw(b, length);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="raw"></param>
        /// <returns></returns>
        protected ParsningActions ParseRaw(string raw)
        {
            string raw2 = raw;
            // If raw lenght is 0. Ignore
            if (raw.Length == 0)
                return  ParsningActions.None;

            // Should we read buffer?
            if (received.Length > 0)
            {
                raw = received + raw;
                received = string.Empty;
            }
            int pos = 0;

            // If wrong Protocol type has been set. change it to Nmdc
            if (firstMsg && raw.StartsWith("$"))
            {
                if (trans != null)
                {
                    return ParsningActions.ChangeToNmdc;
                }
                else if (hub != null)
                {
                    Hub tmpHub = hub;
                    hub.Protocol = new HubNmdcProtocol(hub);
                    tmpHub.Reconnect();
                }
                //return
            }
            firstMsg = false;

            // Loop through Commands.
            while ((pos = raw.IndexOf(Seperator)) != -1 && this.connectionStatus != TcpConnection.Disconnected)
            {
                pos++;

                StrMessage msg = ParseMessage(raw.Substring(0, pos));
                //if (!msg.IsValid)
                //{
                //System.Console.WriteLine(raw.Substring(0, pos));
                //}
                //else
                //{
                //System.Console.WriteLine(msg.Raw);
                //}

                raw = raw.Remove(0, pos);
                // Plugin handling here
                DefaultEventArgs e = new DefaultEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(con, e);
                if (!e.Handled && msg.IsValid)
                    ActOnInMessage(msg);

                // We have received a command that tells us to read binary
                if (IsRawData)
                    break;
            }


            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                received = raw;
            if (IsRawData)
                return ParsningActions.ChangeToBinary;
            return ParsningActions.None;
        }
        protected StrMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            AdcBaseMessage msg = new AdcBaseMessage(con, raw);
            switch (msg.Action)
            {
                case "SUP":
                    msg = new SUP(con, raw); break;
                case "SID":
                    msg = new SID(con, raw); break;
                case "MSG":
                    msg = new MSG(con, raw); break;
                case "INF":
                    msg = new INF(con, raw); break;
                case "STA":
                    msg = new STA(con, raw); break;
                case "QUI":
                    msg = new QUI(con, raw); break;
                case "GPA":
                    msg = new GPA(con, raw); break;
                case "CTM":
                    msg = new CTM(con, raw); break;
                case "SND":
                    msg = new SND(con, raw); break;
                case "GFI":
                    msg = new GFI(con, raw); break;
                case "GET":
                    msg = new GET(con, raw); break;
                case "RCM":
                    msg = new RCM(con, raw); break;
                case "SCH":
                    msg = new SCH(con, raw); break;
                case "RES":
                    msg = new RES(con, raw); break;
                case "PAS":
                    msg = new PAS(con, raw); break;
            }
            return msg;
        }
        #endregion
        #region Act On
        protected void ActOnINF(INF inf)
        {
            if (hub != null && inf.Type.Equals("I"))
            {
                if (hub.RegMode < 0)
                    hub.RegMode = 0;
                UpdateInf();
                Info = inf.UserInfo;
                if (hub != null && Info.Description == null)
                    Update(con, new DefaultEventArgs(Actions.Name, new HubName(Info.DisplayName)));
                else if (hub != null)
                    Update(con, new DefaultEventArgs(Actions.Name, new HubName(Info.DisplayName, Info.Description)));
            }
            else if (trans != null && inf.Type.Equals("C"))
            {
                string token = null;
                // CINF IDE3VACJVAXNOQLGRFQS7D5HYH4A6GLZDO3LJ33HQ TO2718662518
                if (trans.Me != null && trans.Me.ContainsKey("TO"))
                    token = trans.Me.Get("TO");
                else if (inf.UserInfo.ContainsKey("TO"))
                    token = inf.UserInfo.Get("TO");

                TransferRequest req = new TransferRequest(token, null, inf.UserInfo);
                DefaultEventArgs eArgs = new DefaultEventArgs(0, req);
                RequestTransfer(trans, eArgs);
                req = eArgs.Data as TransferRequest;
                if (!eArgs.Handled || req == null)
                {
                    // Can't see user/connection on my allow list
                    trans.Disconnect("No match for Request");
                    return;
                }
                if (!((req.User.ContainsKey(UserInfo.CID) && inf.UserInfo.ContainsKey(UserInfo.CID)) && req.User.Get(UserInfo.CID).Equals(inf.UserInfo.Get(UserInfo.CID))))
                {
                    // For some reason user is trying to tell us it is a diffrent user. We dont like that.
                    DefaultEventArgs e = new DefaultEventArgs((int)TransferErrors.USERID_MISMATCH);
                    Error(trans, e);
                    if (!e.Handled)
                    {
                        trans.Disconnect("User Id Mismatch");
                        return;
                    }
                }
                if (trans.Me == null)
                    trans.Me = req.Me;
                trans.User = req.User;
                info = trans.User;
                trans.Share = req.Share;
                trans.Source = req.Source;
                download = req.Download;

                con.Send(new INF(con, trans.Me));
                if (download)
                {
                    EnsureCurrentSegmentCancelation();
                    // Request new segment from user. IF we have found one. ELSE disconnect.
                    if (GetSegment(true))
                    {
                        OnDownload();
                    }
                    else
                        trans.Disconnect("All content downloaded");
                }
            }
            else if (hub != null)
            {
                User usr = null;
                if ((usr = hub.GetUserById(inf.Id)) == null)
                {
                    if (inf.UserInfo.Mode == ConnectionTypes.Unknown)
                    {
                        inf.UserInfo.Mode = ConnectionTypes.Passive;
                    }
                    Update(con, new DefaultEventArgs(Actions.UserOnline, inf.UserInfo));
                }
                else
                {
                    usr.UserInfo = inf.UserInfo;
                    Update(con, new DefaultEventArgs(Actions.UserInfoChange, usr.UserInfo));
                }
                // This is so we update our own reg/op hub count.
                if (string.Equals(hub.Me.ID, inf.Id))
                {
                    // Should we be marked with key?
                    bool regmodeChanged = false;
                    if (hub.RegMode < 2)
                    {
                        if (((UserInfo.ACCOUNT_FLAG_OPERATOR & inf.UserInfo.Account) == UserInfo.ACCOUNT_FLAG_OPERATOR))
                        {
                            hub.RegMode = 2;
                            regmodeChanged = true;
                        }
                        else if (((UserInfo.ACCOUNT_FLAG_SUPERUSER & inf.UserInfo.Account) == UserInfo.ACCOUNT_FLAG_SUPERUSER))
                        {
                            hub.RegMode = 2;
                            regmodeChanged = true;
                        }
                        else if (((UserInfo.ACCOUNT_FLAG_HUBOWNER & inf.UserInfo.Account) == UserInfo.ACCOUNT_FLAG_HUBOWNER))
                        {
                            hub.RegMode = 2;
                            regmodeChanged = true;
                        }
                    }
                    // Should we be marked as reg?
                    if (hub.RegMode < 1)
                    {
                        if (((UserInfo.ACCOUNT_FLAG_REGISTERED & inf.UserInfo.Account) == UserInfo.ACCOUNT_FLAG_REGISTERED))
                        {
                            hub.RegMode = 1;
                            regmodeChanged = true;
                        }

                    }
                    if (regmodeChanged)
                        UpdateInf();

                    IsReady = true;
                }
            }
        }
        protected  void ActOnMSG(MSG msg)
        {
            if (msg.PmGroup == null)
            {
                MainMessage main = new MainMessage(msg.From, msg.Content);
                Update(con, new DefaultEventArgs(Actions.MainMessage, main));
            }
            else
            {
                PrivateMessage pm = new PrivateMessage(msg.To, msg.From, msg.Content, msg.PmGroup);
                Update(con, new DefaultEventArgs(Actions.PrivateMessage, pm));
            }
        }
        protected  void ActOnSID(SID sid)
        {
            hub.Me.Set(UserInfo.SID, sid.Id);
        }
        protected  void ActOnSTA(STA sta)
        {
            if (hub != null)
            {
                MainMessage main = new MainMessage(info.ID, sta.Content);
                Update(con, new DefaultEventArgs(Actions.MainMessage, main));
            }
        }
        protected  void ActOnGPA(GPA gpa)
        {
            this.gpaString = gpa.RandomData;
            if (trans != null)
            {
                Update(con, new DefaultEventArgs(Actions.Password, null));
            }
            if (hub != null && hub.HubSetting.Password.Length == 0)
            {
                Update(con, new DefaultEventArgs(Actions.Password, null));
            }
            else
                hub.Send(new PAS(hub, gpa.RandomData, hub.HubSetting.Password));
        }
        protected  void ActOnQUI(QUI qui)
        {
            User usr = null;
            if ((usr = hub.GetUserById(qui.Id)) != null)
            {
                Update(con, new DefaultEventArgs(Actions.UserOffline, usr.UserInfo));
                if (usr.ID == hub.Me.ID)
                {
                    // TODO : Banning and redirect handling
                    hub.Disconnect();
                    // Redirect
                    if (!string.IsNullOrEmpty(qui.Address))
                        Update(con, new DefaultEventArgs(Actions.Redirect, new RedirectInfo(qui.Address, qui.Message, qui.DisconnectedBy)));
                        // Banned
                    else
                    {
                        if (qui.Time != -1)
                            // Sets reconnect attempt to infinite
                            hub.KeepAliveInterval = 0;
                        else
                            hub.KeepAliveInterval = qui.Time;
                        Update(con, new DefaultEventArgs(Actions.Banned, new BannedInfo(qui.Time, qui.Message, qui.DisconnectedBy)));
                    }
                }
            }
        }
        protected  void ActOnSUP(SUP sup)
        {
            if (trans != null && !hasSentSUP)
            {
                con.Send(new SUP(con));
            }
            supports = sup;
            // TODO : We should really care about what hub support.
            if (!supports.BASE && !supports.TIGR)
            {
                // We will just simply disconnect if hub doesnt support this right now
                con.Disconnect("Connection doesnt support BASE or BAS0");
            }
#if !COMPACT_FRAMEWORK
            // Encrypted transfers
            if (supports.ADCS)
            {
                if (
                    (hub != null && hub.Me.ContainsKey(UserInfo.SECURE)) ||
                    (trans != null && trans.Me.ContainsKey(UserInfo.SECURE))
                    )
                {
                    con.SecureProtocol = SecurityProtocols.TLS;
                }
            }
#endif
        }
        protected  void  ActOnRES(RES res)
        {
            SearchResultInfo srinfo = new SearchResultInfo(res.Info, res.Id, res.Token);
            if (hub != null)
                Update(con, new DefaultEventArgs(Actions.SearchResult, srinfo));
        }
        protected  void  ActOnSCH(SCH sch)
        {
            UserInfo usr = null;
            if (hub != null)
            {
                User u = hub.GetUserById(sch.Id);
                if (u != null)
                    usr = u.UserInfo;
            }
            else if (trans != null)
                usr = trans.User;

            SendRES(sch.Info, usr);
        }
        public void ActOnInMessage(IConMessage conMsg)
        {
            StrMessage message = (StrMessage)conMsg;
            System.Console.WriteLine("IN:" + message.Raw);

            #region INF
            if (message is INF)
            {
                INF inf = (INF)message;
                ActOnINF(inf);
            }
                #endregion
                #region MSG
            else if (message is MSG && hub != null)
            {
                MSG msg = (MSG)message;
                ActOnMSG(msg);
            }
                #endregion
                #region SID
            else if (message is SID && hub != null)
            {
                SID sid = (SID)message;
                ActOnSID(sid);
            }
                #endregion
                #region STA
            else if (message is STA)
            {
                STA sta = (STA)message;
                ActOnSTA(sta);
            }
                #endregion
                #region GPA
            else if (message is GPA)
            {
                GPA gpa = (GPA)message;
                ActOnGPA(gpa);
            }
                #endregion
                #region QUI
            else if (message is QUI && hub != null)
            {
                QUI qui = (QUI)message;
                ActOnQUI(qui);
            }
                #endregion
                #region SUP
            else if (message is SUP)
            {
                SUP sup = (SUP) message;
                ActOnSUP(sup);
            }
                #endregion
                #region RES
            else if (message is RES)
            {
                RES res = (RES)message;
                ActOnRES(res);
            }
                #endregion
                #region SCH
            else if (message is SCH)
            {
                SCH sch = (SCH)message;
                ActOnSCH(sch);
            }
                #endregion
                #region CTM
            else if (message is CTM && hub != null)
            {
                CTM ctm = (CTM)message;

                // We really hate buggy hubsofts. Only reason we will get this message is because hubsoft dont know diffrent between E and D messages.
                if (ctm.Id == hub.Me.ID)
                    return;

                User usr = null;
                string addr = null;

                // Do we support same protocol?
                double version = 0.0;
                if (ctm.Protocol != null && (ctm.Protocol.StartsWith("ADC/") || ctm.Protocol.StartsWith("ADCS/")))
                {
                    try
                    {
                        version = double.Parse(ctm.Protocol.Substring( ctm.Protocol.IndexOf("/") +1), CultureInfo.GetCultureInfo("en-GB").NumberFormat);
                    }
                    catch { }
                }
                if (version > 1.0)
                {
                    hub.Send(new STA(hub, ctm.Id, hub.Me.ID, "241", "Protocol is not supported. I only support ADC 1.0/ADCS 0.10 and prior", "TO" + ctm.Token + " PR" + ctm.Protocol));
                    return;
                }

                if ((usr = hub.GetUserById(ctm.Id)) != null && usr.UserInfo.ContainsKey(UserInfo.IP))
                {
                    addr = usr.UserInfo.Get(UserInfo.IP);
                    if (string.Equals("213.89.21.14", addr))
                    {
                        addr = "192.168.1.4";
                    }

                    Transfer trans = new Transfer(addr, ctm.Port);
                    trans.Share = hub.Share;
                    // We are doing this because we want to filter out PID and so on.
                    User me = hub.GetUserById(hub.Me.ID);
                    trans.Me = new UserInfo(me.UserInfo);
                    trans.Protocol = new AdcProtocol(trans);
#if !COMPACT_FRAMEWORK
                    if (ctm.Secure)
                        trans.SecureProtocol = SecurityProtocols.TLS;
#endif

                    // Support for prior versions of adc then 1.0
                    string token = ctm.Token;
                    if (version < 1.0 && ctm.Token.StartsWith("TO"))
                        token = ctm.Token.Substring(2);
                    trans.Me.Set("TO", token);

                    Update(con, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(token, hub, usr.UserInfo,false)));
                    Update(con, new DefaultEventArgs(Actions.TransferStarted, trans));
                }
            }
                #endregion
                #region RCM
            else if (message is RCM && hub != null)
            {
                var rcm = (RCM)message;

                // We really hate buggy hubsofts. Only reason we will get this message is because hubsoft dont know diffrent between E and D messages.
                if (rcm.Id == hub.Me.ID)
                    return;

                if (hub.Me.Mode != ConnectionTypes.Passive && hub.Share != null)
                {
                    User usr = null;
                    if ((usr = hub.GetUserById(rcm.Id)) != null)
                    {
                        // Do we support same protocol?
                        double version = 0.0;
                        if (rcm.Protocol != null && (rcm.Protocol.StartsWith("ADC/") || rcm.Protocol.StartsWith("ADCS/")))
                        {
                            try
                            {
                                version = double.Parse(rcm.Protocol.Substring(rcm.Protocol.IndexOf("/") + 1), CultureInfo.GetCultureInfo("en-GB").NumberFormat);
                            }
                            catch { }
                            if (version <= 1.0)
                            {
                                // Support for prior versions of adc then 1.0
                                string token = rcm.Token;
                                if (version < 1.0 && rcm.Token.StartsWith("TO"))
                                    token = rcm.Token.Substring(2);

                                Update(con, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(token, hub, usr.UserInfo, false)));
                                
                                if (rcm.Secure && hub.Me.ContainsKey(UserInfo.SECURE))
                                    hub.Send(new CTM(hub, rcm.Id, rcm.IDTwo, rcm.Protocol, int.Parse(0 + hub.Me.Get(UserInfo.SECURE)), token));
                                else
                                    hub.Send(new CTM(hub, rcm.Id, rcm.IDTwo, rcm.Protocol, hub.Share.Port, token));
                            }
                            else
                            {
                                hub.Send(new STA(hub, rcm.Id, hub.Me.ID, "241", "Protocol is not supported. I only support ADC 1.0 and prior", "TO" + rcm.Token + " PR" + rcm.Protocol));
                                return;
                            }
                        }
                    }
                }
                else
                {
                    // TODO : we should probably return a STA message.
                }
            }
                #endregion
                #region GFI
            else if (message is GFI && this.trans != null)
            {
                GFI gfi = (GFI)message;
                if (gfi.Identifier != null)
                {
                    trans.Content = new ContentInfo();
                    switch (gfi.ContentType)
                    {
                        case "file":        // Requesting file
                            // This is because we have support for old DC++ client and mods like (DCDM who has ASCII encoding)
                            if (gfi.Identifier.Equals("files.xml.bz2"))
                            {
                                trans.Content.Set(ContentInfo.FILELIST, BaseFilelist.XMLBZ);
                                trans.Content.Set(ContentInfo.VIRTUAL, Encoding.UTF8.WebName + gfi.Identifier);
                            }
                            else if (gfi.Identifier.StartsWith("TTH/"))
                            {
                                trans.Content.Set(ContentInfo.TTH, gfi.Identifier.Substring(4));
                            }
                            else
                            {
                                trans.Content.Set(ContentInfo.VIRTUAL, gfi.Identifier);
                            }
                            break;
                        case "list":        // TODO : We dont care about what subdirectoy user whats list for
                            trans.Content.Set(ContentInfo.FILELIST, BaseFilelist.XML);
                            trans.Content.Set(ContentInfo.VIRTUAL, string.Format("{0}files.xml", System.Text.Encoding.UTF8.WebName));
                            break;
                        default:            // We are not supporting type. Disconnect
                            con.Send(new STA(con, "251", "Type not known:" + gfi.ContentType, null));
                            con.Disconnect();
                            return;
                    }
                    SearchInfo si = new SearchInfo();
                    if (trans.Content.ContainsKey(ContentInfo.TTH))
                        si.Set(SearchInfo.TYPE, trans.Content.Get(ContentInfo.TTH));
                    si.Set(SearchInfo.SEARCH, trans.Content.Get(ContentInfo.VIRTUAL));
                    SendRES(si, trans.User);
                }
            }
                #endregion
                #region GET
            else if (message is GET && this.trans != null)
            {
                GET get = (GET)message;
                // If we are supposed to download and other client tries to download. Disconnect.
                if (trans != null && this.download)
                {
                    trans.Disconnect();
                    return;
                }
                bool firstTime = true;

                if (get.Identifier != null)
                {
                    trans.Content = new ContentInfo();
                    switch (get.ContentType)
                    {
                        case "file":        // Requesting file
                            // This is because we have support for old DC++ client and mods like (DCDM who has ASCII encoding)
                            if (get.Identifier.Equals("files.xml.bz2"))
                            {
                                trans.Content.Set(ContentInfo.FILELIST, BaseFilelist.XMLBZ);
                                trans.Content.Set(ContentInfo.VIRTUAL, Encoding.UTF8.WebName + get.Identifier);
                            }
                            else if (get.Identifier.StartsWith("TTH/"))
                            {
                                trans.Content.Set(ContentInfo.TTH, get.Identifier.Substring(4));
                            }
                            else
                            {
                                trans.Content.Set(ContentInfo.VIRTUAL, get.Identifier);
                            }
                            break;
                        case "list":        // TODO : We dont care about what subdirectoy user whats list for
                            trans.Content.Set(ContentInfo.FILELIST, BaseFilelist.XML);
                            trans.Content.Set(ContentInfo.VIRTUAL, string.Format("{0}files.xml", Encoding.UTF8.WebName));
                            break;
                        case "tthl":
                            // TTH/DQSGG2MYKKLXX4N2P7TBPKSC5HVBO3ISYZPLMWA
                            if (get.Identifier.StartsWith("TTH/"))
                            {
                                trans.Content.Set(ContentInfo.TTH, get.Identifier.Substring(4));

                                ContentInfo tmp = trans.Content;
                                if (con.Share != null && con.Share.ContainsContent(ref tmp) && tmp.ContainsKey(ContentInfo.TTHL))
                                {
                                    byte[] bytes = Base32.Decode(tmp.Get(ContentInfo.TTHL));
#if !COMPACT_FRAMEWORK
                                    trans.CurrentSegment = new SegmentInfo(-1, 0, bytes.LongLength);
#else
                                    trans.CurrentSegment = new SegmentInfo(-1, 0, bytes.Length);
#endif

                                    con.Send(new SND(trans, get.ContentType, get.Identifier, new SegmentInfo(-1, trans.CurrentSegment.Start, trans.CurrentSegment.Length)));
                                    // Send content to user
                                    System.IO.MemoryStream ms = new System.IO.MemoryStream(bytes);
                                    ms.Flush();
                                    bytes = ms.ToArray();
                                    con.Send(new BinaryMessage(con, bytes, bytes.Length));
                                    System.Console.WriteLine("TTH Leaves:" + Base32.Encode(bytes));
                                    firstTime = true;
                                }
                            }
                            if (!firstTime)
                            {
                                // We should not get here if file is in share.
                                con.Send(new STA(con, "251", "File not available", null));
                                con.Disconnect();
                            }
                            return;
                        default:            // We are not supporting type. Disconnect
                            con.Send(new STA(con, "251", "Type not known:" + get.ContentType, null));
                            con.Disconnect();
                            return;
                    }
                    trans.CurrentSegment = get.SegmentInfo;
                    byte[] bytesToSend = null;
                    try
                    {
                        // TODO : ZLib compression here doesnt work as we want. It takes much memory and much cpu
                        //Util.Compression.ZLib zlib = null;
                        //if (adcget.ZL1)
                        //    zlib = new Fmdc.Util.Compression.ZLib();
                        while (connectionStatus != TcpConnection.Disconnected && (bytesToSend = GetContent(System.Text.Encoding.UTF8, trans.CurrentSegment.Position, trans.CurrentSegment.Length - trans.CurrentSegment.Position)) != null)
                        {
                            if (firstTime)
                            {
                                con.Send(new SND(trans, get.ContentType, get.Identifier, new SegmentInfo(-1, get.SegmentInfo.Start, trans.CurrentSegment.Length)));
                                firstTime = false;
                            }

                            trans.CurrentSegment.Position += bytesToSend.Length;
                            // We want to compress content with ZLib
                            //if (zlib != null)
                            //{
                            //    zlib.Compress2(bytesToSend);
                            //    bytesToSend = zlib.Read();
                            //}
                            con.Send(new BinaryMessage(trans, bytesToSend, bytesToSend.Length));
                            bytesToSend = null;

                        }

                        // If we compressing data with zlib. We need to send ending bytes too.
                        //if (zlib != null && connectionStatus != Connection.Disconnected)
                        //    trans.Send(new ConMessage(trans, zlib.close()));
                    }
                    catch (System.Exception e) { System.Console.WriteLine("ERROR:" + e); }
                }
                trans.CurrentSegment = new SegmentInfo(-1);
                trans.Content = null;
                if (firstTime)
                {
                    // We should not get here if file is in share.
                    con.Send(new STA(con, "251", "File not available", null));
                    con.Disconnect();
                }
            }
                #endregion
                #region SND
            else if (message is SND)
            {
                SND snd = (SND)message;
                if (!trans.Content.Get(ContentInfo.REQUEST).Equals(snd.Identifier))
                {
                    trans.Disconnect("I want my bytes..");
                    return;
                }
                if (trans.DownloadItem.ContentInfo.Size == -1)
                {
                    trans.DownloadItem.ContentInfo.Size = snd.SegmentInfo.Length;
                    trans.DownloadItem.SegmentSize = snd.SegmentInfo.Length;
                    EnsureCurrentSegmentCancelation();
                    GetSegment(false);
                }
                else if (trans.CurrentSegment != null && trans.CurrentSegment.Length != snd.SegmentInfo.Length)
                {
                    trans.Disconnect("Why would i want to get a diffrent length of bytes then i asked for?");
                    return;
                }
                this.IsRawData = true;
                //trans.ShouldBlockOnSend = true;
            }
            #endregion
        }

        protected void SendRES(SearchInfo info, UserInfo usr)
        {
            IShare share = null;
            if (hub != null || hub.Share != null || usr != null)
            {
                share = hub.Share;
            }
            else if (trans != null || trans.Share != null || usr != null)
            {
                share = trans.Share;
            }
            else
            {
                return;
            }

            // If we dont have a share object. break here.
            if (share == null)
                return;

            int maxReturns = 10;
            string token = null;
            if (info.ContainsKey(SearchInfo.TOKEN))
                token = info.Get(SearchInfo.TOKEN);

            var ret = new List<ContentInfo>(maxReturns);
            // TODO : This lookup can be done nicer
            lock (share)
            {
                foreach (KeyValuePair<string, ContentInfo> var in share)
                {
                    if (var.Value == null)
                        continue;
                    bool foundEnough = false;
                    string ext = info.Get(SearchInfo.EXTENTION);
                    string sch = info.Get(SearchInfo.SEARCH);
                    if (ext != null && sch != null)
                    {
                        ContentInfo contentInfo = new ContentInfo();
                        if (info.ContainsKey(SearchInfo.TYPE))
                        {
                            switch (info.Get(SearchInfo.TYPE))
                            {
                                case "2":

                                    contentInfo.Set(ContentInfo.TTH, info.Get(SearchInfo.SEARCH));
                                    if (hub.Share.ContainsContent(ref contentInfo))
                                    {
                                        ret.Add(contentInfo);
                                    }
                                    // We are looking through whole share here.
                                    // If no TTH matching. Ignore.
                                    foundEnough = true;
                                    break;
                                case "1":
                                default:
                                    if (var.Value.ContainsKey(ContentInfo.VIRTUAL) && (System.IO.Path.GetDirectoryName(var.Value.Get(ContentInfo.VIRTUAL)).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1))
                                        ret.Add(var.Value);
                                    break;
                            }
                        }
                        if (!foundEnough)
                        {
                            string infoExt = System.IO.Path.GetExtension(var.Value.Get(ContentInfo.VIRTUAL)).TrimStart('.');
                            if (
                                var.Value.ContainsKey(ContentInfo.VIRTUAL)
                                && (var.Value.Get(ContentInfo.VIRTUAL).IndexOf(sch, System.StringComparison.OrdinalIgnoreCase) != -1)
                                && (ext.Length == 0 || ext.Contains(infoExt))
                                )
                                ret.Add(var.Value);
                        }
                    }
                    if (foundEnough || ret.Count >= maxReturns)
                        break;
                }
            }

            int x = 0;

            // Test against size restrictions
            for (int i = 0; i < ret.Count; i++)
            {
                bool send = true;
                long size = -1;
                try
                {
                    size = long.Parse(info.Get(SearchInfo.SIZE));
                }
                catch { }
                if (info.ContainsKey(SearchInfo.SIZETYPE) && size != -1)
                {
                    switch (info.Get(SearchInfo.SIZETYPE))
                    {
                        case "1":       // Min Size
                            send = (size <= ret[i].Size);
                            break;
                        case "2":       // Max Size
                            send = (size >= ret[i].Size);
                            break;
                        case "3":       // Equal Size
                            send = (size == ret[i].Size);
                            break;
                    }
                }
                // Should this be sent?
                if (send)
                {
                    RES res = new RES(con, ret[i], token, usr);
                    if (res.Address != null && hub != null)
                    {
                        if (10 > x++)
                        {
                            // Send with UDP
                            try
                            {
                                UdpConnection.Send(res, res.Address);
                            }
                            catch { }
                        }
                    }
                    else
                    {
                        if (5 > x++)
                        {
                            if (hub != null)
                            {
                                // Send through hub
                                hub.Send(res);
                            }
                            else if (trans != null)
                            {
                                trans.Send(res);
                            }
                        }
                    }
                }
            }
        }

        public void ActOnOutMessage(DefaultEventArgs e)
        {
            if (e.Action.Equals(Actions.MainMessage) && hub != null)
            {
                var main = (MainMessage)e.Data;
                hub.Send(new MSG(hub, hub.Me, main.ShowAsMe, main.Content));
            }
            else if (e.Action.Equals(Actions.PrivateMessage) && hub != null)
            {
                var pm = (PrivateMessage)e.Data;
                hub.Send(new MSG(hub, hub.Me, pm.ShowAsMe, pm.Content, pm.To, pm.Group));
            }
            else if (e.Action.Equals(Actions.Password))
            {
                hub.Send(new PAS(hub, this.gpaString, e.Data.ToString()));
            }
            else if (e.Action.Equals(Actions.Search) && hub != null)
            {
                hub.Send(new SCH(hub, (SearchInfo)e.Data, hub.Me.ID));
            }
            else if (e.Action.Equals(Actions.StartTransfer) && hub != null)
            {
                var usr = e.Data as User;
                UserInfo usrInfo;
                if (usr != null)
                    usrInfo = usr.UserInfo;
                else
                    usrInfo = e.Data as UserInfo;

                if (usrInfo == null || hub.Share == null)
                    return;
                // Do user support connecting?
                if (usrInfo.ContainsKey(UserInfo.IP))
                {
                    switch (hub.Me.Mode)
                    {
                        case ConnectionTypes.Direct:
                        case ConnectionTypes.UPnP:
                        case ConnectionTypes.Forward:
                            // We are active and they are active. Let them connect to us
                            // TODO : We should really use something else as token
                            Update(con, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(usrInfo.ID, hub, usrInfo, true)));
                            if (usrInfo.ContainsKey(UserInfo.SECURE) && hub.Me.ContainsKey(UserInfo.SECURE))
                                hub.Send(new CTM(hub, usrInfo.ID, hub.Me.ID, "ADCS/0.10", hub.Share.Port, usrInfo.ID));
                            else
                                hub.Send(new CTM(hub, usrInfo.ID, hub.Me.ID, hub.Share.Port, usrInfo.ID));
                            break;
                        case ConnectionTypes.Passive:
                        case ConnectionTypes.Socket5:
                        case ConnectionTypes.Unknown:
                        default:
                            // We are passive and they are active. Let us connect to them
                            if (usrInfo.Mode == ConnectionTypes.Passive)
                            {
                                break;
                            }
                            // TODO : We should really use something else as token
                            Update(con, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(usrInfo.ID, hub, usrInfo, true)));
                            if (usrInfo.ContainsKey(UserInfo.SECURE) && hub.Me.ContainsKey(UserInfo.SECURE))
                                hub.Send(new RCM(usrInfo.ID, hub, "ADCS/0.10", hub.Me.ID, usrInfo.ID));
                            else
                                hub.Send(new RCM(usrInfo.ID, hub, hub.Me.ID, usrInfo.ID));
                            break;
                    }
                }
                else
                {
                    // Other user doesnt support active connections (We have no ip to connect to)
                    switch (hub.Me.Mode)
                    {
                        case ConnectionTypes.Direct:
                        case ConnectionTypes.UPnP:
                        case ConnectionTypes.Forward:
                            // TODO : We should realy use something else as token
                            Update(con, new DefaultEventArgs(Actions.TransferRequest, new TransferRequest(usrInfo.ID, hub, usrInfo, true)));
                            hub.Send(new CTM(hub, usrInfo.ID, hub.Me.ID, hub.Share.Port, usrInfo.ID));
                            break;
                    }
                }
            }
        }
        #endregion
        #endregion
        #region Event(s)
        void OnUpdate(object sender, DefaultEventArgs e) { }
        protected void OnMessageReceived(object sender, DefaultEventArgs e) { }
        protected void OnMessageToSend(object sender, DefaultEventArgs e)
        {
            if (e.Data is SUP)
            {
                hasSentSUP = true;
            }
        }
        #endregion

        #region IProtocolTransfer Members
        public void OnDownload()
        {
            if (trans == null || trans.User == null || trans.Share == null || trans.Me == null)
                return;
            if (download)
            {
                // We won battle. Start download.
                if (trans.DownloadItem != null && trans.CurrentSegment != null && trans.CurrentSegment.Index != -1)
                {
                    // Set right content string
                    trans.Content = new ContentInfo(ContentInfo.REQUEST, trans.DownloadItem.ContentInfo.Get(ContentInfo.VIRTUAL));
                    if (trans.DownloadItem.ContentInfo.IsFilelist)
                    {
                        if (supports != null && supports.BZIP)
                        {
                            trans.Content.Set(ContentInfo.REQUEST, "files.xml.bz2");
                            trans.DownloadItem.ContentInfo.Set(ContentInfo.FILELIST, BaseFilelist.XMLBZ);
                        }
                        else
                        {
                            trans.Content.Set(ContentInfo.REQUEST, "files.xml");
                            trans.DownloadItem.ContentInfo.Set(ContentInfo.FILELIST, BaseFilelist.XML);
                        }
                    }

                    // $UGetZBlock needs both SupportGetZBlock and SupportXmlBZList.
                    // $UGetBlock needs SupportXmlBZList.
                    // $GetZBlock needs SupportGetZBlock.
                    // $ADCGET needs SupportADCGet.
                    // $Get needs nothing
                    if (supports != null && supports.TIGR)
                    {
                        if (trans.DownloadItem.ContentInfo.ContainsKey(ContentInfo.TTH))
                            trans.Content.Set(ContentInfo.REQUEST, "TTH/" + trans.DownloadItem.ContentInfo.Get(ContentInfo.TTH));

                        trans.Send(new GET(trans, trans.Content, trans.CurrentSegment));
                    }
                    else
                    {
                        trans.Send(new GET(trans, trans.Content, trans.CurrentSegment));
                    }
                }
            }
        }

        [System.Obsolete("This method is depricated. Please use GetSegment instead")]
        public void GetDownloadItem()
        {
            GetSegment(true);
        }

        public bool GetSegment(bool requestNewDownloadItem)
        {
            if (requestNewDownloadItem)
            {
                // Get content
                trans.DownloadItem = null;
                DownloadItem dwnItem = null;
                UserInfo usrInfo = trans.User;
                if (usrInfo != null)
                {
                    DefaultEventArgs eArgs = new DefaultEventArgs(0, dwnItem);
                    ChangeDownloadItem(trans, eArgs);

                    trans.DownloadItem = eArgs.Data as DownloadItem;
                }
            }
            download = (trans.DownloadItem != null && (trans.CurrentSegment = trans.DownloadItem.GetAvailable(trans.Source)).Index != -1);
            return download;
        }

        #endregion
    }
}