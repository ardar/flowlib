using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
}