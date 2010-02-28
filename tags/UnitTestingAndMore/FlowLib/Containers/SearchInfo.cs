
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

using FlowLib.Enums;

namespace FlowLib.Containers
{
    public class SearchInfo : PropertyContainer<string, string>
    {
        public const string SEARCH = "schstr";
        public const string NOSEARCH = "noschstr";
        /// <summary>
        /// 0 = File
        /// 1 = Directory
        /// 2 = Tth
        /// </summary>
        public const string TYPE = "schtype";
        public const string SIZE = "size";
        /// <summary>
        /// Size type.
        /// 0 = No Size restriction
        /// 1 = Min Size
        /// 2 = Max Size
        /// 3 = Equal Size
        /// </summary>
        public const string SIZETYPE = "sizetype";
        public const string EXTENTION = "ext";
        public const string TOKEN = "token";
    }
}
