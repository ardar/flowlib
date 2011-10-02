using Flowertwig.Utils;
using FlowLib.Connections.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
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
            bool validAddress = false;

            int searchPos = 0;
            #region Get From
            int pos1 =0, pos2 = 0;
            if (StringOperations.Find(raw, "Hub:", " ", ref pos1, ref pos2))
            {
                searchPos = pos2;
                pos1 += 4;
                pos2 -= 1;
                from = raw.Substring(pos1, pos2 - pos1);

                validAddress = true;
            }
            else if (((pos1=0) == 0) && StringOperations.Find(raw, "$Search ", " ", ref pos1, ref pos2))
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

                    int.TryParse(tmp[1], out port);
                    //if (port < 	 || port > System.Net.IPEndPoint.MaxPort)
                    //    port = -1;

                    if (ip != null && (System.Net.IPEndPoint.MinPort <= port &&  port <= System.Net.IPEndPoint.MaxPort))
                    {
                        address = new System.Net.IPEndPoint(ip, port);
                        validAddress = true;
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

                    // A search is only valid if it has a valid Adress
                    if (validAddress)
                    {
                        valid = true;
                    }
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
                case ConnectionTypes.Direct:
                case ConnectionTypes.UPnP:
                case ConnectionTypes.Forward:
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
}