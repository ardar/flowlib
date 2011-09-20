
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

using FlowLib.Interfaces;
using FlowLib.Utils.Hash;
using FlowLib.Containers;
using FlowLib.Utils.Convert;
using System.Threading;

namespace FlowLib.Hashing
{
    public class HashTth : IHash
    {
        protected ThreadPriority priority = ThreadPriority.Lowest;
        protected int count = 4;

        public ThreadPriority Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public int ThreadCount
        {
            get { return count; }
            set { count = value; }
        }

        public void Generate(FlowLib.Containers.ContentInfo info)
        {
            if (info.ContainsKey(ContentInfo.STORAGEPATH))
            {
                TthThreaded tth = new TthThreaded(info.Get(ContentInfo.STORAGEPATH));
                tth.ThreadPriority = priority;
                tth.ThreadCount = count;
                byte[] bytes = tth.GetTTH_Value();
                info.Set(ContentInfo.TTH, Base32.Encode(bytes));
                //byte[][][] tree = tth.TTH;
                //if (tree != null && tree.Length > 0)
                //{
                //    int startIndex = 0;
                //    //if (tree.Length >= 8)
                //    //    startIndex = 7;
                //    long length = 0;
                //    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                //    for (int i = 0; i < tree[startIndex].Length; i++)
                //    {
                //        // Ignore the rest if more then 65KiB has been writed
                //        //if (length > 66560)
                //        //    break;
                //        length += tree[startIndex][i].Length;
                //        ms.Write(tree[startIndex][i], 0, tree[startIndex][i].Length);
                //    }

                //    info.Set(ContentInfo.TTHL, Base32.Encode(ms.ToArray()));
                //}
            }
        }

        public bool VerifyData(ref FlowLib.Containers.ContentInfo info, string str)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }

        public bool verifySegment(ref FlowLib.Containers.ContentInfo info, FlowLib.Containers.SegmentInfo seg)
        {
            throw new System.Exception("The method or operation is not implemented.");
        }
    }
}
