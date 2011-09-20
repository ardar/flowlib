
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
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Data;
using FlowLib.Utils.Compression.ZLibSubMethods;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Utils.Compression
{
    public class ZLib
    {
        DeflaterOutputStream zlibstream;
        MemoryStream memory = new MemoryStream();
        long pos = 0;

        public ZLib()
        {
            zlibstream = new DeflaterOutputStream(memory);
        }

        public byte[] close()
        {
            zlibstream.Finish();
            byte[] test = Read();
            zlibstream.Close();
            return test;
        }

        public static string ZBlobCompress(string uncompressed)
        {
            return Compress(uncompressed);
        }

        public static string ZBlobDecompres(string compressed)
        {
            return Decompress(compressed);
        }

        public void Compress2(byte[] bytData)
        {
            zlibstream.Write(bytData, 0, bytData.Length);
        }

        public byte[] Read()
        {
            memory.Position = pos;
            byte[] test = new byte[memory.Length - memory.Position];
            memory.Read(test, 0, test.Length);
            pos += test.Length;
            return test;
        }

        public static byte[] Compress(byte[] bytData)
        {
            MemoryStream ms = new MemoryStream();
            Stream s = new DeflaterOutputStream(ms);
            s.Write(bytData, 0, bytData.Length);
            s.Close();
            return (byte[])ms.ToArray();
        }

        // Compress Method - uses byte array compressedData which is public defined above
        private static string Compress(string uncompressed)
        {
            try
            {
                byte[] bytData = System.Text.Encoding.Default.GetBytes(uncompressed);
                string returndata = System.Text.Encoding.Default.GetString(Compress(bytData));
                return returndata;
            }
            catch
            {
                return null;
            }
        }

        public static byte[] Decompress(byte[] bytInput)
        {
            MemoryStream ms = new MemoryStream();
            int totalLength = 0;
            byte[] writeData = new byte[4096];
            Stream s2 = new InflaterInputStream(new MemoryStream(bytInput));
            try
            {
                while (true)
                {
                    int size = s2.Read(writeData, 0, writeData.Length);
                    if (size > 0)
                    {
                        ms.Write(writeData, 0, size);
                        totalLength += size;
                        //strResult += System.Text.Encoding.ASCII.GetString(writeData, 0, size);
                    }
                    else
                    {
                        break;
                    }
                }
                s2.Close();
                return (byte[])ms.ToArray();
            }
            catch
            {
                return null;
            }

        }
        private static string Decompress(string compressed)
        {
            byte[] bytInput = System.Text.Encoding.Default.GetBytes(compressed);
            bytInput = Decompress(bytInput);
            if (bytInput != null)
                return System.Text.Encoding.ASCII.GetString(bytInput);
            return string.Empty;
        }
    }
}
