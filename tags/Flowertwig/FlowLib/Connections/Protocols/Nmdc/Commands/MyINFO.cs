using FlowLib.Connections.Entities;
using FlowLib.Entities;

namespace FlowLib.Connections.Protocols.Nmdc.Commands
{
    public class MyINFO : HubMessage
    {
        [System.Flags]
        public enum UserStatusFlag
        {
            Unknown = 0,
            Normal = 1,
            Away = 2,
            Server = 4,
            Fireball = 8,
            TLS = 16
        }

        public static UserStatusFlag ConvertByteToStatusFlag(byte value)
        {
            UserStatusFlag flag = UserStatusFlag.Unknown;
            if ((1 & value) == 1)
                flag |= UserStatusFlag.Normal;
            if ((2 & value) == 2)
                flag |= UserStatusFlag.Away;
            if ((4 & value) == 4)
                flag |= UserStatusFlag.Server;
            if ((8 & value) == 8)
                flag |= UserStatusFlag.Fireball;
            if ((16 & value) == 16)
                flag |= UserStatusFlag.TLS;
            return flag;
        }

        public static byte ConvertStatusFlagToByte(UserStatusFlag statusFlag)
        {
            byte statusFlagRawByte = 0;
            if ((UserStatusFlag.Normal & statusFlag) == UserStatusFlag.Normal)
                statusFlagRawByte |= 1;
            if ((UserStatusFlag.Away & statusFlag) == UserStatusFlag.Away)
                statusFlagRawByte |= 2;
            if ((UserStatusFlag.Server & statusFlag) == UserStatusFlag.Server)
                statusFlagRawByte |= 4;
            if ((UserStatusFlag.Fireball & statusFlag) == UserStatusFlag.Fireball)
                statusFlagRawByte |= 8;
            if ((UserStatusFlag.TLS & statusFlag) == UserStatusFlag.TLS)
                statusFlagRawByte |= 16;
            return statusFlagRawByte;
        }

        protected UserInfo info = null;
        protected byte statusFlag;

        public byte StatusFlag
        {
            get { return statusFlag; }
            set { statusFlag = value; }
        }

        public UserInfo UserInfo
        {
            get { return info; }
        }
        // Receiving
        public MyINFO(Hub hub, string raw)
            : base(hub, raw)
        {
            int pos1, pos2;
            if ((pos1 = raw.IndexOf("$MyINFO $ALL ")) != -1)
            {
                if ((pos2 = raw.IndexOf(" ", 13)) > 13)
                {
                    from = raw.Substring(13, pos2 - 13);
                    // Zline<Z++ V:2.00,M:P,H:4/4/25,S:2>$ $DSL$twizmflow@gmail.com$0$
                    string temp = raw.Substring(++pos2);
                    string[] sections = temp.Split('$');
                    info = new UserInfo();
                    info.DisplayName = from;
                    info.Set(UserInfo.STOREID, hub.StoreId + from);
                    if (sections.Length == 6)
                    {
                        int pos = 0;
                        if ((pos = sections[0].LastIndexOf('<')) != -1)
                        {
                            info.Description = sections[0].Substring(0, pos);
                            // <Z++ V:2.00,M:P,H:4/4/25,S:2>
                            info.TagInfo.Tag = sections[0].Substring(pos);
                            // Parsing of tag
                        }
                        else
                        {
                            info.Description = sections[0];
                        }
                        info.Connection = sections[2];
                        if (info.Connection.Length > 0)
                        {
                            statusFlag = (byte)info.Connection[info.Connection.Length - 1];
                            UserStatusFlag flag = ConvertByteToStatusFlag(statusFlag);
                            if ((flag & UserStatusFlag.TLS) == UserStatusFlag.TLS)
                                info.Set(UserInfo.SECURE, "");
                        }

                        info.Email = sections[3];
                        info.Share = sections[4];

                    }
                    if (!string.IsNullOrEmpty(info.DisplayName))
                        IsValid = true;
                }
            }
        }
        // Sending
        public MyINFO(Hub hub)
            : this(hub, ConvertStatusFlagToByte(UserStatusFlag.Normal))
        { }

        public MyINFO(Hub hub, byte flg)
            : base(hub, null)
        {
            this.info = hub.Me;
            this.statusFlag = flg;
            var tag = new System.Text.StringBuilder();
            tag.Append("<");
            tag.Append(info.TagInfo.Version);
            tag.Append(",M:");
            switch (info.TagInfo.Mode) {
                case ConnectionTypes.Socket5: // Socket5
                    tag.Append("5"); break;
                case ConnectionTypes.Direct: // Active
                    tag.Append("A"); break;
                case ConnectionTypes.Passive: // Passive
                default:
                    tag.Append("P"); break;
            }
            tag.Append(",H:");
            tag.Append(info.TagInfo.Normal + "/");   // Normal
            tag.Append(info.TagInfo.Regged + "/");   // Regged
            tag.Append(info.TagInfo.OP + "");    // OP
            tag.Append(",S:" + (info.TagInfo.Slots == -1 ? 0 : info.TagInfo.Slots) + ">");

            UserStatusFlag status = ConvertByteToStatusFlag(statusFlag);
            if (info.ContainsKey(UserInfo.SECURE))
                status |= UserStatusFlag.TLS;

            Raw = "$MyINFO $ALL "
                  + info.DisplayName
                  + " "
                  + info.Description
                  + tag
                  + "$ $"
                  + info.Connection
                  + hub.Protocol.Encoding.GetString(new[] { ConvertStatusFlagToByte(status) }) + "$"
                  + info.Email + "$"
                  + (string.IsNullOrEmpty(info.Share) ? "0" : info.Share)
                  + "$|";
            IsValid = true;
        }

    }
}