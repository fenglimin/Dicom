namespace EK.Capture.Dicom.DicomToolKit
{
    partial class LogControl
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
            this.components = new System.ComponentModel.Container();
            this.ScrollBar = new System.Windows.Forms.VScrollBar();
            this.LogTextBox = new System.Windows.Forms.TextBox();
            this.MenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ClearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // ScrollBar
            // 
            this.ScrollBar.Dock = System.Windows.Forms.DockStyle.Right;
            this.ScrollBar.Location = new System.Drawing.Point(517, 0);
            this.ScrollBar.Name = "ScrollBar";
            this.ScrollBar.Size = new System.Drawing.Size(17, 154);
            this.ScrollBar.TabIndex = 0;
            this.ScrollBar.Scroll += new System.Windows.Forms.ScrollEventHandler(this.ScrollBar_Scroll);
            // 
            // LogTextBox
            // 
            this.LogTextBox.ContextMenuStrip = this.MenuStrip;
            this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LogTextBox.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LogTextBox.Location = new System.Drawing.Point(0, 0);
            this.LogTextBox.Multiline = true;
            this.LogTextBox.Name = "LogTextBox";
            this.LogTextBox.Size = new System.Drawing.Size(517, 154);
            this.LogTextBox.TabIndex = 1;
            this.LogTextBox.WordWrap = false;
            this.LogTextBox.SizeChanged += new System.EventHandler(this.LogTextBox_SizeChanged);
            // 
            // MenuStrip
            // 
            this.MenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ClearToolStripMenuItem});
            this.MenuStrip.Name = "ContextMenuStrip";
            this.MenuStrip.Size = new System.Drawing.Size(100, 26);
            // 
            // ClearToolStripMenuItem
            // 
            this.ClearToolStripMenuItem.Name = "ClearToolStripMenuItem";
            this.ClearToolStripMenuItem.Size = new System.Drawing.Size(99, 22);
            this.ClearToolStripMenuItem.Text = "Clear";
            this.ClearToolStripMenuItem.Click += new System.EventHandler(this.ClearToolStripMenuItem_Click);
            // 
            // LogControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LogTextBox);
            this.Controls.Add(this.ScrollBar);
            this.Name = "LogControl";
            this.Size = new System.Drawing.Size(534, 154);
            this.Load += new System.EventHandler(this.LogControl_Load);
            this.MenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.VScrollBar ScrollBar;
        private System.Windows.Forms.TextBox LogTextBox;
        private System.Windows.Forms.ContextMenuStrip MenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ClearToolStripMenuItem;
    }
}
