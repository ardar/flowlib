// TODO : Can we realy use AppDomain
//#define APPDOMAIN

/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Policy;

namespace FlowLib.Plugins
{
    [Serializable]
    public class PluginContainer
    {
        protected Type type = null;
        protected PluginInfo info = null;
        protected PluginBase instance = null;
        protected AppDomain domain = null;

        public bool IsRunning
        {
            get { return (instance != null); }
        }

        public PluginContainer()
        {

        }

        public PluginContainer(Type assemblyType, PluginInfo info)
        {
            type = assemblyType;
            this.info = info;
        }

        public void Start()
        {
            if (instance == null)
            {
#if APPDOMAIN
                // Diffrent AppDomain
                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = Path.GetDirectoryName(info.FilePath);
                domain = AppDomain.CreateDomain(Path.GetFileName(info.FilePath) + type.GUID.ToString(), null, setup);
                instance = (PluginBase)domain.CreateInstanceAndUnwrap(type.Namespace, type.FullName);
#else
                // Same AppDomain
                //instance = (PluginBase)Activator.CreateInstance(type);
#endif
            }
        }

        public void End()
        {
#if !APPDOMAIN
            //AppDomain.Unload(domain);
            //instance = null;
            //domain = null;
#endif
        }
    }
}
