using System;
using System.Windows.Forms;

namespace DicomViewer
{
    static class Program
    {
        // -b "C:\Users\michael\Desktop\Image Space\No VOILUT\PValues.56.dcm" "C:\Users\michael\Desktop\Image Space\No VOILUT\PValues.56.png"
        // -b "C:\Users\l438125\Desktop\Segmentation Failures\6.0.21.0000\cc_20090226100510_CH.diag.pgm" output.jpg

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