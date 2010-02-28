
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

using System.Net.Sockets;
using FlowLib.Interfaces;
using FlowLib.Events;

namespace FlowLib.Connections
{
    public class UdpConnection
    {
        protected IProtocolUdp protocol;
        protected UdpClient connection = null;
        protected bool listen = false;

        public IProtocolUdp Protocol
        {
            get { return protocol; }
            set { protocol = value; }
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

        public UdpConnection(System.Net.IPEndPoint ip)
        {
            EndPoint = ip;
            connection = new UdpClient(ip);
#if !COMPACT_FRAMEWORK
            connection.DontFragment = true;
#endif
            StartListen();
        }

        void UdpConnection_Update(object sender, FmdcEventArgs e) { }

        protected void OnListen()
        {
            while (listen)
            {
                System.Net.IPEndPoint sender = new System.Net.IPEndPoint(0,0);
                byte[] data = connection.Receive(ref sender);
                Protocol.ParseRaw(data, data.Length, sender);
            }
        }

        protected void StartListen()
        {
            IsListening = true;
            System.Threading.Thread t =  new System.Threading.Thread(new System.Threading.ThreadStart(OnListen));
            t.IsBackground = true;
            t.Start();
        }

        private static bool Send(byte[] raw, System.Net.IPEndPoint ip)
        {
            UdpClient udp = new UdpClient();
            return (udp.Send(raw, raw.Length, ip) == raw.Length);
        }

        public static bool Send(IConMessage message, System.Net.IPEndPoint ip)
        {
            if (message.Bytes != null)
                return Send(message.Bytes, ip);
            return false;
        }
    }
}
