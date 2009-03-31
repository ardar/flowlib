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
            XmlSerializer s = new XmlSerializer(obj.GetType(), new XmlRootAttribute());
            TextWriter w = new StreamWriter(path);
            s.Serialize(w, obj);
            w.Close();
        }

        public static T LoadObject(string path)
        {
            XmlSerializer s = new XmlSerializer(typeof(T), new XmlRootAttribute());
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
                //File.WriteAllBytes(target, new byte[ size]);
                // Doing it as below is working on compact .Net and we can handle big files (if you didnt have 2gb of free ram and wanted to write a 3 gb big file this made a problem before)
                int SEGSIZE = 1024 * 1024;
                using(FileStream fs = new FileStream(target, FileMode.CreateNew, FileAccess.Write, FileShare.None, SEGSIZE))
                //using (FileStream fs = File.Create(target, SEGSIZE))
                {
                    int i = 0;
                    int segmentSize = SEGSIZE;
                    do
                    {
                        // Set length to segment size
                        segmentSize = SEGSIZE;
                        // Is segment size bigger then content size?
                        if (segmentSize > size)
                        {
                            segmentSize = (int)size;
                        }
                        // Are we at last segment
                        else if ((1 + i) * segmentSize > size)
                        {
                            segmentSize = (int)(size % segmentSize);
                        }
                        fs.Write(new byte[segmentSize], 0, segmentSize);
                        System.Threading.Thread.Sleep(1);
                        i++;
                    } while ((i * SEGSIZE) < size);
                }
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
