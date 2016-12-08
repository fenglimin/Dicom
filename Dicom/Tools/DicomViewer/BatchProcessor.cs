using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomViewer
{
    public class BatchProcessor
    {
        static string input = null;
        static string output = null;
        static bool verbose = false;
        static bool invert = false;
        static int rotation = 0;
        static bool flip = false;

        /// <summary>
        /// Runs the conversion if configured.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>Returns -1 if not run, 1 if run successfully and a return value of 0 indicates 
        /// a problem.</returns>
        public static int Run(string[] args)
        {
            int result = -1;
            bool attached = false;
            try
            {
                if (Setup(args))
                {
                    if (verbose)
                    {
                        AttachConsole();
                        attached = true;
                    }

                    Viewer viewer = new Viewer();
                    DataSet dicom = OtherImageFormats.Read(input);
                    viewer.SaveAs(dicom, output, invert, rotation, flip);

                    result = 0;
                }
            }
            catch (Exception ex)
            {
                result = 1;
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (attached)
                {
                    FreeConsole();
                }
            }
            return result;
        }

        /// <summary>
        /// Parse command line arguments decides if batch mode should be run
        /// </summary>
        /// <param name="args"></param>
        /// <returns>True if we should run in batch mode, False otherwise.</returns>
        static bool Setup(string[] args)
        {
            bool batch = false;
            for (int n = 0; n < args.Length; n++)
            {
                string arg = args[n];
                switch (arg.ToLower())
                {
                    case "-b":
                        batch = true;
                        break;
                    case "-v":
                        batch = true;
                        verbose = true;
                        break;
                    case "-i":
                        batch = true;
                        invert = true;
                        break;
                    case "-r":
                        batch = true;
                        rotation = Int32.Parse(args[++n]) % 360;
                        break;
                    case "-f":
                        batch = true;
                        flip = true;
                        break;
                    case "?":
                        Console.WriteLine(Usage);
                        break;
                    default:
                        if (input == null)
                        {
                            input = args[n];
                        }
                        else
                        {
                            output = args[n];
                        }
                        break;
                }
            }
            if (batch)
            {
                if(output == null)
                {
                    output = input;
                }
                if (input == null) throw new Exception("You must specify an input in batch mode.");
                if (!File.Exists(input)) throw new Exception("Input file does not exist.");
                if (rotation % 90 != 0) throw new Exception("Rotation must be 0, 90, 180 or 270.");
            }
            return batch;
        }

        /// <summary>
        /// Returns the usage.
        /// </summary>
        public static String Usage
        {
            get
            {
                StringBuilder text = new StringBuilder();
                text.Append(String.Format(@"
DicomViewer -b input ouput [-v] [-r 90 | 180 | 270 ] [-f]
where:  input is a DICOM file and output is the file to create based on extension.
supported extensions are dcm,raw,bmp,jpg,tif,tiff,gif,png,emf,exif,wmf.
        If invoked with no flags, the user interface is shown.

        -b forces batch mode, any other option below implies batch mode
        -i invert the pixels before saving
        -r rotates the pixels before saving
        -f flips the images before saving
        -v writes output to the console,
        ? shows this usage.

If run in batch mode, the return code will be assigned to $ERRORLEVEL$

examples: 
DicomViewer -b ""input.dcm"" ""output.jpg"" 
Converts input to a JPEG named output.
"));
                return text.ToString();
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeConsole();

        [DllImport("kernel32", SetLastError = true)]
        static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

        private static void AttachConsole()
        {
            //get a pointer to the forground window.  The idea here is that 
            //if the user is starting our application from an existing console 
            //shell, that shell will be the uppermost window.  We'll get it 
            //and attach to it 
            IntPtr ptr = GetForegroundWindow();

            int id;
            GetWindowThreadProcessId(ptr, out id);

            Process process = Process.GetProcessById(id);
            if (process.ProcessName == "cmd")    //Is the uppermost window a cmd process? 
            {
                AttachConsole(process.Id);
            }
            else
            {
                //no console AND we're in console mode ... create a new console. 
                AllocConsole();
            }
        }
    }
}
