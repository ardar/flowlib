using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    /// <summary>
    /// This is not a NMDC command.
    /// It is a class used to parse $GetZBlock, $UGetBlock and $UGetZBlock in one place.
    /// </summary>
    public class GetBlocks : StrMessage
    {
        protected long start = 0;
        protected long length = -1;
        protected string filename = string.Empty;
        protected bool zLibCompression = false;
        protected System.Text.Encoding encoding = System.Text.Encoding.ASCII;

        public System.Text.Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        public bool ZLibCompression
        {
            get { return zLibCompression; }
            set { zLibCompression = value; }
        }

        public long Start
        {
            get { return start; }
            set { start = value; }
        }
        public long Length
        {
            get { return length; }
            set { length = value; }
        }
        public string FileName
        {
            get { return filename; }
            set { filename = value; }
        }

        public GetBlocks(IConnection con, string filename, bool zlibComp, long start, long length)
            : this(con,filename,zlibComp)
        {
            this.start = start;
            this.length = length;
        }

        public GetBlocks(IConnection con, string filename, bool zlibComp)
            : base(con, null)
        {
            this.filename = filename;
            this.zLibCompression = zlibComp;
        }

        public GetBlocks(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                string tmp = raw.Substring(pos);
                string[] sec = tmp.Split(' ');
                for (int i = 0; i < sec.Length; i++)
                {
                    switch (i)
                    {
                        case 0:
                            try
                            {
                                start = long.Parse(sec[i]);
                            }
                            catch { }
                            break;
                        case 1:
                            try
                            {
                                length = long.Parse(sec[i]);
                            }
                            catch { }
                            break;
                        case 2:
                        default:
                            if (filename.Length == 0)
                                filename = sec[i];
                            else
                                filename += " " +sec[i];
                            valid = true;
                            break;
                    }
                    if (!System.Text.Encoding.ASCII.Equals(encoding))
                        filename = encoding.GetString(System.Text.Encoding.ASCII.GetBytes(filename));
                }
            }
        }
    }
}