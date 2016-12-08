namespace DicomEditor
{
    partial class FindForm
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
            this.UpRadioButton = new System.Windows.Forms.RadioButton();
            this.DownRadioButton = new System.Windows.Forms.RadioButton();
            this.FindTextBox = new System.Windows.Forms.TextBox();
            this.FindButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.FindLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // UpRadioButton
            // 
            this.UpRadioButton.AutoSize = true;
            this.UpRadioButton.Location = new System.Drawing.Point(123, 40);
            this.UpRadioButton.Name = "UpRadioButton";
            this.UpRadioButton.Size = new System.Drawing.Size(39, 17);
            this.UpRadioButton.TabIndex = 1;
            this.UpRadioButton.Text = "Up";
            this.UpRadioButton.UseVisualStyleBackColor = true;
            // 
            // DownRadioButton
            // 
            this.DownRadioButton.AutoSize = true;
            this.DownRadioButton.Checked = true;
            this.DownRadioButton.Location = new System.Drawing.Point(190, 40);
            this.DownRadioButton.Name = "DownRadioButton";
            this.DownRadioButton.Size = new System.Drawing.Size(53, 17);
            this.DownRadioButton.TabIndex = 2;
            this.DownRadioButton.TabStop = true;
            this.DownRadioButton.Text = "Down";
            this.DownRadioButton.UseVisualStyleBackColor = true;
            // 
            // FindTextBox
            // 
            this.FindTextBox.Location = new System.Drawing.Point(76, 14);
            this.FindTextBox.Name = "FindTextBox";
            this.FindTextBox.Size = new System.Drawing.Size(245, 20);
            this.FindTextBox.TabIndex = 0;
            // 
            // FindButton
            // 
            this.FindButton.Location = new System.Drawing.Point(87, 63);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(75, 23);
            this.FindButton.TabIndex = 3;
            this.FindButton.Text = "Find";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(168, 63);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // FindLabel
            // 
            this.FindLabel.AutoSize = true;
            this.FindLabel.Location = new System.Drawing.Point(14, 21);
            this.FindLabel.Name = "FindLabel";
            this.FindLabel.Size = new System.Drawing.Size(56, 13);
            this.FindLabel.TabIndex = 5;
            this.FindLabel.Text = "Find what:";
            // 
            // FindForm
            // 
            this.AcceptButton = this.FindButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(333, 94);
            this.ControlBox = false;
            this.Controls.Add(this.FindLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.FindTextBox);
            this.Controls.Add(this.DownRadioButton);
            this.Controls.Add(this.UpRadioButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "FindForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Find";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.FindForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton UpRadioButton;
        private System.Windows.Forms.RadioButton DownRadioButton;
        private System.Windows.Forms.TextBox FindTextBox;
        private System.Windows.Forms.Button FindButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label FindLabel;
    }
}