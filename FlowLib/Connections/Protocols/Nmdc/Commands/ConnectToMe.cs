using Flowertwig.Utils.Enums;
using FlowLib.Connections.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class ConnectToMe : HubMessage
    {
        #region Variables
        protected string address = null;
        protected int port = -1;
        protected bool tls = false;
        #endregion
        #region Properties
        public string Address
        {
            get { return address; }
        }
        public int Port
        {
            get { return port; }
        }
        public bool TLS
        {
            get { return tls; }
        }
        #endregion
        public ConnectToMe(string toNick, int port, Client client)
            : this(toNick, port, client, SecurityProtocols.None)
        {

        }
        public ConnectToMe(string toNick, int port, Client client, SecurityProtocols secProt)
            : base(client, null)
        {
            this.to = toNick;
            this.port = port;

            this.address = client.LocalAddress.Address.ToString();
            if (client.Me.ContainsKey(UserInfo.IP))
                this.address = client.Me.Get(UserInfo.IP);

            if ((SecurityProtocols.TLS & secProt) == SecurityProtocols.TLS)
                tls = true;

            Raw = "$ConnectToMe "+this.to+" " + this.address + ":" + (tls ? client.Me.Get(UserInfo.SECURE) + "S" : this.port.ToString()) + "|";
            if (!string.IsNullOrEmpty(to) && port > 0 && port < 65535 && !string.IsNullOrEmpty(address))
                IsValid = true;
        }

        public ConnectToMe(Client client, string raw)
            : base(client, raw)
        {
            // $ConnectToMe FMDC 82.182.95.201:6900
            string[] sections = raw.Split(' ');
            if (sections.Length != 3)
                return;
            to = sections[1];
            string[] address = sections[2].Split(':');
            if (address.Length != 2)
                return;
            this.address = address[0];
            try
            {
                if ((tls = address[1].EndsWith("S", System.StringComparison.OrdinalIgnoreCase)))
                    address[1] = address[1].TrimEnd('S');
                port = int.Parse(address[1]);
            }
            catch { }
            if (!string.IsNullOrEmpty(to) && !string.IsNullOrEmpty(this.address) && port > 0 && port < 65535)
                IsValid = true;
        }
    }
}