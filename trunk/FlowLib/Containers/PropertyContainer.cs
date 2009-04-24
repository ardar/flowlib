
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

using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FlowLib.Containers
{
    public class PropertyContainer<TKey, TValue>
    {
        protected SortedList<TKey, TValue> list = null;

        #region Constructors
        public PropertyContainer()
        {
            list = new SortedList<TKey, TValue>(); ;
        }
        public PropertyContainer(PropertyContainer<TKey, TValue> pList)
        {
            list = new SortedList<TKey, TValue>(pList.list);
        }
        public PropertyContainer(IComparer<TKey> comparer)
        {
            list = new SortedList<TKey, TValue>(comparer);
        }
        public PropertyContainer(IDictionary<TKey, TValue> dictionary)
        {
            list = new SortedList<TKey, TValue>(dictionary);
        }
        public PropertyContainer(int capacity)
        {
            list = new SortedList<TKey, TValue>(capacity);
        }
        public PropertyContainer(IDictionary<TKey, TValue> dictionary, IComparer<TKey> comparer)
        {
            list = new SortedList<TKey, TValue>(dictionary, comparer);
        }
        public PropertyContainer(int capacity, IComparer<TKey> comparer)
        {
            list = new SortedList<TKey, TValue>(capacity, comparer);
        }
        #endregion
        #region Properties
        // Summary:
        //     Gets or sets the number of elements that the underlaying System.Collections.Generic.SortedList<TKey,TValue>
        //     can contain.
        //
        // Returns:
        //     The number of elements that the underlaying System.Collections.Generic.SortedList<TKey,TValue>
        //     can contain.
        //
        // Exceptions:
        //   System.ArgumentOutOfRangeException:
        //     System.Collections.Generic.SortedList<TKey,TValue>.Capacity is set to a value
        //     that is less than System.Collections.Generic.SortedList<TKey,TValue>.Count.
        [XmlAttribute(AttributeName = "Capacity")]
        public int Capacity
        {
            get { return list.Capacity; }
            set { list.Capacity = value; }
        }
        //
        // Summary:
        //     Gets the System.Collections.Generic.IComparer<T> for the sorted list.
        //
        // Returns:
        //     The System.IComparable<T> for the current System.Collections.Generic.SortedList<TKey,TValue>.
        public IComparer<TKey> Comparer { get { return list.Comparer; } }
        //
        // Summary:
        //     Gets the number of key/value pairs contained in the System.Collections.Generic.SortedList<TKey,TValue>.
        //
        // Returns:
        //     The number of key/value pairs contained in the System.Collections.Generic.SortedList<TKey,TValue>.
        public int Count { get { return list.Count; } }
        //
        // Summary:
        //     Gets a collection containing the keys in the System.Collections.Generic.SortedList<TKey,TValue>.
        //
        // Returns:
        //     A System.Collections.Generic.IList<T> containing the keys in the System.Collections.Generic.SortedList<TKey,TValue>.
        public IList<TKey> Keys
        {
            get { return list.Keys; }
        }
        //
        // Summary:
        //     Gets a collection containing the values in the System.Collections.Generic.SortedList<TKey,TValue>.
        //
        // Returns:
        //     A System.Collections.Generic.IList<T> containing the keys in the System.Collections.Generic.SortedList<TKey,TValue>.
        public IList<TValue> Values
        {
            get { return list.Values; }
        }

        // Summary:
        //     Gets or sets the value associated with the specified key.
        //
        // Parameters:
        //   key:
        //     The key whose value to get or set.
        //
        // Returns:
        //     The value associated with the specified key. If the specified key is not
        //     found, attempting to get it returns the default value for the value type
        //     TValue, and attempting to set it creates a new element using the specified
        //     key.
        //
        // Exceptions:
        //   System.ArgumentNullException:
        //     key is null.
        //
        //   System.Collections.Generic.KeyNotFoundException:
        //     The property is retrieved and key does not exist in the collection.
        public TValue this[TKey key]
        {
            get { return list[key]; }
            set { list[key] = value; }
        }

        #endregion
        #region Functions
        public void Add(TKey key, TValue value) { list.Add(key, value); }
        public void Clear() { list.Clear(); }
        public bool ContainsKey(TKey key) { return list.ContainsKey(key); }
        public bool ContainsValue(TValue value) { return list.ContainsValue(value); }
        //public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { return list.GetEnumerator(); }
        //IEnumerator IEnumerable.GetEnumerator()
        //{
        //    return list.GetEnumerator();
        //}
        public int IndexOfKey(TKey key) { return list.IndexOfKey(key); }
        public int IndexOfValue(TValue value) { return list.IndexOfValue(value); }
        public bool Remove(TKey key) { return list.Remove(key); }
        public void RemoveAt(int index) { list.RemoveAt(index); }
        public void TrimExcess() { list.TrimExcess(); }
        public bool TryGetValue(TKey key, out TValue value) { return list.TryGetValue(key, out value); }
        #endregion
        #region Own functions
        public void Set(TKey key, TValue obj)
        {
            list.Remove(key);
            list.Add(key, obj);
        }

        public TValue Get(TKey key)
        {
            TValue obj = default(TValue);
            list.TryGetValue(key, out obj);
            return obj;
        }
        #endregion

        [XmlArray(IsNullable=true, ElementName="Items")]
        [XmlArrayItem(ElementName="Item")]
        public PropertyContainerItems Items
        {
            get { return new PropertyContainerItems(ref list); }
            set
            {
                if (value != null)
                {
                    list = value.list;
                }
            }
        }

        #region XmlSeralization
        [XmlIgnore()]
        public bool CapacitySpecified
        {
            get { return Capacity != 4; }
            set { }
        }
        [XmlIgnore()]
        public bool ItemsSpecified
        {
            get { return list.Count > 0; }
            set { }
        }
        #endregion


        public class PropertyContainerItems :IEnumerable<FlowKeyValuePair<TKey, TValue>>, IEnumerable
        {
            public SortedList<TKey, TValue> list = null;

            public PropertyContainerItems(ref SortedList<TKey, TValue> list)
            {
                this.list = list;
            }

            #region IEnumerable<FlowKeyValuePair<TKey,TValue>> Members
            public void Add(System.Object obj)
            {
                FlowKeyValuePair<TKey, TValue> flow = obj as FlowKeyValuePair<TKey, TValue>;
                if (flow == null)
                    return;
                list.Add(flow.Key, flow.Value);
            }

            public IEnumerator<FlowKeyValuePair<TKey, TValue>> GetEnumerator()
            {
                return new FlowEnumerator(list.GetEnumerator());
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            #endregion

        }

        class FlowEnumerator : IEnumerator<FlowKeyValuePair<TKey, TValue>>
        {
            IEnumerator<KeyValuePair<TKey, TValue>> enumerator = null;

            public FlowEnumerator()
            {

            }

            public FlowEnumerator(IEnumerator<KeyValuePair<TKey, TValue>> enumerator)
            {
                this.enumerator = enumerator;
            }

            #region IEnumerator<FlowKeyValuePair<TKey,TValue>> Members
            public FlowKeyValuePair<TKey, TValue> Current
            {
                get
                {
                    KeyValuePair<TKey, TValue> value = enumerator.Current;
                    return new FlowKeyValuePair<TKey, TValue>(value.Key, value.Value);
                }
            }
            #endregion

            #region IDisposable Members
            public void Dispose()
            {
                if (enumerator != null)
                    enumerator.Dispose();
            }
            #endregion

            #region IEnumerator Members
            object IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                if (enumerator != null)
                    return enumerator.MoveNext();
                else
                    return false;
            }

            public void Reset()
            {
                if (enumerator != null)
                    enumerator.Reset();
            }
            #endregion
        }
    }
}
