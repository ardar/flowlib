
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
    public class Rectangle : Control
    {
        protected int posX, posY;
        protected int width, height;
        protected ConsoleColor bgcolor = Console.BackgroundColor;
        protected ConsoleColor fgcolor = Console.ForegroundColor;

        public int X
        {
            get { return posX; }
        }
        public int Y
        {
            get { return posY; }
        }
        public int Width
        {
            get { return width; }
        }
        public int Height
        {
            get { return height; }
        }
        public ConsoleColor FgColor
        {
            get { return fgcolor; }
            set { fgcolor = value; }
        }
        public ConsoleColor BgColor
        {
            get { return bgcolor; }
            set { bgcolor = value; }
        }

        public Rectangle(int x, int y, int width, int height)
            : this(x, y, width, height, Console.ForegroundColor, Console.BackgroundColor) { }
        public Rectangle(int x, int y, int width, int height, ConsoleColor fg, ConsoleColor bg)
        {
            posX = x;
            posY = y;
            this.width = width;
            this.height = height;
            bgcolor = bg;
            fgcolor = fg;
        }

        public override void Show()
        {
            Draw(this.fgcolor, this.bgcolor);
            base.Show();
        }

        public override void Hide()
        {
            Clear();
        }

        public virtual void Clear()
        {
            Draw(Console.ForegroundColor, Console.BackgroundColor);
            base.Show();
        }

        protected virtual void Draw(ConsoleColor fgcolor, ConsoleColor bgcolor)
        {
            int cX = Console.CursorLeft;
            int cY = Console.CursorTop;
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            Console.ForegroundColor = fgcolor;
            Console.BackgroundColor = bgcolor;
            Console.CursorVisible = false;

            for (int x = posX; x < posX + width; x++)
            {
                for (int y = posY; y < posY + height; y++)
                {
                    Console.CursorLeft = x;
                    Console.CursorTop = y;
                    Console.Write(" ");
                }
            }
            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
            Console.CursorLeft = cX;
            Console.CursorTop = cY;
            Console.CursorVisible = true;
        }

    }
}
