﻿using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class ForceMove : HubMessage
    {
        protected string address = null;
        public string Address
        {
            get { return address; }
        }
        public ForceMove(Hub hub, string raw)
            : base(hub, raw)
        {
            // $ForceMove flowlib.dummy.org
            string[] com = raw.Split(' ');
            if (com.Length != 2)
                return;
            this.address = com[1];
            if (!string.IsNullOrEmpty(address))
                IsValid = true;
        }
    }
}