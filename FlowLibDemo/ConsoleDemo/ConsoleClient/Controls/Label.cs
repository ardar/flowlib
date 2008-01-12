
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

namespace ConsoleDemo.ConsoleClient.Controls
{
    public class Label : Rectangle
    {
        protected string text = string.Empty;

        public string Text
        {
            get { return text; }
            set 
            {
                text = value;
                if (!hidden)
                    Show();
            }
        }

        public Label(int x, int y, string str)
            : base(x,y, -1, 1)
        {
            text = str;
            if (text.Contains("\n"))
            {
                // Calc height
                string[] rows = text.Split('\n');
                height = rows.Length;
                // Calc Width
                for (int i = 0; i < rows.Length; i++)
                {
                    if (rows[i].Length > width)
                        width = rows[i].Length;
                }
            }
            else
            {
                width = text.Length;
            }
        }

        public override void Show()
        {
            base.Show();
            int cX = Console.CursorLeft;
            int cY = Console.CursorTop;
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;

            Console.CursorLeft = posX;
            Console.CursorTop = posY;
            Console.CursorVisible = false;

            Console.Write(text);

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.CursorLeft = cX;
            Console.CursorTop = cY;
            Console.CursorVisible = true;
        }
    }
}
