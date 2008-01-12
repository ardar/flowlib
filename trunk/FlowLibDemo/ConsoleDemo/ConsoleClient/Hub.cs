
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

namespace ConsoleDemo.ConsoleClient
{
    public class Hub : Window, IBaseUpdater
    {
        public event FmdcEventHandler UpdateBase;

        // TITLE
        int titleL = 21;
        int titleT = 1;
        // Status
        int statusL = 0;
        int statusT = -1;   // This will place it in bottom

        HubSetting setting = new HubSetting();
        System.Timers.Timer updateTimer = new System.Timers.Timer();
        
        Con.Hub hub = null;
        string name = null;
        bool updating = false;
        int connectionStatus = -1;

        /// <summary>
        /// 0 = Main messages
        /// 1 = Private Messages
        /// 2 = Passwords
        /// 3 = Address
        /// 4 = Port
        /// </summary>
        int messageType = -1;
        string message = "";

        public Hub()
        {
            updateTimer.Interval = 1 * 1000;
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);
            updateTimer.Start();

            setting.DisplayName = "FlowLibConsole";
            setting.Protocol = "Auto";
            setting.Port = -1;
        }

        void updateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (base.showed)
                Update();
        }

        void hub_ConnectionStatusChange(object sender, FmdcEventArgs e)
        {
            connectionStatus = e.Action;
        }

        public override void Input(ConsoleKeyInfo keyInfo)
        {
            switch (keyInfo.Key)
            {
                case ConsoleKey.Enter:
                    switch (messageType)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            setting.Address = message; break;
                        case 4:
                            try{
                                setting.Port = int.Parse(message);
                            }catch{}
                            if (setting.Port > 65535 || setting.Port < 1)
                                setting.Port = -1;
                            hub = new FlowLib.Connections.Hub(setting, this);
                            hub.ConnectionStatusChange -= hub_ConnectionStatusChange;
                            hub.ConnectionStatusChange += new FmdcEventHandler(hub_ConnectionStatusChange);
                            try
                            {
                                hub.Connect();
                            }
                            catch
                            {
                                setting.Address = "";
                            }
                            break;
                    }
                    UpdateField();
                    message = "";
                    return;
                default:
                    message += keyInfo.KeyChar;
                    Console.Write(keyInfo.KeyChar);
                    return;
                case ConsoleKey.Escape:
                    base.Input(keyInfo);
                    UpdateField();
                    return;
            }
        }

        public override bool Command(ConsoleKeyInfo keyInfo, string cmd)
        {
            switch (cmd)
            {
                default:
                    break;
            }

            return false;
        }


        public override void Show()
        {
            Console.CursorLeft = 0;
            Console.CursorTop = 1;
            Console.WriteLine("################ Hub ################");

            Update();
            UpdateField();
            base.Show();
        }

        protected void UpdateField()
        {
            string msg = "";
            // Set message text
            if (setting.Address.Length == 0)
            {
                messageType = 3;
                msg = "IP/DNS:" + msg;
            }
            else if (setting.Port == -1)
            {
                messageType = 4;
                msg = "Port:" + msg;
            }
            else if (connectionStatus == Con.TcpConnection.Connected)
            {
                messageType = 0;
                msg = "MainMessage:" + msg;
            }
            else
            {
            }

            Console.CursorLeft = 0;
            Console.CursorTop = 0;
            Console.Write(msg);
            // Clear input field
            string tmp = "";
            while (message.Length +1 >= tmp.Length)
                tmp += " ";
            Console.Write(tmp);

            defaultLeft = msg.Length;
            Console.CursorLeft = defaultLeft;
        }

        protected void Update()
        {
            if (updating)
                return;
            updating = true;

            int tmpL = 0;
            int tmpT = 0;

            int posL = Console.CursorLeft;
            int posT = Console.CursorTop;
            Console.CursorVisible = false;
            #region Title
            Console.CursorTop = titleT;
            Console.CursorLeft = titleL;
            #endregion
            #region Connection Status
            if (statusT == -1)
            {
                tmpT = Console.WindowHeight - 1;
            }
            else
            {
                tmpT = statusT;
            }
            Console.CursorTop = tmpT;
            Console.CursorLeft = statusL;
            switch (connectionStatus)
            {
                case -1:
                    Console.Write("# Need more info"); break;
                case Con.TcpConnection.Connected:
                    Console.Write("# Connected"); break;
                case Con.TcpConnection.Connecting:
                    Console.Write("# Connecting"); break;
                case Con.TcpConnection.Disconnected:
                    Console.Write("# Disconnected"); break;
                default:
                    break;
            }
            #endregion

            // restore pos
            Console.CursorVisible = true;
            Console.CursorLeft = posL;
            Console.CursorTop = posT;
            updating = false;
        }
    }
}
