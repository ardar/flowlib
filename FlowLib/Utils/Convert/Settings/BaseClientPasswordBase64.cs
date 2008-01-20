
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

using System.Text;

namespace FlowLib.Utils.Convert.Settings
{
    public class BaseClientPasswordBase64 : DCpp
    {
        public override void NodeInfo(string nodeName, string attrName, string attrValue, bool read)
        {
            bool handled = false;
            if (read)
            {
                switch (nodeName)
                {
                    case "Hub":
                        switch (attrName)
                        {
                            case "UserPassword":
                                current.Password = Encoding.UTF8.GetString(System.Convert.FromBase64String(attrValue));
                                handled = true;
                                break;
                        }
                        break;
                }
            }
            else
            {
                if (attrName.Equals("Password"))
                {
                    base.NodeInfo(nodeName, attrName, System.Convert.ToBase64String(Encoding.UTF8.GetBytes(attrValue)), read);
                    handled = true;
                }
            }


            if (!handled)
                base.NodeInfo(nodeName, attrName, attrValue, read);
        }
    }
}
