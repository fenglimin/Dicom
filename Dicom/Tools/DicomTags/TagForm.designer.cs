namespace DicomTags
{
    partial class TagForm
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
            this.ResultsListBox = new System.Windows.Forms.ListBox();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.TagTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(167, 403);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 0;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // ResultsListBox
            // 
            this.ResultsListBox.FormattingEnabled = true;
            this.ResultsListBox.Location = new System.Drawing.Point(13, 47);
            this.ResultsListBox.Name = "ResultsListBox";
            this.ResultsListBox.Size = new System.Drawing.Size(384, 290);
            this.ResultsListBox.TabIndex = 4;
            this.ResultsListBox.SelectedIndexChanged += new System.EventHandler(this.ResultsListBox_SelectedIndexChanged);
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Location = new System.Drawing.Point(13, 12);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(384, 20);
            this.FilterTextBox.TabIndex = 5;
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // TagTextBox
            // 
            this.TagTextBox.AcceptsReturn = true;
            this.TagTextBox.Location = new System.Drawing.Point(15, 345);
            this.TagTextBox.Multiline = true;
            this.TagTextBox.Name = "TagTextBox";
            this.TagTextBox.ReadOnly = true;
            this.TagTextBox.Size = new System.Drawing.Size(382, 48);
            this.TagTextBox.TabIndex = 6;
            // 
            // TagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(409, 438);
            this.ControlBox = false;
            this.Controls.Add(this.TagTextBox);
            this.Controls.Add(this.FilterTextBox);
            this.Controls.Add(this.ResultsListBox);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagForm";
            this.Text = "Part 6 : Data Dictionary";
            this.Load += new System.EventHandler(this.TagForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.ListBox ResultsListBox;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.TextBox TagTextBox;
    }
}