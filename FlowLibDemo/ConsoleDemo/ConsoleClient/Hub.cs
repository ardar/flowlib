
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

using Con = FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Managers;
using FlowLib.Utils.FileLists;
using ConsoleDemo.ConsoleClient.Controls;


namespace ConsoleDemo.ConsoleClient
{
    public class Hub : Window, IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        HubSetting setting = new HubSetting();
        System.Timers.Timer updateTimer = new System.Timers.Timer();
        
        Con.Hub hub = null;
        string name = null;
        bool updating = false;
        int connectionStatus = -1;

        // Controls
        TextArea mainchat = new TextArea(0, 2, Console.WindowWidth - 21, Console.WindowHeight - 4);
        ListBox userlist = new ListBox(Console.WindowWidth - 20, 2, 20, Console.WindowHeight - 4);
        TextField input = new TextField(0, Console.WindowHeight - 1, Console.WindowWidth);

        public Hub()
        {
            updateTimer.Interval = 1 * 1000;
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);
            updateTimer.Start();

            setting.DisplayName = "FlowLibConsole";
            setting.Protocol = "Auto";
            setting.Port = -1;

            // Controls
            mainchat.BgColor = ConsoleColor.Gray;
            mainchat.FgColor = ConsoleColor.Black;
            Controls.Add(mainchat);

            userlist.BgColor = ConsoleColor.Gray;
            userlist.FgColor = ConsoleColor.Blue;
            Controls.Add(userlist);

            input.BgColor = ConsoleColor.White;
            input.FgColor = ConsoleColor.Black;
            Controls.Add(input);
        }

        void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!Hidden)
                Update();
        }

        void hub_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            connectionStatus = e.Action;
        }

        public override void Show()
        {
            int width = 20;
            int height = 5;
            int px = (Console.WindowWidth / 2) - width;
            int py = (Console.WindowHeight / 2) - height;

            Rectangle popup = new Rectangle(px, py, width * 2, height * 2, ConsoleColor.Gray, ConsoleColor.DarkBlue);
            popup.Show();

            Label popupTitle = new Label(px, py, "Connection Setting", width * 2);
            popupTitle.BgColor = ConsoleColor.Blue;
            popupTitle.FgColor = ConsoleColor.DarkBlue;
            popupTitle.Show();

            TextField address = new TextField(px + 1, py + 2, "IP/DNS:", 31);
            address.BgColor = ConsoleColor.DarkBlue;
            address.FgColor = ConsoleColor.Gray;
            address.Show();

            TextField port = new TextField(px + 1, py + 4, "Port:", 33);
            port.BgColor = ConsoleColor.DarkBlue;
            port.FgColor = ConsoleColor.Gray;
            port.Show();

            TextField nick = new TextField(px + 1, py + 6, "DisplayName:", 26);
            nick.BgColor = ConsoleColor.DarkBlue;
            nick.FgColor = ConsoleColor.Gray;
            nick.Show();

            Button buttonConnect = new Button(px + 1, py + 8, "Connect");
            buttonConnect.OnSelect += new EventHandler(buttonConnect_OnSelect);
            buttonConnect.BgColor = ConsoleColor.DarkBlue;
            buttonConnect.FgColor = ConsoleColor.Gray;
            buttonConnect.Show();

            // Select one at a time
            System.Net.IPAddress ip = null;
            do
            {
                address.Focus();
                if (!string.IsNullOrEmpty(address.Input))
                {
                    try
                    {
                        ip = System.Net.Dns.GetHostEntry(address.Input).AddressList[0];
                    }
                    catch (System.Exception)
                    {
                        // We are not going to try to catch this as developer that used this class made something wrong if this has to be thrown.
                        try
                        {
                            ip = System.Net.IPAddress.Parse(address.Input);
                        }
                        catch { }
                    }
                }
            } while (ip == null);
            int p = 0;
            // Port
            do
            {
                port.Focus();
                try
                {
                    p = int.Parse(port.Input);
                }
                catch { }
            } while (p <= 0 && p > 65535);


            do
            {
                nick.Focus();
            } while (string.IsNullOrEmpty(nick.Input));

            buttonConnect.Focus();

            base.Show();
        }

        void buttonConnect_OnSelect(object sender, EventArgs e)
        {
            // TODO : Do connect here
        }

        protected void Update()
        {

        }
    }
}
