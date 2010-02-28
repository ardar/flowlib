
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
using System.Text;
using ConsoleDemo.ConsoleClient.Controls;

namespace ConsoleDemo.ConsoleClient
{
    public class Setting : Window
    {
        protected string externalIp = "";

        // Controls
        Button buttonExit = new Button(0, 0, "Exit");
        Button buttonSetting = new Button(5, 0, "Setting");

        public Setting()
        {
            Rectangle menu = new Rectangle(0, 0, Console.WindowWidth, 1);
            menu.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(menu);

            buttonExit.OnSelect += new EventHandler(exit_OnSelect);
            buttonExit.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(buttonExit);

            buttonSetting.BgColor = ConsoleColor.Black;
            Controls.Add(buttonSetting);



        }

        void exit_OnSelect(object sender, EventArgs e)
        {
            this.Hide();
        }
    }
}
