using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    /// <summary>
    /// User Disconnected from hub.
    /// From will contain the username that quited.
    /// </summary>
    public class Quit : HubMessage
    {
        public Quit(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                this.from = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(from))
                    IsValid = true;
            }
        }
    }
}