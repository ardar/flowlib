
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

namespace FlowLib.Events
{
    public class Actions
    {
        /// <summary>
        /// We needs to supply a password and we have none saved.
        /// Return password in e.Data
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
        /// <summary>
        /// Hub have requested you to be redirected to a diffrent hub.
        /// You have been disconnected from hub when this even occurs.
        /// object is RedirectInfo
        /// </summary>
        public const int Redirect = 19;
        /// <summary>
        /// You have been banned from hub.
        /// You have been disconnected from hub when this event occurs.
        /// Reconnect time has also been set to the one in BannedInfo
        /// object is BannedInfo
        /// </summary>
        public const int Banned = 20;
        /// <summary>
        /// Telling hub that we want to start transfer with User
        /// object is User
        /// </summary>
        public const int StartTransfer = 21;



#if !COMPACT_FRAMEWORK
        /// <summary>
        /// Used when validating a remote certificate in a secure connection.
        /// object is CertificateValidationInfo
        /// </summary>
        public const int SecurityValidateRemoteCertificate = 1000;
        /// <summary>
        /// Used to select Certificate to use in a secure connection.
        /// object is LocalCertificationSelectionInfo
        /// </summary>
        public const int SecuritySelectLocalCertificate = 1001;
#endif
    }
}
