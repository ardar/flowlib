using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Get : StrMessage
    {
        protected string file = string.Empty;
        protected long start = 0;

        /// <summary>
        /// Filename requested.
        /// </summary>
        public string File
        {
            get { return file; }
            set { file = value; }
        }
        /// <summary>
        /// Start pos for file.
        /// WARNING: NOT counted from ONE, it is from ZERO!
        /// </summary>
        public long Start
        {
            get { return start; }
            set { start = value; }
        }

        public Get(IConnection con, string file, long start)
            : base(con, null)
        {
            this.file = file;
            this.start = start;
            Raw = string.Format("$Get {0}${1}|", File, Start +1);
        }

        public Get(IConnection con, string raw)
            : base(con, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(" ")) != -1 && (pos2 = raw.LastIndexOf("$")) != -1)
            {
                pos++;
                file = raw.Substring(pos, pos2 -pos);
                pos2++;
                string tmp = raw.Substring(pos2);
                try
                {
                    start = long.Parse(tmp);
                    valid = true;
                }
                catch { }
                if (start > 0)
                {
                    start--;
                }
            }
        }
    }
}