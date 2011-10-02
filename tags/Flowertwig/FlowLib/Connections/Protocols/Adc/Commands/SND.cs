using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class SND : AdcBaseMessage
    {
        protected string contentType = null;
        protected string identifier = null;
        protected SegmentInfo segment = null;

        public string ContentType
        {
            get { return contentType; }
        }
        public string Identifier
        {
            get { return identifier; }
        }
        public SegmentInfo SegmentInfo
        {
            get { return segment; }
        }

        public SND(IConnection con, string contentType, string identifier, SegmentInfo info)
            : base(con, null)
        {
            this.contentType = contentType;
            this.identifier = identifier;
            this.segment = info;

            Raw = string.Format("CSND {0} {1} {2} {3}\n", contentType, identifier, info.Start, info.Length);
        }

        public SND(IConnection con, string raw)
            : base(con, raw)
        {
            if (param.Count >= 4)
            {
                contentType = param[0];
                identifier = param[1];
                try
                {
                    long start = long.Parse(param[2]);
                    long length = long.Parse(param[3]);
                    segment = new SegmentInfo(-1, start, length);
                    valid = true;
                }
                catch { }
            }

        }
    }
}