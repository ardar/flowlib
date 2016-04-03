# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

  1. Start listening on MessageReceived and MessageToSend event.
  1. e.Data will contain a inherit class of StrMessage.

# Note #

  1. This is before protocol handling (**NO** action has been triggered)
  1. This is before message has actualy been sent. (If connection is not alive, message will not be sent)
  1. You can tell protocol that you want to handle this message yourself by setting e.Handled = true. (Protocol will now ignore sending/receiving of this command)


# Code #

```
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class DisplayRawMessages
    {
        public DisplayRawMessages()
        {
            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            Hub hubConnection = new Hub(settings);
            hubConnection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(hubConnection_ProtocolChange);
            hubConnection.Connect();
        }

        void hubConnection_ProtocolChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            Hub hubConnection = sender as Hub;
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.MessageReceived -= Protocol_MessageReceived;
                prot.MessageToSend -= Protocol_MessageToSend;
            }
            hubConnection.Protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(Protocol_MessageReceived);
            hubConnection.Protocol.MessageToSend += new FlowLib.Events.FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
                System.Console.WriteLine(string.Format("OUT: {0}", msg.Raw));
        }

        void Protocol_MessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
               System.Console.WriteLine(string.Format("IN: {0}", msg.Raw));
        }
    }
}
```