using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

using ClientExample.Containers;

namespace ClientExample
{
    static class Program
    {
        public static AppSetting Settings = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool hasSettings = true;
            try
            {
                string path = AppSetting.GetSettingsFile();
                Settings = FlowLib.Utils.FileOperations<AppSetting>.LoadObject(path);
            }
            catch { }
            finally
            {
                if (Settings == null)
                {
                    Settings = new AppSetting();
                    hasSettings = false;
                }
            }

            if (!hasSettings || !Settings.Installed)
            {
                Guide.GuideWindow guide = new ClientExample.Guide.GuideWindow();
                guide.ShowDialog();
            }

            if (Settings.Installed)
                Application.Run(new Client.Interface.MainWindow());
            else
                MessageBox.Show("Some how you have not installed this app properly. Please contact dev and tell him what steps you choosed.");
        }
    }
}
