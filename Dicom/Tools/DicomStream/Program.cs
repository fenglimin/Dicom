using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomStream
{
    class Program
    {
        static Dictionary<string, string> macros = new Dictionary<string, string>()
        {
            { "NOW", "Date, Time or DateTime of Now." },
            { "UID", "Random UID." },
        };
        static readonly string MacroDelimiter = "%";
        static Dictionary<string, string> edits = new Dictionary<string,string>();
        static string input = null;
        static string output = null;
        static string SearchPattern = @"(?<tag>(\([0-9a-fA-f]{4},[0-9a-fA-f]{4}\)\d?)+)=(?<value>.+)";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// -i "..\..\..\..\DicomToolkit\Test\Data\Mwl\00004.dcm" -o "edited.dcm" (0020,000d)=%UID% (0040,0100)(0040,0002)=%NOW%
        /// </remarks>
        static void Main(string[] args)
        {
            try
            {
                Setup(args);

                DataSet dicom = OpenFile();

                EditFile(dicom);

                WriteFile(dicom);

            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void WriteFile(DataSet dicom)
        {
            using (FileStream stream = new FileStream(output, FileMode.Create, FileAccess.Write))
            {
                dicom.Write(stream);
                stream.Close();
            }
        }

        static void EditFile(DataSet dicom)
        {
            foreach (KeyValuePair<string, string> kvp in edits)
            {
                Tag tag = Tag.Parse(kvp.Key);
                dicom[kvp.Key].Value = ExpandMacros(tag, kvp.Value);
            }
        }

        static DataSet OpenFile()
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
        /// Expands any macros within the text.
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="original"></param>
        /// <returns>The text with macros expanded.</returns>
        static string ExpandMacros(Tag tag, string original)
        {
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
                        switch (macro)
                        {
                            case "uid":
                                text.Append(UidMacro(tag));
                                begin++;
                                break;
                            case "now":
                                text.Append(DateTimeMacro(tag));
                                begin++;
                                break;
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
            return text.ToString();
        }

        static string UidMacro(Tag tag)
        {
            return Element.NewUid();
        }

        static string DateTimeMacro(Tag tag)
        {
            string value = null;
            switch (tag.VR)
            {
                case "DA":
                    value = DateTime.Now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "TM":
                    value = DateTime.Now.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    break;
                case "DT":
                default:
                    value = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                    break;
            }
            return value;
        }

        static void ShowUsage()
        {
            Console.WriteLine(String.Format(
@"DicomStream -i input [-o output] [tag=value]+
where:  input is the input file path,
        output is optional output file path, 
        tag=value are optional tag value pairs where the tag is a tag to edit and value is the value to set it to.

example: DicomStream -i untitled.dcm (0020,000d)=""%UID%""
sets the StudyInstanceUID to a new random UID

The following macros are currently supported:
"));

            StringBuilder text = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in macros)
            {
                string macro = String.Format("{0}{1}{0}", MacroDelimiter, kvp.Key);
                text.Append(String.Format("{0,-12} {1}\n", macro, kvp.Value));
            }
            Console.WriteLine(text.ToString());
        }

        static void Setup(string[] args)
        {
            if (args.Length < 2)
            {
                ShowUsage();
                return;
            }
            for (int n = 0; n < args.Length; n++)
            {
                string arg = args[n];
                switch (arg.ToLower())
                {
                    case "-i":
                        if (n < args.Length)
                            input = args[++n];
                        break;
                    case "-o":
                        if (n < args.Length)
                            output = args[++n];
                        break;
                    default:
                        {
                            Match match = Regex.Match(arg, SearchPattern);
                            if (match.Success)
                            {
                                edits.Add((string)match.Groups["tag"].Value, (string)match.Groups["value"].Value);
                            }
                        }
                        break;
                }
            }
            if (input == null)
            {
                throw new Exception("You must specify input.");
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
            /**/

            if (output == null)
            {
                output = input;
            }

        }
    }
}
