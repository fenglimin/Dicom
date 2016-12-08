using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedListTest.Service
{
	public class ReceivedDicomElements : DicomElementsBase
    {
		//public ImagePersistenceStatus ImagePersistenceStatus { get; set; }
        public bool SavedToDisk { get; set; }
        public bool SavedToDicomDir { get; set; }
        public EK.Capture.Dicom.DicomToolKit.DataSet Elements { get; set; }

        public List<string> SavedDicomDirs;
        public List<string> SavedFileNames;

		public ReceivedDicomElements()
		{
		    SavedDicomDirs = new List<string>();
            SavedFileNames = new List<string>();

		    SavedToDicomDir = false;
		    SavedToDisk = false;
		}

	    public void OnDicomDirSaved(string dicomDir)
	    {
	        SavedToDicomDir = true;
            if (!SavedDicomDirs.Contains(dicomDir))
                SavedDicomDirs.Add(dicomDir);
	    }

	    public void OnDiskSaved(string fileName)
	    {
	        SavedToDisk = true;
            if (!SavedFileNames.Contains(fileName))
                SavedFileNames.Add(fileName);
	    }

	    public bool IsSavedToDicomDir(string dicomDir)
	    {
	        return SavedDicomDirs.Contains(dicomDir);
	    }
    }
}
