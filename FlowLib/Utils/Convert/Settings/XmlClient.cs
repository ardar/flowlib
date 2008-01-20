
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
using System.Collections.Generic;

namespace FlowLib.Utils.Convert.Settings
{
    public abstract class XmlClient : BaseClient
    {
        protected List<string> nodes = new List<string>();
        protected XmlDocument document = null;

        public List<string> Nodes
        {
            get { return nodes; }
        }

        public override bool Read(byte[] data)
        {
            //StreamReader sr = new StreamReader(ms,System.Text.Encoding.Default);
            //document.Load(ms);
            MemoryStream ms = new MemoryStream(data);
            if (document == null)
            {
                document = new XmlDocument();
                document.Load(ms);
            }

            foreach (string node in nodes)
            {
                XmlNodeList elemList = document.GetElementsByTagName(node);
                for (int i = 0; i < elemList.Count; i++)
                {
                    NewNode(node);
                    foreach (XmlAttribute var in elemList[i].Attributes)
                    {
                        NodeInfo(node, var.Name, var.Value);
                        //System.Console.WriteLine(var.Name + ":" + var.Value);
                    }
                    EndNode(node);
                    //System.Console.WriteLine(elemList[i].InnerXml);
                }
            }
            return false;
        }

        public abstract void NewNode(string nodeName);
        public abstract void EndNode(string nodeName);
        public abstract void NodeInfo(string nodeName, string attrName, string attrValue);
    }
}
