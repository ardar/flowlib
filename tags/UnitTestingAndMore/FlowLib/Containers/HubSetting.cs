
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

namespace FlowLib.Containers
{
    /// <summary>
    /// Class repressenting hub setting
    /// </summary>
    public class HubSetting : PropertyContainer<string, string>
    {
        protected string name = string.Empty;
        protected string address = string.Empty;
        protected int port = 411;
        protected string description = string.Empty;
        protected string displayName = string.Empty;
        protected string password = string.Empty;
        protected string userdescription = string.Empty;
        protected string protocol = string.Empty;
        protected string shareName = string.Empty;

        /// <summary>
        /// Dummy Constructor
        /// </summary>
        public HubSetting() { }
        /// <summary>
        /// Sets/Gets what protocol that should be used.
        /// string.empty    = Automaticly Determin what protocol
        /// Nmdc            = NMDC
        /// NmdcSecure      = Nmdc (TLS)
        /// Adc             = ADC
        /// AdcSecure      = ADCS
        /// </summary>
        public string Protocol
        {
            get { return protocol; }
            set { protocol = value; }
        }
        /// <summary>
        /// Name of hub
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        /// <summary>
        /// string representation of IP/DNS
        /// </summary>
        public string Address
        {
            get { return address; }
            set { address = value; }
        }
        /// <summary>
        /// Short description of this hub
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        /// <summary>
        /// Display name of user connecting to hub
        /// </summary>
        public string DisplayName
        {
            get { return displayName; }
            set { displayName = value; }
        }
        /// <summary>
        /// Password for user we connect to hub with
        /// </summary>
        public string Password
        {
            get { return password; }
            set { password = value; }
        }
        /// <summary>
        /// User description of user connecting to hub
        /// </summary>
        public string UserDescription
        {
            get { return userdescription; }
            set { userdescription = value; }
        }
        /// <summary>
        /// Port to user when connecting
        /// </summary>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }
        /// <summary>
        /// Share id pointing to Share.
        /// Used for hub
        /// </summary>
        public string ShareId
        {
            get { return shareName; }
            set { shareName = value; }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
