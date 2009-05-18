
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
using FlowLib.Interfaces;
using FlowLib.Containers;

namespace FlowLib.Utils.Statistics
{
    public class GeneralProtocolDataCollector : BaseProtocolDataCollector
    {
        protected long lastReceivedBytes = 0;
        protected long lastSendBytes = 0;
        protected long lastCalcReceivedSpeed = DateTime.Now.Ticks;
        protected long lastCalcSendSpeed = DateTime.Now.Ticks;

        public long TotalBytesSent
        {
            get;
            protected set;
        }

        public long TotalBytesReceived
        {
            get;
            protected set;
        }

        public double MinimumReceiveSpeed
        {
            get;
            protected set;
        }

        public double MaximumReceiveSpeed
        {
            get;
            protected set;
        }

        public double CurrentReceiveSpeed
        {
            get
            {
                // Get how many bytes has been transfered since last time
                long tmpBytes = TotalBytesReceived - lastReceivedBytes;
                // Store current total bytes until next time
                lastReceivedBytes = TotalBytesReceived;
                // Get current time in ticks
                long now = DateTime.Now.Ticks;
                // Get how long time has pased since last time
                TimeSpan tmpTime = new TimeSpan(now - lastCalcReceivedSpeed);
                // Store current time until next time
                lastCalcReceivedSpeed = now;
                // Insanity check
                if (tmpBytes <= 0)
                    return 0;   // No new data
                double value = tmpBytes / tmpTime.TotalSeconds;
                if (double.IsInfinity(value))
                {
                    return -1;
                }
                // Should we set new min/max values?
                if (value > MaximumReceiveSpeed)
                    MaximumReceiveSpeed = value;
                if (MinimumReceiveSpeed == 0 || value < MinimumReceiveSpeed)
                    MinimumReceiveSpeed = value;
                // return speed in bytes/seconds
                return value;
            }
        }

        public double MinimumSendSpeed
        {
            get;
            protected set;
        }

        public double MaximumSendSpeed
        {
            get;
            protected set;
        }

        public double CurrentSendSpeed
        {
            get
            {
                // Get how many bytes has been transfered since last time
                long tmpBytes = TotalBytesSent - lastSendBytes;
                // Store current total bytes until next time
                lastSendBytes = TotalBytesSent;
                // Get current time in ticks
                long now = DateTime.Now.Ticks;
                // Get how long time has pased since last time
                TimeSpan tmpTime = new TimeSpan(now - lastCalcSendSpeed);
                // Store current time until next time
                lastCalcSendSpeed = now;
                // Insanity check
                if (tmpBytes <= 0)
                    return 0;   // No new data
                double value = tmpBytes / tmpTime.TotalSeconds;
                if (double.IsInfinity(value))
                {
                    return -1;
                }
                // Should we set new min/max values?
                if (value > MaximumSendSpeed)
                    MaximumSendSpeed = value;
                if (MinimumSendSpeed == 0 || value < MinimumSendSpeed)
                    MinimumSendSpeed = value;
                // return speed in bytes/seconds
                return value;
            }
        }

        public GeneralProtocolDataCollector(IProtocol prot)
            : base(prot) { }
        public GeneralProtocolDataCollector(IConnection con)
            : base(con) { }


        protected override void HandleReceived(IConMessage msg)
        {
            if (msg is BinaryMessage)
            {
                TotalBytesReceived += ((BinaryMessage)msg).Length;
            }
            else
            {
                TotalBytesReceived += msg.Bytes.Length;
            }
        }

        protected override void HandleSend(IConMessage msg)
        {
            if (msg is BinaryMessage)
            {
                TotalBytesSent += ((BinaryMessage)msg).Length;
            }
            else
            {
                TotalBytesSent += msg.Bytes.Length;
            }
        }
    }
}
