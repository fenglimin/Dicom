namespace DicomExplorer
{
    partial class Explorer
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
            this.SplitContainer = new System.Windows.Forms.SplitContainer();
            this.FolderTreeView = new System.Windows.Forms.TreeView();
            this.FileListView = new System.Windows.Forms.ListView();
            this.ListViewContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ExportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sepToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
            this.EditToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CompareToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
            this.SplitContainer.Panel1.SuspendLayout();
            this.SplitContainer.Panel2.SuspendLayout();
            this.SplitContainer.SuspendLayout();
            this.ListViewContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // SplitContainer
            // 
            this.SplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SplitContainer.Location = new System.Drawing.Point(0, 0);
            this.SplitContainer.Name = "SplitContainer";
            // 
            // SplitContainer.Panel1
            // 
            this.SplitContainer.Panel1.Controls.Add(this.FolderTreeView);
            // 
            // SplitContainer.Panel2
            // 
            this.SplitContainer.Panel2.Controls.Add(this.FileListView);
            this.SplitContainer.Size = new System.Drawing.Size(784, 562);
            this.SplitContainer.SplitterDistance = 261;
            this.SplitContainer.TabIndex = 0;
            // 
            // FolderTreeView
            // 
            this.FolderTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FolderTreeView.Location = new System.Drawing.Point(0, 0);
            this.FolderTreeView.Name = "FolderTreeView";
            this.FolderTreeView.Size = new System.Drawing.Size(261, 562);
            this.FolderTreeView.TabIndex = 0;
            this.FolderTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.FolderTreeView_MouseClick);
            // 
            // FileListView
            // 
            this.FileListView.AutoArrange = false;
            this.FileListView.ContextMenuStrip = this.ListViewContextMenuStrip;
            this.FileListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FileListView.FullRowSelect = true;
            this.FileListView.Location = new System.Drawing.Point(0, 0);
            this.FileListView.Name = "FileListView";
            this.FileListView.Size = new System.Drawing.Size(519, 562);
            this.FileListView.TabIndex = 0;
            this.FileListView.UseCompatibleStateImageBehavior = false;
            this.FileListView.View = System.Windows.Forms.View.Details;
            this.FileListView.VirtualMode = true;
            this.FileListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.FileListView_RetrieveVirtualItem);
            this.FileListView.DoubleClick += new System.EventHandler(this.FileListView_DoubleClick);
            // 
            // ListViewContextMenuStrip
            // 
            this.ListViewContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ExportToolStripMenuItem,
            this.sepToolStripMenuItem,
            this.EditToolStripMenuItem,
            this.ViewToolStripMenuItem,
            this.CompareToolStripMenuItem});
            this.ListViewContextMenuStrip.Name = "contextMenuStrip1";
            this.ListViewContextMenuStrip.Size = new System.Drawing.Size(124, 98);
            this.ListViewContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.ListViewContextMenuStrip_Opening);
            // 
            // ExportToolStripMenuItem
            // 
            this.ExportToolStripMenuItem.Name = "ExportToolStripMenuItem";
            this.ExportToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.ExportToolStripMenuItem.Text = "E&xport";
            this.ExportToolStripMenuItem.Click += new System.EventHandler(this.ExportToolStripMenuItem_Click);
            // 
            // sepToolStripMenuItem
            // 
            this.sepToolStripMenuItem.Name = "sepToolStripMenuItem";
            this.sepToolStripMenuItem.Size = new System.Drawing.Size(120, 6);
            // 
            // EditToolStripMenuItem
            // 
            this.EditToolStripMenuItem.Name = "EditToolStripMenuItem";
            this.EditToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.EditToolStripMenuItem.Text = "&Edit";
            this.EditToolStripMenuItem.Click += new System.EventHandler(this.EditToolStripMenuItem_Click);
            // 
            // ViewToolStripMenuItem
            // 
            this.ViewToolStripMenuItem.Name = "ViewToolStripMenuItem";
            this.ViewToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.ViewToolStripMenuItem.Text = "&View";
            this.ViewToolStripMenuItem.Click += new System.EventHandler(this.ViewToolStripMenuItem_Click);
            // 
            // CompareToolStripMenuItem
            // 
            this.CompareToolStripMenuItem.Name = "CompareToolStripMenuItem";
            this.CompareToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.CompareToolStripMenuItem.Text = "&Compare";
            this.CompareToolStripMenuItem.Click += new System.EventHandler(this.CompareToolStripMenuItem_Click);
            // 
            // Explorer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 562);
            this.Controls.Add(this.SplitContainer);
            this.Name = "Explorer";
            this.Text = "Explorer";
            this.SplitContainer.Panel1.ResumeLayout(false);
            this.SplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
            this.SplitContainer.ResumeLayout(false);
            this.ListViewContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer SplitContainer;
        private System.Windows.Forms.TreeView FolderTreeView;
        private System.Windows.Forms.ListView FileListView;
        private System.Windows.Forms.ContextMenuStrip ListViewContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem ExportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CompareToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator sepToolStripMenuItem;
    }
}