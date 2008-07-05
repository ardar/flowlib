namespace ClientExample.Guide.Connection
{
    partial class Detect
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnInternet = new System.Windows.Forms.Button();
            this.btnLoopback = new System.Windows.Forms.Button();
            this.btnPublicIP = new System.Windows.Forms.Button();
            this.btnNat = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox1.Location = new System.Drawing.Point(12, 41);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(384, 112);
            this.textBox1.TabIndex = 0;
            // 
            // btnInternet
            // 
            this.btnInternet.Enabled = false;
            this.btnInternet.Location = new System.Drawing.Point(12, 12);
            this.btnInternet.Name = "btnInternet";
            this.btnInternet.Size = new System.Drawing.Size(72, 23);
            this.btnInternet.TabIndex = 1;
            this.btnInternet.Text = "Internet";
            this.btnInternet.UseVisualStyleBackColor = true;
            // 
            // btnLoopback
            // 
            this.btnLoopback.Enabled = false;
            this.btnLoopback.Location = new System.Drawing.Point(90, 12);
            this.btnLoopback.Name = "btnLoopback";
            this.btnLoopback.Size = new System.Drawing.Size(72, 23);
            this.btnLoopback.TabIndex = 2;
            this.btnLoopback.Text = "Loopback";
            this.btnLoopback.UseVisualStyleBackColor = true;
            // 
            // btnPublicIP
            // 
            this.btnPublicIP.Enabled = false;
            this.btnPublicIP.Location = new System.Drawing.Point(168, 12);
            this.btnPublicIP.Name = "btnPublicIP";
            this.btnPublicIP.Size = new System.Drawing.Size(72, 23);
            this.btnPublicIP.TabIndex = 4;
            this.btnPublicIP.Text = "Public IP";
            this.btnPublicIP.UseVisualStyleBackColor = true;
            // 
            // btnNat
            // 
            this.btnNat.Enabled = false;
            this.btnNat.Location = new System.Drawing.Point(246, 12);
            this.btnNat.Name = "btnNat";
            this.btnNat.Size = new System.Drawing.Size(72, 23);
            this.btnNat.TabIndex = 5;
            this.btnNat.Text = "NAT";
            this.btnNat.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Location = new System.Drawing.Point(324, 12);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "UPnP";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(321, 159);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 7;
            this.btnOk.Text = "Close";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 164);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 23);
            this.label1.TabIndex = 8;
            this.label1.Text = "Status:";
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(75, 164);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(240, 23);
            this.lblStatus.TabIndex = 9;
            // 
            // Detect
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 190);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnNat);
            this.Controls.Add(this.btnPublicIP);
            this.Controls.Add(this.btnLoopback);
            this.Controls.Add(this.btnInternet);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Detect";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Detect";
            this.Load += new System.EventHandler(this.Detect_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button btnInternet;
        private System.Windows.Forms.Button btnLoopback;
        private System.Windows.Forms.Button btnPublicIP;
        private System.Windows.Forms.Button btnNat;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblStatus;
    }
}