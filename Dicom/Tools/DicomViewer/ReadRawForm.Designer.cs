namespace DicomViewer
{
    partial class ReadRawForm
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
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ColumnsLabel = new System.Windows.Forms.Label();
            this.RowLabel = new System.Windows.Forms.Label();
            this.ColumnsTextBox = new System.Windows.Forms.TextBox();
            this.RowsTextBox = new System.Windows.Forms.TextBox();
            this.NameLabel = new System.Windows.Forms.Label();
            this.BPPComboBox = new System.Windows.Forms.ComboBox();
            this.BPPLabel = new System.Windows.Forms.Label();
            this.Monochrome1CheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(24, 169);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton.Location = new System.Drawing.Point(129, 169);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 1;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // ColumnsLabel
            // 
            this.ColumnsLabel.AutoSize = true;
            this.ColumnsLabel.Location = new System.Drawing.Point(41, 52);
            this.ColumnsLabel.Name = "ColumnsLabel";
            this.ColumnsLabel.Size = new System.Drawing.Size(47, 13);
            this.ColumnsLabel.TabIndex = 2;
            this.ColumnsLabel.Text = "Columns";
            // 
            // RowLabel
            // 
            this.RowLabel.AutoSize = true;
            this.RowLabel.Location = new System.Drawing.Point(54, 78);
            this.RowLabel.Name = "RowLabel";
            this.RowLabel.Size = new System.Drawing.Size(34, 13);
            this.RowLabel.TabIndex = 3;
            this.RowLabel.Text = "Rows";
            // 
            // ColumnsTextBox
            // 
            this.ColumnsTextBox.Location = new System.Drawing.Point(94, 49);
            this.ColumnsTextBox.Name = "ColumnsTextBox";
            this.ColumnsTextBox.Size = new System.Drawing.Size(100, 20);
            this.ColumnsTextBox.TabIndex = 4;
            this.ColumnsTextBox.TextChanged += new System.EventHandler(this.Anywhere_TextChanged);
            // 
            // RowsTextBox
            // 
            this.RowsTextBox.Location = new System.Drawing.Point(94, 75);
            this.RowsTextBox.Name = "RowsTextBox";
            this.RowsTextBox.Size = new System.Drawing.Size(100, 20);
            this.RowsTextBox.TabIndex = 5;
            this.RowsTextBox.TextChanged += new System.EventHandler(this.Anywhere_TextChanged);
            // 
            // NameLabel
            // 
            this.NameLabel.Location = new System.Drawing.Point(13, 9);
            this.NameLabel.Name = "NameLabel";
            this.NameLabel.Size = new System.Drawing.Size(203, 27);
            this.NameLabel.TabIndex = 6;
            // 
            // BPPComboBox
            // 
            this.BPPComboBox.FormattingEnabled = true;
            this.BPPComboBox.Items.AddRange(new object[] {
            "8",
            "12",
            "14",
            "16"});
            this.BPPComboBox.Location = new System.Drawing.Point(94, 101);
            this.BPPComboBox.Name = "BPPComboBox";
            this.BPPComboBox.Size = new System.Drawing.Size(100, 21);
            this.BPPComboBox.TabIndex = 7;
            // 
            // BPPLabel
            // 
            this.BPPLabel.AutoSize = true;
            this.BPPLabel.Location = new System.Drawing.Point(54, 104);
            this.BPPLabel.Name = "BPPLabel";
            this.BPPLabel.Size = new System.Drawing.Size(24, 13);
            this.BPPLabel.TabIndex = 8;
            this.BPPLabel.Text = "Bits";
            // 
            // Monochrome1CheckBox
            // 
            this.Monochrome1CheckBox.AutoSize = true;
            this.Monochrome1CheckBox.Checked = true;
            this.Monochrome1CheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Monochrome1CheckBox.Location = new System.Drawing.Point(18, 128);
            this.Monochrome1CheckBox.Name = "Monochrome1CheckBox";
            this.Monochrome1CheckBox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.Monochrome1CheckBox.Size = new System.Drawing.Size(94, 17);
            this.Monochrome1CheckBox.TabIndex = 9;
            this.Monochrome1CheckBox.Text = "Monochrome1";
            this.Monochrome1CheckBox.UseVisualStyleBackColor = true;
            // 
            // ReadRawForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(228, 208);
            this.ControlBox = false;
            this.Controls.Add(this.Monochrome1CheckBox);
            this.Controls.Add(this.BPPLabel);
            this.Controls.Add(this.BPPComboBox);
            this.Controls.Add(this.NameLabel);
            this.Controls.Add(this.RowsTextBox);
            this.Controls.Add(this.ColumnsTextBox);
            this.Controls.Add(this.RowLabel);
            this.Controls.Add(this.ColumnsLabel);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Name = "ReadRawForm";
            this.Text = "Open Raw File";
            this.Load += new System.EventHandler(this.ReadRawForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private new System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Label ColumnsLabel;
        private System.Windows.Forms.Label RowLabel;
        private System.Windows.Forms.TextBox ColumnsTextBox;
        private System.Windows.Forms.TextBox RowsTextBox;
        private System.Windows.Forms.Label NameLabel;
        private System.Windows.Forms.ComboBox BPPComboBox;
        private System.Windows.Forms.Label BPPLabel;
        private System.Windows.Forms.CheckBox Monochrome1CheckBox;
    }
}