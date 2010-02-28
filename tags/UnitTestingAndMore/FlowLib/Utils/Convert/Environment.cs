
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

using System;

namespace FlowLib.Utils.Convert
{
    public class Environment
    {
        /// <summary>
        /// This conversion is based on info from:
        /// http://support.microsoft.com/kb/304283
        /// http://www.codeguru.com/cpp/misc/misc/system/article.php/c8973/
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetOperatingSystemName()
        {
            string name = string.Empty;
            System.Version version = (System.Version)System.Environment.OSVersion.Version;
            int platform = (int)System.Environment.OSVersion.Platform;
            
            switch (platform)
            {
                case (int)PlatformID.Win32Windows:     // Windows 95/98/ME
                    switch (version.Minor)
                    {
                        case 0: name = "Windows 95"; break;
                        case 10:
                            name = "Windows 98";
                            if (version.Revision.ToString().Equals("2222A"))
                                name += " Second Edition";
                            break;
                        case 90: name = "Windows Me"; break;
                    }
                    break;
                case (int)PlatformID.Win32NT:     // Windows NT/2000/XP/Vista
                    switch (version.Major)
                    {
                        case 3:
                            name = "Windows NT ";
                            switch (version.Minor)
                            {
                                case 0: name += "3"; break;
                                case 1: name += "3.1"; break;
                                case 5: name += "3.5"; break;
                                case 51:
                                default:
                                    name += "3.51"; break;
                            }
                            break;
                        case 4:
                            name = "Windows NT 4.0"; break;
                        case 5:
                            switch (version.Minor)
                            {
                                case 0: name = "Windows 2000"; break;
                                case 1: name = "Windows XP"; break;
                                case 2: name = "Windows Server 2003"; break;
                            }
                            break;
                        case 6:
                            name = "Windows Vista"; break;
                    }
                    break;
                case 3:
                    name = "Windows CE " + version.ToString(); break;
                case 4:
                case 128:
                    name = "Linux/Unix " + version.ToString(); break;
            }
            // If we havnt found a matching os. Write Full OS String
            if (name.Length == 0)
            {
                name = System.Environment.OSVersion.ToString();
            }
            // This is for finding new OS
            //name += string.Format("\r\nToString:{0}\r\n\tPlatform:{1}\r\n\tServicePack:{2}\r\n\tVersionString{3}\r\n\t{4}", System.Environment.OSVersion.ToString(), System.Environment.OSVersion.Platform, System.Environment.OSVersion.ServicePack, System.Environment.OSVersion.VersionString,
            //    string.Format("Version(Build:{0}, Major:{1}, MajorRevision:{2}, Minor:{3}, MinorRevision:{4}, Revision:{5})", System.Environment.OSVersion.Version.Build, System.Environment.OSVersion.Version.Major, System.Environment.OSVersion.Version.MajorRevision, System.Environment.OSVersion.Version.Minor, System.Environment.OSVersion.Version.MinorRevision, System.Environment.OSVersion.Version.Revision));
            return name;
        }
    }
}
