
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
    /// CzDZ
    /// </summary>
    public class CzDZ : DCppMod
    {
        public CzDZ()
        {
            System.Collections.Generic.List<string> hubAttr = Nodes["Hub"];

            hubAttr.Remove("Bottom");
            hubAttr.Remove("Top");
            hubAttr.Remove("Right");
            hubAttr.Remove("Left");

            hubAttr.Add("UserPassword");
            hubAttr.Add("ColumsOrder");
            hubAttr.Add("ColumsWidth");
            hubAttr.Add("ColumsVisible");
            hubAttr.Add("WindowPosX");
            hubAttr.Add("WindowPosY");
            hubAttr.Add("WindowSizeX");
            hubAttr.Add("WindowSizeY");
            hubAttr.Add("WindowType");
            hubAttr.Add("ChatUserSplit");
            hubAttr.Add("UserListState");
            hubAttr.Add("StealthMode");
        }
    }
}
