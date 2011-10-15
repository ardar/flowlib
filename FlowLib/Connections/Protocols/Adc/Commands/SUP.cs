using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class SUP : AdcBaseMessage
    {
        protected bool bas = false;
        protected bool bzip = false;
        protected bool tigr = false;
        protected bool adcs = false;

        public bool BASE
        {
            get { return bas; }
        }

        public bool BZIP
        {
            get { return bzip; }
        }

        public bool TIGR
        {
            get { return tigr; }
        }

        public bool ADCS
        {
            get { return adcs; }
            set { adcs = value; }
        }

        /// <summary>
        /// Sending Support to hub
        /// </summary>
        public SUP(IConnection con)
            : base(con, null)
        {
            if (con is Client)
            {
                if (((Client)con).Me.ContainsKey(UserInfo.SECURE))
                    Raw = "HSUP " + AdcProtocol.Support + " ADADC0" + "\n";
                else
                    Raw = "HSUP " + AdcProtocol.Support + "\n";
            }
            else
            {
                if (((Transfer)con).Me.ContainsKey(UserInfo.SECURE))
                    Raw = "CSUP " + AdcProtocol.TransferSupport + " ADADC0" + "\n";
                else
                    Raw = "CSUP " + AdcProtocol.TransferSupport + "\n";
            }
            ParseRaw();
        }

        public SUP(IConnection con, string raw)
            : base(con, raw)
        {
            ParseRaw();
        }

        protected void ParseRaw()
        {
            foreach (string sup in param)
            {
                // We hate old DC++ clients with stupid support
                if (sup.Equals("ADBAS0"))
                {
                    bas = true;
                    bzip = true;
                    tigr = true;
                }
                if (sup.Equals("ADTIGR"))
                    tigr = true;
                if (sup.Equals("ADBZIP"))
                    bzip = true;
                if (sup.Equals("ADBASE"))
                    bas = true;
                if (sup.Equals("ADADC0"))
                    adcs = true;
            }

            if (bas && tigr)
                valid = true;
        }
    }
}