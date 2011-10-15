using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class RES : AdcBaseMessage
    {
        protected ContentInfo info = null;
        protected string token = null;
        protected long slots;
        protected System.Net.IPEndPoint address = null;

        public System.Net.IPEndPoint Address
        {
            get { return address; }
        }
        public ContentInfo Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
            }
        }
        public string Token
        {
            get { return token; }
        }
        public long Slots
        {
            get { return slots; }
        }

        public RES(IConnection con, string raw)
            : base(con, raw)
        {
            info = new ContentInfo();
            foreach (string var in param)
            {
                if (var.Length < 2)
                    continue;
                string key = var.Substring(0, 2);
                string value = var.Substring(2);
                switch (key)
                {
                    case "FN":
                        info.Set(ContentInfo.VIRTUAL, AdcProtocol.ConvertIncomming(value));
                        valid = true;
                        break;
                    case "SI":
                        try
                        {
                            info.Size = long.Parse(value);
                        }
                        catch { }
                        break;
                    case "SL":
                        try
                        {
                            slots = long.Parse(value);
                        }
                        catch { }
                        break;
                    case "TO":
                        token = value;
                        break;
                    case "TR":
                        info.Set(ContentInfo.TTH, value);
                        break;
                    default:
                        // We dont know this value but to allow developer to support this anyway we will store it.
                        info.Set(key, value);
                        break;
                }
            }
        }

        public RES(IConnection con, ContentInfo info, string token, UserInfo usr)
            : base(con, null)
        {
            this.info = info;

            if (
                usr.ContainsKey(UserInfo.UDPPORT)
                && usr.ContainsKey("SU")
                && usr.ContainsKey(UserInfo.IP)
                && (usr.Get("SU").Contains("UDP4") || usr.Get("SU").Contains("UDP6"))
                )
            {
                type = "U";
                int port = -1;
                string addr = usr.Get(UserInfo.IP);
                try
                {
                    port = int.Parse(usr.Get(UserInfo.UDPPORT));
                    if (port < 0 || port > 65535)
                        port = 0;
                    address = new System.Net.IPEndPoint(System.Net.IPAddress.Parse(addr), port);
                }
                catch { }
            }
            else if (con is Client)
            {
                type = "D";
            }
            else
            {
                type = "C";
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (info.ContainsKey(ContentInfo.VIRTUAL))
                sb.Append(" FN" + AdcProtocol.ConvertOutgoing(info.Get(ContentInfo.VIRTUAL)));
            sb.Append(" SI" + info.Size.ToString());
            if (!string.IsNullOrEmpty(token))
                sb.Append(" TO" + token);
            // TODO : Add Slots handling
            Raw = string.Format("{0}RES{1}\n", type, sb.ToString());
        }
    }
}