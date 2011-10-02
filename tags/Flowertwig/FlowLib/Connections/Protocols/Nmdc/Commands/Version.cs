using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Version : HubMessage
    {
        protected string version;
        public string Content
        {
            get { return version; }
        }
        public Version(Hub hub)
            : base(hub, null)
        {
            version = "1.0091";
            Raw = "$Version "+version+"|";
            IsValid = true;
        }
    }
}