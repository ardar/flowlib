
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

using System.Collections.Generic;

namespace FlowLib.Containers
{
    public class PropertyContainer<TKey, TObj> : SortedList<TKey, TObj>
    {
        public void Set(TKey key, TObj obj)
        {
            Remove(key);
            Add(key, obj);
        }

        public TObj Get(TKey key)
        {
            TObj obj = default(TObj);
            TryGetValue(key, out obj);
            return obj;
        }
    }

    


    public class UserInfoSpecial : SortedList<string, object>
    {
        public static string CID = "cid";
        public static string SID = "sid";
        public static string IP = "ip";
    }
}
