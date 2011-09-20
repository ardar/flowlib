
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

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Utils.Convert.Settings
{
    /// <summary>
    /// DC++ Modification
    /// Some setting will be auto renamed so that easy convertion between diffrent mods can be done.
    /// 
    /// </summary>
    public class DCppMod : DCpp
    {
        public DCppMod() : base() { }
        public DCppMod(bool beforeDCpp0_403) : base(beforeDCpp0_403) { }

        public override void NodeInfo(string nodeName, string attrName, string attrValue, bool read)
        {
            if (read)
            {
                // Reading
                switch (nodeName)
                {
                    case "Hub":
                        switch (attrName)
                        {
                            case "UserProtected":   // Converting to ProtectedUsers
                            case "ProtectedPrefixes":
                                attrName = "ProtectedUsers";
                                break;
                            case "HeaderOrder":     // Converting to ColumsOrder
                                attrName = "ColumsOrder";
                                break;
                            case "HeaderWidths":     // Converting to ColumsWidth
                                attrName = "ColumsWidth";
                                break;
                            case "HeaderVisible":   // Converting to ColumsVisible
                                attrName = "ColumsVisible";
                                break;
                            case "WindowPosY":      // Converting to Top
                                attrName = "Top";
                                break;
                            case "WindowPosX":      // Converting to Left
                                attrName = "Left";
                                break;
                            case "WindowSizeX":     // Converting to Right
                                attrName = "Right";
                                break;
                            case "WindowSizeY":     // Converting to Bottom
                                attrName = "Bottom";
                                break;
                            case "LogChat":         // Converting to LogMainchat (This is more clear what it should actually log)
                                attrName = "LogMainchat";
                                break;
                            case "UserIp":          // Converting to IP
                                attrName = "IP";
                                break;
                            case "AwayMessage":     // Converting to AwayMsg
                                attrName = "AwayMsg";
                                break;
                            case "ShowUserlist":       // Converting to UserListState
                                attrName = "UserListState";
                                break;
                            case "UserPassword":        // Converting to Password
                                attrName = "Password";
                                attrValue = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(attrValue));
                                break;
                        }
                        break;
                }
            }
            else
            {
                // Writing
                switch (nodeName)
                {
                    case "Hub":
                        switch (attrName)
                        {
                            case "ProtectedUsers":   // Converting from ProtectedUsers
                                if (nodes.ContainsKey("UserProtected"))
                                    attrName = "UserProtected";
                                if (nodes.ContainsKey("ProtectedPrefixes"))
                                    attrName = "ProtectedPrefixes";
                                break;
                            case "ColumsOrder":     // Converting from ColumsOrder
                                if (nodes.ContainsKey("HeaderOrder"))
                                    attrName = "HeaderOrder";
                                break;
                            case "ColumsWidth":     // Converting from ColumsWidth
                                if (nodes.ContainsKey("HeaderWidths"))
                                    attrName = "HeaderWidths";
                                break;
                            case "ColumsVisible":   // Converting from ColumsVisible
                                if (nodes.ContainsKey("HeaderVisible"))
                                    attrName = "HeaderVisible";
                                break;
                            case "Top":      // Converting from Top
                                if (nodes.ContainsKey("WindowPosY"))
                                    attrName = "WindowPosY";
                                break;
                            case "Left":      // Converting from Left
                                if (nodes.ContainsKey("WindowPosX"))
                                    attrName = "WindowPosX";
                                break;
                            case "Right":     // Converting from Right
                                if (nodes.ContainsKey("WindowSizeX"))
                                   attrName = "WindowSizeX";
                                break;
                            case "Bottom":     // Converting from Bottom
                                if (nodes.ContainsKey("WindowSizeY"))
                                    attrName = "WindowSizeY";
                                break;
                            case "LogMainchat":         // Converting from LogMainchat
                                if (nodes.ContainsKey("LogChat"))
                                    attrName = "LogChat";
                                break;
                            case "IP":          // Converting from IP
                                if (nodes.ContainsKey("UserIp"))
                                    attrName = "UserIp";
                                break;
                            case "AwayMsg":     // Converting from AwayMsg
                                if (nodes.ContainsKey("AwayMessage"))
                                    attrName = "AwayMessage";
                                break;
                            case "Email":       // Converting from Email
                                if (nodes.ContainsKey("UserEmail"))
                                    attrName = "UserEmail";
                                break;
                            case "UserListState":   // Converting from UserListState
                                if (nodes.ContainsKey("ShowUserlist"))
                                    attrName = "ShowUserlist";
                                break;
                            case "Password":        // Converting from Password
                                if (nodes.ContainsKey("UserPassword"))
                                {
                                    attrName = "UserPassword";
                                    attrValue = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(attrValue));
                                }
                                break;
                        }
                        break;
                }
            }
            base.NodeInfo(nodeName, attrName, attrValue, read);
        }
    }
}
