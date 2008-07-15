namespace ClientExample.Guide.Storage
{
    partial class StorageView
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
            this.hardDriveInfo1 = new ClientExample.Controls.HardDriveInfo();
            this.SuspendLayout();
            // 
            // hardDriveInfo1
            // 
            this.hardDriveInfo1.Location = new System.Drawing.Point(22, 21);
            this.hardDriveInfo1.Name = "hardDriveInfo1";
            this.hardDriveInfo1.Size = new System.Drawing.Size(282, 68);
            this.hardDriveInfo1.TabIndex = 0;
            // 
            // StorageView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(563, 308);
            this.Controls.Add(this.hardDriveInfo1);
            this.Name = "StorageView";
            this.Text = "StorageView";
            this.Load += new System.EventHandler(this.StorageView_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ClientExample.Controls.HardDriveInfo hardDriveInfo1;
    }
}