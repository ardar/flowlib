
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
using System.Text;

namespace FlowLib.Utils.Convert
{
    public static class General
    {
        /// <summary>
        /// Byte IEEE 1541 standard.
        /// http://en.wikipedia.org/wiki/IEEE_1541
        /// </summary>
        [Flags]
        public enum BinaryPrefixes
        {
            B = 0,
            KiB,
            MiB,
            GiB,
            TiB,
            PiB,
            EiB,
            ZiB,
            YiB
        }
        /// <summary>
        /// Formats a long to the highest binary prefix accourding to IEEE 1541 standard.
        /// http://en.wikipedia.org/wiki/IEEE_1541
        /// </summary>
        /// <example>The above code will give you:227,54 MiB
        /// <code>
        /// Util.Convert.BinaryPrefixes bp;
        /// string f = string.Format("{0} {1}", Util.Convert.FormatBytes(238594534, out bp), bp);
        /// </code>
        /// </example>
        /// <param name="bytes">long representation as bytes</param>
        /// <param name="bp">BinaryPrefix object that will show what prefix type returned data is of</param>
        /// <returns>formated bytes after conversion with 2 decimals. Look at bp for prefix type</returns>
        public static double FormatBytes(long bytes, out BinaryPrefixes bp)
        {
            return FormatBytes(bytes, 2, out bp);
        }

        /// <summary>
        /// Formats a long to the highest binary prefix accourding to IEEE 1541 standard.
        /// http://en.wikipedia.org/wiki/IEEE_1541
        /// </summary>
        /// <example>The above code will give you:227,54 MiB
        /// <code>
        /// Util.Convert.BinaryPrefixes bp;
        /// string f = string.Format("{0} {1}", Util.Convert.FormatBytes(238594534, out bp), bp);
        /// </code>
        /// </example>
        /// <param name="bytes">double representation as bytes</param>
        /// <param name="bp">BinaryPrefix object that will show what prefix type returned data is of</param>
        /// <returns>formated bytes after conversion with 2 decimals. Look at bp for prefix type</returns>
        public static double FormatBytes(double bytes, out BinaryPrefixes bp)
        {
            return FormatBytes(bytes, 2, out bp);
        }

        /// <summary>
        /// Formats a long to the highest binary prefix accourding to IEEE 1541 standard.
        /// http://en.wikipedia.org/wiki/IEEE_1541
        /// </summary>
        /// <example>The above code will give you:227,54 MiB
        /// <code>
        /// Util.Convert.BinaryPrefixes bp;
        /// string f = string.Format("{0} {1}", Util.Convert.FormatBytes(238594534, out bp), bp);
        /// </code>
        /// </example>
        /// <param name="bytes">long representation as bytes</param>
        /// <param name="decimals">Number of decimals the return data should have</param>
        /// <param name="bp">BinaryPrefix object that will show what prefix type returned data is of</param>
        /// <returns>formated bytes after conversion. Look at bp for prefix type</returns>
        public static double FormatBytes(long bytes, int decimals, out BinaryPrefixes bp)
        {
            return FormatBytes((double)bytes, decimals, out bp);
        }

        /// <summary>
        /// Formats a long to the highest binary prefix accourding to IEEE 1541 standard.
        /// http://en.wikipedia.org/wiki/IEEE_1541
        /// </summary>
        /// <example>The above code will give you:227,54 MiB
        /// <code>
        /// Util.Convert.BinaryPrefixes bp;
        /// string f = string.Format("{0} {1}", Util.Convert.FormatBytes(238594534, out bp), bp);
        /// </code>
        /// </example>
        /// <param name="bytes">double representation as bytes</param>
        /// <param name="decimals">Number of decimals the return data should have</param>
        /// <param name="bp">BinaryPrefix object that will show what prefix type returned data is of</param>
        /// <returns>formated bytes after conversion. Look at bp for prefix type</returns>
        public static double FormatBytes(double bytes, int decimals, out BinaryPrefixes bp)
        {
            bp = 0;
            while ((bytes / 1024d) > 1)
            {
                bytes /= 1024d;
                bp++;
            }
            Decimal des = new Decimal(bytes);
            bytes = Decimal.ToDouble(Decimal.Round(des, 2));
            return bytes;
        }
        /// <summary>
        /// Decodes a base64 string
        /// </summary>
        /// <param name="data">string that will be converted</param>
        /// <returns>true if the conversion succeeded or false if it didnt</returns>
        public static bool Base64StringDecode(string data)
        {
            try
            {
                System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();

                byte[] todecode_byte = System.Convert.FromBase64String(data);
                int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
                char[] decoded_char = new char[charCount];
                utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
                data = new String(decoded_char);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Encodes a base64 string
        /// </summary>
        /// <param name="data">string that will be converted</param>
        /// <returns>true if the conversion succeeded or false if it didnt</returns>
        public static bool Base64StringEncode(string data)
        {
            try
            {
                byte[] encData_byte = new byte[data.Length];
                encData_byte = System.Text.Encoding.UTF8.GetBytes(data);
                data = System.Convert.ToBase64String(encData_byte);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Converts a byte[] to a string where the string has the content of every byte[x].
        /// </summary>
        /// <param name="byteStr">byte[] that will be converted</param>
        /// <returns>String representing the content of the byte[]</returns>
        public static string FromByteArrayToString(byte[] byteStr)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < byteStr.Length; i++)
            {
#if !COMPACT_FRAMEWORK
                sb.Append(Char.ConvertFromUtf32(byteStr[i]));
#else
                sb.Append((Char)byteStr[i]);
#endif
            }
            return sb.ToString();
        }

        public static string BitArrayToString(System.Collections.BitArray b)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < b.Length; i++)
            {
                sb.Append(b.Get(i) ? "1" : "0");
            }
            return sb.ToString();
        }

        public static bool[] BitArrayToBoolArray(System.Collections.BitArray b)
        {
            // TODO: When we switch to .net 3,5..
            if (b == null)
                return new bool[] { };

            bool[] tmp = new bool[b.Count - 1];
            for (int i = 0; i < b.Length; i++)
            {
                tmp[i] = b.Get(i);
            }
            return tmp;
        }

        public static System.Collections.BitArray StringToBitArray(string str)
        {
            System.Collections.BitArray bit = new System.Collections.BitArray(str.Length, false);
            for (int i = 0; i < str.Length; i++)
			{
                switch (str[i])
                {
                    case '1':
                        bit.Set(i, true);
                        break;
                }
			}
            return bit;
        }
    }
}
