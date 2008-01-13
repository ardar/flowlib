
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
using ConsoleDemo.ConsoleClient.Controls;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;


namespace ConsoleDemo.ConsoleClient
{
    class Program : Window
    {
        Window activeWindow = null;
        System.Collections.Generic.List<Window> windows = new System.Collections.Generic.List<Window>();
        bool shouldExit = false;

        Button exit = new Button(0, 0, "Exit");
        Button settings = new Button(5, 0, "Settings");
        //Button exit = new Button(14, 0, "Exit");



        static void Main(string[] args)
        {
            new Program();
        }

        public Program()
        {
            MakeGui();
            activeWindow = this;
            activeWindow.Show();
        }

        void MakeGui()
        {
            Rectangle menu = new Rectangle(0, 0, Console.WindowWidth, 1);
            menu.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(menu);

            exit.OnSelect += new EventHandler(exit_OnSelect);
            exit.BgColor = ConsoleColor.Blue;
            Controls.Add(exit);

            settings.OnSelect += new EventHandler(settings_OnSelect);
            settings.BgColor = ConsoleColor.Blue;
            Controls.Add(settings);
        }

        void settings_OnSelect(object sender, EventArgs e)
        {

        }

        void exit_OnSelect(object sender, EventArgs e)
        {
            shouldExit = true;
            activeWindow.Hide();
        }

        //public override void Hide()
        //{
        //    base.Hide();
        //    foreach (Control var in Controls)
        //    {
        //        var.Hide();
        //    }
        //}
    }
}
