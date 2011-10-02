using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
}