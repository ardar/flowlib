using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetListLen : StrMessage
    {
        public GetListLen(IConnection con, string raw)
            : base(con, raw)
        {

        }
        public GetListLen(IConnection con)
            : base(con, null)
        {
            Raw = "$GetListLen|";
        }
    }
}