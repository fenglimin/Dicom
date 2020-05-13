namespace ExtendedListTest
{
	partial class TinyPACS
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TinyPACS));
            this.tcDicomFiles = new System.Windows.Forms.TabControl();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_Open = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_Save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_SaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Cached = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_OpenWhenReceived = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_SaveWhenReceived = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripComboBox_DicomDir = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripButton_OpenDicomDir = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_SaveDicomDir = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_Test = new System.Windows.Forms.ToolStripButton();
            this.toolStripComboBox_Message = new System.Windows.Forms.ToolStripComboBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tcDicomFiles
            // 
            this.tcDicomFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tcDicomFiles.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tcDicomFiles.Location = new System.Drawing.Point(1, 30);
            this.tcDicomFiles.Name = "tcDicomFiles";
            this.tcDicomFiles.SelectedIndex = 0;
            this.tcDicomFiles.Size = new System.Drawing.Size(1442, 668);
            this.tcDicomFiles.TabIndex = 1;
            this.tcDicomFiles.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.tcDicomFiles_DrawItem);
            this.tcDicomFiles.SelectedIndexChanged += new System.EventHandler(this.tcDicomFiles_SelectedIndexChanged);
            this.tcDicomFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tcDicomFiles_MouseDown);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_Open,
            this.toolStripButton_Save,
            this.toolStripButton_SaveAs,
            this.toolStripSeparator1,
            this.toolStripButton_Cached,
            this.toolStripSeparator2,
            this.toolStripButton_OpenWhenReceived,
            this.toolStripButton_SaveWhenReceived,
            this.toolStripSeparator3,
            this.toolStripComboBox_DicomDir,
            this.toolStripButton_OpenDicomDir,
            this.toolStripButton_SaveDicomDir,
            this.toolStripSeparator4,
            this.toolStripButton_Test,
            this.toolStripComboBox_Message});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1442, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_Open
            // 
            this.toolStripButton_Open.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Open.Image = global::ExtendedListTest.Properties.Resources.Open;
            this.toolStripButton_Open.ImageTransparentColor = System.Drawing.SystemColors.ButtonFace;
            this.toolStripButton_Open.Name = "toolStripButton_Open";
            this.toolStripButton_Open.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Open.Text = "Open...";
            this.toolStripButton_Open.Click += new System.EventHandler(this.toolStripButton_Open_Click);
            // 
            // toolStripButton_Save
            // 
            this.toolStripButton_Save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Save.Enabled = false;
            this.toolStripButton_Save.Image = global::ExtendedListTest.Properties.Resources.Save;
            this.toolStripButton_Save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Save.Name = "toolStripButton_Save";
            this.toolStripButton_Save.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Save.Text = "Save";
            this.toolStripButton_Save.Click += new System.EventHandler(this.toolStripButton_Save_Click);
            // 
            // toolStripButton_SaveAs
            // 
            this.toolStripButton_SaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_SaveAs.Enabled = false;
            this.toolStripButton_SaveAs.Image = global::ExtendedListTest.Properties.Resources.SaveAs;
            this.toolStripButton_SaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_SaveAs.Name = "toolStripButton_SaveAs";
            this.toolStripButton_SaveAs.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_SaveAs.Text = "SaveAs";
            this.toolStripButton_SaveAs.Visible = false;
            this.toolStripButton_SaveAs.Click += new System.EventHandler(this.toolStripButton_SaveAs_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Cached
            // 
            this.toolStripButton_Cached.Enabled = false;
            this.toolStripButton_Cached.Image = global::ExtendedListTest.Properties.Resources.Cache2;
            this.toolStripButton_Cached.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Cached.Name = "toolStripButton_Cached";
            this.toolStripButton_Cached.Size = new System.Drawing.Size(91, 22);
            this.toolStripButton_Cached.Text = "Received (0)";
            this.toolStripButton_Cached.Click += new System.EventHandler(this.toolStripButton_Cached_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_OpenWhenReceived
            // 
            this.toolStripButton_OpenWhenReceived.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_OpenWhenReceived.Image = global::ExtendedListTest.Properties.Resources.Untitled2;
            this.toolStripButton_OpenWhenReceived.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_OpenWhenReceived.Name = "toolStripButton_OpenWhenReceived";
            this.toolStripButton_OpenWhenReceived.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_OpenWhenReceived.Text = "Open Image When Received";
            this.toolStripButton_OpenWhenReceived.Click += new System.EventHandler(this.toolStripButton_OpenWhenReceived_Click);
            // 
            // toolStripButton_SaveWhenReceived
            // 
            this.toolStripButton_SaveWhenReceived.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_SaveWhenReceived.Image = global::ExtendedListTest.Properties.Resources.Untitled1;
            this.toolStripButton_SaveWhenReceived.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_SaveWhenReceived.Name = "toolStripButton_SaveWhenReceived";
            this.toolStripButton_SaveWhenReceived.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_SaveWhenReceived.Text = "Save Image When Received";
            this.toolStripButton_SaveWhenReceived.Click += new System.EventHandler(this.toolStripButton_SaveWhenReceived_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripComboBox_DicomDir
            // 
            this.toolStripComboBox_DicomDir.DropDownHeight = 300;
            this.toolStripComboBox_DicomDir.IntegralHeight = false;
            this.toolStripComboBox_DicomDir.Name = "toolStripComboBox_DicomDir";
            this.toolStripComboBox_DicomDir.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBox_DicomDir.ToolTipText = "Select or New a Dicom Dir";
            this.toolStripComboBox_DicomDir.KeyDown += new System.Windows.Forms.KeyEventHandler(this.toolStripComboBox_DicomDir_KeyDown);
            this.toolStripComboBox_DicomDir.TextChanged += new System.EventHandler(this.toolStripComboBox_DicomDir_TextChanged);
            // 
            // toolStripButton_OpenDicomDir
            // 
            this.toolStripButton_OpenDicomDir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_OpenDicomDir.Image = global::ExtendedListTest.Properties.Resources.Open;
            this.toolStripButton_OpenDicomDir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_OpenDicomDir.Name = "toolStripButton_OpenDicomDir";
            this.toolStripButton_OpenDicomDir.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_OpenDicomDir.Text = "Open Dicom Dir";
            this.toolStripButton_OpenDicomDir.Click += new System.EventHandler(this.toolStripButton_OpenDicomDir_Click);
            // 
            // toolStripButton_SaveDicomDir
            // 
            this.toolStripButton_SaveDicomDir.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_SaveDicomDir.Enabled = false;
            this.toolStripButton_SaveDicomDir.Image = global::ExtendedListTest.Properties.Resources.SaveAs;
            this.toolStripButton_SaveDicomDir.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_SaveDicomDir.Name = "toolStripButton_SaveDicomDir";
            this.toolStripButton_SaveDicomDir.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_SaveDicomDir.Text = "toolStripButton1";
            this.toolStripButton_SaveDicomDir.ToolTipText = "Save to Dicom Dir";
            this.toolStripButton_SaveDicomDir.Click += new System.EventHandler(this.toolStripButton_SaveDicomDir_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_Test
            // 
            this.toolStripButton_Test.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_Test.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_Test.Image")));
            this.toolStripButton_Test.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_Test.Name = "toolStripButton_Test";
            this.toolStripButton_Test.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_Test.Text = "Test";
            this.toolStripButton_Test.Click += new System.EventHandler(this.toolStripButton_Test_Click);
            // 
            // toolStripComboBox_Message
            // 
            this.toolStripComboBox_Message.AutoSize = false;
            this.toolStripComboBox_Message.DropDownHeight = 600;
            this.toolStripComboBox_Message.DropDownWidth = 500;
            this.toolStripComboBox_Message.IntegralHeight = false;
            this.toolStripComboBox_Message.MaxDropDownItems = 30;
            this.toolStripComboBox_Message.Name = "toolStripComboBox_Message";
            this.toolStripComboBox_Message.Size = new System.Drawing.Size(1000, 23);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Dicom files|*.dcm";
            // 
            // TinyPACS
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1442, 698);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.tcDicomFiles);
            this.Name = "TinyPACS";
            this.Text = "Tiny PACS";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form2_FormClosed);
            this.Load += new System.EventHandler(this.Form2_Load);
            this.SizeChanged += new System.EventHandler(this.TinyPACS_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

        private System.Windows.Forms.TabControl tcDicomFiles;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Open;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_Cached;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripButton toolStripButton_Test;
        private System.Windows.Forms.ToolStripButton toolStripButton_Save;
        private System.Windows.Forms.ToolStripButton toolStripButton_SaveAs;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripButton toolStripButton_OpenWhenReceived;
        private System.Windows.Forms.ToolStripButton toolStripButton_SaveWhenReceived;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_DicomDir;
        private System.Windows.Forms.ToolStripButton toolStripButton_SaveDicomDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripComboBox toolStripComboBox_Message;
		private System.Windows.Forms.ToolStripButton toolStripButton_OpenDicomDir;
	}
}