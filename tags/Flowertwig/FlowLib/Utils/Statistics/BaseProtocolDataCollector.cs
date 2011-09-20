
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

using System;
using FlowLib.Interfaces;

namespace FlowLib.Utils.Statistics
{
    public abstract class BaseProtocolDataCollector : IProtocolDataCollector
    {
        protected IProtocol protocol = null;
        protected IConnection connection = null;
        protected bool bdisposed = false;

        /// <summary>
        /// Indicates if connection has been disposed or not.
        /// </summary>
        public bool IsDisposed
        {
            get { return bdisposed; }
        }

        public BaseProtocolDataCollector(IProtocol prot)
        {
            AddProtocol(prot);
        }

        public BaseProtocolDataCollector(IConnection con)
            : this(con.Protocol)
        {
            connection = con;
            connection.ProtocolChange += new FlowLib.Events.FmdcEventHandler(OnProtocolChange);
        }

        protected void Calc(ref DateTime last)
        {

        }

        protected void AddProtocol(IProtocol prot)
        {
            if (prot != null && !prot.IsDisposed)
            {
                protocol = prot;
                protocol.MessageReceived += new FlowLib.Events.FmdcEventHandler(OnMessageReceived);
                protocol.MessageToSend += new FlowLib.Events.FmdcEventHandler(OnMessageToSend);
            }
        }

        protected void RemoveProtocol()
        {
            if (protocol != null)
            {
                protocol.MessageToSend -= OnMessageToSend;
                protocol.MessageReceived -= OnMessageReceived;
                protocol = null;
            }
        }

        protected void OnProtocolChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            IProtocol prot = e.Data as IProtocol;
            IConnection con = sender as IConnection;
            if (con != null && con.Protocol != null)
            {
                RemoveProtocol();
                AddProtocol(con.Protocol);
            }

        }

        protected void OnMessageToSend(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            IConMessage msg = e.Data as IConMessage;
            if (msg != null)
            {
                HandleSend(msg);
            }
        }

        protected void OnMessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            IConMessage msg = e.Data as IConMessage;
            if (msg != null)
            {
                HandleReceived(msg);
            }
        }

        protected abstract void HandleReceived(IConMessage msg);
        protected abstract void HandleSend(IConMessage msg);

        public virtual void Dispose()
        {
            RemoveProtocol();
            if (connection != null)
            {
                connection.ProtocolChange -= OnProtocolChange;
                connection = null;
            }
            bdisposed = true;
        }
    }
}
