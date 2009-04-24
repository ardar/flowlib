
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
    // We need to add all standard types that we want to use here.
    [XmlInclude(typeof(HubSetting)), XmlInclude(typeof(ContentInfo)), XmlInclude(typeof(VirtualDir))]
    public class SettingItem
    {
        protected int key;
        protected object value = null;
        protected object defaultvalue = null;
        protected bool equal = false;
        /// <summary>
        /// Is triggered when value is changed. Data contains the old value.
        /// </summary>
        public event FmdcEventHandler UpdatedSetting;

        /// <summary>
        /// Gets bool value indicating that value == defaultvalue or not.
        /// </summary>
        [XmlIgnore()]
        public bool IsDefault
        {
            get { return equal; }
            set { }
        }

        /// <summary>
        /// Gets/sets default value.
        /// </summary>
        [XmlIgnore()]
        public object DefaultValue
        {
            get { return defaultvalue; }
            set { 
                defaultvalue = value;
                Check();
            }
        }

        #region For Xml handling
        /// <summary>
        /// If Value != defaultvalue && value is of type Bool.
        /// This will return true. Else false.
        /// </summary>
        [XmlIgnore()]
        public bool ValueBoolSpecified
        {
            get { return (!equal && (value != null && value.GetType().Equals(typeof(bool)))); }
            set { }
        }
        /// <summary>
        /// This is for XML handling.
        /// If Value is of type Bool.
        /// It can be returned from here. 
        /// Else it will return false.
        /// </summary>
        [XmlAttribute("ValueBool")]
        public bool ValueBool
        {
            get
            {
                if (value != null && value.GetType().Equals(typeof(bool)))
                    return (bool)value;
                return false;
            }
            set { this.value = value; }
        }
        /// <summary>
        /// If Value != defaultvalue && value is of type Long.
        /// This will return true. Else false.
        /// </summary>
        [XmlIgnore()]
        public bool ValueLongSpecified
        {
            get { return (!equal && (value != null && value.GetType().Equals(typeof(long)))); }
            set { }
        }
        /// <summary>
        /// This is for XML handling.
        /// If Value is of type Long.
        /// It can be returned from here. 
        /// Else it will return -1.
        /// </summary>
        [XmlAttribute("ValueLong")]
        public long ValueLong
        {
            get
            {
                if (value != null)
                {
                    if (value != null && value.GetType().Equals(typeof(long)))
                        return (long)value;
                    else if (value.GetType().Equals(typeof(int)))
                        return (long)(int)value;
                }
                return -1;
            }
            set { this.value = value; }
        }
        /// <summary>
        /// If Value != defaultvalue && value is of type Int.
        /// This will return true. Else false.
        /// </summary>
        [XmlIgnore()]
        public bool ValueIntSpecified
        {
            get { return (!equal && (value != null && value.GetType().Equals(typeof(int)))); }
            set { }
        }
        /// <summary>
        /// This is for XML handling.
        /// If Value is of type Int.
        /// It can be returned from here. 
        /// Else it will return -1.
        /// </summary>
        [XmlAttribute("ValueInt")]
        public int ValueInt
        {
            get {
                if (value != null)
                {
                    if (value.GetType().Equals(typeof(int)))
                        return (int)value;
                    else if (value.GetType().Equals(typeof(long)) && int.MaxValue > (long)value)
                        return (int)(long)value;
                }
                return -1;
            }
            set { this.value = value; }
        }
        /// <summary>
        /// This is for XML handling.
        /// If Value is of type String
        /// It can be returned from here.
        /// Else it will return null.
        /// </summary>
        public string ValueString
        {
            get
            {
                if (value != null && value.GetType().Equals(typeof(string)))
                    return (string)value;
                else if (value != null)
                    return value.ToString();
                else return null;
            }
            set { this.value = value; }
        }
        /// <summary>
        /// If Value != defaultvalue && value is of type string.
        /// This will return true. Else false.
        /// </summary>
        [XmlIgnore()]
        public bool ValueStringSpecified
        {
            get { return (!equal && (value != null && value.GetType().Equals(typeof(string)))); }
            set { }
        }
        #endregion
        /// <summary>
        /// If Value != defaultvalue && value is of type string.
        /// This will return true. Else false.
        /// </summary>
        [XmlIgnore()]
        public bool ValueSpecified
        {
            get
            {
                return (!equal && (value != null &&
                  !value.GetType().Equals(typeof(string)) &&
                  !value.GetType().Equals(typeof(int)) &&
                  !value.GetType().Equals(typeof(bool)) &&
                  !value.GetType().Equals(typeof(long))
              ));
            }
            set { }
        }
        /// <summary>
        /// Gets/sets Object Value
        /// </summary>
        public object Value
        {
            get { return this.value; }
            set {
                this.value = value;
                Check();
                UpdatedSetting(this, new FmdcEventArgs(key, value));
            }
        }
        /// <summary>
        /// Gets/sets key for this value
        /// </summary>
        [XmlAttribute("Key")]
        public int Key
        {
            get { return key; }
            set { key = value; }
        }
        /// <summary>
        /// This should not be used. It is needed for xml handling.
        /// </summary>
        public SettingItem() {
            UpdatedSetting = new FmdcEventHandler(OnUpdateSetting);
        }

        protected void Check()
        {
            #region Change indication for if value is equal to default value.
            if (DefaultValue == null)
            {
                if (Value == null)
                    equal = true;
            }
            else
            {
                equal = (DefaultValue.Equals(Value));
            }
            #endregion
        }

        /// <summary>
        /// Creates an instance of SettingItem and sets Value and DefaultValue to value
        /// </summary>
        /// <param name="value">object that should be saved</param>
        public SettingItem(object value)
            : this(value, value)
        {
        }
        /// <summary>
        /// Creates an instance of SettingItem and sets Value and DefaultValue
        /// </summary>
        /// <param name="value">object that should be saved</param>
        /// <param name="defaultvalue">default value of this object. If default value == value this SettingItem will not be saved.</param>
        public SettingItem(object value, object defaultvalue)
            : this()
        {
            this.value = value;
            DefaultValue = defaultvalue;
        }


        protected void OnUpdateSetting(object sender, FmdcEventArgs e)
        {

        }
    }
}
