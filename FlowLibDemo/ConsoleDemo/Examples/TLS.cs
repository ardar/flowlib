using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleDemo.Examples
{
    public class TLS
    {
        public TLS()
        {
            new TLSServer();
            System.Threading.Thread.Sleep(2 * 1000);
            new TLSClient();
        }
    }
}
