using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class HubName : HubMessage
    {
        #region Variables
        protected string content = string.Empty;
        protected string name = string.Empty;
        protected string topic = string.Empty;
        #endregion
        #region Properties
        public string Content
        {
            get { return content; }
        }
        public string Name
        {
            get { return name; }
        }
        public string Topic
        {
            get { return topic; }
        }
        #endregion
        public HubName(Client client, string raw)
            : base(client, raw)
        {
            int pos, pos2;
            if ((pos = raw.IndexOf(' ')) != -1)
            {
                pos++;
                content = raw.Substring(pos);
                if ((pos2 = content.IndexOf(" - ")) != -1)
                {
                    name = content.Substring(0, pos2);
                    topic = content.Substring(pos2 + 3);
                }
                else
                {
                    name = content;
                }
            }
            IsValid = true;
        }
    }
}