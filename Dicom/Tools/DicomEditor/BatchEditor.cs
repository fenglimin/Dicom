using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    static class BatchEditor
    {
        internal static Dictionary<string, string> Macros = new Dictionary<string, string>()
        {
            { "NOW(#)",         "DA=Today's Date, TM=Current Time or DT=Current Date and Time. # modifies the DateTime +/-#y#m#d#h#n#s" },
            { "UID",            "New UID." },
            { "DATE",           "Random Date." },
            { "TIME",           "Random Time." },
            { "ALPHA(#)",       "Random Alpha string." },
            { "NUMERIC(#)",     "Random Numeric string." },
            { "ALPHANUMERIC(#)","Random Alphanumeric string." },
            { "ASCENDING(#)",   "An Ascending number. Milliseconds since 2012" },
            { "NULL",           "Empty out a tag." },
            { "REMOVE",         "Remove a tag, if it exists." },
        };

        static bool batch = false;
        static string input = null;
        static string output = null;
        static readonly string MacroDelimiter = "?";
        static readonly string SearchPattern = @"(?<tag>(\([0-9a-fA-f]{4},[0-9a-fA-f]{4}\)\d?)+)=(?<value>.*)";
        static readonly string DeltaPattern = @"((?<years>[-+]?\d+)y)?((?<months>[-+]?\d+)m)?((?<days>[-+]?\d+)d)?((?<hours>[-+]?\d+)h)?((?<minutes>[-+]?\d+)n)?((?<seconds>[-+]?\d+)s)?";

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>-i "..\..\..\..\DicomToolkit\Test\Data\Mwl\00004.dcm" -o "edited.dcm" (0020,000d)=?UID? (0040,0100)(0040,0002)=?NOW? (0008,0050)=?NUMERIC(8)? (0008,0050)=?ASCENDING?</remarks>
        /// <remarks>-i "..\..\..\..\DicomToolkit\Test\Data\Mwl\00004.dcm" -o "edited.dcm" -e "..\..\edits.txt"</remarks>
        public static bool Run(string[] args)
        {
            try
            {
                Dictionary<string, string> edits = Setup(args);

                if (batch)
                {
                    DataSet dicom = (input != null) ? OpenFile(input) : new DataSet();

                    EditFile(dicom, edits);

                    WriteFile(dicom, output);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Console.WriteLine(ex.Message);
            }
            return batch;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static Dictionary<string, string> Setup(string[] args)
        {
            Dictionary<string, string> edits = new Dictionary<string, string>();

            for (int n = 0; n < args.Length; n++)
            {
                string arg = args[n];
                switch (arg.ToLower())
                {
                    case "-i":
                        if (n < args.Length)
                            input = args[++n];
                        batch = true;
                        break;
                    case "-o":
                        if (n < args.Length)
                            output = args[++n];
                        batch = true;
                        break;
                    case "-e":
                        if (n < args.Length)
                            edits = LoadEdits(args[++n]);
                        batch = true;
                        break;
                    case "?":
                        Console.WriteLine(Usage);
                        break;
                    default:
                        if (ParseEdit(arg, edits))
                        {
                            batch = true;
                        }
                        break;
                }
            }
            if (batch && input == null && output == null)
            {
                throw new Exception("You must specify an input and/or output file in batch mode.");
            }

            /*
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("input={0}\n", input));
            if (output != null) text.Append(String.Format("output={0}\n", input));
            foreach (KeyValuePair<string, string> kvp in edits)
            {
                text.Append(String.Format("{0}={1}\n", kvp.Key, kvp.Value));
            }
            Console.WriteLine(text.ToString());
            //MessageBox.Show(text.ToString());
            /**/

            if (output == null)
            {
                output = input;
            }

            return edits;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicom"></param>
        /// <param name="filename"></param>
        public static void Save(DataSet dicom, String filename)
        {
            using (StreamWriter stream = new StreamWriter(filename))
            {
                foreach (Element element in dicom.InOrder)
                {
                    string value = String.Empty;
                    if (element.Value == null)
                    {
                        value = "?NULL?";
                    }
                    else if (element.Value is Array)
                    {
                        StringBuilder text = new StringBuilder();
                        foreach (object entry in element.Value as Array)
                        {
                            if (text.Length > 0)
                            {
                                text.Append(@"\");
                            }
                            text.Append(entry.ToString());
                        }
                        value = text.ToString();
                    }
                    else
                    {
                        value = element.Value.ToString();
                    }
                    stream.Write(String.Format("{0}={1}\r\n", element.Tag, value));
                }
                stream.Close();
            }
        }

        public static DataSet ProcessFile(string filename)
        {
            DataSet dicom = new DataSet();
            string current = Directory.GetCurrentDirectory();
            try
            {
                FileInfo info = new FileInfo(filename);
                Dictionary<string, string> edits = LoadEdits(filename);
                
                Directory.SetCurrentDirectory(info.DirectoryName);
                EditFile(dicom, edits);
            }
            catch
            {
                Directory.SetCurrentDirectory(current);
            }
            return dicom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        static bool ParseEdit(string text, Dictionary<string, string> edits)
        {
            bool result = false;
            Match match = Regex.Match(text, SearchPattern);
            if (match.Success)
            {
                result = true;
                string tag = (string)match.Groups["tag"].Value;
                string value = (string)match.Groups["value"].Value;
                if (tag.ToLower() == t.PixelData.ToLower() && !IsQuotedString(value))
                {
                    value = "\"" + value + "\"";
                }
                edits.Add(tag, value);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        static Dictionary<string, string> LoadEdits(string path)
        {
            Dictionary<string, string> edits = new Dictionary<string, string>();
            using (StreamReader file = new StreamReader(path))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    ParseEdit(line, edits);
                }

                file.Close();
            }
            return edits;
        }

        /// <summary>
        /// 
        /// </summary>
        public static String Usage
        {
            get
            {
                StringBuilder text = new StringBuilder();
                text.Append(String.Format(@"
DicomEditor -i input [-o output] [tag=value]+ [-e edits]
where:  input is the input file path,
        output is optional output file path, 
        tag=value are optional tag value pairs where the tag is a tag to edit and value is the value to set it to.
        edits is the path to a text file containing tag value pairs.
        ? shows this usage.
If none of the above options are specified, the editor is opened interactively.

example: DicomEditor -i untitled.dcm (0020,000d)=""?UID?""
sets the StudyInstanceUID to a new UID.

The following macros are currently supported:
"));

                foreach (KeyValuePair<string, string> kvp in BatchEditor.Macros)
                {
                    string macro = String.Format("{0}{1}{0}", MacroDelimiter, kvp.Key);
                    text.Append(String.Format("{0},{1}\n", macro, kvp.Value));
                }
                return text.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicom"></param>
        /// <param name="output"></param>
        static void WriteFile(DataSet dicom, string output)
        {
            using (FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                dicom.Write(stream);
                stream.Close();
            }
        }

        static void EditFile(DataSet dicom, Dictionary<string, string> edits)
        {
            foreach (KeyValuePair<string, string> kvp in edits)
            {
                ExpandMacros(dicom, kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        static DataSet OpenFile(string input)
        {
            DataSet dicom = new DataSet();
            using (FileStream stream = new FileStream(input, FileMode.Open, FileAccess.Read))
            {
                dicom.Read(stream);
                stream.Close();
            }
            return dicom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="vr"></param>
        /// <returns></returns>
        static object ReadBinary(string file, string vr)
        {
            object data = null;
            using (FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read))
            {
                int size = (int)stream.Length;
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Little);
                switch (vr)
                {
                    case "OB":
                        data = reader.ReadBytes(size);
                        break;
                    case "SS":
                    case "US":
                    case "OW":
                        data = reader.ReadWords(size / 2);
                        break;
                    default:
                        throw new ArgumentException(String.Format("ReadBinary: Unsupported type, {0}", vr));
                }
            }
            return data;
        }

        static bool IsQuotedString(string text)
        {
            return text.StartsWith("\"") && text.EndsWith("\"");
        }

        /// <summary>
        /// Expands any macros within the text.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="original"></param>
        /// <returns>The text with macros expanded.</returns>
        static void ExpandMacros(DataSet dicom, string key, string original)
        {
            Tag tag = Tag.Parse(key);

            if (IsQuotedString(original))
            {
                string path = original.Replace("\"", String.Empty);

                // type is only used for the ReadBinary method
                string type = tag.VR.Split(",".ToCharArray())[0];
                // TODO this is a HACK, resolve the type some better way
                switch(tag.Name)
                {
                    case "(7fe0,0010)":
                        if (dicom.Contains(t.BitsAllocated))
                        {
                            type = ((ushort)dicom[t.BitsAllocated].Value == 8) ? "OB" : "OW";
                        }
                        break;
                }

                if (File.Exists(path))
                {
                    dicom.Set(key, ReadBinary(path, type));
                    return;
                }
                else
                {
                    throw new FileNotFoundException("File not found.", path);
                }

            }

            StringBuilder text = new StringBuilder();
            int index = 0;
            try
            {
                int begin;
                int end;
                int length = original.Length;
                while (index < length)
                {
                    begin = original.Substring(index).IndexOf(MacroDelimiter);
                    if (begin != -1)
                    {
                        begin += index;
                        end = original.Substring(begin + 1).IndexOf(MacroDelimiter);
                        if (end == -1)
                        {
                            text.Append(original.Substring(index));
                            break;
                        }
                        end++;
                        text.Append(original.Substring(index, begin - index));
                        string fragment = original.Substring(begin, end);
                        string macro = fragment.Substring(1).ToLower();
                        // TODO make sure macro is in the macro list before executing

                        // as an example, we are parsing the following: something(8,2,1)
                        string[] args = null;
                        if (macro.Contains('('))
                        {
                            int pos = macro.IndexOf('(');
                            fragment = macro.Substring(pos+1);                              // 8,2,1)
                            macro = macro.Substring(0, pos);                                // something
                            if (fragment.Contains(')'))
                            {
                                fragment = fragment.Substring(0, fragment.IndexOf(')'));    // 8,2,1
                            }
                            args = fragment.Split(",".ToCharArray());                       // {8,2,1}
                        }
                        switch (macro)
                        {
                            case "uid":
                                text.Append(UidMacro(tag));
                                begin++;
                                break;
                            case "now":
                                text.Append(DateTimeMacro(tag, args));
                                begin++;
                                break;
                            case "date":
                                text.Append(Generator.GetDate());
                                begin++;
                                break;
                            case "time":
                                text.Append(Generator.GetTime());
                                begin++;
                                break;
                            case "alpha":
                                 text.Append(Generator.GetAlphaString(Int32.Parse(args[0])));
                               begin++;
                                break;
                            case "numeric":
                                text.Append(Generator.GetNumericString(Int32.Parse(args[0])));
                                begin++;
                                break;
                            case "alnum":
                                text.Append(Generator.GetAlphaNumericString(Int32.Parse(args[0])));
                                begin++;
                                break;
                            case "ascending":
                                text.Append(Generator.GetAscendingNumber());
                                begin++;
                                break;
                            case "":
                            case "null":
                                dicom.Set(key, null);
                                return;     // do not continue to parse
                            case "remove":
                                dicom.Remove(key);
                                return;     // do not continue to parse
                            default:
                                text.Append(fragment);
                                break;
                         }
                        index = begin + end;
                    }
                    else
                    {
                        text.Append(original.Substring(index));
                        break;
                    }
                }
            }
            catch
            {
                text = new StringBuilder(original);
            }
            dicom.Set(key, text.ToString());
        }

        static string UidMacro(Tag tag)
        {
            return Element.NewUid();
        }

        /// <summary>
        /// ?NOW(-1y11m14d1h1n1s)? now minus 1 year, 11 months, 14 days, 1 hour, 1 minute and 1 second
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static string DateTimeMacro(Tag tag, string[] args)
        {
            string value = null;
            DateTime dt = DateTime.Now;
            if (args != null && args.Length > 0)
            {
                Match match = Regex.Match(args[0], DeltaPattern);
                if (match.Success)
                {
                    if (match.Groups["years"].ToString() != String.Empty)
                    {
                        dt = dt.AddYears(Int32.Parse(match.Groups["years"].ToString()));
                    }
                    if (match.Groups["months"].ToString() != String.Empty)
                    {
                        dt = dt.AddMonths(Int32.Parse(match.Groups["months"].ToString()));
                    }
                    if (match.Groups["days"].ToString() != String.Empty)
                    {
                        dt = dt.AddDays(Int32.Parse(match.Groups["days"].ToString()));
                    }
                    if (match.Groups["hours"].ToString() != String.Empty)
                    {
                        dt = dt.AddHours(Int32.Parse(match.Groups["hours"].ToString()));
                    }
                    if (match.Groups["minutes"].ToString() != String.Empty)
                    {
                        dt = dt.AddMinutes(Int32.Parse(match.Groups["minutes"].ToString()));
                    }
                    if (match.Groups["seconds"].ToString() != String.Empty)
                    {
                        dt = dt.AddSeconds(Int32.Parse(match.Groups["seconds"].ToString()));
                    }
                }
                else
                {
                    throw new Exception(String.Format("Bad pattern, {0}", args[0]));
                }
            }
            switch (tag.VR)
            {
                case "DA":
                    value = dt.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "TM":
                    value = dt.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "DT":
                default:
                    value = dt.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }
            return value;
        }

    }
}
