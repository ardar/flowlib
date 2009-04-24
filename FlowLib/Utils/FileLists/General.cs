
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

namespace FlowLib.Utils.FileLists
{
    public static class General
    {
        /// <summary>
        /// Adds/Update common filelists to this share and saves them in directory specified
        /// Filelist included are:
        /// BZList, XmlBzList (UTF-8 and ASCII)
        /// </summary>
        /// <param name="share">Share you want to update/add filelist to</param>
        /// <param name="directory">Directory where you want to save filelists in</param>
        public static void AddCommonFilelistsToShare(Share share, string directory)
        {
            // Xml Utf-8 (Current DC++)
            FlowLib.Utils.FileLists.FilelistXmlBz2 xml = new FlowLib.Utils.FileLists.FilelistXmlBz2(share);
            xml.SystemPath = directory;
            xml.Encoding = System.Text.Encoding.UTF8;
            xml.CreateFilelist();
            share.RemoveFile(xml.ContentInfo);
            share.AddFile(xml.ContentInfo);
            // Xml Ascii (Early DC++)
            xml.Encoding = System.Text.Encoding.ASCII;
            xml.CreateFilelist();
            share.RemoveFile(xml.ContentInfo);
            share.AddFile(xml.ContentInfo);
            // Xml Utf-8 (Adc Standard list)
            xml.Bz2 = false;
            xml.SystemPath = directory;
            xml.Encoding = System.Text.Encoding.UTF8;
            xml.CreateFilelist();
            share.RemoveFile(xml.ContentInfo);
            share.AddFile(xml.ContentInfo);
            // BzList
            FlowLib.Utils.FileLists.FilelistMyList dclst = new FlowLib.Utils.FileLists.FilelistMyList(share);
            dclst.SystemPath = directory;
            dclst.CreateFilelist();
            share.RemoveFile(dclst.ContentInfo);
            share.AddFile(dclst.ContentInfo);
        }
    }
}
