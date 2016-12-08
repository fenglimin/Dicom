namespace ExtendedListTest.CustomControl
{
	partial class ucDircomDir
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ucDircomDir));
			SynapticEffect.Forms.ToggleColumnHeader toggleColumnHeader2 = new SynapticEffect.Forms.ToggleColumnHeader();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.imageListDicomDir = new System.Windows.Forms.ImageList(this.components);
			this.tabControl_DicomDirFiles = new System.Windows.Forms.TabControl();
			this.treeListView_DicomDir = new SynapticEffect.Forms.TreeListView();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.treeListView_DicomDir);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.tabControl_DicomDirFiles);
			this.splitContainer1.Size = new System.Drawing.Size(1158, 520);
			this.splitContainer1.SplitterDistance = 339;
			this.splitContainer1.TabIndex = 0;
			// 
			// imageListDicomDir
			// 
			this.imageListDicomDir.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListDicomDir.ImageStream")));
			this.imageListDicomDir.TransparentColor = System.Drawing.Color.Transparent;
			this.imageListDicomDir.Images.SetKeyName(0, "Patients.png");
			this.imageListDicomDir.Images.SetKeyName(1, "Patient.png");
			this.imageListDicomDir.Images.SetKeyName(2, "Study.png");
			this.imageListDicomDir.Images.SetKeyName(3, "Series.png");
			this.imageListDicomDir.Images.SetKeyName(4, "Image.png");
			// 
			// tabControl_DicomDirFiles
			// 
			this.tabControl_DicomDirFiles.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl_DicomDirFiles.Location = new System.Drawing.Point(0, 0);
			this.tabControl_DicomDirFiles.Name = "tabControl_DicomDirFiles";
			this.tabControl_DicomDirFiles.SelectedIndex = 0;
			this.tabControl_DicomDirFiles.Size = new System.Drawing.Size(815, 520);
			this.tabControl_DicomDirFiles.TabIndex = 0;
			this.tabControl_DicomDirFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl_DicomDirFiles_MouseDown);
			// 
			// treeListView_DicomDir
			// 
			this.treeListView_DicomDir.BackColor = System.Drawing.SystemColors.Window;
			this.treeListView_DicomDir.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			toggleColumnHeader2.Hovered = false;
			toggleColumnHeader2.Image = null;
			toggleColumnHeader2.Index = 0;
			toggleColumnHeader2.Pressed = false;
			toggleColumnHeader2.ScaleStyle = SynapticEffect.Forms.ColumnScaleStyle.Slide;
			toggleColumnHeader2.Selected = false;
			toggleColumnHeader2.Text = "DicomDir";
			toggleColumnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			toggleColumnHeader2.Visible = true;
			toggleColumnHeader2.Width = 1000;
			this.treeListView_DicomDir.Columns.AddRange(new SynapticEffect.Forms.ToggleColumnHeader[] {
            toggleColumnHeader2});
			this.treeListView_DicomDir.ColumnSortColor = System.Drawing.Color.Gainsboro;
			this.treeListView_DicomDir.ColumnTrackColor = System.Drawing.Color.WhiteSmoke;
			this.treeListView_DicomDir.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeListView_DicomDir.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.treeListView_DicomDir.GridLineColor = System.Drawing.Color.WhiteSmoke;
			this.treeListView_DicomDir.HeaderMenu = null;
			this.treeListView_DicomDir.Indent = 20;
			this.treeListView_DicomDir.ItemHeight = 19;
			this.treeListView_DicomDir.ItemMenu = null;
			this.treeListView_DicomDir.LabelEdit = false;
			this.treeListView_DicomDir.Location = new System.Drawing.Point(0, 0);
			this.treeListView_DicomDir.MultiSelect = true;
			this.treeListView_DicomDir.Name = "treeListView_DicomDir";
			this.treeListView_DicomDir.RowSelectColor = System.Drawing.SystemColors.Highlight;
			this.treeListView_DicomDir.RowTrackColor = System.Drawing.Color.WhiteSmoke;
			this.treeListView_DicomDir.ShowLines = true;
			this.treeListView_DicomDir.ShowRootLines = true;
			this.treeListView_DicomDir.Size = new System.Drawing.Size(339, 520);
			this.treeListView_DicomDir.SmallImageList = this.imageListDicomDir;
			this.treeListView_DicomDir.StateImageList = null;
			this.treeListView_DicomDir.TabIndex = 0;
			this.treeListView_DicomDir.Text = "treeListView1";
			this.treeListView_DicomDir.SizeChanged += new System.EventHandler(this.treeListView_DicomDir_SizeChanged);
			this.treeListView_DicomDir.DoubleClick += new System.EventHandler(this.treeListView_DicomDir_DoubleClick);
			this.treeListView_DicomDir.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.treeListView_DicomDir_MouseDoubleClick);
			// 
			// ucDircomDir
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ucDircomDir";
			this.Size = new System.Drawing.Size(1158, 520);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.SplitContainer splitContainer1;
		private SynapticEffect.Forms.TreeListView treeListView_DicomDir;
		private System.Windows.Forms.ImageList imageListDicomDir;
		private System.Windows.Forms.TabControl tabControl_DicomDirFiles;
	}
}
