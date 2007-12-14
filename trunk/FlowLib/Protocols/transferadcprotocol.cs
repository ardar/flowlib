
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

using FlowLib.Events;
using FlowLib.Interfaces;
using FlowLib.Containers;

namespace FlowLib.Protocols
{
    /// <summary>
    /// Transfer Adc Protocol
    /// </summary>
    public class TransferAdcProtocol : IProtocol
    {
        #region Variables
        protected IConnection con = null;
        protected string received = "";
        protected bool rawData = false;

        public event FmdcEventHandler MessageReceived;
        public event FmdcEventHandler MessageToSend;
        #endregion

        #region Properties
        public string Name
        {
            get { return "Adc"; }
        }
        public IConMessage KeepAliveCommand
        {
            get { return null; }
        }
        public IConMessage FirstCommand
        {
            get { return null; }    // TODO : Add CSUP
        }
        public System.Text.Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
        public string Seperator
        {
            get { return "\n"; }
        }
        #endregion
        #region Constructor(s)
        public TransferAdcProtocol(IConnection con)
        {
            this.con = con;
            MessageReceived = new FmdcEventHandler(OnMessageReceived);
            MessageToSend = new FmdcEventHandler(OnMessageToSend);
        }
        #endregion
        #region Parse
        public void ParseRaw(byte[] b, int length)
        {
            if (rawData)
            {
                // TODO : Fixa mottagning av data.
            }
            else
            {
                ParseRaw(this.Encoding.GetString(b, 0, length));
            }
        }
        public void ParseRaw(string raw)
        {
            // If raw lenght is 0. Ignore
            if (raw.Length == 0)
                return;

            // Should we read buffer?
            if (received.Length > 0)
            {
                raw = received + raw;
                received = string.Empty;
            }
            int pos;
            // Loop through Commands.
            while ((pos = raw.IndexOf(Seperator)) > 0)
            {
                pos++;
                TransferMessage msg = ParseMessage(raw.Substring(0, pos));
                raw = raw.Remove(0, pos);
                // Plugin handling here
                FmdcEventArgs e = new FmdcEventArgs(Actions.CommandIncomming, msg);
                MessageReceived(con, e);
                if (!e.Handled)
                    ActOnInMessage(msg);
                pos++;
            }
            // Right now we are not supporting changing of protocol directly. Else it should have been here.
            // If wrong Protocol type has been set. change it to ADC
            /*
            if (hub.RegMode == -1 && raw.StartsWith("ISUP"))
            {   // Setting hubtype to ADC
                hub.HubSetting.Protocol = 1;
                hub.Protocol = new HubAdcProtocol(hub);
                hub.Reconnect();
            }
            */
            // If Something is still left. Save it to buffer for later use.
            if (raw.Length > 0)
                received = raw;

        }
        protected TransferMessage ParseMessage(string raw)
        {
            raw = raw.Replace(this.Seperator, "");
            TransferMessage msg = new TransferMessage(con, raw);
            switch (raw[0])
            {
                case '$':
                    int pos;
                    string cmd = null;
                    if ((pos = raw.IndexOf(' ')) != -1)
                        cmd = raw.Substring(0, pos).ToLower();
                    else
                    {
                        if (raw.Length >= 10)
                            break;
                        cmd = raw.ToLower();
                    }
                    if (cmd == null || cmd.Equals(string.Empty))
                        break;
                    switch (cmd)
                    {
                        // TODO : add commands here.
                        case "$lock": break;
                    }
                    break;
                default:
                    break;
                // No command. Ignore.
            }
            return msg;
        }
        #endregion
        public bool OnSend(IConMessage msg)
        {
            FmdcEventArgs e = new FmdcEventArgs(Actions.CommandOutgoing, msg);
            MessageToSend(con, e);
            if (!e.Handled)
            {
                return true;
            }
            return false;
        }

        public void ActOnInMessage(IConMessage comMsg)
        {
            TransferMessage message = (TransferMessage)comMsg;
            // TODO : Add message handling here.
        }
        public void ActOnOutMessage(FmdcEventArgs e)
        {

        }
        #region Event(s)
        protected void OnMessageReceived(object sender, FmdcEventArgs e)
        {

        }
        protected void OnMessageToSend(object sender, FmdcEventArgs e)
        {

        }
        #endregion
    }
}
