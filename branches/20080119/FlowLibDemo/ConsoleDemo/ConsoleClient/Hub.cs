
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

        HubSetting setting;
        System.Timers.Timer updateTimer = new System.Timers.Timer();
        
        Con.Hub hub = null;
        string name = null;
        bool updating = false;
        int connectionStatus = -1;
        System.Collections.Generic.List<string> userList = new System.Collections.Generic.List<string>();
        System.Collections.Generic.List<string> msgList = new System.Collections.Generic.List<string>();

        // Controls
        ListBox listMainchat;
        ListBox listUserlist;
        TextField input;

        // Buttons
        Button buttonExit = new Button(0, 0, "Exit");
        Button buttonSetting = new Button(5, 0, "Setting");
        Button buttonHub = new Button(14, 0, "Hub");

        public Hub()
        {
            listMainchat = new ListBox(0, 2, Console.WindowWidth - 21, Console.WindowHeight - 4, ref msgList);
            listUserlist = new ListBox(Console.WindowWidth - 20, 2, 20, Console.WindowHeight - 4, ref userList);
            input = new TextField(0, Console.WindowHeight - 1, Console.WindowWidth);

            updateTimer.Interval = 1 * 1000;
            updateTimer.Elapsed += new System.Timers.ElapsedEventHandler(updateTimer_Elapsed);
            updateTimer.Start();

            // Controls
            listMainchat.BgColor = ConsoleColor.Gray;
            listMainchat.FgColor = ConsoleColor.Black;
            Controls.Add(listMainchat);

            listUserlist.BgColor = ConsoleColor.Gray;
            listUserlist.FgColor = ConsoleColor.Blue;
            Controls.Add(listUserlist);

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
            if (setting == null)
            {
                setting = new HubSetting();
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
                setting.Address = address.Input;
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
                setting.Port = p;

                do
                {
                    nick.Focus();
                } while (string.IsNullOrEmpty(nick.Input));
                setting.DisplayName = nick.Input;

                buttonConnect.Focus();

                setting.Protocol = "Auto";
                setting.Port = -1;
            }


            base.Show();
        }

        void buttonConnect_OnSelect(object sender, EventArgs e)
        {
            hub = new FlowLib.Connections.Hub(setting, this);
            hub.ConnectionStatusChange += new FmdcEventHandler(hub_ConnectionStatusChange);
            hub.ProtocolChange += new FmdcEventHandler(hub_ProtocolChange);
            hub.Connect();
            // TODO : Do connect here
        }

        void hub_ProtocolChange(object sender, FmdcEventArgs e)
        {
            IProtocol prot = e.Data as IProtocol;
            if (prot != null)
            {
                prot.Update -= Protocol_Update;
                //prot.MessageReceived -= Protocol_MessageReceived;
                //prot.MessageToSend -= Protocol_MessageToSend;
            }
            hub.Protocol.Update += new FmdcEventHandler(Protocol_Update);
            //hub.Protocol.MessageReceived += new FmdcEventHandler(Protocol_MessageReceived);
            //hub.Protocol.MessageToSend += new FmdcEventHandler(Protocol_MessageToSend);
        }

        void Protocol_MessageToSend(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
            {
                msgList.Add(string.Format("OUT: {0}", msg.Raw));
                listMainchat.Show();
            }
        }

        void Protocol_MessageReceived(object sender, FmdcEventArgs e)
        {
            StrMessage msg = e.Data as StrMessage;
            if (msg != null)
            {
                msgList.Add(string.Format("IN: {0}", msg.Raw));
                listMainchat.Show();
            }
        }

        void Protocol_Update(object sender, FmdcEventArgs e)
        {
            UserInfo usr = null;
            switch (e.Action)
            {
                case Actions.MainMessage:
                    MainMessage msg = e.Data as MainMessage;
                    if (msg != null)
                    {
                        msgList.Add(string.Format("<{0}> {1}", msg.From, msg.Content));
                        listMainchat.Show();
                    }
                    break;
                case Actions.UserOnline:
                     usr = e.Data as UserInfo;
                    if (usr != null)
                    {
                        userList.Add(usr.DisplayName);
                        int pos = userList.IndexOf(usr.DisplayName);
                        if ((listUserlist.Height + listUserlist.TopRowIndex) >= pos && pos >= listUserlist.TopRowIndex)
                            listUserlist.Show();
                    }
                    break;

                case Actions.UserOffline:
                    usr = e.Data as UserInfo;
                    if (usr != null)
                    {
                        int pos = userList.IndexOf(usr.DisplayName);
                        if ((listUserlist.Height + listUserlist.TopRowIndex) >= pos && pos >= listUserlist.TopRowIndex)
                        {
                            listUserlist.Show();
                        }
                        userList.Remove(usr.DisplayName);
                    }
                    break;
                default:
                    break;
            }
        }

        protected void Update()
        {
            if (updating)
                return;
            updating = true;
            //listMainchat.Show();
            updating = false;
        }
    }
}
