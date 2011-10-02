using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Hello : HubMessage
    {
        public Hello(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                from = raw.Substring(pos, raw.Length - pos);
                if (!string.IsNullOrEmpty(from))
                    IsValid = true;
            }
        }
    }
}