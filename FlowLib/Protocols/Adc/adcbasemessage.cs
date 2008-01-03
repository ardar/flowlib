
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

using FlowLib.Protocols;
using FlowLib.Interfaces;
using FlowLib.Containers;
using FlowLib.Connections;

using System.Collections.Generic;

namespace FlowLib.Protocols.Adc
{
    public class AdcBaseMessage : HubMessage
    {
        protected string action = null;
        protected string type = null;
        protected List<string> param = null;
        protected string id = null;
        protected string idtwo = null;
        /// <summary>
        /// Action
        /// </summary>
        public string Action
        {
            get { return action; }
        }
        /// <summary>
        /// Message type specifies how messages should be routed.
        /// B   =   Broadcast. Hub must send message to all connected clients, including the sender of the message.
        /// D   =   Direct message. The hub must send the message to the target_sid user.
        /// E   =   Echo message. The hub must send the message to the target_sid user and the my_sid user.
        /// F   =   Feature broadcast. The hub must send message to all clients that support both all required (+) and no excluded (-) features named. The feature name is matched against the corresponding SU field in INF sent by each client.
        /// H   =   Hub message. Clients must use this message type when a message is intended for the hub only.
        /// I   =   Info message. Hubs must use this message type when sending a message to a client that didn't come from another client.
        /// </summary>
        public string Type
        {
            get { return type; }
        }
        /// <summary>
        /// Data after Type and Cmd
        /// Example:
        /// On the command "BINF CVNQ HN3\n"
        /// Param will be "CVNQ HN3"
        /// </summary>
        public List<string> Param
        {
            get { return param; }
        }

        public string Id
        {
            get { return id; }
        }

        /// <summary>
        /// Present in D Messages
        /// </summary>
        public string IDTwo
        {
            get { return idtwo; }
        }

        public AdcBaseMessage(Hub hub, string raw)
            : base(hub, raw)
        {
            if (raw == null)
                return;

            bool hasId = true;
            bool hasId2 = true;
            param = new System.Collections.Generic.List<string>(raw.Split(' '));
            if (param.Count >= 1 && param[0].Length == 4)
            {
                type = param[0].Substring(0, 1);
                switch (type)
                {
                    case "B":       // Broadcast Message
                        hasId = true;
                        break;
                    case "C":       // Client Message
                        hasId = true;
                        break;
                    case "I":
                        hasId = false;
                        break;
                    case "D":       // Direct message
                        hasId = true;
                        hasId2 = true;
                        break;
                    case "U":       // UDP Message
                        hasId = true;
                        break;
                }

                action = param[0].Substring(1, 3);
                param.RemoveAt(0);
            }

            if (hasId && param.Count >= 1)
            {
                id = param[0];
                param.RemoveAt(0);
            }
            if (hasId2 && param.Count >= 1)
            {
                idtwo = param[0];
                param.RemoveAt(0);
            }
        }
    }
}
