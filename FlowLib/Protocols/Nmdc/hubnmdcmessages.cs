
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

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Protocols.HubNmdc
{
    #region Receive AND Send
    public class To : HubMessage
    {
        protected string content = null;
        public string Content
        {
            get { return content; }
        }
        // for receiving
        public To(Hub hub, string raw)
            : base(hub, raw)
        {
            //$To: Flow84 From: PIP $<PIP> Hejsan|
            int pos1, pos2;
            if (raw.StartsWith("$To: ") && (pos1 = raw.IndexOf(" From: ")) != -1)
            {
                to = raw.Substring(5, pos1 - 5);
                if ((pos2 = raw.IndexOf(" $", pos1)) != -1)
                {
                    pos1 += 7;
                    from = raw.Substring(pos1, pos2 - pos1);
                    pos2 += 2;
                    content = raw.Substring(pos2);
                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(content))
                        valid = true;
                }
            }
        }
        // for sending 
        public To(Hub hub, string to, string content)
            : base(hub, null)
        {
            this.from = hub.Me.DisplayName;
            this.to = to;
            this.content = "<" + this.from + "> " + content;
            this.Raw = "$To: " + this.to + " From: " + this.from + " $" + this.content + "|";
            IsValid = true;
            //$To: Flow84 From: PIP $<PIP> Hejsan|
        }
    }
    public class MainChat : HubMessage
    {
        protected string content = null;
        public string Content
        {
            get { return content; }
        }
        // for receiving
        public MainChat(Hub hub, string raw)
            : base(hub, raw)
        {
            if (raw.StartsWith("<"))
            {
                string tmp = raw.Substring(1);
                int pos = 0;
                // If first is on a pos more then max allowed nick size(35), ignore it.
                if ((pos = tmp.IndexOf("> ")) > 35 || pos < 1)
                {
                    content = raw;
                }
                else
                {
                    from = tmp.Substring(0, pos);
                    content = tmp.Substring(pos + 2);
                }
                // This is a real Main chat msg as we have from.
                // But as none follows standard we will allow msg even if it not a valid mainchat msg starting with <
                //if (!string.IsNullOrEmpty(from))
                //    valid = true;
            }
            else
            {
                content = raw;
            }
            valid = true;
        }
        // for sending
        public MainChat(Hub hub, string from, string content)
            : base(hub, null)
        {
            // TODO : Fix so we dont have from as param as we is not using it.
            // We are just using this from to specify what constructor to use... we know it is ugly.
            this.content = content;
            this.from = from;
            Raw = "<" + this.from + "> " + this.content + "|";
            IsValid = true;
        }
    }
    public class Supports : HubMessage
    {
        protected string[] support = null;
        protected bool tls = false;

        public bool SupportTLS
        {
            get { return tls; }
        }

        public string[] Support
        {
            get { return support; }
        }

        public new string Raw
        {
            get { return base.Raw; }
            set
            {
                base.Raw = value;
                int pos;
                if ((pos = raw.IndexOf(" ")) != -1)
                {
                    pos++;
                    string tmp = raw.Substring(pos);
                    support = tmp.Split(' ');
                    foreach (string sup in support)
                    {
                        switch (sup.ToLower())
                        {
                            case "tls":
                                tls = true;
                                break;
                            default:
                                break;
                        }
                    }
                    IsValid = true;
                }

            }
        }

        /// <summary>
        /// This Constructor is for sending
        /// </summary>
        /// <param name="hub"></param>
        public Supports(Hub hub)
            : base(hub, null)
        {
            if (hub.Me.ContainsKey(UserInfo.SECURE))
                Raw = "$Supports NoHello NoGetINFO TLS |";
            else
                Raw = "$Supports NoHello NoGetINFO |";
            IsValid = true;
        }

        /// <summary>
        /// This Constructor is for receiving
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="raw"></param>
        public Supports(Hub hub, string raw)
            : base(hub, null)
        {
            Raw = raw;
        }
    }
    public class MyINFO : HubMessage
    {
        [System.Flags]
        public enum UserStatusFlag
        {
            Unknown = 0,
            Normal = 1,
            Away = 2,
            Server = 4,
            Fireball = 8,
            TLS = 16
        }

        public static UserStatusFlag ConvertByteToStatusFlag(byte value)
        {
            UserStatusFlag flag = UserStatusFlag.Unknown;
            if ((1 & value) == 1)
                flag |= UserStatusFlag.Normal;
            if ((2 & value) == 2)
                flag |= UserStatusFlag.Away;
            if ((4 & value) == 4)
                flag |= UserStatusFlag.Server;
            if ((8 & value) == 8)
                flag |= UserStatusFlag.Fireball;
            if ((16 & value) == 16)
                flag |= UserStatusFlag.TLS;
            return flag;
        }

        public static byte ConvertStatusFlagToByte(UserStatusFlag statusFlag)
        {
            byte statusFlagRawByte = 0;
            if ((UserStatusFlag.Normal & statusFlag) == UserStatusFlag.Normal)
                statusFlagRawByte |= 1;
            if ((UserStatusFlag.Away & statusFlag) == UserStatusFlag.Away)
                statusFlagRawByte |= 2;
            if ((UserStatusFlag.Server & statusFlag) == UserStatusFlag.Server)
                statusFlagRawByte |= 4;
            if ((UserStatusFlag.Fireball & statusFlag) == UserStatusFlag.Fireball)
                statusFlagRawByte |= 8;
            if ((UserStatusFlag.TLS & statusFlag) == UserStatusFlag.TLS)
                statusFlagRawByte |= 16;
            return statusFlagRawByte;
        }

        protected UserInfo info = null;
        protected byte statusFlag;

        public byte StatusFlag
        {
            get { return statusFlag; }
            set { statusFlag = value; }
        }

        public UserInfo UserInfo
        {
            get { return info; }
        }
        // Receiving
        public MyINFO(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos1, pos2;
            if ((pos1 = raw.IndexOf("$MyINFO $ALL ")) != -1)
            {
                if ((pos2 = raw.IndexOf(" ", 13)) > 13)
                {
                    from = raw.Substring(13, pos2 - 13);
                    // Zline<Z++ V:2.00,M:P,H:4/4/25,S:2>$ $DSL$twizmflow@gmail.com$0$
                    string temp = raw.Substring(++pos2);
                    string[] sections = temp.Split('$');
                    info = new UserInfo();
                    info.DisplayName = from;
                    info.Set(UserInfo.STOREID, hub.HubSetting.Address + hub.HubSetting.Port.ToString() + from);
                    if (sections.Length == 6)
                    {
                        int pos = 0;
                        if ((pos = sections[0].LastIndexOf('<')) != -1)
                        {
                            info.Description = sections[0].Substring(0, pos);
                            // <Z++ V:2.00,M:P,H:4/4/25,S:2>
                            info.TagInfo.Tag = sections[0].Substring(pos);
                            // Parsing of tag
                        }
                        else
                        {
                            info.Description = sections[0];
                        }
                        info.Connection = sections[2];
                        if (info.Connection.Length > 0)
                        {
                            statusFlag = (byte)info.Connection[info.Connection.Length - 1];
                            UserStatusFlag flag = ConvertByteToStatusFlag(statusFlag);
                            if ((flag & UserStatusFlag.TLS) == UserStatusFlag.TLS)
                                info.Set(UserInfo.SECURE, "");
                        }

                        info.Email = sections[3];
                        info.Share = sections[4];

                    }
                    if (!string.IsNullOrEmpty(info.DisplayName))
                        IsValid = true;
                }
            }
        }
        // Sending
        public MyINFO(Hub hub)
            : this(hub, ConvertStatusFlagToByte(UserStatusFlag.Normal))
        { }

        public MyINFO(Hub hub, byte flg)
            : base(hub, null)
        {
            this.info = hub.Me;
            this.statusFlag = flg;
            System.Text.StringBuilder tag = new System.Text.StringBuilder();
            tag.Append("<");
            tag.Append(info.TagInfo.Version);
            tag.Append(",M:");
            switch (info.TagInfo.Mode) {
                case Enums.ConnectionTypes.Socket5: // Socket5
                    tag.Append("5"); break;
                case Enums.ConnectionTypes.Direct: // Active
                    tag.Append("A"); break;
                case Enums.ConnectionTypes.Passive: // Passive
                default:
                    tag.Append("P"); break;
            }
            tag.Append(",H:");
            tag.Append(info.TagInfo.Normal.ToString() + "/");   // Normal
            tag.Append(info.TagInfo.Regged.ToString() + "/");   // Regged
            tag.Append(info.TagInfo.OP.ToString() + "");    // OP
            tag.Append(",S:" + (info.TagInfo.Slots == -1 ? 0 : info.TagInfo.Slots).ToString() + ">");

            UserStatusFlag status = ConvertByteToStatusFlag(statusFlag);
            if (info.ContainsKey(UserInfo.SECURE))
                status |= UserStatusFlag.TLS;

            Raw = "$MyINFO $ALL "
                + info.DisplayName
                + " "
                + info.Description
                + tag
                + "$ $"
                + info.Connection
                + hub.Protocol.Encoding.GetString(new byte[] { ConvertStatusFlagToByte(status) }) + "$"
                + info.Email + "$"
                + (string.IsNullOrEmpty(info.Share) ? "0" : info.Share)
                + "$|";
            IsValid = true;
        }

    }
    #endregion
    #region Receive
    public class Lock : HubMessage
    {
        protected string key = null;
        public string Key
        {
            get { return key; }
        }
        public Lock(Hub hub, string raw) : base(hub, raw)
        {
            /*********
             * This code has been copied from the CoreDC project.
             * Many thanks :D
             *********/

            string lck = raw.Replace("$Lock ", "");
            int iPos = lck.IndexOf(" Pk=", 1);
            if (iPos > 0) lck = lck.Substring(0, iPos);

            char[] arrChar = new char[lck.Length];
            int[] arrRet = new int[lck.Length];
            arrChar[0] = lck[0];
            int tmp = lck[0];
            for (int i = 1; i < lck.Length; i++)
            {
                //arrChar[i] = lck[i];
                byte[] test = hub.Protocol.Encoding.GetBytes(new char[] { lck[i] });
                arrChar[i] = (char)test[0];
                arrRet[i] = arrChar[i] ^ arrChar[i - 1];
            }
            arrRet[0] = arrChar[0] ^ arrChar[lck.Length - 1] ^ arrChar[lck.Length - 2] ^ 5;
            string sKey = "";
            for (int n = 0; n < lck.Length; n++)
            {
                arrRet[n] = ((arrRet[n] * 16 & 240)) | ((arrRet[n] / 16) & 15);
                int j = arrRet[n];
                switch (j)
                {
                    case 0:
                    case 5:
                    case 36:
                    case 96:
                    case 124:
                    case 126:
                        sKey += string.Format("/%DCN{0:000}%/", j);
                        break;
                    default:
                        sKey += hub.Protocol.Encoding.GetChars(new byte[] { System.Convert.ToByte((char)j) })[0];
                        break;
                }
            }
            key = sKey;
            if (!string.IsNullOrEmpty(key))
                IsValid = true;
        }

    //    protected static char Chr(byte src)
    //    {
    //        return (hub.Protocol.Encoding.GetChars(new byte[] { src })[0]);
    //    }
    }
    /// <summary>
    /// Class name is NOT a spell mistake.
    /// Nmdc protocol command is spelled like this so we will too :)
    /// </summary>
    public class ValidateDenide : HubMessage
    {
        public ValidateDenide(Hub hub, string raw) : base(hub, raw) { IsValid = true; }
    }
    public class GetPass : HubMessage
    {
        public GetPass(Hub hub, string raw) 
            : base(hub, raw)
        {
            IsValid = true;
        }
    }

    public class Search : HubMessage
    {
        protected SearchInfo info = null;
        protected System.Net.IPEndPoint address = null;

        public System.Net.IPEndPoint Address
        {
            get { return address; }
            set { address = value; }
        }

        public SearchInfo Info
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

        public Search(Hub hub, string raw)
            : base(hub, raw)
        {
            int searchPos = 0;
            #region Get From
            int pos1 =0, pos2 = 0;
            if (Utils.StringOperations.Find(raw, "Hub:", " ", ref pos1, ref pos2))
            {
                searchPos = pos2;
                pos1 += 4;
                pos2 -= 1;
                from = raw.Substring(pos1, pos2 - pos1);
            }
            else if (((pos1=0) == 0) && Utils.StringOperations.Find(raw, "$Search ", " ", ref pos1, ref pos2))
            {
                searchPos = pos2;
                pos1 += 8;
                pos2 -= 1;
                string[] tmp = raw.Substring(pos1, pos2 - pos1).Split(':');
                if (tmp.Length == 2)
                {
                    System.Net.IPAddress ip = null;
                    int port = -1;
                    try
                    {
                        ip = System.Net.IPAddress.Parse(tmp[0]);
                    }
                    catch { }
                    try
                    {
                        port = int.Parse(tmp[1]);
                        if (port < 0 || port > 65535)
                            port = 0;
                    }
                    catch { }

                    if (ip != null)
                    {
                        address = new System.Net.IPEndPoint(ip, port);
                    }
                }
            }
            #endregion
            #region Search Info
            if (searchPos != 0)
            {
                info = new SearchInfo();
                string[] sections = raw.Substring(searchPos).Split('?');
                if (sections.Length == 5)
                {
                    #region Size Info
                    info.Set(SearchInfo.SIZETYPE, "0");
                    if (sections[0] == "T")
                    {
                        info.Set(SearchInfo.SIZETYPE, (sections[1] == "F" ? "1" : "2"));
                    }
                    long size = 0;
                    try
                    {
                        size = long.Parse(sections[2]);
                    }
                    catch { size = 0; }
                    info.Set(SearchInfo.SIZE, size.ToString());
                    #endregion
                    #region Extention
                    string ext = string.Empty;
                    System.Text.StringBuilder sb = new System.Text.StringBuilder(7);
                    switch (sections[3])
                    {
                        case "2":       // Audio
                            sb.Append("mp3 ");
                            sb.Append("mp2 ");
                            sb.Append("wav ");
                            sb.Append("au ");
                            sb.Append("rm ");
                            sb.Append("mid ");
                            sb.Append("sm");
                            break;
                        case "3":       // Compressed
                            sb.Append("zip ");
                            sb.Append("arj ");
                            sb.Append("rar ");
                            sb.Append("lzh ");
                            sb.Append("gz ");
                            sb.Append("z ");
                            sb.Append("arc ");
                            sb.Append("pak");
                            break;
                        case "4":       // Documents
                            sb.Append("doc ");
                            sb.Append("txt ");
                            sb.Append("wri ");
                            sb.Append("pdf ");
                            sb.Append("ps ");
                            sb.Append("tex");
                            break;
                        case "5":       // Executables
                            sb.Append("exe ");
                            sb.Append("pm ");
                            sb.Append("bat ");
                            sb.Append("com");
                            break;
                        case "6":       // Pictures
                            sb.Append("gif ");
                            sb.Append("jpg ");
                            sb.Append("jpeg ");
                            sb.Append("bmp ");
                            sb.Append("pcx ");
                            sb.Append("png ");
                            sb.Append("wmf ");
                            sb.Append("psd");
                            break;
                        case "7":       // Videos
                            sb.Append("mpg ");
                            sb.Append("mpeg ");
                            sb.Append("avi ");
                            sb.Append("asf ");
                            sb.Append("mov");
                            break;
                        case "8":       // Folders
                            sb.Append("$0");
                            info.Set(SearchInfo.TYPE, "1");
                            break;
                        case "9":       // TTH Search
                            sb.Append("$1");
                            info.Set(SearchInfo.TYPE, "2");
                            break;
                    }
                    info.Set(SearchInfo.EXTENTION, sb.ToString());
                    #endregion
                    #region Search String
                    if (sections[4].StartsWith("TTH:"))
                        info.Set(SearchInfo.SEARCH, sections[4].Substring(4));
                    else
                        info.Set(SearchInfo.SEARCH, sections[4]);
                    #endregion
                    valid = true;
                }
            }
            #endregion
        }

        public Search(Hub hub, SearchInfo info)
            : base(hub,null)
        {
            this.info = info;

            #region Id
            string id = null;
            switch (hub.Me.Mode)
            {
                case FlowLib.Enums.ConnectionTypes.Direct:
                case FlowLib.Enums.ConnectionTypes.UPnP:
                case FlowLib.Enums.ConnectionTypes.Forward:
                    string port = hub.Share.Port.ToString();
                    if (hub.Me.ContainsKey(UserInfo.UDPPORT))
                        port = hub.Me.Get(UserInfo.UDPPORT);

                    if (hub.Me.ContainsKey(UserInfo.IP))
                    {
                        id = string.Format("{0}:{1}", hub.Me.Get(UserInfo.IP), port);
                    }
                    else
                    {
                        id = string.Format("{0}:{1}", hub.LocalAddress.Address, port);
                    }
                    break;
                default:
                    id = string.Format("Hub:{0}", hub.Me.ID);
                    break;
            }
            #endregion
            string search = string.Empty;
            #region Size
            string size = "F?F?0";
            if (info.ContainsKey(SearchInfo.SIZETYPE))
            {
                switch (info.Get(SearchInfo.SIZETYPE))
                {
                    case "1":     // Min Size
                        size = "T?F" + info.Get(SearchInfo.SIZE);
                        break;
                    case "2":     // Max Size
                        size = "T:T" + info.Get(SearchInfo.SIZE);
                        break;
                }
            }
            #endregion
            #region Extention
            string type = "?1";
            switch (info.Get(SearchInfo.EXTENTION))
            {
                case "mp3":
                case "mp2":
                case "wav":
                case "au":
                case "rm":
                case "mid":
                case "sm":
                    type = "?2";
                    break;
                case "zip":
                case "arj":
                case "rar":
                case "lzh":
                case "gz":
                case "z":
                case "arc":
                case "pak":
                    type = "?3";
                    break;
                case "doc":
                case "txt":
                case "wri":
                case "pdf":
                case "ps":
                case "tex":
                    type = "?4";
                    break;
                case "pm":
                case "exe":
                case "bat":
                case "com":
                    type = "?5";
                    break;
                case "gif":
                case "jpg":
                case "jpeg":
                case "bmp":
                case "pcx":
                case "png":
                case "wmf":
                case "psd":
                    type = "?6";
                    break;
                case "mpg":
                case "mpeg":
                case "avi":
                case "asf":
                case "mov":
                    type = "?7";
                    break;
                case "$0":
                    type = "?8";
                    break;
                case "$1":
                    type = "?9";
                    break;
            }
            #endregion

            string schStr = info.Get(SearchInfo.SEARCH);
            schStr = schStr.Replace(" ", "$");

            #region Is TTH Search?
            switch (info.Get(SearchInfo.TYPE))
            {
                case "1":   // Directory
                    type = "?8";
                    break;
                case "2":   // TTH
                    type = "?9";
                    schStr = "TTH:" + schStr;
                    break;
            }
            #endregion

            search = string.Format("{0}{1}?{2}", size, type, schStr);
            Raw = string.Format("$Search {0} {1}|", id, search);
        }
    }

    public static class Dummy
    {
        public static string ConvertToNmdc(string str)
        {
            str = str.Replace("&", "&amp;");
            str = str.Replace("&#124;", "&#36;");
            str = str.Replace("$", "&#124;");
            return str;
        }

        public static string ConvertFromNmdc(string str)
        {
            str = str.Replace("&#124;", "$");
            str = str.Replace("&#36;", "&#124;");
            str = str.Replace("&amp;", "&");
            return str;
        }
    }

    public class SR : HubMessage
    {
        protected ContentInfo info = null;
        protected string address = null;
        protected string content = null;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
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

        public SR(Hub hub, string raw)
            : base(hub, raw)
        {
            Parse();
        }

        protected void Parse()
        {
            // Directory
            // $SR DC++0.699 Books 1/1TTH:AYAAMAAGAADAABQAAYAAEKCAAAAG633LOMAAAAA (127.0.0.1:411)
            // File
            // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv5898936 1/1TTH:QIFN7FMMSHXLRZDZUGTRZO5RWUY4IONS6JRVQSA (127.0.0.1:411)
            int pos1 = 0, pos2 = 0;
            string[] sections = raw.Split('\x005');

            if (sections.Length >= 2)
            {
                info = new ContentInfo();

                // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv
                #region User Id
                if (Utils.StringOperations.Find(sections[0], "$SR ", " ", ref pos1, ref pos2))
                {
                    pos1 += 4;
                    pos2 -= 1;
                    from = sections[0].Substring(pos1, pos2 - pos1);
                    pos2++;
                }
                #endregion

                int index = 0;
                // If file, we will have 3
                if (sections.Length == 3)
                {
                    info.Set(ContentInfo.VIRTUAL, sections[0].Substring(pos2));
                    index = 1;
                }
                else if (sections.Length == 2)
                {
                    index = 0;
                }
                int tmp;

                // Get directory name or file size
                if ((tmp = sections[index].LastIndexOf(" ")) != -1)
                {
                    if (index == 0)
                    {
                        info.Set(ContentInfo.VIRTUAL, sections[0].Substring(pos2, tmp - pos2));
                    }
                    long size;
                    try
                    {
                        size = long.Parse(sections[index].Substring(0, tmp));
                        info.Size = size;
                    }
                    catch { }
                }
                index++;
                pos1 = 0;
                pos2 = 0;
                // Content & Address
                if (Utils.StringOperations.Find(sections[index], "(", ")", ref pos1, ref pos2))
                {
                    content = sections[index].Substring(0, pos1).Trim();
                    if (content.StartsWith("TTH:"))
                    {
                        info.Set(ContentInfo.TTH, content.Substring(4));
                    }
                    pos1++;
                    pos2--;
                    address = sections[index].Substring(pos1, pos2 - pos1);
                    valid = true;
                }
            }
        }

        public SR(Hub hub, ContentInfo info, bool directoryOnly, string from)
            : base(hub, null)
        {
            this.info = info;
            // Directory
            // $SR DC++0.699 Books 1/1TTH:AYAAMAAGAADAABQAAYAAEKCAAAAG633LOMAAAAA (127.0.0.1:411)
            // File
            // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv5898936 1/1TTH:QIFN7FMMSHXLRZDZUGTRZO5RWUY4IONS6JRVQSA (127.0.0.1:411)
            string content = null;
            if (directoryOnly)
                content = System.IO.Path.GetDirectoryName(info.Get(ContentInfo.VIRTUAL));
            else
                content = info.Get(ContentInfo.VIRTUAL) + "\x005" + info.Size.ToString();
            // TODO : We are not supporting slot system right now so we will respond that we have all slots open.
            string slots = string.Format("{0}/{0}", hub.Me.TagInfo.Slots); 
            // TODO : We are not saving hub name as it is now so we cant use it.
            string hubname = string.Empty;
            if (info.ContainsKey(ContentInfo.TTH) && !directoryOnly)
                hubname = "TTH:" + info.Get(ContentInfo.TTH);
            string passive = string.Empty;
            if (!string.IsNullOrEmpty(from))
                passive = "\x005" + from;
            Raw = string.Format("$SR {0} {1} {2}\x005{3} ({4}:{5}){6}|", hub.Me.ID, content, slots, hubname, hub.RemoteAddress.Address.ToString(), hub.RemoteAddress.Port, passive);
        }
    }

    public class ConnectToMe : HubMessage
    {
        #region Variables
        protected string address = null;
        protected int port = -1;
        protected bool tls = false;
        #endregion
        #region Properties
        public string Address
        {
            get { return address; }
        }
        public int Port
        {
            get { return port; }
        }
        public bool TLS
        {
            get { return tls; }
        }
        #endregion
        public ConnectToMe(string toNick, int port, Hub hub)
            : this(toNick, port, hub, FlowLib.Enums.SecureProtocols.None)
        {

        }
        public ConnectToMe(string toNick, int port, Hub hub, Enums.SecureProtocols secProt)
            : base(hub, null)
        {
            this.to = toNick;
            this.port = port;

            this.address = hub.LocalAddress.Address.ToString();
            if (hub.Me.ContainsKey(UserInfo.IP))
                this.address = hub.Me.Get(UserInfo.IP);

            if ((Enums.SecureProtocols.TLS & secProt) == FlowLib.Enums.SecureProtocols.TLS)
                tls = true;

            Raw = "$ConnectToMe "+this.to+" " + this.address + ":" + (tls ? hub.Me.Get(UserInfo.SECURE) + "S" : this.port.ToString()) + "|";
            if (!string.IsNullOrEmpty(to) && port > 0 && port < 65535 && !string.IsNullOrEmpty(address))
                IsValid = true;
        }

        public ConnectToMe(Hub hub, string raw)
            : base(hub, raw)
        {
            // $ConnectToMe FMDC 82.182.95.201:6900
            string[] sections = raw.Split(' ');
            if (sections.Length != 3)
                return;
            to = sections[1];
            string[] address = sections[2].Split(':');
            if (address.Length != 2)
                return;
            this.address = address[0];
            try
            {
                if ((tls = address[1].EndsWith("S", System.StringComparison.OrdinalIgnoreCase)))
                    address[1] = address[1].TrimEnd('S');
                port = int.Parse(address[1]);
            }
            catch { }
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(this.address) && port > 0 && port < 65535)
                IsValid = true;
        }
    }
    public class RevConnectToMe : HubMessage
    {
        public RevConnectToMe(string remoteNick, Hub hub)
            : base(hub, null)
        {
            from = hub.Me.DisplayName;
            to = remoteNick;
            Raw = "$RevConnectToMe " + from + " " + to + "|";
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                IsValid = true;
        }

        public RevConnectToMe(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                if ((pos2 = raw.IndexOf(" ",++pos)) != -1)
                {
                    from = raw.Substring(pos, pos2++ -pos);
                    to = raw.Substring(pos2);
                    if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                        IsValid = true;
                }
            }
        }
    }
    public class ForceMove : HubMessage
    {
        protected string address = null;
        public string Address
        {
            get { return address; }
        }
        public ForceMove(Hub hub, string raw)
            : base(hub, raw)
        {
            // $ForceMove flowlib.dummy.org
            string[] com = raw.Split(' ');
            if (com.Length != 2)
                return;
            this.address = com[1];
            if (!string.IsNullOrEmpty(address))
                IsValid = true;
        }
    }
    /// <summary>
    /// User Disconnected from hub.
    /// From will contain the username that quited.
    /// </summary>
    public class Quit : HubMessage
    {
        public Quit(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                this.from = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(from))
                    IsValid = true;
            }
        }
    }
    public class NickList : HubMessage
    {
        protected string[] nicks = null;
        public string[] List
        {
            get { return nicks; }
        }
        public NickList(Hub hub, string raw)
            : base(hub, raw)
        {
            // $NickList <nick1>$$<nick2>$$<nick3>$$
            int pos1;
            if ((pos1 = raw.IndexOf(" ")) != -1)
            {
                string tmp = raw.Substring(pos1);
                char[] test = { '$', '$' };
#if !COMPACT_FRAMEWORK
                nicks = tmp.Split(test, System.StringSplitOptions.RemoveEmptyEntries);
#else
                System.Collections.Generic.List<string> lst = new System.Collections.Generic.List<string>(tmp.Split(test));
                while (lst.Remove(string.Empty)) { }
                nicks = lst.ToArray();
#endif
                if (nicks.Length > 0)
                    IsValid = true;
            }
        }
    }
    /// <summary>
    /// Indicates that current user is an operator.
    /// Username will be in To.
    /// </summary>
    public class LogedIn : HubMessage
    {
        public LogedIn(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                this.to = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(to))
                    IsValid = true;
            }
        }
    }
    public class OpList : HubMessage
    {
        protected string[] ops = null;
        public string[] List
        {
            get { return ops; }
        }
        public OpList(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos1;
            if ((pos1 = raw.IndexOf(" ")) != -1)
            {
                string tmp = raw.Substring(++pos1);
                char[] test = { '$', '$' };
#if !COMPACT_FRAMEWORK
                ops = tmp.Split(test, System.StringSplitOptions.RemoveEmptyEntries); // command.Split("$$", StringSplitOptions.RemoveEmptyEntries);
#else
                System.Collections.Generic.List<string> lst = new System.Collections.Generic.List<string>(tmp.Split(test));
                while (lst.Remove(string.Empty)) { }
                ops = lst.ToArray();
#endif
            }
            IsValid = true;
        }
    }
    public class Hello : HubMessage
    {
        public Hello(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                from = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(from))
                    IsValid = true;
            }
        }
    }
    public class HubName : HubMessage
    {
        #region Variables
        protected string content = string.Empty;
        protected string name = string.Empty;
        protected string topic = string.Empty;
        #endregion
        #region Properties
        public string Content
        {
            get { return content; }
        }
        public string Name
        {
            get { return name; }
        }
        public string Topic
        {
            get { return topic; }
        }
        #endregion
        public HubName(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                content = raw.Substring(pos);
                if ((pos2 = content.IndexOf(" - ")) != -1)
                {
                    name = content.Substring(0, pos2);
                    topic = content.Substring(pos2 + 3);
                }
                else
                {
                    name = content;
                }
            }
            IsValid = true;
        }
    }
    #endregion
    #region Send
    public class Key : HubMessage
    {
        protected string key = null;
        public string Content
        {
            get { return key; }
        }
     
        public Key(Hub hub, string key)
            : base(hub, null)
        {
            this.key = key;
            Raw = "$Key " + Content + "|";
            if (!string.IsNullOrEmpty(key))
                IsValid = true;
        }
    }
    public class Version : HubMessage
    {
        protected string version;
        public string Content
        {
            get { return version; }
        }
        public Version(Hub hub)
            : base(hub, null)
        {
            version = "1.0091";
            Raw = "$Version "+version+"|";
            IsValid = true;
        }
    }
    public class GetNickList : HubMessage
    {
        public GetNickList(Hub hub) 
            : base(hub, null)
        {
            Raw = "$GetNickList|";
            IsValid = true;
        }
    }
    public class MyPass : HubMessage
    {
        protected string pass = null;
        public string Password
        {
            get { return pass; }
            set { 
                pass = value;
                Raw = "$MyPass "+pass+"|";
                IsValid = !string.IsNullOrEmpty(pass);
            }
        }
        public MyPass(Hub hub)
            : base(hub, null)
        {
            Password = hub.HubSetting.Password;
        }
        public MyPass(Hub hub, string pass)
            : base(hub, null)
        {
            Password = pass;
        }
    }
    public class ValidateNick : HubMessage
    {
        public ValidateNick(Hub hub)
            : base(hub, null)
        {
            from = hub.HubSetting.DisplayName;
            Raw = "$ValidateNick " + from + "|";
            if (!string.IsNullOrEmpty(from))
                IsValid = true;
        }
    }
    #endregion
}
