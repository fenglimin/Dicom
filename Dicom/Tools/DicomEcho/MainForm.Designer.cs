namespace DicomEcho
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
            this.CallingAETitleTextBox = new System.Windows.Forms.TextBox();
            this.CalledAETitleComboBox = new System.Windows.Forms.ComboBox();
            this.IPAddressTextBox = new System.Windows.Forms.TextBox();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.StatusBar = new System.Windows.Forms.StatusBar();
            this.RunButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.CallingAETitleLabel = new System.Windows.Forms.Label();
            this.CalledAETitleLabel = new System.Windows.Forms.Label();
            this.IPAddressLabel = new System.Windows.Forms.Label();
            this.PortLabel = new System.Windows.Forms.Label();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.AboutButton = new System.Windows.Forms.Button();
            this.HostNameButton = new System.Windows.Forms.Button();
            this.LogControl = new EK.Capture.Dicom.DicomToolKit.LogControl();
            this.SuspendLayout();
            // 
            // CallingAETitleTextBox
            // 
            this.CallingAETitleTextBox.Location = new System.Drawing.Point(97, 12);
            this.CallingAETitleTextBox.Name = "CallingAETitleTextBox";
            this.CallingAETitleTextBox.Size = new System.Drawing.Size(142, 20);
            this.CallingAETitleTextBox.TabIndex = 0;
            // 
            // CalledAETitleComboBox
            // 
            this.CalledAETitleComboBox.FormattingEnabled = true;
            this.CalledAETitleComboBox.Location = new System.Drawing.Point(97, 40);
            this.CalledAETitleComboBox.Name = "CalledAETitleComboBox";
            this.CalledAETitleComboBox.Size = new System.Drawing.Size(142, 21);
            this.CalledAETitleComboBox.Sorted = true;
            this.CalledAETitleComboBox.TabIndex = 1;
            this.CalledAETitleComboBox.SelectedIndexChanged += new System.EventHandler(this.CalledAETitleComboBox_SelectedIndexChanged);
            // 
            // IPAddressTextBox
            // 
            this.IPAddressTextBox.Location = new System.Drawing.Point(314, 41);
            this.IPAddressTextBox.Name = "IPAddressTextBox";
            this.IPAddressTextBox.Size = new System.Drawing.Size(127, 20);
            this.IPAddressTextBox.TabIndex = 2;
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(482, 41);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(51, 20);
            this.PortTextBox.TabIndex = 3;
            // 
            // StatusBar
            // 
            this.StatusBar.Location = new System.Drawing.Point(0, 371);
            this.StatusBar.Name = "StatusBar";
            this.StatusBar.Size = new System.Drawing.Size(687, 22);
            this.StatusBar.TabIndex = 4;
            this.StatusBar.Text = "Enter new values or select from the dropdown and click Run";
            // 
            // RunButton
            // 
            this.RunButton.Location = new System.Drawing.Point(233, 75);
            this.RunButton.Name = "RunButton";
            this.RunButton.Size = new System.Drawing.Size(75, 23);
            this.RunButton.TabIndex = 5;
            this.RunButton.Text = "Run";
            this.RunButton.UseVisualStyleBackColor = true;
            this.RunButton.Click += new System.EventHandler(this.RunButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(314, 75);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 6;
            this.CloseButton.Text = "Close";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // CallingAETitleLabel
            // 
            this.CallingAETitleLabel.AutoSize = true;
            this.CallingAETitleLabel.Location = new System.Drawing.Point(13, 15);
            this.CallingAETitleLabel.Name = "CallingAETitleLabel";
            this.CallingAETitleLabel.Size = new System.Drawing.Size(78, 13);
            this.CallingAETitleLabel.TabIndex = 7;
            this.CallingAETitleLabel.Text = "Calling AE Title";
            // 
            // CalledAETitleLabel
            // 
            this.CalledAETitleLabel.AutoSize = true;
            this.CalledAETitleLabel.Location = new System.Drawing.Point(15, 48);
            this.CalledAETitleLabel.Name = "CalledAETitleLabel";
            this.CalledAETitleLabel.Size = new System.Drawing.Size(76, 13);
            this.CalledAETitleLabel.TabIndex = 8;
            this.CalledAETitleLabel.Text = "Called AE Title";
            // 
            // IPAddressLabel
            // 
            this.IPAddressLabel.AutoSize = true;
            this.IPAddressLabel.Location = new System.Drawing.Point(250, 48);
            this.IPAddressLabel.Name = "IPAddressLabel";
            this.IPAddressLabel.Size = new System.Drawing.Size(58, 13);
            this.IPAddressLabel.TabIndex = 9;
            this.IPAddressLabel.Text = "IP Address";
            // 
            // PortLabel
            // 
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(450, 48);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(26, 13);
            this.PortLabel.TabIndex = 10;
            this.PortLabel.Text = "Port";
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(551, 38);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(89, 23);
            this.DeleteButton.TabIndex = 11;
            this.DeleteButton.Text = "Delete Entry";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // AboutButton
            // 
            this.AboutButton.Location = new System.Drawing.Point(617, 75);
            this.AboutButton.Name = "AboutButton";
            this.AboutButton.Size = new System.Drawing.Size(23, 23);
            this.AboutButton.TabIndex = 12;
            this.AboutButton.Text = "?";
            this.AboutButton.UseVisualStyleBackColor = true;
            this.AboutButton.Click += new System.EventHandler(this.HelpButton_Click);
            // 
            // HostNameButton
            // 
            this.HostNameButton.Location = new System.Drawing.Point(243, 10);
            this.HostNameButton.Name = "HostNameButton";
            this.HostNameButton.Size = new System.Drawing.Size(130, 23);
            this.HostNameButton.TabIndex = 13;
            this.HostNameButton.Text = "Set To Computer Name";
            this.HostNameButton.UseVisualStyleBackColor = true;
            this.HostNameButton.Click += new System.EventHandler(this.HostNameButton_Click);
            // 
            // LogControl
            // 
            this.LogControl.Location = new System.Drawing.Point(12, 104);
            this.LogControl.Name = "LogControl";
            this.LogControl.Size = new System.Drawing.Size(663, 260);
            this.LogControl.TabIndex = 14;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 393);
            this.Controls.Add(this.LogControl);
            this.Controls.Add(this.HostNameButton);
            this.Controls.Add(this.AboutButton);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.PortLabel);
            this.Controls.Add(this.IPAddressLabel);
            this.Controls.Add(this.CalledAETitleLabel);
            this.Controls.Add(this.CallingAETitleLabel);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.RunButton);
            this.Controls.Add(this.StatusBar);
            this.Controls.Add(this.PortTextBox);
            this.Controls.Add(this.IPAddressTextBox);
            this.Controls.Add(this.CalledAETitleComboBox);
            this.Controls.Add(this.CallingAETitleTextBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "MainForm";
            this.Text = "Dicom Echo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox CallingAETitleTextBox;
        private System.Windows.Forms.ComboBox CalledAETitleComboBox;
        private System.Windows.Forms.TextBox IPAddressTextBox;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.StatusBar StatusBar;
        private System.Windows.Forms.Button RunButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label CallingAETitleLabel;
        private System.Windows.Forms.Label CalledAETitleLabel;
        private System.Windows.Forms.Label IPAddressLabel;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Button AboutButton;
        private System.Windows.Forms.Button HostNameButton;
        private EK.Capture.Dicom.DicomToolKit.LogControl LogControl;
    }
}

