
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
using System.Collections.Generic;
using System.Text;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;

namespace ConsoleDemo.ConsoleClient.Controls
{
    public class Button : Label, IFocusable
    {
        public delegate void Action();
        public event EventHandler OnSelect;

        public Button(int x, int y, string text)
            : base(x, y, text)
        {
            OnSelect = new EventHandler(Button_OnSelect);
        }

        public int Focus()
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.CursorLeft = X;
            Console.CursorTop = Y;
            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;

            ConsoleKeyInfo key;
            int value = -1;
            do
            {
                key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Tab:
                        value = 1;
                        break;
                    case ConsoleKey.Enter:
                        OnSelect(this, new EventArgs());
                        value = 0;
                        break;
                }
            } while (value == -1);
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            return value;
        }

        void Button_OnSelect(object sender, EventArgs e) { }
    }
}
