namespace DicomEditor
{
    partial class HexControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ScrollBar = new System.Windows.Forms.VScrollBar();
            this.TextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // ScrollBar
            // 
            this.ScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.ScrollBar.Location = new System.Drawing.Point(558, 0);
            this.ScrollBar.Name = "ScrollBar";
            this.ScrollBar.Size = new System.Drawing.Size(17, 125);
            this.ScrollBar.TabIndex = 0;
            this.ScrollBar.ValueChanged += new System.EventHandler(this.ScrollBar_ValueChanged);
            // 
            // TextBox
            // 
            this.TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TextBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBox.Location = new System.Drawing.Point(0, 0);
            this.TextBox.Margin = new System.Windows.Forms.Padding(0);
            this.TextBox.Multiline = true;
            this.TextBox.Name = "TextBox";
            this.TextBox.Size = new System.Drawing.Size(558, 122);
            this.TextBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.TextBox_OnMouseWheel);
            this.TextBox.TabIndex = 1;
            // 
            // HexControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.TextBox);
            this.Controls.Add(this.ScrollBar);
            this.Name = "HexControl";
            this.Size = new System.Drawing.Size(575, 125);
            this.VisibleChanged += new System.EventHandler(this.HexControl_VisibleChanged);
            this.MouseLeave += new System.EventHandler(this.HexControl_Leave);
            this.Leave += new System.EventHandler(this.HexControl_Leave);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.VScrollBar ScrollBar;
        private System.Windows.Forms.TextBox TextBox;
    }
}
