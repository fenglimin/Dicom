namespace DicomEditor
{
    partial class Browser
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
            this.TreeView = new System.Windows.Forms.TreeView();
            this.TagContextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.NewTagToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.CopyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExpandAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FindToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ViewBinaryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ImportValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExportValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.HexControl = new DicomEditor.HexControl();
            this.TagContextMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // TreeView
            // 
            this.TreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.TreeView.ContextMenuStrip = this.TagContextMenuStrip;
            this.TreeView.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeView.HideSelection = false;
            this.TreeView.LabelEdit = true;
            this.TreeView.Location = new System.Drawing.Point(0, 0);
            this.TreeView.Name = "TreeView";
            this.TreeView.Size = new System.Drawing.Size(625, 451);
            this.TreeView.TabIndex = 0;
            this.TreeView.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeView_BeforeLabelEdit);
            this.TreeView.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.TreeView_AfterLabelEdit);
            this.TreeView.Click += new System.EventHandler(this.HexControl_Leave);
            this.TreeView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeView_MouseUp);
            // 
            // TagContextMenuStrip
            // 
            this.TagContextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.NewTagToolStripMenuItem,
            this.DeleteToolStripMenuItem,
            this.CopyToolStripMenuItem,
            this.toolStripSeparator1,
            this.ExpandAllToolStripMenuItem,
            this.FindToolStripMenuItem,
            this.toolStripSeparator2,
            this.ViewBinaryToolStripMenuItem,
            this.toolStripSeparator3,
            this.ImportValueToolStripMenuItem,
            this.ExportValueToolStripMenuItem});
            this.TagContextMenuStrip.Name = "ContextMenuStrip";
            this.TagContextMenuStrip.Size = new System.Drawing.Size(181, 220);
            this.TagContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(this.TagContextMenuStrip_Opening);
            // 
            // NewTagToolStripMenuItem
            // 
            this.NewTagToolStripMenuItem.Name = "NewTagToolStripMenuItem";
            this.NewTagToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Insert;
            this.NewTagToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.NewTagToolStripMenuItem.Text = "&Add";
            this.NewTagToolStripMenuItem.Click += new System.EventHandler(this.NewTagToolStripMenuItem_Click);
            // 
            // DeleteToolStripMenuItem
            // 
            this.DeleteToolStripMenuItem.Name = "DeleteToolStripMenuItem";
            this.DeleteToolStripMenuItem.ShortcutKeys = System.Windows.Forms.Keys.Delete;
            this.DeleteToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.DeleteToolStripMenuItem.Text = "&Delete";
            this.DeleteToolStripMenuItem.Click += new System.EventHandler(this.DeleteToolStripMenuItem_Click);
            // 
            // CopyToolStripMenuItem
            // 
            this.CopyToolStripMenuItem.Name = "CopyToolStripMenuItem";
            this.CopyToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.CopyToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.CopyToolStripMenuItem.Text = "&Copy";
            this.CopyToolStripMenuItem.Click += new System.EventHandler(this.CopyToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // ExpandAllToolStripMenuItem
            // 
            this.ExpandAllToolStripMenuItem.Name = "ExpandAllToolStripMenuItem";
            this.ExpandAllToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.E)));
            this.ExpandAllToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ExpandAllToolStripMenuItem.Text = "&Expand All";
            this.ExpandAllToolStripMenuItem.Click += new System.EventHandler(this.ExpandAllToolStripMenuItem_Click);
            // 
            // FindToolStripMenuItem
            // 
            this.FindToolStripMenuItem.Name = "FindToolStripMenuItem";
            this.FindToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
            this.FindToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.FindToolStripMenuItem.Text = "&Find";
            this.FindToolStripMenuItem.Click += new System.EventHandler(this.FindToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // ViewBinaryToolStripMenuItem
            // 
            this.ViewBinaryToolStripMenuItem.Name = "ViewBinaryToolStripMenuItem";
            this.ViewBinaryToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            this.ViewBinaryToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ViewBinaryToolStripMenuItem.Text = "View &Binary";
            this.ViewBinaryToolStripMenuItem.Click += new System.EventHandler(this.ViewBinaryToolStripMenuItem_Click);
            // 
            // ImportValueToolStripMenuItem
            // 
            this.ImportValueToolStripMenuItem.Name = "ImportValueToolStripMenuItem";
            this.ImportValueToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.I)));
            this.ImportValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ImportValueToolStripMenuItem.Text = "Import &Value";
            this.ImportValueToolStripMenuItem.Click += new System.EventHandler(this.ImportValueToolStripMenuItem_Click);
            // 
            // ExportValueToolStripMenuItem
            // 
            this.ExportValueToolStripMenuItem.Name = "ExportValueToolStripMenuItem";
            this.ExportValueToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.ExportValueToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.ExportValueToolStripMenuItem.Text = "Export Value";
            this.ExportValueToolStripMenuItem.Click += new System.EventHandler(this.ExportValueToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // HexControl
            // 
            this.HexControl.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.HexControl.Location = new System.Drawing.Point(0, 0);
            this.HexControl.Name = "HexControl";
            this.HexControl.Size = new System.Drawing.Size(577, 121);
            this.HexControl.TabIndex = 0;
            this.HexControl.Visible = false;
            this.HexControl.Leave += new System.EventHandler(this.HexControl_Leave);
            // 
            // Browser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 451);
            this.Controls.Add(this.HexControl);
            this.Controls.Add(this.TreeView);
            this.Name = "Browser";
            this.Text = "untitled";
            this.Load += new System.EventHandler(this.Browser_Load);
            this.Move += new System.EventHandler(this.Browser_Move);
            this.TagContextMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView TreeView;
        private System.Windows.Forms.ContextMenuStrip TagContextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem NewTagToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem DeleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem CopyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExpandAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewBinaryToolStripMenuItem;
        private HexControl HexControl;
        private System.Windows.Forms.ToolStripMenuItem FindToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ImportValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExportValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;

    }
}