namespace DicomEditor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.MainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.FileMenuItem = new System.Windows.Forms.MenuItem();
            this.NewMenuItem = new System.Windows.Forms.MenuItem();
            this.OpenMenuItem = new System.Windows.Forms.MenuItem();
            this.CloseMenuItem = new System.Windows.Forms.MenuItem();
            this.SaveMenuItem = new System.Windows.Forms.MenuItem();
            this.SaveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuFileRecentFile = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.ExitMenuItem = new System.Windows.Forms.MenuItem();
            this.EditMenuItem = new System.Windows.Forms.MenuItem();
            this.FindMenuItem = new System.Windows.Forms.MenuItem();
            this.ViewMenuItem = new System.Windows.Forms.MenuItem();
            this.DataSetMenuItem = new System.Windows.Forms.MenuItem();
            this.DICOMDIRMenuItem = new System.Windows.Forms.MenuItem();
            this.WindowMenuItem = new System.Windows.Forms.MenuItem();
            this.CascadeMenuItem = new System.Windows.Forms.MenuItem();
            this.TileVerticalMenuItem = new System.Windows.Forms.MenuItem();
            this.TileHorizontalMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.LoggingMenuItem = new System.Windows.Forms.MenuItem();
            this.ToolsMenuItem = new System.Windows.Forms.MenuItem();
            this.DeIdentifyMenuItem = new System.Windows.Forms.MenuItem();
            this.CheckIODMenuItem = new System.Windows.Forms.MenuItem();
            this.StorageCommitMenuItem = new System.Windows.Forms.MenuItem();
            this.PACSModeMenuItem = new System.Windows.Forms.MenuItem();
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
            this.EditMenuItem,
            this.ViewMenuItem,
            this.WindowMenuItem,
            this.ToolsMenuItem,
            this.HelpMenuItem});
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.Index = 0;
            this.FileMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.NewMenuItem,
            this.OpenMenuItem,
            this.CloseMenuItem,
            this.SaveMenuItem,
            this.SaveAsMenuItem,
            this.menuItem1,
            this.menuFileRecentFile,
            this.menuItem2,
            this.ExitMenuItem});
            this.FileMenuItem.Text = "&File";
            this.FileMenuItem.Popup += new System.EventHandler(this.Menu_Popup);
            // 
            // NewMenuItem
            // 
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
            // CloseMenuItem
            // 
            this.CloseMenuItem.Index = 2;
            this.CloseMenuItem.Text = "&Close";
            this.CloseMenuItem.Click += new System.EventHandler(this.CloseMenuItem_Click);
            // 
            // SaveMenuItem
            // 
            this.SaveMenuItem.Index = 3;
            this.SaveMenuItem.Text = "&Save";
            this.SaveMenuItem.Click += new System.EventHandler(this.SaveMenuItem_Click);
            // 
            // SaveAsMenuItem
            // 
            this.SaveAsMenuItem.Index = 4;
            this.SaveAsMenuItem.Text = "Save As ...";
            this.SaveAsMenuItem.Click += new System.EventHandler(this.SaveAsMenuItem_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 5;
            this.menuItem1.Text = "-";
            // 
            // menuFileRecentFile
            // 
            this.menuFileRecentFile.Index = 6;
            this.menuFileRecentFile.Text = "Recent File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 7;
            this.menuItem2.Text = "-";
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Index = 8;
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // EditMenuItem
            // 
            this.EditMenuItem.Index = 1;
            this.EditMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.FindMenuItem});
            this.EditMenuItem.Text = "&Edit";
            // 
            // FindMenuItem
            // 
            this.FindMenuItem.Index = 0;
            this.FindMenuItem.Text = "&Find";
            this.FindMenuItem.Click += new System.EventHandler(this.FindMenuItem_Click);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.Index = 2;
            this.ViewMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.DataSetMenuItem,
            this.DICOMDIRMenuItem});
            this.ViewMenuItem.Text = "&View";
            // 
            // DataSetMenuItem
            // 
            this.DataSetMenuItem.Checked = true;
            this.DataSetMenuItem.Index = 0;
            this.DataSetMenuItem.RadioCheck = true;
            this.DataSetMenuItem.Text = "DataSet";
            this.DataSetMenuItem.Click += new System.EventHandler(this.ViewMenuItem_Click);
            // 
            // DICOMDIRMenuItem
            // 
            this.DICOMDIRMenuItem.Index = 1;
            this.DICOMDIRMenuItem.RadioCheck = true;
            this.DICOMDIRMenuItem.Text = "DICOMDIR";
            this.DICOMDIRMenuItem.Click += new System.EventHandler(this.ViewMenuItem_Click);
            // 
            // WindowMenuItem
            // 
            this.WindowMenuItem.Index = 3;
            this.WindowMenuItem.MdiList = true;
            this.WindowMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.CascadeMenuItem,
            this.TileVerticalMenuItem,
            this.TileHorizontalMenuItem,
            this.menuItem3,
            this.LoggingMenuItem});
            this.WindowMenuItem.Text = "&Window";
            this.WindowMenuItem.Popup += new System.EventHandler(this.Menu_Popup);
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
            // menuItem3
            // 
            this.menuItem3.Index = 3;
            this.menuItem3.Text = "-";
            // 
            // LoggingMenuItem
            // 
            this.LoggingMenuItem.Index = 4;
            this.LoggingMenuItem.Text = "&Logging";
            this.LoggingMenuItem.Click += new System.EventHandler(this.LoggingMenuItem_Click);
            // 
            // ToolsMenuItem
            // 
            this.ToolsMenuItem.Index = 4;
            this.ToolsMenuItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.DeIdentifyMenuItem,
            this.CheckIODMenuItem,
            this.StorageCommitMenuItem,
            this.PACSModeMenuItem});
            this.ToolsMenuItem.Text = "&Tools";
            this.ToolsMenuItem.Popup += new System.EventHandler(this.ToolsMenuItem_Popup);
            // 
            // DeIdentifyMenuItem
            // 
            this.DeIdentifyMenuItem.Index = 0;
            this.DeIdentifyMenuItem.Text = "&De-Identify";
            this.DeIdentifyMenuItem.Click += new System.EventHandler(this.DeIdentifyMenuItem_Click);
            // 
            // CheckIODMenuItem
            // 
            this.CheckIODMenuItem.Index = 1;
            this.CheckIODMenuItem.Text = "&Verify IOD";
            this.CheckIODMenuItem.Click += new System.EventHandler(this.CheckIODMenuItem_Click);
            // 
            // StorageCommitMenuItem
            // 
            this.StorageCommitMenuItem.Index = 2;
            this.StorageCommitMenuItem.Text = "&Storage Commit";
            this.StorageCommitMenuItem.Click += new System.EventHandler(this.StorageCommitMenuItem_Click);
            // 
            // PACSModeMenuItem
            // 
            this.PACSModeMenuItem.Index = 3;
            this.PACSModeMenuItem.Text = "&PACS Mode";
            this.PACSModeMenuItem.Click += new System.EventHandler(this.PACSModeMenuItemClick);
            // 
            // HelpMenuItem
            // 
            this.HelpMenuItem.Index = 5;
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
            this.StatusStrip.Location = new System.Drawing.Point(0, 762);
            this.StatusStrip.Name = "StatusStrip";
            this.StatusStrip.Padding = new System.Windows.Forms.Padding(1, 0, 19, 0);
            this.StatusStrip.Size = new System.Drawing.Size(1000, 22);
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
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 784);
            this.Controls.Add(this.StatusStrip);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Menu = this.MainMenu;
            this.Name = "MainForm";
            this.Text = "Dicom Editor";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.MdiChildActivate += new System.EventHandler(this.MainForm_MdiChildActivate);
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
        private System.Windows.Forms.MenuItem OpenMenuItem;
        private System.Windows.Forms.MenuItem CloseMenuItem;
        private System.Windows.Forms.MenuItem ExitMenuItem;
        private System.Windows.Forms.MenuItem NewMenuItem;
        private System.Windows.Forms.MenuItem WindowMenuItem;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem CascadeMenuItem;
        private System.Windows.Forms.MenuItem TileVerticalMenuItem;
        private System.Windows.Forms.MenuItem TileHorizontalMenuItem;
        private System.Windows.Forms.MenuItem SaveMenuItem;
        private System.Windows.Forms.MenuItem menuFileRecentFile;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem SaveAsMenuItem;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem LoggingMenuItem;
        private System.Windows.Forms.MenuItem DeIdentifyMenuItem;
        private System.Windows.Forms.MenuItem HelpMenuItem;
        private System.Windows.Forms.MenuItem AboutMenuItem;
        private System.Windows.Forms.MenuItem EditMenuItem;
        private System.Windows.Forms.MenuItem FindMenuItem;
        private System.Windows.Forms.MenuItem CheckIODMenuItem;
        private System.Windows.Forms.MenuItem ToolsMenuItem;
        private System.Windows.Forms.StatusStrip StatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel StatusStripStatusLabel;
        private System.Windows.Forms.MenuItem ViewMenuItem;
        private System.Windows.Forms.MenuItem DataSetMenuItem;
        private System.Windows.Forms.MenuItem DICOMDIRMenuItem;
        private System.Windows.Forms.MenuItem StorageCommitMenuItem;
        private System.Windows.Forms.MenuItem PACSModeMenuItem;
    }
}

