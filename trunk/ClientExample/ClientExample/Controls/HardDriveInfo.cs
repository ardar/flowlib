using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using System.IO;

namespace ClientExample.Controls
{
    public partial class HardDriveInfo : UserControl
    {
        protected DriveInfo drive = null;
        protected int procentage = 0;


        public HardDriveInfo()
        {
            InitializeComponent();
        }

        public DriveInfo DriveInfo
        {
            get { return drive; }
            set
            {
                drive = value;
                label1.Text = drive.Name;
                try
                {
                    progressBar1.Maximum = 100;
                    procentage = (int)(((double)drive.TotalFreeSpace / (double)drive.TotalSize) * 100);
                    if (procentage < 5)
                    {
                        progressBar1.ForeColor = Color.Red;
                        progressBar1.BackColor = Color.Red;
                    }
                    else if (procentage < 15)
                    {
                        progressBar1.ForeColor = Color.Orange;
                        progressBar1.BackColor = Color.Orange;
                    }
                    else if (procentage < 20)
                    {
                        progressBar1.ForeColor = Color.Yellow;
                        progressBar1.BackColor = Color.Yellow;
                    }
                    else if (procentage < 25)
                    {
                        progressBar1.ForeColor = Color.YellowGreen;
                        progressBar1.BackColor = Color.YellowGreen;
                    }
                    else if (procentage < 30)
                    {
                        progressBar1.ForeColor = Color.Green;
                        progressBar1.BackColor = Color.Green;
                    }
                    progressBar1.Value = 100 - procentage;
                }
                catch
                {
                    progressBar1.ForeColor = Color.Gray;
                }

            }
        }

        private void HardDriveInfo_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = new Bitmap(typeof(Program), @"Images.drive.gif");
        }
    }
}
