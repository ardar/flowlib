
/*
 *
 * Copyright (C) 2009 Mattias Blomqvist, patr-blo at dsv dot su dot se
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 *
 */

using FlowLib.Enums;

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing User Information. This is used by User Class.
    /// </summary>
    public class UserInfo : PropertyContainer<string, string>
    {
        /// <summary>
        /// Private Id for User
        /// </summary>
        public const string PID = "pid";
        /// <summary>
        /// Client Id
        /// </summary>
        public const string CID = "cid";
        /// <summary>
        /// Session ID (Direct Connect network, ADC protocol)
        /// </summary>
        public const string SID = "sid";
        /// <summary>
        /// Port for Udp Connections
        /// </summary>
        public const string UDPPORT = "udpport";
        /// <summary>
        /// User IP Adress
        /// </summary>
        public const string IP = "ip";
        /// <summary>
        /// Client support secure connections
        /// </summary>
        public const string SECURE = "secure";

        public const string STOREID = "storeid";

        public const int ACCOUNT_FLAG_USER = 0;
        public const int ACCOUNT_FLAG_BOT = 1;
        public const int ACCOUNT_FLAG_REGISTERED = 2;
        public const int ACCOUNT_FLAG_OPERATOR = 4;
        public const int ACCOUNT_FLAG_SUPERUSER = 8;
        public const int ACCOUNT_FLAG_HUBOWNER = 16;

        #region Variables
        protected string name = "";
        protected string share_exact = "";
        protected string share = "";
        protected string description = "";
        protected string connection = "";
        protected string email = "";
        protected int account = -1;

        protected TagInfo taginfo = new TagInfo(true);

        #endregion
        #region Properties
        /// <summary>
        /// User account status
        /// Available flags are ACCOUNT_FLAG_*
        /// To know if super user use this:
        /// ((ACCOUNT_FLAG_SUPERUSER & value) == ACCOUNT_FLAG_SUPERUSER)
        /// To set super user:
        /// value |= UserInfo.ACCOUNT_FLAG_SUPERUSER
        /// To remove super user:
        /// value ^= UserInfo.ACCOUNT_FLAG_SUPERUSER
        /// </summary>
        public int Account
        {
            get { return account; }
            set { account = value; }
        }
        /// <summary>
        /// User Id (SID or Display name)
        /// </summary>
        public string ID
        {
            get
            {
                if (ContainsKey(UserInfo.SID))
                    return Get(UserInfo.SID);
                else
                    return DisplayName;
            }
        }

        /// <summary>
        /// Store Id (Used for Source)
        /// If STOREID exist as key that will be returned.
        /// Else If CID exist as key that will be returned.
        /// Else ID will be returned
        /// </summary>
        public string StoreID
        {
            get
            {
                if (ContainsKey(STOREID))
                    return Get(STOREID);
                else if (ContainsKey(CID))
                    return Get(CID);
                else
                    return ID;
            }
        }

        /// <summary>
        /// Display name for user
        /// </summary>
        public string DisplayName
        {
            get
            {
                return name;
            }
            set { name = value; }
        }
        /// <summary>
        /// User description
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
        /// <summary>
        /// User Email
        /// </summary>
        public string Email
        {
            get { return email; }
            set { email = value; }
        }
        /// <summary>
        /// User Tag Information
        /// </summary>
        public TagInfo TagInfo
        {
            get { return taginfo; }
            set
            {
                taginfo = value;
            }
        }
        /// <summary>
        /// User Connection
        /// </summary>
        public string Connection
        {
            get { return connection; }
            set { connection = formatConnection(value); }
        }
        /// <summary>
        /// Share in IEEE 1541 standard format
        /// </summary>
        public string ShareIEEE1541
        {
            //set { share = value; }
            get { return share; }
        }
        /// <summary>
        /// Share in exact format
        /// </summary>
        public string Share
        {
            set
            {
                share_exact = value;
                formatShare(value);
            }
            get { return share_exact; }
        }
        /// <summary>
        /// Specifies if this user is operator or not
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsOperator
        {
            get
            {
                return ((ACCOUNT_FLAG_OPERATOR | account) == ACCOUNT_FLAG_OPERATOR);
            }
            set
            {
                if (value)
                    account &= UserInfo.ACCOUNT_FLAG_OPERATOR;
                else
                    account ^= UserInfo.ACCOUNT_FLAG_OPERATOR;
            }
        }
        /// <summary>
        /// Indicates what connection mode user have.
        /// </summary>
        public ConnectionTypes Mode
        {
            get { return taginfo.Mode; }
            set { taginfo.Mode = value; }
        }
        #endregion

        public UserInfo() { }

        public UserInfo(UserInfo value)
        {
            TagInfo.GenerateTag = value.TagInfo.GenerateTag;
            // If this is just a update. Dont empty all other values.
            if (value.DisplayName.Length != 0)
                DisplayName = value.DisplayName;
            TagInfo = new TagInfo(value.TagInfo);

            if (value.Share.Length != 0)
                Share = value.Share;
            if (value.Email.Length != 0)
                Email = value.Email;
            if (value.Description.Length != 0)
                Description = value.Description;
            if (value.Connection.Length != 0)
                Connection = value.Connection;
            if (value.Account != -1)
                Account = value.Account;

            lock (value)
            {
                foreach (System.Collections.Generic.KeyValuePair<string, string> pair in value.list)
                {
                    Set(pair.Key, pair.Value);
                }
            }
        }
        #region Functions
        protected void formatShare(string instr)
        {
            long share = 0;
            try
            {
                share = long.Parse(instr);
            }
            catch (System.Exception)
            {
                share = 0;
            }
            Utils.Convert.General.BinaryPrefixes bp;
            this.share = string.Format("{0} {1}", Utils.Convert.General.FormatBytes(share, 2, out bp), bp);
        }

        protected string formatConnection(string str)
        {
            if (str.Length == 0)
                return str;
            if (str.Length > 20)
                str.Substring(0, 20);
            return str.Substring(0, str.Length);
        }
        #endregion
    }
}
