using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomReceiver
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			var testFile = @"D:\Documents\Dose Report\1.2.840.113564.10001.2016033015344433716-dose report-CBCT.dcm";
			var stream = new FileStream(testFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

			var dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
			dicom.Read(stream);

			foreach (Element element in dicom)
			{
				// do not show group length tags
				if (element.element == 0)
					continue;
				//TreeNode node = root.Nodes.Add(element.GetPath() + " tag", Title(element));
				//FillElement(element, node);
			}
		}
	}
}
