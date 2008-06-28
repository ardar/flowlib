using System;

using ConsoleDemo.Examples;

namespace ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            new ActiveEmptySharingUsingTLS();
            System.Threading.Thread.Sleep(10 * 1000);
            new PassiveDownloadFilelistFromUserUsingTLS();
            Console.Read();
        }

        Program()
        {
        }
    }
}
