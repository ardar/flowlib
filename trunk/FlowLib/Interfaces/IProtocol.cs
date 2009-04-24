
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

using FlowLib.Events;
using FlowLib.Containers;

namespace FlowLib.Interfaces
{
    /// <summary>
    /// Interface that can be used if plugin(s) want to support a new protocol. (ADCS and Compressed are 2 examples)
    /// </summary>
    public interface IProtocol : IUpdate, System.IDisposable
    {
        /// <summary>
        /// Is triggered when a message from server is received.
        /// </summary>
        event FmdcEventHandler MessageReceived;
        /// <summary>
        /// Is triggered before a message to server is beeing sent.
        /// </summary>
        event FmdcEventHandler MessageToSend;
        /// <summary>
        /// Events have happen in this hub and we want to tell others.
        /// Uses Actions.X as action property for e.
        /// </summary>
        //event FmdcEventHandler Update;
        /// <summary>
        /// If this isnt null. We will send this message to see if connection is still alive.
        /// </summary>
        IConMessage KeepAliveCommand
        {
            get;
        }
        /// <summary>
        /// Returns the name of this protocol.
        /// TODO : Should be canged to a static property.
        /// </summary>
        string Name
        {
            get;
        }
        /// <summary>
        /// If this isnt null. We will send this on connect.
        /// </summary>
        IConMessage FirstCommand
        {
            get;
        }
        /// <summary>
        /// Indicates if connection has been disposed or not.
        /// </summary>
        bool IsDisposed
        {
            get;
        }

        /// <summary>
        /// Parsing raw command into a Inherit class of HubMessage
        /// </summary>
        /// <param name="hub">Hub where raw command was received</param>
        /// <param name="raw">Raw data that was received</param>
        /// <returns>Inherit class of ConMessage, If parsing was unsuccessfull ConUnknownMessage will be returned.</returns>
        void ParseRaw(byte[] b, int length);
        /// <summary>
        /// Doing action depending on incomming message.
        /// </summary>
        /// <param name="message">HubMessage to react on</param>
        void ActOnInMessage(IConMessage message);
        /// <summary>
        /// Converting Internal containers to protocol messages.
        /// </summary>
        /// <param name="hub">Hub where message should be sent to</param>
        /// <param name="message">Internal Message Container.
        /// Known message containers are:
        /// MainMessage
        /// PrivateMessage  
        /// </param>
        void ActOnOutMessage(FmdcEventArgs e);
        bool OnSend(IConMessage msg);
        /// <summary>
        /// Gets/sets Encoding to use to for raw representation.
        /// NMDC = System.Text.Encoding.GetEncoding(1252)
        /// ADC  = Encoding.UTF8
        /// </summary>
        System.Text.Encoding Encoding
        {
            get;
            set;
        }
        /// <summary>
        /// Gets/sets what string pattern that should indicate a new command.
        /// NMDC = "|"
        /// ADC  = "\r"
        /// </summary>
        string Seperator
        {
            get;
        }
    }
}
