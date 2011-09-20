
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
using FlowLib.Containers;

namespace FlowLib.Utils.Convert.Settings
{
    public abstract class BaseClient
    {
        protected List<HubSetting> hubs = new List<HubSetting>();
        protected HubSetting current = null;

        public List<HubSetting> Hubs
        {
            get { return hubs; }
            set { hubs = new List<HubSetting>(value); }
        }

        public bool Read(string path)
        {
            if (File.Exists(path))
            {
#if !COMPACT_FRAMEWORK
                return Read(File.ReadAllBytes(path));
#else
                byte[] data;
                using (System.IO.FileStream fs = System.IO.File.OpenRead(path))
                {
                    data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    fs.Close();
                    return Read(data);
                }
#endif

            }
            else
            {
                // TODO : We should try to look for a file here
            }
            return false;
        }

        public void Write(string path)
        {
            FlowLib.Utils.FileOperations.PathExists(path);
#if !COMPACT_FRAMEWORK
            File.WriteAllBytes(path, Write());
#else
            using (FileStream fs = File.OpenWrite(path))
            {
                byte[] data = Write();
                fs.Write(data, 0, data.Length);
                fs.Flush();
                fs.Dispose();
                fs.Close();
            }
#endif
        }

        public abstract byte[] Write();
        public abstract bool Read(byte[] data);
    }
}
