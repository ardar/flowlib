using System.Windows.Forms;
using FlowLib.Connections;
using System.Drawing;

namespace ClientExample.Controls
{
    public class MessageTab : TabPage
    {
        delegate void update();

        protected MessagePanel msg = new MessagePanel();
        protected int status = TcpConnection.Disconnected;

        public int OnlineStatus
        {
            get { return status; }
            set
            {
                status = value;
                Invoke(new update(UpdateStatus));
            }
        }

        protected void UpdateStatus()
        {
            switch (status)
            {
                case TcpConnection.Connecting:
                case TcpConnection.Connected:
                case TcpConnection.Disconnected:
                    ImageIndex = status;
                    break;
                default:
                    ImageIndex = TcpConnection.Connected;
                    break;
            }
        }

        public MessagePanel MessagePanel
        {
            get { return msg; }
            set { msg = value; }
        }

        public MessageTab()
        {
            msg.Dock = DockStyle.Fill;
            Controls.Add(msg);
            while (Handle == null) {}
            OnlineStatus = TcpConnection.Connected;
       }
    }
}
