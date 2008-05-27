
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Net.Security;
using System.Security.Authentication;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace FlowLib.Connections
{
	public class TServer
	{
		static X509Certificate2 serverCertificate2 = null;
        static X509Certificate serverCertificate = null;
        // The certificate parameter specifies the name of the file 
		// containing the machine certificate.

		public static void RunServer(string certificate)
		{
			// http://www.microsoft.com/msdownload/platformsdk/sdkupdate/XPSP2FULLInstall.htm

			serverCertificate2 = new X509Certificate2(certificate);
			serverCertificate = X509Certificate.CreateFromCertFile(certificate);

			// Create a TCP/IP (IPv4) socket and listen for incoming connections.
			//TcpListener listener = new TcpListener(IPAddress.Any, 8080);
			TcpListener listener = new TcpListener(IPAddress.Any, 8000);
			listener.Start();
			while (true)
			{
				Console.WriteLine("Waiting for a client to connect...");
				// Application blocks while waiting for an incoming connection.
				// Type CNTL-C to terminate the server.
				TcpClient client = listener.AcceptTcpClient();
				ProcessClient(client);
			}
		}

        // The following method is invoked by the RemoteCertificateValidationDelegate.
        public static bool ValidateClientCertificate(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            //if (sslPolicyErrors == SslPolicyErrors.None)
                return true;

            Console.WriteLine("Certificate error: {0}", sslPolicyErrors);

            // Do not allow this client to communicate with unauthenticated servers.
            return false;
        }

        public static X509Certificate SelectLocalCertificate(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssures)
        {
            //Console.WriteLine(sender.ToString());
            //return localCertificates[0];
            //return serverCertificate;
            return serverCertificate2;
            //return null;
        }

		static void ProcessClient(TcpClient client)
		{
			// A client has connected. Create the 
			// SslStream using the client's network stream.
			//SslStream sslStream = new SslStream(
			//	client.GetStream(), false);
            SslStream sslStream = new SslStream(
                client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(ValidateClientCertificate),
                new LocalCertificateSelectionCallback(SelectLocalCertificate)
                );
            // Authenticate the server but don't require the client to authenticate.
			try
			{
                sslStream.AuthenticateAsServer(new X509Certificate(),
                    true, SslProtocols.Default, true);
                //sslStream.AuthenticateAsServer(serverCertificate2,
                //    true, SslProtocols.Tls, true);
                // Display the properties and settings for the authenticated stream.
				DisplaySecurityLevel(sslStream);
				DisplaySecurityServices(sslStream);
				DisplayCertificateInformation(sslStream);
				DisplayStreamProperties(sslStream);

				// Set timeouts for the read and write to 5 seconds.
				sslStream.ReadTimeout = 5000;
				sslStream.WriteTimeout = 5000;
				// Read a message from the client.   
				Console.WriteLine("Waiting for client message...");
				string messageData = ReadMessage(sslStream);
				Console.WriteLine("Received: {0}", messageData);

				// Write a message to the client.
				byte[] message = Encoding.UTF8.GetBytes("Hello from the server.<EOF>");
				Console.WriteLine("Sending hello message.");
				sslStream.Write(message);
			}
			catch (AuthenticationException e)
			{
				System.IO.File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "error2.txt", e.ToString());

				Console.WriteLine("Exception: {0}", e.Message);
				if (e.InnerException != null)
				{
					Console.WriteLine("Inner exception: {0}", e.InnerException.Message);
				}
				Console.WriteLine("Authentication failed - closing the connection.");
				sslStream.Close();
				client.Close();
				return;
			}
			finally
			{
				// The client stream will be closed with the sslStream
				// because we specified this behavior when creating
				// the sslStream.
				sslStream.Close();
				client.Close();
			}
		}

		static string ReadMessage(SslStream sslStream)
		{
			// Read the  message sent by the client.
			// The client signals the end of the message using the
			// "<EOF>" marker.
			byte[] buffer = new byte[2048];
			StringBuilder messageData = new StringBuilder();
			int bytes = -1;
			do
			{
				// Read the client's test message.
				bytes = sslStream.Read(buffer, 0, buffer.Length);

				// Use Decoder class to convert from bytes to UTF8
				// in case a character spans two buffers.
				Decoder decoder = Encoding.UTF8.GetDecoder();
				char[] chars = new char[decoder.GetCharCount(buffer, 0, bytes)];
				decoder.GetChars(buffer, 0, bytes, chars, 0);
				messageData.Append(chars);
				// Check for EOF or an empty message.
				if (messageData.ToString().IndexOf("<EOF>") != -1)
				{
					break;
				}
			} while (bytes != 0);

			return messageData.ToString();
		}
		static void DisplaySecurityLevel(SslStream stream)
		{
			Console.WriteLine("Cipher: {0} strength {1}", stream.CipherAlgorithm, stream.CipherStrength);
			Console.WriteLine("Hash: {0} strength {1}", stream.HashAlgorithm, stream.HashStrength);
			Console.WriteLine("Key exchange: {0} strength {1}", stream.KeyExchangeAlgorithm, stream.KeyExchangeStrength);
			Console.WriteLine("Protocol: {0}", stream.SslProtocol);
		}
		static void DisplaySecurityServices(SslStream stream)
		{
			Console.WriteLine("Is authenticated: {0} as server? {1}", stream.IsAuthenticated, stream.IsServer);
			Console.WriteLine("IsSigned: {0}", stream.IsSigned);
			Console.WriteLine("Is Encrypted: {0}", stream.IsEncrypted);
		}
		static void DisplayStreamProperties(SslStream stream)
		{
			Console.WriteLine("Can read: {0}, write {1}", stream.CanRead, stream.CanWrite);
			Console.WriteLine("Can timeout: {0}", stream.CanTimeout);
		}
		static void DisplayCertificateInformation(SslStream stream)
		{
			Console.WriteLine("Certificate revocation list checked: {0}", stream.CheckCertRevocationStatus);

			X509Certificate localCertificate = stream.LocalCertificate;
			if (stream.LocalCertificate != null)
			{
				Console.WriteLine("Local cert was issued to {0} and is valid from {1} until {2}.",
					localCertificate.Subject,
					localCertificate.GetEffectiveDateString(),
					localCertificate.GetExpirationDateString());
			}
			else
			{
				Console.WriteLine("Local certificate is null.");
			}
			// Display the properties of the client's certificate.
			X509Certificate remoteCertificate = stream.RemoteCertificate;
			if (stream.RemoteCertificate != null)
			{
				Console.WriteLine("Remote cert was issued to {0} and is valid from {1} until {2}.",
					remoteCertificate.Subject,
					remoteCertificate.GetEffectiveDateString(),
					remoteCertificate.GetExpirationDateString());
			}
			else
			{
				Console.WriteLine("Remote certificate is null.");
			}
		}
	}
}
