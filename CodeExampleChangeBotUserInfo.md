# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

# Code #
```
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class ChangeBotUserInfo
    {
        public ChangeBotUserInfo()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            Hub hubConnection = new Hub(settings);

            // Connection mode in UserInfo. Direct/Passive/Socket5
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            // Client/Bot name and version in UserInfo.
            hubConnection.Me.TagInfo.Version = "SpeedBot V:1.0";
            // Connection in UserInfo
            hubConnection.Me.Connection = "104KiB/s";
            // User Description in UserInfo
            hubConnection.Me.Description = "Testing you for a better feature";
            hubConnection.Connect();
        }
    }
}

```