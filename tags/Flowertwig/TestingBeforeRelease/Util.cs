using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace TestingBeforeRelease
{
    public static class Util
    {
        private static string _applicationsDirectory;

        static Util()
        {
            DirectoryInfo di = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            _applicationsDirectory = di.Parent.Parent.FullName + Path.DirectorySeparatorChar + "Applications" + Path.DirectorySeparatorChar;
        }

        public static void InitilizeAll()
        {
            IsYnHubRunning();
            IsAdchppRunning();
            IsDCppRunning();
            IsStrongDCRunning();
        }

        public static void IsYnHubRunning()
        {
            #region Make sure YnHub is running

            var ynhub = Process.GetProcessesByName("YnHub").FirstOrDefault();
            if (ynhub == null)
            {
                if (!StartApplication("YnHub"))
                    throw new AssertFailedException("YnHub is not running");
            }
            #endregion
        }

        public static void IsAdchppRunning()
        {
            #region Make sure ADCHpp is running
            var proc = Process.GetProcessesByName("adchppd").FirstOrDefault();
            if (proc == null)
            {
                //if (!StartApplication("Adchpp") || !StartApplication("AdchppSecure", "-c {AppDir}configSecure"))
                if (!StartApplication("Adchpp") || !StartApplication("AdchppSecure"))
                    throw new AssertFailedException("ADCH++ is not running");
            }
            #endregion
        }

        public static void IsDCppRunning()
        {
            #region Make sure DC++ is running
            var dcpp = Process.GetProcessesByName("DCPlusPlusActive").FirstOrDefault();
            if (dcpp == null)
            {
                if (!StartApplication("DCppActive"))
                throw new AssertFailedException("DC++ is not running");
            }
            #endregion
        }

        public static void IsStrongDCRunning()
        {
            #region Make sure StrongDC++ is running
            var strong = Process.GetProcessesByName("StrongDC").FirstOrDefault();
            if (strong == null)
            {
                if (!StartApplication("StrongDCPassive"))
                    throw new AssertFailedException("StrongDC is not running");
            }
            #endregion
        }

        private static bool StartApplication(string directory)
        {
            return StartApplication(directory, null);
        }

        private static bool StartApplication(string directory, string arg)
        {
            try
            {
                string appDir = _applicationsDirectory + directory + Path.DirectorySeparatorChar;
                DirectoryInfo di = new DirectoryInfo(appDir);
                FileInfo fi = di.GetFiles("*.exe", SearchOption.TopDirectoryOnly).FirstOrDefault();
                if (fi == null)
                    return false;

                if (arg == null)
                {
                    Process.Start(fi.FullName);
                }
                else
                {
                    StringBuilder sbArg = new StringBuilder(arg);
                    sbArg.Replace("{AppDir}", appDir);

                    Process.Start(fi.FullName, sbArg.ToString());
                }
                // Wait 5 sec to let program start before we continue.
                System.Threading.Thread.Sleep(5 * 1000);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
        }
    }
}
