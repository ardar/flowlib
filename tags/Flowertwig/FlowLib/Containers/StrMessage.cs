
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

using FlowLib.Interfaces;

namespace FlowLib.Containers
{
    public class StrMessage : ConMessage
    {
        #region Variables
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
        /// raw message
        /// </summary>
        public virtual string Raw
        {
            get { return raw; }
            set
            {
                raw = value;
                if (con != null && raw != null)
                    bytes = con.Protocol.Encoding.GetBytes(raw);
            }
        }
        #endregion

        public StrMessage(IConnection con, string raw)
            : base(con, null)
        {
            Raw = raw;
        }

    }
}
