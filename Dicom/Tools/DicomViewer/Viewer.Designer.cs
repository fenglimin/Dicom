namespace DicomViewer
{
    partial class Viewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.components = new System.ComponentModel.Container();
			this.PictureBox = new System.Windows.Forms.PictureBox();
			this.ViewerContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.InvertToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RotateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.FlipToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CropToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.HistogramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ViewOverlaysToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).BeginInit();
			this.ViewerContextMenuStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// PictureBox
			// 
			this.PictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.PictureBox.ContextMenuStrip = this.ViewerContextMenuStrip;
			this.PictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PictureBox.Location = new System.Drawing.Point(0, 0);
			this.PictureBox.Name = "PictureBox";
			this.PictureBox.Size = new System.Drawing.Size(1035, 930);
			this.PictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
			this.PictureBox.TabIndex = 0;
			this.PictureBox.TabStop = false;
			this.PictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseDown);
			this.PictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseMove);
			this.PictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBox_MouseUp);
			// 
			// ViewerContextMenuStrip
			// 
			this.ViewerContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.InvertToolStripMenuItem,
            this.RotateToolStripMenuItem,
            this.FlipToolStripMenuItem,
            this.CropToolStripMenuItem,
            this.toolStripSeparator1,
            this.HistogramToolStripMenuItem});
			this.ViewerContextMenuStrip.Name = "ViewerContextMenuStrip";
			this.ViewerContextMenuStrip.Size = new System.Drawing.Size(131, 120);
			this.ViewerContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ViewerContextMenuStrip_Opening);
			// 
			// InvertToolStripMenuItem
			// 
			this.InvertToolStripMenuItem.Name = "InvertToolStripMenuItem";
			this.InvertToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.InvertToolStripMenuItem.Text = "&Invert";
			this.InvertToolStripMenuItem.Click += new System.EventHandler(this.InvertToolStripMenuItem_Click);
			// 
			// RotateToolStripMenuItem
			// 
			this.RotateToolStripMenuItem.Name = "RotateToolStripMenuItem";
			this.RotateToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.RotateToolStripMenuItem.Text = "&Rotate";
			this.RotateToolStripMenuItem.Click += new System.EventHandler(this.RotateToolStripMenuItem_Click);
			// 
			// FlipToolStripMenuItem
			// 
			this.FlipToolStripMenuItem.Name = "FlipToolStripMenuItem";
			this.FlipToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.FlipToolStripMenuItem.Text = "&Flip";
			this.FlipToolStripMenuItem.Click += new System.EventHandler(this.FlipToolStripMenuItem_Click);
			// 
			// CropToolStripMenuItem
			// 
			this.CropToolStripMenuItem.Name = "CropToolStripMenuItem";
			this.CropToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.CropToolStripMenuItem.Text = "&Crop";
			this.CropToolStripMenuItem.Click += new System.EventHandler(this.CropToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this.toolStripSeparator1.Name = "toolStripSeparator1";
			this.toolStripSeparator1.Size = new System.Drawing.Size(127, 6);
			// 
			// HistogramToolStripMenuItem
			// 
			this.HistogramToolStripMenuItem.Name = "HistogramToolStripMenuItem";
			this.HistogramToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
			this.HistogramToolStripMenuItem.Text = "&Histogram";
			this.HistogramToolStripMenuItem.Click += new System.EventHandler(this.HistogramToolStripMenuItem_Click);
			// 
			// ViewOverlaysToolStripMenuItem
			// 
			this.ViewOverlaysToolStripMenuItem.Checked = true;
			this.ViewOverlaysToolStripMenuItem.CheckOnClick = true;
			this.ViewOverlaysToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.ViewOverlaysToolStripMenuItem.Name = "ViewOverlaysToolStripMenuItem";
			this.ViewOverlaysToolStripMenuItem.Size = new System.Drawing.Size(147, 22);
			this.ViewOverlaysToolStripMenuItem.Text = "View &Overlays";
			this.ViewOverlaysToolStripMenuItem.Click += new System.EventHandler(this.ViewOverlaysToolStripMenuItem_Click);
			// 
			// Viewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1035, 930);
			this.Controls.Add(this.PictureBox);
			this.Name = "Viewer";
			this.Text = "Image";
			this.Activated += new System.EventHandler(this.Viewer_Activated);
			this.Load += new System.EventHandler(this.Viewer_Load);
			this.Move += new System.EventHandler(this.Viewer_Move);
			this.Resize += new System.EventHandler(this.Viewer_Resize);
			((System.ComponentModel.ISupportInitialize)(this.PictureBox)).EndInit();
			this.ViewerContextMenuStrip.ResumeLayout(false);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox PictureBox;
        private System.Windows.Forms.ContextMenuStrip ViewerContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem CropToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewOverlaysToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem InvertToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HistogramToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem RotateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FlipToolStripMenuItem;
    }
}