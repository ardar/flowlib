using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class ForceMove : HubMessage
    {
        protected string address = null;
        public string Address
        {
            get { return address; }
        }
        public ForceMove(Client client, string raw)
            : base(client, raw)
        {
            // $ForceMove flowlib.dummy.org
            string[] com = raw.Split(' ');
            if (com.Length != 2)
                return;
            this.address = com[1];
            if (!string.IsNullOrEmpty(address))
                IsValid = true;
        }
    }
}