using Flowertwig.Utils.Connections.Interfaces;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class UGetZBlock : GetBlocks
    {
        public UGetZBlock(IConnection con, string filename, long start, long length)
            : base(con, filename, true, start, length)
        {
            ZLibCompression = true;
            Encoding = System.Text.Encoding.UTF8;
            Raw = string.Format("$UGetZBlock {0} {1} {2}|", Start, Length, FileName);
        }

        public UGetZBlock(IConnection con, string raw)
            : base(con, raw)
        {
            ZLibCompression = true;
            Encoding = System.Text.Encoding.UTF8;
            valid = true;
        }
    }
}