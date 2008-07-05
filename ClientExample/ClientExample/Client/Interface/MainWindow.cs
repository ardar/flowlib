using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ClientExample.Client.Interface
{
    public partial class MainWindow : Form
    {
        protected MessageWindow msgWindow = new MessageWindow();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            toolBtnUsers.Image = new Bitmap(typeof(Program), @"Images.user.gif");
            toolBtnMessages.Image = new Bitmap(typeof(Program), @"Images.pen.gif");

            ListView listview = new ListView();
            listview.View = View.Details;
            listview.Columns.Add("Nick", 190);
            listview.Items.Add("-=Aquila=-");
            listview.Items.Add("-=OpChat=-");
            listview.Items.Add("ChrisShUK");
            listview.Items.Add("Daywalker™");
            listview.Items.Add("exPERten");
            listview.Items.Add("Flow84");
            listview.Items.Add("Gabberworld");
            listview.Items.Add("Gnuff");
            listview.Items.Add("honda");
            listview.Items.Add("MikeJJ");
            listview.Items.Add("Misçhiêvøus");
            listview.Items.Add("Pharaoh");
            listview.Items.Add("Pothead");
            listview.Items.Add("SeNcO");
            listview.Items.Add("TheNOP");
            listview.Items.Add("Tiberian");
            listview.Scrollable = false;

            ClientExample.Controls.ExpandablePanel panel = new ClientExample.Controls.ExpandablePanel();
            panel.TopLabel.Text = " Test Hub (16 users)";
            panel.Panel.Controls.Add(listview);
            listview.Dock = DockStyle.Fill;
            list1.Items.Add(panel);
            panel.Dock = DockStyle.Fill;

            ClientExample.Controls.ExpandablePanel panel2 = new ClientExample.Controls.ExpandablePanel();
            panel2.TopLabel.Text = "Wiee";
            //panel2.Panel.Controls.Add(listview);
            Button b = new Button();
            b.Text = "Button :D";
            panel2.Panel.Controls.Add(b);
            list1.Items.Add(panel2);




        }

        private void toolBtnUsers_Click(object sender, EventArgs e)
        {

        }

        private void toolBtnMessages_Click(object sender, EventArgs e)
        {
            Screen screen = Screen.FromControl(this);
            int pos = this.Left + this.Width + msgWindow.Width;
            msgWindow.Show();
            msgWindow.WindowState = FormWindowState.Normal;
            if (pos < screen.WorkingArea.Left + screen.WorkingArea.Width)
                msgWindow.Location = new Point(this.Left + this.Width, this.Top);
            else
                msgWindow.Location = new Point(this.Left - msgWindow.Width, this.Top);
        }
    }
}
