
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using FlowLib.Enums;

namespace FlowLib.Containers
{
    /// <summary>
    /// User Tag information
    /// </summary>
    public class TagInfo
    {
        private string tag = "";
        private ConnectionTypes mode = ConnectionTypes.Unknown;
        private string version = "";
        private int hubs_normal = 0;
        private int hubs_regged = 0;
        private int hubs_op = 0;
        private int slots = 0;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public TagInfo() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="update">
        /// Setting this to true will set values to improper ones.
        /// This so we know if we have Updated value or if we should ignore it</param>
        public TagInfo(bool update)
        {
            mode = 0;
            hubs_op = -1;
            hubs_normal = -1;
            hubs_regged = -1;
            slots = -1;
            tag = null;
            version = null;
        }

        /// <summary>
        /// Client Version information
        /// </summary>
        public string Version
        {
            get { return version; }
            set { version = value; }
        }

        /// <summary>
        /// Normal Hub count
        /// </summary>
        public int Normal
        {
            get { return hubs_normal; }
            set { hubs_normal = value; }
        }

        /// <summary>
        /// Regged Hub count
        /// </summary>
        public int Regged
        {
            get { return hubs_regged; }
            set { hubs_regged = value; }
        }
        /// <summary>
        /// OP Hub count
        /// </summary>
        public int OP
        {
            get { return hubs_op; }
            set { hubs_op = value; }
        }

        /// <summary>
        /// Open Slots
        /// </summary>
        public int Slots
        {
            get { return slots; }
            set { slots = value; }
        }
        /// <summary>
        /// Client Connection Mode.
        /// Passive, Active, Socket
        /// </summary>
        public ConnectionTypes Mode
        {
            get { return mode; }
            set { mode = value; }
        }

        public string Tag
        {
            get { return tag; }
            set { tag = value; }
        }

        public void CreateTag()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append("<" + version);                   // Version
            sb.Append(",M:");                           // Mode

            switch (mode)
            {
                case ConnectionTypes.Unknown:
                case ConnectionTypes.Passive:
                    sb.Append("P");                         // Passive
                    break;
                case ConnectionTypes.Socket5:
                    sb.Append("5");
                    break;
                default:
                    sb.Append("A");                         // Active
                    break;
            }
            sb.Append(",H:" + hubs_normal.ToString());  // Normal Hubs
            sb.Append("/" + hubs_regged.ToString());    // Regged Hubs
            sb.Append("/" + hubs_op.ToString());        // OP Hubs
            sb.Append(",S:" + slots.ToString());        // Slots
            sb.Append(">");
            tag = sb.ToString();
        }

        public override string ToString()
        {
            return tag;
        }
    }
}
