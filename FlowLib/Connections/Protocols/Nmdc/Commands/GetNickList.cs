using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetNickList : HubMessage
    {
        public GetNickList(Client client) 
            : base(client, null)
        {
            Raw = "$GetNickList|";
            IsValid = true;
        }
    }
}