using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace BufferFix
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Truncates the image buffer to size specified internally.");
                Console.WriteLine("usage:BufferFix input [output]");
                Console.WriteLine("If output is not provided, the DICOM file will be updated in place.");
            }

            FileStream input = null;
            FileStream output = null;
            try
            {
                input = new FileStream(args[0], FileMode.Open, FileAccess.Read);
                DataSet dicom = new DataSet();
                if (!dicom.Read(input))
                {
                    Console.WriteLine("Failed to parse.");
                }

                input.Close();
                input.Dispose();
                input = null;

                ushort rows = (ushort)dicom[t.Rows].Value;
                ushort columns = (ushort)dicom[t.Columns].Value;
                ushort size = (ushort)((ushort)dicom[t.BitsAllocated].Value / (ushort)8);
                short[] pixels = (short[])dicom[t.PixelData].Value;

                int expected = rows * columns;
                if (expected < pixels.Length)
                {
                    short[] temp = new short[expected];
                    System.Array.Copy(pixels, temp, expected);
                    dicom[t.PixelData].Value = temp;

                    string target = (args.Length == 1)?args[0]:args[1];
                    output = new FileStream(target, FileMode.Create, FileAccess.Write);
                    dicom.Write(output);
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (input != null)
                {
                    input.Close();
                    input.Dispose();
                }
                if (output != null)
                {
                    output.Close();
                    output.Dispose();
                }
            }
        }
    }
}
