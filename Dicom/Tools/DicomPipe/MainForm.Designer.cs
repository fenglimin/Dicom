namespace DicomPipe
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
            this.PipeGroupBox = new System.Windows.Forms.GroupBox();
            this.LoggingCheckBox = new System.Windows.Forms.CheckBox();
            this.ScriptLabel = new System.Windows.Forms.Label();
            this.ScriptComboBox = new System.Windows.Forms.ComboBox();
            this.PipePortLabel = new System.Windows.Forms.Label();
            this.PipePortTextBox = new System.Windows.Forms.TextBox();
            this.ScpGroupBox = new System.Windows.Forms.GroupBox();
            this.ScpDeleteButton = new System.Windows.Forms.Button();
            this.ScpTitleComboBox = new System.Windows.Forms.ComboBox();
            this.ScpPortLabel = new System.Windows.Forms.Label();
            this.ScpAddressLabel = new System.Windows.Forms.Label();
            this.ScpTitleLabel = new System.Windows.Forms.Label();
            this.ScpPortTextBox = new System.Windows.Forms.TextBox();
            this.ScpAddressTextBox = new System.Windows.Forms.TextBox();
            this.StopStartButton = new System.Windows.Forms.Button();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusStripLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.LogControl = new EK.Capture.Dicom.DicomToolKit.LogControl();
            this.ScuGroupBox = new System.Windows.Forms.GroupBox();
            this.ScuDeleteButton = new System.Windows.Forms.Button();
            this.ScuTitleComboBox = new System.Windows.Forms.ComboBox();
            this.ScuPortLabel = new System.Windows.Forms.Label();
            this.ScuAddressLabel = new System.Windows.Forms.Label();
            this.ScuTitleLabel = new System.Windows.Forms.Label();
            this.ScuPortTextBox = new System.Windows.Forms.TextBox();
            this.ScuAddressTextBox = new System.Windows.Forms.TextBox();
            this.PipeGroupBox.SuspendLayout();
            this.ScpGroupBox.SuspendLayout();
            this.StatusStrip.SuspendLayout();
            this.ScuGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // PipeGroupBox
            // 
            this.PipeGroupBox.Controls.Add(this.LoggingCheckBox);
            this.PipeGroupBox.Controls.Add(this.ScriptLabel);
            this.PipeGroupBox.Controls.Add(this.ScriptComboBox);
            this.PipeGroupBox.Controls.Add(this.PipePortLabel);
            this.PipeGroupBox.Controls.Add(this.PipePortTextBox);
            this.PipeGroupBox.Location = new System.Drawing.Point(34, 121);
            this.PipeGroupBox.Name = "PipeGroupBox";
            this.PipeGroupBox.Size = new System.Drawing.Size(362, 84);
            this.PipeGroupBox.TabIndex = 0;
            this.PipeGroupBox.TabStop = false;
            this.PipeGroupBox.Text = "Pipe";
            // 
            // LoggingCheckBox
            // 
            this.LoggingCheckBox.AutoSize = true;
            this.LoggingCheckBox.Location = new System.Drawing.Point(252, 48);
            this.LoggingCheckBox.Name = "LoggingCheckBox";
            this.LoggingCheckBox.Size = new System.Drawing.Size(100, 17);
            this.LoggingCheckBox.TabIndex = 7;
            this.LoggingCheckBox.Text = "Enable Logging";
            this.LoggingCheckBox.UseVisualStyleBackColor = true;
            // 
            // ScriptLabel
            // 
            this.ScriptLabel.AutoSize = true;
            this.ScriptLabel.Location = new System.Drawing.Point(28, 20);
            this.ScriptLabel.Name = "ScriptLabel";
            this.ScriptLabel.Size = new System.Drawing.Size(34, 13);
            this.ScriptLabel.TabIndex = 6;
            this.ScriptLabel.Text = "Script";
            // 
            // ScriptComboBox
            // 
            this.ScriptComboBox.FormattingEnabled = true;
            this.ScriptComboBox.Location = new System.Drawing.Point(69, 17);
            this.ScriptComboBox.Name = "ScriptComboBox";
            this.ScriptComboBox.Size = new System.Drawing.Size(159, 21);
            this.ScriptComboBox.TabIndex = 5;
            // 
            // PipePortLabel
            // 
            this.PipePortLabel.AutoSize = true;
            this.PipePortLabel.Location = new System.Drawing.Point(37, 53);
            this.PipePortLabel.Name = "PipePortLabel";
            this.PipePortLabel.Size = new System.Drawing.Size(26, 13);
            this.PipePortLabel.TabIndex = 4;
            this.PipePortLabel.Text = "Port";
            // 
            // PipePortTextBox
            // 
            this.PipePortTextBox.Location = new System.Drawing.Point(69, 48);
            this.PipePortTextBox.Name = "PipePortTextBox";
            this.PipePortTextBox.Size = new System.Drawing.Size(159, 20);
            this.PipePortTextBox.TabIndex = 2;
            // 
            // ScpGroupBox
            // 
            this.ScpGroupBox.Controls.Add(this.ScpDeleteButton);
            this.ScpGroupBox.Controls.Add(this.ScpTitleComboBox);
            this.ScpGroupBox.Controls.Add(this.ScpPortLabel);
            this.ScpGroupBox.Controls.Add(this.ScpAddressLabel);
            this.ScpGroupBox.Controls.Add(this.ScpTitleLabel);
            this.ScpGroupBox.Controls.Add(this.ScpPortTextBox);
            this.ScpGroupBox.Controls.Add(this.ScpAddressTextBox);
            this.ScpGroupBox.Location = new System.Drawing.Point(323, 12);
            this.ScpGroupBox.Name = "ScpGroupBox";
            this.ScpGroupBox.Size = new System.Drawing.Size(246, 105);
            this.ScpGroupBox.TabIndex = 5;
            this.ScpGroupBox.TabStop = false;
            this.ScpGroupBox.Text = "SCP";
            // 
            // ScpDeleteButton
            // 
            this.ScpDeleteButton.Location = new System.Drawing.Point(230, 14);
            this.ScpDeleteButton.Name = "ScpDeleteButton";
            this.ScpDeleteButton.Size = new System.Drawing.Size(16, 23);
            this.ScpDeleteButton.TabIndex = 12;
            this.ScpDeleteButton.Text = "X";
            this.ScpDeleteButton.UseVisualStyleBackColor = true;
            this.ScpDeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // ScpTitleComboBox
            // 
            this.ScpTitleComboBox.FormattingEnabled = true;
            this.ScpTitleComboBox.Location = new System.Drawing.Point(69, 16);
            this.ScpTitleComboBox.Name = "ScpTitleComboBox";
            this.ScpTitleComboBox.Size = new System.Drawing.Size(159, 21);
            this.ScpTitleComboBox.TabIndex = 7;
            this.ScpTitleComboBox.SelectedValueChanged += new System.EventHandler(this.TitleComboBox_SelectedValueChanged);
            // 
            // ScpPortLabel
            // 
            this.ScpPortLabel.AutoSize = true;
            this.ScpPortLabel.Location = new System.Drawing.Point(37, 76);
            this.ScpPortLabel.Name = "ScpPortLabel";
            this.ScpPortLabel.Size = new System.Drawing.Size(26, 13);
            this.ScpPortLabel.TabIndex = 4;
            this.ScpPortLabel.Text = "Port";
            // 
            // ScpAddressLabel
            // 
            this.ScpAddressLabel.AutoSize = true;
            this.ScpAddressLabel.Location = new System.Drawing.Point(5, 50);
            this.ScpAddressLabel.Name = "ScpAddressLabel";
            this.ScpAddressLabel.Size = new System.Drawing.Size(58, 13);
            this.ScpAddressLabel.TabIndex = 3;
            this.ScpAddressLabel.Text = "IP Address";
            // 
            // ScpTitleLabel
            // 
            this.ScpTitleLabel.AutoSize = true;
            this.ScpTitleLabel.Location = new System.Drawing.Point(19, 24);
            this.ScpTitleLabel.Name = "ScpTitleLabel";
            this.ScpTitleLabel.Size = new System.Drawing.Size(44, 13);
            this.ScpTitleLabel.TabIndex = 1;
            this.ScpTitleLabel.Text = "AE Title";
            // 
            // ScpPortTextBox
            // 
            this.ScpPortTextBox.Location = new System.Drawing.Point(69, 73);
            this.ScpPortTextBox.Name = "ScpPortTextBox";
            this.ScpPortTextBox.Size = new System.Drawing.Size(159, 20);
            this.ScpPortTextBox.TabIndex = 5;
            // 
            // ScpAddressTextBox
            // 
            this.ScpAddressTextBox.Location = new System.Drawing.Point(69, 47);
            this.ScpAddressTextBox.Name = "ScpAddressTextBox";
            this.ScpAddressTextBox.Size = new System.Drawing.Size(159, 20);
            this.ScpAddressTextBox.TabIndex = 4;
            // 
            // StopStartButton
            // 
            this.StopStartButton.Location = new System.Drawing.Point(445, 141);
            this.StopStartButton.Name = "StopStartButton";
            this.StopStartButton.Size = new System.Drawing.Size(96, 28);
            this.StopStartButton.TabIndex = 1;
            this.StopStartButton.Text = "Start";
            this.StopStartButton.UseVisualStyleBackColor = true;
            this.StopStartButton.Click += new System.EventHandler(this.StopStartButton_Click);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusStripLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 461);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(680, 22);
            this.StatusStrip.SizingGrip = false;
            this.StatusStrip.TabIndex = 6;
            // 
            // StatusStripLabel
            // 
            this.StatusStripLabel.Name = "StatusStripLabel";
            this.StatusStripLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // LogControl
            // 
            this.LogControl.AutoSize = true;
            this.LogControl.Location = new System.Drawing.Point(12, 214);
            this.LogControl.Name = "LogControl";
            this.LogControl.Size = new System.Drawing.Size(668, 244);
            this.LogControl.TabIndex = 7;
            // 
            // ScuGroupBox
            // 
            this.ScuGroupBox.Controls.Add(this.ScuDeleteButton);
            this.ScuGroupBox.Controls.Add(this.ScuTitleComboBox);
            this.ScuGroupBox.Controls.Add(this.ScuPortLabel);
            this.ScuGroupBox.Controls.Add(this.ScuAddressLabel);
            this.ScuGroupBox.Controls.Add(this.ScuTitleLabel);
            this.ScuGroupBox.Controls.Add(this.ScuPortTextBox);
            this.ScuGroupBox.Controls.Add(this.ScuAddressTextBox);
            this.ScuGroupBox.Location = new System.Drawing.Point(12, 12);
            this.ScuGroupBox.Name = "ScuGroupBox";
            this.ScuGroupBox.Size = new System.Drawing.Size(246, 105);
            this.ScuGroupBox.TabIndex = 8;
            this.ScuGroupBox.TabStop = false;
            this.ScuGroupBox.Text = "SCU";
            // 
            // ScuDeleteButton
            // 
            this.ScuDeleteButton.Location = new System.Drawing.Point(230, 14);
            this.ScuDeleteButton.Name = "ScuDeleteButton";
            this.ScuDeleteButton.Size = new System.Drawing.Size(16, 23);
            this.ScuDeleteButton.TabIndex = 12;
            this.ScuDeleteButton.Text = "X";
            this.ScuDeleteButton.UseVisualStyleBackColor = true;
            // 
            // ScuTitleComboBox
            // 
            this.ScuTitleComboBox.FormattingEnabled = true;
            this.ScuTitleComboBox.Location = new System.Drawing.Point(69, 16);
            this.ScuTitleComboBox.Name = "ScuTitleComboBox";
            this.ScuTitleComboBox.Size = new System.Drawing.Size(159, 21);
            this.ScuTitleComboBox.TabIndex = 7;
            this.ScuTitleComboBox.SelectedValueChanged += new System.EventHandler(this.TitleComboBox_SelectedValueChanged);
            // 
            // ScuPortLabel
            // 
            this.ScuPortLabel.AutoSize = true;
            this.ScuPortLabel.Location = new System.Drawing.Point(37, 76);
            this.ScuPortLabel.Name = "ScuPortLabel";
            this.ScuPortLabel.Size = new System.Drawing.Size(26, 13);
            this.ScuPortLabel.TabIndex = 4;
            this.ScuPortLabel.Text = "Port";
            // 
            // ScuAddressLabel
            // 
            this.ScuAddressLabel.AutoSize = true;
            this.ScuAddressLabel.Location = new System.Drawing.Point(5, 50);
            this.ScuAddressLabel.Name = "ScuAddressLabel";
            this.ScuAddressLabel.Size = new System.Drawing.Size(58, 13);
            this.ScuAddressLabel.TabIndex = 3;
            this.ScuAddressLabel.Text = "IP Address";
            // 
            // ScuTitleLabel
            // 
            this.ScuTitleLabel.AutoSize = true;
            this.ScuTitleLabel.Location = new System.Drawing.Point(19, 24);
            this.ScuTitleLabel.Name = "ScuTitleLabel";
            this.ScuTitleLabel.Size = new System.Drawing.Size(44, 13);
            this.ScuTitleLabel.TabIndex = 1;
            this.ScuTitleLabel.Text = "AE Title";
            // 
            // ScuPortTextBox
            // 
            this.ScuPortTextBox.Location = new System.Drawing.Point(69, 73);
            this.ScuPortTextBox.Name = "ScuPortTextBox";
            this.ScuPortTextBox.Size = new System.Drawing.Size(159, 20);
            this.ScuPortTextBox.TabIndex = 5;
            // 
            // ScuAddressTextBox
            // 
            this.ScuAddressTextBox.Location = new System.Drawing.Point(69, 47);
            this.ScuAddressTextBox.Name = "ScuAddressTextBox";
            this.ScuAddressTextBox.Size = new System.Drawing.Size(159, 20);
            this.ScuAddressTextBox.TabIndex = 4;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 483);
            this.Controls.Add(this.ScuGroupBox);
            this.Controls.Add(this.LogControl);
            this.Controls.Add(this.StopStartButton);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.ScpGroupBox);
            this.Controls.Add(this.PipeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Dicom Pipe";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.PipeGroupBox.ResumeLayout(false);
            this.PipeGroupBox.PerformLayout();
            this.ScpGroupBox.ResumeLayout(false);
            this.ScpGroupBox.PerformLayout();
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ScuGroupBox.ResumeLayout(false);
            this.ScuGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox PipeGroupBox;
        private System.Windows.Forms.Label PipePortLabel;
        private System.Windows.Forms.TextBox PipePortTextBox;
        private System.Windows.Forms.GroupBox ScpGroupBox;
        private System.Windows.Forms.Label ScpPortLabel;
        private System.Windows.Forms.Label ScpAddressLabel;
        private System.Windows.Forms.Label ScpTitleLabel;
        private System.Windows.Forms.TextBox ScpPortTextBox;
        private System.Windows.Forms.TextBox ScpAddressTextBox;
        private System.Windows.Forms.Button StopStartButton;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusStripLabel;
        private System.Windows.Forms.Label ScriptLabel;
        private System.Windows.Forms.ComboBox ScriptComboBox;
        private System.Windows.Forms.ComboBox ScpTitleComboBox;
        private System.Windows.Forms.Button ScpDeleteButton;
        private EK.Capture.Dicom.DicomToolKit.LogControl LogControl;
        private System.Windows.Forms.CheckBox LoggingCheckBox;
        private System.Windows.Forms.GroupBox ScuGroupBox;
        private System.Windows.Forms.Button ScuDeleteButton;
        private System.Windows.Forms.ComboBox ScuTitleComboBox;
        private System.Windows.Forms.Label ScuPortLabel;
        private System.Windows.Forms.Label ScuAddressLabel;
        private System.Windows.Forms.Label ScuTitleLabel;
        private System.Windows.Forms.TextBox ScuPortTextBox;
        private System.Windows.Forms.TextBox ScuAddressTextBox;
    }
}

