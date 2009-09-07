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

using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace FlowLib.Utils
{
    public static class FileOperations<T>
    {
        public static void SaveObject(string path, T obj)
        {
            FileOperations.PathExists(path);
            XmlSerializer s = new XmlSerializer(obj.GetType());
            TextWriter w = new StreamWriter(path);
            s.Serialize(w, obj);
            w.Close();
        }

        public static T LoadObject(string path)
        {
            XmlSerializer s = new XmlSerializer(typeof(T));
            if (File.Exists(path))
            {
                T obj;
                TextReader r = new StreamReader(path);
                obj = (T)s.Deserialize(r);
                r.Close();
                return obj;
            }
            else
            {
                return default(T);
            }
        }
    }

    public static class FileOperations
    {
        public static bool IsValidFilePath(string path)
        {
            char[] forbiddenChars = System.IO.Path.GetInvalidPathChars();
            for (int i = 0; i < forbiddenChars.Length; i++)
            {
                if (path.Contains(forbiddenChars[i].ToString()))
                {
                    // path is containing invalid chars
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidFileName(string name)
        {
#if !COMPACT_FRAMEWORK
            char[] forbiddenChars = System.IO.Path.GetInvalidFileNameChars();
#else            
            char[] forbiddenChars = System.IO.Path.GetInvalidPathChars();
#endif
            for (int i = 0; i < forbiddenChars.Length; i++)
            {
                if (name.Contains(forbiddenChars[i].ToString()))
                {
                    // filename contains invalid chars.
                    return false;
                }
            }
            return true;
        }

        public static void AllocateFile(string target, long size)
        {
            if (!PathExists(target))
            {
                FileStream fs = new FileStream(target, FileMode.OpenOrCreate);
                fs.SetLength(size);
                fs.Close();
            }
        }

        public static bool PathExists(string target)
        {
            char sep = System.IO.Path.DirectorySeparatorChar;
            int pos;
            if ((pos = target.LastIndexOf(sep)) > 0)
            {
                string dir = target.Substring(0, pos);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }
            return System.IO.File.Exists(target);
        }
    }
}
