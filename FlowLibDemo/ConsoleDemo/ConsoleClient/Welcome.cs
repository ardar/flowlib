
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

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;

namespace ConsoleDemo.ConsoleClient
{
    public class Welcome : Window
    {
        Setting setting = null;
        Window activeWindow = null;

        public Welcome(Setting setting)
        {
            this.setting = setting;
        }
        public override void Show()
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 1;
            Console.WriteLine("################ WELCOME TO FLOWLIB - CONSOLE EXAMPLE ################");
            Console.WriteLine(" Below is commands that are available.");
            Console.WriteLine(" Remember that you can always exit program using -exit.");
            Console.WriteLine(" -setting        -   Open settings window");
            Console.WriteLine(" -hub         -   Open hub window");
            Console.WriteLine(" -exit           -   closes application");
            Console.WriteLine(" -help           -   Open this window");
            Console.WriteLine(" esc button      -   clears input window");
            Console.WriteLine("");
            Console.WriteLine(" -welcome        -   Open this window");
            Console.WriteLine(" -help           -   Open this window");
            Console.WriteLine("################ WELCOME TO FLOWLIB - CONSOLE EXAMPLE ################");
            base.Show();
        }

        public override bool Command(ConsoleKeyInfo keyInfo, string cmd)
        {
            switch (cmd)
            {
                case "setting":
                    activeWindow.Hide();
                    activeWindow = setting;
                    activeWindow.Show();
                    return true;
                case "hub":
                    activeWindow.Hide();
                    activeWindow = new Hub();
                    activeWindow.Show();
                    return true;
                default:
                    activeWindow.Command(keyInfo, cmd);
                    break;
            }

            return false;
        }
    }
}
