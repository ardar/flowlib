
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

namespace FlowLib.Utils.FileLists
{
    public class FilelistXmlBz2 : BaseFilelist
    {
        protected bool fromXml = false;
        protected Stack<string> dirs = new Stack<string>();
        protected long changed = 0;

        protected System.IO.MemoryStream xmlBaseStream = null;
        protected System.IO.FileStream baseStream = null;
        protected Encoding encoding = Encoding.UTF8;
        protected XmlTextWriter writer = null;
        protected string systemPath = null;
        protected string virtualFileName = string.Empty;
        protected string generator = string.Empty;
        protected string cid = string.Empty;
        protected bool bz2 = true;
        protected string extention = ".xml.bz2";
        protected long size = 0;

        public override Containers.ContentInfo ContentInfo
        {
            get
            {
                ContentInfo content = new ContentInfo();
                content.Set(ContentInfo.VIRTUAL, virtualFileName + encoding.WebName + "files" + extention);
                content.Set(ContentInfo.FILELIST, XMLBZ);
                content.Set(ContentInfo.STORAGEPATH, systemPath + FileName);
                content.Size = size;
                content.LastModified = System.DateTime.Now.Ticks;
                return content;
            }
        }

        public bool Bz2
        {
            get { return bz2; }
            set {
                if (value)
                    extention = ".xml.bz2";
                else
                    extention = ".xml";
                bz2 = value;
            }
        }

        protected string FileName
        {
            get
            {
                if (share != null)
                    return share.Name + virtualFileName + encoding.WebName + extention;
                else
                    return virtualFileName + encoding.WebName + extention;
            }
        }

        public string SystemPath
        {
            get { return systemPath; }
            set { systemPath = value; }
        }

        public Encoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        public string Generator
        {
            get { return generator; }
            set { generator = value; }
        }

        public string CID
        {
            get { return cid; }
            set { cid = value; }
        }

        public FilelistXmlBz2(byte[] content, bool isBz2)
            : base(null)
        {
            xmlBaseStream = new System.IO.MemoryStream();
            xmlBaseStream.Write(content, 0, content.Length);
            Bz2 = isBz2;
            fromXml = true;
        }

        public FilelistXmlBz2(Share share)
            : base(share)
        {
            systemPath = "";
        }


        public FilelistXmlBz2(Share share, string systemPath)
            : base(share)
        {
            this.systemPath = systemPath;
        }

        public FilelistXmlBz2(Share share, Encoding xmlEncoding)
            : this(share)
        {
            this.encoding = xmlEncoding;
        }

        public FilelistXmlBz2(Share share, Encoding xmlEncoding, string systemPath)
            : this(share, xmlEncoding)
        {
            this.systemPath = systemPath;
        }

        public override void CreateShare()
        {
            if (!fromXml)
                throw new System.ArgumentException("This method cant be used when you created this object from this constructor");
            share = new Share(string.Empty);
            xmlBaseStream.Position = 0;
            XmlTextReader reader = null;
            string virtualDir = string.Empty;
            long created = 0;

            // If this list is comressed
            if (bz2)
            {
                System.IO.MemoryStream tmpStream = new System.IO.MemoryStream();
                Utils.Compression.Bz2.Decompress(xmlBaseStream, tmpStream);
                tmpStream.Position = 0;
                reader = new XmlTextReader(tmpStream);
            }
            else
            {
                reader = new XmlTextReader(xmlBaseStream);
            }
            if (reader != null)
            {
                do
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.HasAttributes)
                            {
                                if (reader.Name.Equals("File", StringComparison.OrdinalIgnoreCase))
                                {
                                    string name = reader.GetAttribute("Name");
                                    long size = -1;
                                    try
                                    {
                                        size = long.Parse(reader.GetAttribute("Size"));
                                    }
                                    catch { }
                                    string tth = reader.GetAttribute("TTH");
                                    Containers.ContentInfo ci = null;
                                    if (string.IsNullOrEmpty(tth))
                                    {
                                        ci = new ContentInfo(ContentInfo.NAME, name);
                                    }
                                    else
                                    {
                                        ci = new ContentInfo(ContentInfo.TTH,tth);
                                    }
                                    ci.Size = size;

                                    #region Adding virtual dir
                                    if (true || changed > created)
                                    {
                                        created = DateTime.Now.Ticks;
                                        virtualDir = "";
                                        string[] tmpDirs = dirs.ToArray();
                                        for (int i = tmpDirs.Length -1; i >= 0; i--)
                                        {
                                            virtualDir += tmpDirs[i] + @"\";
                                        }
                                    }
                                    ci.Set(ContentInfo.VIRTUAL, virtualDir + name);
                                    ci.Set(ContentInfo.NAME, name);
                                    #endregion

                                    AddFile(ci);
                                }
                                else if (reader.Name.Equals("Directory", StringComparison.OrdinalIgnoreCase))
                                {
                                    // Add directory
                                    string name = reader.GetAttribute("Name");
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        StartDirectory(name);
                                    }
                                }
                                else if (reader.Name.Equals("FileListing", StringComparison.OrdinalIgnoreCase))
                                {
                                    generator = reader.GetAttribute("Generator");
                                    cid = reader.GetAttribute("CID");
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name.Equals("Directory"))
                            {
                                // Remove directory
                                EndDirectory();
                            }
                            break;
                    }
                } while (reader.Read());
            }
        }

        protected override void StartFilelist()
        {
            if (!fromXml)
            {
                try
                {
                    hasWritenFilelist = false;
                    xmlBaseStream = new System.IO.MemoryStream();
                    writer = new XmlTextWriter(xmlBaseStream, encoding);
                    writer.Formatting = Formatting.Indented;
                    writer.IndentChar = '\t';
                    writer.Indentation = 1;
                    writer.WriteStartDocument(true);
                    writer.WriteStartElement("FileListing");
                    writer.WriteStartAttribute("Version");
                    writer.WriteValue("1");
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("Generator");
                    writer.WriteValue(generator);
                    writer.WriteEndAttribute();
                    writer.WriteStartAttribute("Base");
                    writer.WriteValue("/");
                    writer.WriteEndAttribute();
                }
                catch (System.IO.IOException)
                {
                    // TODO : Add Events for showing errors here.
                }
            }
            else
            {
                // TODO : 
            }
        }

        protected override void EndFilelist()
        {
            try
            {
                #region End Xml document
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                #endregion
                Utils.FileOperations.PathExists(systemPath);
                baseStream = new System.IO.FileStream(systemPath + FileName, System.IO.FileMode.Create);
                xmlBaseStream.Position = 0;
                // We dont want to have filelist compressed by bz2.
                #region Write xml content to file.
                if (!bz2)
                {
                    byte[] buffer = new byte[2048];
                    size = 0;
                    int pos;
                    while ((pos = xmlBaseStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        baseStream.Write(buffer, 0, pos);
                        size += pos;
                    }
                    writer.Close();
                    baseStream.Flush();
                    baseStream.Close();
                    return;
                }
                #endregion
                #region Bz2

                size = Utils.Compression.Bz2.Compress(xmlBaseStream, baseStream, 512);
                writer.Close();
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
            if (!fromXml)
            {
                // Make XML filelist
                writer.WriteStartElement("Directory");
                writer.WriteStartAttribute("Name");
                writer.WriteValue(name);
                writer.WriteEndAttribute();
            }
            else
            {
                // Make share from xml filelist
                dirs.Push(name);
                changed = DateTime.Now.Ticks;
            }
        }

        protected override void EndDirectory()
        {
            if (!fromXml)
            {
                // Make XML filelist
                writer.WriteEndElement();
            }
            else
            {
                // Make share from xml filelist
                dirs.Pop();
                changed = DateTime.Now.Ticks;
            }
        }

        protected override void AddFile(ContentInfo content)
        {
            if (!fromXml)
            {
                // Make XML filelist
                string tmpName = content.Get(ContentInfo.VIRTUAL);
                int pos;
                if ((pos = tmpName.LastIndexOf(seperator)) != -1)
                    tmpName = content.Get(ContentInfo.VIRTUAL).Substring(++pos);

                writer.WriteStartElement("File");
                writer.WriteStartAttribute("Name");
                writer.WriteValue(tmpName);
                writer.WriteEndAttribute();
                writer.WriteStartAttribute("Size");
                writer.WriteValue(content.Size);
                writer.WriteEndAttribute();
                if (content.IsTth)
                {
                    writer.WriteStartAttribute("TTH");
                    writer.WriteValue(content.Get(Containers.ContentInfo.TTH));
                    writer.WriteEndAttribute();
                }
                writer.WriteEndElement();
            }
            else
            {
                // Make share from xml filelist
                share.AddFile(content);
            }
        }
    }
}
