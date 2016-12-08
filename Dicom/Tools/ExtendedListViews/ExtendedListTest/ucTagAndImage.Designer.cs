namespace ExtendedListTest
{
	partial class ucTagAndImage
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
			SynapticEffect.Forms.ToggleColumnHeader toggleColumnHeader1 = new SynapticEffect.Forms.ToggleColumnHeader();
			SynapticEffect.Forms.ToggleColumnHeader toggleColumnHeader2 = new SynapticEffect.Forms.ToggleColumnHeader();
			SynapticEffect.Forms.ToggleColumnHeader toggleColumnHeader3 = new SynapticEffect.Forms.ToggleColumnHeader();
			this.tagTreeList = new SynapticEffect.Forms.TreeListView();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// tagTreeList
			// 
			this.tagTreeList.AllowColumnReorder = true;
			this.tagTreeList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.tagTreeList.BackColor = System.Drawing.SystemColors.Window;
			this.tagTreeList.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			toggleColumnHeader1.Hovered = false;
			toggleColumnHeader1.Image = null;
			toggleColumnHeader1.Index = 0;
			toggleColumnHeader1.Pressed = false;
			toggleColumnHeader1.ScaleStyle = SynapticEffect.Forms.ColumnScaleStyle.Slide;
			toggleColumnHeader1.Selected = false;
			toggleColumnHeader1.Text = "Tag Tree";
			toggleColumnHeader1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			toggleColumnHeader1.Visible = true;
			toggleColumnHeader1.Width = 300;
			toggleColumnHeader2.Hovered = false;
			toggleColumnHeader2.Image = null;
			toggleColumnHeader2.Index = 0;
			toggleColumnHeader2.Pressed = false;
			toggleColumnHeader2.ScaleStyle = SynapticEffect.Forms.ColumnScaleStyle.Slide;
			toggleColumnHeader2.Selected = false;
			toggleColumnHeader2.Text = "Attribute Name";
			toggleColumnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			toggleColumnHeader2.Visible = true;
			toggleColumnHeader2.Width = 100;
			toggleColumnHeader3.Hovered = false;
			toggleColumnHeader3.Image = null;
			toggleColumnHeader3.Index = 0;
			toggleColumnHeader3.Pressed = false;
			toggleColumnHeader3.ScaleStyle = SynapticEffect.Forms.ColumnScaleStyle.Slide;
			toggleColumnHeader3.Selected = false;
			toggleColumnHeader3.Text = "Attribute Value";
			toggleColumnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			toggleColumnHeader3.Visible = true;
			toggleColumnHeader3.Width = 200;
			this.tagTreeList.Columns.AddRange(new SynapticEffect.Forms.ToggleColumnHeader[] {
            toggleColumnHeader1,
            toggleColumnHeader2,
            toggleColumnHeader3});
			this.tagTreeList.ColumnSortColor = System.Drawing.Color.Gainsboro;
			this.tagTreeList.ColumnTrackColor = System.Drawing.Color.DimGray;
			this.tagTreeList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tagTreeList.FullRowSelect = false;
			this.tagTreeList.GridLineColor = System.Drawing.Color.WhiteSmoke;
			this.tagTreeList.GridLines = true;
			this.tagTreeList.HeaderMenu = null;
			this.tagTreeList.Indent = 20;
			this.tagTreeList.ItemHeight = 19;
			this.tagTreeList.ItemMenu = null;
			this.tagTreeList.LabelEdit = true;
			this.tagTreeList.Location = new System.Drawing.Point(3, 3);
			this.tagTreeList.Name = "tagTreeList";
			this.tagTreeList.RowSelectColor = System.Drawing.SystemColors.Highlight;
			this.tagTreeList.RowTrackColor = System.Drawing.Color.WhiteSmoke;
			this.tagTreeList.ShowLines = true;
			this.tagTreeList.ShowRootLines = true;
			this.tagTreeList.Size = new System.Drawing.Size(626, 608);
			this.tagTreeList.SmallImageList = null;
			this.tagTreeList.StateImageList = null;
			this.tagTreeList.TabIndex = 0;
			this.tagTreeList.Text = "treeListView1";
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.pictureBox1.Image = global::ExtendedListTest.Properties.Resources.ImagePattern;
			this.pictureBox1.Location = new System.Drawing.Point(635, 3);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(541, 613);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// ucTagAndImage
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.tagTreeList);
			this.Name = "ucTagAndImage";
			this.Size = new System.Drawing.Size(1174, 614);
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private SynapticEffect.Forms.TreeListView tagTreeList;
		private System.Windows.Forms.PictureBox pictureBox1;
	}
}
