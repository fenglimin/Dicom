namespace DicomEditor
{
    partial class NewTagForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.ResultsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.ShowButton = new System.Windows.Forms.Button();
            this.Clear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(105, 433);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(206, 433);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // ResultsCheckedListBox
            // 
            this.ResultsCheckedListBox.CheckOnClick = true;
            this.ResultsCheckedListBox.FormattingEnabled = true;
            this.ResultsCheckedListBox.Location = new System.Drawing.Point(13, 47);
            this.ResultsCheckedListBox.Name = "ResultsCheckedListBox";
            this.ResultsCheckedListBox.Size = new System.Drawing.Size(366, 349);
            this.ResultsCheckedListBox.TabIndex = 1;
            this.ResultsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ResultsCheckedListBox_ItemCheck);
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Location = new System.Drawing.Point(13, 12);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(366, 20);
            this.FilterTextBox.TabIndex = 0;
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // ShowButton
            // 
            this.ShowButton.Location = new System.Drawing.Point(305, 402);
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.Size = new System.Drawing.Size(75, 23);
            this.ShowButton.TabIndex = 4;
            this.ShowButton.Text = "Show";
            this.ShowButton.UseVisualStyleBackColor = true;
            this.ShowButton.Click += new System.EventHandler(this.ShowButton_Click);
            // 
            // Clear
            // 
            this.Clear.Location = new System.Drawing.Point(224, 402);
            this.Clear.Name = "Clear";
            this.Clear.Size = new System.Drawing.Size(75, 23);
            this.Clear.TabIndex = 5;
            this.Clear.Text = "Clear";
            this.Clear.UseVisualStyleBackColor = true;
            this.Clear.Click += new System.EventHandler(this.Clear_Click);
            // 
            // NewTagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(391, 468);
            this.ControlBox = false;
            this.Controls.Add(this.Clear);
            this.Controls.Add(this.ShowButton);
            this.Controls.Add(this.FilterTextBox);
            this.Controls.Add(this.ResultsCheckedListBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewTagForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add New Tag";
            this.Load += new System.EventHandler(this.NewTagForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckedListBox ResultsCheckedListBox;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Button ShowButton;
        private System.Windows.Forms.Button Clear;
    }
}