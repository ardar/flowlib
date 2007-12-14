
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

using FlowLib.Interfaces;
using FlowLib.Connections;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing a message from a Hub (p2p Direct Connect)
    /// </summary>
    public class HubMessage : ConMessage, IHubMessage
    {
        #region Variables
        protected string from = null;       // User ID
        protected string to = null;         // User ID
        protected string raw = null;        // Raw Message
        protected bool valid = false;       // Tells if message is a valid/correct.
        #endregion
        #region Properties
        /// <summary>
        /// Indicates if parsing of message was OK and if message was delivered in correct state
        /// </summary>
        public bool IsValid
        {
            get { return valid; }
            set { valid = true; }
        }

        /// <summary>
        /// Tells what hub message is related to
        /// </summary>
        public Hub Hub
        {
            get { return (Hub)con; }
            set {
                if (value is Hub)
                    con = value; 
            }
        }
        /// <summary>
        /// Id of user that message was received from
        /// </summary>
        public string From
        {
            get { return from; }
        }
        /// <summary>
        /// Id of user that message was sent to
        /// </summary>
        public string To
        {
            get { return to; }
        }
        /// <summary>
        /// raw message
        /// </summary>
        public string Raw
        {
            get { return raw; }
            set { 
                raw = value;
                if (Hub != null && raw != null)
                    bytes = Hub.Protocol.Encoding.GetBytes(raw);
            }
        }
        #endregion
        /// <summary>
        /// Creating HubMessage
        /// </summary>
        /// <param name="hub">Hub that this message is related to</param>
        /// <param name="raw">raw message</param>
        public HubMessage(Hub hub, string raw)
            : this(hub, raw, null, null) { }

        /// <summary>
        /// Creating HubMessage
        /// </summary>
        /// <param name="hub">Hub that this message is related to</param>
        /// <param name="raw">raw message</param>
        /// <param name="from">User id of the user message was from</param>
        /// <param name="to">User id of the user message is to</param>
        public HubMessage(Hub hub, string raw, string from, string to)
            : base(hub,null)
        {
            this.Raw = raw;
            this.from = from;
            this.to = to;
        }
    }
}
