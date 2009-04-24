
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

using System.Xml;
using System.Text;
using FlowLib.Containers;
using FlowLib.Enums;
using System.Collections.Generic;
using System;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Utils.FileLists
{
    public class FilelistMyList : BaseFilelist
    {
        protected System.IO.FileStream baseStream = null;
        protected StringBuilder sb = null;
        protected Stack<string> dirs = new Stack<string>();

        protected bool fromFile = false;
        protected string systemPath = null;
        protected string fileName = null;
        protected bool bz2 = true;
        protected string extention = ".bz2";
        protected long size = 0;
        protected long changed = 0;
        protected long indent = 0;

        public string SystemPath
        {
            get { return systemPath; }
            set { systemPath = value; }
        }

        public override ContentInfo ContentInfo
        {
            get
            {
                ContentInfo content = new ContentInfo();
                content.Set(ContentInfo.VIRTUAL, "MyList" + extention);
                content.Set(ContentInfo.STORAGEPATH, systemPath + share.Name + ".DcLst" + extention);
                content.Set(ContentInfo.FILELIST, BZ);

                content.Size = size;
                content.LastModified = System.DateTime.Now.Ticks;
                return content;
            }
        }

        public bool Bz2
        {
            get { return bz2; }
            set
            {
                if (value)
                    extention = ".bz2";
                else
                    extention = ".DcLst";
                bz2 = value;
            }
        }

        public FilelistMyList(byte[] content, bool isBz2)
            : base(null)
        {
            sb = new StringBuilder();
            if (!isBz2)
                sb.Append(System.Text.Encoding.UTF8.GetString(content));
            else
            {
                System.IO.MemoryStream tmpStream = new System.IO.MemoryStream();
                Utils.Compression.Bz2.Decompress(new System.IO.MemoryStream(content), tmpStream);
                tmpStream.Position = 0;
                sb.Append(System.Text.Encoding.UTF8.GetString(tmpStream.ToArray()));
            }
            Bz2 = isBz2;
            fromFile = true;
        }

        public FilelistMyList(Share share)
            : base(share)
        {
            systemPath = "";
            if (share != null)
                fileName = share.Name;
        }


        public FilelistMyList(Share share, string systemPath)
            : base(share)
        {
            this.systemPath = systemPath;
            if (share != null)
                fileName = share.Name;
        }

        protected override void StartFilelist()
        {
            sb = new StringBuilder();
        }

        protected override void EndFilelist()
        {
            try
            {
                Utils.FileOperations.PathExists(ContentInfo.Get(ContentInfo.STORAGEPATH));
                baseStream = new System.IO.FileStream(ContentInfo.Get(ContentInfo.STORAGEPATH), System.IO.FileMode.Create);
                byte[] data = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                #region Bz2
                if (bz2)
                {
                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    ms.Write(data, 0, data.Length);
                    ms.Position = 0;
                    size = Utils.Compression.Bz2.Compress(ms, baseStream, 512);
                }
                else
                {
                    baseStream.Write(data, 0, data.Length);
                }
                #endregion
                // If no error has occured. Show that it has finished without errors.
                hasWritenFilelist = true;
            }
            catch (System.UnauthorizedAccessException) { }
            catch (System.Security.SecurityException) { }
            catch (System.ArgumentException) { }
            catch (System.IO.IOException) { }
            // TODO : Add Events for showing errors here.
        }

        protected override void StartDirectory(string name)
        {
            if (!fromFile)
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append("\t");
                }
                sb.Append(name);
                sb.AppendLine();
                indent++;
            }
            else
            {
                // Make share from filelist
                dirs.Push(name);
                changed = DateTime.Now.Ticks;
            }
        }

        protected override void EndDirectory()
        {
            if (!fromFile)
            {
                indent--;
            }
            else
            {
                // Make share from filelist
                dirs.Pop();
                changed = DateTime.Now.Ticks;
            }
        }

        protected override void AddFile(ContentInfo content)
        {
            if (!fromFile)
            {
                for (int i = 0; i < indent; i++)
                {
                    sb.Append("\t");
                }
                sb.Append(System.IO.Path.GetFileName(content.Get(ContentInfo.VIRTUAL)));
                sb.Append("|");
                sb.Append(content.Size.ToString());
                sb.AppendLine();
            }
            else
            {
                // Make share from filelist
                share.AddFile(content);
            }
        }

        public override void CreateShare()
        {
            if (!fromFile)
                throw new System.ArgumentException("This method cant be used when you created this object from this constructor");
            share = new Share(string.Empty);

            sb.Replace("\r\n", "\n");
            string content = sb.ToString();
            int i;

            while ((i = content.IndexOf("\n")) != -1)
            {
                string name = content.Substring(0, i);
                int sep;
                if ((sep = name.IndexOf("|")) != -1)
                {
                    // We have file info
                    long size = -1;
                    try
                    {
                        size = long.Parse(name.Substring(sep + 1));
                    }
                    catch { }
                    name = name.Substring(0, sep).Trim('\t');
                    ContentInfo ci = new ContentInfo(ContentInfo.NAME, name);
                    ci.Size = size;

                    #region Adding virtual dir
                    string virtualDir = @"";

                    string[] tmpDirs = dirs.ToArray();
                    for (int vpos = tmpDirs.Length - 1; vpos >= 0; vpos--)
                    {
                        virtualDir += tmpDirs[vpos] + @"\";
                    }
                    ci.Set(ContentInfo.VIRTUAL, virtualDir + name);
                    ci.Set(ContentInfo.NAME, name);
                    #endregion
                    AddFile(ci);
                }
                else
                {
                    // We have a directory
                    int ind = 0;
                    while ((sep = name.IndexOf("\t")) != -1)
                    {
                        name = name.Remove(sep, +1);
                        ind++;
                    }
                    // Remove directory
                    if (ind <= indent && indent != 0)
                    {
                        EndDirectory();
                    }
                    if (ind >= indent)
                    {
                        StartDirectory(name);
                    }
                }
                content = content.Remove(0, i+1);
            }
        }
    }
}
