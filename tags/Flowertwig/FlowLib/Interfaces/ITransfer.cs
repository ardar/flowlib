
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

namespace FlowLib.Interfaces
{
    public interface ITransfer : IConnection
    {
        /// <summary>
        /// Occurs when DownloadItem has been changed.
        /// </summary>
        event FmdcEventHandler DownloadItemChanged;

        SegmentInfo CurrentSegment
        {
            get;
            set;
        }

        long LastEventTimeStamp
        {
            get;
            set;
        }

        /// <summary>
        /// Source releated to this transfer
        /// </summary>
        Source Source
        {
            get;
            set;
        }

        /// <summary>
        /// Current ContentInfo for this connection.
        /// </summary>
        DownloadItem DownloadItem
        {
            get;
            set;
        }
        /// <summary>
        /// User that representate us
        /// </summary>
        UserInfo Me
        {
            get;
            set;
        }
        /// <summary>
        /// Other user we are connected to
        /// </summary>
        UserInfo User
        {
            get;
            set;
        }

        /// <summary>
        /// Current ContentInfo for this connection.
        /// </summary>
        ContentInfo Content
        {
            get;
            set;
        }
    }
}
