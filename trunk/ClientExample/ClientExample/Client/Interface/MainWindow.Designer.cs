namespace ClientExample.Client.Interface
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolBtnUsers = new System.Windows.Forms.ToolStripButton();
            this.toolBtnMessages = new System.Windows.Forms.ToolStripButton();
            this.list1 = new ClientExample.Controls.List();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolBtnUsers,
            this.toolBtnMessages});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(214, 25);
            this.toolStrip1.TabIndex = 3;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolBtnUsers
            // 
            this.toolBtnUsers.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBtnUsers.Image = ((System.Drawing.Image)(resources.GetObject("toolBtnUsers.Image")));
            this.toolBtnUsers.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBtnUsers.Name = "toolBtnUsers";
            this.toolBtnUsers.Size = new System.Drawing.Size(23, 22);
            this.toolBtnUsers.Text = "toolStripButton1";
            this.toolBtnUsers.Click += new System.EventHandler(this.toolBtnUsers_Click);
            // 
            // toolBtnMessages
            // 
            this.toolBtnMessages.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolBtnMessages.Image = ((System.Drawing.Image)(resources.GetObject("toolBtnMessages.Image")));
            this.toolBtnMessages.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolBtnMessages.Name = "toolBtnMessages";
            this.toolBtnMessages.Size = new System.Drawing.Size(23, 22);
            this.toolBtnMessages.Text = "Open Messages";
            this.toolBtnMessages.Click += new System.EventHandler(this.toolBtnMessages_Click);
            // 
            // list1
            // 
            this.list1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.list1.Location = new System.Drawing.Point(0, 28);
            this.list1.Name = "list1";
            this.list1.Size = new System.Drawing.Size(214, 502);
            this.list1.TabIndex = 4;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(214, 530);
            this.Controls.Add(this.list1);
            this.Controls.Add(this.toolStrip1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Xmple";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ClientExample.Controls.ExpandablePanel extendablePanel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolBtnUsers;
        private System.Windows.Forms.ToolStripButton toolBtnMessages;
        private ClientExample.Controls.List list1;
    }
}