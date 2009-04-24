
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

        public SND(IConnection con, string contentType, string identifier, SegmentInfo info)
            : base(con, null)
        {
            this.contentType = contentType;
            this.identifier = identifier;
            this.segment = info;

            Raw = string.Format("CSND {0} {1} {2} {3}\n", contentType, identifier, info.Start, info.Length);
        }

        public SND(IConnection con, string raw)
            : base(con, raw)
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

        public string ContentType
        {
            get { return contentType; }
        }
        public string Identifier
        {
            get { return identifier; }
        }

        public GFI(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 2)
            {
                contentType = param[0];
                identifier = param[1];
                valid = true;
            }
        }

        public GFI(IConnection con, string type, string contentId)
            : base(con, null)
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
        public GET(IConnection con, string raw)
            : base(con, raw)
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

        public GET(IConnection con, ContentInfo info, SegmentInfo segment)
            : this(con, info, segment, "file") { }

        public GET(IConnection con, ContentInfo info, SegmentInfo segment, string type)
            : base(con, null)
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
        protected bool secure = false;

        public bool Secure
        {
            get { return secure; }
        }

        public string Protocol
        {
            get { return protocol; }
        }
        public string Token
        {
            get { return token; }
        }

        public RCM(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 2)
            {
                protocol = param[0];
                secure = protocol.StartsWith("ADCS/");
                token = param[1];
                valid = true;
            }
        }

        public RCM(string token, IConnection con, string meid, string userid)
            : this(token, con, "ADC/1.0", meid, userid) { }
        public RCM(string token, IConnection con, string protocol, string meid, string userid)
            : base(con, null)
        {
            this.protocol = protocol;
            this.token = token;
            Raw = string.Format("DRCM {0} {1} {2} {3}\n", meid, userid, protocol, token);
        }
    }
    public class CTM : AdcBaseMessage
    {
        protected string protocol = null;
        protected int port = -1;
        protected string token = null;
        protected bool secure = false;

        public string Protocol
        {
            get { return protocol; }
        }

        public bool Secure
        {
            get { return secure; }
        }

        public int Port
        {
            get { return port; }
        }
        public string Token
        {
            get { return token; }
        }
        public CTM(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 3)
            {
                protocol = param[0];
                secure = protocol.StartsWith("ADCS/");
                try
                {
                    port = int.Parse(param[1]);
                    valid = true;
                }
                catch { }
                token = param[2];
            }
        }

        public CTM(IConnection con, string usrId, string meId, int port, string token)
            : this(con, usrId, meId, "ADC/1.0", port, token) { }

        public CTM(IConnection con, string usrId, string meId, string protocol, int port, string token)
            : base(con, null)
        {
            Raw = string.Format("DCTM {3} {4} {0} {1} {2}\n", protocol, port.ToString(), token, meId, usrId);
        }
    }
    public class SCH : AdcBaseMessage
    {
        protected SearchInfo info = new SearchInfo();
        public SearchInfo Info
        {
            get { return info; }
        }

        public SCH(IConnection con, string raw)
            : base(con, raw)
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

        public SCH(IConnection con, SearchInfo info, string userId)
            : base(con, null)
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
                    sb.Append(" EX" + AdcProtocol.ConvertOutgoing(extention));
                }
            }
            #endregion
            #region TY
            if (info.ContainsKey(SearchInfo.TYPE))
            {
                switch (info.Get(SearchInfo.TYPE))
                {
                    case "0":
                        sb.Append(" TY" + AdcProtocol.ConvertOutgoing("1")); break;
                    case "1":
                        sb.Append(" TY" + AdcProtocol.ConvertOutgoing("2")); break;
                    case "2":
                        sb.Append(" AN" + info.Get(SearchInfo.SEARCH)); break;
                }
            }
            #endregion
            #region AN
            if (info.ContainsKey(SearchInfo.SEARCH) && info.ContainsKey(SearchInfo.TYPE))
                sb.Append(" AN" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.SEARCH)));
            #endregion
            #region NO
            if (info.ContainsKey(SearchInfo.NOSEARCH))
                sb.Append(" NO" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.NOSEARCH)));
            #endregion
            #region TO
            if (info.ContainsKey(SearchInfo.TOKEN))
                sb.Append(" TO" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.TOKEN)));
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
            Raw = string.Format("BSCH {0}{1}\n", userId, sb.ToString());
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

        public RES(IConnection con, string raw)
            : base(con, raw)
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
                        info.Set(ContentInfo.VIRTUAL, AdcProtocol.ConvertIncomming(value));
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
                    case "TR":
                        info.Set(ContentInfo.TTH, value);
                        break;
                    default:
                        // We dont know this value but to allow developer to support this anyway we will store it.
                        info.Set(key, value);
                        break;
                }
            }
        }

        public RES(IConnection con, ContentInfo info, string token, UserInfo usr)
            : base(con, null)
        {
            this.info = info;

            if (
                usr.ContainsKey(UserInfo.UDPPORT)
                && usr.ContainsKey("SU")
                && usr.ContainsKey(UserInfo.IP)
                && (usr.Get("SU").Contains("UDP4") || usr.Get("SU").Contains("UDP6"))
                )
            {
                type = "U";
                int port = -1;
                string addr = usr.Get(UserInfo.IP);
                try
                {
                    port = int.Parse(usr.Get(UserInfo.UDPPORT));
                    if (port < 0 || port > 65535)
                        port = 0;
                    address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(addr), port);
                }
                catch { }
            }
            else if (con is Hub)
            {
                type = "D";
            }
            else
            {
                type = "C";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (info.ContainsKey(ContentInfo.VIRTUAL))
                sb.Append(" FN" + AdcProtocol.ConvertOutgoing(info.Get(ContentInfo.VIRTUAL)));
            sb.Append(" SI" + info.Size.ToString());
            if (!string.IsNullOrEmpty(token))
                sb.Append(" TO" + token);
            // TODO : Add Slots handling
            Raw = string.Format("{0}RES{1}\n", type, sb.ToString());
        }
    }
    public class MSG : AdcBaseMessage
    {
        protected string content = null;
        protected bool me = false;
        protected string pmgroup = null;
        protected string from = null;
        protected string to = null;
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
        public string From
        {
            get { return from; }
            set { from = value; }
        }
        public string To
        {
            get { return to; }
            set { to = value; }
        }
        /// <summary>
        /// Constructor for recieving
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="raw"></param>
        public MSG(IConnection con, string raw)
            : base(con, raw)
        {
            //I: IMSG Have\sa\snice\sday,\sand\sbehave\s;)
            //B: BMSG SHJV PIP
            //D: DMSG SHJV SH5B PIIP!! PMSHJV
            if (param == null)
                return;
            if (param.Count >= 1)
            {
                from = id;
                to = idtwo;
                content = param[0];
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
                content = AdcProtocol.ConvertIncomming(content);
                valid = true;
            }
        }
        /// <summary>
        /// Constructor for sending Main Chat messages
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="me"></param>
        /// <param name="content"></param>
        public MSG(IConnection con, UserInfo usr,  bool me, string content)
            : this(con, usr, me, content, null, null)
        {
            // BMSG SHJV PIP
            Raw = "BMSG " + this.from + " " + AdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + "\n";
        }
        /// <summary>
        /// Constructor for sending Private messages
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="me"></param>
        /// <param name="content"></param>
        /// <param name="to"></param>
        /// <param name="group"></param>
        public MSG(IConnection con, UserInfo usr, bool me, string content, string to, string group)
            : base(con, null)
        {
            this.to = to;
            this.pmgroup = group;
            this.me = me;
            this.content = content;
            if (usr.ContainsKey(UserInfo.SID))
                this.from = usr.Get(UserInfo.SID);
            if (this.to == null || this.pmgroup == null)
            {
                if (this.pmgroup != null)
                    this.to = this.pmgroup;
            }
            // DMSG SHJV SH5B PIIP!! PMSHJV
            //Raw = "DMSG " + this.from + " " + this.to + " " + AdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + " PM"+((this.pmgroup != null) ? (this.pmgroup) : this.from) + "\n";
            Raw = "EMSG " + this.from + " " + this.to + " " + AdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + " PM" + ((this.pmgroup != null) ? (this.pmgroup) : this.from) + "\n";
        }
    }
    public class SUP : AdcBaseMessage
    {
        protected bool bas = false;
        protected bool bzip = false;
        protected bool tigr = false;
        protected bool adcs = false;

        public bool BASE
        {
            get { return bas; }
        }

        public bool BZIP
        {
            get { return bzip; }
        }

        public bool TIGR
        {
            get { return tigr; }
        }

        public bool ADCS
        {
            get { return adcs; }
            set { adcs = value; }
        }

        /// <summary>
        /// Sending Support to hub
        /// </summary>
        public SUP(IConnection con)
            : base(con, null)
        {
            if (con is Hub)
            {
                if (((Hub)con).Me.ContainsKey(UserInfo.SECURE))
                    Raw = "HSUP " + AdcProtocol.Support + " ADADC0" + "\n";
                else
                    Raw = "HSUP " + AdcProtocol.Support + "\n";
            }
            else
            {
                if (((Transfer)con).Me.ContainsKey(UserInfo.SECURE))
                    Raw = "CSUP " + AdcProtocol.TransferSupport + " ADADC0" + "\n";
                else
                    Raw = "CSUP " + AdcProtocol.TransferSupport + "\n";
            }
            ParseRaw();
        }

        public SUP(IConnection con, string raw)
            : base(con, raw)
        {
            ParseRaw();
        }

        protected void ParseRaw()
        {
            foreach (string sup in param)
            {
                // We hate old DC++ clients with stupid support
                if (sup.Equals("ADBAS0"))
                {
                    bas = true;
                    bzip = true;
                    tigr = true;
                }
                if (sup.Equals("ADTIGR"))
                    tigr = true;
                if (sup.Equals("ADBZIP"))
                    bzip = true;
                if (sup.Equals("ADBASE"))
                    bas = true;
                if (sup.Equals("ADADC0"))
                    adcs = true;
            }

            if (bas && tigr)
                valid = true;
        }
    }
    public class INF : AdcBaseMessage
    {
        protected UserInfo info = new UserInfo();
        protected string token = null;
        public UserInfo UserInfo
        {
            get { return info; }
            set
            {
                info = value;
                CreateRaw();
            }
        }

        public string Token
        {
            get { return token; }
        }

        // Receiving
        public INF(IConnection con, string raw)
            : base(con, raw)
        {
            if (param == null)
                return;
            info.TagInfo.GenerateTag = true;
            info.Set(UserInfo.SID, id);

            if (typeValid)
                valid = true;

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
                        info.Email = AdcProtocol.ConvertIncomming(value);
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
                    case "TO":
                        token = value;
                        info.Set(key, value);
                        break;
                    case "OP":      // Before ADC 1.0
                        info.IsOperator = (value == "1");
                        break;
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
                        info.DisplayName = AdcProtocol.ConvertIncomming(value);
                        break;
                    case "VE":
                        info.TagInfo.Version = AdcProtocol.ConvertIncomming(value);
                        break;
                    case "DE":
                        info.Description = AdcProtocol.ConvertIncomming(value);
                        break;
                    case "CT":
                        try
                        {
                            info.Account = int.Parse(value);
                        }
                        catch { }
                        break;
                    case "SU":
                        info.Set(key, value);
                        string[] supports = value.Split(',');
                        foreach (string sup in supports)
                        {
                            switch (sup)
                            {
                                case "ADC0":
                                    info.Set(UserInfo.SECURE, "");
                                    break;
                            }
                        }
                        break;
                    default:
                        // We will add all unhandled fields to user. This is because developer may know how to handle it even if we dont.
                        info.Set(key, value);
                        break;
                }
            }
        }
        public INF(IConnection con, UserInfo info)
            : base(con, null)
        {
            this.info = info;
            CreateRaw();
        }

        /// <summary>
        /// Compares 2 INF against eachother and then create a new one containing stuff that is different
        /// </summary>
        /// <param name="inf1"></param>
        /// <param name="inf2"></param>
        /// <returns></returns>
        public static INF MakeInfFromDifference(INF inf1, INF inf2)
        {
            if (inf2 == null)
                return inf1;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(inf1.Type + inf1.Action);
            bool hasId = false;
            bool hasId2 = false;
            switch (inf1.Type)
            {
                case "B":       // Broadcast Message
                case "U":       // UDP Message
                    hasId = true;
                    break;
                case "E":       // Echo message
                case "D":       // Direct message
                    hasId = true;
                    hasId2 = true;
                    break;
                case "C":       // Client Message
                case "I":       // Info message
                default:        // Unknown types
                    break;
            }
            if (hasId)
                sb.Append(" " + inf1.Id);
            if (hasId2)
                sb.Append(" " + inf1.IDTwo);

            for (int x1 = 0; x1 < inf1.Param.Count; x1++)
            {
                bool found = false;
                for (int x2 = 0; x2 < inf2.Param.Count; x2++)
			    {
                    if (inf1.Param[x1].Equals(inf2.Param[x2]))
                    {
                        found = true;
                        break;
                    }
			    }

                // We havnt found a match. Add this one.
                if (!found)
                    sb.Append(" " + inf1.Param[x1]);

            }

            sb.Append("\n");
            // [20:37:58] BINF GQEH IDZE4ZCKFL5KEX7H7AXQW2Q7BCVLRG4AODPWJHP5Q PDAOY7W7NO2CEFQVEPQDIAP7UNNMI6L6VQSZPJY3I DE HN0 HO0 HR0 NIXmpl-633523558744590000 SL2 SF0 SS0 VEXmple\sV:20080720
            // [20:44:00] BINF GQEH IDZE4ZCKFL5KEX7H7AXQW2Q7BCVLRG4AODPWJHP5Q PDAOY7W7NO2CEFQVEPQDIAP7UNNMI6L6VQSZPJY3I DE HN1 HO0 HR0 NIXmpl-633523558744590000 SL2 SF0 SS0 VEXmple\sV:20080720
            return new INF(inf1.con, sb.ToString());
        }

        private void CreateRaw()
        {
            if (info == null)
                return;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (con is Transfer)
            {
                sb.Append("CINF");
                sb.Append(" ID" + info.Get(UserInfo.CID));
                if (info.ContainsKey("TO"))
                    sb.Append(" TO" + info.Get("TO"));
            }
            else
            {
                sb.Append("BINF " + info.Get(UserInfo.SID));
                // Do PID exist? If not. Create PID and CID
                FlowLib.Utils.Hash.Tiger tiger = new FlowLib.Utils.Hash.Tiger();
                byte[] data = null;
                if (!info.ContainsKey(UserInfo.PID))
                {
                    data = tiger.ComputeHash(System.Text.Encoding.UTF8.GetBytes("FlowLib" + System.DateTime.Now.Ticks.ToString()));
                    info.Set(UserInfo.PID, FlowLib.Utils.Convert.Base32.Encode(data));
                }
                // Make CID from PID. This will always be done.
                data = tiger.ComputeHash(FlowLib.Utils.Convert.Base32.Decode(info.Get(UserInfo.PID)));
                info.Set(UserInfo.CID, FlowLib.Utils.Convert.Base32.Encode(data));

                sb.Append(" ID" + info.Get(UserInfo.CID));
                sb.Append(" PD" + info.Get(UserInfo.PID));
                sb.Append(" DE" + AdcProtocol.ConvertOutgoing(info.Description));

                sb.Append(" HN" + info.TagInfo.Normal.ToString());
                sb.Append(" HO" + info.TagInfo.OP.ToString());
                sb.Append(" HR" + info.TagInfo.Regged.ToString());
                sb.Append(" NI" + AdcProtocol.ConvertOutgoing(info.DisplayName));
                sb.Append(" SL" + info.TagInfo.Slots); // Upload Slots Open
                sb.Append(" SF" + (con.Share != null ? con.Share.HashedCount : 0));  // Shared Files
                sb.Append(" SS" + (con.Share != null ? con.Share.HashedSize : 0));    // Share Size in bytes
                if (con.Share != null)
                {
                    System.Text.StringBuilder support = new System.Text.StringBuilder();
                    switch (info.TagInfo.Mode)
                    {
                        case FlowLib.Enums.ConnectionTypes.Direct:
                        case FlowLib.Enums.ConnectionTypes.UPnP:
                        case FlowLib.Enums.ConnectionTypes.Forward:
                            string ip = info.Get(UserInfo.IP);
                            if (string.IsNullOrEmpty(ip) || ip.Contains("."))
                            {
                                if (string.IsNullOrEmpty(ip))
                                    sb.Append(" I40.0.0.0");
                                else
                                    sb.Append(" I4" + ip);
                                support.Append("TCP4,");    // Support
                                support.Append("UDP4,");    // Support
                                sb.Append(" U4" + con.Share.Port.ToString());
                            }
                            else
                            {
                                sb.Append(" I6" + ip);
                                support.Append("TCP6,");    // Support
                                support.Append("UDP6,");    // Support
                                sb.Append(" U6" + con.Share.Port.ToString());
                            }
                            break;
                    }

                    support.Append("BZIP,");         // Support
                    if (info.ContainsKey(UserInfo.SECURE))
                        support.Append("ADC0,");         // Support
                    sb.Append(" SU" + support.ToString());  // Support
                }
                sb.Append(" VE" + AdcProtocol.ConvertOutgoing(info.TagInfo.Version));
            }
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
        public PAS(IConnection con, string raw)
            : base(con, raw)
        {
        }

        public PAS(IConnection con, string randomdata, string password)
            : base(con, null)
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
            try
            {
                Utils.Hash.Tiger tiger = new FlowLib.Utils.Hash.Tiger();
                byte[] data = System.Text.Encoding.UTF8.GetBytes(password);
                System.IO.MemoryStream ms = new System.IO.MemoryStream();
                ms.Write(data, 0, data.Length);
                data = Utils.Convert.Base32.Decode(random);
                ms.Write(data, 0, data.Length);

                data = tiger.ComputeHash(ms.ToArray());
                encrypted = Utils.Convert.Base32.Encode(data);
            }
            catch { }
        }
    }
    #endregion
    #region Receive
    public class SID : AdcBaseMessage
    {
        public SID(IConnection con, string raw)
            : base(con, raw)
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

        public QUI(IConnection con, string raw)
            : base(con, raw)
        {
            if (param == null)
                return;
            if (param.Count >= 1)
            {
                id = param[0];
                param.RemoveAt(0);
                valid = true;
            }
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

        public STA(IConnection con, string raw)
            : base(con, raw)
        {
            if (param == null)
                return;
            if (param.Count >= 2 && param[0].Length == 3)
            {
                severity = param[0].Substring(0, 1);
                code = param[0].Substring(1);
                content = AdcProtocol.ConvertIncomming(param[1]);
                valid = true;
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

        public STA(IConnection con, string code, string description, string param)
            : this(con, null, null, code, description, param) { }

        public STA(IConnection con, string userId, string meId, string code, string description, string param)
            : base(con, null)
        {
            if (param != null)
                param = " " + param;
            else
                param = string.Empty;
            if (!string.IsNullOrEmpty(userId))
                Raw = string.Format("DSTA {0} {1} {2} {3}{4}\n", meId,userId, code, AdcProtocol.ConvertOutgoing(description), param);
            else
                Raw = string.Format("CSTA {0} {1}{2}\n", code, AdcProtocol.ConvertOutgoing(description), param);
        }
    }
    public class GPA : AdcBaseMessage
    {
        protected string randomData = null;

        public string RandomData
        {
            get { return randomData; }
        }

        public GPA(IConnection con, string raw)
            : base(con, raw)
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
