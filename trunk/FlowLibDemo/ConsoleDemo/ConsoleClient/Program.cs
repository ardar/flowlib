
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
using System.Text;
using ConsoleDemo.ConsoleClient.Controls;
using ConsoleDemo.ConsoleClient.Controls.Interfaces;


namespace ConsoleDemo.ConsoleClient
{
    class Program : Window
    {
        Window activeWindow = null;
        //System.Collections.Generic.List<Window> windows = new System.Collections.Generic.List<Window>();

        // Controls
        Button buttonExit = new Button(0, 0, "Exit");
        Button buttonSetting = new Button(5, 0, "Setting");
        Button buttonHub = new Button(14, 0, "Hub");

        TextArea txtWelcome = new TextArea(1, 2, Console.WindowWidth - 1, Console.WindowHeight - 2);

        // Windows
        Setting windowSetting = new Setting();
        Hub windowHub = new Hub();

        static void Main(string[] args)
        {
            ConsoleColor fg = Console.ForegroundColor;
            ConsoleColor bg = Console.BackgroundColor;

            new Program();

            Console.ForegroundColor = fg;
            Console.BackgroundColor = bg;
        }

        public Program()
        {
            Console.Title = "Console Demo of FlowLib";
            MakeGui();
            this.Show();
        }

        void MakeGui()
        {
            Rectangle menu = new Rectangle(0, 0, Console.WindowWidth, 1);
            menu.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(menu);

            buttonExit.OnSelect += new EventHandler(exit_OnSelect);
            buttonExit.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(buttonExit);

            buttonSetting.OnSelect += new EventHandler(settings_OnSelect);
            buttonSetting.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(buttonSetting);

            buttonHub.OnSelect += new EventHandler(buttonHub_OnSelect);
            buttonHub.BgColor = ConsoleColor.DarkBlue;
            Controls.Add(buttonHub);

            Console.WriteLine("WindowWith" + Console.WindowWidth);
            Console.WriteLine("WindowHeight" + Console.WindowHeight);

            Console.WriteLine("BufferWith" + Console.BufferWidth);
            Console.WriteLine("BufferHeight" + Console.BufferHeight);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Welcome to this example on how to use FlowLib.");
            sb.AppendLine("");
            sb.AppendLine("If you think this exampe is to advanced.");
            sb.AppendLine("Please look in the examples directory");
            sb.AppendLine("where single functionality is demonstrated.");
            txtWelcome.Text = sb.ToString();
            Controls.Add(txtWelcome);
        }

        void buttonHub_OnSelect(object sender, EventArgs e)
        {
            if (activeWindow != null)
                activeWindow.Hide();
            activeWindow = windowHub;
            activeWindow.Show();
        }

        void settings_OnSelect(object sender, EventArgs e)
        {
            if (activeWindow != null)
                activeWindow.Hide();
            activeWindow = windowSetting;
            activeWindow.Show();
        }

        void exit_OnSelect(object sender, EventArgs e)
        {
            this.Hide();
            Console.Clear();
        }
    }
}
