
/*
 *
 * Copyright (C) 2006 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using System.Collections.Generic;
using FlowLib.Interfaces;
using FlowLib.Containers;
using FlowLib.Plugins;

using System;
using System.IO;
using System.Reflection;

namespace FlowLib.Managers
{
    public class PluginManager
    {
        protected System.Collections.Generic.SortedList<string, PluginContainer> availablePlugins = new SortedList<string, PluginContainer>();
        public string FileName = "Plugins";


        public void Load()
        {
            Load(AppDomain.CurrentDomain.BaseDirectory);
        }

        public void Load(string dir)
        {
            if (string.IsNullOrEmpty(dir))
                dir = AppDomain.CurrentDomain.BaseDirectory;

            string[] fileNames = Directory.GetFiles(dir, "*.dll");
            foreach (string fName in fileNames)
            {
                Assembly asm = Assembly.LoadFile(fName);
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsSubclassOf(typeof(PluginBase)))
                    {
                        PluginInfo pi = new PluginInfo(fName);
                        PluginContainer pc = new PluginContainer(type, pi);
                        availablePlugins.Add(fName, pc);
                        // We can only handle one plugin foreach dll.
                        break;
                    }
                }
            }
        }

        public bool LoadPlugin(string path)
        {
            PluginContainer pc = null;
            if (availablePlugins.TryGetValue(path, out pc))
            {
                pc.Start();
                return true;
            }
            return false;
        }

        public bool UnloadPlugin(string path)
        {
            PluginContainer pc = null;
            if (availablePlugins.TryGetValue(path, out pc))
            {
                pc.End();
                return true;
            }
            return false;
        }
    }
}
