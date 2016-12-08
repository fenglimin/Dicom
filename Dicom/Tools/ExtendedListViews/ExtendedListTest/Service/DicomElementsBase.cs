using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExtendedListTest.Service
{
	public abstract  class DicomElementsBase
	{
		public IPAddress CallingAeIpAddress { get; set; }
		public string CallingAeTitle { get; set; }
		public string FileName { get; set; }
		public DateTime ReceivedDateTime { get; set; }
		public ImageSource ImageSource { get; set; }
		public ImageMemoryStatus ImageStatus { get; set; }

		public string IpAddress
		{
			get
			{
				var temp = CallingAeIpAddress.ToString();
				var token = temp.LastIndexOf(':');
				return temp.Substring(token + 1);
			}

		}
	}
}
