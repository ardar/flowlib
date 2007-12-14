using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class ConnectToHub
    {
        public ConnectToHub()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLibNick";
            // The below is one way to say what protocol we should use when connecting to hub.
            //settings.Protocol = "Nmdc";

            Hub hubConnection = new Hub(settings);
            // This is a other way to say what protocol we should use when connecting
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);

            hubConnection.Connect();
        }
    }
}
