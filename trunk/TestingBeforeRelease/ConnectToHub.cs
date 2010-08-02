using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FlowLib.Containers;
using FlowLib.Connections;
using FlowLib.Interfaces;
using System.Threading;
using FlowLib.Events;
using FlowLib.Containers.Security;
using TestingBeforeRelease.Utils;

namespace TestingBeforeRelease
{
    [TestClass]
    public class ConnectToHub
    {
        private bool _isFinished;
        private bool _isConnected;
        private int _regMode = -1;
        private HubSetting _settings;

        [TestMethod]
        public void ConnectToHub_YnHub_UsingAutoProtocol()
        {
            HubTest("Auto");

            if (!_isConnected)
                throw new AssertFailedException("Unable to connect to YnHub using Auto Protocol");
            if (!_isFinished)
                throw new AssertFailedException("Connection established but no valid handshake to YnHub using Auto Protocol");
        }

        [TestMethod]
        public void ConnectToHub_YnHub_UsingNmdcProtocol()
        {
            HubTest("Nmdc");

            if (!_isConnected)
                throw new AssertFailedException("Unable to connect to YnHub using Nmdc Protocol");
            if (!_isFinished)
                throw new AssertFailedException("Connection established but no valid handshake to YnHub using Nmdc Protocol");
        }

        [TestMethod]
        public void ConnectToHub_Adc_UsingAutoProtocol()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2780;
            HubTest("Auto");

            if (!_isConnected)
                throw new AssertFailedException("Unable to connect to Adchpp using Auto Protocol");
            if (!_isFinished)
                throw new AssertFailedException("Connection established but no valid handshake to Adchpp using Auto Protocol");
        }

        [TestMethod]
        public void ConnectToHub_Adc_UsingAdcProtocol()
        {
            _settings.Address = "127.0.0.1";
            _settings.Port = 2780;
            HubTest("Adc");

            if (!_isConnected)
                throw new AssertFailedException("Unable to connect to Adchpp using Adc Protocol");
            if (!_isFinished)
                throw new AssertFailedException("Connection established but no valid handshake to Adchpp using Adc Protocol");
        }

        [TestMethod]
        public void ConnectToHub_Adcs_UsingAdcsProtocol()
        {
			//_settings.Address = "127.0.0.1";
			//_settings.Port = 2781;
			_settings.Address = "devpublic.adcportal.com";
			_settings.Port = 16591;
			HubTest("AdcSecure");

            if (!_isConnected)
                throw new AssertFailedException("Unable to connect to Adchpp using Adcs Protocol");
            if (!_isFinished)
                throw new AssertFailedException("Connection established but no valid handshake to Adchpp using Adcs Protocol");
        }
        [TestInitialize()]
        public void Init()
        {
            Application.InitilizeAll();

            _settings = new HubSetting();
            _settings.Address = "127.0.0.1";
            _settings.Port = 411;
            _settings.DisplayName = "FlowLib";
            _settings.Password = "Password";
        }

        [TestCleanup()]
        public void CleanUp()
        {
            _isFinished = false;
            _regMode = -1;
            _isConnected = false;
        }

        private void HubTest(string protocol)
        {
            _settings.Protocol = protocol;

            Hub hubConnection = new Hub(_settings);
            hubConnection.ConnectionStatusChange += new FlowLib.Events.FmdcEventHandler(OnConnectionStatusChange);
            hubConnection.SecureUpdate += new FmdcEventHandler(hubConnection_SecureUpdate);
            Hub.RegModeUpdated += new FmdcEventHandler(Hub_RegModeUpdated);

            hubConnection.Connect();

            int i = 0;
            while (!_isFinished && i++ < 20)
            {
                Thread.Sleep(100);
            }

            hubConnection.ConnectionStatusChange -= OnConnectionStatusChange;
            hubConnection.Disconnect("Test time exceeded");
            hubConnection.Dispose();
        }

        void hubConnection_SecureUpdate(object sender, FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.SecurityValidateRemoteCertificate:
                    CertificateValidationInfo ct = e.Data as CertificateValidationInfo;
                    if (ct != null)
                    {
                        ct.Accepted = true;
                        e.Data = ct;
                        e.Handled = true;
                    }
                    break;
            }
        }

        void Hub_RegModeUpdated(object sender, FmdcEventArgs e)
        {
            Hub hub = sender as Hub;
            if (_settings == hub.HubSetting)
            {
                _regMode = (int)e.Action;
                if (_regMode >= 0)
                {
                    _isFinished = true;
                }
            }
        }
        
        void OnConnectionStatusChange(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case TcpConnection.Connected:
                    _isConnected = true;
                    break;
            }
        }
    }
}
