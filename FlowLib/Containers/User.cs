
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
        /// User information for current user
        /// </summary>
        public UserInfo UserInfo
        {
            get { return userinfo; }
            set
            {
                // If this is just a update. Dont empty all other values.
                if (value.DisplayName.Length != 0)
                    userinfo.DisplayName = value.DisplayName;
                if (value.TagInfo.Mode != ConnectionTypes.Unknown)
                    userinfo.TagInfo.Mode = value.TagInfo.Mode;
                // Tag
                if (value.TagInfo.Tag != null)
                {
                    userinfo.TagInfo.Tag = value.TagInfo.Tag;
                }
                else
                {   // We need to regenerate the tag
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
                    if (value.SID.Length != 0)
                        userinfo.SID = value.SID;
                    if (value.CID.Length != 0)
                        userinfo.CID = value.CID;
                    // We want to create the tag if we have a version and a CID
                    // This is because we only want to create the tag if it is ADC protocol
                    if (userinfo.TagInfo.Version != null && userinfo.CID.Length != 0 && userinfo.ID.Length != 0)
                        userinfo.TagInfo.CreateTag();
                }
                if (value.IP.Length != 0)
                    userinfo.IP = value.IP;
                if (value.Share.Length != 0)
                    userinfo.Share = value.Share;
                if (value.Email.Length != 0)
                    userinfo.Email = value.Email;
                if (value.Description.Length != 0)
                    userinfo.Description = value.Description;
                if (value.Connection.Length != 0)
                    userinfo.Connection = value.Connection;
                if (value.IsSetOP)
                    userinfo.IsOperator = value.IsOperator;

                //userinfo = value;
                // TODO : Gui Update thingy.
                //updateView();
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
            if (userinfo.DisplayName.Length == 0)
                return userinfo.SID;
            return userinfo.DisplayName;
        }
    }
}
