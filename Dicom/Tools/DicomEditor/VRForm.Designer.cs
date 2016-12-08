namespace DicomEditor
{
    partial class VRForm
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
            this.button1 = new System.Windows.Forms.Button();
            this.OKButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.VRComboBox = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.Location = new System.Drawing.Point(126, 50);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(24, 50);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(22, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "VR";
            // 
            // VRComboBox
            // 
            this.VRComboBox.FormattingEnabled = true;
            this.VRComboBox.Items.AddRange(new object[] {
            "AE, Application Entity",
            "AS, Age String",
            "AT, Attribute Tag",
            "CS, Code String",
            "DA, Date",
            "DS, Decimal String",
            "DT, Date Time",
            "FL, Floating Point Single",
            "FD, Floating Point Double",
            "IS, Integer String",
            "LO, Long String",
            "LT, Long Text",
            "OB, Other Byte String",
            "OF, Other Float String",
            "OW, Other Word String",
            "PN, Person Name",
            "SH, Short String",
            "SL, Signed Long",
            "SQ, Sequence of Items",
            "SS, Signed Short",
            "ST, Short Text",
            "TM, Time",
            "UI, Unique Identifier (UID)",
            "UL, Unsigned Long",
            "UN, Unknown",
            "US, Unsigned Short",
            "UT, Unlimited Text"});
            this.VRComboBox.Location = new System.Drawing.Point(40, 16);
            this.VRComboBox.Name = "VRComboBox";
            this.VRComboBox.Size = new System.Drawing.Size(161, 21);
            this.VRComboBox.TabIndex = 5;
            // 
            // VRForm
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button1;
            this.ClientSize = new System.Drawing.Size(213, 85);
            this.ControlBox = false;
            this.Controls.Add(this.VRComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.OKButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MinimizeBox = false;
            this.Name = "VRForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Value Representation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox VRComboBox;
    }
}