using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetPass : HubMessage
    {
        public GetPass(Client client, string raw) 
            : base(client, raw)
        {
            IsValid = true;
        }
    }
}