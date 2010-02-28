
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

using System.Collections;
using System.Collections.Generic;
using System;
using System.Xml.Serialization;
using FlowLib.Enums;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing Share/Download/Upload Item.
    /// Most probably a file on your system.
    /// </summary>
    public class ContentInfo :
        PropertyContainer<string, string>,
        IComparer<ContentInfo>,
        IComparer,
        IComparable
    {
        public const string ID = "id";
        public const string TTH = "tth";
        public const string TTHL = "tthl";
        /// <summary>
        /// Virtual path in your/others share
        /// </summary>
        public const string VIRTUAL = "virtual";
        public const string NAME = "name";
        /// <summary>
        /// Storage path for this content
        /// </summary>
        public const string STORAGEPATH = "storage";
        public const string FILELIST = "filelist";
        public const string REQUEST = "request";   // This should only be set when we want / send content

        protected long modified = 0;
        protected long size = -1;

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
        /// Indicates if this content is a filelist
        /// </summary>
        [XmlIgnore()]
        public bool IsFilelist
        {
            get { return ContainsKey(FILELIST); }
        }

        /// <summary>
        /// Indicates that file that this info was created from
        /// had the System.IO.FileAttributes.Hidden attribute set.
        /// </summary>
        public bool IsHidden
        {
            get { return ((extend | ContentExtend.Hidden) == extend); }
            set
            {
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
            set
            {
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
            get
            { return ContainsKey(TTH); }
        }
        /// <summary>
        /// Indicates that the id of this contentinfo is a TTH (Tiger Tree Hash)
        /// </summary>
        [XmlIgnore()]
        public bool IsTth
        {
            get { return ContainsKey(TTH); }
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
        /// File size of target
        /// </summary>
        [XmlAttribute(AttributeName = "Size")]
        public long Size
        {
            get { return size; }
            set { size = value; }
        }

        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool LastModifiedSpecified
        {
            get { return LastModified != 0; }
            set { }
        }

        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        [XmlIgnore()]
        public bool SizeSpecified
        {
            get { return Size != 0; }
            set { }
        }

        /// <summary>
        /// Dummy Constructor for Xml Serlization
        /// </summary>
        public ContentInfo()
        {
        }

        /// <summary>
        /// Creating ContentInfo with key and its value
        /// </summary>
        /// <param name="key">Key to add value for</param>
        /// <param name="value">value that should be added</param>
        public ContentInfo(string key, string value)
        {
            Set(key, value);
        }

        protected int Compare(bool x, bool y)
        {
            // are x and y valid?
            if (x || y)
            {
                if (x && y)
                    return 0;
                else if (x)
                    return -1;
                else
                    return 1;
            }
            return -2;
        }

        public int Compare(ContentInfo x, ContentInfo y)
        {
            // are x and y valid?
            int i;
            if ((i = Compare(x == null, y == null)) != -2)
                return i;
			if ((i = Compare(x.ContainsKey(ID), y.ContainsKey(ID))) != -2)
				if (i == 0)
					return string.Compare(x[ID], y[ID]);
				else
					return i;
			if ((i = Compare(x.ContainsKey(TTH), y.ContainsKey(TTH))) != -2)
				if (i == 0)
					return string.Compare(x[TTH], y[TTH]);
				else
					return i;
			if ((i = Compare(x.ContainsKey(VIRTUAL), y.ContainsKey(VIRTUAL))) != -2)
				if (i == 0)
					return string.Compare(x[VIRTUAL], y[VIRTUAL]);
				else
					return i;
            if ((i = x.LastModified.CompareTo(y.LastModified)) != 0)
                return i;
            if ((i = x.Size.CompareTo(y.Size)) != 0)
                return i;
            // We dont know how to compare them anymore so make them equal :)
            return 0;
        }
        public int Compare(object x, object y)
        {
            ContentInfo xContent = x as ContentInfo;
            ContentInfo yContent = y as ContentInfo;
            return Compare(xContent, yContent);
        }
        public int CompareTo(object obj)
        {
            return Compare(this, obj);
        }
    }
}
