
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
        private bool generateTag = false;

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

        public TagInfo(TagInfo value)
        {
            if (value.Mode != ConnectionTypes.Unknown)
                Mode = value.Mode;
            if (!value.GenerateTag/* value.TagInfo.Tag != null*/)
                Tag = value.Tag;
            if (value.Version != null)
                Version = value.Version;
            if (value.Normal != -1)
                Normal = value.Normal;
            if (value.Regged != -1)
                Regged = value.Regged;
            if (value.OP != -1)
                OP = value.OP;
            if (value.Slots != -1)
                Slots = value.Slots;

        }

        /// <summary>
        /// Should tag be generated
        /// </summary>
        public bool GenerateTag
        {
            get { return generateTag; }
            set { generateTag = value; }
        }

        /// <summary>
        /// Client Version information
        /// </summary>
        public string Version
        {
            get { return version; }
            set { version = value; CreateTag(); }
        }

        /// <summary>
        /// Normal Hub count
        /// </summary>
        public int Normal
        {
            get { return hubs_normal; }
            set { hubs_normal = value; CreateTag(); }
        }

        /// <summary>
        /// Regged Hub count
        /// </summary>
        public int Regged
        {
            get { return hubs_regged; }
            set { hubs_regged = value; CreateTag(); }
        }
        /// <summary>
        /// OP Hub count
        /// </summary>
        public int OP
        {
            get { return hubs_op; }
            set { hubs_op = value; CreateTag(); }
        }

        /// <summary>
        /// Open Slots
        /// </summary>
        public int Slots
        {
            get { return slots; }
            set { slots = value; CreateTag(); }
        }
        /// <summary>
        /// Client Connection Mode.
        /// Passive, Active, Socket
        /// </summary>
        public ConnectionTypes Mode
        {
            get { return mode; }
            set { mode = value; CreateTag(); }
        }

        public string Tag
        {
            get { return tag; }
            set {
                tag = value;
                if (tag == null)
                    return;
                // TODO : This tag parsing should be rewrited
                // <++ V:0.699,M:A,H:2/0/0,S:1>
                string tmp = tag;
                tag = tag.Trim('<','>');
                string[] tagsections = tag.Split(',');
                if (tagsections.Length > 1)
                    version = tagsections[0];
                for (int i = 1; i < tagsections.Length; i++)
                {
                    if (tagsections[i].Length < 2)
                        continue;
                    string key = tagsections[i].Substring(0, 2);
                    string val = tagsections[i].Substring(2);

                    switch (key)
                    {
                        case "M:":
                            switch (val)
                            {
                                case "A":
                                    mode = ConnectionTypes.Direct;
                                    break;
                                case "P":
                                    mode = ConnectionTypes.Passive;
                                    break;
                                case "5":
                                    mode = ConnectionTypes.Socket5;
                                    break;
                                default:
                                    mode = ConnectionTypes.Unknown;
                                    break;
                            }

                            break;
                        case "H:":
                            string[] sections;
                            if ((sections = val.Split('/')).Length == 3)
                            {
                                try
                                {
                                    hubs_normal = int.Parse(sections[0]);
                                    hubs_regged = int.Parse(sections[1]);
                                    hubs_op = int.Parse(sections[2]);
                                }
                                catch { }

                            }
                            break;
                        case "S:":
                            try
                            {
                                slots = int.Parse(val);
                            }
                            catch { }
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        public void CreateTag()
        {
            if (!generateTag)
                return;
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
