using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    /// <summary>
    /// Indicates that current user is an operator.
    /// Username will be in To.
    /// </summary>
    public class LogedIn : HubMessage
    {
        public LogedIn(Client client, string raw)
            : base(client, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                this.to = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(to))
                    IsValid = true;
            }
        }
    }
}