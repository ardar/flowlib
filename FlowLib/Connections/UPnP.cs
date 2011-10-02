
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Events;
using FlowLib.Connections.Interfaces;
using FlowLib.Connections.Protocols;
using FlowLib.Entities.UPnP;
using FlowLib.Interfaces;

namespace FlowLib.Connections
{
    public class UPnP : IUPnP
    {
        protected IProtocolUPnP upnpProtocol = null;
        protected bool listen = false;
        protected IBaseUpdater updater = null;
        protected Socket client = null;

        public SortedList<string, UPnPDevice> RootDevices
        {
            get;
            set;
        }


        public IProtocolUPnP ProtocolUPnP
        {
            get { return upnpProtocol; }
            set 
            {
                upnpProtocol = value;
            }
        }

        public bool IsListening
        {
            get { return listen; }
            set { listen = value; }
        }

        public System.Net.IPEndPoint EndPoint
        {
            get;
            set;
        }

        public UPnP() : this(null, null, null) { }
        public UPnP(IPEndPoint point) : this(point, null) { }
        public UPnP(IBaseUpdater updater) : this((IPEndPoint) null, updater) { }
		public UPnP(IPEndPoint point, IBaseUpdater updater) : this(point, updater, null) { }
		public UPnP(IBaseUpdater updater, string searchTarget) : this(null, updater, searchTarget) { }
		public UPnP(IPEndPoint point, IBaseUpdater updater, string searchTarget)
        {
            RootDevices = new SortedList<string, UPnPDevice>();
            if (point == null)
                point = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            EndPoint = point;
            if (updater != null)
            {
                updater.UpdateBase += new EventHandler(OnUpdateBase);
                this.updater = updater;
            }
            client = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
			if (string.IsNullOrEmpty(searchTarget))
			{
				ProtocolUPnP = new UPnPProtocol(this);
			}
			else
			{
				ProtocolUPnP = new UPnPProtocol(this, searchTarget);
			}
        }

        protected virtual void OnUpdateBase(object sender, DefaultEventArgs e)
        {
            ProtocolUPnP.ActOnOutMessage(e);
        }

        protected void OnListen()
        {
            var sender = new IPEndPoint(IPAddress.Any, EndPoint.Port);
            var senderEP = (EndPoint)sender;
            //client.Bind(senderEP);
            System.Threading.Thread.Sleep(1 * 1000);
            while (IsListening)
            {
                var sender2 = new IPEndPoint(IPAddress.Any, 0);
                var senderEP2 = (EndPoint)sender2;
                var data = new byte[1024];
                client.ReceiveFrom(data, ref senderEP2);
                ProtocolUPnP.ParseRaw(data, data.Length, (IPEndPoint)senderEP2);
            }
        }

        public void StartListen()
        {
            if (IsListening)
                return;
            IsListening = true;
            var t = new System.Threading.Thread(OnListen) {IsBackground = true};
            t.Start();
        }

        public void Discover()
        {
            if (!IsListening)
                StartListen();
            Send((IConMessage) ProtocolUPnP.FirstCommand);
        }

        protected bool Send(byte[] raw)
        {
            return (client.SendTo(raw, raw.Length, SocketFlags.None, EndPoint) == raw.Length);
        }

        protected bool Send(IConMessage message)
        {
            if (ProtocolUPnP.OnSend(message))
                return Send(message.Bytes);
            return false;
        }
    }
}
