namespace ClientExample.Guide
{
    partial class GuideWindow
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.ucWelcome1 = new ClientExample.Guide.UCWelcome();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.ucInstallationMode1 = new ClientExample.Guide.UCInstallationMode();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.ucImport1 = new ClientExample.Guide.UCImport();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.ucConnection1 = new ClientExample.Guide.UCConnection();
            this.tabPage100 = new System.Windows.Forms.TabPage();
            this.ucFinished1 = new ClientExample.Guide.UCFinished();
            this.btnNext = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabPage4.SuspendLayout();
            this.tabPage100.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Controls.Add(this.tabPage100);
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(648, 372);
            this.tabControl1.TabIndex = 100;
            this.tabControl1.TabStop = false;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.ucWelcome1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(640, 346);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Welcome";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // ucWelcome1
            // 
            this.ucWelcome1.BackColor = System.Drawing.Color.White;
            this.ucWelcome1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucWelcome1.Location = new System.Drawing.Point(3, 3);
            this.ucWelcome1.Name = "ucWelcome1";
            this.ucWelcome1.Size = new System.Drawing.Size(634, 340);
            this.ucWelcome1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.ucInstallationMode1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(640, 346);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Installation Mode";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // ucInstallationMode1
            // 
            this.ucInstallationMode1.BackColor = System.Drawing.Color.White;
            this.ucInstallationMode1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucInstallationMode1.Location = new System.Drawing.Point(3, 3);
            this.ucInstallationMode1.Name = "ucInstallationMode1";
            this.ucInstallationMode1.Size = new System.Drawing.Size(634, 340);
            this.ucInstallationMode1.TabIndex = 0;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.ucImport1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(640, 346);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Import";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // ucImport1
            // 
            this.ucImport1.BackColor = System.Drawing.Color.White;
            this.ucImport1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucImport1.Location = new System.Drawing.Point(3, 3);
            this.ucImport1.Mode = 0;
            this.ucImport1.Name = "ucImport1";
            this.ucImport1.Size = new System.Drawing.Size(634, 340);
            this.ucImport1.TabIndex = 0;
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.ucConnection1);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Size = new System.Drawing.Size(640, 346);
            this.tabPage4.TabIndex = 4;
            this.tabPage4.Text = "Connection";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // ucConnection1
            // 
            this.ucConnection1.BackColor = System.Drawing.Color.White;
            this.ucConnection1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucConnection1.Location = new System.Drawing.Point(0, 0);
            this.ucConnection1.Name = "ucConnection1";
            this.ucConnection1.Size = new System.Drawing.Size(640, 346);
            this.ucConnection1.TabIndex = 0;
            // 
            // tabPage100
            // 
            this.tabPage100.Controls.Add(this.ucFinished1);
            this.tabPage100.Location = new System.Drawing.Point(4, 22);
            this.tabPage100.Name = "tabPage100";
            this.tabPage100.Size = new System.Drawing.Size(640, 346);
            this.tabPage100.TabIndex = 3;
            this.tabPage100.Text = "Finished";
            this.tabPage100.UseVisualStyleBackColor = true;
            // 
            // ucFinished1
            // 
            this.ucFinished1.BackColor = System.Drawing.Color.White;
            this.ucFinished1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ucFinished1.Location = new System.Drawing.Point(0, 0);
            this.ucFinished1.Name = "ucFinished1";
            this.ucFinished1.Size = new System.Drawing.Size(640, 346);
            this.ucFinished1.TabIndex = 0;
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(559, 374);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(85, 23);
            this.btnNext.TabIndex = 0;
            this.btnNext.Text = "Next";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select installation path to import from";
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.MyComputer;
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // GuideWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(648, 400);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.tabControl1);
            this.Name = "GuideWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Installation Guide";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage4.ResumeLayout(false);
            this.tabPage100.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private UCWelcome ucWelcome1;
        private UCInstallationMode ucInstallationMode1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.Button btnNext;
        private UCImport ucImport1;
        private System.Windows.Forms.TabPage tabPage100;
        private UCFinished ucFinished1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TabPage tabPage4;
        private UCConnection ucConnection1;
    }
}