
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

namespace FlowLib.Protocols.Adc
{
    #region Receive AND Send
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
            string[] sections = param.Split(' ');
            if (sections.Length == 1)
                content = sections[0];
            else if (sections.Length == 2)
            {
                from = sections[0];
                content = sections[1];
            }
            else if (sections.Length >= 3)
            {
                from = sections[0];
                to = sections[1];
                content = sections[2];
                for (int i = 3; i < sections.Length; i++)
                {
                    if (sections[i].StartsWith("PM"))
                        pmgroup = sections[i].Substring(2);
                    else if (sections[i].Equals("ME1"))
                        me = true;
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
            this.Raw = "BMSG " + this.from + " " + HubAdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + "\n";
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
            this.from = hub.Me.SID;
            if (this.to == null || this.pmgroup == null)
            {
                if (this.pmgroup != null)
                    this.to = this.pmgroup;
                else
                    return;
            }
            // DMSG SHJV SH5B PIIP!! PMSHJV
            this.Raw = "DMSG " + this.from + " " + this.to + " " + HubAdcProtocol.ConvertOutgoing(this.content) + (this.me ? " ME1" : "") + ((this.pmgroup != null) ? (" PM" + this.pmgroup) : this.to) + "\n";
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
            // IINF is from hub. all other should be from user =)
            // NIDCDev\sPublic HU1 HI1 DEThe\spublic\sDirect\sConnect\sdevelopment\shub VEADCH++\sv2.0.0-Release
            // TUOD SF0 SL1 SS0 SUTCP4,UDP4 DEPASIV HN4 HO0 HR0 I489.38.33.162 U41090 IDARJQDWZKC4MMC7PLQNOYSPHVI7V62QPS4IRF5KA EMLIVIUANDREI70@YAHOO.COM US104857600 VE++\s0.698 NI[RO][B][QUICK-NET]LIVIU
            string[] sections = param.Split(' ');
            int i = 0;
            if (this.Type == "B")
            {
                from = sections[0];
                info.SID = from;
                i = 1;
            }
            for (; i < sections.Length; i++)
            {
                if (sections[i].Length < 2)
                    continue;
                string key = sections[i].Substring(0, 2);
                string value = sections[i].Substring(2);
                switch (key)
                {
                    case "ID":
                        info.CID = value;
                        break;
                    case "PD":
                        break;
                    case "I4":
                        info.IP = value;
                        break;
                    case "I6":
                        info.IP = value;
                        break;
                    case "U4":
                        break;
                    case "U6":
                        break;
                    case "SS":
                        info.Share = value;
                        break;
                    case "SF":
                        break;
                    case "US":
                        break;
                    case "DS":
                        break;
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
                    case "AS":
                        break;
                    case "AM":
                        break;
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
                    case "TO":
                        break;
                    case "OP":
                        info.IsOperator = (value == "1");
                        break;
                    case "AW":
                        break;
                    case "BO":
                        break;
                    case "HI":
                        break;
                    case "HU":
                        break;
                    case "SU":
                        break;
                    case "NI":
                        info.DisplayName = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "VE":
                        info.TagInfo.Version = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    case "DE":
                        info.Description = HubAdcProtocol.ConvertIncomming(value);
                        break;
                    default:
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
            sb.Append("BINF " + info.SID);
            sb.Append(" ID" + info.CID);
            sb.Append(" PD" + info.PID);
            sb.Append(" DE" + HubAdcProtocol.ConvertOutgoing(info.Description));

            sb.Append(" HN" + info.TagInfo.Normal.ToString());
            sb.Append(" HO" + info.TagInfo.OP.ToString());
            sb.Append(" HR" + info.TagInfo.Regged.ToString());

            // TODO: Add check for TCP-IP 4 or 6
            sb.Append(" I40.0.0.0");
            sb.Append(" NI" + HubAdcProtocol.ConvertOutgoing(info.DisplayName));
            sb.Append(" SL" + info.TagInfo.Slots); // Upload Slots Open
            sb.Append(" SF" + (Hub.Share != null ? Hub.Share.HashedCount : 0));  // Shared Files
            sb.Append(" SS" + (Hub.Share != null ? Hub.Share.HashedSize : 0));    // Share Size in bytes
            //sb.Append(" SU" + "ADC0,TCP4,UDP4 U4-6536");  // TODO : Add Support
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
            // TODO : Add Encryption here.
        }
    }
    #endregion
    #region Receive
    public class SID : AdcBaseMessage
    {
        public SID(Hub hub, string raw)
            : base(hub, raw)
        {

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
            string[] sections = param.Split(' ');
            from = sections[0];
            for (int i = 1; i < sections.Length; i++)
            {
                if (sections[i].Length < 2)
                    continue;
                string key = sections[i].Substring(0, 2);
                string value = sections[i].Substring(2);
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
            string[] sections = param.Split(' ');
            if (sections[0].Length == 3)
            {
                severity = sections[0].Substring(0, 1);
                code = sections[0].Substring(1);
            }
            for (int i = 1; i < sections.Length; i++)
            {
                if (sections[i].Length < 2)
                    continue;
                string key = sections[i].Substring(0, 2);
                string value = sections[i].Substring(2);
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
    }
    public class GPA : AdcBaseMessage
    {
        public GPA(Hub hub, string raw) : base(hub, raw) { }
    }
    #endregion
}
