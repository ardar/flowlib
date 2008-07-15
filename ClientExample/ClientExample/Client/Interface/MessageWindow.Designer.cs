namespace ClientExample.Client.Interface
{
    partial class MessageWindow
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
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.statusImageList = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ImageList = this.statusImageList;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(512, 344);
            this.tabControl1.TabIndex = 0;
            // 
            // statusImageList
            // 
            this.statusImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.statusImageList.ImageSize = new System.Drawing.Size(9, 9);
            this.statusImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // MessageWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 344);
            this.Controls.Add(this.tabControl1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MessageWindow";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Messages";
            this.Load += new System.EventHandler(this.MessageWindow_Load);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.ImageList statusImageList;

    }
}