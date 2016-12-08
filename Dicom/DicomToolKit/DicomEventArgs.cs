using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EK.Capture.Dicom.DicomToolKit
{
	public class DicomEventArgs : EventArgs
	{
		public string CallingAeTitle { get; set; }
		public IPAddress CallingAeIpAddress { get; set; }
	}
}
