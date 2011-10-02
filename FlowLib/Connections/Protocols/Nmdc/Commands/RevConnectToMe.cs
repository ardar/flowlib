using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class RevConnectToMe : HubMessage
    {
        public RevConnectToMe(string remoteNick, Hub hub)
            : base(hub, null)
        {
            from = hub.Me.DisplayName;
            to = remoteNick;
            Raw = "$RevConnectToMe " + from + " " + to + "|";
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                IsValid = true;
        }

        public RevConnectToMe(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                if ((pos2 = raw.IndexOf(" ",++pos)) != -1)
                {
                    from = raw.Substring(pos, pos2++ -pos);
                    to = raw.Substring(pos2);
                    if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(from))
                        IsValid = true;
                }
            }
        }
    }
}