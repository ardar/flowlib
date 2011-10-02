using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
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
}