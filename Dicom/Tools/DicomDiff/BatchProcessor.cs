using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomDiff
{
    internal class Results
    {
        public Dictionary<string, string> different = new Dictionary<string, string>();
        public Dictionary<string, bool> mismatched = new Dictionary<string, bool>();

        public int Differences
        {
            get
            {
                return different.Count;
            }
        }

        public int Mismatches
        {
            get
            {
                return mismatched.Count;
            }
        }
    }

    public class BatchProcessor
    {
        static string first = null;
        static string second = null;
        static List<string> excluded = new List<string>();
        static bool verbose = false;
        static string output = null;
        static readonly string TagPattern = @"(?<tag>(\([0-9a-fA-f]{4},[0-9a-fA-f]{4}\)\d?)+)";

        /// <summary>
        /// Runs the comparision if configured and return the result.
        /// </summary>
        /// <param name="args">The command line arguments</param>
        /// <returns>Returns -1 if not run and the number of differences otherwise.  A return value of 0 indicates 
        /// no differences found.</returns>
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

                    DataSet left = new DataSet();
                    left.Read(first);

                    DataSet right = new DataSet();
                    right.Read(second);

                    ArrayList keys = CreateMasterList(left, right);

                    RemoveExcludedTags(keys, excluded);

                    Results differences = FindDifferences(left, right, keys);

                    result = differences.Differences + differences.Mismatches;

                    if (verbose || output != null)
                    {
                        Report(differences);
                    }
                }
            }
            catch (Exception ex)
            {
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
        /// Create A list of the union of the two sets of DICOM tags.
        /// </summary>
        /// <param name="right">One set of tags.</param>
        /// <param name="left">The other set of tags.</param>
        /// <returns>A list of the combined set of tags.</returns>
        public static ArrayList CreateMasterList(DataSet right, DataSet left)
        {
            ArrayList keys = new ArrayList();

            foreach (Element element in left.InOrder)
            {
                keys.Add(element.GetPath());
            }

            foreach (Element element in right.InOrder)
            {
                string key = element.GetPath();
                if (!keys.Contains(key))
                {
                    keys.Add(element.GetPath());
                }
            }

            keys.Sort();

            return keys;
        }

        private static void RemoveExcludedTags(ArrayList keys, List<string> excluded)
        {
            foreach (string key in excluded)
            {
                List<string> matches = new List<string>();
                string expression = "^" + key.Replace(")(", ").+(").Replace("(", "\\(").Replace(")", "\\)");
                foreach (string temp in keys)
                {
                    if (Regex.Match(temp, expression).Success)
                    {
                        matches.Add(temp);
                    }
                }
                foreach (string match in matches)
                {
                    keys.Remove(match);
                }
            }
        }

        internal static Results FindDifferences(DataSet left, DataSet right, ArrayList keys)
        {
            Results differences = new Results();
            foreach (string key in keys)
            {
                if (!left.Contains(key) || !right.Contains(key)) 
                {
                    differences.mismatched.Add(key, left.Contains(key));
                }
                else if (!ElementsCompare(left[key], right[key]))
                {
                    string first = ( left[key].Value == null) ? "(null)" : left[key].Value.ToString();
                    string second = ( right[key].Value == null) ? "(null)" : right[key].Value.ToString();

                    differences.different.Add(key, String.Format("{0},{1}", first, second));
                }
            }
            return differences;
        }

        /// <summary>
        /// Compares two DICOM Elements, based on the type of the first
        /// </summary>
        /// <param name="a">One of the values to compare</param>
        /// <param name="b">The other value to compare</param>
        /// <returns>True if the elements compare, False otherwise</returns>
        public static unsafe bool ElementsCompare(Element e1, Element e2)
        {
            bool result = false;
            object a = e1.Value, b = e2.Value;
            // handle case if one or both values are null
            if (a == null || b == null)
            {
                result = a == null && b == null;
            }
            else if(e1.VR != e2.VR)
            {
                result = false;
            }
            else
            {
                int n;
                switch (e1.VR)
                {
                    case "OB":
                    case "OW":
                        {
                            Array aa = a as Array;
                            Array ab = b as Array;
                            // TODO this avoids, but does not handle Encapsulated PixelData
                            if (aa != null && ab != null && aa.Length == ab.Length)
                            {

                                int length = aa.Length;

                                // each array could be either byte[] or short[], so we try and fix what we can
                                byte[] baa = a as byte[];
                                byte[] bab = b as byte[];
                                short[] saa = a as short[];
                                short[] sab = b as short[];
                                fixed(short* psa = saa, psb = sab)
                                fixed (byte* pba = baa, pbb = bab)
                                {
                                    // compare the two arrays byte by byte
                                    byte* pa = (pba != null) ? pba : (byte*)psa, pb = (pbb != null) ? pbb : (byte*)psb;
                                    for (n = 0; n < length; n++)
                                    {
                                        if (*pa++ != *pb++)
                                        {
                                            break;
                                        }
                                    }
                                    result = (n >= length);
                                }
                            }
                        }
                        break;
                    case "OF":
                        {
                            float[] a1 = a as float[];
                            float[] b2 = b as float[];
                            if (a1.Length == b2.Length)
                            {
                                int length = a1.Length;
                                fixed (float* pa = a1, pb = b2)
                                {
                                    float* pa1 = pa, pb1 = pb;
                                    for (n = 0; n < length; n++)
                                    {
                                        if (*pa1++ != *pb1++)
                                        {
                                            break;
                                        }
                                    }
                                    result = (n >= length);
                                }
                            }
                        }
                        break;
                    default:
                        if ((e1.Value is Array && ((Array)e1.Value).Length > 64) || (e2.Value is Array && ((Array)e2.Value).Length > 64))
                        {
                            Array a1 = e1.Value as Array;
                            Array a2 = e2.Value as Array;
                            if (a1 == null)
                            {
                            }
                            if (a2 == null)
                            {
                            }
                            if (a1.Length == a2.Length)
                            {
                                int length = a1.Length;
                                for (n = 0; n < length; n++)
                                {
                                    if (a1.GetValue(n).ToString() != a2.GetValue(n).ToString())
                                    {
                                        break;
                                    }
                                }
                                result = (n >= length);
                            }
                        }
                        else
                        {
                            result = e1.ToString() == e2.ToString();
                        }
                        break;
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
                    case "-i":
                        if (n < args.Length)
                        {
                            ParseIgnore(args[++n]);
                        }
                        batch = true;
                        break;
                    case "-o":
                        if (n < args.Length)
                        {
                            output = args[++n];
                        }
                        batch = true;
                        break;
                    case "-v":
                        verbose = true;
                        break;
                    case "?":
                        Console.WriteLine(Usage);
                        break;
                    default:
                        if (first == null)
                        {
                            first = args[n];
                        }
                        else
                        {
                            second = args[n];
                        }
                        break;
                }
            }
            if (batch && (first == null || second == null))
            {
                throw new Exception("You must specify two input files in batch mode.");
            }
            return batch;
        }

        static void Report(Results differences)
        {
            TextWriter writer = null;
            try
            {
                if (verbose)
                {
                    writer = Console.Out;
                }
                if (output != null)
                {
                    try
                    {
                        writer = new StreamWriter(output, false, Encoding.Default);
                    }
                    // exception could be thrown if folder does not exist
                    catch
                    {
                        writer = Console.Out;
                    }
                }

                ReportDifferences(writer, differences);
                writer.WriteLine();
                ReportMismatches(writer, differences);
                writer.WriteLine();
                ReportExcludedTags(writer);
            }
            finally
            {
                writer.Flush();
            }
       }

        static void ReportDifferences(TextWriter writer, Results differences)
        {
            writer.WriteLine(String.Format("Differences {0} tag(s)", differences.Differences));
            foreach (string key in differences.different.Keys)
            {
                Tag tag = Tag.Parse(key);
                writer.WriteLine(String.Format("{0}:{1}\n\t{2}", tag.Description, tag.ToString(), differences.different[key]));
            }
        }

        static void ReportMismatches(TextWriter writer, Results differences)
        {
            writer.WriteLine(String.Format("Mismatches {0} tag(s)", differences.Mismatches));
            foreach (KeyValuePair<string, bool> kvp in differences.mismatched)
            {
                Tag tag = Tag.Parse(kvp.Key);
                writer.WriteLine(String.Format("{0} {1}:{2}", kvp.Value ? "<<<" : ">>>", tag.Description, tag.ToString()));
            }
        }

        static void ReportExcludedTags(TextWriter writer)
        {
            writer.WriteLine(String.Format("Excluded {0} tag(s)", excluded.Count));
            foreach (string key in excluded)
            {
                Tag tag = Tag.Parse(key);
                writer.WriteLine(String.Format("{0}:{1}", tag.Description, tag.ToString()));
            }
        }

        /// <summary>
        /// Add a valid tag or a tags in an exclude file.
        /// </summary>
        /// <param name="text">The text to test if valid tag or valid tag exclude file.</param>
        static void ParseIgnore(string text)
        {
            if(!AddExcluded(text) && File.Exists(text))
            {
                AppendExcludedTags(text);
            }
        }

        /// <summary>
        /// Test string and add it to excluded tags if valid tag pattern
        /// </summary>
        /// <param name="key">The string to test</param>
        /// <returns>True if valid and added, False otherwise.</returns>
        static bool AddExcluded(string key)
        {
            bool result = false;
            Match match = Regex.Match(key, TagPattern);
            if (match.Success && !excluded.Contains(key))
            {
                excluded.Add(key);
                result = true;
            }
            return result;
        }

        /// <summary>
        /// Opens exclude file and adds any valid tags therein.
        /// </summary>
        /// <param name="path">The path to an exclude file.</param>
        static void AppendExcludedTags(string path)
        {
            using (StreamReader file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    AddExcluded(line);
                }
                file.Close();
            }
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
DicomDiff first second [-b] [-i tag | -i file]* [-o output] [-v]
where:  first and second are paths to the files to compare,
        If invoked with no other arguments, the user interface is shown.

        -b forces batch mode, any other option below implies batch mode
        -i tag, ignores a single DICOM tag (can be used many times),
        -i file, ignores all the tags listed in file
        -o writes output to a file,
        -v writes output to the console,
        ? shows this usage.

If run in batch mode, the return code will be assigned to $ERRORLEVEL$

examples: 
DicomDiff ""first.dcm"" ""second.dcm"" 
launches the UI comparing two files.

DicomDiff -b ""first.dcm"" ""second.dcm"" 
compares two files in batch mode

DicomDiff -b ""first.dcm"" ""second.dcm"" -i (7fe0,0010)
compares two files in batch mode, ignoring tag (7fe0,0010)
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
            if (process.ProcessName == "cmd" )    //Is the uppermost window a cmd process? 
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
