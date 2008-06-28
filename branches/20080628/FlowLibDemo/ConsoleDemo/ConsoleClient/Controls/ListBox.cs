
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
using System.Collections.Generic;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;

namespace ConsoleDemo.ConsoleClient.Controls
{
    public class ListBox :  TextArea
    {
        protected int index = 0;

        public int SelectedIndex
        {
            get { return index; }
        }

        public ListBox(int x, int y, int width, int height)
            : base(x, y, width, height)
        {

        }
        public ListBox(int x, int y, int width, int height, ref List<string> list)
            : base(x, y, width, height)
        {
            base.text = list;
        }

        public override int Focus()
        {
            index = Console.CursorTop - Y;
            return base.Focus();
        }
    }
}
