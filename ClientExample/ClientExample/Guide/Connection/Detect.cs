using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

using FlowLib.Connections;
using FlowLib.Events;
using FlowLib.Utils;
using FlowLib.Managers;
using FlowLib.Containers;
using FlowLib.Interfaces;

namespace ClientExample.Guide.Connection
{
    public partial class Detect : Form
    {
        static TransferManager transferManager = new TransferManager();

        Thread workerThread = new Thread(new ParameterizedThreadStart(OnWorking));
        TcpConnectionListener tcpListener = new TcpConnectionListener(6000);

        Queue<string> messageQueue = new Queue<string>();
        System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
        bool isUpdating = false;
        bool isDone = false;
        int intDots = 0;

        int hasInternet = 0;
        int hasInternalListenerAccess = 0;
        static int hasInternalReceiveAccess = 0;
        int hasRetreivedPublicIp = 0;
        static int hasPublicReceiveAccess = 0;
        string publicIP = null;

        public int Mode
        {
            get
            {
                // Passive Mode
                int value = 1;
                // No Internet/DNS lookup
                if (hasInternet != 2)
                    value = 0;
                // Active Mode
                if (hasInternalListenerAccess == 1 && hasInternalReceiveAccess == 1 && hasPublicReceiveAccess == 1 && hasRetreivedPublicIp == 2)
                    value = 2;

                return value;
            }
        }

        public Detect()
        {
            hasInternalReceiveAccess = 0;
            hasPublicReceiveAccess = 0;

            InitializeComponent();

            updateTimer.Tick += new EventHandler(updateTimer_Tick);
            updateTimer.Interval = 200;
            updateTimer.Start();

            workerThread.IsBackground = true;
            workerThread.Start(this);
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            if (isUpdating)
                return;
            isUpdating = true;

            if (messageQueue.Count > 0)
            {
                lock (messageQueue)
                {
                    textBox1.AppendText(messageQueue.Dequeue() + "\r\n");
                }
            }

            ChangeBtnStatus(btnInternet, hasInternet);
            ChangeBtnStatus(btnLoopback, hasInternalListenerAccess + hasInternalReceiveAccess);
            ChangeBtnStatus(btnPublicIP, hasRetreivedPublicIp);
            ChangeBtnStatus(btnNat, hasInternalListenerAccess + hasPublicReceiveAccess);

            if (!isDone)
            {
                string str = "Testing";
                for (int i = 0; i < intDots; i++)
                    str += ".";
                Text = str;
                lblStatus.Text = str;
                if (++intDots == 4)
                    intDots = 0;
            }
            else
            {
                string str = "Done, ";
                switch (Mode)
                {
                    case 0:
                        str += "couldn't find a working Internet connection.";
                        break;
                    case 1:
                        str += "limited connectivity (Passive mode)";
                        break;
                    case 2:
                        str += "full connectivity (Active mode)";
                        break;
                }
                Text = str;
                lblStatus.Text = str;
            }

            isUpdating = false;
        }

        private void Detect_Load(object sender, EventArgs e)
        {

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            tcpListener.End();
        }

        private void AddMessage(string msg)
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(msg);
            }
        }

        private void ChangeBtnStatus(Button btn, int mode)
        {
            btn.Enabled = mode > 0;
            switch (mode)
            {
                case 2:
                    btn.ForeColor = Color.Green;
                    break;
                case 1:
                    btn.ForeColor = Color.Orange;
                    break;
                default:
                    btn.ForeColor = Color.Red;
                    break;
            }
        }

        private static void OnWorking(object obj)
        {
            Detect det = obj as Detect;
            if (det == null)
                return;
            #region Do we have access to google.com?
            det.AddMessage("# Do you have Internet access?");
            det.AddMessage("\tRetreiving www.google.com");

            string content = null;
            if (( content = WebOperations.GetPage("http://www.google.com")) != string.Empty && content.IndexOf("google", StringComparison.OrdinalIgnoreCase) != -1)
            {
                det.AddMessage("\tSuccessfully retreived content from www.google.com");
                det.hasInternet = 2;
            }
            else if (content != string.Empty)
            {
                det.AddMessage("\tGot a diffrent page then google.");
                det.AddMessage("\tDo you need to login to access Internet?");
                det.hasInternet = 1;
            }
            else
            {
                det.AddMessage("\tUnable to retreive content from www.google.com.");
                det.hasInternet = 0;
            }
            #endregion
            #region Listening for connections on loopback
            det.AddMessage("# Are you allowed to open a loopback port on your computer?");
            det.tcpListener.Update += new FmdcEventHandler(tcpListener_Update);
            det.tcpListener.Start();
            try
            {
                Transfer transfer = new Transfer("127.0.0.1", 6000);
                FlowLib.Containers.UserInfo me = new FlowLib.Containers.UserInfo();
                    me.DisplayName = "loopback";
                    me.TagInfo.Version = "FlowLibPowered";
                transfer.Share = new FlowLib.Containers.Share("temp");
                transfer.Me = me;
                transfer.Source = new FlowLib.Containers.Source("127.0.0.1", "loopback");
                TransferRequest req = new TransferRequest("loopback", null, new UserInfo());
                transferManager.AddTransferReq(req);

                transfer.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(transfer);
                transferManager.StartTransfer(transfer);
                det.hasInternalListenerAccess = 1;
                det.AddMessage("\tYou seem to be allowed to open a loopback port.");
                det.AddMessage("#Can you receive data on loopback port?");
            }
            catch (System.Exception e)
            {
                det.AddMessage("\tYou are not allowed to open internal port. Reason:" + e.Message);
                det.hasInternalListenerAccess = 0;
            }
            // Wait 10 sec before continue.
            int i = 0;
            do
            {
                Thread.Sleep(500);
            }
            while (Detect.hasInternalReceiveAccess != 1 && i++ < 20);
            if (Detect.hasInternalReceiveAccess != 1)
            {
                det.AddMessage("\tYou are not allowed receive data on loopback port.");
            }
            else
            {
                det.AddMessage("\tYou received data on loopback port.");
            }
            #endregion
            #region Retreive public IP
            det.AddMessage("# Retreiving public IP");
            content = null;
            if ((content = WebOperations.GetPage("http://whatismyip.org")) != string.Empty)
            {

                if (Regex.IsMatch(content, @"[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"))
                {
                    det.publicIP = content;
                    det.hasRetreivedPublicIp = 2;
                    det.AddMessage("\tSuccessfully retreived public ip: " + content);
                }
                else
                {
                    det.AddMessage("\tRequest for ip at whatismyip.org failed because: " + content);
                    det.hasRetreivedPublicIp = 1;
                }
            }
            else
            {
                det.AddMessage("\tUnable to retreive public IP from whatismyip.org");
                det.hasRetreivedPublicIp = 0;
            }
            #endregion
            #region Listening for connections on public IP
            try
            {
                Transfer transfer = new Transfer(det.publicIP, 6000);
                FlowLib.Containers.UserInfo me = new FlowLib.Containers.UserInfo();
                me.DisplayName = "external";
                me.TagInfo.Version = "FlowLibPowered";
                transfer.Share = new FlowLib.Containers.Share("temp");
                transfer.Me = me;
                transfer.Source = new FlowLib.Containers.Source(det.publicIP, "external");
                TransferRequest req = new TransferRequest("external", null, new UserInfo());
                transferManager.AddTransferReq(req);

                transfer.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(transfer);
                transferManager.StartTransfer(transfer);

                det.AddMessage("#Can you receive data on public port?");
                // Wait 10 sec before continue.
                i = 0;
                do
                {
                    Thread.Sleep(500);
                }
                while (Detect.hasPublicReceiveAccess != 1 && i++ < 20);
                if (Detect.hasPublicReceiveAccess != 1)
                {
                    det.AddMessage("\tYou are not allowed receive data on public port.");
                }
                else
                {
                    det.AddMessage("\tYou received data on public port.");
                }


            }
            catch (System.Exception e)
            {

            }
            #endregion
            #region UPnP
            // TODO : Do UPnP request here.
            #endregion
            det.isDone = true;
        }

        static void tcpListener_Update(object sender, FlowLib.Events.FmdcEventArgs e)
        {
            switch (e.Action)
            {
                case Actions.TransferStarted:
                    Transfer trans = e.Data as Transfer;
                    if (trans != null)
                    {
                        if (trans.Protocol == null)
                        {
                            trans.Protocol = new FlowLib.Protocols.TransferNmdcProtocol(trans);
                            trans.Listen();
                            transferManager.AddTransfer(trans);
                        }

                        trans.Protocol.RequestTransfer += new FmdcEventHandler(Protocol_RequestTransfer);
                        e.Handled = true;
                    }
                    break;
            }
        }

        static void Protocol_RequestTransfer(object sender, FmdcEventArgs e)
        {
            TransferRequest req = e.Data as TransferRequest;
            req = transferManager.GetTransferReq(req.Key);
            if (req != null)
            {
                switch (req.Key)
                {
                    case "loopback":
                        Detect.hasInternalReceiveAccess = 1;
                        break;
                    case "external":
                        Detect.hasPublicReceiveAccess = 1;
                        break;
                }
                transferManager.RemoveTransferReq(req.Key);
            }
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
