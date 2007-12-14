
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
                    return;
                from = tmp.Substring(0, pos);
                content = tmp.Substring(pos + 2);
                if (!string.IsNullOrEmpty(from))
                    valid = true;
            }
            else
            {
                content = raw;
            }
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
        public string[] Support
        {
            get { return support; }
        }
        /// <summary>
        /// This Constructor is for sending
        /// </summary>
        /// <param name="hub"></param>
        public Supports(Hub hub)
            : base(hub, null)
        {
            Raw = "$Supports NoHello NoGetINFO |";
            IsValid = true;
        }

        /// <summary>
        /// This Constructor is for receiving
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="raw"></param>
        public Supports(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                string tmp = raw.Substring(pos);
                support = tmp.Split(' ');
                IsValid = true;
            }
        }
    }
    public class MyINFO : HubMessage
    {
        protected UserInfo info = null;
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
                    if (sections.Length == 6)
                    {
                        int pos = 0;
                        if ((pos = sections[0].LastIndexOf('<')) != -1)
                        {
                            info.Description = sections[0].Substring(0, pos);
                            info.TagInfo.Tag = sections[0].Substring(pos);
                            // <Z++ V:2.00,M:P,H:4/4/25,S:2>
                            // Parsing of tag
                            string[] tagsections = info.TagInfo.Tag.Split(',');
                            if (tagsections.Length >= 4)
                            {
                                switch (tagsections[1])
                                {
                                    case "M:A":
                                        info.Mode = Enums.ConnectionTypes.Direct;
                                        break;
                                    case "M:P":
                                        info.Mode = Enums.ConnectionTypes.Passive;
                                        break;
                                    case "M:5":
                                        info.Mode = Enums.ConnectionTypes.Socket5;
                                        break;
                                }
                            }

                        }
                        else
                        {
                            info.Description = sections[0];
                        }
                        info.Connection = sections[2];
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
            : base(hub, null)
        {
            this.info = hub.Me;
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
            tag.Append(",S:" + info.TagInfo.Slots.ToString() + ">");

            Raw = "$MyINFO $ALL "
                + info.DisplayName
                + " "
                + info.Description
                + tag
                + "$ $"
                + info.Connection
                + "$"      // TODO : Add User status (Normal, away, server, fireball and so on)
                + info.Email + "$"
                + info.Share
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
                byte[] test = System.Text.Encoding.Default.GetBytes(new char[] { lck[i] });
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
                        sKey += System.Text.Encoding.Default.GetChars(new byte[] { System.Convert.ToByte((char)j) })[0];
                        break;
                }
            }
            key = sKey;
            if (!string.IsNullOrEmpty(key))
                IsValid = true;
        }

        protected static char Chr(byte src)
        {
            return (System.Text.Encoding.Default.GetChars(new byte[] { src })[0]);
        }
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
    public class ConnectToMe : HubMessage
    {
        #region Variables
        string address = null;
        int port = -1;
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
        #endregion
        public ConnectToMe(string toNick, int port, Hub hub)
            : base(hub, null)
        {
            this.to = toNick;
            this.port = port;

            this.address = hub.LocalAddress.Address.ToString();
            
            Raw = "$ConnectToMe "+this.to+" " + this.address + ":" + this.port+ "|";
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
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(this.address) && int.TryParse(address[1], out port) && port > 0 && port < 65535)
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
            // $ForceMove porno.zapto.org
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
                nicks = tmp.Split(test);
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
                ops = tmp.Split(test); // command.Split("$$", StringSplitOptions.RemoveEmptyEntries);
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
