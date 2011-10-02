using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Sending : StrMessage
    {
        protected long length = -1;

        public long Length
        {
            get { return length; }
            set { length = value; }
        }

        public Sending(IConnection con)
            : base(con, null)
        {
            if (length == -1)
                Raw = "$Sending|";
            else
                Raw = string.Format("$Sending {0}|", length);
        }

        public Sending(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                try
                {
                    length = long.Parse(raw.Substring(pos));
                    valid = true;
                }
                catch { }
            }
        }
    }
}