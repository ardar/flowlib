using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestingBeforeRelease.Utils.Processes
{
    public static class DCPlusPlus
    {
        public static Process CurrentInstance
        {
            get { return Application.GetProcess("DCPlusPlusActive"); }
        }

        public static void MakeSureItIsRunning()
        {
            Process proc = CurrentInstance;
            if (proc == null)
            {
                if (!Application.StartApplication("DCppActive"))
                    throw new AssertFailedException("DC++ is not running");
            }
        }

        public static void Kill()
        {
            Process proc = CurrentInstance;
            if (proc != null)
            {
                proc.Close();
            }
        }
    }
}
