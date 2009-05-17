
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
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Protocols;

using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Generic;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Managers
{
    /// <summary>
    /// Class handling transfers
    /// </summary>
    public class TransferManager
    {
        protected SortedList<string, TransferRequest> requests = new SortedList<string, TransferRequest>();
        protected SortedList<string, ITransfer> transfers = new SortedList<string, ITransfer>();

		public SortedList<string, TransferRequest> Requests
		{
			get
            {
                lock (this)
                {
                    return requests;
                }
            }
		}

        public SortedList<string, ITransfer> Transfers
        {
            get
            {
                lock (this)
                {
                    return transfers;
                }
            }
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
            lock (this)
            {
                if (!requests.ContainsKey(key))
                    requests.Add(key, new TransferRequest(key, hub, user));
            }
            // TODO : Add req limiter.
        }

        /// <summary>
        /// Add Transfer request
        /// This is used when other users want something from us.
        /// </summary>
        /// <param name="req">Transfer request to add</param>
        public void AddTransferReq(TransferRequest req)
        {
            lock (this)
            {
                if (!requests.ContainsKey(req.Key))
                    requests.Add(req.Key, req);
            }
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
            lock (this)
            {
                requests.TryGetValue(key, out req);
            }
            return req;
        }
        /// <summary>
        /// Removing Transfer request with key if found
        /// </summary>
        /// <param name="key">Transfer request</param>
        /// <returns>Returns true if a match was found and removed</returns>
        public bool RemoveTransferReq(string key)
        {
            lock (this)
            {
                return requests.Remove(key);
            }
        }

        public void AddTransfer(ITransfer trans)
        {
            string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
            RemoveTransfer(id);
            trans.ConnectionStatusChange += new FmdcEventHandler(trans_ConnectionStatusChange);
            // Add transfer to list.
            lock (this)
            {
                transfers.Add(id, trans);
            }
        }

        public ITransfer GetTransfer(string key)
        {
            ITransfer old = null;
            lock (this)
            {
                transfers.TryGetValue(key, out old);
            }
            return old;
        }

        public bool RemoveTransfer(string key)
        {
            ITransfer old = null;
            lock (this)
            {
                if (transfers.TryGetValue(key, out old))
                {
                    old.Disconnect("Too many connections.");
                    return transfers.Remove(key);
                }
            }
            return false;
        }

        /// <summary>
        /// If connection to remote ip and port already exist that connection will be closed and then the new one will be connected
        /// </summary>
        /// <param name="trans">Transfer you want to start</param>
        public void StartTransfer(ITransfer trans)
        {
            AddTransfer(trans);
            // Connect transfer.
#if !COMPACT_FRAMEWORK
            Thread t = new Thread(new ParameterizedThreadStart(ConnectTransfer));
            t.IsBackground = true;
            t.Start(trans);
#else
            Thread t = new Thread(new ThreadStart(ConnectTransfer));
            t.IsBackground = true;
            t.Start(trans);
#endif
        }

        void trans_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            ITransfer trans = sender as ITransfer;
            if (e.Action == TcpConnection.Disconnected)
            {
                trans.ConnectionStatusChange -= trans_ConnectionStatusChange;
                string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
                lock (this)
                {
                    transfers.Remove(id);
                }
            }
        }

        /// <summary>
        /// Disconnects and removes all transfers.
        /// Removes all TransferRequests.
        /// </summary>
        public void Clear()
        {
            SortedList<string, ITransfer> tmpList = null;
            lock (this)
            {
                tmpList = new SortedList<string, ITransfer>(transfers);
            }
            foreach (KeyValuePair<string, ITransfer> var in tmpList)
            {
                var.Value.Disconnect();
            }
            lock (this)
            {
                transfers.Clear();
                requests.Clear();
            }
        }

        public void EndTransfer(ITransfer trans)
        {
            string id = string.Format("{0}{1}", trans.RemoteAddress.Address.ToString(), trans.RemoteAddress.Port);
            trans.Disconnect();
            lock (this)
            {
                transfers.Remove(id);
            }
        }

#if COMPACT_FRAMEWORK
        private void ConnectTransfer()
        {
            ConnectTransfer(Thread.CurrentThread.GetData());
        }
#endif
        private void ConnectTransfer(object obj)
        {
            if (obj is ITransfer)
            {
                ITransfer t = (ITransfer)obj;
                t.Connect();
            }
        }
    }
}
