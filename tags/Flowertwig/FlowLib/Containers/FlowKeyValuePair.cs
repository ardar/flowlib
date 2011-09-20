
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

using System.Xml.Serialization;

namespace FlowLib.Containers
{
    public class FlowKeyValuePair<TKey, TValue>
    {
        protected bool changeKey = false;
        protected bool changeValue = false;

        protected TKey vKey = default(TKey);
        protected TValue vValue = default(TValue);

        [XmlAttribute(AttributeName = "Key")]
        public TKey Key
        {
            get { return vKey; }
            set
            {
                if (changeKey)
                {
                    vKey = value;
                    changeKey = false;
                }
            }
        }
        [XmlAttribute(AttributeName = "Value")]
        public TValue Value
        {
            get { return vValue; }
            set
            {
                if (changeValue)
                {
                    vValue = value;
                    changeValue = false;
                }
            }
        }

        public FlowKeyValuePair() { /* Dummy for serialization */ changeKey = true; changeValue = true; }

        public FlowKeyValuePair(TKey key, TValue value)
        {
            vKey = key;
            vValue = value;
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
