using System;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEcho
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Logging.LogLevel = LogLevel.Verbose;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}