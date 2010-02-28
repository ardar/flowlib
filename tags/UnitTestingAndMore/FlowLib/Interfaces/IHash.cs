
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

using FlowLib.Containers;

namespace FlowLib.Interfaces
{

    // TODO : Add this for enabling many hashing algorithms in Share.
    public interface IHash
    {
        /// <summary>
        /// Generate Hash Id and Hash Validation data
        /// </summary>
        /// <param name="info">ContentInfo to generate has for</param>
        void Generate(ContentInfo info);
        /// <summary>
        /// Validate Hash Validation data against hash id.
        /// Validation data will be added if match.
        /// </summary>
        /// <param name="info">ContentInfo that contains Hash id</param>
        /// <param name="str">Hash validation data. This will be added to ContentInfo if verify succeeds.</param>
        /// <returns>If validation data and id matches it will return true, else false.</returns>
        bool VerifyData(ref ContentInfo info, string str);
        /// <summary>
        /// Verify downloaded segment against hash
        /// </summary>
        /// <param name="info">ContentInfo telling us where to find content</param>
        /// <param name="seg">SegmentInfo telling us what part of file we want to verify</param>
        /// <returns>returns true if byte[] in segment interval matches validation data</returns>
        bool verifySegment(ref ContentInfo info, SegmentInfo seg);
    }
}
