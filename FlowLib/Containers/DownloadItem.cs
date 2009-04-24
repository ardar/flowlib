
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
using FlowLib.Events;
using FlowLib.Enums;
using FlowLib.Utils.Convert;
using System.Xml.Serialization;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing Item to be/is downloaded
    /// </summary>
    public class DownloadItem
        : System.Collections.Generic.IComparer<DownloadItem>,
        IComparer,
        System.IComparable
    {
        //public enum DownloadPriority
        //{
        //    Default = -1,
        //    Pause = 0,
        //    Lowest,
        //    Low,
        //    Normal,
        //    High,
        //    Highest
        //}

        //public enum DownloadIdTypes
        //{
        //    FileName = 0,
        //    Tth = 1,
        //    FileList = 2
        //}

        /// <summary>
        /// Segment has been downloaded
        /// </summary>
        public event FmdcEventHandler SegmentCompleted;
        /// <summary>
        /// Downloading of a new segment has been started
        /// </summary>
        public event FmdcEventHandler SegmentStarted;
        /// <summary>
        /// Downloading of a segment has been canceled
        /// </summary>
        public event FmdcEventHandler SegmentCanceled;
        /// <summary>
        /// All segments in this DownloadItem has been downloaded
        /// </summary>
        public event FmdcEventHandler DownloadCompleted;

        #region Variables
        protected ContentInfo info = null;

        protected long segmentSize = 1024 * 1024;
        protected BitArray segmentsDownloaded = null;
        protected BitArray segmentsInProgress = null;
        protected long added = -1;
        protected int segDoneCount = 0;
        protected int segTotalCount = 0;

        //protected DownloadPriority priority = DownloadPriority.Default;
        #endregion

        #region Properties
        [XmlAttribute( AttributeName="DoneSegmentCount")]
        public int DoneSegmentCount
        {
            get { return segDoneCount; }
            set { segDoneCount = value; }
        }

        public int TotalSegmentCount
        {
            get { return segTotalCount; }
            set { segTotalCount = value; }
        }

        public bool[] SegmentsDownloaded
        {
            get
            {
                lock (this)
                {
                    return General.BitArrayToBoolArray(segmentsDownloaded);
                }
            }
            set
            {
                lock (this)
                {
                    segmentsDownloaded = new BitArray(value);
                }
            }
        }
        [System.Xml.Serialization.XmlIgnore()]
        public bool[] SegmentsInProgress
        {
            get
            {
                lock (this)
                {
                    return General.BitArrayToBoolArray(segmentsInProgress);
                }
            }
            set
            {
                lock (this)
                {
                    segmentsInProgress = new BitArray(value);
                }
            }
        }

        /// <summary>
        /// ContentInfo containing systemname and stuff for this download item
        /// </summary>
        public ContentInfo ContentInfo
        {
            get { return info; }
            set {
                info = value; 
            }
        }
        /// <summary>
        /// Segment Size for this download
        /// </summary>
        [XmlAttribute(AttributeName = "SegmentSize")]
        public long SegmentSize
        {
            get { return segmentSize; }
            set { segmentSize = value; }
        }

        //public DownloadPriority Property
        //{
        //    get { return priority; }
        //    set { priority = value; }
        //}

        /// <summary>
        /// Indicates when this item was created
        /// </summary>
        [XmlAttribute(AttributeName = "Added")]
        public long Added
        {
            get { return added; }
            set { added = value; }
        }
        #endregion

        #region Constructor(s)
        /// <summary>
        /// Dummy constructor for xml serialization only
        /// </summary>
        public DownloadItem()
        {
            this.DownloadCompleted = new FmdcEventHandler(DownloadItem_DownloadCompleted);
            this.SegmentCanceled = new FmdcEventHandler(DownloadItem_SegmentCanceled);
            this.SegmentCompleted = new FmdcEventHandler(DownloadItem_SegmentCompleted);
            this.SegmentStarted = new FmdcEventHandler(DownloadItem_SegmentStarted);
        }

        void DownloadItem_SegmentStarted(object sender, FmdcEventArgs e) { }
        void DownloadItem_SegmentCompleted(object sender, FmdcEventArgs e) { }
        void DownloadItem_SegmentCanceled(object sender, FmdcEventArgs e) { }
        void DownloadItem_DownloadCompleted(object sender, FmdcEventArgs e) { }

        /// <summary>
        /// Creating DownloadItem
        /// </summary>
        /// <param name="content">ContentInfo representing fileinfo for this download item</param>
        public DownloadItem(ContentInfo content)
            : this()
        {
            info = content;
            Added = System.DateTime.Now.Ticks;
        }
        #endregion

        /// <summary>
        /// Get first available free segment pos.
        /// -1 means there is no free segments left.
        /// -2 means no size have been set for this download item yet.
        /// </summary>
        /// <returns></returns>
        public SegmentInfo GetAvailable()
        {
            return GetAvailable(null);
        }
        /// <summary>
        /// Get first available free segment pos.
        /// -1 means there is no free segments left.
        /// -2 means no size have been set for this download item yet.
        /// </summary>
        /// <returns></returns>
        public SegmentInfo GetAvailable(Source src)
        {
            lock (this)
            {
                if (info.Size > 0)
                {
                    // Segments has not been set yet. set it.
                    if (segmentsInProgress == null)
                    {
                        long segCount = 0;
                        if ((segCount = (info.Size / segmentSize)) == 0 || (info.Size % segmentSize) != 0)
                            segCount++;
                        segmentsDownloaded = new BitArray((int)segCount);
                        segmentsInProgress = new BitArray((int)segCount);
                        segTotalCount = (int)segCount;
                    }
                    // Get progress
                    BitArray tmp = null;
                            tmp = new BitArray(segmentsDownloaded);
                            tmp = tmp.Or(this.segmentsInProgress);
                    for (int i = 0; i < tmp.Count; i++)
                    {
                        if (!tmp.Get(i))
                        {
                            // Set length to segment size
                            long lengthToDownload = SegmentSize;
                            // Is segment size bigger then content size?
                            if (SegmentSize > ContentInfo.Size)
                            {
                                lengthToDownload = ContentInfo.Size;
                            }
                            // Are we at last segment
                            else if ((1 + i) * SegmentSize > ContentInfo.Size)
                            {
                                lengthToDownload = ContentInfo.Size % SegmentSize;
                            }

                            Start(i, src, false);
                            return new SegmentInfo(i, i * SegmentSize, lengthToDownload);
                        }
                    }
                    return new SegmentInfo(-1);
                }
                return new SegmentInfo(-2);
            }
        }

        public void Cancel(int pos, Source src)
        {
            lock (this)
            {
                if (segmentsInProgress != null && pos >= 0 && segmentsInProgress.Get(pos))
                {
                    segmentsInProgress.Set(pos, false);
                }
            }
            SegmentCanceled(this, new FmdcEventArgs(pos, src));
        }

        public bool Start(int pos, Source src)
        {
            return Start(pos, src, true);
        }
        protected bool Start(int pos, Source src, bool lockResources)
        {
            if (lockResources)
                System.Threading.Monitor.Enter(this);
            bool value;
            try
            {
                value = (!segmentsDownloaded.Get(pos) && !segmentsInProgress.Get(pos));
                if (value)
                {
                    segmentsInProgress.Set(pos, true);
                    SegmentStarted(this, new FmdcEventArgs(pos, src));
                }
            }
            finally
            {
                if (lockResources)
                    System.Threading.Monitor.Exit(this);
            }
            return value;
        }

        public void Finished(int pos, Source src)
        {
            lock (this)
            {
                segmentsDownloaded.Set(pos, true);
                segmentsInProgress.Set(pos, false);
            }
            segDoneCount++;
            // Tell everyone that one segment is finished
            SegmentCompleted(this, new FmdcEventArgs(pos, src));

            // Tell everyone that this downloadItem is finished
            if (segDoneCount == segTotalCount)
            {
                DownloadCompleted(this, new FmdcEventArgs(0, src));
            }
        }

        #region Comparing
        public int Compare(DownloadItem x, DownloadItem y)
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
            DownloadItem xDownloadItem = x as DownloadItem;
            DownloadItem yDownloadItem = y as DownloadItem;
            if (xDownloadItem == null || yDownloadItem == null)
            {
                if (xDownloadItem == null && yDownloadItem == null)
                {
                    return System.Collections.Generic.Comparer<object>.Default.Compare(x, y);
                }
                else if (xDownloadItem == null)
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
                return xDownloadItem.Compare(xDownloadItem, yDownloadItem);
            }

        }
        public int CompareTo(object obj)
        {
            return Compare(this, obj);
        }
        #endregion
        #region XmlSeralization
        [XmlIgnore()]
        public bool DoneSegmentCountSpecified
        {
            get { return segDoneCount != 0; }
            set { }
        }
        [XmlIgnore()]
        public bool AddedSpecified
        {
            get { return Added != 0; }
            set { }
        }
        [XmlIgnore()]
        public bool SegmentsDownloadedSpecified
        {
            get { return segmentsDownloaded != null && segmentsDownloaded.Count != 0; }
            set { }
        }
        [XmlIgnore()]
        public bool TotalSegmentCountSpecified
        {
            get { return TotalSegmentCount != 0; }
            set { }
        }
        [XmlIgnore()]
        public bool SegmentSizeSpecified
        {
            get { return SegmentSize != 1024 * 1024; }
            set { }
        }
        #endregion
    }
}
