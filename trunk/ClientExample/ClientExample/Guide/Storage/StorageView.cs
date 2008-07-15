using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace ClientExample.Guide.Storage
{
    public partial class StorageView : Form
    {
        public StorageView()
        {
            InitializeComponent();
        }

        private void StorageView_Load(object sender, EventArgs e)
        {
            System.IO.DriveInfo[] drivs = System.IO.DriveInfo.GetDrives();
            foreach (System.IO.DriveInfo drive in drivs)
            {
                if (drive.IsReady && hardDriveInfo1.DriveInfo == null)
                    hardDriveInfo1.DriveInfo = drive;
                //if (drive.DriveType == System.IO.DriveType.Fixed)
                //{

                //}
            }
        }
    }
}
