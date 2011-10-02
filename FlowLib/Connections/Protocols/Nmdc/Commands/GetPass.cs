using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetPass : HubMessage
    {
        public GetPass(Hub hub, string raw) 
            : base(hub, raw)
        {
            IsValid = true;
        }
    }
}