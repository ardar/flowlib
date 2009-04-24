
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
using FlowLib.Interfaces;

namespace FlowLib.Protocols
{
    public abstract class BaseTransferProtocol
    {
        protected ITransfer trans = null;   // Current transfer where this protocol is used

        protected virtual byte[] GetContent(System.Text.Encoding encoding, long start, long length)
        {
            Share share = trans.Share;
            Containers.ContentInfo info = trans.Content;
            if (share != null && share.ContainsContent(ref info))
            {
                trans.Content = info;
                if (length == -1 && start == 0)
                    trans.CurrentSegment.Length = length = trans.Content.Size;
                return share.GetContent(trans.Content, start, length);
            }
            return null;
        }

    }
}
