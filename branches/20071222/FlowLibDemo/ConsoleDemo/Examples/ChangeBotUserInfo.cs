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
            settings.DisplayName = "FlowLibNick";
            // The below is one way to say what protocol we should use when connecting to hub.
            //settings.Protocol = "Nmdc";

            Hub hubConnection = new Hub(settings);
            // This is a other way to say what protocol we should use when connecting
            hubConnection.Protocol = new FlowLib.Protocols.HubNmdcProtocol(hubConnection);

            // Connection mode in UserInfo. Direct/Passive/Socket5
            hubConnection.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
            // Client/Bot name and version in UserInfo.
            hubConnection.Me.TagInfo.Version = "SpeedBot V:2.0";
            // Connection in UserInfo
            hubConnection.Me.Connection = "104KiB/s";
            // User Description in UserInfo
            hubConnection.Me.Description = "Testing you for a better feature";
            hubConnection.Connect();
        }
    }
}