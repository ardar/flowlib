
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *  
 */

using FlowLib.Interfaces;
using FlowLib.Protocols;
using FlowLib.Containers;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Protocols.TransferNmdc
{
    public class MyNick : StrMessage
    {
        protected UserInfo info = null;
        /// <summary>
        /// Contains UserInfo (Username) of the user we are connecting to.
        /// </summary>
        public UserInfo Info
        {
            get { return info; }
            set {
                info = value;
                Raw = "$MyNick " + info.DisplayName +"|";
            }
        }

        public MyNick(IConnection con, UserInfo info)
            : base(con, null)
        {
            Info = info;
        }
        
        public MyNick(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1) {
                pos++;
                info = new UserInfo();
                info.DisplayName = raw.Substring(pos);
                valid = true;
                // TODO : Check against Usernames in hub should be done here.
                return;
            }
            con.Disconnect("Handshake was failing.");
        }
    }
    public class Supports : StrMessage
    {
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
        protected string[] support = null;
        protected bool zlig = false;
        protected bool tthl = false;
        protected bool tthf = false;
        protected bool miniSlots = false;
        protected bool xmlBZList = false;
        protected bool bzList = false;
        protected bool adcGet = false;
        protected bool getZBlock = false;
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
            set { 
                support = value;
                SetFromRaw();
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
                    case "ZLIG": zlig = true; break;
                    case "TTHL": tthl = true; break;
                    case "TTHF": tthf = true; break;
                    case "MiniSlots": miniSlots = true; break;
                    case "XmlBZList": xmlBZList = true; break;
                    case "BZList": bzList = true; break;
                    case "ADCGet": adcGet = true; break;
                    case "GetZBlock": getZBlock = true; break;
                }
            }
        }

    }
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
    public class Lock : StrMessage
    {
        protected bool extended = false;
        protected string pk = string.Empty;
        protected string key = string.Empty;
        /// <summary>
        /// Indicates that client supports $Supports
        /// </summary>
        public bool Extended
        {
            get { return extended; }
            set { 
                extended = value;
                MakeRaw();
            }
        }
        /// <summary>
        /// PK value of Lock.
        /// </summary>
        public string PK
        {
            get { return pk; }
            set { 
                pk = value;
                MakeRaw();
            }
        }
        /// <summary>
        /// Calculated key.
        /// </summary>
        public string Key
        {
            get { return key; }
            set { 
                key = value;
                MakeRaw();
            }
        }

        protected void MakeRaw()
        {
            string ext = "ABCABCABCABCABCAABCABCABCABCABCABC";
            if (extended)
                ext = "EXTENDEDPROTOCOL" + ext.Substring(16);
            Raw = "$Lock " + ext + " Pk=" + pk + "|";
        }

        public Lock(ITransfer trans)
            : base(trans, null)
        {
            if (trans.Me == null)
                throw new System.ArgumentNullException("Transfer.Me can't be null.");

            extended = true;

			pk = trans.Me.TagInfo.Version.Replace(" ", "").Replace("V:", "") +"ABCABCABCABCABCA";
            if (pk.Length > 16)
                pk = pk.Substring(0, 16);
            MakeRaw();
        }

        public Lock(IConnection con, string raw)
            : base(con, raw)
        {
            extended = raw.Contains("EXTENDEDPROTOCOL");
            int pos;
            if ((pos = raw.IndexOf("Pk=")) != -1)
            {
                pos += 3;
                pk = raw.Substring(pos);
                valid = true;
            }

            // TODO : Now we have the below code on 2 diffrent places. Use the one in hubnmdcmessages
            /*
             * This LockToKey does NOT use Microsoft.VisualBasic as a reference 
             * also strips $Lock and Pk=
             * Written by Gargol (gargol@gbot.nu)
             */
            string lck = raw;
            lck = lck.Replace("$Lock ", "");
            int iPos = lck.IndexOf(" Pk=", 1);
            if (iPos > 0) lck = lck.Substring(0, iPos);
            int[] arrChar = new int[lck.Length + 1];
            int[] arrRet = new int[lck.Length + 1];
            arrChar[1] = lck[0];
            for (int i = 2; i < lck.Length + 1; i++)
            {
                arrChar[i] = lck[i - 1];
                arrRet[i] = arrChar[i] ^ arrChar[i - 1];
            }
            arrRet[1] = arrChar[1] ^ arrChar[lck.Length] ^ arrChar[lck.Length - 1] ^ 5;
            string sKey = "";
            for (int n = 1; n < lck.Length + 1; n++)
            {
                arrRet[n] = ((arrRet[n] * 16) & 240) | ((arrRet[n] / 16) & 15);
                int j = arrRet[n];
                switch (j)
                {
                    case 0:
                    case 5:
                    case 36:
                    case 96:
                    case 124:
                    case 126:
                        sKey += "/%DCN"
                             + ((string)("00" + j.ToString())).Substring(j.ToString().Length - 1)
                             + "%/";
                        break;
                    default:
                        sKey += Chr(System.Convert.ToByte((char)j));
                        break;
                }
            }
            Key = sKey;
        }

        protected static char Chr(byte src)
        {
            return (System.Text.Encoding.Default.GetChars(new byte[] { src })[0]);
        }
    }
    public class Key : StrMessage
    {
        protected string value = string.Empty;
        /// <summary>
        /// Calculated key
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        public Key(string calcKey, IConnection con)
            : base(con, null)
        {
            Raw = "$Key " + calcKey + "|";
        }

        public Key(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                value = raw.Substring(pos);
                valid = true;
            }
        }
    }
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
    public class Error : StrMessage
    {
        protected string message = string.Empty;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }

        public Error(IConnection con, string raw)
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

        public Error(string message, IConnection con)
            : base(con, null)
        {
            this.message = message;
            Raw = "$Error "+message+"|";
        }
    }
    public class GetListLen : StrMessage
    {
        public GetListLen(IConnection con, string raw)
            : base(con, raw)
        {

        }
        public GetListLen(IConnection con)
            : base(con, null)
        {
            Raw = "$GetListLen|";
        }
    }
    public class MaxedOut : StrMessage
    {
        public MaxedOut(IConnection con, string raw)
            : base(con, raw)
        {
            valid = true;
        }
        public MaxedOut(IConnection con)
            : base(con, null)
        {
            Raw = "$MaxedOut|";
        }
    }
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

    #region Get Content
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
    public class GetZBlock : GetBlocks
    {
        public GetZBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, true, start, length)
        {
            ZLibCompression = true;
            Raw = string.Format("$GetZBlock {0} {1} {2}|", Start, Length, filename);
        }

        public GetZBlock(IConnection con, string raw)
            : base(con, raw)
        {
            ZLibCompression = true;
            valid = true;
        }
    }
    public class UGetBlock : GetBlocks
    {
        public UGetBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, false,start, length)
        {
            Encoding = System.Text.Encoding.UTF8;
            Raw = string.Format("$UGetBlock {0} {1} {2}|", Start, Length, FileName);
        }

        public UGetBlock(IConnection con, string raw)
            : base(con, raw)
        {
            Encoding = System.Text.Encoding.UTF8;
            valid = true;
        }
    }
    public class UGetZBlock : GetBlocks
    {
        public UGetZBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, true, start, length)
        {
            ZLibCompression = true;
            Encoding = System.Text.Encoding.UTF8;
            Raw = string.Format("$UGetZBlock {0} {1} {2}|", Start, Length, FileName);
        }

        public UGetZBlock(IConnection con, string raw)
            : base(con, raw)
        {
            ZLibCompression = true;
            Encoding = System.Text.Encoding.UTF8;
            valid = true;
        }
    }
    #endregion
    #region Send Content
    public class Send : StrMessage
    {
        public Send(IConnection con)
            : base(con, null)
        {
            Raw = "$Send|";
        }

        public Send(IConnection con, string raw)
            : base(con, raw)
        {
            valid = true;
        }
    }
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
    #endregion
}
