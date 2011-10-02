using System.Text;
using Flowertwig.Utils.Hashing;
using FlowLib.Connections.Interfaces;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
        public INF(IDirectConnectConnection con, string raw)
            : base(con, raw)
        {
            if (param == null)
                return;
            info.TagInfo.GenerateTag = true;
            info.Set(UserInfo.SID, id);
            // We set connection mode to unknown until we get a SU field.
            info.TagInfo.Mode = ConnectionTypes.Unknown;

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
                        // Set connection to passive by default.
                        info.TagInfo.Mode = ConnectionTypes.Passive;
                        foreach (string sup in supports)
                        {
                            switch (sup)
                            {
                                case "ADC0":
                                    info.Set(UserInfo.SECURE, "");
                                    break;
                                case "TCP4":
                                case "TCP6":
                                    info.TagInfo.Mode = ConnectionTypes.Direct;
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
        public INF(IDirectConnectConnection con, UserInfo info)
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
            var sb = new StringBuilder(inf1.Type + inf1.Action);
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
            return new INF(inf1.DcConnection, sb.ToString());
        }

        private void CreateRaw()
        {
            if (info == null)
                return;
            var sb = new StringBuilder();
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
                var tiger = new Tiger();
                byte[] data = null;
                if (!info.ContainsKey(UserInfo.PID))
                {
                    data = tiger.ComputeHash(Encoding.UTF8.GetBytes("FlowLib" + System.DateTime.Now.Ticks));
                    info.Set(UserInfo.PID, Base32.Encode(data));
                }
                // Make CID from PID. This will always be done.
                data = tiger.ComputeHash(Base32.Decode(info.Get(UserInfo.PID)));
                info.Set(UserInfo.CID, Base32.Encode(data));

                sb.Append(" ID" + info.Get(UserInfo.CID));
                sb.Append(" PD" + info.Get(UserInfo.PID));
                sb.Append(" DE" + AdcProtocol.ConvertOutgoing(info.Description));

                sb.Append(" HN" + info.TagInfo.Normal);
                sb.Append(" HO" + info.TagInfo.OP);
                sb.Append(" HR" + info.TagInfo.Regged);
                sb.Append(" NI" + AdcProtocol.ConvertOutgoing(info.DisplayName));
                sb.Append(" SL" + info.TagInfo.Slots); // Upload Slots Open
                sb.Append(" SF" + (DcConnection.Share != null ? DcConnection.Share.HashedCount : 0));  // Shared Files
                sb.Append(" SS" + (DcConnection.Share != null ? DcConnection.Share.HashedSize : 0));    // Share Size in bytes
                if (DcConnection.Share != null)
                {
                    var support = new StringBuilder();
                    switch (info.TagInfo.Mode)
                    {
                        case ConnectionTypes.Direct:
                        case ConnectionTypes.UPnP:
                        case ConnectionTypes.Forward:
                            string ip = info.Get(UserInfo.IP);
                            if (string.IsNullOrEmpty(ip) || ip.Contains("."))
                            {
                                if (string.IsNullOrEmpty(ip))
                                    sb.Append(" I40.0.0.0");
                                else
                                    sb.Append(" I4" + ip);
                                support.Append("TCP4,");    // Support
                                support.Append("UDP4,");    // Support
                                sb.Append(" U4" + DcConnection.Share.Port);
                            }
                            else
                            {
                                sb.Append(" I6" + ip);
                                support.Append("TCP6,");    // Support
                                support.Append("UDP6,");    // Support
                                sb.Append(" U6" + DcConnection.Share.Port);
                            }
                            break;
                    }

                    support.Append("BZIP,");         // Support
                    if (info.ContainsKey(UserInfo.SECURE))
                        support.Append("ADC0,");         // Support
                    sb.Append(" SU" + support);  // Support
                }
                sb.Append(" VE" + AdcProtocol.ConvertOutgoing(info.TagInfo.Version));
            }
            sb.Append("\n");
            Raw = sb.ToString();
        }
    }
}