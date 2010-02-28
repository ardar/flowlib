
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
    /// Class representing a download source (User used for downloading)
    /// </summary>
    public class Source
        : System.Collections.Generic.IComparer<Source>,
        System.Collections.IComparer,
        System.IComparable
    {
        protected string conId = string.Empty;
        protected string userId = string.Empty;
        /// <summary>
        /// Connection Id
        /// </summary>
        [XmlAttribute(AttributeName = "ConId")]
        public string ConnectionId
        {
            get { return conId; }
			set { conId = value; }
        }
        /// <summary>
        /// User Id
        /// </summary>
        [XmlAttribute(AttributeName = "UserId")]
        public string UserId
        {
            get { return userId; }
			set { userId = value; }
        }

        /// <summary>
        /// Create source, used for xml seralization
        /// </summary>
        protected Source() { }

        /// <summary>
        /// Creates source from connection id and user id
        /// </summary>
        /// <param name="conId">Connection id</param>
        /// <param name="userId">User id</param>
        public Source(string conId, string userId)
        {
            this.conId = conId;
            this.userId = userId;
        }

        public override string ToString()
        {
            return string.Format("Source[{0}, {1}]", conId, userId);
        }

        #region Comparing
        public int Compare(Source x, Source y)
        {
            int cmp = 0;
            #region Compares Connection Id
            if (x.conId == null || y.conId == null)
            {
                if (x.conId == null && y.conId == null)
                {

                }
                else if (x.conId == null)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if ((cmp = x.conId.CompareTo(y.conId)) != 0)
                    return cmp;
            }
            #endregion
            #region Compares User Id
            if (x.userId == null || y.userId == null)
            {
                if (x.userId == null && y.userId == null)
                {
                    return 0;
                }
                else if (x.userId == null)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return x.userId.CompareTo(y.userId);
            }
            #endregion
        }

        public int Compare(object x, object y)
        {
            Source xSource = x as Source;
            Source ySource = y as Source;
            if (xSource == null || ySource == null)
            {
                if (xSource == null && ySource == null)
                {
                    return System.Collections.Generic.Comparer<object>.Default.Compare(x, y);
                }
                else if (xSource == null)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return xSource.Compare(xSource, ySource);
            }
        }

        public int CompareTo(object obj)
        {
            return this.Compare(this, obj);
        }
        #endregion
    }
}
