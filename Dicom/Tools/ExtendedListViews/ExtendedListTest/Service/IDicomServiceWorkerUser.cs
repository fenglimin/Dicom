using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EK.Capture.Dicom.DicomToolKit;

namespace ExtendedListTest.Service
{
    public interface IDicomServiceWorkerUser
    {
        string GetAtTitle();
        int GetPort();
        string GetActiveStorage();

	    bool OpenWhenReceived();
        bool SaveWhenReceived();

		void ShowElements(ReceivedDicomElements receivedDicomElements);
	    void OnDicomElementsReceived(ReceivedDicomElements receivedDicomElements);
		void OnDicomDirSaved(ReceivedDicomElements receivedDicomElements, string dicomDir);

        void ShowMessage(string message, bool hasError, bool isShowMessageBox);
    }
}