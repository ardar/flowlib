/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using FlowLib.Containers;
using System;

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
        static SortedList<string, FileStreamContainer> _openStreams = new SortedList<string, FileStreamContainer>(5);
        static System.Threading.Timer _openstreamTimer;

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

        //public static void AllocateFile(string target, long size)
        //{
        //    if (!PathExists(target))
        //    {
        //        FileStream fs = new FileStream(target, FileMode.OpenOrCreate);
        //        fs.SetLength(size);
        //        fs.Close();
        //    }
        //}

        public static void AllocateFile(string path, long size)
        {
            if (!PathExists(path))
            {
                FileStreamContainer fsc = null;
                FileStream fs = null;

                // Make sure FileStreamContainer is not removed while we are trying to access it.
                lock (_openStreams)
                {
                    if (_openStreams.ContainsKey(path))
                    {
                        fsc = _openStreams[path];
                        fs = fsc.FileStream;
                    }
                    else
                    {
                        fs = new FileStream(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Write);
                        fsc = new FileStreamContainer
                        {
                            FileStream = fs,

                        };
                        _openStreams.Add(path, fsc);
                    }
                    fsc.LastAccessed = DateTime.Now.Ticks;
                }

                fs.SetLength(size);
                fsc.LastAccessed = DateTime.Now.Ticks;
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

        public static void CheckFileStreamContainers(object sender)
        {
            lock (_openStreams)
            {
                List<string> keys = new List<string>();
                long maxTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 30)).Ticks;
                foreach (KeyValuePair<string, FileStreamContainer> pair in _openStreams)
                {
                    if (pair.Value.LastAccessed < maxTime)
                    {
                        FileStream fs = pair.Value.FileStream;
                        lock (fs)
                        {
                            fs.Dispose();
                            fs.Close();
                            keys.Add(pair.Key);
                        }
                    }
                }

                foreach (string key in keys)
                {
                    _openStreams.Remove(key);
                }
            }
        }

        public static void ForceClose(string path)
        {
            lock (_openStreams)
            {
                FileStreamContainer fsc = null;
                FileStream fs = null;
                if (_openStreams.ContainsKey(path))
                {
                    fsc = _openStreams[path];
                    fs = fsc.FileStream;
                    lock (fs)
                    {
                        fs.Dispose();
                        fs.Close();
                    }
                    _openStreams.Remove(path);
                }
            }
        }

        public static void WriteContent(string path, ref SegmentInfo currentSegment, byte[] data, int length)
        {
            FileStreamContainer fsc = null;
            FileStream fs = null;
            // Make sure FileStreamContainer is not removed while we are trying to access it.
            lock (_openStreams)
            {
                if (_openStreams.ContainsKey(path))
                {
                    fsc = _openStreams[path];
                    fs = fsc.FileStream;
                }
                else
                {
                    fs = new FileStream(path, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.Write);
                    fsc = new FileStreamContainer
                    {
                        FileStream = fs
                    };
                    _openStreams.Add(path, fsc);
                }
                fsc.LastAccessed = DateTime.Now.Ticks;
            }

            // Make sure we are the only ones writing to this file
            lock (fs)
            {
                Console.WriteLine(string.Format("\tFile:{1}, {3}/{2}", path, currentSegment.Start + currentSegment.Position, currentSegment.Length, currentSegment.Position + length));
                // Lock this segment of file
                fs.Lock(currentSegment.Start, currentSegment.Length);
                // Set position
                fs.Position = currentSegment.Start + currentSegment.Position;
                // Write this byte array to file
                fs.Write(data, 0, length);
                // Saves and unlocks file
                fs.Flush();
                fs.Unlock(currentSegment.Start, currentSegment.Length);
            }

            fsc.LastAccessed = DateTime.Now.Ticks;


            if (_openstreamTimer == null)
            {
                const int interval = 10 * 1000;
                _openstreamTimer = new System.Threading.Timer(new System.Threading.TimerCallback(CheckFileStreamContainers), null, interval, interval);
            }
        }
    }
}
