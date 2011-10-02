using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class UGetBlock : GetBlocks
    {
        public UGetBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, false,start, length)
        {
            Encoding = System.Text.Encoding.UTF8;
            Raw = string.Format("$UGetBlock {0} {1} {2}|", Start, Length, FileName);
        }

        public UGetBlock(IConnection con, string raw)
            : base(con, raw)
        {
            Encoding = System.Text.Encoding.UTF8;
            valid = true;
        }
    }
}