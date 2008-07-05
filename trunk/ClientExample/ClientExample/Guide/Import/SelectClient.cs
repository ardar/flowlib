using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using FlowLib.Utils.Convert.Settings;
using FlowLib.Containers;

namespace ClientExample.Guide.Import
{
    public partial class SelectClient : Form
    {
        bool converted = false;

        List<HubSetting> settings = null;

        public List<HubSetting> Settings
        {
            get { return settings; }
        }

        public List<string> Files
        {
            set
            {
                foreach (string item in value)
                {
                    comboBox2.Items.Add(item);   
                }
                if (comboBox2.Items.Count > 0)
                    comboBox2.SelectedIndex = 0;
            }
        }

        public SelectClient()
        {
            InitializeComponent();
        }

        void comboBox2_SelectedValueChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = false;

            label1.Enabled = false;
            label2.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                comboBox1.Enabled = true;
                label2.Enabled = true;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                button3.Enabled = true;
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (converted)
            {
                this.Close();
                return;
            }

            button3.Enabled = false;

            try
            {
                // Convert from client?
                BaseClient clientFrom = null;
                switch (comboBox1.Text)
                {
                    case "AirDC (2.01)":
                        clientFrom = new AirDC(); break;
                    case "Apex (0.40)":
                        clientFrom = new Apex(Apex.Versions.v0_40); break;
                    case "Apex (1.0.0b5)":
                        clientFrom = new Apex(Apex.Versions.v1_00Beta5); break;
                    case "BCDC":
                        clientFrom = new BCDC(); break;
                    case "CrzDC (Beta3)":
                        clientFrom = new CrZDC(); break;
                    case "CZDZ (?)":
                        clientFrom = new CzDZ(); break;
                    case "DC++":
                        clientFrom = new DCpp(); break;
                    case "DCDM":
                        clientFrom = new DCDM(); break;
                    case "fulDC (6.78)":
                        clientFrom = new FulDC(); break;
                    case "IceDC (1.00a)":
                        clientFrom = new IceDC(); break;
                    case "LDC (1.00v2a)":
                        clientFrom = new LDC(); break;
                    case "RSX (1.00)":
                        clientFrom = new RSX(); break;
                    case "StrongDC-Lite (131)":
                        clientFrom = new StrongDCLite(); break;
                    case "StrongDC (2.1)":
                        clientFrom = new StrongDC(); break;
                    case "ZionBlue (2.01/2.02)":
                        clientFrom = new ZionBlue(ZionBlue.Versions.v2_01); break;
                    case "ZionBlue (2.03)":
                        clientFrom = new ZionBlue(ZionBlue.Versions.v2_03); break;
                    case "ZionBlue (2.04)":
                        clientFrom = new ZionBlue(ZionBlue.Versions.v2_04); break;
                    case "ZionBlue (2.05)":
                        clientFrom = new ZionBlue(ZionBlue.Versions.v2_05); break;
                    case "zK (0.710)":
                        clientFrom = new zK(); break;
                }

                bool error = false;

                if (clientFrom == null)
                {
                    MessageBox.Show("Some Error occured. Restart app and try again.");
                    error = true;
                }

                if (!error)
                {
                    clientFrom.Read(openFileDialog1.FileName);

                    settings = clientFrom.Hubs;
                    MessageBox.Show("Convertion has been done");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                //System.IO.File.WriteAllText(System.AppDomain.CurrentDomain.BaseDirectory + "exceptions.txt", ex.ToString());
            }

            converted = true;
            button3.Enabled = true;
            button3.Text = "Close";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            comboBox1.Enabled = true;
            foreach (object item in comboBox1.Items)
	        {
                StringBuilder sb = new StringBuilder(item.ToString().ToLower());
                sb.Replace(" ", "");
                sb.Replace("(", "");
                sb.Replace(")", "");
                sb.Replace("/", "");
                sb.Replace("+", "");
                sb.Replace("?", "");
                sb.Replace(".", "");
                sb.Replace("0", "");
                sb.Replace("1", "");
                sb.Replace("2", "");
                sb.Replace("3", "");
                sb.Replace("4", "");
                sb.Replace("5", "");
                sb.Replace("6", "");
                sb.Replace("7", "");
                sb.Replace("8", "");
                sb.Replace("9", "");

        		 if (comboBox2.Text.IndexOf(sb.ToString(),  StringComparison.OrdinalIgnoreCase) != -1)
                 {
                     comboBox1.SelectedItem = item;
                     break;
                 }
	        }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
            button3.Text = "Next";
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            Close();
        }
    }
}