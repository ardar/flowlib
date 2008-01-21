
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

namespace FlowLib.Utils.Convert.Settings
{
    /// <summary>
    /// DC++ to 0.4033
    /// </summary>
    public class DCpp0_403 :DCpp
    {
        public DCpp0_403() : base() { }

        public override bool Read(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            StreamReader sr = new StreamReader(ms, System.Text.Encoding.Default);
            document = new System.Xml.XmlDocument();
            document.Load(sr);
            return base.Read(data);
        }
    }
}
