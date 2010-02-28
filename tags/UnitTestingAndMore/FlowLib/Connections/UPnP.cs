
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

using FlowLib.Connections;
using FlowLib.Interfaces;
using FlowLib.Protocols;
using FlowLib.Containers.UPnP;

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

        public UPnP() : this(null, null) { }
        public UPnP(IPEndPoint point) : this(point, null) { }
        public UPnP(IBaseUpdater updater) : this(null, updater) { }
        public UPnP(IPEndPoint point, IBaseUpdater updater)
        {
            RootDevices = new SortedList<string, UPnPDevice>();
            if (point == null)
                point = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);
            EndPoint = point;
            if (updater != null)
            {
                updater.UpdateBase += new FlowLib.Events.FmdcEventHandler(OnUpdateBase);
                this.updater = updater;
            }
            client = new Socket(AddressFamily.InterNetwork,
            SocketType.Dgram, ProtocolType.Udp);
            ProtocolUPnP = new UPnPProtocol(this);
        }

        protected virtual void OnUpdateBase(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            ProtocolUPnP.ActOnOutMessage(e);
        }

        protected void OnListen()
        {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, EndPoint.Port);
            EndPoint senderEP = (EndPoint)sender;
            //client.Bind(senderEP);
            System.Threading.Thread.Sleep(1 * 1000);
            while (IsListening)
            {
                IPEndPoint sender2 = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderEP2 = (EndPoint)sender2;
                byte[] data = new byte[1024];
                client.ReceiveFrom(data, ref senderEP2);
                ProtocolUPnP.ParseRaw(data, data.Length, (IPEndPoint)senderEP2);
            }
        }

        public void StartListen()
        {
            if (IsListening)
                return;
            IsListening = true;
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(OnListen));
            t.IsBackground = true;
            t.Start();
        }

        public void Discover()
        {
            if (!IsListening)
                StartListen();
            Send(ProtocolUPnP.FirstCommand);
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
