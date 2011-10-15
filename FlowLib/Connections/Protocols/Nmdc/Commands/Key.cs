using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class Key : StrMessage
    {
        protected string value = null;
        public string Content
        {
            get { return value; }
        }
        public Key(string calcKey, IConnection con)
            : base(con, null)
        {
            Raw = "$Key " + calcKey + "|";
        }

        public Key(IConnection con, string raw)
            : base(con, raw)
        {
            int pos;
            if ((pos = raw.IndexOf(" ")) != -1)
            {
                pos++;
                value = raw.Substring(pos);
                valid = true;
            }
        }
        public Key(Client client, string value)
            : base(client, null)
        {
            this.value = value;
            Raw = "$Key " + Content + "|";
            if (!string.IsNullOrEmpty(value))
                IsValid = true;
        }
     
    }
}