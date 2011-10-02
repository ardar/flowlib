using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Failed : StrMessage
    {
        protected string message = string.Empty;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public Failed(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                message = raw.Substring(pos);
                valid = true;
            }
        }
        public Failed(string message, IConnection con)
            : base(con, null)
        {
            this.message = message;
            Raw = "$Failed "+message+"|";
        }
    }
}