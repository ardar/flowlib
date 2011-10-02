using Flowertwig.Utils.Connections.Interfaces;
using Flowertwig.Utils.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class GET : AdcBaseMessage
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
        public GET(IConnection con, string raw)
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

        public GET(IConnection con, ContentInfo info, SegmentInfo segment)
            : this(con, info, segment, "file") { }

        public GET(IConnection con, ContentInfo info, SegmentInfo segment, string type)
            : base(con, null)
        {
            string req = null;
            if (info.ContainsKey(ContentInfo.REQUEST))
                req = info.Get(ContentInfo.REQUEST);
            else if (info.ContainsKey(ContentInfo.TTH))
                req = info.Get(ContentInfo.TTH);
            else if (info.ContainsKey(ContentInfo.VIRTUAL))
                req = info.Get(ContentInfo.VIRTUAL);

            if (req == null)
                throw new System.ArgumentException("ContentInfo must contain any of: REQUEST, TTH or VIRTUAL");

            // TODO : Add support for list also
            Raw = string.Format("CGET {0} {1} {2} {3}\n", type, req, segment.Start, segment.Length);
        }
    }
}