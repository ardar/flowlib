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

using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;

namespace FlowLib.Utils
{
    public static class WebOperations
    {
        private static string ReplaceInput(string input)
        {
            input = input.Replace("\r", "");
            input = input.Replace("\n", "");
            input = input.Replace("'", "\"");
            return input;
        }
        private static string FindSeperator(string input, string findStr)
        {
            int tmp1;
            string endString = null;
            if ((tmp1 = input.IndexOf(findStr)) != -1)
            {
                tmp1 += findStr.Length;
                switch (input[tmp1])
                {
                    case '"':
                        endString = "\" ";
                        break;
                    case '\'':
                        endString = "' ";
                        break;
                    default:
                        endString = " ";
                        break;
                }
            }
            return endString;
        }
        public static void FindInputValues(string tmpString, SortedList<string, string> inputs)
        {
            int pos1, tmp1 = 0, tmp2 = 0;

            #region Add Input values (this is so we get random data)
            while (StringOperations.Find(tmpString, "<input", ">", ref tmp1, ref tmp2))
            {
                pos1 = tmp2;
                string name = string.Empty;
                string value = string.Empty;
                #region Find name and value
                string input = tmpString.Substring(tmp1, tmp2 - tmp1);
                input = ReplaceInput(input);
                #region Name
                tmp1 = 0;
                tmp2 = 0;

                string endString = FindSeperator(input, "name=");
                if (endString != null && StringOperations.Find(input, "name=", endString, ref tmp1, ref tmp2))
                {
                    tmp1 += 5 + (1 % endString.Length);
                    tmp2 -= endString.Length;
                    name = input.Substring(tmp1, tmp2 - tmp1);
                }
                #endregion
                #region Value
                tmp1 = 0;
                tmp2 = 0;
                endString = FindSeperator(input, "value=");
                if (endString != null && StringOperations.Find(input, "value=", endString, ref tmp1, ref tmp2))
                {
                    tmp1 += 6 + (1 % endString.Length);
                    tmp2 -= endString.Length;
                    value = input.Substring(tmp1, tmp2 - tmp1);
                }
                #endregion
                #endregion
                if (!string.IsNullOrEmpty(name) || !string.IsNullOrEmpty(value))
                {
                    inputs.Add(name, value);
                }
                tmpString = tmpString.Remove(0, pos1);
                tmp1 = 0;
                tmp2 = 0;
            }
            #endregion
        }
        public static string GetPage(string url)
        {
            return GetPage(url, null, null, null);
        }

        public static string GetPage(string url, ref WebHeaderCollection responseHeaders)
        {
            return GetPage(url, null, null, null, ref responseHeaders);
        }

        public static string GetPage(string url, string method, string contentType, SortedList<string, string> values)
        {
            WebHeaderCollection responseHeaders = null;
            return GetPage(url, method, contentType, values, ref responseHeaders);
        }


        public static string GetPage(string url, string method, string contentType, SortedList<string, string> values, ref WebHeaderCollection responseHeaders)
        {
            string tmp = string.Empty;
            try
            {
                HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create(url);
                wReq.UserAgent = "FlowLib";

#if !COMPACT_FRAMEWORK
                wReq.CookieContainer = new CookieContainer();
                // Enable cookie support
                if (responseHeaders != null && responseHeaders.HasKeys())
                {
                    string str = responseHeaders.Get("Set-Cookie");
                    if (!string.IsNullOrEmpty(str))
                    {
                        string[] tmp2= str.Split('=', ' ', ';', ',', '\t', '\n', '\r');

                        if (tmp2.Length >= 2){
                            Cookie cookie = new Cookie(tmp2[0], tmp2[1]);
                            wReq.CookieContainer.Add(wReq.RequestUri, cookie);
                        }
                        //wReq.CookieContainer.Add(new Cookie(
                        //wReq.Headers.Add("Cookie", str);

                        //wReq.Headers.Set("Cookie", str);
                    }
                }
#endif

                if (!string.IsNullOrEmpty(method) && !string.IsNullOrEmpty(contentType) && values != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (KeyValuePair<string, string> var in values)
                    {
                        sb.Append("&");
                        sb.Append(var.Key);
                        sb.Append("=");
                        sb.Append(var.Value);
                    }
                    byte[] data = Encoding.UTF8.GetBytes(sb.ToString());

                    // Set values for the request back
                    wReq.Method = method;
                    wReq.ContentType = contentType;
                    wReq.ContentLength = data.Length;

                    // Write to request stream.
                    Stream streamOut = wReq.GetRequestStream();
                    streamOut.Write(data, 0, data.Length);
                    streamOut.Flush();
                    streamOut.Close();
                }

                // Get the response stream.
                WebResponse wResp = wReq.GetResponse();
                Stream respStream = wResp.GetResponseStream();
                StreamReader reader = new StreamReader(respStream, Encoding.UTF8);
                tmp = reader.ReadToEnd();

                responseHeaders = wResp.Headers;

                // Close the response and response stream.
                wResp.Close();
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
            return tmp;
        }

    }
}
