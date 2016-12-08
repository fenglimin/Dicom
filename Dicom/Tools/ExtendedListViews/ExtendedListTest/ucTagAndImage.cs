using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;
using SynapticEffect.Forms;

namespace ExtendedListTest
{
	public partial class ucTagAndImage : UserControl
	{
		public ucTagAndImage()
		{
			InitializeComponent();

			var testFile = @"D:\Documents\Dose Report\1.2.840.113564.10001.2016033015344433716-dose report-CBCT.dcm";
			var stream = new FileStream(testFile, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);

			var dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
			dicom.Read(stream);

			foreach (Element element in dicom)
			{
				// do not show group length tags
				if (element.element == 0)
					continue;



				var node = new TreeListNode {Text = element.GetPath()};
				node.SubItems.Add(element.Description);

				tagTreeList.Nodes.Add(node);
				FillElement(element, node);
			}
		}

		private void FillElement(Element element, TreeListNode node)
		{
			if (element is Sequence)
			{
				int count = ((Sequence)element).Items.Count;
				for (int n = 0; n < count; n++)
				{
					Elements item = ((Sequence)element).Items[n];
					
					foreach (Element child in item)
					{
						// do not show group length tags
						if (child.element == 0)
							continue;

						var childNode = new TreeListNode();
						childNode.Text = child.GetPath();
						childNode.SubItems.Add(child.Description);

						node.Nodes.Add(childNode);
						FillElement(child, childNode);
					}
				}
			}
			else if (element is PixelData)
			{
				//if (((PixelData)element).IsEncapsulated)
				//{
				//	int count = ((PixelData)element).Frames.Count;
				//	for (int n = 0; n < count; n++)
				//	{
				//		TreeNode frameNode = node.Nodes.Add(element.GetPath() + " frame" + n.ToString(), "Frame");
				//		string text = String.Format("{0} byte(s).", ((PixelData)element).Frames[n].Length);
				//		frameNode.Nodes.Add(element.GetPath() + " value", text);
				//	}
				//}
				//else
				//{
				//	FillValue(element, node);
				//}
			}
			else
			{
				FillValue(element, node);
			}
		}

		private void FillValue(Element element, TreeListNode node)
		{
			if (element.Value is Array)
			{
				StringBuilder text = new StringBuilder();
				if (((Array)element.Value).Length > 12)
				{
					text.Append(String.Format("{0} byte(s).", element.Length));
				}
				else
				{
					bool first = true;
					foreach (object entry in element.Value as Array)
					{
						// we cannot bas this on text.Length because the fist string could be empty
						if (!first)
						{
							text.Append("\\");
						}
						else
						{
							first = false;
						}
						text.Append(entry.ToString());
						if (text.Length > 80)
						{
							text.Append(" ...");
							break;
						}
					}
				}
				node.SubItems.Add(text.ToString());
			}
			else
			{
				object value = element.Value;
				node.SubItems.Add( (value == null) ? String.Empty : value.ToString());
			}
		}
	}
}
