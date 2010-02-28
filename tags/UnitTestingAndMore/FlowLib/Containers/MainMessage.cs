
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
    /// Class representing Main Messages in p2p networks like Direct Connect
    /// </summary>
    public class MainMessage
    {
        protected string from = null;
        protected string content = null;
        protected bool me = false;
        /// <summary>
        /// Gets/sets if this message should be showed in me format ( *[nick] [content] )
        /// </summary>
        public bool ShowAsMe
        {
            get { return me; }
            set { me = value; }
        }
        /// <summary>
        /// Gets User Id that indicates who this message was sent from. Can be null if it couldnt be identified.
        /// </summary>
        public string From
        {
            get { return from; }
        }
        /// <summary>
        /// Gets MainMessage content. 
        /// </summary>
        public string Content
        {
            get { return content; }
        }

        /// <summary>
        /// Creates instance of Main Message for internal use.
        /// </summary>
        /// <param name="from">User Id that indicates who sent it</param>
        /// <param name="content">Main Message content</param>
        public MainMessage(string from, string content)
        {
            this.from = from;
            this.content = content;
        }
    }
}
