
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

using FlowLib.Interfaces;
using FlowLib.Protocols;
using FlowLib.Containers;
using FlowLib.Connections;

namespace FlowLib.Protocols.Adc
{
    #region Receive AND Send
    public class SND : AdcBaseMessage
    {
        protected string contentType = null;
        protected string identifier = null;
        protected SegmentInfo segment = null;

        public SND(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 4)
            {
                contentType = param[0];
                identifier = param[1];
                try
                {
                    long start = int.Parse(param[2]);
                    long length = int.Parse(param[3]);
                    segment = new SegmentInfo(-1, start, length);
                    valid = true;
                }
                catch { }
            }
        }
    }
    public class GFI : AdcBaseMessage
    {
        protected string contentType = null;
        protected string identifier = null;

        public GFI(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 2)
            {
                contentType = param[0];
                identifier = param[1];
                valid = true;
            }
        }

        public GFI(Hub hub, string type, string contentId)
            : base(hub, null)
        {
            contentType = type;
            identifier = contentId;
            Raw = string.Format("CGFI {0} {1}\n", type, contentId);
        }
    }
    public class GET : AdcBaseMessage
    {
        protected string contentType = null;
        protected string identifier = null;
        protected SegmentInfo segment = null;

        public string ContentType
        {
            get { return contentType; }
        }
        public string Identifier
        {
            get { return identifier; }
        }
        public SegmentInfo SegmentInfo
        {
            get { return segment; }
        }
        public GET(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 4)
            {
                contentType = param[0];
                identifier = param[1];
                try
                {
                    long start = int.Parse(param[2]);
                    long length = int.Parse(param[3]);
                    segment = new SegmentInfo(-1, start, length);
                    valid = true;
                }
                catch { }
            }
        }

        public GET(Hub hub, ContentInfo info, SegmentInfo segment)
            : this(hub, info, segment, "file") { }

        public GET(Hub hub, ContentInfo info, SegmentInfo segment, string type)
            : base(hub, null)
        {
            string req = null;
            if (info.ContainsKey(ContentInfo.REQUEST))
                req = info.Get(ContentInfo.REQUEST);
            else if (info.ContainsKey(ContentInfo.TTH))
                req = info.Get(ContentInfo.TTH);
            else if (info.ContainsKey(ContentInfo.VIRTUAL))
                req = info.Get(ContentInfo.VIRTUAL);

            if (req == null)
                throw new System.ArgumentException("ContentInfo must contain any of: REQUEST, TTH or VIRTUAL");

            // TODO : Add support for list also
            Raw = string.Format("CGET {0} {1} {2} {3}\n", type, req, segment.Position, segment.Length);
        }
    }
    public class RCM : AdcBaseMessage
    {
        protected string protocol = null;
        protected string token = null;

        public string Protocol
        {
            get { return protocol; }
        }
        public string Token
        {
            get { return token; }
        }

        public RCM(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 2)
            {
                protocol = param[0];
                token = param[1];
                valid = true;
            }
        }

        public RCM(string token, Hub hub)
            : this(token, hub, "ADC/1.0") { }
        public RCM(string token, Hub hub, string protocol)
            : base(hub, null)
        {
            this.protocol = protocol;
            this.token = token;
            Raw = string.Format("DRCM {0} {1}\n", protocol, token);
        }
    }
    public class CTM : AdcBaseMessage
    {
        protected string protocol = null;
        protected int port = -1;
        protected string token = null;

        public string Protocol
        {
            get { return protocol; }
        }
        public int Port
        {
            get { return port; }
        }
        public string Token
        {
            get { return token; }
        }
        public CTM(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 3)
            {
                protocol = param[0];
                try
                {
                    port = int.Parse(param[1]);
                    valid = true;
                }
                catch { }
                token = param[2];
            }
        }

        public CTM(Hub hub, int port, string token)
            : this(hub, "ADC/1.0", port, token) { }

        public CTM(Hub hub, string protocol, int port, string token)
            : base(hub, null)
        {
            Raw = string.Format("DCTM {0} {1} {2}\n", protocol, port.ToString(), token);
        }
    }
    public class SCH : AdcBaseMessage
    {
        protected SearchInfo info = new SearchInfo();
        public SearchInfo Info
        {
            get { return info; }
        }

        public SCH(Hub hub, string raw)
            : base(hub, raw)
        {
            // BSCH NRQF TRUDHPNB4BUIQV2LAI4HDWRL3KLJTUSXCTAMJHNII TOauto
            for (int i = 0; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "AN":
                        //TODO : We should make it possible to have more then 1 AN.
                        if (info.ContainsKey(SearchInfo.SEARCH))
                            value = value + " " + info.Get(SearchInfo.SEARCH);
                        info.Set(SearchInfo.SEARCH, value);
                        valid = true;
                        break;
                    case "NO":
                        //TODO : We should make it possible to have more then 1 AN.
                        info.Set(SearchInfo.NOSEARCH, value);
                        break;
                    case "EX":
                        if (info.ContainsKey(SearchInfo.EXTENTION))
                            value = value + " " + info.Get(SearchInfo.EXTENTION);
                        info.Set(SearchInfo.EXTENTION, value);
                        break;
                    case "LE":
                        info.Set(SearchInfo.SIZETYPE, "2");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "GE":
                        info.Set(SearchInfo.SIZETYPE, "1");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "EQ":
                        info.Set(SearchInfo.SIZETYPE, "3");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "TO":
                        info.Set(SearchInfo.TOKEN, value);
                        break;
                    case "TY":
                        switch (value)
                        {
                            case "1":   // File
                                break;
                            case "2":   // Directory
                                info.Set(SearchInfo.TYPE, "1");
                                break;
                            default:
                                break;
                        }
                        break;
                    case "TR":
                        info.Set(SearchInfo.SEARCH, value);
                        info.Set(SearchInfo.TYPE, "2");
                        valid = true;
                        break;
                }
            }
        }

        public SCH(Hub hub, SearchInfo info)
            : base(hub, null)
        {
            this.info = info;
            // BSCH NRQF TRUDHPNB4BUIQV2LAI4HDWRL3KLJTUSXCTAMJHNII TOauto
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            #region EX
            if (info.ContainsKey(SearchInfo.EXTENTION))
            {
                string[] ext = info.Get(SearchInfo.EXTENTION).Split(' ');
                foreach (string extention in ext)
                {
                    sb.Append(" EX" + extention);
                }
            }
            #endregion
            #region TY
            if (info.ContainsKey(SearchInfo.TYPE))
            {
                switch (info.Get(SearchInfo.TYPE))
                {
                    case "0":
                        sb.Append(" TY" + HubAdcProtocol.ConvertOutgoing("1")); break;
                    case "1":
                        sb.Append(" TY" + HubAdcProtocol.ConvertOutgoing("2")); break;
                    case "2":
                        sb.Append(" AN" + info.Get(SearchInfo.SEARCH)); break;
                }
            }
            #endregion
            #region AN
            if (info.ContainsKey(SearchInfo.SEARCH) && info.ContainsKey(SearchInfo.TYPE))
                sb.Append(" AN" + HubAdcProtocol.ConvertOutgoing(info.Get(SearchInfo.SEARCH)));
            #endregion
            #region NO
            if (info.ContainsKey(SearchInfo.NOSEARCH))
                sb.Append(" NO" + HubAdcProtocol.ConvertOutgoing(info.Get(SearchInfo.NOSEARCH)));
            #endregion
            #region TO
            if (info.ContainsKey(SearchInfo.TOKEN))
                sb.Append(" TO" + HubAdcProtocol.ConvertOutgoing(info.Get(SearchInfo.TOKEN)));
            #endregion
            #region Size Type
            if (info.ContainsKey(SearchInfo.SIZETYPE))
            {
                string size = info.Get(SearchInfo.SIZE);
                switch (info.Get(SearchInfo.SIZETYPE))
                {
                    case "1":
                        sb.Append(" GE" + size);
                        break;
                    case "2":
                        sb.Append(" LE" + size);
                        break;
                    case "3":
                        sb.Append(" EQ" + size);
                        break;
                }
            }
            #endregion
            Raw = string.Format("BSCH {0}{1}\n", hub.Me.ID, sb.ToString());
        }
    }
    public class RES : AdcBaseMessage
    {
        protected ContentInfo info = null;
        protected string token = null;
        protected long slots;
        protected System.Net.IPEndPoint address = null;

        public System.Net.IPEndPoint Address
        {
            get { return address; }
        }

        public ContentInfo Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
            }
        }

        public string Token
        {
            get { return token; }
        }

        public long Slots
        {
            get { return slots; }
        }

        public RES(Hub hub, string raw)
            : base(hub, raw)
        {
            info = new ContentInfo();
            foreach (string var in param)
            {
                if (var.Length < 2)
                    continue;
                string key = var.Substring(0, 2);
                string value = var.Substring(2);
                switch (key)
                {
                    case "FN":
                        info.Set(ContentInfo.VIRTUAL, value);
                        valid = true;
                        break;
                    case "SI":
                        try
                        {
                            info.Size = long.Parse(value);
                        }
                        catch { }
                        break;
                    case "SL":
                        try
                        {
                            slots = long.Parse(value);
                        }
                        catch { }
                        break;
                    case "TO":
                        token = value;
                        break;
                }
            }
        }

        public RES(Hub hub, ContentInfo info, string token, string from)
            : base(hub, null)
        {
            this.info = info;

            User usr = null;
            if ((usr = hub.GetUserById(from)) != null)
            {
                if (
                    usr.UserInfo.ContainsKey(UserInfo.UDPPORT)
                    && usr.UserInfo.ContainsKey("SU")
                    && usr.UserInfo.ContainsKey(UserInfo.IP)
                    && (usr.UserInfo.Get("SU").Contains("UDP4") || usr.UserInfo.Get("SU").Contains("UDP6"))
                    )
                {
                    type = "U";
                    int port = -1;
                    string addr = usr.UserInfo.Get(UserInfo.IP);
                    try
                    {
                        port = int.Parse(usr.UserInfo.Get(UserInfo.UDPPORT));
                        if (port < 0 || port > 65535)
                            port = 0;
                        address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(addr), port);
                    }
                    catch { }
                }
                else
                {
                    type = "D";
                }

                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                if (info.ContainsKey(ContentInfo.VIRTUAL))
                    sb.Append(" FN" + info.Get(ContentInfo.VIRTUAL));
                sb.Append(" SI" + info.Size.ToString());
                if (!string.IsNullOrEmpty(token))
                    sb.Append(" TO" + token);
                // TODO : Add Slots handling
                Raw = string.Format("{0}RES{1}\n", type, sb.ToString());
            }
        }
    }
    public class MSG : AdcBaseMessage
    {
        protected string content = null;
        protected bool me = false;
        protected string pmgroup = null;
        /// <summary>
        /// Group/chatroom where Pm was received from.
        /// </summary>
        public string PmGroup
        {
            get { return pmgroup; }
        }
        public string Content
        {
            get { return content; }
        }
        /// <summary>
        /// Constructor for recieving
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="raw"></param>
        public MSG(Hub hub, string raw)
            : base(hub, raw)
        {
            //I: IMSG Have\sa\snice\sday,\sand\sbehave\s;)
            //B: BMSG SHJV PIP
            //D: DMSG SHJV SH5B PIIP!! PMSHJV
            if (param == null)
                return;
            if (param.Count >= 1)
            {
                from = id;
                content = param[0];
                valid = true;
            }
            // Param
            for (int i = 1; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0,2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "PM":
                        pmgroup = value;
                        break;
                    case "ME":
                        me = value.Equals("1");
                        break;
                }
            }

            // Replacing stuff
            if (content != null)
            {
                content = HubAdcProtocol.ConvertIncomming(content);
            }
        }
        /// <summary>
        /// Constructor for sending Main Chat messages
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="me"></param>
        /// <param name="content"></param>
        public MSG(Hub hub, bool me, string content)
            : this(hub, me, content, null, null)
        {
            // BMSG SHJV PIP
            Raw = "BMSG " + this.from + " " + HubAdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + "\n";
        }
        /// <summary>
        /// Constructor for sending Private messages
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="me"></param>
        /// <param name="content"></param>
        /// <param name="to"></param>
        /// <param name="group"></param>
        public MSG(Hub hub, bool me, string content, string to, string group)
            : base(hub, null)
        {
            this.to = to;
            this.pmgroup = group;
            this.me = me;
            this.content = content;
            if (hub.Me.ContainsKey(UserInfo.SID))
                this.from = hub.Me.Get(UserInfo.SID);
            if (this.to == null || this.pmgroup == null)
            {
                if (this.pmgroup != null)
                    this.to = this.pmgroup;
                else
                    return;
            }
            // DMSG SHJV SH5B PIIP!! PMSHJV
            Raw = "DMSG " + this.from + " " + this.to + " " + HubAdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + ((this.pmgroup != null) ? (" PM" + this.pmgroup) : this.to) + "\n";
        }
    }
    public class SUP : AdcBaseMessage
    {
        /// <summary>
        /// Sending Support to hub
        /// </summary>
        public SUP(Hub hub)
            : base(hub, null)
        {
            Raw = "HSUP " + HubAdcProtocol.Support + "\n";
        }

        public SUP(Hub hub, string raw)
            : base(hub, raw)
        {

        }
    }
    public class INF : AdcBaseMessage
    {
        protected UserInfo info = new UserInfo();
        public UserInfo UserInfo
        {
            get { return info; }
            set
            {
                info = value;
                CreateRaw();
            }
        }
        // Receiving
        public INF(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param == null)
                return;
            info.TagInfo.GenerateTag = true;
            info.Set(UserInfo.SID, id);
            // IINF is from hub. all other should be from user =)
            // NIDCDev\sPublic HU1 HI1 DEThe\spublic\sDirect\sConnect\sdevelopment\shub VEADCH++\sv2.0.0-Release
            // TUOD SF0 SL1 SS0 SUTCP4,UDP4 DEPASIV HN4 HO0 HR0 I489.38.33.162 U41090 IDARJQDWZKC4MMC7PLQNOYSPHVI7V62QPS4IRF5KA EMLIVIUANDREI70@YAHOO.COM US104857600 VE++\s0.698 NI[RO][B][QUICK-NET]LIVIU
            for (int i = 0; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "ID":
                        info.Set(UserInfo.CID, value);
                        break;
                    case "I4":
                        info.Set(UserInfo.IP, value);
                        break;
                    case "I6":
                        info.Set(UserInfo.IP, value);
                        break;
                    case "U4":
                        info.Set(UserInfo.UDPPORT, value);
                        break;
                    case "U6":
                        info.Set(UserInfo.UDPPORT, value);
                        break;
                    case "SS":
                        info.Share = value;
                        break;
                    //case "SF":
                    //    break;
                    //case "US":
                    //    break;
                    //case "DS":
                    //    break;
                    case "SL":
                        try
                        {
                            info.TagInfo.Slots = int.Parse(value);
                        }
                        catch (System.Exception)
                        {
                            info.TagInfo.Slots = 0;
                        }
                        break;
                    //case "AS":
                    //    break;
                    //case "AM":
                    //    break;
                    case "EM":
                        info.Email = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "HN":
                        try
                        {
                            info.TagInfo.Normal = int.Parse(value);
                        }
                        catch (System.Exception)
                        {
                            info.TagInfo.Normal = 0;
                        }
                        break;
                    case "HR":
                        try
                        {
                            info.TagInfo.Regged = int.Parse(value);
                        }
                        catch (System.Exception)
                        {
                            info.TagInfo.Regged = 0;
                        }
                        break;
                    case "HO":
                        try
                        {
                            info.TagInfo.OP = int.Parse(value);
                        }
                        catch (System.Exception)
                        {
                            info.TagInfo.OP = 0;
                        }
                        break;
                    //case "TO":
                    //    break;
                    //case "OP":
                    //    info.IsOperator = (value == "1");
                    //    break;
                    //case "AW":
                    //    break;
                    //case "BO":
                    //    break;
                    //case "HI":
                    //    break;
                    //case "HU":
                    //    break;
                    //case "SU":
                    //    break;
                    case "NI":
                        info.DisplayName = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "VE":
                        info.TagInfo.Version = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "DE":
                        info.Description = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "CT":
                        try
                        {
                            info.Account = int.Parse(value);
                        }
                        catch { }
                        break;
                    default:
                        // We will add all unhandled fields to user. This is because developer may know how to handle it even if we dont.
                        info.Set(key, value);
                        break;
                }
            }
        }
        public INF(Hub hub)
            : base(hub, null)
        {
            info = hub.Me;
            CreateRaw();
        }

        private void CreateRaw()
        {
            if (info == null)
                return;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("BINF " + info.Get(UserInfo.SID));
            // Do PID exist? If not. Create PID and CID
            FlowLib.Utils.Hash.Tiger tiger = new FlowLib.Utils.Hash.Tiger();
            byte[] data = null;
            if (!info.ContainsKey(UserInfo.PID))
            {
                data = tiger.ComputeHash(System.Text.Encoding.UTF8.GetBytes("FlowLib" + System.DateTime.Now.Ticks.ToString()));
                info.Set(UserInfo.PID,FlowLib.Utils.Convert.Base32.Encode(data));
            }
            // Make CID from PID. This will always be done.
            data = tiger.ComputeHash(FlowLib.Utils.Convert.Base32.Decode(info.Get(UserInfo.PID)));
            info.Set(UserInfo.CID, FlowLib.Utils.Convert.Base32.Encode(data));

            sb.Append(" ID" + info.Get(UserInfo.CID));
            sb.Append(" PD" + info.Get(UserInfo.PID));
            sb.Append(" DE" + HubAdcProtocol.ConvertOutgoing(info.Description));

            sb.Append(" HN" + info.TagInfo.Normal.ToString());
            sb.Append(" HO" + info.TagInfo.OP.ToString());
            sb.Append(" HR" + info.TagInfo.Regged.ToString());
            sb.Append(" NI" + HubAdcProtocol.ConvertOutgoing(info.DisplayName));
            sb.Append(" SL" + info.TagInfo.Slots); // Upload Slots Open
            sb.Append(" SF" + (Hub.Share != null ? Hub.Share.HashedCount : 0));  // Shared Files
            sb.Append(" SS" + (Hub.Share != null ? Hub.Share.HashedSize : 0));    // Share Size in bytes
            if (Hub.Share != null)
            {
                string ip = info.Get(UserInfo.IP);
                if (string.IsNullOrEmpty(ip) || ip.Contains("."))
                {
                    if (string.IsNullOrEmpty(ip))
                        sb.Append(" I40.0.0.0");
                    else
                        sb.Append(" I4" + ip);
                    sb.Append(" SU" + "TCP4,UDP4");  // Support
                    sb.Append(" U4" + Hub.Share.Port.ToString());
                }
                else
                {
                    sb.Append(" I6" + ip);
                    sb.Append(" SU" + "TCP6,UDP6");  // Support
                    sb.Append(" U6" + Hub.Share.Port.ToString());
                }
            }
            sb.Append(" VE" + HubAdcProtocol.ConvertOutgoing(Hub.Me.TagInfo.Version));
            sb.Append("\n");
            Raw = sb.ToString();
        }
    }
    #endregion
    #region Send
    public class PAS : AdcBaseMessage
    {
        protected string password = null;
        protected string random = null;
        protected string encrypted = null;

        public string Password
        {
            get { return password; }
            set { 
                password = value;
                EncryptPassword();
                CreateRaw();
            }
        }
        public PAS(Hub hub, string randomdata) : this(hub, randomdata, hub.HubSetting.Password) { }
        public PAS(Hub hub, string randomdata, string password)
            : base(hub, null)
        {
            this.random = randomdata;
            Password = password;
        }

        private void CreateRaw()
        {
            Raw = "HPAS " + encrypted + "\n";
        }

        private void EncryptPassword()
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(password + random);
            Utils.Hash.Tiger tiger = new FlowLib.Utils.Hash.Tiger();
            data = tiger.ComputeHash(data);
            encrypted = Utils.Convert.Base32.Encode(data);
        }
    }
    #endregion
    #region Receive
    public class SID : AdcBaseMessage
    {
        public SID(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 1)
            {
                id = param[0];
                valid = true;
            }
        }
    }
    /// <summary>
    /// User has quited. From has the User Id.
    /// </summary>
    public class QUI : AdcBaseMessage
    {
        protected string by = null;
        protected long time = 0;
        protected string msg = null;
        protected string address = null;
        protected bool unwanted = false;

        public bool Unwanted
        {
            get { return unwanted; }
        }

        public string DisconnectedBy
        {
            get { return by; }
        }
        public string Address
        {
            get { return address; }
        }
        public string Message
        {
            get { return msg; }
        }
        public long Time
        {
            get { return time; }
        }

        public QUI(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param == null)
                return;
            for (int i = 0; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "ID":
                        by = value;
                        break;
                    case "TL":
                        try
                        {
                            time = long.Parse(value);
                        }
                        catch (System.Exception) { }
                        break;
                    case "MS":
                        msg = value;
                        break;
                    case "RD":
                        address = value;
                        break;
                    case "DI":
                        unwanted = true;
                        break;
                }
            }
        }
    }
    public class STA : AdcBaseMessage
    {
        protected string pfc = null;
        protected string ptl = null;
        protected string pto = null;
        protected string ppr = null;
        protected string pfl = null;
        protected string pip = null;
        public string FC
        {
            get { return pfc; }
        }
        public string TL
        {
            get { return ptl; }
        }
        public string TO
        {
            get { return pto; }
        }
        public string PR
        {
            get { return ppr; }
        }
        public string FL
        {
            get { return pfl; }
        }
        public string IP
        {
            get { return pip; }
        }
        protected string severity = null;
        protected string code = null;
        protected string content = null;
        public string Severity
        {
            get { return severity; }
        }
        public string Code
        {
            get { return code; }
        }
        public string Content
        {
            get { return content; }
        }

        public STA(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param == null)
                return;
            if (param.Count >= 2 && param[0].Length == 3)
            {
                severity = param[0].Substring(0, 1);
                code = param[0].Substring(1);
                content = HubAdcProtocol.ConvertIncomming(param[1]);
            }

            for (int i = 2; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "FC":
                        pfc = value;
                        break;
                    case "TL":
                        ptl = value;
                        break;
                    case "FL":
                        pfl = value;
                        break;
                    case "TO":
                        pto = value;
                        break;
                    case "PR":
                        ppr = value;
                        break;
                    case "IP":
                        pip = value;
                        break;
                }
            }
        }

        public STA(Hub hub, string code, string description, string param)
            : this(hub, null, code, description, param) { }

        public STA(Hub hub, string userId, string code, string description, string param)
            : base(hub, null)
        {
            param = " " + param;
            if (string.IsNullOrEmpty(userId))
                Raw = string.Format("DSTA {0} {1} DE{2}{3}\n", userId, code, HubAdcProtocol.ConvertOutgoing(description), param);
            else
                Raw = string.Format("CSTA {0} DE{1}{2}\n", code, HubAdcProtocol.ConvertOutgoing(description), param);
        }
    }
    public class GPA : AdcBaseMessage
    {
        protected string randomData = null;

        public string RandomData
        {
            get { return randomData; }
        }

        public GPA(Hub hub, string raw)
            : base(hub, raw)
        {
            if (param.Count >= 1)
            {
                randomData = param[0];
                valid = true;
            }
        }
    }
    #endregion
}
