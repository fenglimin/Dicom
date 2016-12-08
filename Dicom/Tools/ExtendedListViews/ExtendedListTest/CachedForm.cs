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
using SynapticEffect.Forms;

namespace ExtendedListTest
{
    public partial class CachedForm : Form
    {
        private IDicomServiceWorkerUser dicomServiceWorkerUser;
	    private DicomServiceWorker dicomServiceWorker;

        public CachedForm(IDicomServiceWorkerUser dicomServiceWorkerUser, DicomServiceWorker dicomServiceWorker)
        {
            InitializeComponent();

            this.dicomServiceWorkerUser = dicomServiceWorkerUser;
	        this.dicomServiceWorker = dicomServiceWorker;
        }

        private void CachedForm_Load(object sender, EventArgs e)
        {
            LoadList();
        }

        private void LoadList()
        {
			foreach (var receivedDicomElements in dicomServiceWorker.ListCachedElements.Where(x => x.ImageStatus == ImageMemoryStatus.CachedInMemory))
            {
				var cachedDicomElements = dicomServiceWorker.CreateCachedFromReceived(receivedDicomElements);

                var node = new TreeListNode
                {
					Text = cachedDicomElements.ReceivedDateTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
	                AdditionalData = receivedDicomElements
                };

	            
				node.SubItems.Add(cachedDicomElements.CallingAeTitle);
				node.SubItems.Add(cachedDicomElements.IpAddress);
				node.SubItems.Add(cachedDicomElements.ImageSource.ToString());
				node.SubItems.Add(cachedDicomElements.PatientName);
				node.SubItems.Add(cachedDicomElements.PatientId);
				node.SubItems.Add(cachedDicomElements.Modality);
				node.SubItems.Add(cachedDicomElements.AccessionNumber);

                tlvCache.Nodes.Add(node);
            }
        }

        private void tlvCache_Click(object sender, EventArgs e)
        {

        }

        private void tlvCache_DoubleClick(object sender, EventArgs e)
        {


			
        }

        private void tlvCache_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void tlvCache_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var node = tlvCache.SelectedNodes[0];
            if (node == null)
                return;


            var receivedDicomElements = node.AdditionalData as ReceivedDicomElements;
            if (receivedDicomElements == null)
                return;


            if (receivedDicomElements.ImageSource == ImageSource.StorageCommitment)
            {
                var form = new StorageCommitmentForm(dicomServiceWorkerUser, dicomServiceWorker, receivedDicomElements);
                form.ShowDialog();
            }
            else
            {
                dicomServiceWorkerUser.ShowElements(receivedDicomElements);
                if (e.Button != MouseButtons.Right)
                {
                    tlvCache.Nodes.Remove(node);
                    tlvCache.Refresh();
                }
                else
                {
                    Close();
                }
            }
        }
    }
}
