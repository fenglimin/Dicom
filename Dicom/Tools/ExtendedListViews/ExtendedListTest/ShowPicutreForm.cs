using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace ExtendedListTest
{
	public partial class ShowPicutreForm : Form
	{
		private bool isMouseDown = false;
		private Point mouseDownPos;

		public ShowPicutreForm()
		{
			InitializeComponent();
		}

		private void ShowPicutreForm_Load(object sender, EventArgs e)
		{
		}

		public void SetImage(System.Drawing.Image image)
		{
			pictureBox1.Image = image;
			Text = string.Format("Show Picture in Original Size : {0}x{1}", pictureBox1.Image.Width, pictureBox1.Image.Height);
		}

		private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
		{
			isMouseDown = true;
			mouseDownPos = e.Location;
		}

		private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
		{
			if (isMouseDown)
			{
				//var newX = panel1.HorizontalScroll.Value;
				//newX += e.Location.X - mouseDownPos.X;// ? 1 : -1;
				//if (newX < panel1.HorizontalScroll.Minimum) 
				//	newX = panel1.HorizontalScroll.Minimum;
				//if (newX > panel1.HorizontalScroll.Maximum) 
				//	newX = panel1.HorizontalScroll.Maximum;

				//var newY = panel1.VerticalScroll.Value;
				//newY += e.Location.Y - mouseDownPos.Y;// ? 1 : -1;

				//if (newY < panel1.VerticalScroll.Minimum) 
				//	newY = panel1.VerticalScroll.Minimum;
				//if (newY > panel1.VerticalScroll.Maximum)
				//	newY = panel1.VerticalScroll.Maximum;

				//panel1.HorizontalScroll.Value = newX;
				//panel1.VerticalScroll.Value = newY;

				//mouseDownPos = e.Location;
			}

		}

		private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
		{
			isMouseDown = false;
		}
	}
}
