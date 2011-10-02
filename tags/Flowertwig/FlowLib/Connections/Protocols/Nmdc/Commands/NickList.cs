using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class NickList : HubMessage
    {
        protected string[] nicks = null;
        public string[] List
        {
            get { return nicks; }
        }
        public NickList(Hub hub, string raw)
            : base(hub, raw)
        {
            // $NickList <nick1>$$<nick2>$$<nick3>$$
            int pos1;
            if ((pos1 = raw.IndexOf(" ")) != -1)
            {
                string tmp = raw.Substring(pos1);
                char[] test = { '$', '$' };
#if !COMPACT_FRAMEWORK
                nicks = tmp.Split(test, System.StringSplitOptions.RemoveEmptyEntries);
#else
                System.Collections.Generic.List<string> lst = new System.Collections.Generic.List<string>(tmp.Split(test));
                while (lst.Remove(string.Empty)) { }
                nicks = lst.ToArray();
#endif
                if (nicks.Length > 0)
                    IsValid = true;
            }
        }
    }
}