
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

using FlowLib.Interfaces;
using FlowLib.Events;
using FlowLib.Containers;
using FlowLib.Managers;

namespace FlowLib.Connections
{
    /// <summary>
    /// Represents a User to User Connection in the p2p network Direct Connect.
    /// </summary>
    public class Transfer : TcpConnection, ITransfer
    {
        /// <summary>
        /// Occurs when DownloadItem has been changed.
        /// </summary>
        public event FmdcEventHandler DownloadItemChanged;

        protected UserInfo me = null;
        protected UserInfo user = null;
        protected Share share = null;
        protected DownloadItem downloadItem = null;
        protected int downloadSegmentId = -1;
        protected ContentInfo info = new ContentInfo();
        protected SegmentInfo currentSegment = new SegmentInfo(-1);
        protected long lastEventTimeStamp = 0;
        protected Source source = null;

        public Source Source
        {
            get { return source; }
            set { source = value; }
        }

        public long LastEventTimeStamp
        {
            get { return lastEventTimeStamp; }
            set { lastEventTimeStamp = value; }
        }

        public new IProtocolTransfer Protocol
        {
            get { return (IProtocolTransfer)protocol; }
            set { protocol = (IProtocol)value; }
        }

        public SegmentInfo CurrentSegment
        {
            get { return currentSegment; }
            set { currentSegment = value; }
        }

        /// <summary>
        /// Current ContentInfo for this connection.
        /// </summary>
        public ContentInfo Content
        {
            get { return info; }
            set { info = value; }
        }

        /// <summary>
        /// Other user we are connected to
        /// </summary>
        public UserInfo User
        {
            get { return user; }
            set { user = value; }
        }

        /// <summary>
        /// DownloadItem 
        /// </summary>
        public DownloadItem DownloadItem
        {
            get { return downloadItem; }
            set
            {
                downloadItem = value;
                DownloadItemChanged(this, new FmdcEventArgs(0, downloadItem));
            }
        }

        /// <summary>
        /// Sharing instance for this transfer
        /// </summary>
        public override Share Share
        {
            get { return share; }
            set { share = value; }
        }

        /// <summary>
        /// User that representate us
        /// </summary>
        public UserInfo Me
        {
            get { return me; }
            set { me = value; }
        }

        /// <summary>
        /// Creating Transfer with s as the underlaying socket
        /// </summary>
        /// <param name="s">Socket you want to have as underlaying connection</param>
        public Transfer(System.Net.Sockets.Socket s)
            : base(s)
        {
            DownloadItemChanged = new FmdcEventHandler(OnDownloadItemChanged);
            if (socket.Connected)
            {
                localAddress = (System.Net.IPEndPoint)socket.LocalEndPoint;
                remoteAddress = (System.Net.IPEndPoint)socket.RemoteEndPoint;
            }
        }

        /// <summary>
        /// Creating Transfer with address and prt.
        /// </summary>
        /// <param name="addy">string representation of a IP/DNS address</param>
        /// <param name="prt">int port representation</param>
        public Transfer(string addy, int port)
            : this(addy, port, null)
        {

        }

        /// <summary>
        /// Creating Transfer with addy as the remote address.
        /// </summary>
        /// <param name="addy">IPEndPoint that we want as our remote address</param>
        public Transfer(System.Net.IPEndPoint addy)
            : base(addy)
        {
            DownloadItemChanged = new FmdcEventHandler(OnDownloadItemChanged);
        }

        /// <summary>
        /// Creating Transfer with address and prt.
        /// </summary>
        /// <param name="addy">string representation of a IP/DNS address</param>
        /// <param name="prt">int port representation</param>
        /// <param name="protocol">Protocol to be used by this connection</param>
        public Transfer(string addy, int port, IProtocolTransfer protocol)
            : base(addy, port)
        {
            DownloadItemChanged = new FmdcEventHandler(OnDownloadItemChanged);
            this.Protocol = protocol;
        }

        #region Events
        void OnDownloadItemChanged(object sender, FmdcEventArgs e) { }
        #endregion
    }
}
