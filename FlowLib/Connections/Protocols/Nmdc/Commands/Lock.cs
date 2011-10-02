using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;
using FlowLib.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Lock : HubMessage
    {
        protected bool extended = false;
        protected string pk = string.Empty;
        protected string key = null;
        public string Key
        {
            get { return key; }
            set
            {
                key = value;
                MakeRaw();
            }
        }
        /// <summary>
        /// Indicates that client supports $Supports
        /// </summary>
        public bool Extended
        {
            get { return extended; }
            set
            {
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
            set
            {
                pk = value;
                MakeRaw();
            }
        }

        public Lock(ITransfer trans)
            : base(trans, null)
        {
            if (trans.Me == null)
                throw new System.ArgumentNullException("Transfer.Me can't be null.");

            extended = true;

            pk = trans.Me.TagInfo.Version.Replace(" ", "").Replace("V:", "") + "ABCABCABCABCABCA";
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

        public Lock(Hub hub, string raw) : base(hub, raw)
        {
            /*********
             * This code has been copied from the CoreDC project.
             * Many thanks :D
             *********/

            string lck = raw.Replace("$Lock ", "");
            int iPos = lck.IndexOf(" Pk=", 1);
            if (iPos > 0) lck = lck.Substring(0, iPos);

            char[] arrChar = new char[lck.Length];
            int[] arrRet = new int[lck.Length];
            arrChar[0] = lck[0];
            int tmp = lck[0];
            for (int i = 1; i < lck.Length; i++)
            {
                //arrChar[i] = lck[i];
                byte[] test = hub.Protocol.Encoding.GetBytes(new char[] { lck[i] });
                arrChar[i] = (char)test[0];
                arrRet[i] = arrChar[i] ^ arrChar[i - 1];
            }
            arrRet[0] = arrChar[0] ^ arrChar[lck.Length - 1] ^ arrChar[lck.Length - 2] ^ 5;
            string sKey = "";
            for (int n = 0; n < lck.Length; n++)
            {
                arrRet[n] = ((arrRet[n] * 16 & 240)) | ((arrRet[n] / 16) & 15);
                int j = arrRet[n];
                switch (j)
                {
                    case 0:
                    case 5:
                    case 36:
                    case 96:
                    case 124:
                    case 126:
                        sKey += string.Format("/%DCN{0:000}%/", j);
                        break;
                    default:
                        sKey += hub.Protocol.Encoding.GetChars(new byte[] { System.Convert.ToByte((char)j) })[0];
                        break;
                }
            }
            key = sKey;
            if (!string.IsNullOrEmpty(key))
                IsValid = true;
        }

        protected void MakeRaw()
        {
            string ext = "ABCABCABCABCABCAABCABCABCABCABCABC";
            if (extended)
                ext = "EXTENDEDPROTOCOL" + ext.Substring(16);
            Raw = "$Lock " + ext + " Pk=" + pk + "|";
        }

        protected static char Chr(byte src)
        {
            return (System.Text.Encoding.Default.GetChars(new byte[] { src })[0]);
        }
    }
}