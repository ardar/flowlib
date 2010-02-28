
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

namespace FlowLib.Utils.ErrorHandling
{
    public static class HttpErrorReporting
    {
        static string webpage = null;
        static string userId = null;
        static System.Exception lastException = null;
        static string appId = null;

        public static bool Report(System.Exception ex)
        {
            lastException = ex;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            GenerateReport(ex, sb);
            return ParseResponse(SendReport(sb));
        }

        /// <summary>
        /// Parsing response from server.
        /// </summary>
        static bool ParseResponse(string response)
        {
            // Parse response from server.
            return (response.EndsWith("[OK]"));
        }

        public static string SendReport(System.Text.StringBuilder sb)
        {
            if (webpage == null)
                throw new System.NullReferenceException("Webpage can't be null");
            if (appId == null)
                throw new System.NullReferenceException("Application Id can't be null");

            string url = webpage;
            if (!string.IsNullOrEmpty(userId))
                url += GetUrlSeperator(url) + "userid=" + userId;

            System.Net.WebRequest wReq = System.Net.WebRequest.Create(url);
            if (wReq is System.Net.HttpWebRequest)
                ((System.Net.HttpWebRequest)wReq).UserAgent = appId;
            System.Net.WebResponse wResp = wReq.GetResponse();
            if (wResp is System.Net.HttpWebResponse)
            {
                System.DateTime tmp = ((System.Net.HttpWebResponse)wResp).LastModified;	// Last Modified
            }
            // Get the response stream.
            System.IO.Stream respStream = wResp.GetResponseStream();
            System.IO.StreamReader reader = new System.IO.StreamReader(respStream, System.Text.Encoding.ASCII);
            string respHTML = reader.ReadToEnd();
            // Close the response and response stream.
            wResp.Close();
            return respHTML;
        }

        /// <summary>
        /// Get Url seperator to use next in url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        static string GetUrlSeperator(string url)
        {
            if (url.Contains("?"))
                return "&";
            return "?";
        }


        /// <summary>
        /// Generating Report
        /// </summary>
        /// <param name="ex">Exception that was throwed</param>
        static void GenerateReport(System.Exception ex, System.Text.StringBuilder sb)
        {
            // Exception
            sb.Append("error=");
            GenerateExceptionReport(ex, sb);
            // Environment
            sb.Append("os=");
            GenerateOSReport(sb);
            sb.Append("external=");
            // TODO : Add External Information 
        }

        /// <summary>
        /// Generating Operating System Information
        /// </summary>
        /// <param name="sb">System.Text.StringBuilder object to append info to.</param>
        static void GenerateOSReport(System.Text.StringBuilder sb)
        {
            sb.Append("OSF:" + System.Environment.OSVersion.ToString() + "\r\n");              // Full OS String
            sb.Append("OSP:" + (int)System.Environment.OSVersion.Platform + "\r\n");                // OS Platform
            sb.Append("OSV:" + System.Environment.OSVersion.Version + "\r\n");                 // OS Version
            // Walk around for Windows Mobile.
#if !COMPACT_FRAMEWORK
            sb.Append("OSS:" + System.Environment.OSVersion.ServicePack + "\r\n");             // Os ServicePack.
#endif
        }

        /// <summary>
        /// Generating Exception Information to sb
        /// </summary>
        /// <param name="e">Exception that we want to generate information on</param>
        /// <param name="sb">System.Text.StringBuilder to generate information to</param>
        public static void GenerateExceptionReport(System.Exception e, System.Text.StringBuilder sb) 
        {
            if (e != null)
            {
                if (e.GetType().FullName != null)
                {
                    sb.Append("Exception type: " + e.GetType().FullName + "\r\n");
                    // This is if we know we can collect special info for a specific Exception
                    // Walk around for Windows Mobile.
#if !COMPACT_FRAMEWORK
                    switch (e.GetType().ToString())
                    {
                        case "System.Reflection.ReflectionTypeLoadException":
                            System.Reflection.ReflectionTypeLoadException rtle = (System.Reflection.ReflectionTypeLoadException)e;
                            for (int i = 0; i < rtle.LoaderExceptions.Length; i++)
                                sb.Append("\t" + rtle.LoaderExceptions[i].Message + "\r\n");
                            break;
                    }
#endif
                }
                if (e.Message != null)
                    sb.Append("Exception text: " + e.Message + "\r\n");
                // Walk around for Windows Mobile.
#if !COMPACT_FRAMEWORK
                if (e.TargetSite != null)
                    sb.Append("Function Name: " + e.TargetSite.ToString() + "\r\n");
#endif
                if (e.StackTrace != null)
                    sb.Append("StackTrace: " + e.StackTrace.ToString() + "\r\n");
                // Walk around for Windows Mobile.
#if !COMPACT_FRAMEWORK
                if (e.Source != null)
                    sb.Append("Source: " + e.Source + "\r\n");
#endif
                if (e.InnerException != null)
                {
                    sb.Append("InnerException: \r\n");
                    GenerateExceptionReport(e.InnerException, sb);
                    sb.Append("\r\n");
                }
            }
        }
        static HttpErrorReporting()
        {
            
        }
    }
}
