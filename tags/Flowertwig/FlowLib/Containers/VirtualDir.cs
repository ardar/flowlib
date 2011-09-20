
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
using System.Xml.Serialization;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing Virtual directory in Share
    /// </summary>
    public class VirtualDir
    {
        protected string virtualPath = string.Empty;
        protected string systemPath = string.Empty;
        protected long totalSize = 0;
        protected long totalCount = 0;
        protected long tthSize = 0;
        protected long tthCount = 0;
        protected bool isChanging = false;
        protected bool isHashing = false;

        /// <summary>
        /// Virtual name for systempath
        /// </summary>
        public string VirtualPath
        {
            get { return virtualPath; }
            set { virtualPath = value; }
        }
        /// <summary>
        /// File system path
        /// </summary>
        public string SystemPath
        {
            get { return systemPath; }
            set { systemPath = value; }
        }
        /// <summary>
        /// Gets total ContentInfo size that has been hashed
        /// </summary>
        public long HashedSize
        {
            get { return tthSize; }
            set { tthSize = value; }
        }
        /// <summary>
        /// Gets ContentInfo count that has been hashed
        /// </summary>
        public long HashedCount
        {
            get { return tthCount; }
            set { tthCount = value; }
        }
        /// <summary>
        /// Gets total Content size in share
        /// </summary>
        public long TotalSize
        {
            get { return totalSize; }
            set { totalSize = value; }
        }
        /// <summary>
        /// Gets total ContentInfo count in share
        /// </summary>
        public long TotalCount
        {
            get { return totalCount; }
            set { totalCount = value; }
        }
        /// <summary>
        /// Indicates if changes to this virtual dir is currently done
        /// </summary>
        [XmlIgnore()]
        public bool IsChanging
        {
            get { return isChanging; }
        }
        /// <summary>
        /// Indicates if hashing to this virtual dir is currently done
        /// </summary>
        [XmlIgnore()]
        public bool IsHashing
        {
            get { return isHashing; }
            set { isHashing = value; }

        }

        /// <summary>
        /// Default constructor, for XML handling.
        /// </summary>
        public VirtualDir() { }
        /// <summary>
        /// Creates VirtualDir
        /// </summary>
        /// <param name="system">file system path</param>
        /// <param name="virt">virtual name for filesystem dir</param>
        public VirtualDir(string system, string virt)
        {
            systemPath = system;
            virtualPath = virt;
        }
    }
}
