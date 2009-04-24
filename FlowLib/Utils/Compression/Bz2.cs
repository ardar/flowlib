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

// used for the stream
using System.IO;
using FlowLib.Utils.Compression.Bz2SubMethods;

namespace FlowLib.Utils.Compression
{
    public class Bz2
    {
        public Bz2()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public string UnpackFile(string strFileName)
        {
            string strReturn;
            string strOutPutFileName;

            // Check for filename length
            if (strFileName.Length > 4)
            {
                // Check for .bz2 file
                if (strFileName.Substring(strFileName.Length - 4) == ".bz2")
                {
                    // get output filename
                    strOutPutFileName = strFileName.Substring(0, strFileName.Length - 4);

                    try
                    {
                        // load input file into stream
                        // start by getting filesize
                        FileInfo fiInput = new FileInfo(strFileName);
                        Int32 intFileSize = System.Convert.ToInt32(fiInput.Length);
                        fiInput = null;

                        // define an array to use as buffer
                        char[] bufInput = new char[intFileSize];

                        // create binary reader from the input file
                        BinaryReader srInput = new BinaryReader(new FileStream(strFileName, FileMode.Open, FileAccess.Read));

                        // get the underlying stream from the reader to use for decompression
                        Stream streamIn = srInput.BaseStream;

                        // check if output file exists
                        if (File.Exists(strOutPutFileName))
                        {
                            // delete the pre existing file
                            File.Delete(strOutPutFileName);
                        }

                        // create binary writer for output file
                        BinaryWriter srOutput = new BinaryWriter(new FileStream(strOutPutFileName, FileMode.CreateNew));

                        // get the underlying stream to use for decompression
                        Stream streamOut = srOutput.BaseStream;

                        // seand input and output streams to SharpZipLib Decompress code
                        Decompress(streamIn, streamOut);

                        // dispose of streams and such
                        bufInput = null;
                        streamIn.Dispose();
                        srInput = null;
                        streamOut.Dispose();
                        srOutput = null;

                        strReturn = "Uncompress successful";
                    }
                    catch (Exception ex)
                    {
                        strReturn = ex.Message;
                    }

                }
                else
                {
                    strReturn = "Not Bz2 format";
                }
            }
            else
            {
                strReturn = "Filename too short";
            }

            return strReturn;
        }

        /// <summary>
        /// Decompress <paramref name="instream">input</paramref> writing 
        /// decompressed data to <paramref name="outstream">output stream</paramref>
        /// </summary>
        public static void Decompress(Stream instream, Stream outstream)
        {
            System.IO.Stream bos = outstream;
            System.IO.Stream bis = instream;
            BZip2InputStream bzis = new BZip2InputStream(bis);
            int ch = bzis.ReadByte();
            while (ch != -1)
            {
                bos.WriteByte((byte)ch);
                ch = bzis.ReadByte();
            }
            bos.Flush();
        }

        /// <summary>
        /// Compress <paramref name="instream">input stream</paramref> sending 
        /// result to <paramref name="outputstream">output stream</paramref>
        /// </summary>
        public static long Compress(Stream instream, Stream outstream, int blockSize)
        {
            System.IO.Stream bos = outstream;
            System.IO.Stream bis = instream;
            int ch;
            BZip2OutputStream bzos = new BZip2OutputStream(bos, blockSize);
            byte[] buffer = new byte[512];
            while ((ch = bis.Read(buffer, 0,buffer.Length)) != 0)
            {
                bzos.Write(buffer, 0, ch);
            }
            bis.Close();
            bzos.Flush();
            bzos.Close();
            return bzos.Size;
        }
    }
}
