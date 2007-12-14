
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

namespace FlowLib.Containers
{
    public class SegmentInfo
    {
        protected int vPosition = -1;
        protected long vStart = 0;
        protected long vLength = -1;
        /// <summary>
        /// Segment position
        /// </summary>
        public int Position
        {
            get { return vPosition; }
        }

        public long Start
        {
            get { return vStart; }
        }
        /// <summary>
        /// Segment data length
        /// </summary>
        public long Length
        {
            get { return vLength; }
        }

        /// <summary>
        /// Creating SegmentInfo with pos.
        /// This should only be used for error notification
        /// </summary>
        /// <param name="pos">Segment position</param>
        public SegmentInfo(int pos)
        {
            vPosition = pos;
        }
        /// <summary>
        /// Creating SegmentInfo with pos and length info for downloadinfo.
        /// </summary>
        /// <param name="pos">Segment position</param>
        /// <param name="length">Data lenght for this segment</param>
        public SegmentInfo(int pos, long length)
            : this(pos)
        {
            vLength = length;
        }
        /// <summary>
        /// Creating SegmentInfo with pos and length info for downloadinfo.
        /// </summary>
        /// <param name="pos">Segment position</param>
        /// <param name="start">Data pos where to start from</param>
        /// <param name="length">Data lenght for this segment</param>
        public SegmentInfo(int pos, long start, long length)
            : this(pos, length)
        {
            vStart = start;
        }
    }
}
