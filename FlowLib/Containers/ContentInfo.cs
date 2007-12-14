
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

using System;
using System.Xml.Serialization;
using FlowLib.Enums;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing Share/Download/Upload Item.
    /// Most probably a file on your system.
    /// </summary>
    public class ContentInfo
    {
        protected string systemPath = string.Empty;
        protected string virtualName = string.Empty;
        protected long modified = 0;
        protected string id = string.Empty;
        protected long size = -1;

        protected ContentIdTypes type = ContentIdTypes.None;
        protected ContentExtend extend = ContentExtend.None;
        #region For content made of files.
        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool IsHiddenSpecified
        {
            get { return IsHidden; }
            set { }
        }
        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool IsSystemSpecified
        {
            get { return IsSystem; }
            set { }
        }
        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool IdSpecified
        {
            get { return ((type | ContentIdTypes.None) == type); }
            set { }
        }
        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool IdTypeSpecified
        {
            get { return ((type | ContentIdTypes.None) == type); }
            set { }
        }
        /// <summary>
        /// Indicates if this content is a filelist
        /// </summary>
        [XmlIgnore()]
        public bool IsFilelist
        {
            get { return ((type | ContentIdTypes.Filelist) == type); }
        }

        /// <summary>
        /// Indicates that file that this info was created from
        /// had the System.IO.FileAttributes.Hidden attribute set.
        /// </summary>
        public bool IsHidden
        {
            get { return ((extend | ContentExtend.Hidden) == extend); }
            set {
                if (((extend | ContentExtend.Hidden) == extend) != value)
                    extend = ~ContentExtend.Hidden;
            }
        }
        /// <summary>
        /// Indicates that file that this info was created from
        /// had the System.IO.FileAttributes.System attribute set.
        /// </summary>
        public bool IsSystem
        {
            get { return ((extend | ContentExtend.System) == extend); }
            set {
                if (((extend | ContentExtend.System) == extend) != value)
                    extend = ~ContentExtend.System;
            }
        }
        #endregion
        /// <summary>
        /// Indicates that the id of this contentinfo is some sort of a hash.
        /// It could be a tiger tree hash but it doesnt have to be.
        /// </summary>
        [XmlIgnore()]
        public bool IsHashed
        {
            get { return ((type | ContentIdTypes.Hash) == type); }
        }
        /// <summary>
        /// Indicates that the id of this contentinfo is a TTH (Tiger Tree Hash)
        /// </summary>
        [XmlIgnore()]
        public bool IsTth
        {
            get { return ((type | ContentIdTypes.TTH) == type); }
        }
        /// <summary>
        /// Id type of this contentinfo
        /// </summary>
        public ContentIdTypes IdType
        {
            get { return type; }
            set { type = value; }
        }
        /// <summary>
        /// Virtual path in your/others share
        /// </summary>
        public string VirtualName
        {
            get { return virtualName; }
            set { virtualName = value; }
        }
        /// <summary>
        /// System path for this file
        /// </summary>
        public string SystemPath
        {
            get { return systemPath; }
            set { systemPath = value; }
        }
        /// <summary>
        /// DateTime tick on when file was last changed
        /// </summary>
        public long LastModified
        {
            get { return modified; }
            set { modified = value; }
        }
        /// <summary>
        /// Id of this contentinfo
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }
        /// <summary>
        /// File size of target
        /// </summary>
        public long Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// Dummy Constructor for Xml Serlization
        /// </summary>
        public ContentInfo()
        {

        }

        /// <summary>
        /// Creating ContentInfo with a id and the id type
        /// </summary>
        /// <param name="id">Id for this ContentInfo</param>
        /// <param name="type">Type of id</param>
        public ContentInfo(string id, ContentIdTypes type)
        {
            this.id = id;
            this.type = type;
        }
    }
}
