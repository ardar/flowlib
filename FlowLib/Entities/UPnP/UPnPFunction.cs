
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

using System.Collections.Generic;
using System.Text;

namespace FlowLib.Entities.UPnP
{
    public class UPnPFunction
    {
        public string Name
        {
            get;
            set;
        }

        public SortedList<string, string> Arguments
        {
            get;
            set;
        }

        public ServiceBase Service
        {
            get;
            set;
        }

        /// <summary>
        /// Value is the numeric representation of HttpStatusCode
        /// </summary>
        public int ErrorCode
        {
            get;
            set;
        }

        public UPnPFunction()
        {
            Arguments = new SortedList<string, string>();
            ErrorCode = 0;
        }

		public override string ToString()
		{
			var sb = new StringBuilder();
			if (Service != null && Service.Device != null && Service.Device.Information != null)
			{
				sb.Append("Device: ");
				sb.Append(Service.Device.Information.Sender);
				sb.Append(" (");
				sb.Append(Service.Device.Information.DeviceType);
				sb.AppendLine(")");
			}
			sb.Append(Name);
			sb.AppendLine("(");
			foreach (KeyValuePair<string, string> item in Arguments)
			{
				sb.AppendFormat("\t{0} = {1}\r\n", item.Key, item.Value);
			}
			sb.AppendLine(")");
			return sb.ToString();
		}
    }
}
