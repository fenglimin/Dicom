using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedListTest.Service
{
	public class CachedDicomElements : DicomElementsBase
	{
		public string PatientId { get; set; }
		public string PatientName { get; set; }
		public string Modality { get; set; }
		public string AccessionNumber { get; set; }
	}
}
