
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

using System.Net;
using System.Text;
using Flowertwig.Utils.Connections.Entities;
using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Events;
using FlowLib.Connections.Protocols.Adc.Commands;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols
{
    /// <summary>
    /// We are using a second class for responding on udp messages.
    /// This is so others cant try to say they are a hub.
    /// </summary>
    public class UdpProtocol : IProtocolUdp
    {
        public event EventHandler MessageReceived;
        public event EventHandler MessageToSend;
        public event EventHandler Update;

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
            MessageReceived = new EventHandler(UdpNmdcProtocol_MessageReceived);
            MessageToSend = new EventHandler(UdpNmdcProtocol_MessageToSend);
            Update = new EventHandler(UdpNmdcProtocol_Update);
        }

        public void Dispose()
        {
            // We dont need todo anything here..
        }

        void UdpNmdcProtocol_Update(object sender, DefaultEventArgs e) { }
        void UdpNmdcProtocol_MessageToSend(object sender, DefaultEventArgs e) { }
        void UdpNmdcProtocol_MessageReceived(object sender, DefaultEventArgs e) { }

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
                    AdcBaseMessage adc = null;
                    if (!(adc = new RES(null, raw)).IsValid)
                        adc = null;
                    if (adc == null && !(adc = new RES(null, raw)).IsValid)
                        adc = null;
                    msg = adc;
                    break;
            }
            // Do we support message type?
            if (msg == null)
                return;

            // Plugin handling here
            DefaultEventArgs e = new DefaultEventArgs(Actions.CommandIncomming, msg);
            MessageReceived(this, e);
            if (!e.Handled)
                ActOnInMessage(msg);
        }

        public void ActOnInMessage(IConMessage message) { }
        public void ActOnOutMessage(DefaultEventArgs e) { }

        public bool OnSend(IConMessage msg)
        {
            DefaultEventArgs e = new DefaultEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(this, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }
    }
}
