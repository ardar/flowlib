
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
        public enum Problems
        {
            None = 0,
            DifferentPage = 1 /* Do you need to login to access internet? */
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
            End = 2048
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

        /// <summary>
        /// Default Port = 31773
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
        #endregion

        public Detect()
        {
			ProgressChanged = new ProgressChange(OnProgressChanged);
			SuccessChanged = new ProgressChange(OnSuccessChanged);
            UpdateBase = new FmdcEventHandler(OnUpdateBase);
            upnp = new UPnP(this);
            upnp.ProtocolUPnP.Update += new FmdcEventHandler(OnUPnPUpdate);
            Port = 31773;
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
                case Functions.End:
                    break;
            }
        }

        protected void DoInternetAccess()
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
        protected void DoInternalListenRecieveAccess()
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
        protected void DoExternalIp()
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
        protected void DoExternalRecieveAccess()
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

                // Wait 10 seconds before continue
                int i = 0;
                do
                {
                    Thread.Sleep(50);
                } while (((Functions.ExternalRecieveAccess & Success) != Functions.ExternalRecieveAccess) && i++ > 200);

                // clean;
                tcpListener.Update -= DoInternalListenAccess_Update;
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

        protected void OnUPnPUpdate(object sender, FmdcEventArgs e)
        {
            IUPnP upnp = sender as IUPnP;
            FlowLib.Containers.UPnP.UPnPDevice device = e.Data as FlowLib.Containers.UPnP.UPnPDevice;
            if (upnp != null && device != null)
            {
                switch (e.Action)
                {
                    case Actions.UPnPRootDeviceFound:
                        Success |= Functions.UPnPDevices;
                        break;
                    case Actions.UPnPDeviceUpdated:
                        foreach (ServiceBase service in device.Services)
                        {
                            if (WANIPConnectionService.IsMatching(service))
                            {
                                Success |= Functions.UPnPIGD;
                                WANIPConnectionService wanipService = new WANIPConnectionService(service);
                                #region GetExternalIPAddress
                                try
                                {
                                    Progress = Functions.UPnPExternalIp;
                                    IPAddress tmpAddress = wanipService.GetExternalIPAddress(this);
                                    if (tmpAddress != null)
                                    {
                                        ExternalIP = tmpAddress;
                                        Success |= Functions.UPnPExternalIp;
                                    }
                                }
                                catch { }
                                #endregion
                                #region AddPortMapping
                                Progress = Functions.UPnPAddMapping;
                                WANIPConnectionService.PortMapping mapping = new WANIPConnectionService.PortMapping(
                                    string.Empty,
                                    Port,
                                    "TCP",
                                    Port,
                                    string.Empty,
                                    true,
                                    /*"FlowLibPowered - Connection Detection",*/
                                    "FlowLibPowered",
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
                        break;
                }
            }
        }

        protected void DoUPnPDevices()
        {
            try
            {
                // Wait 10 seconds before continue
                int i = 0;
                do
                {
                    upnp.Discover();
                    Thread.Sleep(1 * 1000);
                } while (((Functions.UPnPDevices & Success) != Functions.UPnPDevices) && i++ > 10);
            }
            finally
            {
                if ((Functions.UPnPDevices & Success) == Functions.UPnPDevices)
                {
                    Progress = Functions.End;
                }
                else
                {
                    Progress = Functions.UPnPIGD;
                }
            }
        }

        protected void DoUPnPIGD()
        {
            try
            {
                System.Collections.Generic.SortedList<string, UPnPDevice> tmpDevices = new System.Collections.Generic.SortedList<string, UPnPDevice>(upnp.RootDevices);
                foreach (System.Collections.Generic.KeyValuePair<string, UPnPDevice> devicePair in tmpDevices)
                {
                    FlowLib.Events.FmdcEventArgs e = new FlowLib.Events.FmdcEventArgs(Actions.UPnPDeviceDescription, devicePair.Key);
                    UpdateBase(this, e);
                }
            }
            finally
            {
                Progress = Functions.End;
            }
        }

        void DoExternalRecieveAccess_Update(object sender, FmdcEventArgs e)
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
        void DoExternalRecieveAccess_RequestTransfer(object sender, FmdcEventArgs e)
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
        void DoInternalListenAccess_Update(object sender, FlowLib.Events.FmdcEventArgs e)
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
        void DoInternalListenAccess_RequestTransfer(object sender, FmdcEventArgs e)
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
            workingThread.Abort();
        }
    }
}
