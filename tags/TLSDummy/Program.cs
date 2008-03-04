





using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace TLS
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{

			//string certificate = AppDomain.CurrentDomain.BaseDirectory + "selfcert.pfx";
			//System.Security.Cryptography.X509Certificates.X509Certificate2 serverCertificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(certificate, "123456");
			//bool p = serverCertificate.HasPrivateKey;


			//Application.EnableVisualStyles();
			//Application.SetCompatibleTextRenderingDefault(false);
			//Application.Run(new Form1());
			try
			{
				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(RunServer));
				t.IsBackground = true;
				t.Start();

				FlowLib.Connections.TClient.RunClient("127.0.0.1", "FlowLib");
			}
			catch (System.Exception e)
			{
				System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "error.txt", e.ToString());
			}
		}

		public static void RunServer()
		{
			// selfcert.pfx
			//FlowLib.Connections.TServer.RunServer(AppDomain.CurrentDomain.BaseDirectory + "CodeProject.cer");
			FlowLib.Connections.TServer.RunServer(AppDomain.CurrentDomain.BaseDirectory + "selfcert.pfx");
		}
	}
}
