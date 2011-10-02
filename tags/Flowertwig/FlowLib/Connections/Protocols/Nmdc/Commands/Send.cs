using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Send : StrMessage
    {   
        public Send(IConnection con)
            : base(con, null)
        {
            Raw = "$Send|";
        }

        public Send(IConnection con, string raw)
            : base(con, raw)
        {
            valid = true;
        }
    }
}