
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

using FlowLib.Events;
using FlowLib.Interfaces;

using System.Net.Sockets;
using System.Net;
using System;

namespace FlowLib.Connections
{
    public class TcpConnectionListener : IUpdate
    {
        /// <summary>
        /// Events have happen in this hub and we want to tell others.
        /// </summary>
        public event FmdcEventHandler Update;

        protected Socket listenSocket = null;
        protected IPEndPoint ep = null;
        protected int maximumDependingConnections = 10;

        protected TcpConnectionListener()
        {
            Update = new FmdcEventHandler(TcpConnectionListener_Update);
        }

        void TcpConnectionListener_Update(object sender, FmdcEventArgs e) { }

        public TcpConnectionListener(int port)
            : this()
        {
            listenSocket = new Socket(
                                        AddressFamily.InterNetwork,
                                        SocketType.Stream,
                                        ProtocolType.Tcp);
            ep = new IPEndPoint(IPAddress.Any, port);
            listenSocket.Bind(ep);
        }

        public void Start()
        {
            try
            {
                // start listening
                listenSocket.Listen(maximumDependingConnections);
                SetupConnection(listenSocket);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
        }

        public void End()
        {
            listenSocket.Close();
        }

        protected void SetupConnection(Socket sc)
        {
            try
            {
                AsyncCallback startTransfer = new AsyncCallback(OnStartConnection);
                sc.BeginAccept(startTransfer, sc);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
        }

        protected void OnStartConnection(IAsyncResult ar)
        {
            try
            {
                Socket s = (Socket)ar.AsyncState;
                Transfer t = new Transfer(s.EndAccept(ar));
                FmdcEventArgs e = new FmdcEventArgs(Actions.TransferStarted, t);
                Update(this, e);
                if (!e.Handled)
                    t.Disconnect();
                SetupConnection(s);
            }
            catch (ObjectDisposedException) { /* End has been called */ }
        }
    }
}
