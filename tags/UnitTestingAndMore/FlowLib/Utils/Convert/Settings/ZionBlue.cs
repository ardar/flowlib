
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
    /// Based on Zion Blue (2.01 - 2.05)
    /// This dev should really be kicked hard..
    /// Why in hell do you want to change values in all versions?!
    /// </summary>
    public class ZionBlue : DCppMod
    {
        public enum Versions
        {
            v2_01,
            v2_02,
            v2_03,
            v2_04,
            v2_05
        }

        public ZionBlue(Versions version)
            :base()
        {
            System.Collections.Generic.List<string> hubAttr = Nodes["Hub"];

            switch (version)
            {
                case Versions.v2_01:
                case Versions.v2_02:
                    hubAttr.Add("UserPassword");
                    break;
                case Versions.v2_03:
                    break;
                case Versions.v2_04:
                    hubAttr.Add("ShowJoins");

                    hubAttr.Add("DescriptionFormat");
                    hubAttr.Add("CID");
                    hubAttr.Add("UserConnection");
                    hubAttr.Add("UserUpload");
                    hubAttr.Add("AwayMessage");
                    hubAttr.Add("UserEmail");
                    hubAttr.Add("NoShare");
                    hubAttr.Add("ClientEmulation");
                    break;
                case Versions.v2_05:
                    hubAttr.Add("AwayMsg");
                    hubAttr.Add("Email");
                    hubAttr.Add("ShowJoins");
                    break;
            }

            hubAttr.Remove("Bottom");
            hubAttr.Remove("Top");
            hubAttr.Remove("Right");
            hubAttr.Remove("Left");

            hubAttr.Add("WindowPosX");
            hubAttr.Add("WindowPosY");
            hubAttr.Add("WindowSizeX");
            hubAttr.Add("WindowSizeY");
            hubAttr.Add("WindowType");
            hubAttr.Add("ChatUserSplit");
            hubAttr.Add("UserListState");
            hubAttr.Add("HeaderOrder");
            hubAttr.Add("HeaderWidths");
            hubAttr.Add("HeaderVisible");

            hubAttr.Add("CheckOnConnect");
            hubAttr.Add("CheckClients");
            hubAttr.Add("CheckFilelists");
            hubAttr.Add("UseMyinfoDetect");
            hubAttr.Add("CheckFakeShare");
            hubAttr.Add("ExtendedCheck1");
            hubAttr.Add("ExtendedCheck2");
            hubAttr.Add("ExtendedCheck3");
            hubAttr.Add("CheckKey");
            hubAttr.Add("UserIp");
            hubAttr.Add("UserProtected");
        }
    }
}
