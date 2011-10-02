using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
}