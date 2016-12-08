using System;
using System.Windows.Forms;

namespace DicomEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // -i Examples\example.dcm -o new02.dcm -e Examples\edits.txt
            // -b -i "..\..\..\..\DicomToolkit\Test\Data\Mwl\00004.dcm" (0020,000d)=?UID? (0040,0100)(0040,0002)=?NOW? (0008,0050)=?RANDOM?"
            // -b -e "text.txt" -o "output.dcm"
            // run in batch mode if the command arguments indicate
            if(!BatchEditor.Run(args))
            {
                // otherwise run as a WinForms editor
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }
        }
    }
}