using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Reflection;
using EK.Capture.Dicom.DicomToolKit.Properties;

namespace EK.Capture.Dicom.DicomToolKit
{
    internal class Tag2 : Tag
    {
        string vt;
        string dependency;
        //string @default;

        public Tag2(Tag tag)
            : base(tag.Group, tag.Element, tag.VR, tag.VM, tag.Description, tag.Retired, tag.Protected)
        {
            this.vt = "3";
            this.dependency = String.Empty;
            //this.@default = String.Empty;
        }

        public Tag2(short group, short element, string vr, string vm, string description, bool retired, bool phi, bool check)
            : base(group, element, vr, vm, description, retired, phi)
        {
            this.vt = "3";
            this.dependency = String.Empty;
            //this.@default = String.Empty;
        }

        public static new Tag2 Parse(string key)
        {
            return new Tag2(Tag.Parse(key));
        }

        public string VT
        {
            get
            {
                return vt;
            }
            set
            {
                vt = value;
            }
        }

        public string Dependency
        {
            get
            {
                return dependency;
            }
            set
            {
                dependency = value;
            }
        }

        //public string Default
        //{
        //    get
        //    {
        //        return @default;
        //    }
        //    set
        //    {
        //        vt = @default;
        //    }
        //}
    }

    public class Iod
    {
        static Dictionary<string, Dictionary<string, Tag2>> cache = new Dictionary<string, Dictionary<string, Tag2>>();
        static XmlDocument document = null;
        static bool assist = false;

        public static bool AddMissingType2Tags
        {
            get
            {
                return assist;
            }
            set
            {
                assist = value;
            }
        }

        public static string Xml
        {
            get
            {
                CheckDocument();
                return document.OuterXml;
            }
            set
            {
                if (value == null)
                {
                    document = null;
                }
                else
                {
                    try
                    {
                        cache.Clear();
                        document = new XmlDocument();
                        document.LoadXml(value);
                    }
                    catch
                    {
                        document = null;
                        throw;
                    }
                }
            }
        }

        public static DataSet Build(string iod)
        {
            Dictionary<string, Tag2> tags = GetTags(iod);

            DataSet elements = new DataSet();
            foreach (Tag2 tag in tags.Values)
            {
                elements.Add(tag.ToString(), null);
            }
            return elements;
        }

        public static bool Verify(Elements elements)
        {
            bool result = false;
            try
            {
                if (elements.Contains(t.SOPClassUID))
                {
                    // find the iod that matches the SOPClassUID
                    string sopclass = (string)elements[t.SOPClassUID].Value;
                    string field = null;
                    // find the name of the SOPClassUID
                    Type classes = typeof(SOPClass);
                    foreach (FieldInfo info in classes.GetFields())
                    {
                        if (sopclass == info.GetValue(null).ToString())
                        {
                            field = info.Name;
                            break;
                        }
                    }
                    if (field != null)
                    {
                        CheckDocument();
                        string path = String.Format(@"dicom/iod[contains(@classes,'{0}')]", field);
                        // navigate to an IOD and iterate the includes found there
                        XmlNode node = document.SelectSingleNode(path);
                        if (node != null)
                        {
                            Logging.Log(LogLevel.Verbose, "\nValidating an IOD of {0}", field);
                            string iod = node.Attributes["name"].Value;
                            result = Verify(elements, iod);
                            Logging.Log(LogLevel.Verbose, "Validation of IOD {0}", result ? "success!" : "failed!");
                        }
                        else
                        {
                            Logging.Log(LogLevel.Error, "Unable to find a matching IOD for {0}, unable to verify.", field);
                        }
                    }
                    else
                    {
                        Logging.Log(LogLevel.Error, "Unsupported SOPClassUID {0}, unable to verify", sopclass);
                    }
                }
                else
                {
                    Logging.Log(LogLevel.Error, "Missing SOPClassUID, unable to verify.");
                }
            }
            catch (Exception ex)
            {
                result = false;
                Logging.Log(LogLevel.Error, ex.Message);
            }
            finally
            {
            }
            return result;
        }

        public static bool Verify(Elements elements, string iod)
        {
            return Verify(elements, iod, null);
        }

        public static bool Verify(Elements elements, string iod, Elements missing)
        {
            Dictionary<string, Tag2> tags = GetTags(iod);

            //Console.WriteLine("\n");

            bool results = true;
            string ignore = null;
            foreach (string key in tags.Keys)
            {
                Tag2 tag = tags[key];

                if (ignore != null)
                {
                    if (tag.ToString().StartsWith(ignore))
                    {
                        continue;
                    }
                    else
                        ignore = null;
                }

                bool exists = elements.Contains(key);
                switch (tag.VT)
                {
                    case "1":
                    case "1C":
                        if (tag.VT == "1C" && !Depends(elements, tag.Dependency))
                        {
                            if (tag.VR == "SQ")
                            {
                                ignore = key;
                            }
                            break;
                        }
                        // must exist and not be null
                        if (!exists || elements[key].Value == null)
                        {
                            if (missing != null)
                            {
                                missing.Add(key, null);
                            }
                            Logging.Log(LogLevel.Verbose, "Missing type {0} tag {1} {2}", tag.VT, key, tag.Description);
                            results = false;
                        }
                        break;
                    case "2":
                    case "2C":
                        if (tag.VT == "2C" && !Depends(elements, tag.Dependency))
                        {
                            break;
                        }
                        // must exist, but can be null
                        if (!exists)
                        {
                            if (assist)
                            {
                                // if it does not exist, we add it with a null value
                                elements.Add(key, null);
                            }
                            else
                            {
                                if (missing != null)
                                {
                                    missing.Add(key, null);
                                }
                                Logging.Log(LogLevel.Verbose, "Missing type {0} tag {1} {2}", tag.VT, key, tag.Description);
                            }
                        }
                        break;
                        // totally optional
                    case "3":
                        // if an optional sequence does not exist, ignore its contents
                        if(!exists && tag.VR == "SQ")
                        {
                            ignore = key;
                        }
                        break;
                }
            }
            return results;
        }

        private static bool Depends(Elements elements, string dependency)
        {
            bool depends = false;
            if(dependency != String.Empty)
            {
                Match match = Regex.Match(dependency, @"(?<tag>\(([x0-9a-fA-f]{4}),([0-9a-fA-f]{4})\))=(?<expression>(.)*)");
                if (match.Success)
                {
                    string key = match.Groups["tag"].ToString();
                    string expression = match.Groups["expression"].ToString();
                    bool exists = elements.Contains(key);
                    //Console.WriteLine("{0} exists={1} {2}", dependency, exists.ToString(), (exists) ? elements[key].ToString() : String.Empty);
                    //(0020,0037)=!
                    // if the dependency is not supposed to exist, then we are good if the element does not exist
                    if (expression == "!")
                    {
                        depends = !exists;
                    }
                    // if the dependant element exists, we must match it against the expression
                    else if(exists)
                    {
                        //(0028,0120)=...
                        // if the dependant element has a non-null value
                        if (expression == "...")
                        {
                            depends = elements[key].Value != null;
                        }
                        //(0028,0101)=6:16
                        // if the dependant element should fall into a range
                        else if (expression.Contains(":"))
                        {
                            string[] values = expression.Split(":".ToCharArray());
                            if (values.Length == 2)
                            {
                                // the value should/could be any numeric type, so we get into a string first, then double
                                double value = Double.Parse(elements[key].Value.ToString());
                                double v0 = Double.Parse(values[0].ToString());
                                double v1 = Double.Parse(values[1].ToString());

                                double min = Math.Min(v0, v1);
                                double max = Math.Max(v0, v1);

                                if (min <= value && value <= max)
                                {
                                    depends = true;
                                }
                            }
                        }
                        //(0018,1700)=RECTANGULAR|CIRCULAR
                        // if the dependant element contains a certain value or values
                        else
                        {
                            if (elements[key].Value != null)
                            {
                                string value = elements[key].ToString();
                                string[] values = expression.Split("|".ToCharArray());
                                for (int n = 0; n < values.Length; n++)
                                {
                                    if (value == values[n])
                                    {
                                        depends = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (!exists)
                    {
                        // the dependant element does not exist
                        // the dependant element does not fall into a range
                        // the dependant element does not contain a certain value or values
                        depends = (expression == "!");
                    }
                }
            }
            return depends;
        }

        private static void CheckDocument()
        {
            if (document == null)
            {
                cache.Clear();
                System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Resources));
                string xml = (string)resources.GetObject("dicom");

                document = new XmlDocument();
                document.LoadXml(xml);
            }
        }

        internal static Dictionary<string, Tag2> GetTags(string iod)
        {
            Dictionary<string, Tag2> tags = null;
            lock (cache)
            {
                CheckDocument();
                if(!cache.ContainsKey(iod))
                {
                    tags = new Dictionary<string, Tag2>();
                    cache.Add(iod, tags);

                    string path = String.Format(@"dicom/iod[@name='{0}']/include", iod);
                    // navigate to an IOD and iterate the includes found there
                    XmlNodeList nodes = document.SelectNodes(path);

                    Logging.Log(LogLevel.Verbose, "Loading definition of {0} ...", iod);

                    foreach (XmlNode node in nodes)
                    {
                        BuildIncludes(node, tags, null);
                    }
                }
                else
                {
                    tags = cache[iod];
                }
            }
            return tags;
        }

        private static string Depth(Tag tag)
        {
            string depth = "";
            while (tag != null && tag.Parent != null)
            {
                depth += "\t";
                tag = tag.Parent;
            }
            return depth;
        }

        private static void BuildIncludes(XmlNode node, Dictionary<string, Tag2> tags, Tag2 parent)
        {
            string depth = Depth(parent);
            //Console.WriteLine("{0}include {1}", depth, node.Attributes["name"].Value);

            string path = String.Format(@"dicom/*[@name='{0}']", node.Attributes["name"].Value);
            // navigate to another place in the document and iterate the elements and includes found there
            XmlNode include = node.OwnerDocument.SelectSingleNode(path);
            
            BuildElements(include.ChildNodes, tags, parent);

            //Console.WriteLine("{0}end", depth);
        }

        private static void BuildElements(XmlNodeList nodes, Dictionary<string, Tag2> tags, Tag2 parent)
        {
            string depth = Depth(parent);
            // iterate the node of elements, expanding sequences and includes
            foreach (XmlNode node in nodes)
            {
                string key;
                bool contains;
                switch (node.Name)
                {
                    case "element":
                        key = node.Attributes["tag"].Value.ToLower();
                        contains = tags.ContainsKey(key);
                        //Console.WriteLine("{0}{1}{2},{3} - {4} - {5}", depth, contains ? "*" : "", key, node.Attributes["vt"].Value, Dictionary.Instance[key].Description, (node.Attributes["dependency"] != null) ? node.Attributes["dependency"].Value : "");
                        // different modules may contain the same tag
                        if (!contains)
                        {
                            // if we have not seen this tag before, add it
                            Tag2 temp = new Tag2(Dictionary.Instance[key]);
                            temp.Parent = parent;
                            temp.VT = node.Attributes["vt"].Value;
                            if(node.Attributes["dependency"] != null)
                                temp.Dependency = node.Attributes["dependency"].Value;
                            tags.Add(key, temp);
                        }
                        else
                        {
                            // otherwise we want to keep track of the lowest vt and associated dependency
                            Tag2 temp = tags[key];
                            string vt = Min(temp.VT, node.Attributes["vt"].Value);
                            if (temp.VT != vt)
                            {
                                temp.VT = vt;
                                if (node.Attributes["dependency"] != null)
                                {
                                    temp.Dependency = node.Attributes["dependency"].Value;
                                }
                            }
                            else if (temp.Dependency != String.Empty && node.Attributes["dependency"] != null && temp.Dependency != node.Attributes["dependency"].Value)
                            {
                                Logging.Log(LogLevel.Warning, "Multiple dependencies for {0}", key);
                            }
                        }
                        break;
                    case "sequence":
                        key = node.Attributes["tag"].Value.ToLower();
                        contains = tags.ContainsKey(key);
                        //Console.WriteLine("{0}{1}{2},{3} - {4} - {5}", depth, contains ? "*" : "", key, node.Attributes["vt"].Value, Dictionary.Instance[key].Description, (node.Attributes["dependency"] != null) ? node.Attributes["dependency"].Value : "");
                        if (!contains)
                        {
                            Tag2 temp = new Tag2(Dictionary.Instance[key]);
                            temp.Parent = parent;
                            temp.VT = node.Attributes["vt"].Value;
                            if (node.Attributes["dependency"] != null)
                                temp.Dependency = node.Attributes["dependency"].Value;
                            tags.Add(key, temp);
                            BuildElements(node.ChildNodes, tags, temp);
                        }
                        //Console.WriteLine("{0}{1}end", depth, contains ? "*" : "");
                        break;
                    case "include":
                        BuildIncludes(node, tags, parent);
                        break;
                }
            }
        }

        static string Min(string first, string second)
        {
            if (String.Compare(first, second, false, CultureInfo.InvariantCulture) > 0)
                return second;
            return first;
        }

        /// <summary>
        /// Read MIMDicomTemplate from the project folder, and create the dicom.xml resource file
        /// </summary>
        internal static void Convert()
        {
            XPathDocument mim = new XPathDocument(@"..\..\MIMDicomTemplate.xml");

            XslCompiledTransform xsl = new XslCompiledTransform();
            xsl.Load(@"..\..\MIMDicomTemplate.xsl");

            XmlTextWriter writer = new XmlTextWriter(@"..\..\dicom.xml", null);

            xsl.Transform(mim, null, writer);

        }
    }
}
