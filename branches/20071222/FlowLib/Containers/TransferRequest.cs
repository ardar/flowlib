
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

using FlowLib.Interfaces;
using FlowLib.Connections;

namespace FlowLib.Containers
{
    public class TransferRequest
    {
        UserInfo user = null;
        Hub hub = null;
        string key = null;

        public string Key
        {
            get { return key; }
            set { key = value; }
        }

        public UserInfo Me
        {
            get { return hub.Me; }
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

        public TransferRequest(string pKey, Hub pHub, UserInfo pUser)
        {
            key = pKey;
            hub = pHub;
            user = pUser;
        }
    }
}
