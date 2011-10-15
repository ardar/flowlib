using Flowertwig.Utils;
using Flowertwig.Utils.Entities;
using FlowLib.Connections.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class SR : HubMessage
    {
        protected ContentInfo info = null;
        protected string address = null;
        protected string content = null;

        public string Content
        {
            get { return content; }
            set { content = value; }
        }

        public string Address
        {
            get { return address; }
            set { address = value; }
        }

        public ContentInfo Info
        {
            get
            {
                return info;
            }
            set
            {
                info = value;
            }
        }

        public SR(Client client, string raw)
            : base(client, raw)
        {
            Parse();
        }

        protected void Parse()
        {
            // Directory
            // $SR DC++0.699 Books 1/1TTH:AYAAMAAGAADAABQAAYAAEKCAAAAG633LOMAAAAA (127.0.0.1:411)
            // File
            // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv5898936 1/1TTH:QIFN7FMMSHXLRZDZUGTRZO5RWUY4IONS6JRVQSA (127.0.0.1:411)
            int pos1 = 0, pos2 = 0;
            string[] sections = raw.Split('\x005');

            if (sections.Length >= 2)
            {
                info = new ContentInfo();

                // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv
                #region User Id
                if (StringOperations.Find(sections[0], "$SR ", " ", ref pos1, ref pos2))
                {
                    pos1 += 4;
                    pos2 -= 1;
                    from = sections[0].Substring(pos1, pos2 - pos1);
                    pos2++;
                }
                #endregion

                int index = 0;
                // If file, we will have 3
                if (sections.Length == 3)
                {
                    info.Set(ContentInfo.VIRTUAL, sections[0].Substring(pos2));
                    index = 1;
                }
                else if (sections.Length == 2)
                {
                    index = 0;
                }
                int tmp;

                // Get directory name or file size
                if ((tmp = sections[index].LastIndexOf(" ")) != -1)
                {
                    if (index == 0)
                    {
                        info.Set(ContentInfo.VIRTUAL, sections[0].Substring(pos2, tmp - pos2));
                    }
                    long size;
                    try
                    {
                        size = long.Parse(sections[index].Substring(0, tmp));
                        info.Size = size;
                    }
                    catch { }
                }
                index++;
                pos1 = 0;
                pos2 = 0;
                // Content & Address
                if (StringOperations.Find(sections[index], "(", ")", ref pos1, ref pos2))
                {
                    content = sections[index].Substring(0, pos1).Trim();
                    if (content.StartsWith("TTH:"))
                    {
                        info.Set(ContentInfo.TTH, content.Substring(4));
                    }
                    pos1++;
                    pos2--;
                    address = sections[index].Substring(pos1, pos2 - pos1);
                    valid = true;
                }
            }
        }

        public SR(Client client, ContentInfo info, bool directoryOnly, string from)
            : base(client, null)
        {
            this.info = info;
            // Directory
            // $SR DC++0.699 Books 1/1TTH:AYAAMAAGAADAABQAAYAAEKCAAAAG633LOMAAAAA (127.0.0.1:411)
            // File
            // $SR DC++0.699 BIGFILE\pgm2-kebabtestarna.wmv5898936 1/1TTH:QIFN7FMMSHXLRZDZUGTRZO5RWUY4IONS6JRVQSA (127.0.0.1:411)
            string content = null;
            if (directoryOnly)
                content = System.IO.Path.GetDirectoryName(info.Get(ContentInfo.VIRTUAL));
            else
                content = info.Get(ContentInfo.VIRTUAL) + "\x005" + info.Size.ToString();
            // TODO : We are not supporting slot system right now so we will respond that we have all slots open.
            string slots = string.Format("{0}/{0}", client.Me.TagInfo.Slots); 
            // TODO : We are not saving hub name as it is now so we cant use it.
            string hubname = string.Empty;
            if (info.ContainsKey(ContentInfo.TTH) && !directoryOnly)
                hubname = "TTH:" + info.Get(ContentInfo.TTH);
            string passive = string.Empty;
            if (!string.IsNullOrEmpty(from))
                passive = "\x005" + from;
            Raw = string.Format("$SR {0} {1} {2}\x005{3} ({4}:{5}){6}|", client.Me.ID, content, slots, hubname, client.RemoteAddress.Address.ToString(), client.RemoteAddress.Port, passive);
        }
    }
}