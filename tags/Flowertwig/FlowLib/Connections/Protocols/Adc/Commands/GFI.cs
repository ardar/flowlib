using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
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
}