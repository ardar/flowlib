
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

using FlowLib.Containers;
using System.Collections.Generic;
using FlowLib.Events;

namespace FlowLib.Managers
{
    /// <summary>
    /// Static class containing shares
    /// </summary>
    public class ShareManager
    {
        protected const int IndexShareNames = 0;
        protected string directory;
        public string FileName = "Shares";
        protected SortedList<string, Share> shares = new SortedList<string, Share>();

        public ShareManager()
        {
#if !COMPACT_FRAMEWORK
            directory = System.AppDomain.CurrentDomain.BaseDirectory;
#else
            directory = System.IO.Directory.GetCurrentDirectory();
#endif
        }
        /// <summary>
        /// Adding share to manager
        /// </summary>
        /// <param name="s">Share we want to add</param>
        /// <returns>Returns true if share name is not already existing (and is added)</returns>
        public bool AddShare(Share s)
        {
            if (!s.Name.Contains("|") && !shares.ContainsKey(s.Name))
            {
                shares.Add(s.Name, s);
                Save();
                return true;
            }
            return false;
        }
        /// <summary>
        /// Get share names in a list
        /// </summary>
        /// <returns>list of names of shares</returns>
        public IList<string> GetShareNames()
        {
            return shares.Keys;
        }
        /// <summary>
        /// Returns share with the specified id
        /// </summary>
        /// <param name="id">id we want to get share of</param>
        /// <param name="s">Returning share if matching id</param>
        /// <returns>Returns true if id match a share</returns>
        public bool GetShare(string id, out Share s)
        {
            return shares.TryGetValue(id, out s);
        }
        /// <summary>
        /// Removes share from this
        /// </summary>
        /// <param name="id">id for share you want to remove</param>
        /// <returns>returns true if manager contains a share with id as name</returns>
        public bool RemoveShare(string id)
        {
            Share s;
            if (shares.TryGetValue(id, out s))
            {
                if (shares.Remove(id))
                {
                    s.Remove();
                    Save();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Checks if share contains a contentinfo
        /// </summary>
        /// <param name="shareId">Name of a share we should look in</param>
        /// <param name="info">ContentInfo we should look for</param>
        /// <returns>Returns true if share name exist and if contentinfo exist in that share</returns>
        public bool ContainsContent(string shareId, ContentInfo info)
        {
            Share s;
            if (GetShare(shareId, out s))
                return s.ContainsContent(ref info);
            return false;
        }
        /// <summary>
        /// Get data for the ContentInfo specified
        /// </summary>
        /// <param name="shareId">Share name for share to look in</param>
        /// <param name="info">ContentInfo to get byte[] from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Length of byte[]</param>
        /// <returns>if share and contentinfo exist. data for this contentinfo will be returned</returns>
        public byte[] GetContent(string shareId, ContentInfo info, long start, long length)
        {
            Share s;
            if (GetShare(shareId, out s))
                return s.GetContent(info, start, length);
            return null;
        }
        /// <summary>
        /// Returns when the share with the specified id was last modified.
        /// </summary>
        /// <param name="shareId">name of share you want to know when it was changed</param>
        /// <returns>Ticks showing when share was last changed, -1 means share was not found or not changed</returns>
        public long GetLastModified(string shareId)
        {
            Share s;
            if (GetShare(shareId, out s))
                return s.LastModified;
            return -1;
        }
        /// <summary>
        /// Load shares from setting files in folder System.AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public void Load()
        {
            Load(directory);
        }
        /// <summary>
        /// Load shares from setting files from dir
        /// </summary>
        /// <param name="dir">directory where you want to load your shares from</param>
        public void Load(string dir)
        {
            directory = dir;
            SettingsGroup setting = new SettingsGroup();
            SettingsGroup.Load(dir + FileName + ".xml", out setting, FileName);

            // Share names
            string tmpshareNames = setting.GetString(IndexShareNames);
            if (tmpshareNames != null)
            {
                // Load shares
                string[] tmpNames = tmpshareNames.Split('|');
                foreach (string var in tmpNames)
                {
                    if (var == string.Empty)
                        continue;
                    Share s = new Share(var);
                    s.Load(dir);
                    shares.Add(s.Name, s);
                }
                tmpNames = null;
            }
            // If default share doesnt exist, create it.
            if (!shares.ContainsKey("Default"))
            {
                Share s = new Share("Default");
                AddShare(s);
            }
            tmpshareNames = null;
            Reload();
        }

        private void OnReload()
        {
            /***
             * This while-loop is just for walking around a exception.
             * If we remove/add a share foreach loop will throw exception as content in list has been changed.
             **/
            bool needsRestart = true;
            while (needsRestart)
            {
                needsRestart = false;
                try
                {
                    foreach (KeyValuePair<string, Share> item in shares)
                    {
                        item.Value.Reload();
                    }
                }
                catch (System.InvalidOperationException)
                {
                    needsRestart = true;
                }
            }
        }
        /// <summary>
        /// Calls reload of every share in manager
        /// </summary>
        public void Reload()
        {
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(OnReload));
            t.IsBackground = true;
            t.Priority = System.Threading.ThreadPriority.Lowest;
            t.Start();
        }
        /// <summary>
        /// Calls save with System.AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public void Save()
        {
            Save(directory);
        }
        /// <summary>
        /// Saves shares in directory specified
        /// </summary>
        /// <param name="dir">Directory where we should save settings to</param>
        public void Save(string dir)
        {
            directory = dir;
            SettingsGroup setting = new SettingsGroup();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (KeyValuePair<string, Share> item in shares)
            {
                sb.Append(item.Key);
                sb.Append("|");
            }
            if (sb.Length > 0)
                setting.Add(IndexShareNames, new SettingItem(sb.ToString(), null));
            FlowLib.Utils.FileOperations.PathExists(dir);
            SettingsGroup.Save(dir + FileName + ".xml", setting, FileName);
            setting = null;
        }
    }

}
