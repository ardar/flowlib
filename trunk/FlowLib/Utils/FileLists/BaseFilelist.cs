
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

using System.Collections.Generic;
using System;
using FlowLib.Containers;

namespace FlowLib.Utils.FileLists
{
    public abstract class BaseFilelist
    {
        public const string UNKNOWN = "UNKNOWN";
        public const string XMLBZ = "XMLBZ";
        public const string XML = "XML";
        public const string BZ = "BZ";
        public const string HUFFMAN = "HUFFMAN";

        protected char seperator = '\\';
        protected Share share = null;
        protected bool hasWritenFilelist = false;
        protected BaseFilelist(Share share) { this.share = share; }
        protected abstract void StartFilelist();
        protected abstract void EndFilelist();
        protected abstract void StartDirectory(string name);
        protected abstract void EndDirectory();
        protected abstract void AddFile(ContentInfo content);
        public abstract void CreateShare();

        public char Seperator
        {
            get { return seperator; }
            set { seperator = value; }
        }

        public bool HasWritenFilelist
        {
            get { return hasWritenFilelist; }
        }

        public Share Share
        {
            get { return share; }
        }

        public abstract ContentInfo ContentInfo
        {
            get;
        }

        public void CreateFilelist()
        {
            StartFilelist();
            #region Get shared files and sort them
            // This is ugly as hell. But _I_ dont know a better way todo it right now.
            SortedList<string, ContentInfo> vlist = new SortedList<string, ContentInfo>((int)share.TotalCount);
            try
            {
                foreach (KeyValuePair<string, ContentInfo> item in share)
                {
                    if (item.Value != null)
                        vlist.Add(item.Value.Get(ContentInfo.VIRTUAL), item.Value);
                }
            }
            catch (Exception e)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (KeyValuePair<string, ContentInfo> item in share)
                {
                    sb.AppendLine(string.Format("Key: {0}, Virtual: {1}", item.Key, item.Value.Get(ContentInfo.VIRTUAL)));
                }
                Exception en = new Exception(sb.ToString(), e);
                throw en;
            }
            #endregion
            #region Add Content
            string[] lastDirs = null;
            foreach (KeyValuePair<string, ContentInfo> item in vlist)
            {
                string[] currentDirs = null;
                int pos;
                if (item.Value != null && (pos = item.Value.Get(ContentInfo.VIRTUAL).LastIndexOf(seperator)) != -1)
                {
                    string tmpPath = item.Value.Get(ContentInfo.VIRTUAL).Substring(0, pos);
                    currentDirs = tmpPath.Split(seperator);
                    #region remove directories
                    int differFrom = 0;
                    if (lastDirs != null)
                    {
                        for (int i = 0; i < lastDirs.Length; i++)
                        {
                            if (currentDirs.Length > i && lastDirs[i].Equals(currentDirs[i]))
                            {
                                differFrom++;
                                continue;
                            }
                            // Decrease directory
                            EndDirectory();
                        }
                    }
                    #endregion
                    #region add directories
                    for (int i = differFrom; i < currentDirs.Length; i++)
                    {
                        if (currentDirs[i].Length > 0)
                            StartDirectory(currentDirs[i]);
                    }
                    #endregion
                    #region add files
                    AddFile(item.Value);
                    #endregion
                    lastDirs = currentDirs;
                }
            }
            #endregion
            EndFilelist();
        }
    }
}
