using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomTouch
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("\nDicomTouch <path> [today]|[#]");
                Console.WriteLine("alters ScheduledProcedureStepStartDate for all files at <path>.");
                Console.WriteLine("where <path> is a path name that can include a file spec, and optionally,");
                Console.WriteLine("where [today] (the actual word today) sets all dates to today, OR");
                Console.WriteLine("where [#] sets the date to a random day in the range of # days around today.");
                return;
            }

            int range = 0;
            if(args.Length > 1)
            {
                int temp;
                if(args[1].ToLower() == "today")
                {
                    range = 0;
                }
                else if(Int32.TryParse(args[1], out temp))
                {
                    range = temp;
                }
            }

            string path = Directory.GetCurrentDirectory();
            string specification = "*.dcm";
            // are we being given a directory or a filename
            if (Directory.Exists(args[0]))
            {
                path = args[0];
            }
            else
            {
                int position = args[0].LastIndexOf('\\');
                if (position > 0)
                {
                    path = args[0].Substring(0, position);
                    specification = args[0].Substring(position+1);
                }
                else
                {
                    specification = args[0];
                }
            }
            Console.WriteLine(String.Format("version={3}\npath={0}\nspecification={1}\nrange={2}", path, specification, range, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()));

            Random random = new Random();
            DirectoryInfo folder = new DirectoryInfo(path);
            foreach (FileInfo file in folder.GetFiles(specification, SearchOption.TopDirectoryOnly))
            {
                Console.WriteLine(String.Format("file={0}", file.Name));

                try
                {
                    FileStream input = new FileStream(file.FullName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
                    DataSet dicom = new DataSet();
                    dicom.Read(input);

                    Sequence sequence = dicom["(0040,0100)"] as Sequence;
                    Elements item = sequence.Items[0];
                    item["(0040,0002)"].Value = DateTime.Now.AddDays(random.Next(-range, range)).ToString("yyyyMMdd");
                    //item["(0040,0003)"].Value = String.Format("{0:00}{1:00}{2:00}.000", random.Next(0, 23), random.Next(0, 59), random.Next(0, 59));

                    input.Close();
                    input.Dispose();

                    File.Delete(file.FullName);

                    dicom.Write(file.FullName);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
