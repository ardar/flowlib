
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
using System.Collections.Generic;
using FlowLib.Containers;

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Utils.Convert.Settings
{
    public abstract class XmlClient : BaseClient
    {
        protected SortedList<string, List<string>> nodes = new SortedList<string, List<string>>();
        protected XmlDocument document = null;
        protected XmlElement xmlCurrent = null;
        protected XmlElement xmlParent = null;

        public SortedList<string, List<string>> Nodes
        {
            get { return nodes; }
        }

        public override byte[] Write()
        {
            if (document == null)
            {
                document = new XmlDocument();
            }

            XmlDeclaration xmlDec;
            //xmlDec = document.CreateXmlDeclaration("1.0", System.Text.Encoding.UTF8.BodyName, "yes");
            xmlDec = document.CreateXmlDeclaration("1.0", "utf-8", "yes");
            document.AppendChild(xmlDec);

            XmlElement xmlFavorites = document.CreateElement("Favorites");
            xmlParent = document.CreateElement("Hubs");
            foreach (HubSetting var in hubs)
            {
                current = var;
                if (Nodes.ContainsKey("Hub"))
                {
                    NewNode("Hub", false);

                    NodeInfo("Hub", "Name", current.Name, false);
                    NodeInfo("Hub", "Description", current.Description, false);
                    NodeInfo("Hub", "Nick", current.DisplayName, false);
                    NodeInfo("Hub", "Password", current.Password, false);
                    NodeInfo("Hub", "Server", ToAddress(), false);
                    NodeInfo("Hub", "UserDescription", current.UserDescription, false);

                    List<string> hubattributes = Nodes["Hub"];

                    foreach (FlowKeyValuePair<string, string> attr in current.Items)
                    {
                        if (hubattributes.Contains(attr.Key))
                            NodeInfo("Hub", attr.Key, attr.Value, false);
                    }

                    EndNode("Hub", false);
                }
            }
            xmlFavorites.AppendChild(xmlParent);
            document.AppendChild(xmlFavorites);

            MemoryStream ms = new MemoryStream();
            document.Save(ms);
            return ms.ToArray();
        }
        public override bool Read(byte[] data)
        {
            if (document == null)
            {
                document = new XmlDocument();

                string xml = System.Text.Encoding.UTF8.GetString(data);
                xml = xml.Replace("\x001", "LOWCHAR001");
                if (xml.IndexOf('<') == 1)
                    xml = xml.Remove(0, 1);
                document.LoadXml(xml);
            }

            foreach (string node in nodes.Keys)
            {
                XmlNodeList elemList = document.GetElementsByTagName(node);
                for (int i = 0; i < elemList.Count; i++)
                {
                    NewNode(node, true);
                    foreach (XmlAttribute var in elemList[i].Attributes)
                    {
                        NodeInfo(node, var.Name, var.Value, true);
                        //System.Console.WriteLine(var.Name + ":" + var.Value);
                    }
                    EndNode(node, true);
                    //System.Console.WriteLine(elemList[i].InnerXml);
                }
            }
            return false;
        }
        public virtual void NewNode(string nodeName, bool read)
        {
            if (!read)
                xmlCurrent = document.CreateElement(nodeName);
        }
        public virtual void EndNode(string nodeName, bool read)
        {
            if (!read)
                xmlParent.AppendChild(xmlCurrent);
        }
        public virtual void NodeInfo(string nodeName, string attrName, string attrValue, bool read)
        {
            if (!read)
            {
                if (attrValue.Contains("LOWCHAR001"))
                    attrValue = attrValue.Replace("LOWCHAR001", "\x001");

                xmlCurrent.SetAttribute(attrName, attrValue);
            }
        }
        public virtual void FromAddress(string address)
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

            if (string.IsNullOrEmpty(current.Address))
                current.Address = address;

            current.Port = port;
        }
        public virtual string ToAddress()
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
