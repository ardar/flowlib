using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class GetZBlock : GetBlocks
    {
        public GetZBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, true, start, length)
        {
            ZLibCompression = true;
            Raw = string.Format("$GetZBlock {0} {1} {2}|", Start, Length, filename);
        }

        public GetZBlock(IConnection con, string raw)
            : base(con, raw)
        {
            ZLibCompression = true;
            valid = true;
        }
    }
}