# Description #

Tries to determine what connection mode is possible for user (Passive/Active)

# Code #

```

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
using FlowLib.Utils.Connection;

namespace ConsoleDemo.Examples
{
    public class WhatConnectionIsPossible
    {
        public WhatConnectionIsPossible()
        {
            Detect detect = new Detect();
            // Add listener for the ProgressChanged event. This is so we can give user feedback on what is being done right now.
            detect.ProgressChanged += new Detect.ProgressChange(detect_ProgressChanged);
            // Tells detection in what mode it should run. If you dont know what this is you should probably use NoThread.
            // NoThread =           Works in current thread.
            // ThreadInBackground = Works in a thread where IsBackground is set to true.
            // ThreadInForeground = Works in a thread where IsBackground is set to false.
            detect.Start(Detect.WorkMethod.ThreadInBackground);
        }

        void detect_ProgressChanged(Detect sender, Detect.Functions prog)
        {
            switch (prog)
            {
                case Detect.Functions.Start:
                    Console.WriteLine("Connection detection has started");
                    break;
                case Detect.Functions.End:
                    Console.WriteLine("Finished");
                    Console.Write("Connection Mode:");
                    switch (sender.ConnectionType)
	                {
                        case 0:     // No Internet Access?
                            Console.WriteLine("It seems like you have no Internet connection.");
                            Console.WriteLine("External IP:" + sender.ExternalIP);
                            break;
                        case 1:     // Passive mode
                            Console.WriteLine("It seems like you can only be passive.");
                            Console.WriteLine("External IP:" + sender.ExternalIP);
                            break;
                        case 2:     // Active mode
                            Console.WriteLine("It seems like you can be active. Congratulations!");
                            Console.WriteLine("External IP:" + sender.ExternalIP);
                            break;
                        case 4:     // Active mode through UPnP
                            Console.WriteLine("It seems like you can be active (Through UPnP). Congratulations!");
                            if (sender.ExternalIP != sender.ExternalIPUPnP)
                            {
                                Console.WriteLine("External IP:" + sender.ExternalIP);
                                Console.WriteLine("External IP (According to your IGD):" + sender.ExternalIPUPnP);
                            }
                            else
                            {
                                Console.WriteLine("External IP:" + sender.ExternalIP);
                            }
                            Console.WriteLine("Internal IP:" + sender.InternalIP);
                            break;
	                }
                    Console.WriteLine("Port:" + sender.Port);
                    break;
                default:
                    Console.WriteLine("Working on:" + prog);
                    break;
            }
        }
    }
}

```