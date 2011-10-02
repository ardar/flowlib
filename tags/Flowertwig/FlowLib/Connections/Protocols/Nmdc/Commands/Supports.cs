using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Supports : HubMessage
    {
        protected string[] support = null;
        protected bool tls = false; // Hub support

        /********
         * SUPPORTS:
         * BZList, Bz2 compressed filelist
         * XmlBZList, Bz2 compressed xml filelist
         * GetZBlock, ZLib compressed UGetBlock, ($UGetZBlock command)
         * ADCGet, Get command from ADC protocol draft.
         * ZLIG, Extention to ADCGet that compress filecontent with ZLib
         * TTHL, Extention to ADCGet that will return TTH tree as binary
         * TTHF, EXtention to ADCGet that allows use of TTH for getting file.
         * MiniSlots, A special type of slot that has been historically used to transfer small files and file lists.
         *******/
        protected bool zlig = false; // client support
        protected bool tthl = false; // client support
        protected bool tthf = false; // client support
        protected bool miniSlots = false; // client support
        protected bool xmlBZList = false; // client support
        protected bool bzList = false; // client support
        protected bool adcGet = false; // client support
        protected bool getZBlock = false; // client support

        public bool SupportTLS
        {
            get { return tls; }
        }
        /// <summary>
        /// ZLIG: http://dcpp.net/wiki/index.php/ZLIG
        /// This feature indicates support for compressing the stream of data sent by $ADCGET with the ZLib library.
        /// To enable compression, ZL1 is used as a parameter to $ADCGET and is also echoed back with $ADCSND.
        /// </summary>
        public bool ZLIG
        {
            get { return zlig; }
            set
            {
                zlig = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// TTHL: http://dcpp.net/wiki/index.php/TTHL
        /// This feature indicates support for the "tthl" namespace for $ADCGET.
        /// This namespace allows transfer of the intermediate (leaf) hashes used to calculate the TTH root hash.
        /// Those hashes allow verfication of segments of the associated file.
        /// </summary>
        public bool TTHL
        {
            get { return tthl; }
            set
            {
                tthl = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// TTHF: http://dcpp.net/wiki/index.php/TTHF
        /// This feature indicates support for the retrieving a file by its TTH through $ADCGET.
        /// Instead of a filename, use "TTH/hash", where hash is the Base32 encoded representation of its TTH root hash.
        /// </summary>
        public bool TTHF
        {
            get { return tthf; }
            set
            {
                tthf = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// MiniSlots: http://dcpp.net/wiki/index.php/MiniSlots
        /// This extension signals support for the concept of a "mini-slot"
        /// </summary>
        public bool MiniSlots
        {
            get { return miniSlots; }
            set
            {
                miniSlots = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// XmlBZList: http://dcpp.net/wiki/index.php/XmlBZList
        /// Supporting this means supporting UTF-8 XML file lists.
        /// </summary>
        public bool XmlBZList
        {
            get { return xmlBZList; }
            set
            {
                xmlBZList = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// BZList: http://dcpp.net/wiki/index.php/BZList
        /// Support for a bzip2 compressed filelist
        /// </summary>
        public bool BZList
        {
            get { return bzList; }
            set
            {
                bzList = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// ADCGet: http://dcpp.net/wiki/index.php/ADCGet
        /// This feature indicates support for $ADCGET, a file retrieval command backported from the ADC draft.
        /// </summary>
        public bool ADCGet
        {
            get { return adcGet; }
            set
            {
                adcGet = value;
                SetValuesToRaw();
            }
        }
        /// <summary>
        /// GetZBlock: http://dcpp.net/wiki/index.php/GetZBlock
        /// Instead of $Get and $Send, use "$GetZBlock start numbytes filename|"
        /// </summary>
        public bool GetZBlock
        {
            get { return getZBlock; }
            set
            {
                getZBlock = value;
                SetValuesToRaw();
            }
        }

        public string[] Support
        {
            get { return support; }
            set
            {
                support = value;
                SetFromRaw();
            }
        }

        public new string Raw
        {
            get { return base.Raw; }
            set
            {
                base.Raw = value;
                int pos;
                if ((pos = raw.IndexOf(" ")) != -1)
                {
                    pos++;
                    string tmp = raw.Substring(pos);
                    support = tmp.Split(' ');
                    foreach (string sup in support)
                    {
                        switch (sup.ToLower())
                        {
                            case "tls":
                                tls = true;
                                break;
                            default:
                                break;
                        }
                    }
                    IsValid = true;
                }

            }
        }

        /// <summary>
        /// Constructor for sending Support command.
        /// </summary>
        /// <param name="con">Connection where commands should be sent.</param>
        public Supports(IConnection con)
            : base(con, null)
        {
            xmlBZList = true;
            getZBlock = true;
            adcGet = true;
            zlig = false;
            tthf = true;
            tthl = true;
            miniSlots = false;
            bzList = true;

            // TODO : Remove this when we have enabled ZLib compression
            getZBlock = false;
            // TODO : Remove this when TTHL works
            tthl = false;
            SetValuesToRaw();
        }

        /// <summary>
        /// Constructor for receiving Support command.
        /// </summary>
        /// <param name="con">Connection where command was received on</param>
        /// <param name="raw">unmodified command</param>
        public Supports(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                string tmp = raw.Substring(pos);
                Support = tmp.Split(' ');
                valid = true;
                return;
            }
        }
        /// <summary>
        /// Check and sets specific SUPPORTS, Like: ZLIG, TTHL, TTHF, from Values
        /// </summary>
        protected void SetValuesToRaw()
        {
            string tmp = "$Supports";
            if (BZList) tmp += " BZList";
            if (XmlBZList) tmp += " XmlBZList";
            if (GetZBlock) tmp += " GetZBlock";
            if (ADCGet) tmp += " ADCGet";
            if (ZLIG) tmp += " ZLIG";
            if (TTHF) tmp += " TTHF";
            if (TTHL) tmp += " TTHL";
            if (MiniSlots) tmp += " MiniSlots";
            tmp += "|";
            Raw = tmp;
        }
        /// <summary>
        /// Check and sets specific SUPPORTS, Like: ZLIG, TTHL, TTHF, from Raw
        /// </summary>
        protected void SetFromRaw()
        {
            if (support == null)
                return;
            zlig = false;
            tthl = false;
            tthf = false;
            miniSlots = false;
            xmlBZList = false;
            bzList = false;
            adcGet = false;
            getZBlock = false;
            for (int i = 0; i < support.Length; i++)
            {
                switch (support[i])
                {
                    case "ZLIG":
                        zlig = true;
                        break;
                    case "TTHL":
                        tthl = true;
                        break;
                    case "TTHF":
                        tthf = true;
                        break;
                    case "MiniSlots":
                        miniSlots = true;
                        break;
                    case "XmlBZList":
                        xmlBZList = true;
                        break;
                    case "BZList":
                        bzList = true;
                        break;
                    case "ADCGet":
                        adcGet = true;
                        break;
                    case "GetZBlock":
                        getZBlock = true;
                        break;
                }
            }
        }

        /// <summary>
        /// This Constructor is for sending
        /// </summary>
        /// <param name="hub"></param>
        public Supports(Hub hub)
            : base(hub, null)
        {
            if (hub.Me.ContainsKey(UserInfo.SECURE))
                Raw = "$Supports NoHello NoGetINFO TLS |";
            else
                Raw = "$Supports NoHello NoGetINFO |";
            IsValid = true;
        }

        /// <summary>
        /// This Constructor is for receiving
        /// </summary>
        /// <param name="hub"></param>
        /// <param name="raw"></param>
        public Supports(Hub hub, string raw)
            : base(hub, null)
        {
            Raw = raw;
        }
    }
}