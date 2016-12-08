namespace ExtendedListTest
{
	partial class StorageCommitmentForm
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
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.tbPort = new System.Windows.Forms.TextBox();
			this.tbAeIpAddress = new System.Windows.Forms.TextBox();
			this.tbAeTitle = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.lbFailedImage = new System.Windows.Forms.ListBox();
			this.label4 = new System.Windows.Forms.Label();
			this.rbFailed = new System.Windows.Forms.RadioButton();
			this.rbSuccess = new System.Windows.Forms.RadioButton();
			this.btSend = new System.Windows.Forms.Button();
			this.btClose = new System.Windows.Forms.Button();
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.tbPort);
			this.groupBox1.Controls.Add(this.tbAeIpAddress);
			this.groupBox1.Controls.Add(this.tbAeTitle);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Location = new System.Drawing.Point(12, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(301, 118);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Destination";
			// 
			// tbPort
			// 
			this.tbPort.Location = new System.Drawing.Point(138, 80);
			this.tbPort.Name = "tbPort";
			this.tbPort.Size = new System.Drawing.Size(142, 20);
			this.tbPort.TabIndex = 5;
			this.tbPort.Text = "5040";
			// 
			// tbAeIpAddress
			// 
			this.tbAeIpAddress.Location = new System.Drawing.Point(139, 53);
			this.tbAeIpAddress.Name = "tbAeIpAddress";
			this.tbAeIpAddress.ReadOnly = true;
			this.tbAeIpAddress.Size = new System.Drawing.Size(142, 20);
			this.tbAeIpAddress.TabIndex = 4;
			// 
			// tbAeTitle
			// 
			this.tbAeTitle.Location = new System.Drawing.Point(139, 25);
			this.tbAeTitle.Name = "tbAeTitle";
			this.tbAeTitle.ReadOnly = true;
			this.tbAeTitle.Size = new System.Drawing.Size(142, 20);
			this.tbAeTitle.TabIndex = 3;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(6, 84);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(126, 13);
			this.label3.TabIndex = 2;
			this.label3.Text = "Storage Commitment Port";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(6, 58);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(71, 13);
			this.label2.TabIndex = 1;
			this.label2.Text = "AE IpAddress";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(6, 28);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(44, 13);
			this.label1.TabIndex = 0;
			this.label1.Text = "AE Title";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.lbFailedImage);
			this.groupBox2.Controls.Add(this.label4);
			this.groupBox2.Controls.Add(this.rbFailed);
			this.groupBox2.Controls.Add(this.rbSuccess);
			this.groupBox2.Location = new System.Drawing.Point(14, 147);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(301, 158);
			this.groupBox2.TabIndex = 6;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Storage Commitment Result";
			// 
			// lbFailedImage
			// 
			this.lbFailedImage.FormattingEnabled = true;
			this.lbFailedImage.Location = new System.Drawing.Point(7, 79);
			this.lbFailedImage.Name = "lbFailedImage";
			this.lbFailedImage.Size = new System.Drawing.Size(271, 69);
			this.lbFailedImage.TabIndex = 7;
			this.lbFailedImage.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lbFailedImage_MouseDoubleClick);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(9, 57);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(86, 13);
			this.label4.TabIndex = 6;
			this.label4.Text = "Failed Image List";
			// 
			// rbFailed
			// 
			this.rbFailed.AutoSize = true;
			this.rbFailed.Location = new System.Drawing.Point(137, 31);
			this.rbFailed.Name = "rbFailed";
			this.rbFailed.Size = new System.Drawing.Size(53, 17);
			this.rbFailed.TabIndex = 1;
			this.rbFailed.Text = "Failed";
			this.rbFailed.UseVisualStyleBackColor = true;
			// 
			// rbSuccess
			// 
			this.rbSuccess.AutoSize = true;
			this.rbSuccess.Checked = true;
			this.rbSuccess.Location = new System.Drawing.Point(7, 31);
			this.rbSuccess.Name = "rbSuccess";
			this.rbSuccess.Size = new System.Drawing.Size(66, 17);
			this.rbSuccess.TabIndex = 0;
			this.rbSuccess.TabStop = true;
			this.rbSuccess.Text = "Success";
			this.rbSuccess.UseVisualStyleBackColor = true;
			// 
			// btSend
			// 
			this.btSend.Location = new System.Drawing.Point(19, 324);
			this.btSend.Name = "btSend";
			this.btSend.Size = new System.Drawing.Size(75, 23);
			this.btSend.TabIndex = 7;
			this.btSend.Text = "Send";
			this.btSend.UseVisualStyleBackColor = true;
			this.btSend.Click += new System.EventHandler(this.btSend_Click);
			// 
			// btClose
			// 
			this.btClose.Location = new System.Drawing.Point(238, 324);
			this.btClose.Name = "btClose";
			this.btClose.Size = new System.Drawing.Size(75, 23);
			this.btClose.TabIndex = 8;
			this.btClose.Text = "Exit";
			this.btClose.UseVisualStyleBackColor = true;
			this.btClose.Click += new System.EventHandler(this.btClose_Click);
			// 
			// StorageCommitmentForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(327, 359);
			this.Controls.Add(this.btClose);
			this.Controls.Add(this.btSend);
			this.Controls.Add(this.groupBox2);
			this.Controls.Add(this.groupBox1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "StorageCommitmentForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "StorageCommitmentForm";
			this.Load += new System.EventHandler(this.StorageCommitmentForm_Load);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbPort;
		private System.Windows.Forms.TextBox tbAeIpAddress;
		private System.Windows.Forms.TextBox tbAeTitle;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.RadioButton rbFailed;
		private System.Windows.Forms.RadioButton rbSuccess;
		private System.Windows.Forms.Button btSend;
		private System.Windows.Forms.Button btClose;
		private System.Windows.Forms.ListBox lbFailedImage;
		private System.Windows.Forms.Label label4;
	}
}