# Description #

_[To learn on how to connect to hub click here.](CodeExamplesConnectToHub.md)_

  1. We need to create a class implementing interface _IBaseUpdater_.
  1. Add a reference to this class in Hub constructor. This is so hub knows it should listen for messages from this class.
  1. Create a generic MainMessage. This is so we dont need to care what protocol hub use.
  1. Now we will tell hub that it we want to send a Mainchat Message. We use UpdateBase for this.
  1. Same thing is done to send a Private Message.
# Code #

```
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ConsoleDemo.Examples
{
    public class SendMainChatOrPMToHub : IBaseUpdater
    {
        #region IBaseUpdater Members
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        #endregion

        public SendMainChatOrPMToHub()
        {
            UpdateBase = new FlowLib.Events.FmdcEventHandler(SendMainChatOrPMToHub_UpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLib";
            settings.Protocol = "Auto";

            Hub hubConnection = new Hub(settings, this);
            hubConnection.Connect();
            // YOU should really listen for regmode change here.
            // Im not doing it here to make example more easy to understand.
            // Wait 10 seconds and hope we are connected.
            System.Threading.Thread.Sleep(10 * 1000);

            // Create mainchat message.
            MainMessage msg = new MainMessage(hubConnection.Me.ID, "Testing");
            // message will here be converted to right format and then be sent.
            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.MainMessage, msg));

            // Create private message.
            PrivateMessage privMsg = new PrivateMessage("DCDM++0.0495", hubConnection.Me.ID, "Testing");

            // message will here be converted to right format and then be sent.
            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(FlowLib.Events.Actions.PrivateMessage, privMsg));
        }

        void SendMainChatOrPMToHub_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) {}
    }
}

```