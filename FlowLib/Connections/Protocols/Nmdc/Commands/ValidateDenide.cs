using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    /// <summary>
    /// Class name is NOT a spell mistake.
    /// Nmdc protocol command is spelled like this so we will too :)
    /// </summary>
    public class ValidateDenide : HubMessage
    {
        public ValidateDenide(Hub hub, string raw) : base(hub, raw) { IsValid = true; }
    }
}