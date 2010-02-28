
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

namespace FlowLib.Containers
{
    /// <summary>
    /// Class representing Private Messages (User to user messages) in p2p networks like Direct Connect
    /// </summary>
    public class PrivateMessage
    {
        protected string to = null;
        protected string from = null;
        protected string content = null;
        protected bool me = false;
        protected string group = null;
        /// <summary>
        /// Group/chatroom where Pm was received from.
        /// </summary>
        public string Group
        {
            get { return group; }
        }
        /// <summary>
        /// Gets/sets if this message should be showed in me format ( *[nick] [content] )
        /// </summary>
        public bool ShowAsMe
        {
            get { return me; }
            set { me = value; }
        }
        /// <summary>
        /// User Id of person who sent message
        /// </summary>
        public string To
        {
            get { return to; }
        }
        /// <summary>
        /// User Id of person who received message
        /// </summary>
        public string From
        {
            get { return from; }
        }
        /// <summary>
        /// Private Message content
        /// </summary>
        public string Content
        {
            get { return content; }
        }

        /// <summary>
        /// Creates instance of Private Message for internal use.
        /// </summary>
        /// <param name="to">User Id of person who sent message</param>
        /// <param name="from">User Id of person who received message</param>
        /// <param name="content">Private Message content</param>
        public PrivateMessage(string to, string from, string content)
            : this(to, from, content, null)
        {

        }

        /// <summary>
        /// Creates instance of Private Message for internal use.
        /// </summary>
        /// <param name="to">User id of person who sent message</param>
        /// <param name="from">User id of person who received message</param>
        /// <param name="content">Private Message content</param>
        /// <param name="group">Group id of group who sent message</param>
        public PrivateMessage(string to, string from, string content, string group)
        {
            this.to = to;
            this.from = from;
            this.content = content;
            this.group = group;
        }
    }
}
