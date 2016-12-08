using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EK.Capture.Dicom.DicomToolKit;

namespace Dictionary
{
    /// <summary>
    /// Used to populate the EK.Capture.Dicom.DicomToolKit.Dictionary
    /// </summary>
    /// <remarks>Select the text from the entire "Registry of DICOM data elements" section in the "Part 6: Data Dictionary" PDF file.  
    /// Paste that text into a text file and pass the path to the text file as a program command line argument.  Several files are output.
    /// dictionary.txt contains the code that you can paste into Dictionary.cs.
    /// cleaned.txt contains the tags that were successfully extracted from the text.</remarks>
    /// rejected.txt contains lines that were ignored.
    /// retired.txt contains tags that have been retired, they are not included in the Dictionary.
    /// The file dictionary.txt WILL need to be manually altered to remove weird characters, etc.  You can look into the other files to get
    /// insight into how the resutls were produced and scan them for any glaring errors.
    class Program
    {
        // this contains all the know multiplicities, if one is missing the tag will be checked
        static string[] multiplicity = { "1", "1-n", "1-8", "2-n", "3-n", "3-3n", "4", "2", "1-2", "1-3", "2-2n", "3", "1-3", "6", "16", "1-32", "1-99", "9", "6-n"};
        static SortedDictionary<string, Tag2> tags;
        static string DicomTagPattern = @"\((?<group>[Xx0-9a-fA-f]{4}),(?<element>[0-9a-fA-f]{4})\)";
        /// <summary>
        /// The following patterns are commonly seen as complete lines in the input, 
        /// they make it easier to decide what to reject.
        /// </summary>
        static string PageNumberPattern = @"Page [0-9]+";               // Page 16
        static string DocumentPattern = @"PS 3.6-[0-9]{4}";             // PS 3.6-2009
        static string StandardPattern = @"- Standard -";                // - Standard -       
        static string TitlePattern = @"Tag Name Keyword VR VM";         // Tag Name Keyword VR VM
        static string RegistryPattern = @"^([0-9]+ Registry of DICOM)";  // 6 Registry of DICOM
        static List<string> multipletypes;

        static void Main(string[] args)
       {
            Initialize();
            // parse the text file
            Extract(args[0]);
            // combine the EK.Capture.Dicom.DicomToolKit.Dictionary and the parsed Tags
            Combine();
            // create the results files
            OutputResults();
        }

        /// <summary>
        /// Write a line of code for each tag in our results.
        /// </summary>
        static void OutputResults()
        {
            StreamWriter dictionary = new StreamWriter("dictionary.txt");
            Hashtable reasons = new Hashtable();

            foreach (KeyValuePair<string, Tag2> kvp in tags)
            {
                if (kvp.Value.Check)
                {
                    string reason = kvp.Value.Reason;
                    if (!reasons.Contains(reason))
                    {
                        reasons[reason] = 0;
                    }
                    reasons[reason] = (int)reasons[reason] + 1;
                    dictionary.WriteLine(String.Format("                // {0}", reason));
                }
                string temp = String.Format("                {0}Add(\"{1}\", \"{2}\", \"{3}\", \"{4}\", {5}, {6});",
                    kvp.Value.Check ? "//" : String.Empty, kvp.Key, kvp.Value.VR, kvp.Value.VM, kvp.Value.Description, (kvp.Value.Retired) ? "true" : "false", kvp.Value.Protected ? "true" : "false");
                dictionary.WriteLine(temp);
            }

            dictionary.WriteLine();
            foreach (DictionaryEntry entry in reasons)
            {
                dictionary.WriteLine(String.Format("{0} = {1}", entry.Key, (int)reasons[entry.Key]));
            }

            dictionary.Close();
            dictionary.Dispose();

            StreamWriter mts = new StreamWriter("multiples.txt");
            foreach (string type in multipletypes)
            {
                mts.WriteLine(type);
            }

            mts.Close();
            mts.Dispose();

        }

        /// <summary>
        /// Combines the parsed tags with the current dictionary.  Record any differences found, or existing
        /// dictionary entries that are not found in the new list.
        /// </summary>
        static void Combine()
        {
            StreamWriter differences = new StreamWriter("differences.txt");
            StreamWriter missing = new StreamWriter("missing.txt");

            // tags contains what we were able to successfully parse this time
            // compare that to the dictionary, to see what may be missing or different

            // go through the known dictionary
            foreach (Tag tag in EK.Capture.Dicom.DicomToolKit.Dictionary.Instance)
            {
                string key = tag.ToString().ToUpper();
                // if it is already known, but was not found this time, add it in and record it as missing
                if (!tags.ContainsKey(key))
                {
                    missing.WriteLine(String.Format("Tag {0} {1}", key, tag.Description));
                    tags.Add(tag.ToString().ToUpper(), new Tag2(tag));
                }
                else
                {
                    Tag2 temp = tags[key];
                    string check = temp.Check ? "*" : String.Empty;
                    // if we find any differences, record the differences
                    string difference = String.Empty;
                    if (tag.Description.Trim() != temp.Description.Trim())
                    {
                    }
                    if(tag.VR != temp.VR)
                    {
                        if (difference != String.Empty)
                            difference += " ";
                        difference += String.Format("vr {0} >> {1}", tag.VR, temp.VR);
                    }
                    if(tag.VM != temp.VM)
                    {
                        if (difference != String.Empty)
                            difference += " ";
                        difference += String.Format("vm {0} >> {1}", tag.VM, temp.VM);
                    }
                    if (difference.Length > 0)
                    {
                        differences.WriteLine(String.Format("{0}Tag {1} {2}", check, key, difference));
                    }
                }
            }

            differences.Close();
            differences.Dispose();

            missing.Close();
            missing.Dispose();
        }

        /// <summary>
        /// Scans the input file and creates two sets of tags, those that are accepted and
        /// those that are rejected.  The results are written to two text files, and the 
        /// accepted tags are also added to a collection of tags.
        /// </summary>
        /// <param name="filename">The filename to parse</param>
        static void Extract(string filename)
        {
            string line, next;

            tags = new SortedDictionary<string, Tag2>();

            StreamWriter cleaned = new StreamWriter("cleaned.txt");
            StreamWriter rejected = new StreamWriter("rejected.txt");

            StreamReader input = new StreamReader(filename);
            line = input.ReadLine();
            //as long as there are lines to read in the input file
            while ((next = input.ReadLine()) != null)
            {
                // assume that we will get a clean parse of a tag
                bool check = false;
                // we can automatically reject some common lines
                if (IsEasilyRejected(line))
                {
                    rejected.WriteLine(line);
                }
                else
                {

                    // some lines are split into several lines
                    // we need to combine partial lines until they fit the proper format
                    // the proper format is a dicom tag followed by a description and keyword, a VR, a VM, and then maybe a "RET"
                    // the tag is strings[0]
                    // "RET" may be at strings[strings.Length - 1]
                    // if the tag is not retired the VR is at strings[strings.Length - 2]
                    // the description is everything after the tag and before strings[strings.Length - 3]

                    // append each next line from the input as long as 
                    // they do not start with a DICOM tag
                    while (!Matches(DicomTagPattern, next))
                    {
                        if (next != null)
                        {
                            // ignore the next line if it is a page number or other recognizable string
                            if (IsEasilyRejected(next))
                            {
                                rejected.WriteLine(next);
                                next = input.ReadLine();
                                continue;
                            }
                        }
                        // we have had to append lines to get a complete line
                        // we need to check the output to make sure.
                        check = true;
                        // append it with a space
                        line += " ";
                        line += next;
                        next = input.ReadLine();
                        if (next == null) break;
                    }

                    // when we reach here, we may have a properly combined line

                    string key = line.Substring(0, 11).ToUpper();
                    // if the line begins with a DICOM tag
                    if (Matches(DicomTagPattern, key))
                    {
                        // try and parse what we have put together
                        Tag2 tag = Parse(line, check);
                        if (tag != null)
                        {
                            cleaned.WriteLine(line);
                            tags.Add(tag.Name.ToUpper(), tag);
                        }
                        else
                        {
                            rejected.WriteLine(line);
                        }
                    }
                    // reject everything else
                    else
                    {
                        rejected.WriteLine(line);
                    }
                }
                line = next;
            }
            // the above loop drops the last line.

            input.Close();
            input.Dispose();

            cleaned.Close();
            cleaned.Dispose();

            rejected.Close();
            rejected.Dispose();

        }

        static void Initialize()
        {
            multipletypes = new List<string>();
        }

        /// <summary>
        /// Parse a combined line into a Tag
        /// </summary>
        /// <param name="line">The line to parse</param>
        /// <returns>A Tag</returns>
        static Tag2 Parse(string line, bool check)
        {
            // we break each line into words based on whitespace
            string[] strings = line.Split(" \t".ToCharArray());

            Tag2 tag = null;
            try
            {
                tag = Tag2.Parse(strings[0].ToUpper().Replace('X', '0'));
                // retired tags are at the end of the line
                tag.Retired = (strings[strings.Length - 1] == "RET");

                tag.VM = String.Empty;
                tag.VR = String.Empty;
                tag.Description = String.Empty;

                try
                {
                    // the next part of the line looks somethign like this
                    // Related General SOP Class UID RelatedGeneralSOPClassUI D UI 1-n
                    // the spaces are generally there because of the lines being appended
                    // we have the name with spaces, the name without spaces, VR and VM
                    int index = 2;
                    tag.Description = EK.Capture.Dicom.DicomToolKit.Dictionary.ModifyWordForDescription(strings[1]);
                    string first = EK.Capture.Dicom.DicomToolKit.Dictionary.ModifyWordForEnumeration(strings[1]).ToLower();
                    // we keep adding up the description until we encounter a word that 
                    // starts with the first word, in the example above, the first word is Related
                    // and we want to stop adding up the description at RelatedGeneralSOPClassUI
                    while (!strings[index].ToLower().Contains(first))
                    {
                        tag.Description += " " + EK.Capture.Dicom.DicomToolKit.Dictionary.ModifyWordForDescription(strings[index++]);
                    }
                    tag.Check = false;
                }
                catch
                {
                    // if we encounter any problems, i.e. never match the first word, 
                    // we have to check the result
                    tag.Check = true;;
                    tag.Reason = "never matched first word";
                }

                // we have the tag, we have the description, now we just need to get the VM and VR
                // there are four things to parse, tag, description, VR, and VM
                if (strings.Length >= 4)
                {
                    // VM is the last word, or second to the last if RET is present
                    tag.VM = (tag.Retired) ? strings[strings.Length - 2] : strings[strings.Length - 1];
                    // if we do not recognize the multiplicity, check it
                    if (!multiplicity.Contains(tag.VM))
                    {
                        tag.Check = true;
                        tag.Reason = "unknown value multiplicity";
                        tag.VM = "?";
                    }
                    // VR is the second or third to last string
                    tag.VR = (tag.Retired) ? strings[strings.Length - 3] : strings[strings.Length - 2];
                    // if we do recognize it, check it
                    if (!EK.Capture.Dicom.DicomToolKit.Dictionary.DicomTypes.Contains(tag.VR))
                    {
                        tag.Check = true;
                        tag.Reason = "unknown value representation";
                        tag.VR = "?";
                    }
                    if (tag.VM != "1" && !multipletypes.Contains(tag.VR))
                    {
                        multipletypes.Add(tag.VR);
                    }
                    // if the tag can have multiple VRs, we find the word "or", and we need to check
                    for (int n = 3; n < strings.Length; n++)
                    {
                        if (strings[n] == "or")
                        {
                            tag.Check = true;
                            tag.Reason = "possible multiple VRs";
                            tag.VR = "?";
                        }
                    }
                }

                // if we are going to check the tag, we may as well give it all the text we have.
                if (tag.Check)
                {
                    tag.Description = line.Substring(12);
                }
            }
            catch
            {
                tag = null;
            }
            if (tag == null || tag.VM == String.Empty || tag.VR == String.Empty || tag.Description == String.Empty)
            {
                tag = null;
            }
            return tag;
        }

        /// <summary>
        /// Returns true if the line matches some commonly recognized useless lines
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        static bool IsEasilyRejected(string line)
        {
            return (Matches(PageNumberPattern, line) || 
                    Matches(DocumentPattern, line) || 
                    Matches(StandardPattern, line) || 
                    Matches(TitlePattern, line) || 
                    Matches(RegistryPattern, line));
        }

        /// <summary>
        /// Does the string match the pattern for a DICOM tag.
        /// </summary>
        /// <param name="pattern">The pattern to match against.</param>
        /// <param name="text">The text to match</param>
        /// <returns>Whether or not the string matches.</returns>
        static bool Matches(string pattern, string text)
        {
            if (text == null)
                return false;
            Match match = Regex.Match(text, pattern);
            return match.Success;
        }
    }

    /// <summary>
    /// A tag that can track whether it needs to be checked and the reason why
    /// </summary>
    /// <remarks>If a problem is encountered while parsing, the tag must be checked inthe output.
    /// </remarks>
    class Tag2 : Tag
    {
        bool check;
        string reason;

        public Tag2(Tag tag)
            : base(tag.Group, tag.Element, tag.VR, tag.VM, tag.Description, tag.Retired, tag.Protected)
        {
            this.check = false;
            this.reason = String.Empty;
        }

        public Tag2(short group, short element, string vr, string vm, string description, bool retired, bool phi, bool check, string reason)
            : base(group, element, vr, vm, description, retired, phi)
        {
            this.check = check;
            this.reason = reason;
        }

        public static new Tag2 Parse(string key)
        {
            return new Tag2(Tag.Parse(key));
        }

        /// <summary>
        /// Whether or not the tag should be checked.
        /// </summary>
        public bool Check
        {
            get
            {
                return check;
            }
            set
            {
                check = value;
            }
        }

        /// <summary>
        /// The reason, if known, why the tag should be checked.
        /// </summary>
        public string Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }
    }

}
