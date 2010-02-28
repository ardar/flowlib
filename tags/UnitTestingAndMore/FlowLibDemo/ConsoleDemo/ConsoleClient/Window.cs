
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
using ConsoleDemo.ConsoleClient.Controls;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;

namespace ConsoleDemo.ConsoleClient
{
    public class Window : Control
    {
        protected int controlIndex = 0;
        protected List<Control> controls = new List<Control>();

        public List<Control> Controls
        {
            get { return controls; }
        }

        public override void Show()
        {
            base.Show();
            Console.Clear();

            foreach (Control var in Controls)
            {
                var.Show();
            }
            bool haveFocusControl = false;
            do
            {
                foreach (Control var in Controls)
                {
                    if (var is IFocusable && !var.Hidden)
                    {
                        haveFocusControl = true;
                        ((IFocusable)var).Focus();
                    }
                    if (Hidden)
                        break;

                }
            } while (haveFocusControl && !Hidden);
        }
        public override void Hide()
        {
            base.Hide();
            foreach (Control var in Controls)
            {
                var.Hide();
            }
        }
    }
}
