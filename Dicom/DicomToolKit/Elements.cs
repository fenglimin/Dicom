using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents a collection of Dicom <see cref="Element">Data Elements</see>.
    /// </summary>
    public class Elements : DicomObject, IEnumerable<Element>, ICloneable
    {
        Dictionary<string, Element> elements = new Dictionary<string, Element>();
        Element parent = null;
        private DataSetOptions options;

        internal bool GroupLengths
        {
            get
            {
                return options.GroupLengths;
            }
            set
            {
                options.GroupLengths = value;
            }
        }

        internal bool UndefinedSequenceLength
        {
            get
            {
                return options.UndefinedSequenceLength;
            }
            set
            {
                options.UndefinedSequenceLength = value;
            }
        }

        internal bool UndefinedItemLength
        {
            get
            {
                return options.UndefinedItemLength;
            }
            set
            {
                options.UndefinedItemLength = value;
            }
        }

        public Element Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if (parent != null)
                {
                    throw new Exception("Cannot set a new parent.");
                }
                else
                {
                    parent = value;
                }
            }
        }

        /// <summary>
        /// Constructs an empty Elements collection.
        /// </summary>
        public Elements()
            : this(null)
        {
        }

        /// <summary>
        /// Constructs an empty Elements collection belonging to a parent sequence item.
        /// </summary>
        public Elements(Element parent)
        {
            this.parent = parent;
        }

	    public string GetSafeString(string key)
	    {
		    var element = Resolve(key.ToLower());
		    if (element == null)
			    return string.Empty;

		    if (element.Value == null)
			    return string.Empty;

		    return element.Value.ToString();
	    }

        /// <summary>
        /// Creates and adds a new Element and initializes the Value.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to Add.</param>
        /// <param name="value">The value to initialize the Element to.</param>
        /// <returns>The newly created Element</returns>
        /// <remarks>The VR of the Element is UN if not in the Dictionary.</remarks>
        public Element Add(string key, object value)
        {
            Tag tag = Tag.Parse(key.ToLower());
            Element element = new Element(tag.Group, tag.Element, null, value);

            // add the new element to the proper parent
            Elements parent = (tag.Parent != null) ? GetParent(this, tag.Parent) : this;
            parent.Add(element);

            return element;
        }

        private Elements GetParent(Elements container, Tag parent)
        {
            Elements elements = null;
            string key = parent.ToString();
            if (!container.Contains(key))
            {
                Elements temp = null;
                if (parent.Parent != null)
                {
                    temp = GetParent(container, parent.Parent);
                }
                else
                {
                    temp = container;
                }
                Sequence sequence = (Sequence)temp.Add(new Sequence(parent.Group, parent.Element));
                elements = sequence.NewItem();
            }
            else
            {
                Sequence sequence = container[key] as Sequence;
                if (sequence.Items.Count == 0)
                {
                    elements = sequence.NewItem();
                }
                else
                {
                    elements = sequence.Items[0];
                }
            }
            return elements;
        }

        /// <summary>
        /// Adds an Element to the collection.
        /// </summary>
        /// <param name="element">The Element to add.</param>
        /// <returns>The Element added.</returns>
        public Element Add(Element element)
        {
            element.Parent = parent;
            elements.Add(Tag.ToString(element), element);
            return element;
        }

        /// <summary>
        /// Sets the value of an element, adds it if it does not exist.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to set or add.</param>
        /// <param name="value">The value to set the Element to.</param>
        public void Set(string key, object value)
        {
            string temp = key.ToLower();
            if (!Contains(temp))
            {
                Add(temp, value);
            }
            else
            {
                this[temp].Value = value;
            }
        }

        /// <summary>
        /// Removes an Element from the collection.
        /// </summary>
        /// <param name="element">The Element to Remove.</param>
        public void Remove(Element element)
        {
            Remove(element.Tag.ToString());
        }

        /// <summary>
        /// Removes an Element from the DataSet.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to Remove.</param>
        public void Remove(string key)
        {
            elements.Remove(key.ToLower());
        }

        /// <summary>
        /// The count of Elements in the collection.
        /// </summary>
        public int Count
        {
            get
            {
                return elements.Count;
            }
        }

        /// <summary>
        /// The size in bytes of the collection if written.
        /// </summary>
        /// <param name="syntax"></param>
        /// <returns></returns>
        public ulong GetSize(string syntax)
        {
            ulong size = 0;
            SpecificCharacterSet encoding = SpecificCharacterSet.Default;
            if (Contains(t.SpecificCharacterSet))
            {
                object text = this[t.SpecificCharacterSet].Value;
                encoding = new SpecificCharacterSet(text);
            }
            foreach (KeyValuePair<string, Element> pair in elements)
            {
                uint temp = pair.Value.GetSize(syntax, encoding);
                //System.Diagnostics.Debug.WriteLine(String.Format("\t{0}:{1}:{2}:{3}", pair.Value.Tag.ToString(), pair.Value.VR, temp, pair.Value.ToString()));
                size += (ulong)temp;
            }
            //System.Diagnostics.Debug.WriteLine(String.Format("{0}", size));
            return size;
        }

        /// <summary>
        /// Indexer into the collection of Elements.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>The Element represented by key.</returns>
        /// <remarks>This returns any Element from the DataSet including File Meta Elements.</remarks>
        public Element this[string key]
        {
            get
            {
                Element element = Resolve(key.ToLower());
                if (element == null)
                {
                    throw(new KeyNotFoundException(String.Format("Key not found, {0}", key)));
                }
                return element;
            }
            set
            {
                elements[key.ToLower()] = value;
            }
        }

        internal bool ContainsKey(string key)
        {
            return elements.ContainsKey(key.ToLower());
        }

        /// <summary>
        /// Whether or not the Element exists in the collection.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>True if the Element is in the collection.</returns>
        /// <remarks>The format of the string is [(group,element)item]*, where (group,element) is a
        /// tag, and item is an item number of a sequence.</remarks>
        public bool Contains(string key)
        {
            return Resolve(key.ToLower()) != null;
        }

        /// <summary>
        /// Whether or not the Element exists in the collection and has a non-null Value.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>True if the Element is in the collection.</returns>
        /// <remarks>See <see cref="Elements.Contains" for a description of the syntax for the key./></remarks>
        public bool ValueExists(string key)
        {
            bool result = false;
            Element element = Resolve(key.ToLower());
            if (element != null)
            {
                // is it a sequence, or a tag with a value.
                result = (element.VR == "SQ" || (element.Value != null && element.Length > 0));
            }
            return result;
        }

        /// <summary>
        /// Finds the Element in the collection with a tag of key.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>The Element if found or null.</returns>
        internal Element Resolve(string key)
        {
            Element element = null;

            if (key.Length <= "(0000,0000)".Length)
            {
                if (elements.ContainsKey(key))
                {
                    element = elements[key];
                }
            }
            else
            {
                MatchCollection matches = Regex.Matches(key, Tag.SearchPattern);

                Elements els = this;
                for (int n = 0; n < matches.Count - 1; n++)
                {
                    Match match = matches[n];
                    short g = short.Parse(match.Groups["group"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                    short e = short.Parse(match.Groups["element"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                    Tag tag = new Tag(g, e);
                    string name = tag.ToString();

                    if (els.ContainsKey(name))
                    {
                        Sequence sequence = (Sequence)els[name];
                        if (sequence.VR == "SQ")
                        {
                            int index = 0;
                            string temp = match.Groups["item"].ToString();
                            if (temp != null && temp.Length > 0)
                            {
                                index = Int32.Parse(temp);
                            }
                            if (sequence.Items.Count <= index)
                            {
                                els = null;
                                Logging.Log(String.Format("{0}, item index {1} out of bounds.", sequence.Tag.ToString(), index));
                                break;
                            }
                            else
                            {
                                els = sequence.Items[index];
                                continue;
                            }
                        }
                        else
                        {
                            els = null;
                            Logging.Log(String.Format("{0} not a sequence.", sequence.Tag.ToString()));
                            break;
                        }
                    }
                    els = null;
                    break;
                }
                if (els != null)
                {
                    Match match = matches[matches.Count - 1];
                    short g = short.Parse(match.Groups["group"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                    short e = short.Parse(match.Groups["element"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                    Tag tag = new Tag(g, e);
                    string name = tag.ToString();
                    
                    if (els.ContainsKey(name))
                    {
                        element = els[name];
                    }
                }
            }

            return element;
        }

        /// <summary>
        /// An enumerator for the entire collection of Elements, including sequences, in ascending tag order.
        /// </summary>
        public IEnumerable<Element> InOrder
        {
            get
            {
                // enumerating over the hashtable gives in keys in hash order
                // and we want the keys in ascending order so we sort the keys first
                ArrayList keys = new ArrayList(elements.Keys);
                keys.Sort();
                // then we iterate over the sorted keys
                foreach (string key in keys)
                {
                    Element element = elements[key] as Element;
                    if (element is Sequence)
                    {
                        foreach (Element child in ((Sequence)element).InOrder)
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return element;
                    }
                }
            }
        }

        /// <summary>
        /// Returns an enumerator for the collection of toplevel Elements in ascending tag order.
        /// </summary>
        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            // enumerating over the hashtable gives in keys in hash order
            // and we want the keys in ascending order so we sort the keys first
            ArrayList keys = new ArrayList(elements.Keys);
            keys.Sort();
            // then we iterate over the sorted keys
            foreach (string key in keys)
            {
                yield return elements[key] as Element;
            }
        }

        /// <summary>
        /// Returns an enumerator for the collection of Elements in ascending tag order.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Element>)this).GetEnumerator();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public override long Size
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public override long Read(Stream stream)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public override long Write(Stream stream)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Writes the collection to a stream using the specified transfer syntax.
        /// </summary>
        /// <param name="filename">The stream to write to.</param>
        /// <returns>The number of bytes written.</returns>
        /// <remarks>See <see cref="Part10Header"></see> for an explanation of the File Meta tags./></remarks>
        public long Write(Stream stream, string syntax)
        {
            if (syntax == Syntax.Unknown)
            {
                syntax = Syntax.ImplicitVrLittleEndian;
            }
            long results = 0;
            SpecificCharacterSet encoding = SpecificCharacterSet.Default;
            if (this.Contains(t.SpecificCharacterSet))
            {
                object text = this[t.SpecificCharacterSet].Value;
                encoding = new SpecificCharacterSet(text);
            }
            foreach (Element element in this)
            {
                //element.Dump();
                if (!GroupLengths && element.Tag.IsGroupLength)
                {
                    continue;
                }
                results += element.Write(stream, syntax, encoding, options);
            }
            return results;
        }

        /// <summary>
        /// Provide a list of the Elements.
        /// </summary>
        /// <returns>A concatenated string of Elements.</returns>
        /// <remarks>The string does not contain the File Meta Elements. See <see cref="Element.Dump"./></remarks>
        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            foreach (Element element in this.InOrder)
            {
                text.Append(element.Dump());
            }
            return text.ToString();
        }

        public string ToXml()
        {
            StringWriter text = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(text);

            writer.WriteStartElement("dataset");
            writer.WriteWhitespace("\r\n");
            foreach (KeyValuePair<string, Element> pair in elements)
            {
                pair.Value.Write(writer);
            }
            writer.WriteEndElement();
            writer.WriteWhitespace("\r\n");

            return text.ToString();
        }


        #region ICloneable Members

        public object Clone()
        {
            Elements clone = new Elements();
            foreach (Element element in this)
            {
                clone.Add((Element)element.Clone());
            }
            return clone;
        }

        #endregion
    }
}
