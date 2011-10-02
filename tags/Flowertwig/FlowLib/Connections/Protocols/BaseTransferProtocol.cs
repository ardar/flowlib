
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

using Flowertwig.Utils.Entities;
using FlowLib.Connections.Interfaces;
using FlowLib.Interfaces;

namespace FlowLib.Connections.Protocols
{
    public abstract class BaseTransferProtocol
    {
        /// <summary>
        /// Connection is ready to be used.
        /// You can now send messages and so on.
        /// object is bool
        /// </summary>
        public const int IsReady = 24;

        protected ITransfer trans = null;   // Current transfer where this protocol is used

        protected virtual byte[] GetContent(System.Text.Encoding encoding, long start, long length)
        {
            IShare share = trans.Share;
            ContentInfo info = trans.Content;
            if (share != null && trans.CurrentSegment != null && share.ContainsContent(ref info))
            {
                trans.Content = info;
                if (length == -1 && start == 0)
                    trans.CurrentSegment.Length = length = trans.Content.Size;
                return share.GetContent(trans.Content, start, length);
            }
            return null;
        }
        protected void EnsureCurrentSegmentCancelation()
        {
            if (trans.DownloadItem != null && trans.CurrentSegment != null)
            {
                // Clean up here please :)
                trans.DownloadItem.Cancel(trans.CurrentSegment.Index, trans.Source);
                trans.CurrentSegment = null;
            }
        }

        protected void EnsureCurrentSegmentFinishing()
        {
            if (trans.DownloadItem != null && trans.CurrentSegment != null)
            {
                // Clean up here please :)
                //FlowLib.Utils.FileOperations.ForceClose(trans.DownloadItem.ContentInfo.Get(ContentInfo.STORAGEPATH));
                trans.DownloadItem.Finished(trans.CurrentSegment.Index, trans.Source);
                trans.CurrentSegment = null;
            }
        }
    }
}
