
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

using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Net;

using FlowLib.Utils;
using FlowLib.Connections;
using FlowLib.Containers;
using FlowLib.Managers;
using FlowLib.Protocols;
using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Containers.UPnP;
using FlowLib.Containers.UPnP.Services;

namespace FlowLib.Utils.Connection
{
    public class Detect : IUPnPUpdater
    {
		public delegate void ProgressChange(Detect sender, Functions prog);

		[Flags]
		public enum WorkMethod
		{
			NoThread,
			ThreadInForeground,
			ThreadInBackground
		}

		[Flags]
        public enum ConnectionMode
        {
            None = 0,
            Passive = 1,
            UPnP = 2,
            DirectConnection = 4
        }
		[Flags]
        public enum Functions
        {
            Start = 0,
            InternetAccess = 1,
            InternalListenAccess = 2,
            InternalRecieveAccess = 4,
            ExternalIp = 8,
            ExternalRecieveAccess = 16,
            UPnPDevices = 32,
            UPnPIGD = 64,
            UPnPExternalIp = 128,
            UPnPAddMapping = 256,
            UPnPGetMapping = 512,
            UPnPDeleteMapping = 1024,
            UPnPExternalRecieveAccess = 2048,
            End = 4096
        }

        #region Variables/Properties
		public event ProgressChange ProgressChanged;
		public event ProgressChange SuccessChanged;
        public event FmdcEventHandler UpdateBase;
        protected Thread workingThread = null;
        protected TcpConnectionListener tcpListener;
        protected TransferManager transferManager = new TransferManager();
		protected Functions fSuccess = Functions.Start;
		protected Functions fProgress = Functions.Start;
        protected IUPnP upnp = null;
        protected WANIPConnectionService wanipService = null;
        protected WANIPConnectionService.PortMapping mapping = null;

        public Functions Success
        {
			get { return fSuccess; }
			protected set
			{
				fSuccess = value;
				SuccessChanged(this, fSuccess);
			}
        }

        public Functions Progress
        {
			get { return fProgress; }
			protected set
			{
				fProgress = value;
				ProgressChanged(this, fProgress);
			}
        }

        public IPAddress ExternalIP
        {
            get;
            protected set;
        }

        public IPAddress ExternalIPUPnP
        {
            get;
            protected set;
        }

        public IPAddress InternalIP
        {
            get;
            set;
        }

        /// <summary>
        /// Default Port = 31173
        /// </summary>
        public int Port
        {
            get;
            set;
        }

		public WorkMethod WorkingMethod
		{
			get;
			protected set;
		}

        /// <summary>
        /// 0 = No Internet access?
        /// 1 = Passive mode
        /// 2 = Active mode
        /// 4 = Active mode through UPnP
        /// </summary>
        public int ConnectionType
        {
            get
            {
                if
                    (
                    (Functions.ExternalRecieveAccess & Success) == Functions.ExternalRecieveAccess
                    && (Functions.InternalListenAccess & Success) == Functions.InternalListenAccess
                    && (Functions.InternalRecieveAccess & Success) == Functions.InternalRecieveAccess
                    && (Functions.ExternalIp & Success) == Functions.ExternalIp
                    && (Functions.InternetAccess & Success) == Functions.InternetAccess
                    )
                {
                    return 2;   // Active
                }
                else if 
                    (
                    (Functions.UPnPDevices & Success) == Functions.UPnPDevices
                    && (Functions.UPnPIGD & Success) == Functions.UPnPIGD
                    && (Functions.UPnPExternalIp & Success) == Functions.UPnPExternalIp
                    && (Functions.UPnPAddMapping & Success) == Functions.UPnPAddMapping
                    && (Functions.UPnPGetMapping & Success) == Functions.UPnPGetMapping
                    && (Functions.UPnPDeleteMapping & Success) == Functions.UPnPDeleteMapping
                    && (Functions.UPnPExternalRecieveAccess & Success) == Functions.UPnPExternalRecieveAccess
                    )
                {
                    return 4;   // UPnP Active
                }
                else if ((Functions.InternetAccess & Success) == Functions.InternetAccess)
                {
                    return 1;   // Passive
                }
                else
                {
                    return 0;   // No internet access
                }
            }
        }

        #endregion

        public Detect()
        {
			ProgressChanged = new ProgressChange(OnProgressChanged);
			SuccessChanged = new ProgressChange(OnSuccessChanged);
            UpdateBase = new FmdcEventHandler(OnUpdateBase);
            upnp = new UPnP(this);
            upnp.ProtocolUPnP.Update += new FmdcEventHandler(OnUPnPUpdate);
            Port = 31173;
        }


        void OnUpdateBase(object sender, FmdcEventArgs e) { }
		protected void OnProgressChanged(Detect sender, Detect.Functions prog) { }
		protected void OnSuccessChanged(Detect sender, Detect.Functions prog) { }
        public void FireUpdateBase(FmdcEventArgs e)
        {
            UpdateBase(this, e);
        }

        /// <summary>
		/// Start detection on a background thread
		/// </summary>
		public void Start()
		{
			Start(WorkMethod.ThreadInBackground);
		}

		public void Start(WorkMethod method)
        {
            Progress = Functions.Start;
            Success = Functions.Start;
			WorkingMethod = method;

			switch (WorkingMethod)
			{
				case WorkMethod.NoThread:
					OnWorking();
					break;
				case WorkMethod.ThreadInForeground:
				case WorkMethod.ThreadInBackground:
					workingThread = new Thread(new ThreadStart(OnWorking));
					switch (WorkingMethod)
					{
						case WorkMethod.ThreadInBackground:
							workingThread.IsBackground = true;
							break;
					}
					workingThread.Start();
					break;
			}

        }

        protected void OnWorking()
        {
            try
            {
                do
                {
                    DoWork();
                    Thread.Sleep(1);
                } while ((Functions.End & Progress) != Functions.End);
            }
            catch (System.Threading.ThreadAbortException)
            {
                // User have requested that detection should stop.
            }
        }

        protected virtual void DoWork()
        {
            // Select method to work on.
            switch (Progress)
            {
                case Functions.Start:
                    Progress = Functions.InternetAccess;
                    break;
                case Functions.InternetAccess:
                    DoInternetAccess();
                    break;
                case Functions.InternalListenAccess:
                    DoInternalListenRecieveAccess();
                    break;
                case Functions.ExternalIp:
                    DoExternalIp();
                    break;
                case Functions.ExternalRecieveAccess:
                    DoExternalRecieveAccess();
                    break;
                case Functions.UPnPDevices:
                    DoUPnPDevices();
                    break;
                case Functions.UPnPIGD:
                    DoUPnPIGD();
                    break;
                case Functions.UPnPExternalRecieveAccess:
                    DoUPnPExternalRecieveAccess();
                    break;
                case Functions.End:
                    break;
            }
        }

        protected virtual void DoInternetAccess()
        {
            try
            {
                string content = null;
                if ((content = WebOperations.GetPage("http://www.google.com")) != string.Empty && content.IndexOf("google", StringComparison.OrdinalIgnoreCase) != -1)
                {
                    Success |= Functions.InternetAccess;
                }
            }
            finally
            {
                Progress = Functions.InternalListenAccess;
            }
        }
        protected virtual void DoInternalListenRecieveAccess()
        {
            try
            {
                tcpListener = new TcpConnectionListener(Port);
                tcpListener.Update += new FmdcEventHandler(DoInternalListenAccess_Update);
                tcpListener.Start();
                try
                {
                    // TODO: We shouldn't limit ourself to IPv4. 
                    Transfer transfer = new Transfer("127.0.0.1", Port);
                    UserInfo me = new UserInfo();
                    me.DisplayName = "loopback";
                    me.TagInfo.Version = "FlowLibPowered";
                    transfer.Share = new Share("temp");
                    transfer.Me = me;
                    // TODO: We shouldn't limit ourself to IPv4. 
                    transfer.Source = new Source("127.0.0.1", "loopback");
                    transferManager.AddTransferReq(new TransferRequest("loopback", null, new UserInfo()));

                    transfer.Protocol = new TransferNmdcProtocol(transfer);
                    transferManager.StartTransfer(transfer);
                    Success |= Functions.InternalListenAccess;
                }
                catch
                {
                    // We do not have InternalListenAccess.
                    Success = Success & ~Functions.InternalListenAccess;
                }

                // Wait 10 seconds before continue
                int i = 0;
                do
                {
                    Thread.Sleep(50);
                } while (((Functions.InternalRecieveAccess & Success) != Functions.InternalRecieveAccess) && i++ > 200);


                // clean;
                tcpListener.Update -= DoInternalListenAccess_Update;
                tcpListener.End();
                tcpListener = null;
            }
            finally
            {
                Progress = Functions.ExternalIp;
            }
        }
        protected virtual void DoExternalIp()
        {
            try
            {
                string content = null;
				// TODO: Check if whatismyip support IPv6
				if ((content = WebOperations.GetPage("http://whatismyip.org")) != string.Empty)
                {
					// TODO: We shouldn't limit ourself to IPv4. 
					if (Regex.IsMatch(content, @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"))
                    {
                        try
                        {
                            ExternalIP = IPAddress.Parse(content);
                            Success |= Functions.ExternalIp;
                        }
                        catch { }
                    }
                }
            }
            finally
            {
                Progress = Functions.ExternalRecieveAccess;
            }
        }
        protected virtual void DoExternalRecieveAccess()
        {
            try
            {
                tcpListener = new TcpConnectionListener(Port);
                tcpListener.Update += new FmdcEventHandler(DoExternalRecieveAccess_Update);
                tcpListener.Start();

                // Don't do this if we don't have external ip
                if ((Functions.ExternalIp & Success) == Functions.ExternalIp)
                {
                    Transfer transfer = new Transfer(ExternalIP.ToString(), Port);
                    UserInfo me = new UserInfo();
                    me.DisplayName = "external";
                    me.TagInfo.Version = "FlowLibPowered";
                    transfer.Share = new Share("temp");
                    transfer.Me = me;
                    transfer.Source = new Source(ExternalIP.ToString(), "external");
                    transferManager.AddTransferReq(new TransferRequest("external", null, new UserInfo()));

                    transfer.Protocol = new TransferNmdcProtocol(transfer);
                    transferManager.StartTransfer(transfer);
                }

                // Wait 30 seconds before continue
                int i = 0;
                do
                {
                    Thread.Sleep(50);
                } while (((Functions.ExternalRecieveAccess & Success) != Functions.ExternalRecieveAccess) && i++ > 600);

                // clean;
                tcpListener.Update -= DoExternalRecieveAccess_Update;
                tcpListener.End();
                tcpListener = null;
            }
            finally
            {
                if ((Functions.ExternalRecieveAccess & Success) == Functions.ExternalRecieveAccess)
                {
                    Progress = Functions.End;
                }
                else
                {
                    Progress = Functions.UPnPDevices;
                }
            }
        }

        protected virtual void OnUPnPUpdate(object sender, FmdcEventArgs e)
        {
            IUPnP upnp = sender as IUPnP;
            FlowLib.Containers.UPnP.UPnPDevice device = e.Data as FlowLib.Containers.UPnP.UPnPDevice;
            if (upnp != null && device != null)
            {
                switch (e.Action)
                {
                    case Actions.UPnPRootDeviceFound:
                        //FlowLib.Events.FmdcEventArgs e3 = new FlowLib.Events.FmdcEventArgs(Actions.UPnPDeviceDescription, device.Information.Sender.ToString());
                        //UpdateBase(this, e3);
                        Success |= Functions.UPnPDevices;
                        break;
                    case Actions.UPnPDeviceUpdated:
                        foreach (ServiceBase service in device.Services)
                        {
                            if (WANIPConnectionService.IsMatching(service))
                            {
                                #region Retreive internal ip
                                if (this.InternalIP == null)
                                {
                                    string routerIP = ((IPEndPoint)device.Information.Sender).Address.ToString();
                                    string[] secRouter = routerIP.Split('.', ':');
                                    System.Net.IPHostEntry hostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                                    if (hostEntry != null)
                                    {
                                        System.Net.IPAddress[] collectionOfIPs = hostEntry.AddressList;
                                        for (int i = 0; i < collectionOfIPs.Length; i++)
                                        {
                                            System.Net.IPAddress ip = collectionOfIPs[i];
                                            string[] secIP = ip.ToString().Split('.', ':');
                                            bool found = true;

                                            for (int y = 0; y < secRouter.Length - 1; y++)
                                            {
                                                if (secIP.Length > y)
                                                {
                                                    if (!secRouter[y].Equals(secIP[y]))
                                                    {
                                                        found = false;
                                                        break;
                                                    }
                                                }
                                            }
                                            if (found)
                                            {
                                                InternalIP = ip;
                                            }
                                        }
                                    }

                                }
                                #endregion
                                if (InternalIP != null)
                                {
                                    wanipService = new WANIPConnectionService(service);
                                    #region GetExternalIPAddress
                                    try
                                    {
                                        Progress = Functions.UPnPExternalIp;
                                        IPAddress tmpAddress = wanipService.GetExternalIPAddress(this);
                                        if (tmpAddress != null)
                                        {
                                            // Is ExternalIP different then the one we got from router?
                                            ExternalIPUPnP = tmpAddress;
                                            if (ExternalIP == null)
                                                ExternalIP = ExternalIPUPnP;
                                            Success |= Functions.UPnPExternalIp;
                                        }
                                    }
                                    catch { }
                                    #endregion
                                    #region AddPortMapping
                                    Progress = Functions.UPnPAddMapping;
                                    mapping = new WANIPConnectionService.PortMapping(
                                        string.Empty,
                                        Port,
                                        "TCP",
                                        Port,
                                        InternalIP.ToString(),
                                        true,
                                        "FlowLibPowered - Connection Detection",
                                        /*"FlowLibPowered",*/
                                        0
                                        );
                                    bool hasAddPortMapping = false;
                                    if (hasAddPortMapping = wanipService.AddPortMapping(this, mapping))
                                    {
                                        Success |= Functions.UPnPAddMapping;

                                    }
                                    #endregion
                                    #region GetSpecificPortMappingEntry
                                    Progress = Functions.UPnPGetMapping;
                                    if (hasAddPortMapping)
                                    {
                                        if (wanipService.GetSpecificPortMappingEntry(this, ref mapping))
                                        {
                                            Success |= Functions.UPnPGetMapping;
                                        }
                                    }
                                    #endregion
                                    #region DeletePortMapping
                                    Progress = Functions.UPnPDeleteMapping;
                                    if (hasAddPortMapping)
                                    {
                                        if (wanipService.DeletePortMapping(this, mapping))
                                        {
                                            Success |= Functions.UPnPDeleteMapping;
                                        }
                                    }
                                    #endregion
                                }
                            }
                            Success |= Functions.UPnPIGD;
                            //Progress = Functions.End;
                        }
                        break;
                }
            }
        }

        protected virtual void DoUPnPDevices()
        {
            try
            {
                // Wait 20 seconds before continue
                int i = 0;
                do
                {
                    upnp.Discover();
                    Thread.Sleep(1 * 1000);
                } while (((Functions.UPnPDevices & Success) != Functions.UPnPDevices) && i++ > 20);
            }
            finally
            {
                //if ((Functions.UPnPDevices & Success) == Functions.UPnPDevices)
                //{
                    Progress = Functions.UPnPIGD;
                //}
                //else
                //{
                //    Progress = Functions.End;
                //}
            }
        }

        protected virtual void DoUPnPIGD()
        {
            try
            {
                // Wait 20 seconds before continue
                int i = 0;
                do
                {
                    Thread.Sleep(1 * 1000);
                } while (((Functions.UPnPIGD & Success) != Functions.UPnPIGD) && i++ > 20);
                System.Collections.Generic.SortedList<string, UPnPDevice> tmpDevices = new System.Collections.Generic.SortedList<string, UPnPDevice>(upnp.RootDevices);
                foreach (System.Collections.Generic.KeyValuePair<string, UPnPDevice> devicePair in tmpDevices)
                {
                    FlowLib.Events.FmdcEventArgs e = new FlowLib.Events.FmdcEventArgs(Actions.UPnPDeviceDescription, devicePair.Key);
                    UpdateBase(this, e);
                }
            }
            finally
            {
                //if ((Functions.UPnPIGD & Success) == Functions.UPnPIGD)
                //{
                    Progress = Functions.UPnPExternalRecieveAccess;
                //}
                //else
                //{
                //    Progress = Functions.End;
                //}
            }
        }

        protected virtual void DoUPnPExternalRecieveAccess()
        {
            try
            {
                tcpListener = new TcpConnectionListener(Port);
                tcpListener.Update += new FmdcEventHandler(DoUPnPExternalRecieveAccess_Update);
                tcpListener.Start();

                // Don't do this if we don't have external ip
                if (
                    (Functions.UPnPDevices & Success) == Functions.UPnPDevices
                    && (Functions.UPnPIGD & Success) == Functions.UPnPIGD
                    && (Functions.UPnPExternalIp & Success) == Functions.UPnPExternalIp
                    && (Functions.UPnPAddMapping & Success) == Functions.UPnPAddMapping
                    && (Functions.UPnPGetMapping & Success) == Functions.UPnPGetMapping
                    && (Functions.UPnPDeleteMapping & Success) == Functions.UPnPDeleteMapping
                    )
                {
                    bool hasAddPortMapping = false;
                    if (hasAddPortMapping = wanipService.AddPortMapping(this, mapping))
                    {

                        //Transfer transfer = new Transfer(ExternalIPUPnP.ToString(), Port);
                        Transfer transfer = new Transfer(ExternalIP.ToString(), Port);
                        UserInfo me = new UserInfo();
                        me.DisplayName = "upnp";
                        me.TagInfo.Version = "FlowLibPowered";
                        transfer.Share = new Share("temp");
                        transfer.Me = me;
                        transfer.Source = new Source(ExternalIP.ToString(), "upnp");
                        transferManager.AddTransferReq(new TransferRequest("upnp", null, new UserInfo()));

                        transfer.Protocol = new TransferNmdcProtocol(transfer);
                        transferManager.StartTransfer(transfer);

                        // Wait 60 seconds before continue
                        int i = 0;
                        do
                        {
                            Thread.Sleep(50);
                        } while (((Functions.UPnPExternalRecieveAccess & Success) != Functions.UPnPExternalRecieveAccess) && i++ > 1200);

                        // clean;
                        wanipService.DeletePortMapping(this, mapping);
                    }
                }

                tcpListener.Update -= DoUPnPExternalRecieveAccess_Update;
                tcpListener.End();
                tcpListener = null;
            }
            finally
            {
                Progress = Functions.End;
            }
        }
        protected void DoUPnPExternalRecieveAccess_Update(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(DoUPnPExternalRecieveAccess_RequestTransfer);
                        e.Handled = true;
                    }
                    break;
            }
        }

        protected virtual void DoUPnPExternalRecieveAccess_RequestTransfer(object sender, FmdcEventArgs e)
        {
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (req != null)
            {
                switch (req.Key)
                {
                    case "upnp":
                        Success |= Functions.UPnPExternalRecieveAccess;
                        break;
                }
                transferManager.RemoveTransferReq(req.Key);
            }
        }

        protected void DoExternalRecieveAccess_Update(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }
                        trans.Protocol.RequestTransfer += new FmdcEventHandler(DoExternalRecieveAccess_RequestTransfer);
                        e.Handled = true;
                    }
                    break;
            }
        }

        protected virtual void DoExternalRecieveAccess_RequestTransfer(object sender, FmdcEventArgs e)
        {
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (req != null)
            {
                switch (req.Key)
                {
                    case "external":
                        Success |= Functions.ExternalRecieveAccess;
                        break;
                }
                transferManager.RemoveTransferReq(req.Key);
            }
        }
        protected virtual void DoInternalListenAccess_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }

                        trans.Protocol.RequestTransfer += new FmdcEventHandler(DoInternalListenAccess_RequestTransfer);
                        e.Handled = true;
                    }
                    break;
            }
        }

        protected virtual void DoInternalListenAccess_RequestTransfer(object sender, FmdcEventArgs e)
        {
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (req != null)
            {
                switch (req.Key)
                {
                    case "loopback":
                        Success |= Functions.InternalRecieveAccess;
                        break;
                }
                transferManager.RemoveTransferReq(req.Key);
            }
        }

        public void Stop()
        {
            if (workingThread != null)
                workingThread.Abort();
        }
    }
}
