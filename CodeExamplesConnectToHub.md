# Description #

With the below code we will connect to a hub.
HubSetting is used to specify connection settings like address and bot display name.
Then we specify what protocol to use for this connection.
This can be done in 2 ways.
**First** is to set Protocol property for settings object.
This can only use inbuilt protocols (Auto, Nmdc, Adc)
**Second** is to set Protocol property for the hub object it self.
Here you can use a class implementing IProtocol.
# Code #
```
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
            settings.DisplayName = "FlowLib";
            // The below is one way to say what protocol we should use when connecting to hub.
            //settings.Protocol = "Nmdc";   // Here we are saying we know it is a Nmdc hub
            //settings.Protocol = "Adc";   // Here we are saying we know it is a Adc hub
            //settings.Protocol = "Auto";   // Here we tell it we dont care what protocol it uses (Adc or Nmdc). Just try to connect.

            Hub hubConnection = new Hub(settings);
            // This is a other way to say what protocol we should use when connecting
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);

            hubConnection.Connect();
        }
    }
}
```