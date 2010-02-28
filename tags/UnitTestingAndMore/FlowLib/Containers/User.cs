
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
    /// Class representing User
    /// </summary>
    public class User
    {
        private UserInfo userinfo = new UserInfo();

        /// <summary>
        /// Creating User with username
        /// </summary>
        /// <param name="username">Display name</param>
        public User(string displayName)
        {
            userinfo.DisplayName = displayName;
            userinfo.Mode = 0;
            //userinfo.OP = false;
        }

        /// <summary>
        /// Creating User with userinfo
        /// </summary>
        /// <param name="usrInfo">UserInfo to create user from</param>
        public User(UserInfo usrInfo)
        {
            userinfo = usrInfo;
        }
        /// <summary>
        /// User information for current user
        /// </summary>
        public UserInfo UserInfo
        {
            get { return userinfo; }
            set
            {
                userinfo.TagInfo.GenerateTag = value.TagInfo.GenerateTag;
                // If this is just a update. Dont empty all other values.
                if (value.DisplayName.Length != 0)
                    userinfo.DisplayName = value.DisplayName;
                if (value.TagInfo.Mode != ConnectionTypes.Unknown)
                    userinfo.TagInfo.Mode = value.TagInfo.Mode;
                if (!value.TagInfo.GenerateTag/* value.TagInfo.Tag != null*/)
                    userinfo.TagInfo.Tag = value.TagInfo.Tag;
                if (value.TagInfo.Version != null)
                    userinfo.TagInfo.Version = value.TagInfo.Version;
                if (value.TagInfo.Normal != -1)
                    userinfo.TagInfo.Normal = value.TagInfo.Normal;
                if (value.TagInfo.Regged != -1)
                    userinfo.TagInfo.Regged = value.TagInfo.Regged;
                if (value.TagInfo.OP != -1)
                    userinfo.TagInfo.OP = value.TagInfo.OP;
                if (value.TagInfo.Slots != -1)
                    userinfo.TagInfo.Slots = value.TagInfo.Slots;
                if (value.Share.Length != 0)
                    userinfo.Share = value.Share;
                if (value.Email.Length != 0)
                    userinfo.Email = value.Email;
                if (value.Description.Length != 0)
                    userinfo.Description = value.Description;
                if (value.Connection.Length != 0)
                    userinfo.Connection = value.Connection;
                if (value.Account != -1)
                    userinfo.Account = value.Account;

                PropertyContainer<string, string>.PropertyContainerItems items = value.Items;
                lock (items)
                {
                    foreach (FlowKeyValuePair<string, string> pair in items)
                    {
                        userinfo.Set(pair.Key, pair.Value);
                    }
                }
            }
        }
        /// <summary>
        /// User id
        /// </summary>
        public string ID
        {
            get { return userinfo.ID; }
        }
		/// <summary>
		/// User Storage Id.
		/// </summary>
		public string StoreID
		{
			get { return userinfo.StoreID; }
		}
        /// <summary>
        /// Indicates if this user is a operator or not
        /// </summary>
        public bool IsOperator
        {
            get { return userinfo.IsOperator; }
            set { userinfo.IsOperator = value; }
        }
        /// <summary>
        /// Display name for user
        /// </summary>
        public string DisplayName
        {
            get { return userinfo.DisplayName; }
        }
        /// <summary>
        /// User description
        /// </summary>
        public string Description
        {
            get { return userinfo.Description; }
        }
        /// <summary>
        /// User tag information
        /// </summary>
        public TagInfo Tag
        {
            get { return userinfo.TagInfo; }
        }
        /// <summary>
        /// User Connection
        /// </summary>
        public string Connection
        {
            get { return userinfo.Connection; }
        }
        /// <summary>
        /// Returns Display name or SID if displayname is empty
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (userinfo.ContainsKey(UserInfo.SID))
                return userinfo.Get(UserInfo.SID);
            return userinfo.DisplayName;
        }
    }
}
