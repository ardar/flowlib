
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

using FlowLib.Interfaces;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing a Transfer message received/sent in Transfer
    /// </summary>
    public class TransferMessage : ConMessage
    {
        #region Variables
        protected string raw = null;    // Raw Message
        #endregion
        #region Properties
        /// <summary>
        /// Data converted to string using protocol encoding
        /// </summary>
        public string Raw
        {
            get { return raw; }
            set
            {
                raw = value;
                if (con != null && raw != null && con.Protocol != null)
                    bytes = con.Protocol.Encoding.GetBytes(raw);
            }
        }
        #endregion
        /// <summary>
        /// Creates Transfer message with connection and raw string
        /// </summary>
        /// <param name="con">Connection where message is related to</param>
        /// <param name="raw">Data converted to string using protocol encoding</param>
        public TransferMessage(IConnection con, string raw)
            : base(con, null)
        {
            this.Raw = raw;
        }
    }
}
