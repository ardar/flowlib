
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

using System.Xml;
using System.Collections.Generic;
using System.Net;

using FlowLib.Interfaces;
using FlowLib.Containers;
using FlowLib.Containers.UPnP;
using FlowLib.Events;

namespace FlowLib.Protocols
{
    public class UPnPProtocol : IProtocolUPnP
    {
        public event FlowLib.Events.FmdcEventHandler MessageReceived;
        public event FlowLib.Events.FmdcEventHandler MessageToSend;
        public event FlowLib.Events.FmdcEventHandler Update;

        protected FlowLib.Interfaces.IUPnP con = null;
        protected bool disposed = false;

        public bool IsDisposed
        {
            get { return disposed; }
            protected set { disposed = value; }
        }

        public string Seperator
        {
            get { throw new System.NotImplementedException(); }
        }
        public IConMessage KeepAliveCommand
        {
            get { throw new System.NotImplementedException(); }
        }
        public string Name
        {
            get { return "UPnP"; }
        }

        public System.Text.Encoding Encoding
        {
            get;
            set;
        }

        public IConMessage FirstCommand
        {
            get
            {
                StrMessage msg = new StrMessage(
                    null,
                    "M-SEARCH * HTTP/1.1\r\n" +
                    "Host:" + con.EndPoint.Address.ToString() + ":" + con.EndPoint.Port.ToString() + "\r\n" +
                     "ST:upnp:rootdevice\r\n" +
                    "Man:\"ssdp:discover\"\r\n" +
                    "MX:3\r\n" +
                     "\r\n" +
                    "\r\n"
                    );
                msg.Bytes = Encoding.GetBytes(msg.Raw);
                return msg;
            }
        }

        public UPnPProtocol(FlowLib.Connections.UPnP connection)
        {
            this.MessageReceived = new FmdcEventHandler(OnMessageReceived);
            this.MessageToSend = new FmdcEventHandler(OnMessageToSend);
            this.Update = new FmdcEventHandler(OnUpdate);

            Encoding = System.Text.Encoding.ASCII;
            con = connection;
        }

        void OnUpdate(object sender, FmdcEventArgs e) { }
        void OnMessageToSend(object sender, FmdcEventArgs e) { }
        void OnMessageReceived(object sender, FmdcEventArgs e) { }

        public void ParseRaw(byte[] b, int length, IPEndPoint point)
        {
            ParseRaw(Encoding.GetString(b, 0, length), point);
        }

        public void ParseRaw(byte[] b, int length)
        {
            throw new System.NotImplementedException();
        }

        protected void ParseRaw(string str, IPEndPoint point)
        {
            UdpMessage msg = new UdpMessage(str, point);
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
            MessageReceived(con, e);
            if (!e.Handled)
                ActOnInMessage(msg);
        }

        /// <summary>
        /// HTTP/1.1 200 OK
        /// ST:upnp:rootdevice
        /// USN:uuid:00-17-9A-6C-FD-EF-0100A8C00::upnp:rootdevice
        /// Location:http://192.168.0.1:80/desc.xml
        /// Cache-Control:max-age=1800
        /// Server:IGD-HTTP/1.1 UPnP/1.0 UPnP-Device-Host/1.0
        /// Ext:
        /// </summary>
        /// <param name="response"></param>
        public UPnPDevice ParseSSDP(string response)
        {
            return ParseSSDP(response, null);
        }
        public UPnPDevice ParseSSDP(string response, EndPoint sender)
        {
            UPnPDevice device = new UPnPDevice();

            device.Information.Sender = sender;

            string[] lines = response.Split('\n');
            for (int lineNr = 0; lineNr < lines.Length; lineNr++)
            {
                int pos = 0;
                if ((pos = lines[lineNr].IndexOf(':')) != -1)
                {
                    string key = lines[lineNr].Substring(0, pos++).ToLower();
                    string value = lines[lineNr].Substring(pos).TrimEnd('\r');
                    switch (key)
                    {
                        case "st":
                            break;
                        case "usn":
                            device.Information.UDN = value;
                            // USN:uuid:00-17-9A-6C-FD-EF-0100A8C00::upnp:rootdevice
                            if ((pos = value.IndexOf("uuid:")) != -1)
                            {
                                pos += 5;
                                int pos2 = 0;
                                if ((pos2 = value.IndexOf("::", pos)) != -1)
                                {
                                    // More values
                                    device.Information.UUID = value.Substring(pos, pos2 - pos);
                                }
                                else
                                {
                                    // One value
                                    device.Information.UUID = value.Substring(pos);
                                }
                            }
                            break;
                        case "location":
                            device.Information.DescriptionURL = value;
                            System.Uri uri = new System.Uri(value);
                            device.Information.URLBase = uri.Host + ":" + uri.Port;
                            break;
                        case "cache-control":
                            if (value.StartsWith("max-age="))
                            {
                                try
                                {
                                    device.Information.MaxAge = int.Parse(value.Replace("max-age=", string.Empty));
                                }
                                catch { }
                            }
                            break;
                        case "server":
                            device.Information.DeviceVersion = value;
                            break;
                    }
                }
            }
            return device;
        }

        public virtual void ParseDescription(ref UPnPDevice device, string xml)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            #region Get Spec Version
            XmlNodeList nodes = document.GetElementsByTagName("specVersion");
            if (nodes.Count == 1)
            {
                nodes = nodes[0].ChildNodes;
                foreach (XmlNode node in nodes)
                {
                    switch (node.Name.ToLower())
                    {
                        case "major":
                            device.Information.SpecVersion.Major = node.InnerText;
                            break;
                        case "minor":
                            device.Information.SpecVersion.Minor = node.InnerText;
                            break;
                    }
                }
            }
            #endregion
            #region Root Device Information
            nodes = document.GetElementsByTagName("device");
            if (nodes.Count > 0)
            {
                nodes = nodes[0].ChildNodes;
                foreach (XmlNode node in nodes)
                {
                    switch (node.Name.ToLower())
                    {
                        case "devicetype":
                            device.Information.DeviceType = node.InnerText;
                            break;
                        case "friendlyname":
                            device.Information.FriendlyName = node.InnerText;
                            break;
                        case "manufacturer":
                            device.Information.Manufacturer = node.InnerText;
                            break;
                        case "manufacturerurl":
                            device.Information.ManufacturerURL = node.InnerText;
                            break;
                        case "modeldescription":
                            device.Information.ModelDescription = node.InnerText;
                            break;
                        case "modelname":
                            device.Information.ModelName = node.InnerText;
                            break;
                        case "udn":
                            device.Information.UDN = node.InnerText;
                            break;
                        case "presentationurl":
                            device.Information.PresentationURL = node.InnerText;
                            break;
                        case "servicelist":
                            XmlNodeList serviceList = node.ChildNodes;
                            foreach (XmlNode xmlService in serviceList)
                            {
                                ServiceBase service = new ServiceBase();
                                if (device.RootDevice != null)
                                    service.Device = device.RootDevice;
                                else
                                    service.Device = device;

                                foreach (XmlNode serviceNode in xmlService)
                                {
                                    switch (serviceNode.Name.ToLower())
                                    {
                                        case "servicetype":
                                            service.Information.serviceType = serviceNode.InnerText;
                                            break;
                                        case "serviceid":
                                            service.Information.ServiceId = serviceNode.InnerText;
                                            break;
                                        case "controlurl":
                                            service.Information.ControlURL = serviceNode.InnerText;
                                            break;
                                        case "eventsuburl":
                                            service.Information.EventSubURL = serviceNode.InnerText;
                                            break;
                                        case "scpdurl":
                                            service.Information.SCPDURL = serviceNode.InnerText;
                                            break;
                                    }
                                }
                                if (device.RootDevice != null)
                                    device.RootDevice.Services.Add(service);
                                else
                                    device.Services.Add(service);
                            }
                            break;
                        case "devicelist":
                            XmlNodeList devicelist = node.ChildNodes;
                            foreach (XmlNode xmlDevice in devicelist)
                            {
                                UPnPDevice subdevice = new UPnPDevice();
                                if (device.RootDevice != null)
                                    subdevice.RootDevice = device.RootDevice;
                                else
                                    subdevice.RootDevice = device;
                                ParseDescription(ref subdevice, xmlDevice.OuterXml);
                                device.SubDevices.Add(subdevice);
                            }
                            break;
                    }
                }
            }
            #endregion
        }

        public void ActOnInMessage(IConMessage message)
        {
            UdpMessage msg = message as UdpMessage;
            if (msg != null)
            {
                // Device Found
                UPnPDevice device = ParseSSDP(msg.Raw, msg.RemoteAddress);
                FmdcEventArgs e = new FmdcEventArgs(0, device);
                Update(con, e);
                if (!e.Handled)
                {
                    string key = device.Information.Sender.ToString();
                    // Do device exist in our list?
                    if (this.con.RootDevices.ContainsKey(key))
                    {
                        // Don't add this device. It already exist in our list.
                    }
                    else
                    {
                        // We don't have this device yet. Add it.
                        con.RootDevices.Add(key, device);
                        FmdcEventArgs e2 = new FmdcEventArgs(Actions.UPnPRootDeviceFound, device);
                        Update(con, e2);
                    }
                }
            }
        }

        public void ActOnOutMessage(FlowLib.Events.FmdcEventArgs e)
        {
            string key = null;
            UPnPDevice device = null;
            switch (e.Action)
            {
                case Actions.UPnPDeviceDescription:
                    key = e.Data as string;
                    if (key != null)
                    {
                        try
                        {
                            if (con.RootDevices.ContainsKey(key))
                            {
                                device = con.RootDevices[key];
                                System.Net.HttpWebRequest httpRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(device.Information.DescriptionURL);
                                WebResponse webResponse = httpRequest.GetResponse();
                                System.IO.StreamReader sr = new System.IO.StreamReader(webResponse.GetResponseStream());
                                string ret = sr.ReadToEnd();
                                sr.Close();

                                FmdcEventArgs e2 = new FmdcEventArgs(Actions.UPnPDeviceDescription, ret);
                                Update(con, e2);
                                if (!e2.Handled)
                                {
                                    ParseDescription(ref device, ret);
                                    Update(con, new FmdcEventArgs(Actions.UPnPDeviceUpdated, device));
                                }
                            }
                        }
                        catch (System.Exception)
                        {
                            // TODO: Make exception handling
                        }
                    }
                    break;
                case Actions.UPnPFunctionCall:
                    UPnPFunction func = e.Data as UPnPFunction;
                    if (func != null)
                    {
                        try
                        {
                            #region Create Envelope
                            System.Text.StringBuilder sb = new System.Text.StringBuilder();
                            sb.Append("<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">");
                            sb.Append("<s:Body>");
                            sb.Append("<u:" + func.Name + " xmlns:u=\"" + func.Service.Information.serviceType + "\">");
                            foreach (KeyValuePair<string, string> argument in func.Arguments)
                            {
                                sb.AppendFormat("<{0}>{1}</{0}>", argument.Key, argument.Value);
                            }
                            sb.Append("</u:" + func.Name + ">");
                            sb.Append("</s:Body>");
                            sb.Append("</s:Envelope>");
                            #endregion
                            #region Create Request
                            byte[] body = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                            string url = null;
                            // Is ControlURL relative or absolute?
                            if (func.Service.Information.ControlURL.StartsWith("/"))
                            {
                                url = "http://" + func.Service.Device.Information.URLBase + func.Service.Information.ControlURL;
                            }
                            else
                            {
                                url = func.Service.Information.ControlURL;
                            }

                            WebRequest wr = WebRequest.Create(url);//+ controlUrl);
                            wr.Headers.Clear();
                            wr.Method = "POST";
                            wr.ContentType = "text/xml; charset=\"utf-8\"";
                            wr.Headers.Add("SOAPAction", "\"" + func.Service.Information.serviceType +
                            "#" + func.Name + "\"");
                            wr.ContentLength = body.Length;
                            #endregion
                            #region Call Service function
                            // TODO: Add error handling in this (If server returns code 500 or something)
                            System.IO.Stream stream = wr.GetRequestStream();
                            stream.Write(body, 0, body.Length);
                            stream.Flush();
                            stream.Close();
                            WebResponse wres = wr.GetResponse();
                            System.IO.StreamReader sr = new
                            System.IO.StreamReader(wres.GetResponseStream());
                            string xml = sr.ReadToEnd();
                            sr.Close();
                            #endregion
                            #region Parse returning data
                            XmlDocument document = new XmlDocument();
                            document.LoadXml(xml);
                            SortedList<string, string> tmpList = new SortedList<string, string>(func.Arguments);
                            foreach (KeyValuePair<string, string> argument in func.Arguments)
                            {
                                XmlNodeList nodes = document.GetElementsByTagName(argument.Key);
                                if (nodes.Count == 1)
                                {
                                    tmpList[argument.Key] = nodes[0].InnerText;
                                }
                            }
                            func.Arguments = tmpList;
                            #endregion
                            #region Return data
                            e.Data = func;
                            e.Handled = true;
                            #endregion
                        }
                        catch (System.Net.WebException webEx)
                        {
                            HttpWebResponse resp = webEx.Response as HttpWebResponse;
                            if (resp != null)
                            {

                                func.ErrorCode = (int)resp.StatusCode;
                            }
                            e.Data = func;
                        }
                        catch
                        {
                            // TODO: Add more specific error handling here
                        }
                    }
                    break;
            }
        }

        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(con, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            MessageReceived -= OnMessageReceived;
            MessageToSend -= OnMessageToSend;
            Update -= OnUpdate;
            Encoding = null;
            con = null;
            IsDisposed = true; 
        }
    }
}
