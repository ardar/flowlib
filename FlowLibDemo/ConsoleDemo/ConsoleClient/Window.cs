
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

namespace ConsoleDemo.ConsoleClient
{
    public class Window
    {
        protected bool showed = false;
        protected int defaultTop = 0;
        protected int defaultLeft = 0;

        public bool IsShowing
        {
            get { return showed; }
            set { showed = value; }
        }

        public virtual void Input(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Escape:
                    int curLef = Console.CursorLeft;
                    Console.CursorLeft = defaultLeft;
                    while (Console.CursorLeft < curLef)
                        Console.Write(" ");
                    Console.CursorLeft = defaultLeft;
                    break;
                default:
                   Console.Write(keyInfo.KeyChar);
                    break;
            }
        }
        public virtual bool Command(ConsoleKeyInfo keyInfo, string cmd) { return false; }
        public virtual void Show() { showed = true; Console.CursorLeft = defaultLeft; Console.CursorTop = defaultTop; }
        public virtual void Hide() { showed = false; Console.Clear(); }
    }
}
