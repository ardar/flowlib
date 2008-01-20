
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using FlowLib.Connections;

namespace FlowLib.Containers
{
    public class TransferRequest
    {
        UserInfo user = null;
        Hub hub = null;
        string key = null;
        int meDownload = -1;      // Not set (We dont know who is going to download)

        public bool Download
        {
            get { return (meDownload != 0); }
        }

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public UserInfo Me
        {
            get { return hub.Me; }
        }

        public Source Source
        {
            get
            {
                string conId = null;
                if (hub != null || hub.RemoteAddress != null)
                {
                    conId = hub.RemoteAddress.ToString();
                }
                return new Source(conId, user.StoreID);
            }
        }

        public UserInfo User
        {
            get { return user; }
            set { user = value;}
        }

        public Share Share
        {
            get { return hub.Share; }
        }

        public int Port
        {
            get
            {
                if (hub.Share != null)
                    return hub.Share.Port;
                return -1;
            }
        }

        /// <summary>
        /// Creates a Transfer request
        /// </summary>
        /// <param name="pKey">Unique key that identifies this request</param>
        /// <param name="pHub">Hub where request came from</param>
        /// <param name="pUser">User that transfer request is for</param>
        /// <param name="download">telling if we know that _we_ will download</param>
        public TransferRequest(string pKey, Hub pHub, UserInfo pUser, bool download)
            : this(pKey, pHub, pUser)
        {
            if (download)
                meDownload = 1;
            else
                meDownload = 0;
        }

        /// <summary>
        /// Creates a Transfer request
        /// </summary>
        /// <param name="pKey">Unique key that identifies this request</param>
        /// <param name="pHub">Hub where request came from</param>
        /// <param name="pUser">User that transfer request is for</param>
        public TransferRequest(string pKey, Hub pHub, UserInfo pUser)
        {
            key = pKey;
            hub = pHub;
            user = pUser;
        }
    }
}
