namespace DicomExplorer
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
            this.button1 = new System.Windows.Forms.Button();
            this.ResultsCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.FilterTextBox = new System.Windows.Forms.TextBox();
            this.ShowButton = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.MoveUpButton = new System.Windows.Forms.Button();
            this.MoveDownButton = new System.Windows.Forms.Button();
            this.AddButtonAddButton = new System.Windows.Forms.Button();
            this.RemoveButton = new System.Windows.Forms.Button();
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
            this.ResultsCheckedListBox.Location = new System.Drawing.Point(13, 92);
            this.ResultsCheckedListBox.Name = "ResultsCheckedListBox";
            this.ResultsCheckedListBox.Size = new System.Drawing.Size(323, 304);
            this.ResultsCheckedListBox.TabIndex = 1;
            this.ResultsCheckedListBox.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ResultsCheckedListBox_ItemCheck);
            // 
            // FilterTextBox
            // 
            this.FilterTextBox.Location = new System.Drawing.Point(12, 57);
            this.FilterTextBox.Name = "FilterTextBox";
            this.FilterTextBox.Size = new System.Drawing.Size(324, 20);
            this.FilterTextBox.TabIndex = 0;
            this.FilterTextBox.TextChanged += new System.EventHandler(this.FilterTextBox_TextChanged);
            // 
            // ShowButton
            // 
            this.ShowButton.Location = new System.Drawing.Point(261, 402);
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.Size = new System.Drawing.Size(75, 23);
            this.ShowButton.TabIndex = 4;
            this.ShowButton.Text = "Show";
            this.ShowButton.UseVisualStyleBackColor = true;
            this.ShowButton.Click += new System.EventHandler(this.ShowButton_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(13, 13);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(187, 21);
            this.comboBox1.TabIndex = 6;
            // 
            // MoveUpButton
            // 
            this.MoveUpButton.Location = new System.Drawing.Point(343, 146);
            this.MoveUpButton.Name = "MoveUpButton";
            this.MoveUpButton.Size = new System.Drawing.Size(43, 23);
            this.MoveUpButton.TabIndex = 7;
            this.MoveUpButton.Text = "Up";
            this.MoveUpButton.UseVisualStyleBackColor = true;
            // 
            // MoveDownButton
            // 
            this.MoveDownButton.Location = new System.Drawing.Point(342, 175);
            this.MoveDownButton.Name = "MoveDownButton";
            this.MoveDownButton.Size = new System.Drawing.Size(44, 23);
            this.MoveDownButton.TabIndex = 8;
            this.MoveDownButton.Text = "Down";
            this.MoveDownButton.UseVisualStyleBackColor = true;
            // 
            // AddButtonAddButton
            // 
            this.AddButtonAddButton.Location = new System.Drawing.Point(206, 13);
            this.AddButtonAddButton.Name = "AddButtonAddButton";
            this.AddButtonAddButton.Size = new System.Drawing.Size(75, 23);
            this.AddButtonAddButton.TabIndex = 9;
            this.AddButtonAddButton.Text = "Add";
            this.AddButtonAddButton.UseVisualStyleBackColor = true;
            // 
            // RemoveButton
            // 
            this.RemoveButton.Location = new System.Drawing.Point(287, 13);
            this.RemoveButton.Name = "RemoveButton";
            this.RemoveButton.Size = new System.Drawing.Size(75, 23);
            this.RemoveButton.TabIndex = 10;
            this.RemoveButton.Text = "Remove";
            this.RemoveButton.UseVisualStyleBackColor = true;
            // 
            // TagForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(391, 468);
            this.ControlBox = false;
            this.Controls.Add(this.RemoveButton);
            this.Controls.Add(this.AddButtonAddButton);
            this.Controls.Add(this.MoveDownButton);
            this.Controls.Add(this.MoveUpButton);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.ShowButton);
            this.Controls.Add(this.FilterTextBox);
            this.Controls.Add(this.ResultsCheckedListBox);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TagForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add New Tag";
            this.Load += new System.EventHandler(this.TagForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckedListBox ResultsCheckedListBox;
        private System.Windows.Forms.TextBox FilterTextBox;
        private System.Windows.Forms.Button ShowButton;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button MoveUpButton;
        private System.Windows.Forms.Button MoveDownButton;
        private System.Windows.Forms.Button AddButtonAddButton;
        private System.Windows.Forms.Button RemoveButton;
    }
}