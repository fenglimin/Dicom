using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomXml
{
    class Program
    {
        static void Main(string[] args)
        {
            FileStream input = null;
            try
            {
                input = new FileStream(args[0], FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                DataSet dicom = new DataSet();
                dicom.Read(input);

                StreamWriter writer = new StreamWriter(args[0] + ".xml");
                writer.Write(dicom.ToXml());    
            
                writer.Flush();
                writer.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Unable to parse the dicom file, " + ex.Message);
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                    input.Dispose();
                    input = null;
                }
            }
        }
    }
}
