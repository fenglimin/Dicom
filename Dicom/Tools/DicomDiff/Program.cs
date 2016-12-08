using System;
using System.Windows.Forms;

namespace DicomDiff
{
    static class Program
    {

        // "C:\Users\l438125\Desktop\dicom files\20050105215637_9104102333.dcm" "C:\Users\l438125\Desktop\dicom files\20051021163544_9309304104.dcm"
        // -i (0008,0005) -i "ignore.txt" "C:\Users\l438125\Desktop\dicom files\20050105215637_9104102333.dcm" "C:\Users\l438125\Desktop\dicom files\20051021163544_9309304104.dcm"

        // "C:\Users\michael\Desktop\dicom files\20050105215637_9104102333.dcm" "C:\Users\michael\Desktop\dicom files\20051021163544_9309304104.dcm"
        // -i (0008,0005) -i "ignore.txt" "C:\Users\michael\Desktop\dicom files\20050105215637_9104102333.dcm" "C:\Users\michael\Desktop\dicom files\20051021163544_9309304104.dcm"

        // -v -i (0008,0005) -i (0032,1064)(0008,0005) "..\..\..\..\DicomToolKit\Test\Data\Mwl\00001.dcm" "..\..\..\..\DicomToolKit\Test\Data\Mwl\00002.dcm"

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static int Main(string[] args)
        {
            int errorlevel = BatchProcessor.Run(args);
            if (errorlevel == -1)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
            return errorlevel;
        }
    }
}
