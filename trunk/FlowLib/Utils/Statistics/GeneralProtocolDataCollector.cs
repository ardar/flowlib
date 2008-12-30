
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

namespace FlowLib.Utils.Statistics
{
    public class GeneralProtocolDataCollector : BaseProtocolDataCollector
    {
        protected long lastReceivedBytes = 0;
        protected long lastSendBytes = 0;
        protected long lastCalcReceivedSpeed = 0;
        protected long lastCalcSendSpeed = 0;

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
                // Should we set new min/max values?
                double value = tmpBytes / tmpTime.TotalSeconds;
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
                lastReceivedBytes = TotalBytesSent;
                // Get current time in ticks
                long now = DateTime.Now.Ticks;
                // Get how long time has pased since last time
                TimeSpan tmpTime = new TimeSpan(now - lastCalcSendSpeed);
                // Store current time until next time
                lastCalcSendSpeed = now;
                // Should we set new min/max values?
                double value = tmpBytes / tmpTime.TotalSeconds;
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
            TotalBytesReceived += msg.Bytes.Length;
        }

        protected override void HandleSend(IConMessage msg)
        {
            TotalBytesSent += msg.Bytes.Length;
        }
    }
}
