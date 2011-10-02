
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using System.Collections.Generic;
using System.Xml.Serialization;
using Flowertwig.Utils.Entities;
using Flowertwig.Utils.Events;
using Flowertwig.Utils.IO;
using FlowLib.Entities;

namespace FlowLib.Managers
{
    /// <summary>
    /// Class handling downloadItems
    /// </summary>
    [System.Serializable()]
    public class DownloadManager
    {
        /// <summary>
        /// Downloading have been completed of segment
        /// </summary>
        public event EventHandler SegmentCompleted;
        /// <summary>
        /// Downloading of segment have been started
        /// </summary>
        public event EventHandler SegmentStarted;
        /// <summary>
        /// Segment downloading have been canceled
        /// </summary>
        public event EventHandler SegmentCanceled;
        /// <summary>
        /// DownloadItem have been completed (Download)
        /// </summary>
        public event EventHandler DownloadCompleted;
        /// <summary>
        /// DownloadItem have been added to DownloadManager
        /// </summary>
        public event EventHandler DownloadAdded;
        /// <summary>
        /// DownloadItem have been removed from DownloadManager
        /// </summary>
        public event EventHandler DownloadRemoved;
        /// <summary>
        /// Source have been added to DownloadManager
        /// </summary>
        public event EventHandler SourceAdded;
        /// <summary>
        /// Source have been removed from DownloadManager
        /// </summary>
        public event EventHandler SourceRemoved;

        protected SortedList<DownloadItem, FlowSortedList<Source>> dwnItems = new SortedList<DownloadItem, FlowSortedList<Source>>();
        protected SortedList<Source, FlowSortedList<DownloadItem>> srcItems = new SortedList<Source, FlowSortedList<DownloadItem>>();

        protected string directory = null;

        [XmlIgnore()]
        public IList<DownloadItem> DownloadItems
        {
            get
            {
                lock (this)
                {
                    return dwnItems.Keys;
                }
            }
        }

        [XmlIgnore()]
        public IList<Source> SourceItems
        {
            get
            {
                lock (this)
                {
                    return srcItems.Keys;
                }
            }
        }

        /// <summary>
        /// This property is here for xml serialization only.
        /// </summary>
        public StoredDownload[] Items
        {
            get
            {
                lock (this)
                {
                    List<StoredDownload> tmpList = new List<StoredDownload>();
                    foreach (var item in dwnItems)
                    {
                        tmpList.Add(new StoredDownload
                        {
                            DownloadItem = item.Key,
                            Sources = item.Value.ToArray()
                        });
                        
                    }
                    return tmpList.ToArray();
                }
            }
            set
            {
                lock (this)
                {
                    foreach (var item in value)
                    {
                        AddDownload((DownloadItem) item.DownloadItem, (Source[]) item.Sources);
                    }
                }
            }
        }

        [System.Xml.Serialization.XmlIgnore()]
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Init listeners for this static class
        /// </summary>
        public DownloadManager()
        {
            FileName = "Downloads";

            SegmentCanceled = new EventHandler(DownloadManager_SegmentCanceled);
            SegmentCompleted = new EventHandler(DownloadManager_SegmentCompleted);
            SegmentStarted = new EventHandler(DownloadManager_SegmentStarted);
            DownloadCompleted = new EventHandler(DownloadManager_DownloadCompleted);

            DownloadAdded = new EventHandler(DownloadManager_DownloadAdded);
            DownloadRemoved = new EventHandler(DownloadManager_DownloadRemoved);
            SourceAdded = new EventHandler(DownloadManager_SourceAdded);
            SourceRemoved = new EventHandler(DownloadManager_SourceRemoved);
        }

        void DownloadManager_SourceRemoved(object sender, DefaultEventArgs e) { }
        void DownloadManager_SourceAdded(object sender, DefaultEventArgs e) { }
        void DownloadManager_DownloadRemoved(object sender, DefaultEventArgs e) { }
        void DownloadManager_DownloadAdded(object sender, DefaultEventArgs e) { }
        void DownloadManager_DownloadCompleted(object sender, DefaultEventArgs e) { }
        void DownloadManager_SegmentStarted(object sender, DefaultEventArgs e) { }
        void DownloadManager_SegmentCompleted(object sender, DefaultEventArgs e) { }
        void DownloadManager_SegmentCanceled(object sender, DefaultEventArgs e) { }

        /// <summary>
        /// Remove source from all downloads
        /// </summary>
        /// <param name="s">Source to remove</param>
        public void RemoveSource(Source s)
        {
            RemoveSource(s, true);
        }

        protected void RemoveSource(Source s, bool shouldLock)
        {
            if (srcItems.ContainsKey(s))
            {
                if (shouldLock)
                    System.Threading.Monitor.Enter(this);
                try
                {
                    FlowSortedList<DownloadItem> tmpSrc = srcItems[s];
                    if (tmpSrc != null)
                    {
                        foreach (DownloadItem var in tmpSrc)
                        {
                            if (dwnItems.ContainsKey(var))
                            {
                                FlowSortedList<Source> tmpDwn = dwnItems[var];
                                if (tmpDwn != null)
                                {
                                    tmpDwn.Remove(s);
                                }
                            }
                        }
                    }
                    srcItems.Remove(s);
                }
                finally
                {
                    if (shouldLock)
                        System.Threading.Monitor.Exit(this);
                }
                SourceRemoved(this, new DefaultEventArgs(0, s));
            }
        }
        /// <summary>
        /// Remove DownloadItem
        /// </summary>
        /// <param name="d">DownloadItem to remove</param>
        public void RemoveDownload(DownloadItem d)
        {
            RemoveDownload(d, true);
        }
        protected void RemoveDownload(DownloadItem d, bool shouldLock)
        {
            if (dwnItems.ContainsKey(d))
            {
                if (shouldLock)
                    System.Threading.Monitor.Enter(this);
                try
                {
                    FlowSortedList<Source> tmpDwn = dwnItems[d];
                    if (tmpDwn != null)
                    {
                        foreach (Source var in tmpDwn)
                        {
                            if (srcItems.ContainsKey(var))
                            {
                                FlowSortedList<DownloadItem> tmpSrc = srcItems[var];
                                if (tmpSrc != null)
                                {
                                    tmpSrc.Remove(d);
                                    if (tmpSrc.Count == 0)
                                        srcItems.Remove(var);
                                }
                            }
                        }
                    }
                    dwnItems.Remove(d);
                }
                finally
                {
                    if (shouldLock)
                        System.Threading.Monitor.Exit(this);
                }
                DownloadRemoved(this, new DefaultEventArgs(0, d));
                d.DownloadCompleted -= d_DownloadCompleted;
                d.SegmentCanceled -= d_SegmentCanceled;
                d.SegmentCompleted -= d_SegmentCompleted;
                d.SegmentStarted -= d_SegmentStarted;
            }
        }
        /// <summary>
        /// Adds downloaditem related to Source
        /// Source can be null
        /// </summary>
        /// <param name="d">DownloadItem to be added</param>
        /// <param name="s">Source to be related to downloaditem</param>
        public void AddDownload(DownloadItem d, Source s)
        {
            AddDownload(d, new Source[] { s });
        }

        /// <summary>
        /// Adds downloaditem related to Source
        /// Source can be null
        /// </summary>
        /// <param name="d">DownloadItem to be added</param>
        /// <param name="sources">Sources to be related to downloaditem</param>
        public void AddDownload(DownloadItem d, Source[] sources)
        {
            // Downloads
            FlowSortedList<Source> tmpDwn = null;
            if (!dwnItems.ContainsKey(d))
            {
                lock (this)
                {
                    tmpDwn = new FlowSortedList<Source>();
                    d.DownloadCompleted += new EventHandler(d_DownloadCompleted);
                    d.SegmentCanceled += new EventHandler(d_SegmentCanceled);
                    d.SegmentCompleted += new EventHandler(d_SegmentCompleted);
                    d.SegmentStarted += new EventHandler(d_SegmentStarted);

                    dwnItems.Add(d, tmpDwn);
                }
                DownloadAdded(this, new DefaultEventArgs(0, d));
            }
            else
            {
                lock (this)
                {
                    // This is if we have a fake downloaditem (that we probably have)
                    d = dwnItems.Keys[dwnItems.IndexOfKey(d)];
                    tmpDwn = dwnItems[d];
                }
            }
            foreach (Source s in sources)
            {
                if (s != null)
                {
                    lock (this)
                    {
                        tmpDwn.Add(s);

                        // Sources
                        FlowSortedList<DownloadItem> tmpSrc = null;
                        if (!srcItems.ContainsKey(s))
                        {
                            tmpSrc = new FlowSortedList<DownloadItem>();
                            srcItems.Add(s, tmpSrc);
                            SourceAdded(this, new DefaultEventArgs(0, s));
                        }
                        else
                        {
                            tmpSrc = srcItems[s];
                        }
                        tmpSrc.Add(d);
                    }
                }
            }
        }

        /// <summary>
        /// Adds Source related to downloaditem
        /// Same as AddDownload. If Download exist source will just be added to downloaditem.
        /// </summary>
        /// <param name="s">Source to be related to downloaditem</param>
        /// <param name="d">DownloadItem to relate to source</param>
        public void AddSource(Source s, DownloadItem d)
        {
            AddDownload(d, s);
        }

        /// <summary>
        /// Removes all downloads and sources
        /// </summary>
        public void Clear()
        {
            lock (this)
            {
                foreach (KeyValuePair<DownloadItem, FlowSortedList<Source>> var in dwnItems)
                {
                    RemoveDownload(var.Key, false);
                }
                foreach (KeyValuePair<Source, FlowSortedList<DownloadItem>> var in srcItems)
                {
                    RemoveSource(var.Key, false);
                }
            }
        }

        void d_SegmentStarted(object sender, DefaultEventArgs e)
        {
            SegmentStarted(sender, e);
        }
        void d_SegmentCompleted(object sender, DefaultEventArgs e)
        {
            SegmentCompleted(sender, e);
        }

        void d_SegmentCanceled(object sender, DefaultEventArgs e)
        {
            SegmentCanceled(sender, e);
        }

        private void d_DownloadCompleted(object sender, DefaultEventArgs e)
        {
            DownloadItem dwnItm = sender as DownloadItem;
            if (dwnItm != null)
            {
                //RemoveDownload((DownloadItem)sender);
                RemoveDownload(dwnItm);
                FileOperations.ForceClose(dwnItm.ContentInfo.Get(ContentInfo.STORAGEPATH));
                DownloadCompleted(sender, e);
            }
        }

        // TODO : Add match source
        //public static void MatchSource(Share share, Source source)
        //{
        //    List<DownloadItem> list = new List<DownloadItem>();


        //    foreach (KeyValuePair<DownloadItem, FlowSortedList<Source>> item in downloadItems)
        //    {
        //        // TODO : Add check here
        //        //item.Key.
        //    }

        //    foreach (ContentInfo item in share)
        //    {

        //    }
        //    //share.GetEnumerator()
        //}

        //public static void MatchSource(SearchResult sr, Source source)
        //{
        //}

        /// <summary>
        /// Do downloadmanager contains source?
        /// </summary>
        /// <param name="s">Do this Source exist</param>
        /// <returns>Returns true if source exist</returns>
        public bool ContainsSource(Source s)
        {
            return srcItems.ContainsKey(s);
        }

        /// <summary>
        /// Do downloadmanager contains downloaditem
        /// </summary>
        /// <param name="d">Do this DownloadItem exist</param>
        /// <returns>Return true if downloaditem exist</returns>
        public bool ContainsDownload(DownloadItem d)
        {
            return dwnItems.ContainsKey(d);
        }

        /// <summary>
        /// If DownloadItem is related to any Sources in DownloadManager.
        /// They will be returned.
        /// </summary>
        /// <param name="d">DownloadItem to find related soures for</param>
        /// <param name="s">Sources found for DownloadItem</param>
        /// <returns>Returns true if Sources can be found for DownloadItem</returns>
        public virtual bool TryGetSources(DownloadItem d, out Source[] s)
        {
            lock (this)
            {
                if (dwnItems.ContainsKey(d))
                {
                    FlowSortedList<Source> tmp = dwnItems[d];
                    if (tmp != null && tmp.Count > 0)
                    {
                        s = tmp.ToArray();
                        return true;
                    }
                }
            }
            s = null;
            return false;
        }

        /// <summary>
        /// If source is related to a downloaditem in downloadmanager.
        /// First match will be returned.
        /// </summary>
        /// <param name="s">Source to find related downloaditems for</param>
        /// <param name="d">DownloadItem found for Source</param>
        /// <returns>Returns true if downloaditem was found for source</returns>
        public virtual bool TryGetDownload(Source s, out DownloadItem d)
        {
            FlowSortedList<DownloadItem> items = null;
            lock (this)
            {
                if (srcItems.TryGetValue(s, out items) && items.Count > 0)
                {
                    d = items[0];
                    return true;
                }
            }
            d = null;
            return false;
        }

        /// <summary>
        /// Calls Load(dir) with System.AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public void Load()
        {
#if !COMPACT_FRAMEWORK
            Load(System.AppDomain.CurrentDomain.BaseDirectory);
#else
            Load(System.IO.Directory.GetCurrentDirectory());
#endif
        }

        public virtual void Load(string dir)
        {
            directory = dir;
            lock (this)
	        {
                DownloadManager tmpMgr = FileOperations<DownloadManager>.LoadObject(dir + FileName + ".xml");
                if (tmpMgr != null)
                {
                    FileName = tmpMgr.FileName;
                    this.Items = tmpMgr.Items;
                }
	        }
        }

        public void Save()
        {
            Save(directory);
        }

        public virtual void Save(string dir)
        {
            lock (this)
        	{
                FileOperations<DownloadManager>.SaveObject(dir + FileName + ".xml", this);
            }
        }
    }
}
