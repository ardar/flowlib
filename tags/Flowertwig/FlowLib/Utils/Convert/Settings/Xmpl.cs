
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

using System.IO;
using System.Xml;
using FlowLib.Containers;

namespace FlowLib.Utils.Convert.Settings
{
    /// <summary>
    /// Xmpl - Client built on FlowLib to show others whats missing in their client and to show how to build stuff on FlowLib 
    /// </summary>
    public class Xmpl : XmlClient
    {
        protected bool oldClient = false;

        public Xmpl()
        {
            System.Collections.Generic.List<string> hubAttr = new System.Collections.Generic.List<string>();

            hubAttr.Add("Name");
            hubAttr.Add("Connect");
            hubAttr.Add("Description");
            hubAttr.Add("Nick");
            hubAttr.Add("Password");
            hubAttr.Add("Server");
            hubAttr.Add("UserDescription");
			hubAttr.Add("UserListState");

            Nodes.Add("Hub", hubAttr);
        }

        public override void NewNode(string nodeName, bool read)
        {
            if (read)
            {
                switch (nodeName)
                {
                    case "Hub":
                        current = new HubSetting();
                        break;
                }
            }
            else
            {
                base.NewNode(nodeName, read);
            }
        }
        public override void NodeInfo(string nodeName, string attrName, string attrValue, bool read)
        {
            if (read)
            {
                switch (nodeName)
                {
                    case "Hub":
                        switch (attrName)
                        {
                            case "Name":
                                current.Name = attrValue;
                                break;
                            case "Description":
                                current.Description = attrValue;
                                break;
                            case "Nick":
                                current.DisplayName = attrValue;
                                break;
                            case "Password":
                                current.Password = attrValue;
                                break;
                            case "Server":
                                FromAddress(attrValue);
                                break;
                            case "UserDescription":
                                current.UserDescription = attrValue;
                                break;
					        case "UserListState":
						        current.Set("UserListState", attrValue);
						        break;
                            default:
                                // We dont have this value in Xmpl. But other clients may have so we will still save it.
                                current.Set(attrName, attrValue);
                                break;
                        }
                        break;
                }
            }
            else
            {
                base.NodeInfo(nodeName, attrName, attrValue, read);
            }
        }
        public override void EndNode(string nodeName, bool read)
        {
            if (read)
            {
                switch (nodeName)
                {
                    case "Hub":
                        hubs.Add(current);
                        break;
                }
            }
            else
            {
                base.EndNode(nodeName, read);
            }
        }
    }
}
