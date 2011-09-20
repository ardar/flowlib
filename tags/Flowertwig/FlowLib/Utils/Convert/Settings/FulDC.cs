
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

namespace FlowLib.Utils.Convert.Settings
{
    /// <summary>
    /// Based on fulDC 6.78
    /// </summary>
    public class FulDC : DCppMod
    {
        public FulDC()
        {
            System.Collections.Generic.List<string> hubAttr = Nodes["Hub"];

            hubAttr.Add("HeaderOrder");
            hubAttr.Add("HeaderWidths");
            hubAttr.Add("HeaderVisible");
            hubAttr.Add("StripIsp");
            hubAttr.Add("ShowUserlist");
            hubAttr.Add("LogMainchat");
        }
    }
}
