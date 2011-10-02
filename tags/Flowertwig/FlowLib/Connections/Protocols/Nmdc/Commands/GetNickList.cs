using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetNickList : HubMessage
    {
        public GetNickList(Hub hub) 
            : base(hub, null)
        {
            Raw = "$GetNickList|";
            IsValid = true;
        }
    }
}