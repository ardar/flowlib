using System;

using ConsoleDemo.Examples;

using System.Collections.Generic;
using FlowLib.Containers;
using FlowLib.Utils;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            //new ActiveEmptySharingUsingTLS();
            //System.Threading.Thread.Sleep(10 * 1000);
            //new PassiveDownloadFilelistFromUserUsingTLS();
            //new PassiveSearch();
            //new AllocateFile();
            new ActiveSearchAndDownload();
            Console.Read();
            //new Program();
        }

        Program()
        {
            //var cnt = new ContentInfo(ContentInfo.VIRTUAL, "Testing");
            //var dwn = new DownloadItem(cnt);
            //cnt.Size = 1024;
            //List<FlowKeyValuePair<string, int>> lst = new List<FlowKeyValuePair<string, int>>();
            //lst.Add(new FlowKeyValuePair<string, int>("pip", 1337));

            // WORKS!!
            //FileOperations<List<FlowKeyValuePair<string, int>>>.SaveObject(AppDomain.CurrentDomain.BaseDirectory + "test.xml", lst);
            //var tmp = FileOperations<List<FlowKeyValuePair<string, int>>>.LoadObject(AppDomain.CurrentDomain.BaseDirectory + "test.xml");

            //List<KeyValuePair<string, int>> lst2 = new List<KeyValuePair<string, int>>();
            //lst2.Add(new KeyValuePair<string, int>("pip", 1337));

            //// does NOT work!
            //FileOperations<List<KeyValuePair<string, int>>>.SaveObject(AppDomain.CurrentDomain.BaseDirectory + "test2.xml", lst2);
            //var tmp2 = FileOperations<List<KeyValuePair<string, int>>>.LoadObject(AppDomain.CurrentDomain.BaseDirectory + "test2.xml");


            // WORKS!!
            //FileOperations<FlowSortedList<DownloadItem>>.SaveObject(AppDomain.CurrentDomain.BaseDirectory + "test2.xml", flst);
            //var tmp2 = FileOperations<FlowSortedList<DownloadItem>>.LoadObject(AppDomain.CurrentDomain.BaseDirectory + "test2.xml");

            

            FlowLib.Managers.DownloadManager mgr = new FlowLib.Managers.DownloadManager();
            var cnt = new FlowLib.Containers.ContentInfo(FlowLib.Containers.ContentInfo.VIRTUAL, "Testing");
            cnt.Size = 1024;

            mgr.AddDownload(new FlowLib.Containers.DownloadItem(cnt), new FlowLib.Containers.Source("127.0.0.1", "Flow84"));
            mgr.Save();

            FlowLib.Managers.DownloadManager tmpMgr = new FlowLib.Managers.DownloadManager();
            tmpMgr.Load();

            return;
        }
    }
}
