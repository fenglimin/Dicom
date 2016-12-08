using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomStress
{
    class Program
    {
        static void Main(string[] args)
        {
            for (int n = 0; n < 10000; n++)
            {
                Read();
                Console.Write(".");
            }
            Console.WriteLine();
        }

        private static void Read()
        {
            DataSet dicom = new DataSet();
            string path = Path.Combine(RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm");
            dicom.Read(path);


            ushort[] pixels = (ushort[])dicom[t.PixelData].Value;
        }

        public static string RootFolder
        {
            get
            {
                string fragment = @"ImageProcessing\EK\Capture\Dicom\";
                string folder = Directory.GetCurrentDirectory();
                folder = folder.Substring(0, folder.IndexOf(fragment) + fragment.Length);
                folder += "DicomToolkit";
                return folder;
            }
        }

    }
}
