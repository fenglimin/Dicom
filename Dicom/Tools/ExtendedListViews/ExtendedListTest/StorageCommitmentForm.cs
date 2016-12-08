using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest.Service;

namespace ExtendedListTest
{
	public partial class StorageCommitmentForm : Form
	{
		private DicomServiceWorker dicomServiceWorker;
		private ReceivedDicomElements receivedDicomElements;
		private IDicomServiceWorkerUser dicomServiceWorkerUser;

		public StorageCommitmentForm(IDicomServiceWorkerUser dicomServiceWorkerUser, DicomServiceWorker dicomServiceWorker, ReceivedDicomElements receivedDicomElements)
		{
			InitializeComponent();

			this.dicomServiceWorker = dicomServiceWorker;
			this.receivedDicomElements = receivedDicomElements;
			this.dicomServiceWorkerUser = dicomServiceWorkerUser;
		}

		private void btSend_Click(object sender, EventArgs e)
		{
			var port = 5040;
			int.TryParse(tbPort.Text, out port);

			dicomServiceWorker.SendStorageCommit(receivedDicomElements, port, rbSuccess.Checked);
		}

		private void btClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void StorageCommitmentForm_Load(object sender, EventArgs e)
		{
			tbAeTitle.Text = receivedDicomElements.CallingAeTitle;
			tbAeIpAddress.Text = receivedDicomElements.IpAddress;

			var failedList = dicomServiceWorker.GetStorageCommitmentReferenceDicomElements(receivedDicomElements)
					.Where(x => x.SavedToDicomDir == false && x.SavedToDisk == false).ToList();

			var hasFailed = failedList.Any();
			rbFailed.Checked = hasFailed;
			rbFailed.Enabled = hasFailed;
			rbSuccess.Enabled = hasFailed;

			foreach (var refDicomElements in failedList)
			{
				lbFailedImage.Items.Add(refDicomElements.Elements.GetSafeStringValue(t.SOPInstanceUID));
			}
		}

		private void lbFailedImage_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			var sopInstanceUid = lbFailedImage.SelectedItem.ToString();
			var elements = dicomServiceWorker.FindReceivedDicomElementsBySopInstanceUid(sopInstanceUid);
			dicomServiceWorkerUser.ShowElements(elements);
		}
	}
}
