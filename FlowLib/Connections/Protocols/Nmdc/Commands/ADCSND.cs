using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class ADCSND : StrMessage
    {
        #region protected variables
        protected string type = string.Empty;
        protected string content = string.Empty;
        protected long start = 0;
        protected long length = 0;
        protected string[] supports = { };
        protected bool zlib = false;
        #endregion
        #region Properties
        public bool ZL1
        {
            get { return zlib; }
            set {
                zlib = value;
                Check();
            }
        }
        /// <summary>
        /// Types of transfer.
        /// Know types are: file, tthl and list.
        /// </summary>
        public string Type
        {
            get { return type; }
            set
            {
                type = value;
                Check();
            }

        }
        /// <summary>
        /// TTH or file name.
        /// </summary>
        public string Content
        {
            get { return content; }
            set
            {
                content = value;
                Check();
            }

        }
        /// <summary>
        /// Where to start transfer.
        /// </summary>
        public long Start
        {
            get { return start; }
            set {
                start = value;
                Check();
            }
        }
        /// <summary>
        /// How many bytes to transfer.
        /// </summary>
        public long Length
        {
            get { return length; }
            set {
                length = value;
                Check();
            }
        }
        /// <summary>
        /// Extra parameters (Used for supports)
        /// </summary>
        public string[] Support
        {
            get { return supports; }
        }

        #endregion
        public ADCSND(IConnection con, string raw)
            : base(con, raw)
        {
            string[] sections = raw.Split(' ');
            for (int i = 0; i < sections.Length; i++)
            {
                switch (i)
                {
                    case 1: type = sections[i]; break;
                    case 2: content = sections[i]; break;
                    case 3:
                        try
                        {
                            start = long.Parse(sections[i]);
                        }
                        catch { }
                        break;
                    case 4:
                        try
                        {
                            length = long.Parse(sections[i]);
                            valid = true;
                        }
                        catch { }
                        break;
                    case 5:
                        if (sections[i].Equals("ZL1"))
                            zlib =true;
                        break;
                }
            }
        }

        public ADCSND(IConnection con)
            : base(con, null)
        {
            Check();
        }

        public void Check()
        {
            Raw = string.Format("$ADCSND {0} {1} {2} {3}{4}|", type, content, start, length, (zlib ? " ZL1" : ""));
        }

    }
}