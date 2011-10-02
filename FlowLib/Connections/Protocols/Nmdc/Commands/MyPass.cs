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
        public MyPass(Hub hub)
            : base(hub, null)
        {
            Password = hub.HubSetting.Password;
        }
        public MyPass(Hub hub, string pass)
            : base(hub, null)
        {
            Password = pass;
        }
    }
}