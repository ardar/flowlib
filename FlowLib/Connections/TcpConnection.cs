
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using FlowLib.Interfaces;
using FlowLib.Containers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Utils;
// For user Handling
using System.Collections.Generic;
// For Connection
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

#if !COMPACT_FRAMEWORK
// Security
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using FlowLib.Containers.Security;
using FlowLib.Enums;
#endif

#if COMPACT_FRAMEWORK
using FlowLib.Utils.CompactFramworkExtensionMethods;
#endif

namespace FlowLib.Connections
{
    /// <summary>
    /// Class represents a Tcp/Ip connection supporting diffrent protocols
    /// </summary>
    public abstract class TcpConnection : IConnection, IDisposable
    {
        /// <summary>
        /// A connection is trying to be made
        /// </summary>
        public const int Connecting = 0;
        /// <summary>
        /// Connection has been made
        /// </summary>
        public const int Connected = 1;
        /// <summary>
        /// Connection has been disconnected
        /// </summary>
        public const int Disconnected = 2;

        #region Events
        /// <summary>
        /// Is triggered when protocol has been changed.
        /// Data is the new Protocol.
        /// </summary>
        public event FmdcEventHandler ProtocolChange;
        /// <summary>
        /// Is triggered when connection status has been changed.
        /// Action is:
        /// 0 = Connecting
        /// 1 = Connected
        /// 2 = Disconnected
        /// </summary>
        public event FmdcEventHandler ConnectionStatusChange;
#if !COMPACT_FRAMEWORK
// Security
        public event FmdcEventHandler SecureUpdate;
#endif
        #endregion
        #region Variables
        protected bool importedSocket = false;
        protected IPEndPoint remoteAddress = null;
        protected IPEndPoint localAddress = null;
        protected Socket socket;
        protected byte[] buffer = new byte[1024];
        protected IProtocol protocol = null;
        protected bool first = true;
        protected bool disposed = false;
        protected bool disposing = false;

#if !COMPACT_FRAMEWORK
// Security
        protected SslStream secStream = null;
        protected SecureProtocols secProtocol = SecureProtocols.None;
#endif

        // Thread signal, It is needed so we know when we have a connection.
        protected System.Threading.ManualResetEvent allDone = new System.Threading.ManualResetEvent(false);

        #endregion
        #region Properties

#if !COMPACT_FRAMEWORK
// Security
        public SslStream SecureStream
        {
            get { return secStream; }
            set { secStream = value; }
        }

        public SecureProtocols SecureProtocol
        {
            get { return secProtocol; }
            set
            {
                secProtocol = value;
                switch (secProtocol)
                {
                    case SecureProtocols.None:
                        if (secStream != null)
                            secStream.Close();
                        secStream = null;
                        socket.Blocking = false;
                        break;
                    default:
                        if (socket == null || !socket.Connected)
                            return;
                        CheckSecure();
                        break;
                }
            }
        }


        protected void CheckSecure()
        {
            try
            {
                if (secProtocol != SecureProtocols.None && secStream == null)
                {
                    // This is ugly as hell! :p
                    // TODO : Make this less ugly
                    TcpClient client = new TcpClient();
                    socket.Blocking = true;
                    client.Client = socket;
                    secStream = new SslStream(
                        client.GetStream(),
                        false,
                        new RemoteCertificateValidationCallback(OnRemoteCertificateValidation),
                        new LocalCertificateSelectionCallback(OnLocalCertificateSelection)
                        );

                    SslProtocols protocol = SslProtocols.None;
                    switch (secProtocol)
                    {
                        case SecureProtocols.SSL2:
                            protocol = SslProtocols.Ssl2;
                            break;
                        case SecureProtocols.SSL3:
                            protocol = SslProtocols.Ssl3;
                            break;
                        case SecureProtocols.TLS:
                            protocol = SslProtocols.Tls;
                            break;
                        default:
                            throw new NotSupportedException("This protocol '" + secProtocol + "' is not supported");
                    }

                    if (importedSocket)
                        secStream.AuthenticateAsServer(new X509Certificate(), true, protocol, true);
                    else
                        secStream.AuthenticateAsClient("", new X509CertificateCollection(), protocol, true);
                }
            }
            catch (AuthenticationException ae)
            {
                SecureUpdate(this, new FmdcEventArgs(Actions.SecurityAuthenticationError, ae));
            }
            catch (PlatformNotSupportedException pe)
            {
                SecureUpdate(this, new FmdcEventArgs(Actions.SecurityAuthenticationError, pe));
            }

        }
#endif
        public bool IsDisposed
        {
            get { return disposed; }
        }

        public bool IsDisposing
        {
            get { return disposing; }
        }

        /// <summary>
        /// Sharing instance for this transfer
        /// </summary>
        public abstract Share Share
        {
            get;
            set;
        }
        /// <summary>
        /// Protocol that is beeing used for this connection.
        /// </summary>
        public IProtocol Protocol
        {
            get { return protocol; }
            set
            {
                IProtocol tmp = protocol;
                protocol = value;
                // Trigger Protocol Changed Event.
                FmdcEventArgs e = new FmdcEventArgs(0, tmp);
                ProtocolChange(this, e);
                if (!e.Handled && tmp != null)
                {
                    // If not marked as handled. Dispose protocol
                    tmp.Dispose();
                }
            }
        }
        /// <summary>
        /// Remote IPEndPoint for the other party
        /// </summary>
        public IPEndPoint RemoteAddress
        {
            get { return remoteAddress; }
        }
        /// <summary>
        /// Local IPEndPoint for our party
        /// </summary>
        public IPEndPoint LocalAddress
        {
            get {
                if (localAddress == null && socket != null)
                    return (IPEndPoint)socket.LocalEndPoint;
                return localAddress;
            }
            set { localAddress = value; }
        }

        #endregion
        #region Constructor(s)/Deconstructor/Dispose
        /// <summary>
        /// Creating TcpConnection
        /// </summary>
        public TcpConnection()
        {
            #region Event(s)
            ProtocolChange = new FmdcEventHandler(OnProtocolChanged);
            ConnectionStatusChange = new FmdcEventHandler(OnConnectionStatusChanged);
#if !COMPACT_FRAMEWORK
// Security
            SecureUpdate = new FmdcEventHandler(OnSecureUpdate);
#endif
            #endregion
        }

        /// <summary>
        /// Creating TcpConnection with s as the underlaying socket
        /// </summary>
        /// <param name="s">Socket you want to have as underlaying connection</param>
        public TcpConnection(Socket s)
            : this()
        {
            this.socket = s;
            importedSocket = true;
        }

        /// <summary>
        /// Creating TcpConnection with addy as the remote address.
        /// </summary>
        /// <param name="addy">IPEndPoint that we want as our remote address</param>
        public TcpConnection(System.Net.IPEndPoint addy)
            : this()
        {
            remoteAddress = addy;
        }

        /// <summary>
        /// Creating TcpConnection with address and prt.
        /// </summary>
        /// <param name="address">string representation of a IP/DNS address</param>
        /// <param name="prt">int port representation</param>
        public TcpConnection(string address, int prt)
            :this()
        {
            System.Net.IPAddress addy = null;
            try
            {
                //addy = System.Net.Dns.GetHostEntry(address).AddressList[0];
                addy = System.Net.IPAddress.Parse(address);
            }
            catch (System.Exception)
            {
                // We are not going to try to catch this as developer that used this class made something wrong if this has to be thrown.
                //addy = System.Net.IPAddress.Parse(address);
                addy = System.Net.Dns.GetHostEntry(address).AddressList[0];
            }
            remoteAddress = new IPEndPoint(addy, prt);
        }

        /// <summary>
        /// Deconstructor
        /// </summary>
        ~TcpConnection()
        {
            Dispose();
        }

        public virtual void Dispose()
        {
            if (!disposed)
            {
                disposing = true;
                ProtocolChange -= OnProtocolChanged;
                ConnectionStatusChange -= OnConnectionStatusChanged;
#if !COMPACT_FRAMEWORK
                SecureUpdate -= OnSecureUpdate;
#endif
                allDone.Close();
                buffer = null;
                localAddress = null;
                remoteAddress = null;
                Disconnect("Dispose");
                if (socket != null)
                {
                    socket.Close();
                    socket = null;
                }
                if (protocol != null)
                {
                    protocol.Dispose();
                    protocol = null;
                }
#if !COMPACT_FRAMEWORK
                if (secStream != null)
                {
                    secStream.Dispose();
                    secStream = null;
                }
#endif
                GC.SuppressFinalize(this);
                disposed = true;
                disposing = false;
            }
        }

        #endregion
        #region Functions
        #region Connect
        /// <summary>
        /// Creates connection to server.
        /// </summary>
        public virtual void Connect()
        {
            // Change Connection status.
            this.ConnectionStatusChange(this, new FmdcEventArgs(Connecting));
            first = true;
            if (Protocol == null)
                throw new NullReferenceException("Protocol is null, You need to set Protocol before trying to connect.");
            try
            {
                // Establish Connection
                socket = new Socket(remoteAddress.AddressFamily
                    , SocketType.Stream
                    , ProtocolType.Tcp);
                // It is needed so we know when we have a connection.
                allDone.Reset();
                socket.Blocking = false;
                System.AsyncCallback onconnect = new System.AsyncCallback(OnConnect);
                socket.BeginConnect(remoteAddress, onconnect, socket);
                // Waits until we have a connection...
                allDone.WaitOne();
            }
            catch (System.Exception e2)
            {
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, e2));
                return;
            }
        }

        /// <summary>
        /// Disconnects connection
        /// </summary>
        public void Disconnect()
        {
            Disconnect(null);
        }

        /// <summary>
        /// Disconnects connection
        /// </summary>
        /// <param name="msg">Message that will be sent out in the ConnectionStatusChange event</param>
        public virtual void Disconnect(string msg)
        {
            try
            {
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, new FmdcException(msg)));

                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger,new LingerOption(false, 0));
                socket.Disconnect(true);
            }
            catch (Exception) { }
        }

        protected virtual void OnConnect(System.IAsyncResult ar)
        {
            // We have a working connection.. :)
            allDone.Set();

            // Socket was the passed in object
            Socket sock = (Socket)ar.AsyncState;
            // Check if we were sucessfull
            try
            {
                sock.EndConnect(ar);
                if (sock.Connected)
                {
                    // Change Connection Status
                    this.ConnectionStatusChange(this, new FmdcEventArgs(Connected));

#if !COMPACT_FRAMEWORK
                    CheckSecure();
#endif

                    SetupRecieveCallback(sock);
                }
                else
                {
                    // Change Connection Status
                    this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected));
                }
            }
            catch (ObjectDisposedException) { }
            catch (SocketException se)
            {
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, se));
            }
        }
        #endregion
        #region Receive
        /// <summary>
        /// Setup the callback for recieved data and loss of conneciton
        /// </summary>
        protected virtual void SetupRecieveCallback(Socket sock)
        {
            try
            {
                if (sock.Connected)
                {
                    // Determin Protocol to use here.
                    //if (Protocol != null && sock.Connected && first)
                    if (Protocol != null && first && !importedSocket)
                    {
                        first = false;
                        if (Protocol.FirstCommand != null)
                            Send(Protocol.FirstCommand);
                    }
                    AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
#if !COMPACT_FRAMEWORK
                    if (secStream != null)
                        secStream.BeginRead(buffer, 0, buffer.Length, recieveData, secStream);
                    else
#endif
                        sock.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, recieveData, sock);
                }
            }
            catch (System.ObjectDisposedException) { }
            catch (ArgumentNullException ane)
            {
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, ane));
            }
            catch (SocketException se)
            {
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, se));
            }
        }
        /// <summary>
        /// Get the new data and send it out to all other connections. 
        /// Note: If no data was recieved the connection has probably 
        /// died.
        /// </summary>
        /// <param name="ar"></param>
        protected virtual void OnRecievedData(System.IAsyncResult ar)
        {
            // Socket was the passed in object
            Socket sock = ar.AsyncState as Socket;
            int nBytesRec = 0;
            // Check if we got any data
            try
            {
#if !COMPACT_FRAMEWORK
                if (sock == null)
                {
                    SslStream handler = ar.AsyncState as SslStream;
                    nBytesRec = handler.EndRead(ar);
                    // TODO : REMOVE
                    sock = socket;
                }
                else
#endif
                    nBytesRec = sock.EndReceive(ar);
                if (nBytesRec > 0)
                {
                    // Send data to protocol.
                    Protocol.ParseRaw(buffer, nBytesRec);

                    // If the connection is still usable restablish the callback
                    SetupRecieveCallback(sock);
                }
                else
                {
                    // If no data was recieved then the connection is probably dead
                    // Change Connection Status
                    this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected));
                }
            }
            catch (System.ObjectDisposedException) { }
            catch (SocketException ex)
            {
                if (ex.ErrorCode != 10057)
                {
                    // Change Connection Status
                    this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, ex));
                }
            }
        }
        #endregion
        /// <summary>
        /// If existing connection exist it will be closed.
        /// Then a new connection attempt will be made
        /// </summary>
        public virtual void Reconnect()
        {
            // If connected, Disconnect.
            if (socket != null && socket.Connected)
                ConnectionStatusChange(this, new FmdcEventArgs(TcpConnection.Disconnected));
            // Connecting.
            Connect();
        }

        /// <summary>
        /// Sets TcpConnection in a listening mode waiting for a message.
        /// NOTE: This function can't be used if you havnt used the TcpConnection(Socket s) constructor.
        /// </summary>
        public virtual void Listen()
        {
            if (!importedSocket)
                throw new InvalidOperationException("To call this function you need to have created this object with the TcpConnection(Socket s) constructor");
            if (socket.Connected)
            {
                localAddress = (System.Net.IPEndPoint)socket.LocalEndPoint;
                remoteAddress = (System.Net.IPEndPoint)socket.RemoteEndPoint;

#if !COMPACT_FRAMEWORK
                CheckSecure();
#endif
                this.SetupRecieveCallback(socket);
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Connected));
            }
        }

        #region Send
        /// <summary>
        /// Sends Raw from msg to server.
        /// </summary>
        /// <param name="msg">Message where </param>
        public virtual void Send(IConMessage msg)
        {
            if (Protocol.OnSend(msg))
            {
                Send(msg.Bytes);
            }
        }
        /// <summary>
        /// Sends byte[] to server.
        /// </summary>
        /// <param name="raw">byte[] that will be sent to server</param>
        public virtual void Send(byte[] raw)
        {
            if (socket == null || !socket.Connected)
            {
                return;
            }
            try
            {
                if (raw == null || raw.Length <= 0)
                    return;

                // Some how Send doesnt work on my Pocket PC. Because of this we use BeginSend // Flow84
                AsyncCallback sendData = new AsyncCallback(OnSendData);
#if !COMPACT_FRAMEWORK
                if (secStream != null)
                    // TODO : Why cant we use async write here?! :S
                    secStream.Write(raw, 0, raw.Length);
                    //secStream.BeginWrite(raw, 0, raw.Length, sendData, secStream);
                else
#endif
                    socket.BeginSend(raw, 0, raw.Length, SocketFlags.None, sendData, socket);
                return;
            }
            catch (ObjectDisposedException) { }
            catch (SocketException se)
            {
                if (se.ErrorCode != 10057)
                {
                    // Change Connection Status
                    this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, se));
                }
            }
        }
        protected virtual void OnSendData(System.IAsyncResult async)
        {

            Socket handler = async.AsyncState as Socket;
            try
            {
#if !COMPACT_FRAMEWORK
                if (handler == null)
                {
                    SslStream handlerS = async.AsyncState as SslStream;
                    handlerS.EndWrite(async);
                }
                else
#endif
                    handler.EndSend(async);
            }
            catch (ObjectDisposedException) { }
            catch (SocketException se)
            {
                // Change Connection Status
                this.ConnectionStatusChange(this, new FmdcEventArgs(Disconnected, se));
            }
        }
        #endregion
        protected virtual void OnProtocolChanged(object sender, FmdcEventArgs e)
        {

        }
        protected virtual void OnConnectionStatusChanged(object sender, FmdcEventArgs e)
        {

        }


#if !COMPACT_FRAMEWORK
        // Secure
        void OnSecureUpdate(object sender, FmdcEventArgs e) { }
        protected bool OnRemoteCertificateValidation(
              object sender,
              X509Certificate certificate,
              X509Chain chain,
              SslPolicyErrors sslPolicyErrors)
        {
            CertificateValidationInfo info = new CertificateValidationInfo(certificate, chain, sslPolicyErrors);
            FmdcEventArgs args = new FmdcEventArgs(Actions.SecurityValidateRemoteCertificate, info);
            SecureUpdate(this, args);
            info = args.Data as CertificateValidationInfo;
            if (info == null)
                return false;
            return info.Accepted;
        }

        protected X509Certificate OnLocalCertificateSelection(
            object sender,
            string targetHost,
            X509CertificateCollection localCertificates,
            X509Certificate remoteCertificate,
            string[] acceptableIssures)
        {
            LocalCertificationSelectionInfo info = new LocalCertificationSelectionInfo(targetHost, localCertificates, remoteCertificate, acceptableIssures);
            FmdcEventArgs args = new FmdcEventArgs(Actions.SecuritySelectLocalCertificate, info);
            SecureUpdate(this, args);
            info = args.Data as LocalCertificationSelectionInfo;
            if (info == null)
                return null;
            return info.SelectedCertificate;
        }
#endif

        #endregion
    }
}
