using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class FileLength : StrMessage
    {
        protected long length = 0;
        public long Length
        {
            get { return length; }
            set { length = value; }
        }

        public FileLength(IConnection con, long length)
            : base(con, null)
        {
            this.length = length;
            Raw = "$FileLength "+length+"|";
        }

        public FileLength(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
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