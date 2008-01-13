
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
using ConsoleDemo.ConsoleClient.Controls.Interfaces;

namespace ConsoleDemo.ConsoleClient.Controls
{
    public class TextField : Label, IFocusable
    {
        string input = "";
        int inputMax = 0;

        public string Input
        {
            get { return input; }
        }

        public TextField(int x, int y, int inputSize)
            : this(x, y, string.Empty, inputSize)
        { }
        public TextField(int x, int y, string label, int inputSize)
            : base(x, y, label)
        {
            width += inputSize;
            inputMax = inputSize;
        }

        public int Focus()
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.CursorLeft = X + Text.Length;
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
                    case ConsoleKey.Escape:
                        input = "";
                        Show();
                        Console.CursorLeft = X + Text.Length;
                        break;
                    case ConsoleKey.Tab:
                        value = 1;
                        break;
                    case ConsoleKey.Enter:
                        value = 0;
                        break;
                    case ConsoleKey.Backspace:
                        if (Console.CursorLeft > (X + Text.Length))
                        {
                            Console.CursorVisible = false;
                            input = input.Remove(Console.CursorLeft - (X + Text.Length +1), 1);
                            Console.CursorLeft--;
                            Console.Write(" ");
                            Console.CursorLeft--;
                            Console.CursorVisible = true;
                        }
                        break;
                    default:
                        if (inputMax > input.Length)
                        {
                            input += key.KeyChar;
                            Console.Write(key.KeyChar);
                        }
                        break;
                }
            } while (value == -1);
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            return value;
        }
    }
}
