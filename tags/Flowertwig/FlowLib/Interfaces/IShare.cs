
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
using System;
using FlowLib.Events;

namespace FlowLib.Interfaces
{
    public interface IShare : IEnumerable<KeyValuePair<string, ContentInfo>>
    {
        /// <summary>
        /// Is triggered when a virtual directory has been added
        /// </summary>
        event EventHandler<EventArgs> VirtualDirAdded;
        /// <summary>
        /// Is triggered when a virtual directory has been removed
        /// </summary>
        event EventHandler<EventArgs> VirtualDirRemoved;
        /// <summary>
        /// Is triggered when hashing process starts
        /// </summary>
        event EventHandler<EventArgs> HashingStarted;
        /// <summary>
        /// Is triggered when hashing process ends
        /// </summary>
        event EventHandler<EventArgs> HashingCompleted;
        /// <summary>
        /// Is triggered when a ContentInfo should be hashed.
        /// This is so you can extend hashing and not use Tiger Tree Hash.
        /// </summary>
        event FmdcEventHandler HashContentInfo;
        /// <summary>
        /// Is triggered when a error is triggered
        /// </summary>
        event FmdcEventHandler ErrorOccured;
        /// <summary>
        /// Value indicating when share was updated has been changed
        /// </summary>
        event FmdcEventHandler LastModifiedChanged;

        /// <summary>
        /// Adds ContentInfo to share
        /// </summary>
        /// <param name="contentInfo">ContentInfo to add</param>
        /// <returns>Returns true if file was successfully added.</returns>
        bool AddFile(ContentInfo contentInfo);
        /// <summary>
        /// Removes ContentInfo from share
        /// </summary>
        /// <param name="contentInfo">ContentInfo to remove</param>
        /// <returns>Returns true if file was successfully removed.</returns>
        bool RemoveFile(ContentInfo contentInfo);
        /// <summary>
        /// Adds virtual directory to share.
        /// </summary>
        /// <param name="key">Key to identify virtual directory</param>
        /// <param name="value">Value to tell share what to index. For FlowLib.Containers.Share it is system directory</param>
        /// <returns>returns true if directory was added successfully</returns>
        bool AddVirtualDir(string key, string value);
        /// <summary>
        /// Removes virtual directory from share
        /// </summary>
        /// <param name="systempath">Value to tell share what to index</param>
        /// <returns>returns true if virtual directory was successfully removed</returns>
        bool RemoveVirtualDir(string systempath);
        /// <summary>
        /// Is ContentInfo in this share?
        /// </summary>
        /// <param name="info">ContentInfo to check for</param>
        /// <returns>returns true if ContentInfo exist in share</returns>
        bool ContainsContent(ref ContentInfo info);
        /// <summary>
        /// Get byte array from start and length.
        /// </summary>
        /// <param name="info">ContentInfo that we will get byte array from</param>
        /// <param name="start">Start position</param>
        /// <param name="length">Lenght of the byte array</param>
        /// <returns>returns byte array for ContentInfo if info exist in share</returns>
        byte[] GetContent(ContentInfo info, long start, long length);
        /// <summary>
        /// Start hash ContentInfos in share
        /// </summary>
        void HashContent();
        /// <summary>
        /// Cancel hashing of ContentInfos in share
        /// </summary>
        void CancelHashing();
        /// <summary>
        /// Loads settings for share setting file from AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        void Load();
        /// <summary>
        /// Loads settings for share setting file from directory.
        /// </summary>
        /// <param name="dir">Directory that you will look for setting file in</param>
        void Load(string dir);
        /// <summary>
        /// Reloads Share.
        /// 1) Checks if added contentinfo is still valid, if not it will be removed.
        /// 2) Checks if more contentinfo can be added.
        /// 3) If changes to share has been done, hash content that has not already been hashed.
        /// </summary>
        void Reload();
        /// <summary>
        /// Saves setting file in directory used for loading
        /// </summary>
        void Save();
        /// <summary>
        /// Saves setting file in directory specified
        /// </summary>
        /// <param name="dir">directory to save in</param>
        void Save(string dir);
        /// <summary>
        /// Removes setting file from directory that was used to load content
        /// </summary>
        /// <returns>returns true only if settings was deleted</returns>
        bool Remove();
        /// <summary>
        /// Removes setting file from directory specified
        /// </summary>
        /// <param name="dir">directory where we want to delete settings</param>
        /// <returns>returns true only if settings was deleted</returns>
        bool Remove(string dir);


        // Properties
        /// <summary>
        /// Specify if content with same has should be shared or not.
        /// </summary>
        bool HashAllowDuplicate { get; set; }
        /// <summary>
        /// Gets ContentInfo count that has been hashed
        /// </summary>
        long HashedCount { get; }
        /// <summary>
        /// Gets total ContentInfo size that has been hashed
        /// </summary>
        long HashedSize { get; }
        /// <summary>
        /// Gets Ticks when share was last updated
        /// </summary>
        long LastModified { get; }
        /// <summary>
        /// Gets/sets name of Share
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Gets port (that could be used IF you want diffrent ports for diffrent shares)
        /// </summary>
        int Port { get; set; }
        /// <summary>
        /// Gets total ContentInfo count in share
        /// </summary>
        long TotalCount { get; }
        /// <summary>
        /// Gets total Content size in share
        /// </summary>
        long TotalSize { get; }
    }
}
