
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

using FlowLib.Containers.UPnP;
using FlowLib.Interfaces;
using FlowLib.Events;

namespace FlowLib.Containers.UPnP.Services
{
    public class WANIPConnectionService : ServiceBase
    {
        public WANIPConnectionService(ServiceBase service) : base(service) { }

        /// <summary>
        /// This method returns true if service type is equal to WANIPConnection.
        /// It ignores version number as all later versions (after 1) should be compable with the first version according to UPnP Forum.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public static bool IsMatching(string serviceType)
        {
            if (!string.IsNullOrEmpty(serviceType))
            {
                return serviceType.StartsWith("urn:schemas-upnp-org:service:WANIPConnection:");
            }
            return false;
        }

        public static bool IsMatching(ServiceBase service)
        {
            return IsMatching(service.Information.serviceType);
        }

        /// <summary>
        /// This action retrieves NAT port mappings one entry at a time. Control points can call this action
        /// with an incrementing array index until no more entries are found on the gateway. If
        /// PortMappingNumberOfEntries is updated during a call, the process may have to start over.
        /// Entries in the array are contiguous. As entries are deleted, the array is compacted, and the
        /// evented variable PortMappingNumberOfEntries is decremented. Port mappings are logically
        /// stored as an array on the IGD and retrieved using an array index ranging from 0 to
        /// PortMappingNumberOfEntries-1.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public PortMapping GetGenericPortMappingEntry(IUPnPUpdater upnp, int index)
        {
            PortMapping mapping = new PortMapping();

            UPnPFunction func = new UPnPFunction();
            func.Name = "GetSpecificPortMappingEntry";
            func.Service = this;

            func.Arguments.Add("NewPortMappingIndex", index.ToString());
            func.Arguments.Add("NewRemoteHost", string.Empty);
            func.Arguments.Add("NewExternalPort", string.Empty);
            func.Arguments.Add("NewProtocol", string.Empty);
            func.Arguments.Add("NewInternalPort", string.Empty);
            func.Arguments.Add("NewInternalClient", string.Empty);
            func.Arguments.Add("NewEnabled", string.Empty);
            func.Arguments.Add("NewPortMappingDescription", string.Empty);
            func.Arguments.Add("NewLeaseDuration", string.Empty);

            FmdcEventArgs e = new FmdcEventArgs(Actions.UPnPFunctionCall, func);
            upnp.FireUpdateBase(e);
            if (e.Handled)
            {
                func = e.Data as UPnPFunction;
                if (func != null)
                {
                    string str = func.Arguments["NewRemoteHost"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.RemoteHost = str;
                    str = func.Arguments["NewExternalPort"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.ExternalPort = int.Parse(str);
                        }
                        catch { }
                    }
                    str = func.Arguments["NewProtocol"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.Protocol = str;
                    str = func.Arguments["NewInternalPort"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.InternalPort = int.Parse(str);
                        }
                        catch { }
                    }
                    str = func.Arguments["NewInternalClient"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.InternalHost = str;
                    str = func.Arguments["NewEnabled"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        switch (str)
                        {
                            case "1":
                                mapping.Enable = true;
                                break;
                            case "0":
                                mapping.Enable = false;
                                break;
                        }
                    }
                    str = func.Arguments["NewPortMappingDescription"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.Description = str;
                    str = func.Arguments["NewLeaseDuration"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.LeaseDuration = int.Parse(str);
                        }
                        catch { }
                    }
                    return mapping;
                }
            }
            return null;
        }
        /// <summary>
        /// This action reports the Static Port Mapping specified by the unique tuple of RemoteHost, ExternalPort and PortMappingProtocol.
        /// </summary>
        /// <param name="mapping"></param>
        /// <returns></returns>
        public bool GetSpecificPortMappingEntry(IUPnPUpdater upnp, ref PortMapping mapping)
        {
            UPnPFunction func = new UPnPFunction();
            func.Name = "GetSpecificPortMappingEntry";
            func.Service = this;

            func.Arguments.Add("NewRemoteHost", (mapping.RemoteHost != null ? mapping.RemoteHost : string.Empty));
            func.Arguments.Add("NewExternalPort", mapping.ExternalPort.ToString());
            func.Arguments.Add("NewProtocol", mapping.Protocol);
            func.Arguments.Add("NewInternalPort", mapping.InternalPort.ToString());
            func.Arguments.Add("NewInternalClient", string.Empty);
            func.Arguments.Add("NewEnabled", string.Empty);
            func.Arguments.Add("NewPortMappingDescription", string.Empty);
            func.Arguments.Add("NewLeaseDuration", string.Empty);

            FmdcEventArgs e = new FmdcEventArgs(Actions.UPnPFunctionCall, func);
            upnp.FireUpdateBase(e);
            if (e.Handled)
            {
                func = e.Data as UPnPFunction;
                if (func != null)
                {
                    string str = func.Arguments["NewRemoteHost"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.RemoteHost = str;
                    str = func.Arguments["NewExternalPort"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.ExternalPort = int.Parse(str);
                        }
                        catch { }
                    }
                    str = func.Arguments["NewProtocol"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.Protocol = str;
                    str = func.Arguments["NewInternalPort"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.InternalPort = int.Parse(str);
                        }
                        catch { }
                    }
                    str = func.Arguments["NewInternalClient"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.InternalHost = str;
                    str = func.Arguments["NewEnabled"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        switch (str)
                        {
                            case "1":
                                mapping.Enable = true;
                                break;
                            case "0":
                                mapping.Enable = false;
                                break;
                        }
                    }
                    str = func.Arguments["NewPortMappingDescription"];
                    if (!string.IsNullOrEmpty(str))
                        mapping.Description = str;
                    str = func.Arguments["NewLeaseDuration"];
                    if (!string.IsNullOrEmpty(str))
                    {
                        try
                        {
                            mapping.LeaseDuration = int.Parse(str);
                        }
                        catch { }
                    }
                }
            }
            return e.Handled;
        }

        public bool AddPortMapping(
            IUPnPUpdater upnp,
            string remoteHost,
            int externalPort,
            string protocol,
            int internalPort,
            string internalHost,
            bool enable,
            string description,
            int leaseDuration
            )
        {
            return AddPortMapping(upnp, new PortMapping(remoteHost, externalPort, protocol, internalPort, internalHost, enable, description, leaseDuration));
        }

        public bool AddPortMapping(IUPnPUpdater upnp, PortMapping mapping)
        {
            UPnPFunction func = new UPnPFunction();
            func.Name = "AddPortMapping";
            func.Service = this;

            func.Arguments.Add("NewRemoteHost", (mapping.RemoteHost != null ? mapping.RemoteHost : string.Empty));
            func.Arguments.Add("NewExternalPort", mapping.ExternalPort.ToString());
            func.Arguments.Add("NewProtocol", mapping.Protocol);
            func.Arguments.Add("NewInternalPort", mapping.InternalPort.ToString());
            func.Arguments.Add("NewInternalClient", mapping.InternalHost);
            func.Arguments.Add("NewEnabled", (mapping.Enable ? "1" : "0"));
            func.Arguments.Add("NewPortMappingDescription", mapping.Description);
            func.Arguments.Add("NewLeaseDuration", mapping.LeaseDuration.ToString());

            FmdcEventArgs e = new FmdcEventArgs(Actions.UPnPFunctionCall, func);
            upnp.FireUpdateBase(e);
            return e.Handled;
        }

        /// <summary>
        /// This action deletes a previously instantiated port mapping. 
        /// As each entry is deleted, the array is compacted, and the evented variable PortMappingNumberOfEntries is decremented.
        /// </summary>
        /// <param name="upnp"></param>
        /// <returns></returns>
        public bool DeletePortMapping(IUPnPUpdater upnp, PortMapping mapping)
        {
            UPnPFunction func = new UPnPFunction();
            func.Name = "DeletePortMapping";
            func.Service = this;

            func.Arguments.Add("NewRemoteHost", (mapping.RemoteHost != null ? mapping.RemoteHost : string.Empty));
            func.Arguments.Add("NewExternalPort", mapping.ExternalPort.ToString());
            func.Arguments.Add("NewProtocol", mapping.Protocol);

            FmdcEventArgs e = new FmdcEventArgs(Actions.UPnPFunctionCall, func);
            upnp.FireUpdateBase(e);
            return e.Handled;
        }

        /// <summary>
        /// This action retrieves the value of the external IP address on this connection instance.
        /// </summary>
        /// <returns>External IP</returns>
        public System.Net.IPAddress GetExternalIPAddress(IUPnPUpdater upnp)
        {
            UPnPFunction func = new UPnPFunction();
            func.Name = "GetExternalIPAddress";
            func.Service = this;

            func.Arguments.Add("NewExternalIPAddress", string.Empty);

            FmdcEventArgs e = new FmdcEventArgs(Actions.UPnPFunctionCall, func);
            upnp.FireUpdateBase(e);
            if (e.Handled)
            {
                func = e.Data as UPnPFunction;
                if (func != null)
                {
                    try
                    {
                        return System.Net.IPAddress.Parse(func.Arguments["NewExternalIPAddress"]);
                    }
                    catch { }
                }
            }
            return null;
        }

        public class PortMapping
        {
            public string RemoteHost
            {
                get;
                set;
            }

            public string InternalHost
            {
                get;
                set;
            }

            public int ExternalPort
            {
                get;
                set;
            }

            public int InternalPort
            {
                get;
                set;
            }

            public string Protocol
            {
                get;
                set;
            }

            public string Description
            {
                get;
                set;
            }

            public bool Enable
            {
                get;
                set;
            }

            public int LeaseDuration
            {
                get;
                set;
            }

            public bool IsValid
            {
                get
                {
                    bool valid = true;
                    if (Description == null ||
                        ExternalPort <= 0 ||
                        InternalHost == null ||
                        InternalPort <= 0 ||
                        Protocol == null ||
                        RemoteHost == null
                        )
                    {
                        valid = false;
                    }
                    return valid;
                }

            }
            public PortMapping() { }
            public PortMapping(
                System.Net.IPAddress remoteHost,
                int externalPort,
                string protocol,
                int internalPort,
                string internalHost,
                bool enable,
                string description,
                int leaseDuration
                )
                : this((remoteHost != null ? remoteHost.ToString() : string.Empty), externalPort, protocol, internalPort, internalHost, enable, description, leaseDuration)
            {

            }

            public PortMapping(
                string remoteHost,
                int externalPort,
                string protocol,
                int internalPort,
                string internalHost,
                bool enable,
                string description,
                int leaseDuration
                )
            {
                RemoteHost = remoteHost;
                ExternalPort = externalPort;
                Protocol = protocol;
                InternalPort = internalPort;
                InternalHost = internalHost;
                Enable = enable;
                Description = description;
                LeaseDuration = leaseDuration;
            }
        }
    }
}
