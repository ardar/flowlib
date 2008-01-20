
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

using System.IO;
using System.Xml;
using FlowLib.Containers;

namespace FlowLib.Utils.Convert.Settings
{
    /// <summary>
    /// DC++
    /// </summary>
    public class DCpp : XmlClient
    {

        public DCpp()
        {
            Nodes.Clear();
            Nodes.Add("Hub");
        }

        public override void NewNode(string nodeName)
        {
            switch (nodeName)
            {
                case "Hub":
                    current = new HubSetting();
                    break;
            }
        }
        public override void NodeInfo(string nodeName, string attrName, string attrValue)
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
                        default:
                            // We dont have this value in FlowLib. But other clients may have so we will still save it.
                            current.Set(attrName, attrValue);
                            break;
                    }
                    break;
            }
        }
        public override void EndNode(string nodeName)
        {
            switch (nodeName)
            {
                case "Hub":
                    hubs.Add(current);
                    break;
            }
        }

        public void FromAddress(string address)
        {
            int pos;
            string protocol = "Nmdc";
            if ((pos = address.IndexOf("://")) != -1)
            {
                switch (address.Substring(0, pos))
                {
                    case "adc":
                        protocol = "Adc";
                        break;
                    case "adcs":
                        protocol = "AdcSecure";
                        break;
                    case "dchub":
                        protocol = "Nmdc";
                        break;
                }
                pos += 3;
                address = address.Remove(0, pos);
            }
            current.Protocol = protocol;

            int port = 411;
            if ((pos = address.IndexOf(":")) != -1)
            {
                current.Address = address.Substring(0, pos);
                pos++;
                try
                {
                    port = int.Parse(address.Substring(pos));
                }
                catch { port = 411; }
            }
            current.Port = port;
        }
        public string ToAddress()
        {
            string protocol = string.Empty;
            switch (current.Protocol)
            {
                case "Nmdc":
                    protocol = "dchub://";
                    break;
                case "Nmdcs":
                    protocol = "dchubs://";
                    break;
                case "Adc":
                    protocol = "adc://";
                    break;
                case "AdcSecure":
                    protocol = "adcs://";
                    break;
            }
            return string.Format("{0}{1}:{2}", protocol, current.Address, current.Port.ToString());
        }
    }
}
