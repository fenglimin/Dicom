namespace DicomRis
{
	partial class MainForm
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
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.folderDialogButton = new System.Windows.Forms.Button();
			this.folderPathTextBox = new System.Windows.Forms.TextBox();
			this.FolderDescriptionToolTip = new System.Windows.Forms.ToolTip(this.components);
			this.GenerateMwlRadioButton = new System.Windows.Forms.RadioButton();
			this.SelectMwlRadioButton = new System.Windows.Forms.RadioButton();
			this.AboutButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			// 
			// folderDialogButton
			// 
			this.folderDialogButton.Location = new System.Drawing.Point(463, 33);
			this.folderDialogButton.Name = "folderDialogButton";
			this.folderDialogButton.Size = new System.Drawing.Size(66, 23);
			this.folderDialogButton.TabIndex = 1;
			this.folderDialogButton.Text = "Open...";
			this.FolderDescriptionToolTip.SetToolTip(this.folderDialogButton, "Folder containing dcm files containing ris patient data, provided in response to " +
        "a mwl queery.");
			this.folderDialogButton.UseVisualStyleBackColor = true;
			this.folderDialogButton.Click += new System.EventHandler(this.folderDialogButton_Click);
			// 
			// folderPathTextBox
			// 
			this.folderPathTextBox.Location = new System.Drawing.Point(238, 34);
			this.folderPathTextBox.Name = "folderPathTextBox";
			this.folderPathTextBox.Size = new System.Drawing.Size(219, 20);
			this.folderPathTextBox.TabIndex = 2;
			this.FolderDescriptionToolTip.SetToolTip(this.folderPathTextBox, "Folder containing dcm files containing ris patient data, provided in response to " +
        "a mwl queery.");
			// 
			// GenerateMwlRadioButton
			// 
			this.GenerateMwlRadioButton.AutoSize = true;
			this.GenerateMwlRadioButton.Checked = true;
			this.GenerateMwlRadioButton.Location = new System.Drawing.Point(22, 12);
			this.GenerateMwlRadioButton.Name = "GenerateMwlRadioButton";
			this.GenerateMwlRadioButton.Size = new System.Drawing.Size(114, 17);
			this.GenerateMwlRadioButton.TabIndex = 5;
			this.GenerateMwlRadioButton.TabStop = true;
			this.GenerateMwlRadioButton.Text = "Generate mwl data";
			this.FolderDescriptionToolTip.SetToolTip(this.GenerateMwlRadioButton, "mwl dcm files were created in <current directory>/out");
			this.GenerateMwlRadioButton.UseVisualStyleBackColor = true;
			// 
			// SelectMwlRadioButton
			// 
			this.SelectMwlRadioButton.AutoSize = true;
			this.SelectMwlRadioButton.Location = new System.Drawing.Point(22, 35);
			this.SelectMwlRadioButton.Name = "SelectMwlRadioButton";
			this.SelectMwlRadioButton.Size = new System.Drawing.Size(210, 17);
			this.SelectMwlRadioButton.TabIndex = 6;
			this.SelectMwlRadioButton.Text = "Select folder containing mwl .dcm file(s)";
			this.FolderDescriptionToolTip.SetToolTip(this.SelectMwlRadioButton, "Navigate to a folder containing mwl dcm files");
			this.SelectMwlRadioButton.UseVisualStyleBackColor = true;
			// 
			// AboutButton
			// 
			this.AboutButton.Location = new System.Drawing.Point(506, 4);
			this.AboutButton.Name = "AboutButton";
			this.AboutButton.Size = new System.Drawing.Size(23, 23);
			this.AboutButton.TabIndex = 13;
			this.AboutButton.Text = "?";
			this.AboutButton.UseVisualStyleBackColor = true;
			this.AboutButton.Click += new System.EventHandler(this.AboutButton_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(548, 77);
			this.Controls.Add(this.AboutButton);
			this.Controls.Add(this.SelectMwlRadioButton);
			this.Controls.Add(this.GenerateMwlRadioButton);
			this.Controls.Add(this.folderPathTextBox);
			this.Controls.Add(this.folderDialogButton);
			this.Name = "MainForm";
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button folderDialogButton;
		private System.Windows.Forms.TextBox folderPathTextBox;
		private System.Windows.Forms.ToolTip FolderDescriptionToolTip;
		private System.Windows.Forms.RadioButton GenerateMwlRadioButton;
		private System.Windows.Forms.RadioButton SelectMwlRadioButton;
		private System.Windows.Forms.Button AboutButton;
	}
}

