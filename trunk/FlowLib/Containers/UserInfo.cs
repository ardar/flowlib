
/*
 *
 * Copyright (C) 2007 Mattias Blomqvist, patr-blo at dsv dot su dot se
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
    public class UserInfo
    {
        #region Variables
        protected string name = "";
        protected string share_exact = "";
        protected string share = "";
        protected string description = "";
        protected string connection = "";
        protected string email = "";
        protected string cid = "";    // Client ID
        protected string sid = "";    // Session ID
        protected string ip = "";
        protected string pid = null;  // PID (For Me only)
        protected int op = -1;
        protected TagInfo taginfo = new TagInfo(true);
        //        private string supports;

        #endregion
        #region Properties
        /// <summary>
        /// User Id (SID or Display name)
        /// </summary>
        public string ID
        {
            get
            {
                if (SID.Length != 0)
                    return SID;
                else
                    return DisplayName;
            }
        }
        /// <summary>
        /// User Session ID (Direct Connect network, ADC protocol)
        /// </summary>
        public string SID
        {
            set { sid = value; }
            get { return sid; }
        }
        /// <summary>
        /// Display name for user
        /// </summary>
        public string DisplayName
        {
            get
            {
                if (name.Length != 0)
                    return name;
                else
                    return SID;
            }
            set { name = value; }
        }
        /// <summary>
        /// User IP Adress
        /// </summary>
        public string IP
        {
            get { return ip; }
            set { ip = value; }
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
        /// Private Id for User
        /// </summary>
        public string PID
        {
            get { return pid; }
            set { pid = value; }
        }
        /// <summary>
        /// Client Id
        /// </summary>
        public string CID
        {
            get { return cid; }
            set { cid = value; }
        }
        /// <summary>
        /// Indicates if operator status has been set to either operator status or normal
        /// </summary>
        public bool IsSetOP
        {
            get { return (op != -1); }
        }
        /// <summary>
        /// Specifies if this user is operator or not
        /// </summary>
        public bool IsOperator
        {
            get
            {
                return (op == 1);
            }
            set
            {
                if (value)
                    op = 1;
                else
                    op = 0;
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
