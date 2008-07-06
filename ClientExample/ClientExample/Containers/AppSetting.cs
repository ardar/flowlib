using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using FlowLib.Containers;

namespace ClientExample.Containers
{
    public class AppSetting
    {
        public static string GetSettingsFile()
        {
            return System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) +
                Path.DirectorySeparatorChar + "Xmpl" + Path.DirectorySeparatorChar + "AppSetting.xml";
        }

        public bool Installed
        {
            get;
            set;
        }

        public List<HubSetting> SavedHubs
        {
            get;
            set;
        }

        public int ConnectionMode
        {
            get;
            set;
        }
    }
}
