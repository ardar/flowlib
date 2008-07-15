using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using FlowLib.Events;

namespace ClientExample.Controls
{
    public partial class ExpandablePanel : UserControl
    {
        public event EventHandler<FmdcEventArgs<int, object>> ExpandedChanged;
        protected delegate void Change();

        protected int intExpandedHeight = 0;
        protected bool bExpanded = true;

        public bool Expanded
        {
            get
            {
                return bExpanded;
            }
            set
            {
                bExpanded = value;
                Invoke(new Change(OnChangeSize));
            }
        }

        protected void OnChangeSize()
        {
            int h = this.Height;
            ChangeSize();
            ExpandedChanged(this, new FmdcEventArgs<int, object>(h, null));
        }

        public Label TopLabel
        {
            get { return label1; }
        }

        public Panel Panel
        {
            get { return splitContainer1.Panel2; }
        }

        public ExpandablePanel()
        {
            while (Handle == null) { }
            ExpandedChanged = new EventHandler<FmdcEventArgs<int, object>>(ExtendablePanel_ExpandedChanged);
            InitializeComponent();
            intExpandedHeight = this.Height;
            Expanded = true;
            splitContainer1.Panel1.Click += new EventHandler(Panel1_Click);
            label1.Click += new EventHandler(Panel1_Click);
        }

        void ExtendablePanel_ExpandedChanged(object sender, FmdcEventArgs<int, object> e) { }

        void Panel1_Click(object sender, EventArgs e)
        {
            Expanded = !Expanded;
        }

        private void ExtendablePanel_Load(object sender, EventArgs e)
        {
            Console.WriteLine("Loading ExtendablePanel");
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Expanded = !Expanded;
        }

        private void ChangeSize()
        {
            if (Expanded)
            {
                splitContainer1.Panel2Collapsed = false;
                Height = intExpandedHeight;
            }
            else
            {
                intExpandedHeight = this.Height;
                Height = splitContainer1.Panel1.Height;
                splitContainer1.Panel2Collapsed = true;
            }
            ChangeImage();
        }

        private void ChangeImage()
        {
            if (Expanded)
                pictureBox1.Image = new Bitmap(typeof(Program), @"Images.expand.gif");
            else
                pictureBox1.Image = new Bitmap(typeof(Program), @"Images.collapse.gif");
        }
    }
}
