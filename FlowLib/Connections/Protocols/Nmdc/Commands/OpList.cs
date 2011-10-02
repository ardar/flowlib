using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class OpList : HubMessage
    {
        protected string[] ops = null;
        public string[] List
        {
            get { return ops; }
        }
        public OpList(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos1;
            if ((pos1 = raw.IndexOf(" ")) != -1)
            {
                string tmp = raw.Substring(++pos1);
                char[] test = { '$', '$' };
#if !COMPACT_FRAMEWORK
                ops = tmp.Split(test, System.StringSplitOptions.RemoveEmptyEntries); // command.Split("$$", StringSplitOptions.RemoveEmptyEntries);
#else
                System.Collections.Generic.List<string> lst = new System.Collections.Generic.List<string>(tmp.Split(test));
                while (lst.Remove(string.Empty)) { }
                ops = lst.ToArray();
#endif
            }
            IsValid = true;
        }
    }
}