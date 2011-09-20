
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

using System.Text;
using System.Net;

using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Protocols.HubNmdc;
using FlowLib.Connections;
using FlowLib.Managers;
using FlowLib.Enums;

namespace FlowLib.Protocols
{
    /// <summary>
    /// We are using a second class for responding on udp messages.
    /// This is so others cant try to say they are a hub.
    /// </summary>
    public class UdpProtocol : IProtocolUdp
    {
        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
        public event FmdcEventHandler Update;

        public IConMessage KeepAliveCommand
        {
            get { return null; }
        }
        public string Name
        {
            get { return "UdpDirectConnect"; }
        }
        public IConMessage FirstCommand
        {
            get { return null; }
        }
        public Encoding Encoding
        {
            get { return null; }
            set { }
        }
        public string Seperator
        {
            get { return null; }
        }
        /// <summary>
        /// This is not implemented for the UDP Protocol
        /// </summary>
        public bool IsDisposed
        {
            get { return false; }
        }

        public UdpProtocol()
        {
            MessageReceived = new FmdcEventHandler(UdpNmdcProtocol_MessageReceived);
            MessageToSend = new FmdcEventHandler(UdpNmdcProtocol_MessageToSend);
            Update = new FmdcEventHandler(UdpNmdcProtocol_Update);
        }

        public void Dispose()
        {
            // We dont need todo anything here..
        }

        void UdpNmdcProtocol_Update(object sender, FmdcEventArgs e) { }
        void UdpNmdcProtocol_MessageToSend(object sender, FmdcEventArgs e) { }
        void UdpNmdcProtocol_MessageReceived(object sender, FmdcEventArgs e) { }

        public void ParseRaw(byte[] b, int length)
        {
            ParseRaw(b, length, null);
        }

        public void ParseRaw(byte[] b, int length, IPEndPoint point)
        {
            string raw = Encoding.GetString(b, 0, length);
            if (raw == null)
                return;

            // If raw lenght is 0. Ignore
            if (raw.Length == 0)
                return;

            ConMessage msg = null;
            switch (raw[0])
            {
                case '$':   // Nmdc
                    int pos;
                    string cmd = null;
                    if ((pos = raw.IndexOf(' ')) != -1)
                        cmd = raw.Substring(0, pos).ToLower();
                    else
                    {
                        if (raw.Length >= 10)
                            break;
                        cmd = raw.ToLower();
                    }
                    if (cmd == null || cmd.Equals(string.Empty))
                        break;
                    switch (cmd)
                    {
                        case "$sr":
                            msg = new ConMessage(null, b);
                            break;
                    }
                    break;
                case 'U':   // Adc
                    Adc.AdcBaseMessage adc = null;
                    if (!(adc = new Adc.RES(null, raw)).IsValid)
                        adc = null;
                    if (adc == null && !(adc = new Adc.RES(null, raw)).IsValid)
                        adc = null;
                    msg = adc;
                    break;
            }
            // Do we support message type?
            if (msg == null)
                return;

            // Plugin handling here
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
            MessageReceived(this, e);
            if (!e.Handled)
                ActOnInMessage(msg);
        }

        public void ActOnInMessage(IConMessage message) { }
        public void ActOnOutMessage(FmdcEventArgs e) { }

        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(this, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }
    }
}
