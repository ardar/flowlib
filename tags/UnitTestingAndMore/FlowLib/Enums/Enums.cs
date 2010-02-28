
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

namespace FlowLib.Enums
{
    /// <summary>
    /// Enum representing Connection Types
    /// Not all are used for public use in Direct Connect
    /// </summary>
    public enum ConnectionTypes
    {
        /// <summary>
        /// Unknown connection type
        /// </summary>
        Unknown = 0,
        /// <summary>
        /// Direct connection (Active mode in Direct Connect Protocol)
        /// </summary>
        Direct,
        /// <summary>
        /// Universal Plug and Play, (Active mode in Direct Connect Protocol)
        /// http://en.wikipedia.org/wiki/Universal_Plug_and_Play
        /// </summary>
        UPnP,
        /// <summary>
        /// Port Forwarding (Firewall, Active mode in Direct Connect Protocol)
        /// </summary>
        Forward,
        /// <summary>
        /// Behind Firewall, (Passive mode in Direct Connect Protocol)
        /// </summary>
        Passive,
        /// <summary>
        /// Socket5 (Passive mode in Direct Connect Protocol)
        /// </summary>
        Socket5
    }
    /// <summary>
    /// Enum used in ContentInfo for showing extended info
    /// </summary>
    [Flags]
    public enum ContentExtend : byte
    {
        None = 0,
        Hidden = 1,
        System = 2
    }
    /// <summary>
    /// Id types, example for ContentInfo
    /// </summary>
    [Flags]
    public enum ContentIdTypes : byte
    {
        /* Has no Id */
        None = 0,
        /// <summary>
        /// Mainly for DownloadItem, we have no hash for this file, just virtualname
        /// </summary>
        Filename,
        /// <summary>
        /// Mainly for DownloadItem, this is a filelist
        /// </summary>
        Filelist,
        FilelistBz = Filelist | 1,
        FilelistXmlBz = Filelist | 2,
        /// <summary>
        /// Mainly for DownloadItem, this is a filelist that is just downloaded to get more alternative sources
        /// </summary>
        Parent,
        /* Id is a hash. It could be TTH but it doesnt have to be */
        Hash,
        /* Id is a hash, tiger tree hash to be exact */
        TTH = Hash | 1
    }

    public enum TransferErrors
    {
        UNKNOWN = 0,
        INACTIVITY = 1,
        NO_FREE_SLOTS = 2,
        FILE_NOT_AVAILABLE = 4,
        USERID_MISMATCH = 8
    }

    /// <summary>
    /// Secure Protocols, can be used to encrypt TcpConnections
    /// </summary>
    [Flags]
    public enum SecureProtocols
    {
        None = 0,
        SSL2 = 1,
        SSL3 = 2,
        TLS = 4
    }
}
