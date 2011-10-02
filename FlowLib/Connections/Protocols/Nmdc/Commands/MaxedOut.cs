using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class MaxedOut : StrMessage
    {
        public MaxedOut(IConnection con, string raw)
            : base(con, raw)
        {
            valid = true;
        }
        public MaxedOut(IConnection con)
            : base(con, null)
        {
            Raw = "$MaxedOut|";
        }
    }
}