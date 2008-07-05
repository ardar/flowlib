using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ClientExample
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Client.Interface.MainWindow());

            //Guide.GuideWindow guide = new ClientExample.Guide.GuideWindow();
            //if (guide.ShowDialog() == DialogResult.OK)
            //{
            //    Application.Run(new MainWindow());
            //}
        }
    }
}
