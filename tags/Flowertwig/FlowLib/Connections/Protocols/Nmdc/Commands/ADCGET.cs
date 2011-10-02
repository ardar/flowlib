using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class ADCGET : StrMessage
    {
        #region protected variables
        protected string type = string.Empty;
        protected string content = string.Empty;
        protected long start = 0;
        protected long length = -1;
        protected string[] supports = null;
        #endregion
        #region public variables
        public bool ZL1 = false;
        #endregion
        #region Properties
        /// <summary>
        /// Types of transfer.
        /// Know types are: file, tthl and list.
        /// </summary>
        public string Type
        {
            get { return type; }
            set { type = value; }
        }
        /// <summary>
        /// TTH or file name.
        /// </summary>
        public string Content
        {
            get { return content; }
            set { content = value; }
        }
        /// <summary>
        /// Where to start transfer.
        /// </summary>
        public long Start
        {
            get { return start; }
            set { start = value; }
        }
        /// <summary>
        /// How many bytes to transfer.
        /// </summary>
        public long Length
        {
            get { return length; }
            set { length = value; }
        }
        /// <summary>
        /// Extra parameters (Used for supports)
        /// </summary>
        public string[] Support
        {
            get { return supports; }
            set
            {
                supports = value;
                Check();
            }
        }
        #endregion

        protected void MakeRaw()
        {
            Raw = string.Format("$ADCGET {0} {1} {2} {3}|", type, content, start, length);
        }

        public ADCGET(IConnection con, string type, string content)
            : base(con, null)
        {
            this.type = type;
            this.content = content;
            MakeRaw();
        }
        public ADCGET(IConnection con, string type, string content, long start, long length, bool zlib)
            : this(con, type, content, start, length)
        {
            ZL1 = zlib;
        }

        public ADCGET(IConnection con, string type, string content, long start, long length)
            : this(con, type, content)
        {
            this.start = start;
            this.length = length;
            MakeRaw();
        }

        public ADCGET(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                string tmp = raw.Substring(pos);
                string supp = string.Empty;
                string[] sections = tmp.Split(' ');
                for (int i = 0; i < sections.Length; i++)
                {
                    switch (i)
                    {
                        case 0: type = sections[i]; break;                  // Type
                        case 1: content = sections[i]; break;               // Content
                        case 2:   // Start pos
                            try
                            {
                                start = long.Parse(sections[i]);
                            }
                            catch { }
                            break;
                        case 3:  // Length pos
                            try
                            {
                                length = long.Parse(sections[i]);
                                valid = true;
                            }
                            catch { }
                            break;
                        default:
                            supp.Insert(0, sections[i] + " ");
                            Check(sections[i]);
                            break;
                    }
                }
                supp = supp.Trim();
                supports = supp.Split(' ');
                content = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.ASCII.GetBytes(content));
            }
        }

        protected void Check(string name)
        {
            switch (name)
            {
                case "ZL1": ZL1 = true; break;
            }
        }

        /// <summary>
        /// Check and sets specific SUPPORTS. Like: ZLIG
        /// </summary>
        protected void Check()
        {
            if (supports == null)
                return;
            for (int i = 0; i < supports.Length; i++)
            {
                Check(supports[i]);
            }
        }
    }
}