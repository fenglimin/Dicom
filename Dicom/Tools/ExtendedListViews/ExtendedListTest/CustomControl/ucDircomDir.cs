using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DicomViewer;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest.Service;
using SynapticEffect.Forms;

namespace ExtendedListTest.CustomControl
{
	public partial class ucDircomDir : UserControl, IElementsBase
	{
		private IDicomServiceWorkerUser dicomServiceWorkerUser;
		private ReceivedDicomElements receivedDicomElements;
		private string lastError;

		public ucDircomDir(ReceivedDicomElements receivedDicomElements, IDicomServiceWorkerUser dicomServiceWorkerUser)
		{
			InitializeComponent();

			tabControl_DicomDirFiles.DrawMode = TabDrawMode.OwnerDrawFixed;
			tabControl_DicomDirFiles.DrawItem += tabControl_DicomDirFiles_DrawItem;


			this.receivedDicomElements = receivedDicomElements;
			this.dicomServiceWorkerUser = dicomServiceWorkerUser;

			receivedDicomElements.FileName = (new FileInfo(receivedDicomElements.FileName)).DirectoryName;

			var dir = new DicomDir(receivedDicomElements.FileName);

			var rootNode = new TreeListNode {Text = "All Patients"};
			treeListView_DicomDir.Nodes.Add(rootNode);
			FillElement(dir.Patients, rootNode);
			rootNode.ExpandAll();
		}

		public ReceivedDicomElements GetReceivedDicomElements()
		{
			return receivedDicomElements;
		}

		public string GetLastError()
		{
			return lastError;
		}

		private void FillElement(IEnumerable<DirectoryRecord> records, TreeListNode root)
		{
			foreach (var record in records)
			{
				var node = CreateNode(record);
				root.Nodes.Add(node);
				var parent = record as DirectoryParent;
				if (parent != null)
				{
					FillElement(parent.Children, node);
				}
			}
		}

		private TreeListNode CreateNode(DirectoryRecord directoryRecord)
		{
			var node = new TreeListNode();

			var recordType = directoryRecord.DirectoryRecordType.ToUpper();
			if (recordType == "PATIENT")
			{
				node.ImageIndex = 1;
				node.Text = string.Format("{0} : {1}, {2}", directoryRecord.DirectoryRecordType,
					directoryRecord.Elements.GetSafeString(t.PatientName),
					directoryRecord.Elements.GetSafeString(t.PatientID));
			}
			else if (recordType == "STUDY")
			{
				node.ImageIndex = 2;
				node.Text = string.Format("{0} : {1}, {2} ({3})", directoryRecord.DirectoryRecordType,
					directoryRecord.Elements.GetSafeString(t.StudyDate),
					directoryRecord.Elements.GetSafeString(t.StudyTime), directoryRecord.Elements.GetSafeString(t.StudyDescription));
			}
			else if (recordType == "SERIES")
			{
				node.ImageIndex = 3;
				node.Text = string.Format("{0} : {1}, {2}", directoryRecord.DirectoryRecordType, directoryRecord.Elements.GetSafeString(t.Modality),
							 directoryRecord.Elements.GetSafeString(t.SeriesNumber));
			}
			else if (recordType == "IMAGE" || recordType == "SR DOCUMENT")
			{
				node.ImageIndex = 4;
				node.Text = string.Format("{0} : {1}, {2}", directoryRecord.DirectoryRecordType, directoryRecord.Elements.GetSafeString(t.InstanceNumber),
							 directoryRecord.Elements.GetSafeString(t.ReferencedSOPInstanceUIDinFile));
				node.AdditionalData = directoryRecord.Elements[t.ReferencedFileID];
			}

			return node;
		}

		private void treeListView_DicomDir_DoubleClick(object sender, EventArgs e)
		{

		}

		private void treeListView_DicomDir_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (treeListView_DicomDir.SelectedNodes.Count != 1)
				return;

			var fileId = treeListView_DicomDir.SelectedNodes[0].AdditionalData as Element;
			if (fileId == null)
				return;

			var filePath = fileId.Value as string[];
			if (filePath == null)
				return;
			
			var inDicomDirPath = filePath.Aggregate(string.Empty, (current, s) => current + ("\\" + s));
			var fullPath = receivedDicomElements.FileName + inDicomDirPath;
			
			try
			{
				var elements = OtherImageFormats.Read(fullPath);
				var receivedDicom = new ReceivedDicomElements
				{
					CallingAeTitle = "Localhost",
					FileName = fullPath,
					Elements = elements,
					ImageSource = ImageSource.LocalDicomFile,
					SavedToDisk = true
				};

				ShowElements(receivedDicom, inDicomDirPath);

				dicomServiceWorkerUser.ShowMessage(fullPath + " opened successfully!", false, false);
			}
			catch (Exception ex)
			{
				dicomServiceWorkerUser.ShowMessage(fullPath + " opened failed! " + ex.Message, true, true);
			}
		}

		private void ShowElements(ReceivedDicomElements receivedDicom, string inDicomDirPath)
		{
			var tabPage = new TabPage { Text = inDicomDirPath };
			UserControl userControl;

			var modality = receivedDicom.Elements.GetSafeStringValue(t.Modality).ToUpper();
			if (modality == "SR")
			{
				userControl = new ucDoseReport(receivedDicom) { Dock = DockStyle.Fill };
			}
			else if (modality == "PR" || receivedDicom.ImageSource == ImageSource.StorageCommitment || receivedDicom.ImageSource == ImageSource.Mpps)
			{
				userControl = new ucTag(receivedDicom) { Dock = DockStyle.Fill };
			}
			else
			{
				userControl = new ucTagAndImage(receivedDicom) { Dock = DockStyle.Fill };
			}


			tabPage.Controls.Add(userControl);
			tabControl_DicomDirFiles.Controls.Add(tabPage);
			tabControl_DicomDirFiles.SelectTab(tabPage);
		}

		private void treeListView_DicomDir_SizeChanged(object sender, EventArgs e)
		{
			//if (Parent != null)
			//	treeListView_DicomDir.Columns[0].Width = Parent.Size.Width;
		}

		private void tabControl_DicomDirFiles_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Right)
			{
				// Find tab control uncer mouse
				var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
				for (var i = 0; i < tabControl_DicomDirFiles.TabCount; i++)
				{
					if (tabControl_DicomDirFiles.GetTabRect(i).IntersectsWith(mouseRect))
					{
						tabControl_DicomDirFiles.SelectedIndex = i;
						break;
					}
				}

				var oldSelectedIndex = tabControl_DicomDirFiles.SelectedIndex;
				tabControl_DicomDirFiles.TabPages.Remove(tabControl_DicomDirFiles.SelectedTab);


				if (oldSelectedIndex >= tabControl_DicomDirFiles.TabCount)
					oldSelectedIndex--;
				if (oldSelectedIndex < 0)
					oldSelectedIndex = 0;

				if (tabControl_DicomDirFiles.TabCount > 0)
					tabControl_DicomDirFiles.SelectTab(oldSelectedIndex);

				GC.Collect();
			}
		}

		private void tabControl_DicomDirFiles_DrawItem(object sender, DrawItemEventArgs e)
		{
			//e.DrawBackground();
			using (Brush br = new SolidBrush(Color.Azure))
			{
				e.Graphics.FillRectangle(br, e.Bounds);

				var font = new Font(e.Font.FontFamily, e.Font.Size - (float)0.3);
				var sz = e.Graphics.MeasureString(tabControl_DicomDirFiles.TabPages[e.Index].Text, font);
				sz.Width -= 20;

				var brush = Brushes.Black;
				if (tabControl_DicomDirFiles.SelectedIndex == e.Index)
					brush = Brushes.Red;
				e.Graphics.DrawString(tabControl_DicomDirFiles.TabPages[e.Index].Text, font, brush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2 - 10, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 2);

				Rectangle rect = e.Bounds;
				rect.Offset(0, 1);
				rect.Inflate(0, -1);
				e.Graphics.DrawRectangle(Pens.DarkGray, rect);
				e.DrawFocusRectangle();
			}
		}
	}
}
