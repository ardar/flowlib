
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

using System.Net;

namespace FlowLib.Containers.UPnP
{
    public class DeviceInformation
    {
        public SpecificationVersion SpecVersion
        {
            get;
            set;
        }

        public string URLBase
        {
            get;
            set;
        }

        public string DeviceType
        {
            get;
            set;
        }

        public string FriendlyName
        {
            get;
            set;
        }

        public string Manufacturer
        {
            get;
            set;
        }

        public string ManufacturerURL
        {
            get;
            set;
        }

        public string ModelDescription
        {
            get;
            set;
        }

        public string ModelNumber
        {
            get;
            set;
        }

        public string serialNumber
        {
            get;
            set;
        }

        public string ModelName
        {
            get;
            set;
        }

        public string UDN
        {
            get;
            set;
        }

        public string UUID
        {
            get;
            set;
        }

        public string PresentationURL
        {
            get;
            set;
        }

        public string DescriptionURL
        {
            get;
            set;
        }

        public string DeviceVersion
        {
            get;
            set;
        }

        public int MaxAge
        {
            get;
            set;
        }

        public EndPoint Sender
        {
            get;
            set;
        }

        public DeviceInformation()
        {
            SpecVersion = new SpecificationVersion();
        }
    }
}
