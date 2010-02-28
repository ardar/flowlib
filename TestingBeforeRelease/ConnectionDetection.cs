using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLib.Utils.Connection;

namespace TestingBeforeRelease
{
    [TestClass]
    public class ConnectionDetection
    {
        [TestMethod]
        public void ConnectionDetection_GetInternalIp()
        {
            Detect det = new Detect();
            det.Port = 9005;
            det.Start(Detect.WorkMethod.NoThread);

            if (det.InternalIP != null)
            {
                // This is for debuging test case
                //throw new AssertFailedException(det.InternalIP.ToString());
            }
            else
            {
                throw new AssertFailedException("Unable to get Internal IP Adress");
            }
        }

        [TestMethod]
        public void ConnectionDetection_GetExternalIp()
        {
            Detect det = new Detect();
            det.Port = 9004;
            det.Start(Detect.WorkMethod.NoThread);

            if (det.ExternalIP != null || det.ExternalIPUPnP != null)
            {
                // This is for debuging test case
                //throw new AssertFailedException(string.Format("{0} : {1}", det.ExternalIP, det.ExternalIPUPnP));
            }
            else
            {
                throw new AssertFailedException("Unable to get External IP Adress");
            }
        }

        [TestMethod]
        public void ConnectionDetection_WorksAllWayToTheEnd()
        {
            Detect det = new Detect();
            det.Port = 9000;
            det.Start(Detect.WorkMethod.NoThread);


            if (det.Progress != Detect.Functions.End)
            {
                throw new AssertFailedException("Detect didn't work it's way all the way to the end.");
            }
        }

        [TestMethod]
        public void ConnectionDetection_DoWeFindAConnection()
        {
            Detect det = new Detect();
            det.Port = 9001;
            det.Start(Detect.WorkMethod.NoThread);

            switch (det.ConnectionType)
            {
                case 0:
                    throw new AssertFailedException("Can't find any usable Internet connection.");
                case 1:
                    throw new AssertFailedException("You only seem to be able to be passive.");
                case 2:
                case 4:
                    break;
                default:
                    throw new AssertFailedException("We got a value that should be possible");
            }
        }
    }
}
