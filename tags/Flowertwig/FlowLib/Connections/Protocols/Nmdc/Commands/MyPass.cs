using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class MyPass : HubMessage
    {
        protected string pass = null;
        public string Password
        {
            get { return pass; }
            set { 
                pass = value;
                Raw = "$MyPass "+pass+"|";
                IsValid = !string.IsNullOrEmpty(pass);
            }
        }
        public MyPass(Client client)
            : base(client, null)
        {
            Password = client.HubSetting.Password;
        }
        public MyPass(Client client, string pass)
            : base(client, null)
        {
            Password = pass;
        }
    }
}