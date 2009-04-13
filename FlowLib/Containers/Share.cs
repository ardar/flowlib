
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
using System.Threading;
using System.IO;

using FlowLib.Events;
using FlowLib.Utils.Hash;
using FlowLib.Utils.Convert;
using FlowLib.Enums;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing shared content in p2p networks like Direct Connect
    /// </summary>
    public class Share : IEnumerable<KeyValuePair<string, ContentInfo>>
    {
        #region Events
        /// <summary>
        /// Specifies that a error has occured.
        /// Events having this as id will send a Exception as data.
        /// </summary>
        public const int ErrorInvalidChars = 0;
        /// <summary>
        /// Specifies that a error has occured.
        /// Events having this as id will send a Exception as data.
        /// </summary>
        public const int ErrorSecurity = 1;
        /// <summary>
        /// Specifies that a error has occured.
        /// Events having this as id will send a Exception as data.
        /// </summary>
        public const int ErrorDirectoryNotFound = 2;
        /// <summary>
        /// Specifies that a error has occured.
        /// Events having this as id will send a Exception as data.
        /// </summary>
        public const int ErrorAccessDeniedBecauseOfIOError = 3;
        /// <summary>
        /// Specifies that a error has occured.
        /// Events having this as id will send a ContentInfo as data.
        /// </summary>
        public const int ErrorContentVirtualNameConflict = 4;
        /// <summary>
        /// Specifies that you are trying to add a directory that you are already sharing.
        /// Can happen when you are trying to add a super directory but already is sharing a subdirectory.
        /// In this case all sub directories that is not already shared will still be added and the ones that are shared will NOT be shared again.
        /// </summary>
        public const int ErrorSourceDirectoryDuplicate = 5;
        /// <summary>
        /// Specifies that X files are missing since last time we saved share.
        /// This can be because we havnt connected to a network drive or have moved directories that we where sharing.
        /// </summary>
        public const int ErrorManyFilesAreMissingSinceLastSave = 6;
        /// <summary>
        /// A file already hashed and a file hashed now has the same hash value (Same content).
        /// This is just a notification. If user have set so it is not allowed content will be removed.
        /// Events having this as id will have a ContentInfo as data.
        /// </summary>
        public const int ErrorHashDuplicate = 7;
        /// <summary>
        /// Unknown exception occurred.
        /// Events having this as id will have the occurred exception as data.
        /// </summary>
        public const int ErrorUnknownError = 8;

        /// <summary>
        /// Is triggered when a virtual directory has been added
        /// </summary>
        public event EventHandler<EventArgs> VirtualDirAdded;
        /// <summary>
        /// Is triggered when a virtual directory has been removed
        /// </summary>
        public event EventHandler<EventArgs> VirtualDirRemoved;
        /// <summary>
        /// Is triggered when hashing process starts
        /// </summary>
        public event EventHandler<EventArgs> HashingStarted;
        /// <summary>
        /// Is triggered when hashing process ends
        /// </summary>
        public event EventHandler<EventArgs> HashingCompleted;
        /// <summary>
        /// Is triggered when a ContentInfo should be hashed.
        /// This is so you can extend hashing and not use Tiger Tree Hash.
        /// </summary>
        public event FmdcEventHandler HashContentInfo;
        /// <summary>
        /// Is triggered when a error is triggered
        /// </summary>
        public event FmdcEventHandler ErrorOccured;
        /// <summary>
        /// Value indicating when share was updated has been changed
        /// </summary>
        public event FmdcEventHandler LastModifiedChanged;
        #endregion

        #region Variables
        protected const int IndexVirtualDirBegin = 0;
        protected const int IndexVirtualDirCount = 1;
        protected const int IndexShareBegin = 2;
        protected const int IndexShareCount = 3;
        protected const int IndexLastModifiedTimeStamp = 4;
        protected const int IndexPort = 5;
        protected const int IndexHashAllowDuplicate = 6;
        protected const int IndexHashAutoSaveCount = 7;
        protected const int IndexHashThreadCount = 8;
        protected const int IndexHashThreadPriority = 9;
        protected const int IndexHashThreadSleep = 10;

        protected int hashDelay = 0;
        protected System.Threading.ThreadPriority hashPriority = ThreadPriority.Lowest;
        protected int hashThreadCount = 4;
        protected int hashAutoSaveCount = 10;
        protected bool hashAllowDuplicate = false;
        protected string directory = null;

        protected int virtualDirBegin = 100;
        protected int shareBegin = 150;
        protected long lastModifiedTimeStamp = -1;

        // Values only stored in memory.
        protected int port = -1;
        protected string name = string.Empty;
        protected bool isHashing = false;
        protected bool cnlHashing = false;
        protected bool isSaving = false;
        #region Lists
        /// <summary>
        /// Holds the Virtual dirs and what system dir it points to.
        /// Key: System path, Value: VirtualDir object
        /// </summary>
        protected SortedList<string, VirtualDir> virtualDirs = new SortedList<string, VirtualDir>();
        /// <summary>
        /// Holds the ContentInfo objects
        /// Key: System path, Value: ContentInfo
        /// </summary>
        protected SortedList<string, ContentInfo> share = new SortedList<string, ContentInfo>();
        /// <summary>
        /// Holds the virtual dir path to all ContentInfo in share.
        /// Key:  virtual dir + contentinfo name, Value: system path
        /// </summary>
        protected SortedList<string, ContentInfo> virtualNames = new SortedList<string, ContentInfo>();
        /// <summary>
        /// Holds Tth of all ContentInfo in share.
        /// We have placed it here because the main 2 protocols used (Nmdc/Adc) both uses tth.
        /// Key:  Tth of contentinfo, Value: systempath
        /// </summary>
        protected SortedList<string, ContentInfo> tthNames = new SortedList<string, ContentInfo>();
        #endregion
        #endregion
        #region Properties
        /// <summary>
        /// Specify if content with same has should be shared or not.
        /// </summary>
        public bool HashAllowDuplicate
        {
            get { return hashAllowDuplicate; }
            set { hashAllowDuplicate = value; }
        }
        /// <summary>
        /// Specify how many ContentInfo that should be done before autosaving
        /// </summary>
        public int HashAutoSaveCount
        {
            get { return hashAutoSaveCount; }
            set { hashAutoSaveCount = value; }
        }
        /// <summary>
        /// Specify how many threads that should be used for hashing.
        /// </summary>
        public int HashThreadCount
        {
            get { return hashThreadCount; }
            set { hashThreadCount = value; }
        }
        /// <summary>
        /// Specify what ThreadPriority level to use for hashing
        /// </summary>
        public System.Threading.ThreadPriority HashThreadPriority
        {
            get { return hashPriority; }
            set { hashPriority = value; }
        }
        /// <summary>
        /// Specify how many milliseconds thread should sleep between hashing of every ContentInfo
        /// </summary>
        public int HashThreadSleep
        {
            get { return hashDelay; }
            set { hashDelay = value; }
        }
        /// <summary>
        /// Gets a list of all virtual directories in Share
        /// </summary>
        public IList<VirtualDir> VirtualDirs
        {
            get { return virtualDirs.Values; }
        }
        /// <summary>
        /// Indicates if share is currently hashing
        /// </summary>
        public bool IsHashing
        {
            get { return isHashing; }
        }
        /// <summary>
        /// Gets ContentInfo count that has been hashed
        /// </summary>
        public long HashedCount
        {
            get
            {
                long total = 0;
                foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
                {
                    total += item.Value.HashedCount;
                }
                return total;
            }
        }
        /// <summary>
        /// Gets total ContentInfo size that has been hashed
        /// </summary>
        public long HashedSize
        {
            get
            {
                long total = 0;
                foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
                {
                    total += item.Value.HashedSize;
                }
                return total;
            }
        }
        /// <summary>
        /// Gets total ContentInfo count in share
        /// </summary>
        public long TotalCount
        {
            get
            {
                long total = 0;
                foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
                {
                    total += item.Value.TotalCount;
                }
                return total;
            }
        }
        /// <summary>
        /// Gets total Content size in share
        /// </summary>
        public long TotalSize
        {
            get
            {
                long total = 0;
                foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
                {
                    total += item.Value.TotalSize;
                }
                return total;
            }
        }
        /// <summary>
        /// Gets Ticks when share was last updated
        /// </summary>
        public long LastModified
        {
            get { return lastModifiedTimeStamp; }
        }
        /// <summary>
        /// Gets port (that could be used IF you want diffrent ports for diffrent shares)
        /// </summary>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        /// <summary>
        /// Gets/sets name of Share
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        #endregion
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name">Name/Id of share</param>
        public Share(string name)
        {
            ErrorOccured = new FmdcEventHandler(OnErrorOccured);
            VirtualDirAdded = new EventHandler<EventArgs>(OnVirtualDirAdded);
            VirtualDirRemoved = new EventHandler<EventArgs>(OnVirtualDirRemoved);
            HashingCompleted = new EventHandler<EventArgs>(OnHashingCompleted);
            HashingStarted = new EventHandler<EventArgs>(OnHashingStarted);
            LastModifiedChanged = new FmdcEventHandler(OnLastModifiedChanged);
            HashContentInfo = new FmdcEventHandler(OnHashContentInfo);
            this.name = name;
        }

        void OnHashContentInfo(object sender, FmdcEventArgs e) {}
        void OnLastModifiedChanged(object sender, FmdcEventArgs e)
        {
            lastModifiedTimeStamp = DateTime.Now.Ticks;
        }
        void OnHashingStarted(object sender, EventArgs e) { }
        void OnHashingCompleted(object sender, EventArgs e) { }
        void OnVirtualDirRemoved(object sender, EventArgs e) { }
        void OnVirtualDirAdded(object sender, EventArgs e) { }

        /// <summary>
        /// Add a virtual directory related to systempath
        /// </summary>
        /// <param name="virtualdir">Virtual directory (Visual name for system path)</param>
        /// <param name="systempath">Systempath where we will search for files to add ContentInfo in</param>
        /// <returns>Returns true if systempath doesnt already exist</returns>
        public virtual bool AddVirtualDir(string virtualdir, string systempath)
        {
            bool value = true;

            if (!virtualdir.EndsWith(@"\"))
                virtualdir = virtualdir + @"\";
            VirtualDir vd = new VirtualDir(systempath, virtualdir);
            try
            {
                virtualDirs.Add(vd.SystemPath, vd);
#if !COMPACT_FRAMEWORK
                Thread t = new Thread(new ParameterizedThreadStart(OnAddVirtualDir));
                t.IsBackground = true;
                t.Start(vd);
#else
                Thread t = new Thread(new ThreadStart(OnAddVirtualDir));
                t.IsBackground = true;
                t.Start(vd);
#endif
            }
            catch (ArgumentException) {
                /* This is if dir has already been added
                 * It shouldnt happen if thread is throwing an exeption.
                 */
                value = false;
            }
            return value;
        }

#if COMPACT_FRAMEWORK
        protected virtual void OnAddVirtualDir()
        {
            OnAddVirtualDir(Thread.CurrentThread.GetData());
        }
#endif

        protected virtual void OnAddVirtualDir(object ovd)
        {
            VirtualDir vd = (VirtualDir)ovd;
            // As indexing files can take looong time. We will save virtual dir here.
            Save();
            lock (share)
            {
                share.Add(vd.SystemPath, null);
            }
            AddDir(vd.SystemPath, vd.VirtualPath, vd);
            // As Hashing can take looong time. We will save share here.
            Save();
            VirtualDirAdded(this, new EventArgs());
            HashContent();
        }

        protected virtual void AddDir(string systempath, string virtualdir, VirtualDir vd)
        {
            if (System.IO.Directory.Exists(systempath))
            {
                foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
                {
                    if (item.Key.Equals(systempath) && !item.Value.VirtualPath.Equals(virtualdir))
                    {
                        ErrorOccured(this, new FmdcEventArgs(Share.ErrorSourceDirectoryDuplicate, systempath));
                        return;
                    }
                }

                // Adding directory to share.
                System.IO.DirectoryInfo dirInfo = null;
                #region Get DirectoryInfo for current directory
                try
                {
                    dirInfo = new System.IO.DirectoryInfo(systempath);
                }
                catch (ArgumentException ae)
                {
                    // path contains invalid characters such as ", <, >, or |. 
                    ErrorOccured(this, new FmdcEventArgs(ErrorInvalidChars, ae));
                    return;
                }
                catch (System.Security.SecurityException se)
                {
                    // User does not have the required permission. 
                    ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, se));
                    return;
                }
                #endregion
                #region Get subdirectories and recursive this function on that.
                try
                {
                    System.IO.DirectoryInfo[] dirs = dirInfo.GetDirectories();
                    for (int i = 0; i < dirs.Length; i++)
                    {
                        AddDir(dirs[i].FullName, virtualdir + dirs[i].Name + @"\", vd);
                    }
                }
                catch (System.IO.DirectoryNotFoundException dnfe)
                {
                    // Sub directory doesnt seem to exist. 
                    // NOTE :  Can this ever happen?
                    ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, dnfe));
                }
                catch (System.UnauthorizedAccessException uae) {
                    // User does not have the required permission. 
                    ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, uae));
                }
                catch (System.Security.SecurityException se)
                {
                    // User does not have the required permission. 
                    ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, se));
                }
                #endregion
                #region Get files in directory
                try
                {
                    System.IO.FileInfo[] filesInfo = dirInfo.GetFiles();
                    for (int i = 0; i < filesInfo.Length; i++)
                    {
                        System.IO.FileInfo fi = filesInfo[i];
                        AddContent(fi, virtualdir, vd);
                    }
                }
                catch (System.Security.SecurityException se)
                {
                    // User does not have the required permission. 
                    ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, se));
                }
                catch (UnauthorizedAccessException uae)
                {
                    // Access denied because of IO error.
                    ErrorOccured(this, new FmdcEventArgs(ErrorAccessDeniedBecauseOfIOError, uae));
                }
                #endregion
            }else{
                // If folder doesnt exist for some reason.
                ErrorOccured(this, new FmdcEventArgs(ErrorDirectoryNotFound, new System.IO.DirectoryNotFoundException("Adding virtual dir failed because directory was not found.")));
            }
        }
        #region For Transfer Protocol that makes Filelists.
        public virtual bool AddFile(ContentInfo contentInfo)
        {
			VirtualDir vd = null;
			lock (virtualDirs)
			{
				if (virtualDirs.ContainsKey("dummy"))
					vd = virtualDirs["dummy"];
				else
				{
					vd = new VirtualDir("dummy", "dummy");
					virtualDirs.Add("dummy", vd);
				}
			}

			if (!contentInfo.ContainsKey(ContentInfo.FILELIST))
			{
				vd.TotalSize += contentInfo.Size;
				vd.TotalCount++;
			}

            bool value = false;
            if (contentInfo.ContainsKey(ContentInfo.STORAGEPATH) && !share.ContainsKey(contentInfo.Get(ContentInfo.STORAGEPATH)))
            {
                lock (share)
                {
                    share.Add(contentInfo.Get(ContentInfo.STORAGEPATH), contentInfo);
                }
                value = true;
            }
            if (contentInfo.ContainsKey(ContentInfo.VIRTUAL) && !virtualNames.ContainsKey(contentInfo.Get(ContentInfo.VIRTUAL)))
            {
                virtualNames.Add(contentInfo.Get(ContentInfo.VIRTUAL), contentInfo);
                value = true;
            }
            if (contentInfo.ContainsKey(ContentInfo.TTH) && !tthNames.ContainsKey(contentInfo.Get(ContentInfo.TTH)))
            {
                tthNames.Add(contentInfo.Get(ContentInfo.TTH), contentInfo);

				if (!contentInfo.ContainsKey(ContentInfo.FILELIST))
				{
					vd.HashedSize += contentInfo.Size;
					vd.HashedCount++;
				}

                value = true;
            }
            return value;
        }

        public virtual bool RemoveFile(ContentInfo contentInfo)
        {
            if (contentInfo.IsTth)
                tthNames.Remove(contentInfo.Get(ContentInfo.TTH));
            virtualNames.Remove(contentInfo.Get(ContentInfo.VIRTUAL));
            lock (share)
            {
                return share.Remove(contentInfo.Get(ContentInfo.STORAGEPATH));
            }
        }
        #endregion
        protected virtual void AddContent(ContentInfo contentInfo)
        {
            foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
            {
                if (contentInfo.Get(ContentInfo.STORAGEPATH).StartsWith(item.Key))
                {
                    AddContent(contentInfo, item.Value);
                    return;
                }
            }
        }


        protected virtual void AddContent(ContentInfo contentInfo, VirtualDir vd)
        {
            // Adding to lists
            try
            {
                ContentInfo virtualValue;
                if (virtualNames.TryGetValue(contentInfo.Get(ContentInfo.VIRTUAL), out virtualValue) && !virtualValue.Get(ContentInfo.STORAGEPATH).Equals(contentInfo.Get(ContentInfo.STORAGEPATH)))
                {
                    // TODO : Making it optional what should happen if we get a Virtual name conflict.
                    // Duplicate of virtual name.
                    // For now we will ignore this content and send it out for others to handle if they want.
                    ErrorOccured(this, new FmdcEventArgs(ErrorContentVirtualNameConflict, contentInfo));
                }
                else
                {
                    if (virtualValue == null)
                    {
                        virtualNames.Add(contentInfo.Get(ContentInfo.VIRTUAL), contentInfo);
                        lock (share)
                        {
                            share.Add(contentInfo.Get(ContentInfo.STORAGEPATH), contentInfo);
                        }
                        // Adding status stuff
                        vd.TotalSize += contentInfo.Size;
                        vd.TotalCount++;
                    }
                    else
                    {
                        // We are trying to add a contentInfo with same name,
                        // See if content has changed. If so we will replace info.
                        ContentInfo tmpContent;
                        if (share.TryGetValue(virtualValue.Get(ContentInfo.STORAGEPATH), out tmpContent))
                        {
                            if (tmpContent.LastModified != contentInfo.LastModified || tmpContent.Size != contentInfo.Size)
                            {
                                // If this content was hashed, decrease hashed count and size.
                                if (tmpContent.IsTth)
                                {
                                    vd.HashedCount--;
                                    vd.HashedSize -= tmpContent.Size;
                                }
                                // Decrease size, not count as we will add content again.
                                vd.TotalSize -= tmpContent.Size;
                                lock (share)
                                {
                                    share.Remove(virtualValue.Get(ContentInfo.STORAGEPATH));
                                    share.Add(contentInfo.Get(ContentInfo.STORAGEPATH), contentInfo);
                                }
                                // Adding status stuff
                                vd.TotalSize += contentInfo.Size;
                            }
                        }
                    }
                }
            }
            catch (ArgumentException) { }
        }
        protected virtual void AddContent(System.IO.FileInfo fileInfo, string virtualname)
        {
            foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
            {
                if (virtualname.StartsWith(item.Key))
                {
                    AddContent(fileInfo, virtualname, item.Value);
                    return;
                }
            }
        }

        protected virtual void AddContent(System.IO.FileInfo fileInfo, string virtualname, VirtualDir vd)
        {
            // Copy interesting info
            ContentInfo info = new ContentInfo();

#if !COMPACT_FRAMEWORK
            info.LastModified = fileInfo.LastWriteTimeUtc.Ticks;
#else
            info.LastModified = fileInfo.LastWriteTime.ToFileTimeUtc();
#endif
            info.Size = fileInfo.Length;
            // TODO : Check fileInfo.Name so it will make a valid virtual name
            info.Set(ContentInfo.VIRTUAL, virtualname + fileInfo.Name);
            info.Set(ContentInfo.STORAGEPATH, fileInfo.FullName);
            info.IsHidden = ((fileInfo.Attributes & System.IO.FileAttributes.Hidden) == System.IO.FileAttributes.Hidden);
            info.IsSystem = ((fileInfo.Attributes & System.IO.FileAttributes.System) == System.IO.FileAttributes.System);
            AddContent(info, vd);
        }

        /// <summary>
        /// Removes virtual dir and ContentInfo related to this systempath
        /// </summary>
        /// <param name="systempath">Systempath to remove</param>
        /// <returns>Returns true if systempath exist as virtual dir</returns>
        public virtual bool RemoveVirtualDir(string systempath)
        {
            if (virtualDirs.ContainsKey(systempath))
            {
                int pos;
                if ((pos = share.IndexOfKey(systempath)) != -1)
                {
                    IList<string> keys = share.Keys;
                    for (int i = pos; i < keys.Count; i++)
                    {
                        if (keys[i].StartsWith(systempath))
                        {
                            ContentInfo info = null;
                            if (share.TryGetValue(keys[i], out info) && info != null)
                            {
                                virtualNames.Remove(info.Get(ContentInfo.VIRTUAL));
                                if (info.IsHashed)
                                    tthNames.Remove(info.Get(ContentInfo.TTH));
                            }
                            lock (share)
                            {
                                share.Remove(keys[i]);
                            }
                            i--;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                bool value = virtualDirs.Remove(systempath);
                Save();
                if (value)
                {
                    VirtualDirRemoved(this, new EventArgs());
                    if (IsHashing)
                        HashContent();
                }
                return value;
            }
            return false;
        }

        public virtual void CancelHashing()
        {
            cnlHashing = true;
        }

        /// <summary>
        /// Hash ContentInfos in share
        /// </summary>
        public virtual void HashContent()
        {
            while (isHashing)
            {
                cnlHashing = true;
                Thread.Sleep(250);
            }
            cnlHashing = false;
            
            // Do we need to hash share?
            if (this.TotalCount > this.HashedCount && !this.isHashing)
            {
                this.isHashing = true;
                HashingStarted(this, new EventArgs());
                VirtualDir tmp = null;
                // As we cant add items and hash items at the same time we will copy it.
                //SortedList<string, ContentInfo> tmpShare = new SortedList<string, ContentInfo>(share);
                SortedList<string, ContentInfo> tmpShare = null;
                lock (share)
                {
                    tmpShare = new SortedList<string, ContentInfo>(share);
                }
                long filesAdded = 0;

                foreach (KeyValuePair<string, ContentInfo> item in tmpShare)
                {
                    if (item.Value != null && !item.Value.IsHashed)
                    {
                        try
                        {
                            // Get virtual dir
                            if (tmp == null || !item.Value.Get(ContentInfo.STORAGEPATH).StartsWith(tmp.SystemPath))
                                tmp = GetMatchingVirtualDir(item.Value.Get(ContentInfo.STORAGEPATH));
                            // It should not be possible for this to be null.
                            if (tmp != null)
                            {
                                tmp.IsHashing = true;

                                // Ask if someone else want to hash this contentinfo
                                //ContentInfo info = item.Value;
                                FmdcEventArgs e = new FmdcEventArgs(0, item.Value);
                                HashContentInfo(this, e);
                                //info = e.Data as ContentInfo;
                                if (!e.Handled)
                                {
                                    Hashing.HashTth tth = new FlowLib.Hashing.HashTth();
                                    tth.Priority = HashThreadPriority;
                                    tth.ThreadCount = HashThreadCount;
                                    tth.Generate(item.Value);
                                }

                                // If file was hashed. Add info
                                if (item.Value.IsHashed)
                                {
                                    tthNames.Add(item.Value.Get(ContentInfo.TTH), item.Value);
                                    filesAdded++;
                                    tmp.HashedCount++;
                                    tmp.HashedSize += item.Value.Size;
                                }
                            }
                            // If we have hashed X count of files we want to save it to file.
                            // This so we dont need to rehash all share if user quits before finished.

                            if (filesAdded >= hashAutoSaveCount && hashAutoSaveCount > 0)
                            {
                                Save();
                                filesAdded = 0;
                            }
                            if (hashDelay > 0)
                                Thread.Sleep(hashDelay);
                        }
                        catch (System.Collections.Generic.KeyNotFoundException) { /* Item seem to have been removed */ }
                        catch (ArgumentException ex)
                        {
                            // Do we allow this? (We need to check here as settings can have changed since last save)
                            if (!hashAllowDuplicate)
                            {
                                lock (share)
                                {
                                    share.Remove(item.Key);
                                }
                                virtualNames.Remove(item.Value.Get(ContentInfo.VIRTUAL));
                                if (tmp != null)
                                {
                                    tmp.TotalCount--;
                                    tmp.TotalSize -= item.Value.Size;
                                }
                            }
                            // Send Notification about this
                            ErrorOccured(this, new FmdcEventArgs(Share.ErrorHashDuplicate, item.Value));
                            //Console.WriteLine(string.Format("Duplicate Item: {0} with TTH: {1}\r\n{2}", item.Value.SystemPath, item.Value.TTHBase32, tthNames[item.Value.TTHBase32]));
                        }
                        catch (System.Security.SecurityException se)
                        {
                            // User does not have the required permission. 
                            ErrorOccured(this, new FmdcEventArgs(ErrorSecurity, se));
                        }
                        catch (Exception e)
                        {
                            /* Unknown error occured. Inform everyone about it */
                            ErrorOccured(this, new FmdcEventArgs(Share.ErrorUnknownError, e));
                        }
                        finally
                        {
                            if (tmp != null)
                            {
                                tmp.IsHashing = false;
                            }
                        }
                    }
                    // If we have a other thread wanting to hash, cancel this one.
                    if (this.cnlHashing)
                    {
                        this.isHashing = false;
                        return;
                    }
                }
                tmpShare.Clear();
                this.isHashing = false;
                // We have now hashed all share and are ready to save to disk.
                Save();
                HashingCompleted(this, new EventArgs());
                // Clean memory when we are done.
                GC.Collect();
            }
        }

        protected VirtualDir GetMatchingVirtualDir(string systemPath)
        {
            // loop until we finds the virtual dir this content should be in.
            foreach (KeyValuePair<string, VirtualDir> virtualItem in virtualDirs)
            {
                if (systemPath.StartsWith(virtualItem.Value.SystemPath))
                {
                    return virtualItem.Value;
                }
            }
            return null;
        }

        /// <summary>
        /// Reloads Share.
        /// 1) Checks if added contentinfo is still valid, if not it will be removed.
        /// 2) Checks if more contentinfo can be added.
        /// 3) If changes to share has been done, hash content that has not already been hashed.
        /// </summary>
        public virtual void Reload()
        {
            #region Remove files that doesnt exist anymore
            SortedList<string, ContentInfo> removedItems = new SortedList<string, ContentInfo>();
            // We will first check if any contentInfo has been removed.
            foreach (KeyValuePair<string, ContentInfo> item in share)
            {
                if (item.Value == null)
                    continue;
                // Do file exist?
                if (!System.IO.File.Exists(item.Key))
                {
                    // As this is a enumerator we cant remove stuff from list here so we will temporary save it in a other list.
                    removedItems.Add(item.Key, item.Value);
                }
            }
            // If we want to remove stuff, do that and then save.
            if (removedItems.Count > 0)
            {
                // TODO : Add check here to notify user if many items has been removed. This is so user can reconnect network drives and so.

                // This is so we dont need to loop all virtual dirs for all files.
                VirtualDir tmp = null;
                foreach (KeyValuePair<string, ContentInfo> item in removedItems)
                {
                    // have we virtual dir temporary stored?
                    if (tmp == null || !tmp.SystemPath.StartsWith(tmp.SystemPath))
                    {
                        tmp = GetMatchingVirtualDir(item.Value.Get(ContentInfo.STORAGEPATH));
                    }
                    if (tmp != null)
                    {
                        // We have found virtual dir.
                        if (item.Value.IsHashed)
                        {
                            tmp.HashedCount--;
                            tmp.HashedSize -= item.Value.Size;
                            tthNames.Remove(item.Value.Get(ContentInfo.TTH));
                        }
                        tmp.TotalCount--;
                        tmp.TotalSize -= item.Value.Size;
                        virtualNames.Remove(item.Value.Get(ContentInfo.VIRTUAL));
                        lock (share)
                        {
                            share.Remove(item.Key);
                        }
                    }
                }
                Save();
            }
            #endregion
            bool changing = false;
            // Refreshing all virtual directories.

            foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
            {
                // If we are adding/removing or on any other way already changing share,
                // we dont want to reload list.
                if (item.Value.IsChanging)
                {
                    changing = true;
                    continue;
                }
                long fileCountBeforeAdding = item.Value.TotalCount;
                AddDir(item.Value.SystemPath, item.Value.VirtualPath, item.Value);
                // If count has changed. Save.
                if (fileCountBeforeAdding != item.Value.TotalCount)
                    Save();
            }
            // TODO : This is a ugly way todo it. Can it be done in a diffrent way?
            if (!changing)
            {
                HashContent();
            }
        }
        /// <summary>
        /// Loads settings for share setting file from AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public void Load()
        {
#if !COMPACT_FRAMEWORK
            Load(System.AppDomain.CurrentDomain.BaseDirectory);
#else
            Load(System.IO.Directory.GetCurrentDirectory());
#endif
        }
        /// <summary>
        /// Loads settings for share setting file from directory.
        /// </summary>
        /// <param name="dir">Directory that you will look for setting file in</param>
        public virtual void Load(string dir)
        {
            directory = dir;
            SettingsGroup setting = new SettingsGroup();
            SettingsGroup.Load(directory + "Share" + name + ".xml", out setting, "Share");
            int tmp = -1;
            #region Update Settings
            tmp = setting.GetInt(IndexLastModifiedTimeStamp);
            if (tmp != -1)
                lastModifiedTimeStamp =tmp;
            HashAllowDuplicate = setting.GetBool(IndexHashAllowDuplicate);
            tmp = setting.GetInt(IndexHashAutoSaveCount);
            if (tmp != -1)
                HashAutoSaveCount = tmp;
            tmp = setting.GetInt(IndexHashThreadCount);
            if (tmp != -1)
                HashThreadCount = tmp;
            tmp = setting.GetInt(IndexHashThreadPriority);
            if (tmp != -1)
                HashThreadPriority = (System.Threading.ThreadPriority)tmp;
            tmp = setting.GetInt(IndexHashThreadSleep);
            if (tmp != -1)
                HashThreadSleep = tmp;
            tmp = setting.GetInt(IndexPort);
            if (tmp != -1)
                Port = tmp;
            #endregion
            #region Update Virtual Dirs
            // Update where Virtual dir list begins.
            tmp = setting.GetInt(IndexVirtualDirBegin);
            if (tmp != -1)
                virtualDirBegin = tmp;
            // Update virtual dir count.
            int virtualCount = setting.GetInt(IndexVirtualDirCount);
            #region Update VirtualDir items
            if (virtualCount != -1 && setting.GetObject(virtualDirBegin) != null)
            {
                for (int i = virtualDirBegin; i < virtualCount+virtualDirBegin; i++)
                {
                    VirtualDir value = (VirtualDir)setting.GetObject(i);
                    if (value != null)
                    {
                        virtualDirs.Add(value.SystemPath, value);
                        lock (share)
                        {
                            share.Add(value.SystemPath, null);
                        }
                    }
                }
            }
            #endregion
            #endregion
            #region Update Share
            // Update where Share list begins.
            tmp = setting.GetInt(IndexShareBegin);
            if (tmp != -1)
                shareBegin = tmp;
            // Update share count.
            int shareCount = setting.GetInt(IndexShareCount);
            #region Update Share items
            if (shareCount != -1 && setting.GetObject(shareBegin) != null)
            {
                for (int i = shareBegin; i < shareCount + shareBegin; i++)
                {
                    ContentInfo content = (ContentInfo)setting.GetObject(i);
                    if (content != null)
                    {
                        lock (share)
                        {
                            share.Add(content.Get(ContentInfo.STORAGEPATH), content);
                        }
                        virtualNames.Add(content.Get(ContentInfo.VIRTUAL), content);
                        // TODO : Add setting so we know if we allow dublicate files with same TTH in share.
                        try
                        {
                            if (content.IsTth)
                                tthNames.Add(content.Get(ContentInfo.TTH), content);
                        }
                        catch (ArgumentException) {
                            // We have already added a file with this tth.
                            // Do we allow this? (We need to check here as settings can have changed since last save)
                            if (!hashAllowDuplicate)
                            {
                                VirtualDir vd;
                                if ((vd = this.GetMatchingVirtualDir(content.Get(ContentInfo.STORAGEPATH))) != null)
                                {
                                    // Remove file if we dont allow duplicates.
                                    virtualNames.Remove(content.Get(ContentInfo.VIRTUAL));
                                    lock (share)
                                    {
                                        share.Remove(content.Get(ContentInfo.STORAGEPATH));
                                    }
                                    vd.TotalCount--;
                                    vd.TotalSize -= content.Size;
                                }
                            }
                        }
                    }
                }

            }
            #endregion
            #endregion
        }
        /// <summary>
        /// Removes setting file from directory that was used to load content
        /// </summary>
        /// <returns></returns>
        public virtual bool Remove()
        {
            return Remove(directory);
        }
        /// <summary>
        /// Removes setting file from directory specified
        /// </summary>
        /// <param name="dir">directory where we want to delete settings</param>
        /// <returns>returns true only if settings was deleted</returns>
        public virtual bool Remove(string dir)
        {
            try
            {
                System.IO.File.Delete(dir + "Share" + name + ".xml");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Saves setting file in directory used for loading
        /// </summary>
        public virtual void Save()
        {
            // Update last modified
            LastModifiedChanged(this, new FmdcEventArgs(0, lastModifiedTimeStamp));
            if (directory != null)
                Save(directory);
        }
        /// <summary>
        /// Saves setting file in directory specified
        /// </summary>
        /// <param name="dir">directory to save in</param>
        public virtual void Save(string dir)
        {
            directory = dir;
            while (isSaving)
            {
                Thread.Sleep(250);
            }
            isSaving = true;
            SettingsGroup setting = new SettingsGroup();
            // Stores where virtual dirs and share list begins and ends.
            setting.Add(IndexVirtualDirBegin, new SettingItem(virtualDirBegin, -1));
            setting.Add(IndexVirtualDirCount, new SettingItem(virtualDirs.Count, -1));
            setting.Add(IndexShareBegin, new SettingItem(shareBegin, -1));
            setting.Add(IndexShareCount, new SettingItem(share.Count, -1));
            setting.Add(IndexLastModifiedTimeStamp, new SettingItem(LastModified, -1));
            setting.Add(IndexHashAllowDuplicate, new SettingItem(HashAllowDuplicate, false));
            setting.Add(IndexHashAutoSaveCount, new SettingItem(HashAutoSaveCount, 10));
            setting.Add(IndexHashThreadCount, new SettingItem(HashThreadCount, 4));
            setting.Add(IndexHashThreadPriority, new SettingItem((int)HashThreadPriority, (int)System.Threading.ThreadPriority.Lowest));
            setting.Add(IndexHashThreadSleep, new SettingItem(HashThreadSleep, 0));
            setting.Add(IndexPort, new SettingItem(Port, -1));


            // Stores Virtual dirs
            int tmpVirtualDirBegin = virtualDirBegin;
            foreach (KeyValuePair<string, VirtualDir> item in virtualDirs)
            {
                setting.Add(tmpVirtualDirBegin++, new SettingItem(item.Value, null));
            }
            // Stores ContentInfo
            int tmpshareBegin = shareBegin;
            foreach (KeyValuePair<string, ContentInfo> item in share)
            {
                if (item.Value != null)
                    setting.Add(tmpshareBegin++, new SettingItem(item.Value, null));
            }

            // Saves info on disc
            FlowLib.Utils.FileOperations.PathExists(directory);
            SettingsGroup.Save(directory + "Share" + name + ".xml", setting, "Share");
            isSaving = false;
        }

        /// <summary>
        /// Do this share contain info as content?
        /// This function will also give you a complete ContentInfo back
        /// </summary>
        /// <param name="info">Content you want to know if it exist in this share or not.</param>
        /// <param name="fillContentInfo">specify if we fill in missing info</param>
        /// <returns>True is returned if content exsist, else False</returns>
        public virtual bool ContainsContent(ref ContentInfo info)
        {
            bool found = false;
            if (info.IsHashed && tthNames.ContainsKey(info.Get(ContentInfo.TTH)))
            {
                info = tthNames[info.Get(ContentInfo.TTH)];
                found = true;
            }
            else if (info.ContainsKey(ContentInfo.STORAGEPATH) && share.ContainsKey(info.Get(ContentInfo.STORAGEPATH)))
            {
                info = share[info.Get(ContentInfo.STORAGEPATH)];
                found = true;
            }
            else if (info.ContainsKey(ContentInfo.VIRTUAL) && virtualNames.ContainsKey(info.Get(ContentInfo.VIRTUAL)))
            {
                info = virtualNames[info.Get(ContentInfo.VIRTUAL)];
                found = true;
            }
            // TODO : Add Filtering matchning here.
            return found;
        }

        /// <summary>
        /// Returns byte[] of the section of this content you want.
        /// </summary>
        /// <param name="info">Tells what content you want, Path and Name has to be set</param>
        /// <param name="start">Tells where to start reading</param>
        /// <param name="length">Tells how long we should read</param>
        /// <returns>If ContentInfo exist in this share it will be returned, else null.</returns>
        public virtual byte[] GetContent(ContentInfo info, long start, long length)
        {
            try
            {
                // Do content exist?
                // Is start more then end?
                // Is end more then info size?
                if (ContainsContent(ref info) && start >= 0 && (start + length) <= info.Size && length != 0)
                {
                    // TODO : Make this Max buffer size a setting.
                    if (length > 4194304)
                        length = 4194304;
                    // Read requested bytes
                    //using (BinaryReader reader = new BinaryReader(new StreamReader(info.SystemPath).BaseStream))
                    //{
                    StreamReader sr = new StreamReader(info.Get(ContentInfo.STORAGEPATH));
                        BinaryReader reader = new BinaryReader(sr.BaseStream);
                        //BinaryReader reader = new BinaryReader(new StreamReader(info.SystemPath).BaseStream);
                        byte[] buffer = new byte[length];
                        reader.BaseStream.Position = start;
                        buffer = reader.ReadBytes(buffer.Length);
                        reader.Close();
                        sr.Close();
                        return buffer;
                    //}
                    // TODO : Adding a kind of buffer here would be nice :)
                }
                // TODO : Change so we are realy looking for more specific errors. Security errors and so on.
            }
            catch (Exception) { /* TODO : This should be changes later to more specific errors like security and io errors */ }
            return null;
        }

        #region OnEvents
        protected void OnErrorOccured(object sender, FmdcEventArgs e)
        {

        }
        #endregion

        #region IEnumerable<KeyValuePair<string,ContentInfo>> Members
        //public IEnumerator<KeyValuePair<string, ContentInfo>> GetEnumerator()
        IEnumerator<KeyValuePair<string, ContentInfo>> IEnumerable<KeyValuePair<string, ContentInfo>>.GetEnumerator()
        {
            lock (share)
            {
                return share.GetEnumerator();
            }
        }
        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            lock (share)
            {
                if (share.Count != 0)
                    return share.GetEnumerator();
                else if (tthNames.Count != 0)
                    return tthNames.GetEnumerator();
                else
                    return virtualNames.GetEnumerator();
            }
        }

        #endregion
    }
}
