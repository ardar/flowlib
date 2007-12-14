
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

using System;

namespace FlowLib.Events
{
    public delegate void FmdcEventHandler(object sender, FmdcEventArgs e);
    
    /// <summary>
    /// Provides data for the internal events.
    /// </summary>
    public class FmdcEventArgs : EventArgs
    {
        protected int action = -1;
        protected object data = null;
        protected bool handled = false;

        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        public FmdcEventArgs(int action) : this(action,null) { }
        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        /// <param name="data">Object that is used to give more info about the action.</param>
        public FmdcEventArgs(int action, object data)
        {
            this.action = action;
            this.data = data;
        }

        /// <summary>
        /// Gets Action that has/should be performed.
        /// </summary>
        public int Action
        {
            get { return action; }
        }
        /// <summary>
        /// Gets/sets object that is used to give more info about action.
        /// </summary>
        public object Data
        {
            get { return data; }
            set { data = value; }
        }
        /// <summary>
        /// Gets/sets if this even has already been handled by someone else.
        /// </summary>
        public bool Handled
        {
            get { return handled; }
            set { handled = value; }
        }
    }
}
