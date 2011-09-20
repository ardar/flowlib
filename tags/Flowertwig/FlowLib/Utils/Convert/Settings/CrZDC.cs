
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
    /// Based on CrZDC Beta 3
    /// </summary>
    public class CrZDC : DCppMod
    {
        public CrZDC()
        {
            System.Collections.Generic.List<string> hubAttr = Nodes["Hub"];

            hubAttr.Remove("Bottom");
            hubAttr.Remove("Top");
            hubAttr.Remove("Right");
            hubAttr.Remove("Left");

            hubAttr.Add("UserPassword");
            hubAttr.Add("ClientEmulation");
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
            hubAttr.Add("RawOne");
            hubAttr.Add("RawTwo");
            hubAttr.Add("RawThree");
            hubAttr.Add("RawFour");
            hubAttr.Add("RawFive");
            hubAttr.Add("RawSix");
            hubAttr.Add("RawSeven");
            hubAttr.Add("RawEight");
            hubAttr.Add("RawNine");
            hubAttr.Add("RawTen");
            hubAttr.Add("UserProtected");
            hubAttr.Add("RawOneActif");
            hubAttr.Add("RawTwoActif");
            hubAttr.Add("RawThreeActif");
            hubAttr.Add("RawFourActif");
            hubAttr.Add("RawFiveActif");
            hubAttr.Add("RawSixActif");
            hubAttr.Add("RawSevenActif");
            hubAttr.Add("RawEightActif");
            hubAttr.Add("RawNineActif");
            hubAttr.Add("RawTenActif");
            hubAttr.Add("CheckOnConnect");
            hubAttr.Add("CheckClients");
            hubAttr.Add("CheckFilelists");
            hubAttr.Add("UseMyinfoDetect");
            hubAttr.Add("CheckFakeShare");
            hubAttr.Add("ExtendedCheck1");
            hubAttr.Add("ExtendedCheck2");
            hubAttr.Add("ExtendedCheck3");
            hubAttr.Add("HeaderOrder");
            hubAttr.Add("CheckKey");
            hubAttr.Add("Mode");
            hubAttr.Add("IP");
            hubAttr.Add("UserIp");
            hubAttr.Add("HideShare");
        }
    }
}
