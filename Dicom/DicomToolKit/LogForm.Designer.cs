namespace EK.Capture.Dicom.DicomToolKit
{
    partial class LogForm
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
            this.components = new System.ComponentModel.Container();
            this.LogContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.errorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.warningToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.infoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.verboseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.LogControl = new EK.Capture.Dicom.DicomToolKit.LogControl();
            this.LogContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // LogContextMenuStrip
            // 
            this.LogContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.errorToolStripMenuItem,
            this.warningToolStripMenuItem,
            this.infoToolStripMenuItem,
            this.verboseToolStripMenuItem});
            this.LogContextMenuStrip.Name = "LogContextMenuStrip";
            this.LogContextMenuStrip.Size = new System.Drawing.Size(115, 92);
            // 
            // errorToolStripMenuItem
            // 
            this.errorToolStripMenuItem.Name = "errorToolStripMenuItem";
            this.errorToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.errorToolStripMenuItem.Text = "Error";
            // 
            // warningToolStripMenuItem
            // 
            this.warningToolStripMenuItem.Name = "warningToolStripMenuItem";
            this.warningToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.warningToolStripMenuItem.Text = "Warning";
            // 
            // infoToolStripMenuItem
            // 
            this.infoToolStripMenuItem.Name = "infoToolStripMenuItem";
            this.infoToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.infoToolStripMenuItem.Text = "Info";
            // 
            // verboseToolStripMenuItem
            // 
            this.verboseToolStripMenuItem.Name = "verboseToolStripMenuItem";
            this.verboseToolStripMenuItem.Size = new System.Drawing.Size(114, 22);
            this.verboseToolStripMenuItem.Text = "Verbose";
            // 
            // LogControl
            // 
            this.LogControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.LogControl.AutoSize = true;
            this.LogControl.Location = new System.Drawing.Point(-1, -1);
            this.LogControl.Name = "LogControl";
            this.LogControl.Size = new System.Drawing.Size(293, 267);
            this.LogControl.TabIndex = 7;
            // 
            // LogForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.LogControl);
            this.Name = "LogForm";
            this.Text = "Log";
            this.SizeChanged += new System.EventHandler(this.Log_SizeChanged);
            this.LogContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip LogContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem errorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem warningToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem infoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem verboseToolStripMenuItem;
        private EK.Capture.Dicom.DicomToolKit.LogControl LogControl;

    }
}