using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    /// <summary>
    /// User has quited. From has the User Id.
    /// </summary>
    public class QUI : AdcBaseMessage
    {
        protected string by = null;
        protected long time = 0;
        protected string msg = null;
        protected string address = null;
        protected bool unwanted = false;

        public bool Unwanted
        {
            get { return unwanted; }
        }

        public string DisconnectedBy
        {
            get { return by; }
        }
        public string Address
        {
            get { return address; }
        }
        public string Message
        {
            get { return msg; }
        }
        public long Time
        {
            get { return time; }
        }

        public QUI(IConnection con, string raw)
            : base(con, raw)
        {
            if (param == null)
                return;
            if (param.Count >= 1)
            {
                id = param[0];
                param.RemoveAt(0);
                valid = true;
            }
            for (int i = 0; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "ID":
                        by = value;
                        break;
                    case "TL":
                        try
                        {
                            time = long.Parse(value);
                        }
                        catch (System.Exception) { }
                        break;
                    case "MS":
                        msg = value;
                        break;
                    case "RD":
                        address = value;
                        break;
                    case "DI":
                        unwanted = true;
                        break;
                }
            }
        }
    }
}