using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
}