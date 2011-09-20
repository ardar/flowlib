
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

using System;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace FlowLib.Utils.CompactFramworkExtensionMethods
{
    public static class CompactEncoding
    {
        public static SortedList<int, object> threadData = new SortedList<int, object>(0);

        public static string GetString(this Encoding enc, byte[] bytes)
        {
            return enc.GetString(bytes, 0, bytes.Length);
        }

        public static void Lock(this FileStream fs, long start, long length)
        {
            // This is just a dummy function for now.
        }

        public static void Unlock(this FileStream fs, long start, long length)
        {
            // This is just a dummy function for now.
        }

        public static void Disconnect(this Socket s, bool value)
        {
            // We should probably do more stuff here.
            if (s.Connected)
            {
                s.Close();
            }
        }

        public static void Start(this Thread thread, object obj)
        {
            threadData.Add(thread.ManagedThreadId, obj);
            thread.Start();
        }

        public static object GetData(this Thread thread)
        {
            object obj = threadData[thread.ManagedThreadId];
            threadData.Remove(thread.ManagedThreadId);
            return obj;
        }

        public static bool IsAlive(this Thread thread)
        {
            return threadData.ContainsKey(thread.ManagedThreadId);
        }
    }
}
