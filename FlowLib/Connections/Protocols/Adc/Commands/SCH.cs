using Flowertwig.Utils.Connections.Interfaces;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Adc.Commands
{
    public class SCH : AdcBaseMessage
    {
        protected SearchInfo info = new SearchInfo();
        public SearchInfo Info
        {
            get { return info; }
        }

        public SCH(IConnection con, string raw)
            : base(con, raw)
        {
            // BSCH NRQF TRUDHPNB4BUIQV2LAI4HDWRL3KLJTUSXCTAMJHNII TOauto
            for (int i = 0; i < param.Count; i++)
            {
                if (param[i].Length < 2)
                    continue;
                string key = param[i].Substring(0, 2);
                string value = param[i].Substring(2);
                switch (key)
                {
                    case "AN":
                        //TODO : We should make it possible to have more then 1 AN.
                        if (info.ContainsKey(SearchInfo.SEARCH))
                            value = value + " " + info.Get(SearchInfo.SEARCH);
                        info.Set(SearchInfo.SEARCH, value);
                        valid = true;
                        break;
                    case "NO":
                        //TODO : We should make it possible to have more then 1 AN.
                        info.Set(SearchInfo.NOSEARCH, value);
                        break;
                    case "EX":
                        if (info.ContainsKey(SearchInfo.EXTENTION))
                            value = value + " " + info.Get(SearchInfo.EXTENTION);
                        info.Set(SearchInfo.EXTENTION, value);
                        break;
                    case "LE":
                        info.Set(SearchInfo.SIZETYPE, "2");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "GE":
                        info.Set(SearchInfo.SIZETYPE, "1");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "EQ":
                        info.Set(SearchInfo.SIZETYPE, "3");
                        try
                        {
                            info.Set(SearchInfo.SIZE, long.Parse(value).ToString());
                        }
                        catch { }
                        break;
                    case "TO":
                        info.Set(SearchInfo.TOKEN, value);
                        break;
                    case "TY":
                        switch (value)
                        {
                            case "1":   // File
                                break;
                            case "2":   // Directory
                                info.Set(SearchInfo.TYPE, "1");
                                break;
                            default:
                                break;
                        }
                        break;
                    case "TR":
                        info.Set(SearchInfo.SEARCH, value);
                        info.Set(SearchInfo.TYPE, "2");
                        valid = true;
                        break;
                }
            }
        }

        public SCH(IConnection con, SearchInfo info, string userId)
            : base(con, null)
        {
            this.info = info;
            // BSCH NRQF TRUDHPNB4BUIQV2LAI4HDWRL3KLJTUSXCTAMJHNII TOauto
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            #region EX
            if (info.ContainsKey(SearchInfo.EXTENTION))
            {
                string[] ext = info.Get(SearchInfo.EXTENTION).Split(' ');
                foreach (string extention in ext)
                {
                    sb.Append(" EX" + AdcProtocol.ConvertOutgoing(extention));
                }
            }
            #endregion
            #region TY
            if (info.ContainsKey(SearchInfo.TYPE))
            {
                switch (info.Get(SearchInfo.TYPE))
                {
                    case "0":
                        sb.Append(" TY" + AdcProtocol.ConvertOutgoing("1")); break;
                    case "1":
                        sb.Append(" TY" + AdcProtocol.ConvertOutgoing("2")); break;
                    case "2":
                        sb.Append(" AN" + info.Get(SearchInfo.SEARCH)); break;
                }
            }
            #endregion
            #region AN
            if (info.ContainsKey(SearchInfo.SEARCH) && info.ContainsKey(SearchInfo.TYPE))
                sb.Append(" AN" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.SEARCH)));
            #endregion
            #region NO
            if (info.ContainsKey(SearchInfo.NOSEARCH))
                sb.Append(" NO" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.NOSEARCH)));
            #endregion
            #region TO
            if (info.ContainsKey(SearchInfo.TOKEN))
                sb.Append(" TO" + AdcProtocol.ConvertOutgoing(info.Get(SearchInfo.TOKEN)));
            #endregion
            #region Size Type
            if (info.ContainsKey(SearchInfo.SIZETYPE))
            {
                string size = info.Get(SearchInfo.SIZE);
                switch (info.Get(SearchInfo.SIZETYPE))
                {
                    case "1":
                        sb.Append(" GE" + size);
                        break;
                    case "2":
                        sb.Append(" LE" + size);
                        break;
                    case "3":
                        sb.Append(" EQ" + size);
                        break;
                }
            }
            #endregion
            Raw = string.Format("BSCH {0}{1}\n", userId, sb.ToString());
        }
    }
}