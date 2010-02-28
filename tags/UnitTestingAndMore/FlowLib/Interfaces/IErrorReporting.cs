
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

namespace FlowLib.Interfaces
{
    public interface IErrorReporting
    {
        /// <summary>
        /// Gets true if [OK] is reseived from server.
        /// </summary>
        bool ResponseOk
        {
            get;
        }
        /// <summary>
        /// Gets response from server.
        /// </summary>
        string Response
        {
            get;
        }
        /// <summary>
        /// Has Report been sent?
        /// </summary>
        bool ReportSent
        {
            get;
        }
        /// <summary>
        /// Gets url used when report sent.
        /// </summary>
        string Url
        {
            get;
        }
        /// <summary>
        /// Gets/sets if clients unice id should be sent.
        /// </summary>
        bool SendUniceId
        {
            get;
            set;
        }
        /// <summary>
        /// Gets full report
        /// </summary>
        string Report
        {
            get;
        }
        /// <summary>
        /// Gets error report.
        /// </summary>
        string ErrorReport
        {
            get;
        }
        /// <summary>
        /// Gets environment report.
        /// </summary>
        string EnvironmentReport
        {
            get;
        }
        /// <summary>
        /// Gets gui.
        /// </summary>
        IBaseUpdater Gui
        {
            get;
        }
        /// <summary>
        /// Generating report.
        /// </summary>
        void GenerateReport();
        /// <summary>
        /// Try to send report to server.
        /// </summary>
        void SendReport();
        /// <summary>
        /// Parsing response from server.
        /// </summary>
        void ParseResponse();
    }
}
