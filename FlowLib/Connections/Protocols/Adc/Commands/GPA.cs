
/*
 *
 * Copyright (C) 2010 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    #region Receive AND Send

    #endregion
    #region Send

    #endregion
    #region Receive

    public class GPA : AdcBaseMessage
    {
        protected string randomData = null;

        public string RandomData
        {
            get { return randomData; }
        }

        public GPA(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 1)
            {
                randomData = param[0];
                valid = true;
            }
        }
    }
    #endregion
}
