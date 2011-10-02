using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class SID : AdcBaseMessage
    {
        public SID(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 1)
            {
                id = param[0];
                valid = true;
            }
        }
    }
}