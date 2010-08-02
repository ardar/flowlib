
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
namespace FlowLib.Containers
{
    public class UploadItem
        : System.Collections.Generic.IComparer<UploadItem>,
        IComparer,
        System.IComparable
    {
        public Source Source
        {
            get;
            set;
        }

        public ContentInfo ContentInfo
        {
            get;
            set;
        }

        public SegmentInfo Segment
        {
            get;
            set;
        }

        public UploadItem(ContentInfo content, Source src, SegmentInfo seg)
        {
            ContentInfo = content;
            Source = src;
            Segment = seg;
        }

        #region Comparing
        public int Compare(UploadItem x, UploadItem y)
        {
            // are x and y valid?
            if (x == null || y == null)
            {
                if (x == null && y == null)
                    return 0;
                else if (x == null)
                    return -1;
                else
                    return 1;
            }
            return x.ContentInfo.Compare(x.ContentInfo, y.ContentInfo);
        }
        public int Compare(object x, object y)
        {
            UploadItem xItem = x as UploadItem;
            UploadItem yItem = y as UploadItem;
            if (xItem == null || yItem == null)
            {
                if (xItem == null && yItem == null)
                {
                    return System.Collections.Generic.Comparer<object>.Default.Compare(x, y);
                }
                else if (xItem == null)
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
                return xItem.Compare(xItem, yItem);
            }

        }
        public int CompareTo(object obj)
        {
            return Compare(this, obj);
        }
        #endregion
    }
}
