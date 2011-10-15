using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class To : HubMessage
    {
        protected string content = null;
        public string Content
        {
            get { return content; }
        }
        // for receiving
        public To(Client client, string raw)
            : base(client, raw)
        {
            //$To: Flow84 From: PIP $<PIP> Hejsan|
            int pos1, pos2;
            if (raw.StartsWith("$To: ") && (pos1 = raw.IndexOf(" From: ")) != -1)
            {
                to = raw.Substring(5, pos1 - 5);
                if ((pos2 = raw.IndexOf(" $", pos1)) != -1)
                {
                    pos1 += 7;
                    from = raw.Substring(pos1, pos2 - pos1);
                    pos2 += 2;
                    content = raw.Substring(pos2);
                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(content))
                        valid = true;
                }
            }
        }
        // for sending 
        public To(Client client, string to, string content)
            : base(client, null)
        {
            this.from = client.Me.DisplayName;
            this.to = to;
            this.content = "<" + this.from + "> " + content;
            this.Raw = "$To: " + this.to + " From: " + this.from + " $" + this.content + "|";
            IsValid = true;
            //$To: Flow84 From: PIP $<PIP> Hejsan|
        }
    }
}