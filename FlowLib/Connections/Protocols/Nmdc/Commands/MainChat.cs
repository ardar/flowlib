using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class MainChat : HubMessage
    {
        protected string content = null;
        public string Content
        {
            get { return content; }
        }
        // for receiving
        public MainChat(Hub hub, string raw)
            : base(hub, raw)
        {
            if (raw.StartsWith("<"))
            {
                string tmp = raw.Substring(1);
                int pos = 0;
                // If first is on a pos more then max allowed nick size(35), ignore it.
                if ((pos = tmp.IndexOf("> ")) > 35 || pos < 1)
                {
                    content = raw;
                }
                else
                {
                    from = tmp.Substring(0, pos);
                    content = tmp.Substring(pos + 2);
                }
                // This is a real Main chat msg as we have from.
                // But as none follows standard we will allow msg even if it not a valid mainchat msg starting with <
                //if (!string.IsNullOrEmpty(from))
                //    valid = true;
            }
            else
            {
                content = raw;
            }
            valid = true;
        }
        // for sending
        public MainChat(Hub hub, string from, string content)
            : base(hub, null)
        {
            // TODO : Fix so we dont have from as param as we is not using it.
            // We are just using this from to specify what constructor to use... we know it is ugly.
            this.content = content;
            this.from = from;
            Raw = "<" + this.from + "> " + this.content + "|";
            IsValid = true;
        }
    }
}