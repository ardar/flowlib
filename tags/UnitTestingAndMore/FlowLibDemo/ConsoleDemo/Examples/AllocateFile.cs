using System;
using System.Collections.Generic;
using System.Text;
using FlowLib.Utils;
using FlowLib.Containers;

namespace ConsoleDemo.Examples
{
    public class AllocateFile
    {
        string currentDir = System.AppDomain.CurrentDomain.BaseDirectory;

        public AllocateFile()
        {
            ContentInfo ci = new ContentInfo(ContentInfo.TTH, "WBAVJMJV53N5YSYTV3SYSNTMWCV3NMQ6WDIWX7I");
            ci.Set(ContentInfo.STORAGEPATH, currentDir + "some-file.iso");
            ci.Size = 2443255643L;

            //Allocate file
            FileOperations.AllocateFile(ci.Get(ContentInfo.STORAGEPATH), ci.Size);
        }
    }
}
