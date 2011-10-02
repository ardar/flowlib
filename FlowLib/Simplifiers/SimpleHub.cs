
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, flowlib at flowertwig dot org
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

using System.Globalization;
using Flowertwig.Utils;
using Flowertwig.Utils.Events;
using FlowLib.Connections;
using FlowLib.Entities;
using FlowLib.Filelists;
using FlowLib.Interfaces;

namespace FlowLib.Simplifiers
{
    public class SimpleHub : IBaseUpdater
    {
        public event EventHandler UpdateBase;
        protected Hub _hubConnection = null;
        protected Share _share = null;
        protected string _clientName = FlowLibInfo.GetName();
        protected double _clientVersion = FlowLibInfo.GetRunningVersionNumber();
        protected CultureInfo _cultureInfo = CultureInfo.GetCultureInfo("en-GB");

        public string ClientName
        {
            get { return _clientName; }
            set 
            {
                _clientName = value;
                UpdateClientInfo();
            }

        }

        public double ClientVersion
        {
            get { return _clientVersion; }
            set
            {
                _clientVersion = value;
                UpdateClientInfo();
            }
        }

        protected void UpdateClientInfo()
        {
            if (string.IsNullOrEmpty(ClientName))
            {
                throw new System.ArgumentException("ClientName is invalid.");
            }
            else if(ClientVersion <= 0)
            {
                throw new System.ArgumentException("ClientVersion is invalid.");
            }

            if (_hubConnection != null && _hubConnection.Me != null && _hubConnection.Me.TagInfo != null)
            {
                _hubConnection.Me.TagInfo.Version = string.Format("{0} v:{1}", ClientName, ClientVersion.ToString(_cultureInfo.NumberFormat));
            }
        }

        public Share Share {
            get { return _share; }
            set 
            {
                _share = value;
                AddFilelistsToShare(value);
                if (_hubConnection != null)
                {
                    _hubConnection.Share = value;
                }
            }
        }

        public string BaseDirectory { get; set; }
        public UserInfo ClientInfo { get { return (_hubConnection != null) ? _hubConnection.Me : null; } }

        public SimpleHub(HubSetting settings)
        {
            UpdateBase = new EventHandler(OnUpdateBase);
            Share = new Share("simpleHubShare");
            BaseDirectory = System.AppDomain.CurrentDomain.BaseDirectory;

            _hubConnection = new Hub(settings, this);

            _hubConnection.Me.TagInfo.Slots = 2;

            // Set common values
            _hubConnection.Me.Set(UserInfo.PID, settings.Get(UserInfo.PID));
            _hubConnection.Me.Set(UserInfo.IP, settings.Get(UserInfo.IP));
            _hubConnection.Me.Set(UserInfo.SECURE, settings.Get(UserInfo.SECURE));
            _hubConnection.Me.Set(UserInfo.UDPPORT, settings.Get(UserInfo.UDPPORT));

            // Adds share to hub
            _hubConnection.Share = Share;


            _hubConnection.ConnectionStatusChange += OnConnectionStatusChange;
            _hubConnection.ProtocolChange += OnProtocolChange;
        }

        protected void OnProtocolChange(object sender, DefaultEventArgs e)
        {

        }

        protected void OnConnectionStatusChange(object sender, DefaultEventArgs e)
        {

        }

        protected void AddFilelistsToShare(Share s)
        {
            // This will add common filelists to share and save them in directory specified.
            General.AddCommonFilelistsToShare(s, BaseDirectory + "MyFileLists" + System.IO.Path.DirectorySeparatorChar);
        }

        public void Connect()
        {
            AddFilelistsToShare(Share);
            _hubConnection.Connect();
        }

        protected bool IsValidMessage(string message)
        {
            return !string.IsNullOrEmpty(message);
        }

        /// <summary>
        /// Send main message (Visible to all).
        /// </summary>
        /// <param name="message">message to send</param>
        public void SendMessage(string message)
        {
            if (!IsValidMessage(message))
            {
                // TODO: Add some functionality for when it was not possible to send message.
            }

            if (_hubConnection != null)
            {
                UpdateBase(this, new DefaultEventArgs(Actions.MainMessage, new MainMessage(_hubConnection.Me.ID, message)));
            }
            else
            {
                // TODO: Add some functionality for when it was not possible to send message.
            }
        }
        /// <summary>
        /// Send private message
        /// </summary>
        /// <param name="userId">UserId of the user you want to send a message to</param>
        /// <param name="message">message to send</param>
        public void SendPrivateMessage(string userId, string message)
        {
            if (!IsValidMessage(message))
            {
                // TODO: Add some functionality for when it was not possible to send message.
            }

            if (_hubConnection != null && _hubConnection.GetUserById(_hubConnection.Me.ID) != null)
            {
                UpdateBase(this,
                           new DefaultEventArgs(Actions.PrivateMessage,
                                                            new PrivateMessage(userId, _hubConnection.Me.ID, message)));
            }
            else
            {
                // TODO: Add some functionality for when it was not possible to send message.
            }
        }
        
        public User GetUserById(string id)
        {
            if (_hubConnection != null)
            {
                return _hubConnection.GetUserById(id);
            }
            return null;
        }

        public User GetUserByPermenantId(string storeId)
        {
            if (_hubConnection != null)
            {
                return _hubConnection.GetUserByStoredId(storeId);
            }
            return null;
        }

        public User GetUserByNick(string nick)
        {
            if (_hubConnection != null)
            {
                return _hubConnection.GetUserByNick(nick);
            }
            return null;
        }

        protected void OnUpdateBase(object sender, DefaultEventArgs e)
        {
            throw new System.NotImplementedException();
        }

    }
}























