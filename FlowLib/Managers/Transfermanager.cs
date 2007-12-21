
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Protocols;

using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

namespace FlowLib.Managers
{
    /// <summary>
    /// Class handling transfers
    /// </summary>
    public class TransferManager
    {
        protected SortedList<string, TransferRequest> requests = new SortedList<string, TransferRequest>();
        //protected SortedList<string, ITransfer> listening = new SortedList<string, ITransfer>();
        protected SortedList<string, ITransfer> transfers = new SortedList<string, ITransfer>();

        public SortedList<string, ITransfer> Transfers
        {
            get { return transfers; }
        }

        public TransferManager()
        {
        }

        /// <summary>
        /// Add Transfer request
        /// This is used when other users want something from us.
        /// </summary>
        /// <param name="key">Unique key (Should be unique even if you are in many hubs with diffrent protocols)</param>
        /// <param name="me">UserInfo of us where request comes from</param>
        /// <param name="share">Share to user for Transfer</param>
        /// <param name="user">UserInfo on user where made the request</param>
        public void AddTransferReq(string key, Hub hub, UserInfo user)
        {
            if (! requests.ContainsKey(key))
                requests.Add(key, new TransferRequest(key, hub, user));
            // TODO : Add req limiter.
        }

        /// <summary>
        /// Add Transfer request
        /// This is used when other users want something from us.
        /// </summary>
        /// <param name="req">Transfer request to add</param>
        public void AddTransferReq(TransferRequest req)
        {
            if (!requests.ContainsKey(req.Key))
                requests.Add(req.Key, req);
            // TODO : Add req limiter.
        }

        /// <summary>
        /// Get Transfer request with the key specified
        /// </summary>
        /// <param name="key">key for request you want</param>
        /// <returns>returns TransferRequest if a match was found, else null</returns>
        public TransferRequest GetTransferReq(string key)
        {
            TransferRequest req = null;
            requests.TryGetValue(key, out req);
            return req;
        }
        /// <summary>
        /// Removing Transfer request with key if found
        /// </summary>
        /// <param name="key">Transfer request</param>
        /// <returns>Returns true if a match was found and removed</returns>
        public bool RemoveTransferReq(string key)
        {
            return requests.Remove(key);
        }

        public void AddTransfer(ITransfer trans)
        {
            string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
            ITransfer old = null;
            if (transfers.TryGetValue(id, out old))
            {
                old.Disconnect("Too many connections.");
                transfers.Remove(id);
            }

            trans.ConnectionStatusChange += new FmdcEventHandler(trans_ConnectionStatusChange);
            // Add transfer to list.
            transfers.Add(id, trans);
        }

        /// <summary>
        /// If connection to remote ip and port already exist that connection will be closed and then the new one will be connected
        /// </summary>
        /// <param name="trans">Transfer you want to start</param>
        public void StartTransfer(ITransfer trans)
        {
            AddTransfer(trans);
            // Connect transfer.
            Thread t = new Thread(new ParameterizedThreadStart(ConnectTransfer));
            t.IsBackground = true;
            t.Start(trans);
        }

        void trans_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            trans.ConnectionStatusChange -= trans_ConnectionStatusChange;
            if (e.Action == TcpConnection.Disconnected)
            {
                string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
                transfers.Remove(id);
            }
        }

        /// <summary>
        /// Disconnects and removes all transfers.
        /// Removes all TransferRequests.
        /// </summary>
        public void Clear()
        {
            foreach (KeyValuePair<string, ITransfer> var in transfers)
            {
                var.Value.Disconnect();
            }
            transfers.Clear();
            requests.Clear();
        }

        public void EndTransfer(ITransfer trans)
        {
            string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
            trans.Disconnect();
            transfers.Remove(id);
        }

        private void ConnectTransfer(object obj)
        {
            if (obj is ITransfer)
            {
                ITransfer t = (ITransfer)obj;
                t.Connect();
            }
        }

        private void OnReceive(System.IAsyncResult ar)
        {
            System.Net.Sockets.UdpClient udp = (System.Net.Sockets.UdpClient)ar.AsyncState;
            System.Net.IPEndPoint sender = new System.Net.IPEndPoint(0, 0);
            byte[] data = udp.EndReceive(ar, ref sender);
        }

    }
}
