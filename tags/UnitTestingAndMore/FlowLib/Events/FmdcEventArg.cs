
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

using System;

namespace FlowLib.Events
{
    public delegate void FmdcEventHandler(object sender, FmdcEventArgs e);


    public class FmdcEventArgs : FmdcEventArgs<int, object>
    {
        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        public FmdcEventArgs(int action) : base(action, null) { }
        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        /// <param name="data">Object that is used to give more info about the action.</param>
        public FmdcEventArgs(int action, object data) : base(action, data) { }
    }

    /// <summary>
    /// Provides data for the internal events.
    /// </summary>
    public class FmdcEventArgs<TAction, TData> : EventArgs
    {
        protected TAction action = default(TAction);
        protected TData data = default(TData);
        protected bool handled = false;

        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        public FmdcEventArgs(TAction action) : this(action, default(TData)) { }
        /// <summary>
        /// Initializes a new instance of this EventArgs
        /// </summary>
        /// <param name="action">Action that has/should be performed</param>
        /// <param name="data">Object that is used to give more info about the action.</param>
        public FmdcEventArgs(TAction action, TData data)
        {
            this.action = action;
            this.data = data;
        }

        /// <summary>
        /// Gets Action that has/should be performed.
        /// </summary>
        public TAction Action
        {
            get { return action; }
        }
        /// <summary>
        /// Gets/sets object that is used to give more info about action.
        /// </summary>
        public TData Data
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
