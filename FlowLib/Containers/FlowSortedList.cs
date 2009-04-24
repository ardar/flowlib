
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
using System.Collections.Generic;
using System.Text;

namespace FlowLib.Containers
{
    /// <summary>
    /// This Class is like the List&#60;T&#62; class with one big diffrent.
    /// It is always sorted.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FlowSortedList<T> : System.Collections.Generic.IEnumerable<T>
    {
        #region Delegates
        public delegate void SingleChangedDelegate(int pos, T item);
        public delegate void ClearedDelegate(int prevCount);
        public delegate void SortedDelegate(IComparer<T> comp);
        #endregion
        #region Variables
        protected List<T> list1 = new List<T>();
        protected IComparer<T> comparer;
        protected bool trigger = false;
        #endregion
        #region Events
        public event SingleChangedDelegate ItemAdded;
        public event SingleChangedDelegate ItemRemoved;
        public event ClearedDelegate ItemsRemoved;
        public event SortedDelegate ItemsSorted;
        #endregion
        #region Properties
        /// <summary>
        /// Gets or sets the total number of elements the internal data structure can
        /// hold without resizing.
        /// </summary>
        public int Capacity
        {
            get { return list1.Capacity; }
            set { list1.Capacity = value; }
        }
        /// <summary>
        /// Gets the number of elements actually contained in the list
        /// </summary>
        public int Count
        {
            get { return list1.Count; }
        }
        public bool TriggerEvents
        {
            get { return trigger; }
            set { trigger = value; }
        }

        public IComparer<T> Comparer
        {
            get { return comparer; }
        }
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the FlowSortedList with the default comparer System.Collections.Generic.Comparer&#60;T&#62;.Default.
        /// </summary>
        public FlowSortedList()
        {
            comparer = Comparer<T>.Default;
            ItemAdded = new FlowSortedList<T>.SingleChangedDelegate(FlowSortedList_ItemAdded);
            ItemRemoved = new FlowSortedList<T>.SingleChangedDelegate(FlowSortedList_ItemRemoved);
            ItemsRemoved = new FlowSortedList<T>.ClearedDelegate(FlowSortedList_ItemsRemoved);
            ItemsSorted = new FlowSortedList<T>.SortedDelegate(FlowSortedList_ItemsSorted);
        }

        void FlowSortedList_ItemsSorted(IComparer<T> comp) {}
        void FlowSortedList_ItemsRemoved(int prevCount) {}
        void FlowSortedList_ItemRemoved(int pos, T item) {}
        void FlowSortedList_ItemAdded(int pos, T item) { }

        /// <summary>
        /// Initializes a new instance of the FlowSortedList with the specified comparer.
        /// </summary>
        /// <param name="comp">
        /// The System.Collections.Generic.IComparer<T> implementation to use when comparing
        /// elements, or null to use the default comparer System.Collections.Generic.Comparer<T>.Default.
        /// </param>
        public FlowSortedList(IComparer<T> comp)
            : this()
        {
            if (comp == null)
                comp = Comparer<T>.Default;
            comparer = comp;
        }

        /// <summary>
        /// Initializes a new instance of the FlowSortedList
        /// that contains elements copied from the specified collection and has sufficient
        /// capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="listToCopy">The collection whose elements are copied to the new list.</param>
        /// <param name="comp">
        /// The System.Collections.Generic.IComparer<T> implementation to use when comparing
        /// elements, or null to use the default comparer System.Collections.Generic.Comparer<T>.Default.
        /// </param>
        public FlowSortedList(FlowSortedList<T> listToCopy)
            : this()
        {
            list1 = new List<T>(listToCopy.list1);
        }
        /// <summary>
        /// Initializes a new instance of the FlowSortedList
        /// that contains elements copied from the specified collection and has sufficient
        /// capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="listToCopy">The collection whose elements are copied to the new list.</param>
        public FlowSortedList(FlowSortedList<T> listToCopy,IComparer<T> comp)
            : this(listToCopy)
        {
            if (comp == null)
                comp = Comparer<T>.Default;
            Sort(comp);
        }
        #endregion
        #region Functions
        /// <summary>
        /// Gets the element at the specified index.
        /// </summary>
        /// <param name="pos">The zero-based index of the element to get.</param>
        /// <returns>The element at the specified index.</returns>
        public T this[int pos]
        {
            get
            {
                // Is Range Valid?
                if (pos < list1.Count && pos >= 0)
                {
                    return list1[pos];
                }
                return default(T);
            }
        }

        /// <summary>
        /// Adds an object to the list.
        /// </summary>
        /// <param name="item">The object to be added</param>
        /// <returns>returns pos it has been added on</returns>
        public int Add(T item)
        {
            #region Checks where to add Item
            int pos = 0;
            if (list1.Count > 0)
            {
                // Check against last item in list.
                if (comparer.Compare(list1[list1.Count - 1], item) <= 0)    // If it is equal or bigger add last.
                    pos = list1.Count;
                else
                    if (list1.Count == 1)
                        pos = 0;
                    else
                        pos = Find(item, 0, list1.Count - 2);
            }
            #endregion
            // TODO : Get pos where to add item.
            list1.Insert(pos, item);    // This will insert item before the current 0 item.
            if (trigger)
                ItemAdded(pos, item);
            return pos;
        }
        /// <summary>
        /// Adds the elements of the specified collection.
        /// </summary>
        /// <param name="collection">
        /// The collection whose elements should be added.
        /// The collection itself cannot be null, but it can contain elements that are
        /// null, if type T is a reference type.
        /// </param>
        public void AddRange(IEnumerable<T> collection)
        {
            foreach (T item in collection)
            {
                Add(item);
            }
        }
        /// <summary>
        /// Removes the first occurrence of a specific object
        /// </summary>
        /// <param name="item">The object to be removed</param>
        /// <returns>returns -1 if item wasn't found. Else it will return pos where it removed item</returns>
        public int Remove(T item)
        {
            int pos = Find(item);
            if (pos != -1)
            {
                list1.RemoveAt(pos);
                if (trigger)
                    ItemRemoved(pos, item);
            }
            return pos;
        }
        /// <summary>
        /// Removes the element at the specified index.
        /// </summary>
        /// <param name="pos">The zero-based index of the element to remove.</param>
        public void RemoveAt(int pos)
        {
            if (pos >= 0 && pos < list1.Count)
            {
                list1.RemoveAt(pos);
            }
        }
        /// <summary>
        /// Removes all elements.
        /// </summary>
        public void Clear()
        {
            int count = list1.Count;
            list1.Clear();
            if (trigger)
                ItemsRemoved(count);
        }
        /// <summary>
        /// Searches for an object and returns the pos in list.
        /// </summary>
        /// <param name="item">The object to be found</param>
        /// <returns>If item was found position where object is will be returned. Else -1</returns>
        public int Find(T item)
        {
            int pos = Find(item, 0, list1.Count);
            if (pos >= 0 && pos < list1.Count)
            {
                if (comparer.Compare(list1[pos], item) == 0)
                    return pos;
            }
            return -1;
        }
        /// <summary>
        /// Searches for an object in a specific range of list and returns the pos in list.
        /// Object is not found pos where it should be will be returned anyway.
        /// </summary>
        /// <param name="finditem">The object to be found</param>
        /// <param name="start">Start pos of range</param>
        /// <param name="end">End pos of range</param>
        /// <returns>Pos where object is or should be</returns>
        protected int Find(T finditem, int start, int end)
        {
            // Is Range Valid?
            if ((start <= list1.Count && start >= 0) &&
                (end <= list1.Count && end >= 0) &&
                (start <= end))
            {
                int pos = (start + end) / 2;
                T item = list1[pos];
                #region Compare finditem with item. If equal, return pos.
                int comp = comparer.Compare(item, finditem);
                switch (comp)
                {
                    case -1:
                        if (start == end)   // If we havnt found founditem. return pos where it should have been.
                            return pos + 1;
                        //return pos;
                        start = pos + 1;
                        break;
                    case 0:
                        return pos;
                    case 1:
                        if (start == end)   // If we havnt found founditem. return pos where it should have been.
                            return pos;
                        end = pos - 1;
                        break;
                }
                #endregion
                // Have have go a new range. Search in this.
                int value = Find(finditem, start, end);
                if (value == -1)
                    return pos;
                else
                    return value;
            }
            else
            {
                // Range is not valid. Return -1
                return -1;
            }
        }
        /// <summary>
        /// Searches the entire sorted list for an element
        /// using the default comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">The object to locate. The value can be null for reference types.</param>
        /// <returns>
        /// The zero-based index of item in the sorted list,
        /// if item is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than item or, if there is
        /// no larger element, the bitwise complement of list.Count.
        /// </returns>
        public int BinarySearch(T item)
        {
            return BinarySearch(item, comparer);
        }
        /// <summary>
        /// Searches the entire sorted list for an element
        /// using the specified comparer and returns the zero-based index of the element.
        /// </summary>
        /// <param name="item">
        /// The object to locate. The value can be null for reference types.
        /// </param>
        /// <param name="comparer">
        /// The System.Collections.Generic.IComparer<T> implementation to use when comparing
        /// elements.-or-null to use the default comparer System.Collections.Generic.Comparer<T>.Default.
        /// </param>
        /// <returns>
        /// The zero-based index of item in the sorted list,
        /// if item is found; otherwise, a negative number that is the bitwise complement
        /// of the index of the next element that is larger than item or, if there is
        /// no larger element, the bitwise complement of list.Count.
        /// </returns>
        public int BinarySearch(T item, IComparer<T> comp)
        {
            if (comp == null)
                comp = Comparer<T>.Default;
            return list1.BinarySearch(item, comp);
        }
        /// <summary>
        /// Sorts the elements in the entire list using the specified comparer.
        /// </summary>
        /// <param name="comp">
        /// The System.Collections.Generic.IComparer<T> implementation to use when comparing
        /// elements, or null to use the default comparer System.Collections.Generic.Comparer<T>.Default.
        /// </param>
        public void Sort(IComparer<T> comp)
        {
            if (comp == null)
                comp = Comparer<T>.Default;
            comparer = comp;
            list1.Sort(comparer);
            if (trigger)
                ItemsSorted(comparer);
        }
        /// <summary>
        /// Reverses the order of the elements in the entire list.
        /// </summary>
        public void Reverse()
        {
            list1.Reverse();
        }
        /// <summary>
        /// Every object in C# inherits the ToString method, which returns a string representation of that object.
        /// </summary>
        /// <returns>string representation of object.</returns>
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < list1.Count; i++)
            {
                str += list1[i].ToString() + "\r\n";
            }
            return str;
        }
        #endregion

        public T[] ToArray()
        {
            return list1.ToArray();
        }

        #region IEnumerable<T> Members
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return list1.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return list1.GetEnumerator();
        }

        #endregion
    }
}
