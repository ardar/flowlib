using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Hello : HubMessage
    {
        public Hello(Client client, string raw)
            : base(client, raw)
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