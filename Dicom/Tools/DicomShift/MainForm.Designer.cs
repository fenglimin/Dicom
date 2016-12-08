namespace DicomShift
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
            this.RadioButton13 = new System.Windows.Forms.RadioButton();
            this.RadioButton14 = new System.Windows.Forms.RadioButton();
            this.RadioButton15 = new System.Windows.Forms.RadioButton();
            this.RadioButton16 = new System.Windows.Forms.RadioButton();
            this.ShiftButton = new System.Windows.Forms.Button();
            this.AddPixelValuesCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // RadioButton13
            // 
            this.RadioButton13.AutoSize = true;
            this.RadioButton13.Location = new System.Drawing.Point(45, 34);
            this.RadioButton13.Name = "RadioButton13";
            this.RadioButton13.Size = new System.Drawing.Size(37, 17);
            this.RadioButton13.TabIndex = 0;
            this.RadioButton13.Tag = "13";
            this.RadioButton13.Text = "13";
            this.RadioButton13.UseVisualStyleBackColor = true;
            this.RadioButton13.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // RadioButton14
            // 
            this.RadioButton14.AutoSize = true;
            this.RadioButton14.Location = new System.Drawing.Point(88, 34);
            this.RadioButton14.Name = "RadioButton14";
            this.RadioButton14.Size = new System.Drawing.Size(37, 17);
            this.RadioButton14.TabIndex = 1;
            this.RadioButton14.Tag = "14";
            this.RadioButton14.Text = "14";
            this.RadioButton14.UseVisualStyleBackColor = true;
            this.RadioButton14.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // RadioButton15
            // 
            this.RadioButton15.AutoSize = true;
            this.RadioButton15.Location = new System.Drawing.Point(131, 34);
            this.RadioButton15.Name = "RadioButton15";
            this.RadioButton15.Size = new System.Drawing.Size(37, 17);
            this.RadioButton15.TabIndex = 2;
            this.RadioButton15.Tag = "15";
            this.RadioButton15.Text = "15";
            this.RadioButton15.UseVisualStyleBackColor = true;
            this.RadioButton15.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // RadioButton16
            // 
            this.RadioButton16.AutoSize = true;
            this.RadioButton16.Location = new System.Drawing.Point(174, 34);
            this.RadioButton16.Name = "RadioButton16";
            this.RadioButton16.Size = new System.Drawing.Size(37, 17);
            this.RadioButton16.TabIndex = 3;
            this.RadioButton16.Tag = "16";
            this.RadioButton16.Text = "16";
            this.RadioButton16.UseVisualStyleBackColor = true;
            this.RadioButton16.CheckedChanged += new System.EventHandler(this.RadioButton_CheckedChanged);
            // 
            // ShiftButton
            // 
            this.ShiftButton.Location = new System.Drawing.Point(79, 99);
            this.ShiftButton.Name = "ShiftButton";
            this.ShiftButton.Size = new System.Drawing.Size(75, 23);
            this.ShiftButton.TabIndex = 4;
            this.ShiftButton.Text = "Shift";
            this.ShiftButton.UseVisualStyleBackColor = true;
            this.ShiftButton.Click += new System.EventHandler(this.ShiftButton_Click);
            // 
            // AddPixelValuesCheckBox
            // 
            this.AddPixelValuesCheckBox.AutoSize = true;
            this.AddPixelValuesCheckBox.Location = new System.Drawing.Point(74, 66);
            this.AddPixelValuesCheckBox.Name = "AddPixelValuesCheckBox";
            this.AddPixelValuesCheckBox.Size = new System.Drawing.Size(105, 17);
            this.AddPixelValuesCheckBox.TabIndex = 5;
            this.AddPixelValuesCheckBox.Text = "Add Pixel Values";
            this.AddPixelValuesCheckBox.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(257, 150);
            this.Controls.Add(this.AddPixelValuesCheckBox);
            this.Controls.Add(this.ShiftButton);
            this.Controls.Add(this.RadioButton16);
            this.Controls.Add(this.RadioButton15);
            this.Controls.Add(this.RadioButton14);
            this.Controls.Add(this.RadioButton13);
            this.Name = "MainForm";
            this.Text = "Dicom Shift";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton RadioButton13;
        private System.Windows.Forms.RadioButton RadioButton14;
        private System.Windows.Forms.RadioButton RadioButton15;
        private System.Windows.Forms.RadioButton RadioButton16;
        private System.Windows.Forms.Button ShiftButton;
        private System.Windows.Forms.CheckBox AddPixelValuesCheckBox;

    }
}

