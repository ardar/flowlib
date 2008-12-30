﻿
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using FlowLib.Interfaces;
using FlowLib.Containers;

namespace FlowLib.Utils.Statistics
{
    /// <summary>
    /// Only counts Protocol commands. It will exclude any file/data transfers.
    /// </summary>
    public class ProtocolMessagesDataCollector : GeneralProtocolDataCollector
    {
        public ProtocolMessagesDataCollector(IConnection con) : base(con) { }
        public ProtocolMessagesDataCollector(IProtocol prot) : base(prot) { }

        protected override void HandleReceived(IConMessage msg)
        {
            if (!(msg is BinaryMessage))
                TotalBytesReceived += msg.Bytes.Length;
        }

        protected override void HandleSend(IConMessage msg)
        {
            if (!(msg is BinaryMessage))
                TotalBytesSent += msg.Bytes.Length;
        }
    }
}