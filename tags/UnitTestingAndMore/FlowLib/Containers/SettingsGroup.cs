
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

using FlowLib.Events;

using System;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace FlowLib.Containers
{
    [XmlRoot("Settings")]
    public class SettingsGroup
    {
        [XmlIgnore()]
        public int Count
        {
            get { return list.Count; }
        }

        [XmlArrayItem("Item", IsNullable=false)]
        public SettingItem[] Items
        {
            get {
                SettingItem[] items = new SettingItem[ list.Values.Count ];
                list.Values.CopyTo(items ,0);
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i].IsDefault)
                        items.SetValue(null, i);
                }
                return items;
            }
            set {
              if( value == null ) return;
              SettingItem[] items = (SettingItem[])value;
              list.Clear();
              foreach (SettingItem item in items)
              {
                  item.UpdatedSetting += new FmdcEventHandler(item_UpdatedSetting);
                  list.Add(item.Key, item);
              }
            }
        }
        //public System.Collections.ArrayList
        protected System.Collections.Generic.SortedList<int, SettingItem> list = new System.Collections.Generic.SortedList<int, SettingItem>();

        public event FmdcEventHandler UpdatedSetting;

        /// <summary>
        /// If key doesnt exist it will be added and value will be set to default.
        /// If it exist default value will be changed.
        /// </summary>
        /// <param name="key">Key identifier</param>
        /// <param name="obj">object that you want to set as the default value</param>
        public void SetDefault(int key, object obj)
        {
            if (!list.ContainsKey(key))
            {
                SettingItem item = new SettingItem(obj);
                item.Key = key;
                item.UpdatedSetting += new FmdcEventHandler(item_UpdatedSetting);
                list.Add(key, item);
            }
            else
            {
                list[key].DefaultValue = obj;
            }
        }

        /// <summary>
        /// Adds item to key if key doesnt exist.
        /// </summary>
        /// <param name="key">Key identifier.</param>
        /// <param name="item">Setting item that you want to add</param>
        public bool Add(int key, SettingItem item)
        {
            item.Key = key;
            item.UpdatedSetting += new FmdcEventHandler(item_UpdatedSetting);
            if (!list.ContainsKey(key))
            {
                list.Add(key, item);
                return true;
            }
            return false;
        }

        // TODO : Remove this one.
        /*
        /// <summary>
        /// If key exist. obj will be the new value.
        /// Old value will be set as default value.
        /// </summary>
        /// <param name="key">Key you want to change value of</param>
        /// <param name="obj">New value for key</param>
        /// <returns>Returns true if key exist, else false</returns>
        public bool Set(int key, object obj)
        {
            if (list.ContainsKey(key))
            {
                SettingItem item = list[key];
                object oldObj = item.Value;
                item.Value = obj;
                item.DefaultValue = oldObj;
                return true;
            }
            return false;
        }
        */
        public SettingItem Get(int key)
        {
            if (list.ContainsKey(key))
                return list[key];
            return null;
        }

        public object GetObject(int key)
        {
            SettingItem item = Get(key);
            if (item != null)
                return item.Value;
            return null;
        }

        public string GetString(int key)
        {
            SettingItem item = Get(key);
            if (item != null)
                return item.ValueString;
            return null;
        }

        public int GetInt(int key)
        {
            SettingItem item = Get(key);
            if (item != null)
                return item.ValueInt;
            return -1;
        }

        public long GetLong(int key)
        {
            SettingItem item = Get(key);
            if (item != null)
                return item.ValueLong;
            return -1;
        }

        public bool GetBool(int key)
        {
            SettingItem item = Get(key);
            if (item != null)
                return item.ValueBool;
            return false;
        }

        public void Remove(int key)
        {
            if (list.Count > key && key >= 0)
                list.Remove(key);
        }

        public virtual void Load()
        {

        }

        public SettingsGroup()
        {
            UpdatedSetting = new FmdcEventHandler(SettingsGroup_UpdatedSetting);
        }

        void SettingsGroup_UpdatedSetting(object sender, FmdcEventArgs e) { }

        protected void item_UpdatedSetting(object sender, FmdcEventArgs e)
        {
            UpdatedSetting(this, e);
        }
        /// <summary>
        /// Saves group to xml.
        /// Xml Root name is "Settings".
        /// </summary>
        /// <param name="path">Exact/Relative path to xml file you want to save settings to.</param>
        /// <param name="group">SettingsGroup you want to save to xml file.</param>
        public static void Save(string path, SettingsGroup group)
        {
            Save(path, group, "Settings");
        }
        /// <summary>
        /// Saves group to xml.
        /// </summary>
        /// <param name="path">Exact/Relative path to xml file you want to save settings to.</param>
        /// <param name="group">SettingsGroup you want to save to xml file.</param>
        /// <param name="root">Xml Root name</param>
        public static void Save(string path, SettingsGroup group, string root)
        {
            XmlSerializer s = new XmlSerializer(group.GetType(), new XmlRootAttribute(root));
            TextWriter w = new StreamWriter(path);
            s.Serialize(w, group);
            w.Close();
        }
        /// <summary>
        /// Loads group from xml.
        /// Xml Root name is "Settings".
        /// </summary>
        /// <param name="path">Exact/Relative path to xml file you want to save settings to.</param>
        /// <param name="group">SettingsGroup you want to load from group.</param>
        public static void Load(string path, out SettingsGroup group)
        {
            Load(path, out group, "Settings");
        }
        /// <summary>
        /// Loads group from xml.
        /// </summary>
        /// <param name="path">Exact/Relative path to xml file you want to save settings to.</param>
        /// <param name="group">SettingsGroup you want to load from group.</param>
        /// <param name="root">Xml Root name</param>
        public static void Load(string path, out SettingsGroup group, string root)
        {
            XmlSerializer s = new XmlSerializer(typeof(SettingsGroup), new XmlRootAttribute(root));
            if (File.Exists(path))
            {
                TextReader r = new StreamReader(path);
                group = (SettingsGroup)s.Deserialize(r);
                r.Close();
            }
            else
            {
                group = new SettingsGroup();
            }
        }
    }
}
