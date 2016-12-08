namespace DicomViewer
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
            this.components = new System.ComponentModel.Container();
            this.MainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.FileMenuItem = new System.Windows.Forms.MenuItem();
            this.NewMenuItem = new System.Windows.Forms.MenuItem();
            this.OpenMenuItem = new System.Windows.Forms.MenuItem();
            this.CompareMenuItem = new System.Windows.Forms.MenuItem();
            this.CloseMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuFileRecentFile = new System.Windows.Forms.MenuItem();
            this.SaveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.ExitMenuItem = new System.Windows.Forms.MenuItem();
            this.WindowMenuItem = new System.Windows.Forms.MenuItem();
            this.CascadeMenuItem = new System.Windows.Forms.MenuItem();
            this.TileVerticalMenuItem = new System.Windows.Forms.MenuItem();
            this.TileHorizontalMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.LoggingMenuItem = new System.Windows.Forms.MenuItem();
            this.HelpMenuItem = new System.Windows.Forms.MenuItem();
            this.AboutMenuItem = new System.Windows.Forms.MenuItem();
            this.StatusStrip = new System.Windows.Forms.StatusStrip();
            this.StatusStripStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainMenu
            // 
            this.MainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FileMenuItem,
            this.WindowMenuItem,
            this.HelpMenuItem});
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.Index = 0;
            this.FileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.NewMenuItem,
            this.OpenMenuItem,
            this.menuFileRecentFile,
            this.CompareMenuItem,
            this.CloseMenuItem,
            this.menuItem3,
            this.SaveAsMenuItem,
            this.menuItem1,
            this.ExitMenuItem});
            this.FileMenuItem.Text = "&File";
            // 
            // NewMenuItem
            // 
            this.NewMenuItem.Enabled = false;
            this.NewMenuItem.Index = 0;
            this.NewMenuItem.Text = "&New";
            this.NewMenuItem.Click += new System.EventHandler(this.NewMenuItem_Click);
            // 
            // OpenMenuItem
            // 
            this.OpenMenuItem.Index = 1;
            this.OpenMenuItem.Text = "&Open ...";
            this.OpenMenuItem.Click += new System.EventHandler(this.OpenMenuItem_Click);
            // 
            // CompareMenuItem
            // 
            this.CompareMenuItem.Index = 2;
            this.CompareMenuItem.Text = "Co&mpare";
            this.CompareMenuItem.Click += new System.EventHandler(this.CompareMenuItem_Click);
            // 
            // menuFileRecentFile
            // 
            this.menuFileRecentFile.Index = 6;
            this.menuFileRecentFile.Text = "Recent File";
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Index = 3;
            this.CloseMenuItem.Text = "&Close";
            this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 4;
            this.menuItem3.Text = "-";
            // 
            // SaveAsMenuItem
            // 
            this.SaveAsMenuItem.Index = 5;
            this.SaveAsMenuItem.Text = "&Save as ...";
            this.SaveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItem_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 8;
            this.menuItem1.Text = "-";
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Index = 7;
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // WindowMenuItem
            // 
            this.WindowMenuItem.Index = 1;
            this.WindowMenuItem.MdiList = true;
            this.WindowMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.CascadeMenuItem,
            this.TileVerticalMenuItem,
            this.TileHorizontalMenuItem,
            this.menuItem2,
            this.LoggingMenuItem});
            this.WindowMenuItem.Text = "&Window";
            // 
            // CascadeMenuItem
            // 
            this.CascadeMenuItem.Index = 0;
            this.CascadeMenuItem.Text = "&Cascade";
            this.CascadeMenuItem.Click += new System.EventHandler(this.CascadeMenuItem_Click);
            // 
            // TileVerticalMenuItem
            // 
            this.TileVerticalMenuItem.Index = 1;
            this.TileVerticalMenuItem.Text = "Tile &Vertical";
            this.TileVerticalMenuItem.Click += new System.EventHandler(this.TileVerticalMenuItem_Click);
            // 
            // TileHorizontalMenuItem
            // 
            this.TileHorizontalMenuItem.Index = 2;
            this.TileHorizontalMenuItem.Text = "Tile &Horizontal";
            this.TileHorizontalMenuItem.Click += new System.EventHandler(this.TileHorizontalMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 3;
            this.menuItem2.Text = "-";
            // 
            // LoggingMenuItem
            // 
            this.LoggingMenuItem.Index = 4;
            this.LoggingMenuItem.Text = "&Logging";
            this.LoggingMenuItem.Click += new System.EventHandler(this.LoggingMenuItem_Click);
            // 
            // HelpMenuItem
            // 
            this.HelpMenuItem.Index = 2;
            this.HelpMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.AboutMenuItem});
            this.HelpMenuItem.Text = "&Help";
            // 
            // AboutMenuItem
            // 
            this.AboutMenuItem.Index = 0;
            this.AboutMenuItem.Text = "&About";
            this.AboutMenuItem.Click += new System.EventHandler(this.AboutMenuItem_Click);
            // 
            // StatusStrip
            // 
            this.StatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusStripStatusLabel});
            this.StatusStrip.Location = new System.Drawing.Point(0, 636);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Size = new System.Drawing.Size(605, 22);
            this.StatusStrip.TabIndex = 1;
            // 
            // StatusStripStatusLabel
            // 
            this.StatusStripStatusLabel.Name = "StatusStripStatusLabel";
            this.StatusStripStatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 658);
            this.Controls.Add(this.StatusStrip);
            this.IsMdiContainer = true;
            this.Menu = this.MainMenu;
            this.Name = "MainForm";
            this.Text = "Dicom Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.StatusStrip.ResumeLayout(false);
            this.StatusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private MruMenu mruMenu;
        private System.Windows.Forms.MainMenu MainMenu;
        private System.Windows.Forms.MenuItem FileMenuItem;
        private System.Windows.Forms.MenuItem NewMenuItem;
        private System.Windows.Forms.MenuItem OpenMenuItem;
        private System.Windows.Forms.MenuItem CloseMenuItem;
        private System.Windows.Forms.MenuItem ExitMenuItem;
        private System.Windows.Forms.MenuItem WindowMenuItem;
        private System.Windows.Forms.MenuItem CascadeMenuItem;
        private System.Windows.Forms.MenuItem TileVerticalMenuItem;
        private System.Windows.Forms.MenuItem TileHorizontalMenuItem;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem SaveAsMenuItem;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuFileRecentFile;
        private System.Windows.Forms.MenuItem HelpMenuItem;
        private System.Windows.Forms.MenuItem AboutMenuItem;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusStripStatusLabel;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem LoggingMenuItem;
        private System.Windows.Forms.MenuItem CompareMenuItem;

    }
}

