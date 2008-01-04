
/*
 *
 * Copyright (C) 2008 Mattias Blomqvist, patr-blo at dsv dot su dot se
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

using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Interfaces;
using FlowLib.Events;
using Nmdc = FlowLib.Protocols.HubNmdc;
using System.Net;

namespace ConsoleDemo.Examples
{
    public class PassiveSearch : IBaseUpdater
    {
        #region IBaseUpdater Members
        public event FlowLib.Events.FmdcEventHandler UpdateBase;
        #endregion

        public PassiveSearch()
        {
            UpdateBase = new FlowLib.Events.FmdcEventHandler(PassiveSearch_UpdateBase);

            HubSetting settings = new HubSetting();
            settings.Address = "127.0.0.1";
            settings.Port = 411;
            settings.DisplayName = "FlowLibNick";
            settings.Protocol = "Nmdc";

            Hub hubConnection = new Hub(settings,this);
            hubConnection.Update += new FlowLib.Events.FmdcEventHandler(hubConnection_Update);
            hubConnection.Connect();

            // Wait 5 seconds
            System.Threading.Thread.Sleep(5 * 1000);

            // Send Search
            SearchInfo searchInfo = new SearchInfo();
            searchInfo.Set(SearchInfo.SEARCH, "Ecma-334");

            UpdateBase(this, new FlowLib.Events.FmdcEventArgs(Actions.Search, searchInfo));
        }

        void PassiveSearch_UpdateBase(object sender, FlowLib.Events.FmdcEventArgs e) { }

        void hubConnection_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.SearchResult:
                    if (e.Data is SearchResultInfo)
                    {
                        SearchResultInfo srInfo = (SearchResultInfo)e.Data;
                    }
                    break;
            }

        }
    }
}
