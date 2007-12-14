/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

namespace FlowLib.Utils
{
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
            char[] forbiddenChars = System.IO.Path.GetInvalidFileNameChars();
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
                File.WriteAllBytes(target, new byte[size]);
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
