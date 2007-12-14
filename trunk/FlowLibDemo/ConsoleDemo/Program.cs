using System;
using System.Collections.Generic;
using System.Text;

using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Interfaces;
using ConsoleDemo.Examples;

namespace ConsoleDemo
{
    class Program
    {
        Share share = new Share("Default Share");
        HubSetting set1 = new HubSetting();
        HubSetting set2 = new HubSetting();
        Hub hub1, hub2;

        static void Main(string[] args)
        {
            new Testing();
            Console.Read();
        }

        Program()
        {
            // This is so we are not joining hub before hashing and adding of files has been finished
            share.HashingCompleted += new EventHandler<EventArgs>(share_HashingCompleted);
            // This is so we are refreshing filelists when share change. We are seperating them just to make this example more clear.
            share.HashingCompleted += new EventHandler<EventArgs>(share_HashingCompleted2);
            share.AddVirtualDir(@"ATI\", @"C:\ATI\");

            share.Port = 5000;
            // TODO : Start listening on port.
            //FlowLib.Managers.TransferManager.StartShareConnections(share);

            //set1.Address = "flow84.no-ip.org";
            //set1.Address = "192.168.1.12";
            set1.Address = "127.0.0.1";
            //set1.Port = 2876;
            set1.Port = 411;
            set1.Name = "FlowLib TestHub";
            set1.DisplayName = "FlowLib";
            set1.Password = "1";
            set1.ShareId = "Default Share";
            set1.Protocol = "Nmdc";

            set2.Name = "Mikejj Dev";
            set2.Address = "mrmikejj.co.uk";
            set2.Port = 1669;
            set2.DisplayName = "FlowLib";
            set2.Protocol = "Nmdc";

            // We want to hold the console window open
            while (true)
                System.Threading.Thread.Sleep(1);
        }

        void share_HashingCompleted2(object sender, EventArgs e)
        {
            #region Adding filelists
            string systempath = @"C:\Private\FMDC\PiP\FlowLibDemo\ConsoleDemo\bin\Debug\MyFileLists\";
            // Xml Utf-8 (Current DC++)
            FlowLib.Utils.FileLists.FilelistXmlBz2 xml = new FlowLib.Utils.FileLists.FilelistXmlBz2(share);
            xml.SystemPath = systempath;
            xml.Encoding = System.Text.Encoding.UTF8;
            xml.CreateFilelist();
            share.RemoveFile(xml.ContentInfo);
            share.AddFile(xml.ContentInfo);
            // Xml Ascii (Early DC++)
            xml.Encoding = System.Text.Encoding.ASCII;
            xml.CreateFilelist();
            share.RemoveFile(xml.ContentInfo);
            share.AddFile(xml.ContentInfo);
            // BzList
            FlowLib.Utils.FileLists.FilelistMyList dclst = new FlowLib.Utils.FileLists.FilelistMyList(share);
            dclst.SystemPath = systempath;
            dclst.CreateFilelist();
            share.RemoveFile(dclst.ContentInfo);
            share.AddFile(dclst.ContentInfo);
            #endregion
        }

        void share_HashingCompleted(object sender, EventArgs e)
        {
            if (hub1 == null)
            {
                UiHub ui = new UiHub();
                hub1 = new Hub(set1);
                hub1.Share = null; // share;
                //hub1.Me.TagInfo.Version = "FlowLib 20061125";
                //hub1.Me.Mode = FlowLib.Enums.ConnectionTypes.Direct;
                //hub1.Me.TagInfo.Slots = 0;
                hub1.Update += new FlowLib.Events.FmdcEventHandler(ui.Hub_Update);
                hub1.Connect();

                hub2 = new Hub(set2);
                // As we are not adding our share in sharemanager we will manualy need to add this share to hub.
                hub2.Share = share;

                //hub2.Me.TagInfo.Version = "FlowLib V:Unknown";
                //hub2.Me.Description = "PiP";
                //hub2.Me.Email = "noreplay@dummy.se";
                //hub2.Me.TagInfo.Slots = 2;
                //hub2.Me.Connection = "1";

                //hub2.Update += new FlowLib.Events.FmdcEventHandler(ui.Hub_Update);

                //hub2.ProtocolChange += new FlowLib.Events.FmdcEventHandler(hub2_ProtocolChange);
                //hub2.Connect();
            }
        }

        void hub2_ProtocolChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            IProtocol prot = e.Data as IProtocol;
            if (prot == null)
                return;
            prot.MessageReceived += new FlowLib.Events.FmdcEventHandler(prot_MessageReceived);
            prot.MessageToSend += new FlowLib.Events.FmdcEventHandler(prot_MessageToSend);
        }

        void prot_MessageToSend(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            HubMessage msg = (HubMessage)e.Data;
            Console.WriteLine("Out: " + msg.Raw);
        }

        void prot_MessageReceived(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            HubMessage msg2 = (HubMessage)e.Data;
            Console.WriteLine("In: " + msg2.Raw);
        }
    }
}
