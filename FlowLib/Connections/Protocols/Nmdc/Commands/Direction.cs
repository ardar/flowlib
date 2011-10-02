using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Direction : StrMessage
    {
        protected bool download = true;
        protected int number = 0;
        /// <summary>
        /// Do other part want to Download?
        /// </summary>
        public bool Download
        {
            get { return download; }
            set { download = value; }
        }
        /// <summary>
        /// Number that other part sends.
        /// </summary>
        public int Number
        {
            get { return number; }
            set { number = value; }
        }
        /// <summary>
        /// Constructor for sending Direction.
        /// </summary>
        /// <param name="con">Connection where command should be sent.</param>
        /// <param name="download">Do we want to download?</param>
        public Direction(IConnection con, bool download)
            : base(con, null)
        {
            System.Random rand = new System.Random();
            number = rand.Next(0, 32767);
            this.download = download;
            Raw = "$Direction " + (download ? "Download" : "Upload")+ " " + number + "|";
        }
        /// <summary>
        /// Constructor for receiving Direction.
        /// </summary>
        /// <param name="con">Connection where command should be sent.</param>
        /// <param name="raw">unmodified command</param>
        public Direction(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                string tmp = raw.Substring(pos);
                if ((pos = tmp.IndexOf(" ")) != -1)
                {
                    switch (tmp.Substring(0, pos))
                    {
                        case "Download":
                            break;
                        case "Upload":
                            download = false;
                            break;
                    }
                    try
                    {
                        number = int.Parse(tmp.Substring(++pos));
                        valid = true;
                    }
                    catch { }
                }
            }
        }
    }
}