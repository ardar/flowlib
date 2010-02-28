
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
    /// Based on Apex 0.40
    /// </summary>
    public class Apex : DCppMod
    {
        public enum Versions
        {
            v0_40,
            v1_00Beta5
        }

        public Apex(Versions version)
        {
            System.Collections.Generic.List<string> hubAttr = Nodes["Hub"];

            hubAttr.Remove("Bottom");
            hubAttr.Remove("Top");
            hubAttr.Remove("Right");
            hubAttr.Remove("Left");

            hubAttr.Add("HeaderOrder");
            hubAttr.Add("HeaderWidths");
            hubAttr.Add("HeaderVisible");
            hubAttr.Add("ChatUserSplit");
            hubAttr.Add("UserListState");

            hubAttr.Add("HideShare");
            hubAttr.Add("Mode");
            hubAttr.Add("IP");

            hubAttr.Add("StealthMode");
            hubAttr.Add("Email");
            hubAttr.Add("AwayMsg");
            hubAttr.Add("ExclChecks");
            hubAttr.Add("ShowJoins");

            switch (version)
            {
                case Versions.v0_40:
                    hubAttr.Add("Encoding");
                    hubAttr.Add("NoAdlSearch");
                    hubAttr.Add("LogChat");
                    hubAttr.Add("MiniTab");
                    hubAttr.Add("AutoOpenOpenChat");
                    hubAttr.Add("HubChats");
                    hubAttr.Add("ProtectedPrefixes");
                    break;
                case Versions.v1_00Beta5:
                    hubAttr.Add("OpChat");
                    hubAttr.Add("RawOne");
                    hubAttr.Add("RawTwo");
                    hubAttr.Add("RawThree");
                    hubAttr.Add("RawFour");
                    hubAttr.Add("RawFive");
                    hubAttr.Add("WindowPosX");
                    hubAttr.Add("WindowPosY");
                    hubAttr.Add("WindowSizeX");
                    hubAttr.Add("WindowSizeY");
                    hubAttr.Add("WindowType");
                    break;
            }
        }
    }
}
