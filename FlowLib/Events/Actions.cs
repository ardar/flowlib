
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

namespace FlowLib.Events
{
    public class Actions
    {
        /// <summary>
        /// We needs to supply a password and we have none saved.
        /// </summary>
        public const int Password = 0;
        /// <summary>
        /// MainChat Message.
        /// </summary>
        public const int MainMessage = 1;
        /// <summary>
        /// Private Message.
        /// </summary>
        public const int PrivateMessage = 2;
        /// <summary>
        /// One User has come online.
        /// object should be of type Containers.UserInfo
        /// </summary>
        public const int UserOnline = 3;
        /// <summary>
        /// [Depricated]
        /// One or Many Users has come online.
        /// object is of type string[] where entry is a User Id.
        /// </summary>
        public const int UsersOnline = 4;
        /// <summary>
        /// One user has gone offline.
        /// object is of type UserInfo
        /// </summary>
        public const int UserOffline = 5;
        /// <summary>
        /// Info about the user has been updated.
        /// object is of type UserInfo
        /// </summary>
        public const int UserInfoChange = 6;
        /// <summary>
        /// [Depricated]
        /// List containing one or more Operators.
        /// object is of type string[] where entry is a User Id
        /// </summary>
        public const int OpUsers = 7;
        /// <summary>
        /// Indicates client status against the hub. (Connecting, Connected, Disconnected)
        /// object is of type HubDelegates.HubStatus
        /// </summary>
        public const int StatusChange = 8;
        /// <summary>
        /// Indicates what Reg Mode we have in hub.
        /// object is of type int and can have:
        /// -1 = We havnt been logged on.
        ///  0 = Normal User.
        ///  1 = Regged User.
        ///  2 = Operator.
        /// </summary>
        public const int RegMode = 9;
        /// <summary>
        /// Tells Gui that it should reload itself (Language, colors and so on)
        /// object is null
        /// </summary>
        public const int Reload = 10;
        /// <summary>
        /// Incomming command from hub.
        /// </summary>
        public const int CommandIncomming = 11;
        /// <summary>
        /// Outgoing command to hub.
        /// </summary>
        public const int CommandOutgoing = 12;
        /// <summary>
        /// Changed what protocol that should be used.
        /// object implements IProtocol
        /// </summary>
        public const int ProtocolChanged = 13;
        /// <summary>
        /// Connection/Hub has got a new name.
        /// </summary>
        public const int Name = 14;
        /// <summary>
        /// New Transfer has been created.
        /// </summary>
        public const int TransferStarted = 15;
        /// <summary>
        /// TransferRequest. This is used when others should connect to us.
        /// </summary>
        public const int TransferRequest = 16;
        /// <summary>
        /// Search should be started.
        /// object is SearchInfo
        /// </summary>
        public const int Search = 17;
        /// <summary>
        /// Search Result received.
        /// object is a SourceResultInfo
        /// </summary>
        public const int SearchResult = 18;
    }

    /*public enum Actions
    {
        /// <summary>
        /// We needs to supply a password and we have none saved.
        /// </summary>
        Password = 0,
        /// <summary>
        /// MainChat Message.
        /// </summary>
        MainMessage = Password +1,
        /// <summary>
        /// Private Message.
        /// </summary>
        PrivateMessage = MainMessage +1,
        /// <summary>
        /// One User has come online.
        /// object should be of type Containers.UserInfo
        /// </summary>
        UserOnline,
        /// <summary>
        /// One or Many Users has come online.
        /// object is of type string[] where entry is a User Id.
        /// </summary>
        UsersOnline,
        /// <summary>
        /// One user has gone offline.
        /// object is of type string and is the User Id.
        /// </summary>
        UserOffline,
        /// <summary>
        /// Info about the user has been updated.
        /// object is of type UserInfo
        /// </summary>
        UserInfoChange,
        /// <summary>
        /// List containing one or more Operators.
        /// object is of type string[] where entry is a User Id
        /// </summary>
        OpUsers,
        /// <summary>
        /// Indicates client status agains the hub. (Connecting, Connected, Disconnected)
        /// object is of type HubDelegates.HubStatus
        /// </summary>
        HubStatusChange,
        /// <summary>
        /// Indicates what Reg Mode we have in hub.
        /// object is of type int and can have:
        /// -1 = We havnt been logged on.
        ///  0 = Normal User.
        ///  1 = Regged User.
        ///  2 = Operator.
        /// </summary>
        HubRegMode,
        /// <summary>
        /// Tells Gui that it should reload itself (Language, colors and so on)
        /// object is null
        /// </summary>
        Reload,
        /// <summary>
        /// Hub Name/Title has been updated.
        /// </summary>
        WindowTitle,
        /// <summary>
        /// Tells Gui that is should close it self.
        /// object is null
        /// </summary>
        WindowClose,
        /// <summary>
        /// Tells Gui that it should be hidden.
        /// object is null
        /// </summary>
        WindowHide,
        /// <summary>
        /// Tells Gui to be showed.
        /// </summary>
        WindowShow,
        /// <summary>
        /// Incomming command from hub.
        /// </summary>
        HubCommandIncomming,
        /// <summary>
        /// Outgoing command to hub.
        /// </summary>
        HubCommandOutgoing
    };*/
}
