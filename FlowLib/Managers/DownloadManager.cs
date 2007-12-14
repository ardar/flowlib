
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

using FlowLib.Containers;
using FlowLib.Events;
using System.Collections.Generic;

namespace FlowLib.Managers
{
    /// <summary>
    /// Class handling downloadItems
    /// </summary>
    public class DownloadManager
    {
        public string FileName = "Downloads";
        
        /// <summary>
        /// Downloading have been completed of segment
        /// </summary>
        public event FmdcEventHandler SegmentCompleted;
        /// <summary>
        /// Downloading of segment have been started
        /// </summary>
        public event FmdcEventHandler SegmentStarted;
        /// <summary>
        /// Segment downloading have been canceled
        /// </summary>
        public event FmdcEventHandler SegmentCanceled;
        /// <summary>
        /// DownloadItem have been completed (Download)
        /// </summary>
        public event FmdcEventHandler DownloadCompleted;
        /// <summary>
        /// DownloadItem have been added to DownloadManager
        /// </summary>
        public event FmdcEventHandler DownloadAdded;
        /// <summary>
        /// DownloadItem have been removed from DownloadManager
        /// </summary>
        public event FmdcEventHandler DownloadRemoved;
        /// <summary>
        /// Source have been added to DownloadManager
        /// </summary>
        public event FmdcEventHandler SourceAdded;
        /// <summary>
        /// Source have been removed from DownloadManager
        /// </summary>
        public event FmdcEventHandler SourceRemoved;

        protected SortedList<DownloadItem, FlowSortedList<Source>> downloadItems = new SortedList<DownloadItem, FlowSortedList<Source>>();
        protected SortedList<Source, FlowSortedList<DownloadItem>> sourceItems = new SortedList<Source, FlowSortedList<DownloadItem>>();

        /// <summary>
        /// Init listeners for this static class
        /// </summary>
        public DownloadManager()
        {
            SegmentCanceled = new FmdcEventHandler(DownloadManager_SegmentCanceled);
            SegmentCompleted = new FmdcEventHandler(DownloadManager_SegmentCompleted);
            SegmentStarted = new FmdcEventHandler(DownloadManager_SegmentStarted);
            DownloadCompleted = new FmdcEventHandler(DownloadManager_DownloadCompleted);

            DownloadAdded = new FmdcEventHandler(DownloadManager_DownloadAdded);
            DownloadRemoved = new FmdcEventHandler(DownloadManager_DownloadRemoved);
            SourceAdded = new FmdcEventHandler(DownloadManager_SourceAdded);
            SourceRemoved = new FmdcEventHandler(DownloadManager_SourceRemoved);
        }

        void DownloadManager_SourceRemoved(object sender, FmdcEventArgs e) { }
        void DownloadManager_SourceAdded(object sender, FmdcEventArgs e) { }
        void DownloadManager_DownloadRemoved(object sender, FmdcEventArgs e) { }
        void DownloadManager_DownloadAdded(object sender, FmdcEventArgs e) { }
        void DownloadManager_DownloadCompleted(object sender, FmdcEventArgs e) { }
        void DownloadManager_SegmentStarted(object sender, FmdcEventArgs e) { }
        void DownloadManager_SegmentCompleted(object sender, FmdcEventArgs e) { }
        void DownloadManager_SegmentCanceled(object sender, FmdcEventArgs e) { }

        /// <summary>
        /// Remove source from all downloads
        /// </summary>
        /// <param name="s">Source to remove</param>
        public void RemoveSource(Source s)
        {
            if (sourceItems.ContainsKey(s))
            {
                FlowSortedList<DownloadItem> tmpSrc = sourceItems[s];
                if (tmpSrc != null)
                {
                    foreach (DownloadItem var in tmpSrc)
                    {
                        if (downloadItems.ContainsKey(var))
                        {
                            FlowSortedList<Source> tmpDwn = downloadItems[var];
                            if (tmpDwn != null)
                            {
                                tmpDwn.Remove(s);
                            }
                        }
                    }
                }
                sourceItems.Remove(s);
                SourceRemoved(null, new FmdcEventArgs(0, s));
            }
        }
        /// <summary>
        /// Remove DownloadItem
        /// </summary>
        /// <param name="d">DownloadItem to remove</param>
        public void RemoveDownload(DownloadItem d)
        {
            if (downloadItems.ContainsKey(d))
            {
                FlowSortedList<Source> tmpDwn = downloadItems[d];
                if (tmpDwn != null)
                {
                    foreach (Source var in tmpDwn)
                    {
                        if (sourceItems.ContainsKey(var))
                        {
                            FlowSortedList<DownloadItem> tmpSrc = sourceItems[var];
                            if (tmpSrc != null)
                            {
                                tmpSrc.Remove(d);
                            }
                        }
                    }
                }
                downloadItems.Remove(d);
                DownloadRemoved(null, new FmdcEventArgs(0, d));
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
            // Downloads
            FlowSortedList<Source> tmpDwn = null;
            if (!downloadItems.ContainsKey(d))
            {
                tmpDwn = new FlowSortedList<Source>();
                downloadItems.Add(d, tmpDwn);
                // TODO : Adding events for downloadItem
                d.DownloadCompleted += new FmdcEventHandler(d_DownloadCompleted);
                DownloadAdded(null, new FmdcEventArgs(0, d));
            }
            else
            {
                tmpDwn = downloadItems[d];
            }
            if (s != null)
            {
                tmpDwn.Add(s);

                // Sources
                FlowSortedList<DownloadItem> tmpSrc = null;
                if (!sourceItems.ContainsKey(s))
                {
                    tmpSrc = new FlowSortedList<DownloadItem>();
                    sourceItems.Add(s, tmpSrc);
                    SourceAdded(null, new FmdcEventArgs(0, s));
                }
                else
                {
                    tmpSrc = sourceItems[s];
                }
                tmpSrc.Add(d);
            }
        }

        private void d_DownloadCompleted(object sender, FmdcEventArgs e)
        {
            RemoveDownload((DownloadItem)sender);
            DownloadCompleted(sender, e);
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
            return sourceItems.ContainsKey(s);
        }

        /// <summary>
        /// Do downloadmanager contains downloaditem
        /// </summary>
        /// <param name="d">Do this DownloadItem exist</param>
        /// <returns>Return true if downloaditem exist</returns>
        public bool ContainsDownload(DownloadItem d)
        {
            return downloadItems.ContainsKey(d);
        }
        /// <summary>
        /// If source is related to a downloaditem in downloadmanager.
        /// First match will be returned.
        /// </summary>
        /// <param name="s">Source to find related downloaditems for</param>
        /// <param name="d">DownloadItem found for Source</param>
        /// <returns>Returns true if downloaditem was found for source</returns>
        public bool TryGetDownload(Source s, out DownloadItem d)
        {
            FlowSortedList<DownloadItem> items = null;
            if (sourceItems.TryGetValue(s, out items) && items.Count > 0)
            {
                d = items[0];
                return true;
            }
            d = null;
            return false;
        }

        /// <summary>
        /// Calls Load(dir) with System.AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public void Load()
        {
            Load(System.AppDomain.CurrentDomain.BaseDirectory);
        }

        // TODO : Do loading
        public void Load(string dir)
        {
        }

        // TODO : Do saving
        public void Save()
        {

        }
    }
}