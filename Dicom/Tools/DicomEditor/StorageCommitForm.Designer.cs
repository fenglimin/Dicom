namespace DicomEditor
{
    partial class StorageCommitForm
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
            this.HostComboBox = new System.Windows.Forms.ComboBox();
            this.HostLabel = new System.Windows.Forms.Label();
            this.SendButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ClearButton = new System.Windows.Forms.Button();
            this.MessageLabel = new System.Windows.Forms.Label();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.SuccessCheckBox = new System.Windows.Forms.CheckBox();
            this.ErrorCheckBox = new System.Windows.Forms.CheckBox();
            this.StatusGroupBox = new System.Windows.Forms.GroupBox();
            this.StatusStrip.SuspendLayout();
            this.StatusGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // HostComboBox
            // 
            this.HostComboBox.FormattingEnabled = true;
            this.HostComboBox.Location = new System.Drawing.Point(55, 23);
            this.HostComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.HostComboBox.Name = "HostComboBox";
            this.HostComboBox.Size = new System.Drawing.Size(194, 21);
            this.HostComboBox.TabIndex = 1;
            // 
            // HostLabel
            // 
            this.HostLabel.AutoSize = true;
            this.HostLabel.Location = new System.Drawing.Point(9, 25);
            this.HostLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.HostLabel.Name = "HostLabel";
            this.HostLabel.Size = new System.Drawing.Size(29, 13);
            this.HostLabel.TabIndex = 2;
            this.HostLabel.Text = "Host";
            // 
            // SendButton
            // 
            this.SendButton.Location = new System.Drawing.Point(12, 66);
            this.SendButton.Margin = new System.Windows.Forms.Padding(2);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(56, 22);
            this.SendButton.TabIndex = 5;
            this.SendButton.Text = "Send";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.AllowDrop = true;
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(203, 58);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(2);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(56, 22);
            this.CancelButton.TabIndex = 6;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(143, 58);
            this.ClearButton.Margin = new System.Windows.Forms.Padding(2);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(56, 22);
            this.ClearButton.TabIndex = 7;
            this.ClearButton.Text = "Clear";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // MessageLabel
            // 
            this.MessageLabel.AutoSize = true;
            this.MessageLabel.Location = new System.Drawing.Point(52, 66);
            this.MessageLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.MessageLabel.Name = "MessageLabel";
            this.MessageLabel.Size = new System.Drawing.Size(0, 13);
            this.MessageLabel.TabIndex = 8;
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 142);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.StatusStrip.Size = new System.Drawing.Size(270, 22);
            this.StatusStrip.TabIndex = 9;
            this.StatusStrip.Text = "statusStrip1";
            // 
            // StatusLabel
            // 
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // SuccessCheckBox
            // 
            this.SuccessCheckBox.AutoSize = true;
            this.SuccessCheckBox.Checked = true;
            this.SuccessCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SuccessCheckBox.Location = new System.Drawing.Point(9, 17);
            this.SuccessCheckBox.Name = "SuccessCheckBox";
            this.SuccessCheckBox.Size = new System.Drawing.Size(67, 17);
            this.SuccessCheckBox.TabIndex = 10;
            this.SuccessCheckBox.Text = "Success";
            this.SuccessCheckBox.UseVisualStyleBackColor = true;
            // 
            // ErrorCheckBox
            // 
            this.ErrorCheckBox.AutoSize = true;
            this.ErrorCheckBox.Location = new System.Drawing.Point(82, 17);
            this.ErrorCheckBox.Name = "ErrorCheckBox";
            this.ErrorCheckBox.Size = new System.Drawing.Size(48, 17);
            this.ErrorCheckBox.TabIndex = 11;
            this.ErrorCheckBox.Text = "Error";
            this.ErrorCheckBox.UseVisualStyleBackColor = true;
            // 
            // StatusGroupBox
            // 
            this.StatusGroupBox.Controls.Add(this.ErrorCheckBox);
            this.StatusGroupBox.Controls.Add(this.SuccessCheckBox);
            this.StatusGroupBox.Location = new System.Drawing.Point(12, 93);
            this.StatusGroupBox.Name = "StatusGroupBox";
            this.StatusGroupBox.Size = new System.Drawing.Size(137, 40);
            this.StatusGroupBox.TabIndex = 12;
            this.StatusGroupBox.TabStop = false;
            this.StatusGroupBox.Text = "Status";
            // 
            // StorageCommitForm
            // 
            this.AcceptButton = this.CancelButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(270, 164);
            this.ControlBox = false;
            this.Controls.Add(this.StatusGroupBox);
            this.Controls.Add(this.StatusStrip);
            this.Controls.Add(this.MessageLabel);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.HostLabel);
            this.Controls.Add(this.HostComboBox);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StorageCommitForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Storage Commit";
            this.Load += new System.EventHandler(this.StorageCommitForm_Load);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.StatusGroupBox.ResumeLayout(false);
            this.StatusGroupBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox HostComboBox;
        private System.Windows.Forms.Label HostLabel;
        private System.Windows.Forms.Button SendButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.Label MessageLabel;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
        private System.Windows.Forms.CheckBox SuccessCheckBox;
        private System.Windows.Forms.CheckBox ErrorCheckBox;
        private System.Windows.Forms.GroupBox StatusGroupBox;

    }
}