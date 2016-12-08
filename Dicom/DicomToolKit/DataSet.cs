using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents a Dicom Data Set.
    /// </summary>
    /// <remarks>A Data Set represents an instance of a real world Information Object. 
    /// A Data Set is constructed of <see cref="Element">Data Elements</see>. Data Elements 
    /// contain the encoded Values of Attributes of that object.</remarks>
    public class DataSet : IEnumerable<Element>
    {
        internal Elements metadata;
        internal Elements elements;
        internal Stream stream;

        private string syntax = Syntax.Unknown;
        // TODO: should this determine all the Enumerators, indexers and other methods that look into the collection of tags
        private bool header = false;

        public SpecificCharacterSet encoding = new SpecificCharacterSet();

        /// <summary>
        /// Constructs an empty Dataset
        /// </summary>
        public DataSet()
        {
            metadata = new Elements();
            metadata.GroupLengths = true;
            elements = new Elements();
        }

	    public string GetSafeStringValue(string tagId)
	    {
		    if (!ValueExists(tagId))
			    return string.Empty;

			var elementValue = this[tagId].Value;
			if (elementValue == null)
			    return string.Empty;

			return elementValue.ToString();
	    }

        public bool Part10Header
        {
            get
            {
                return header;
            }
            set
            {
                header = value;
                if (header)
                {
                    CheckHeader();
                }
            }
        }

        public bool GroupLengths
        {
            get
            {
                return elements.GroupLengths;
            }
            set
            {
                elements.GroupLengths = value;
            }
        }

        public bool UndefinedSequenceLength
        {
            get
            {
                return elements.UndefinedSequenceLength;
            }
            set
            {
                elements.UndefinedSequenceLength = value;
            }
        }

        public bool UndefinedItemLength
        {
            get
            {
                return elements.UndefinedItemLength;
            }
            set
            {
                elements.UndefinedItemLength = value;
            }
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
            Element.Factory(null, tag);
            Element element = new Element(tag.Group, tag.Element, null, value);
            this.Add(element);
            return element;
        }

        /// <summary>
        /// Creates and adds a new Element and initializes the VR and Value.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to Add.</param>
        /// <param name="vr">The VR for the new Element.</param>
        /// <param name="value">The value to initialize the Element to.</param>
        /// <returns>The newly created Element</returns>
        public Element Add(string key, string vr, object value)
        {
            Tag tag = Tag.Parse(key.ToLower());
            string temp = (vr == String.Empty) ? tag.VR : vr;
            Element element = new Element(tag.Group, tag.Element, temp, value);
            this.Add(element);
            return element;
        }

        /// <summary>
        /// Adds an Element to the DataSet.
        /// </summary>
        /// <param name="element">The Element to add.</param>
        /// <returns>The Element added.</returns>
        public Element Add(Element element)
        {
            Elements temp = (element.Group == 0x0002) ? metadata : elements;
            string key = Tag.ToString(element);
            if (!temp.Contains(key))
            {
                temp.Add(element);
            }
            else
            {
                throw new Exception(String.Format("{0} already added", element.Tag.ToString()));
            }
            return element;
        }

        /// <summary>
        /// Sets the value of an element, adds it if it does not exist.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to set or add.</param>
        /// <param name="value">The value to set the Element to.</param>
        public void Set(string key, object value)
        {
            Tag tag = Tag.Parse(key.ToLower());
            Elements temp = (tag.Group == 0x0002) ? metadata : elements;
            temp.Set(key, value);
        }

        /// <summary>
        /// Removes an Element from the DataSet.
        /// </summary>
        /// <param name="element">A string representing the tag of the Element to Remove.</param>
        public void Remove(Element element)
        {
            Elements temp = (element.Group == 0x0002) ? metadata : elements;
            temp.Remove(element);
        }

        /// <summary>
        /// Removes an Element with the tag of key from the DataSet.
        /// </summary>
        /// <param name="key">A string representing the tag of the Element to Remove.</param>
        public void Remove(string key)
        {
            Elements temp = (key.IndexOf("(0002,") == 0) ? metadata : elements;
            temp.Remove(key);
        }

        /// <summary>
        /// An enumerator of all elements in ascending tag order, including elements 
        /// nested within sequences.
        /// </summary>
        /// <remarks>The enumerator does not contain File Meta Elements, but it does contain
        /// each tag including those inside sequences.</remarks>
        public IEnumerable<Element> InOrder
        {
            get
            {
                foreach (Element element in elements.InOrder)
                {
                    yield return element;
                }
            }
        }

        /// <summary>
        /// Returns an enumerator for all root level Elements in ascending order.
        /// </summary>
        /// <remarks>The enumerator contains the File Meta Elements if the <see cref="Part10Header"/> is set to true.</remarks>
        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            if (header)
            {
                foreach (Element meta in metadata)
                {
                    yield return meta;
                }
            }
            foreach (Element element in elements)
            {
                yield return element;
            }
        }

        /// <summary>
        /// Returns an enumerator for all root level Elements in ascending order.
        /// </summary>
        /// <remarks>The enumerator contains the File Meta Elements if the <see cref="Part10Header"/> is set to true.</remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Element>)this).GetEnumerator();
        }

        /// <summary>
        /// Whether or not the Element exists in the DataSet.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>True if the Element is in the DataSet.</returns>
        /// <remarks>See <see cref="Elements.Contains" for a description of the syntax for the key./></remarks>
        public bool Contains(string key)
        {
            Elements temp = (key.IndexOf("(0002,") == 0) ? metadata : elements;
            return temp.Contains(key);
        }

        /// <summary>
        /// Whether or not the Element exists in the DataSet and has a non-null Value.
        /// </summary>
        /// <param name="key">A string representing the Element's tag.</param>
        /// <returns>True if the Element is in the DataSet.</returns>
        /// <remarks>See <see cref="Elements.Contains" for a description of the syntax for the key./></remarks>
        public bool ValueExists(string key)
        {
            Elements temp = (key.IndexOf("(0002,") == 0) ? metadata : elements;
            return temp.ValueExists(key);
        }

        /// <summary>
        /// The collection of Elements
        /// </summary>
        /// <remarks>The collection returned does not contain the File Meta Elements.</remarks>
        public Elements Elements
        {
            get
            {
                return elements;
            }
            set
            {
                elements = value;
            }
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
                Elements temp = (key.IndexOf("(0002,") == 0) ? metadata : elements;
                return temp[key];
            }
            set
            {
                Elements temp = (key.IndexOf("(0002,") == 0) ? metadata : elements;
                temp[key] = value;
            }
        }

        /// <summary>
        /// The Transfer Syntax that the DataSet will use for writing.
        /// </summary>
        /// <remarks>If the DataSet contains a TransferSyntaxUID tag, the Value is also updated.</remarks>
        public string TransferSyntaxUID
        {
            // TODO: Should we get rid of syntax and always tie into this[t.TransferSyntaxUID], creating it if necessary
            get
            {
                return syntax;
            }
            set
            {
                syntax = value;
                if (Contains(t.TransferSyntaxUID))
                {
                    this[t.TransferSyntaxUID].Value = value;
                }
            }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public long Size
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public string ToXml()
        {
            StringWriter text = new StringWriter();
            XmlTextWriter writer = new XmlTextWriter(text);

            writer.WriteStartElement("dataset");
            writer.WriteWhitespace("\r\n");

            if (header)
            {
                foreach (Element element in metadata)
                {
                    element.Write(writer);
                }
            }
            foreach (Element element in elements)
            {
                element.Write(writer);
            }

            writer.WriteEndElement();
            writer.WriteWhitespace("\r\n");

            return text.ToString();
        }

        /// <summary>
        /// Reads a file and appends the tags to the DataSet. 
        /// </summary>
        /// <param name="filename">The path of the file to Read.</param>
        /// <returns>The number of bytes Read.</returns>
        public long Read(string filename)
        {
            return Read(filename, UInt16.MaxValue);
        }

        /// <summary>
        /// Reads a file up to and including a specified group and appends the tags to the DataSet. 
        /// </summary>
        /// <param name="filename">The path of the file to Read.</param>
        /// <param name="group">The group number to Read up to and including.</param>
        /// <returns>The number of bytes Read.</returns>
        /// <remarks>Note: by avoiding reading the PixelData, you can Read Dicom files faster, however you will not 
        /// get any tags in the DataSet after the PixelData tag.</remarks>
        public long Read(string filename, ushort group)
        {
            long result = 0;
            FileStream stream = null;
            try
            {
                stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                result = Read(stream, group);
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Reads a stream and appends the tags to the DataSet. 
        /// </summary>
        /// <param name="stream">The stream to Read.</param>
        /// <returns>The number of bytes Read.</returns>
        public long Read(Stream stream)
        {
            return Read(stream, UInt16.MaxValue);
        }

        /// <summary>
        /// Reads a stream up to and including a specified group and appends the tags to the DataSet. 
        /// </summary>
        /// <param name="stream">The stream to Read.</param>
        /// <param name="group">The group number to Read up to and including.</param>
        /// <returns>The number of bytes Read.</returns>
        /// <remarks>Note: by avoiding reading the PixelData, you can Read Dicom files faster, however you will not 
        /// get any tags in the DataSet after the PixelData tag.</remarks>
        public long Read(Stream stream, ushort group)
        {
            long result = 0;
            try
            {
                this.stream = stream;

                long start = stream.Position;
                // get length of stream
                stream.Seek(0, SeekOrigin.End);
                long size = stream.Length;
                stream.Seek(start, SeekOrigin.Begin);

                long current = ScanHeader(stream, size);
                current = Scan(null, stream, elements, current, size, group, syntax, encoding);

                // current can be greater than size if we end the parsing on a sequence
                // only report a problem if we were asked to read to the very end
                if (current < size && group == UInt16.MaxValue)
                    throw new Exception("Parsing ended early.");

                result = stream.Position - start;
            }
            catch (Exception ex)
            {
                throw;
            }
            return result;
        }

        /// <summary>
        /// Write the DataSet to a file.
        /// </summary>
        /// <param name="filename">The path of the file to write to.</param>
        /// <returns>The number of bytes written.</returns>
        /// <remarks>See <see cref="Part10Header"></see> for an explanation of the File Meta tags./></remarks>
        public long Write(string filename)
        {
            long result = 0;
            FileStream stream = null;
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    result = Write(memory);
                    stream = new FileStream(filename, FileMode.Create);
                    stream.Write(memory.ToArray(), 0, (int)memory.Length);
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Flush();
                    stream.Close();
                    stream.Dispose();
                }
            }
            return result;
        }

        /// <summary>
        /// Write the DataSet to a stream.
        /// </summary>
        /// <param name="filename">The stream to write to.</param>
        /// <returns>The number of bytes written.</returns>
        /// <remarks>See <see cref="Part10Header"></see> for an explanation of the File Meta tags./></remarks>
        public long Write(Stream stream)
        {
            long results = 0;

            // the transfer syntax as set in the dataset overrides the syntax property.
            if (Contains(t.TransferSyntaxUID))
            {
                syntax = (string)this[t.TransferSyntaxUID].Value;
            }
            if (syntax == Syntax.Unknown)
            {
                syntax = Syntax.ImplicitVrLittleEndian;
            }

            SpecificCharacterSet encoding = SpecificCharacterSet.Default;
            if (header)
            {
                CheckHeader();
                SetGroupLengths(metadata, encoding);
                BinaryWriter writer = new BinaryWriter(stream);
                byte[] buffer = new byte[128];
                writer.Write(buffer, 0, buffer.Length);
                writer.Write("DICM".ToCharArray());
                results += buffer.Length + 4;
                // metadata is always written in ExplicitVrLittleEndian
                results += metadata.Write(stream, Syntax.ExplicitVrLittleEndian);
            }

            //Logging.Log(LogLevel.Verbose, "Writing {0}.", Reflection.GetName(typeof(Syntax), syntax));
            if (elements.Contains(t.SpecificCharacterSet))
            {
                object text = this[t.SpecificCharacterSet].Value;
                encoding = new SpecificCharacterSet(text);
            }
            SetGroupLengths(elements, encoding);
            results += elements.Write(stream, syntax);

            return results;
        }

        /// <summary>
        /// Set the DICOM file meta information, if not already set.  
        /// </summary>
        /// <remarks>If not already set: MediaStorageSOPClassUID is set to SOPClassUID or SecondaryCaptureImageStorage;
        /// MediaStorageSOPClassUID is set to SOPInstanceUID or new guid; TransferSyntaxUID is set to current Syntax on DataSet.</remarks>
        private void CheckHeader()
        {
            if (!metadata.Contains(t.FileMetaInformationGroupLength))
            {
                metadata.Set(t.FileMetaInformationGroupLength, 0);
            }
            if (!metadata.Contains(t.FileMetaInformationVersion))
            {
                metadata.Set(t.FileMetaInformationVersion, new byte[] { 0, 1 });
            }
            if (!metadata.Contains(t.MediaStorageSOPClassUID))
            {
                metadata.Set(t.MediaStorageSOPClassUID, elements.ValueExists(t.SOPClassUID) ? (string)elements[t.SOPClassUID].Value : SOPClass.SecondaryCaptureImageStorage);
            }
            if (!metadata.Contains(t.MediaStorageSOPInstanceUID))
            {
                metadata.Set(t.MediaStorageSOPInstanceUID, (elements.ValueExists(t.SOPInstanceUID)) ? (string)elements[t.SOPInstanceUID].Value : Element.NewUid());
            }
            if (!metadata.Contains(t.TransferSyntaxUID))
            {
                metadata.Set(t.TransferSyntaxUID, this.syntax);
            }
            if (!metadata.Contains(t.ImplementationClassUID))
            {
                metadata.Set(t.ImplementationClassUID, Element.NewUid());
            }
            if (!metadata.Contains(t.ImplementationVersionName))
            {
                metadata.Set(t.ImplementationVersionName, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            }

            RemoveHeaderFromBody();
        }

        public void RemoveHeaderFromBody()
        {
            if (elements.Contains(t.FileMetaInformationGroupLength))
            {
                elements.Remove(t.FileMetaInformationGroupLength);
            }
            if (elements.Contains(t.FileMetaInformationVersion))
            {
                elements.Remove(t.FileMetaInformationVersion);
            }
            if (elements.Contains(t.MediaStorageSOPClassUID))
            {
                elements.Remove(t.MediaStorageSOPClassUID);
            }
            if (elements.Contains(t.MediaStorageSOPInstanceUID))
            {
                elements.Remove(t.MediaStorageSOPInstanceUID);
            }
            if (elements.Contains(t.TransferSyntaxUID))
            {
                elements.Remove(t.TransferSyntaxUID);
            }
            if (elements.Contains(t.ImplementationClassUID))
            {
                elements.Remove(t.ImplementationClassUID);
            }
            if (elements.Contains(t.ImplementationVersionName))
            {
                elements.Remove(t.ImplementationVersionName);
            }
        }

        internal static Tag PeekTag(EndianBinaryReader reader, string syntax)
        {
            long rewind = 0;
            Tag tag = null;
            try
            {
                short group = reader.ReadInt16();
                short element = reader.ReadInt16();
                rewind += sizeof(ushort) + sizeof(ushort);

                tag = new Tag(group, element);

                if (Syntax.IsExplicit(syntax) && (!tag.Equals(t.Item) && !tag.Equals(t.ItemDelimitationItem) && !tag.Equals(t.SequenceDelimitationItem)))
                {
                    byte[] vr = reader.ReadBytes(2);
                    rewind += 2;
                    tag.VR = System.Text.Encoding.ASCII.GetString(vr);
                }
            }
            finally
            {
                reader.BaseStream.Position -= rewind;
            }
            return tag;
        }

        /// <summary>
        /// Parses the stream for tags.
        /// </summary>
        /// <param name="parent">The parent Element that the tags will belong to, if any.</param>
        /// <param name="stream">The stream to Read from.</param>
        /// <param name="elements">The Elements collection to add the Elements to.</param>
        /// <param name="current">The current postion in the stream.</param>
        /// <param name="length">The maximum number of bytes to read.</param>
        /// <param name="stop">The group number ot read up to and including.</param>
        /// <param name="syntax">The Transfer Syntax of the stream.</param>
        /// <param name="encoding">The character encoding of the stream.</param>
        /// <returns>The final position in the stream Read to.</returns>
        public static long Scan(Element parent, Stream stream, Elements elements, long current, long length, ushort stop, string syntax, SpecificCharacterSet encoding)
        {
            //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Scan called parent={1}:{2}. current={3} length={4} stop={5}", 
            //    stream.Position, (parent!=null)?parent.Tag.ToString():"root", (parent!=null)?parent.VR:"", current, length, stop));

            // we have to get the stream on top of the current position
            stream.Position = current;

           EndianBinaryReader reader = new EndianBinaryReader(stream, Syntax.GetEndian(syntax));

            // Scan stops when either the stream position exceeds length, or the group exceeds stop,
            // or we reach the end of a file, or we reach an ItemDelimiter or SequenceDelimiter, or
            // we can no longer read
            while (true)
            {
                if (!stream.CanRead)
                {
                    //System.Diagnostics.Debug.WriteLine("stream CanRead==false"); 
                    break;
                }

                if (length != -1 && (ulong)current >= (ulong)length)
                {
                    //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: current={1} exceeds length={2}", stream.Position, current, length));
                    break;
                }

                // keep track of where we are before reading the next element
                long previous = current;
                Tag lookahead = PeekTag(reader, syntax);

                //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Peek {1} in Scan", stream.Position, lookahead.Name));

                // we scan up to and including the group specified by stop
                // so we check if we have gone beyond the specified group
                if (lookahead.Group > stop)
                {
                    //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: group={1} exceeds stop={2}", stream.Position, lookahead.Group, stop));
                    // we need to rewind to before this element
                    current = previous;
                    break;
                }

                if (length == -1)
                {
                    //if (lookahead.Equals(t.ItemDelimitationItem) || lookahead.Equals(t.SequenceDelimitationItem))
                    if (lookahead.Equals(t.ItemDelimitationItem))
                    {
                        //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: undefined length, found {1}:{2}", stream.Position, lookahead.Description, lookahead.Name));
                        // position stream beyond delimitation tag
                        stream.Position += sizeof(ushort) * 2;
                        // no need to check zero
                        int zero = reader.ReadInt32();
                        break;
                    }
                }

                Exception exception = null;
                Element element = Element.Factory(parent, lookahead);
                try
                {
                    //System.Diagnostics.Debug.WriteLine(String.Format("reading {0}", lookahead.ToString()));
                    element.Read(stream, syntax, encoding);
                }
                catch (Exception ex)
                {
                    // we are going to delay throwing an exception from reading
                    // until we see if we are going to add it to the collection
                    exception = ex;
                }

                current = stream.Position;

                // if we have the SpecificCharacterSet, create an Encoding for the rest of the DataSet
                if (element.Tag.Equals(t.SpecificCharacterSet))
                {
                    encoding = new SpecificCharacterSet(element.Value);
                }
                else if (element.Tag.Equals(t.PixelData) && !Syntax.IsExplicit(syntax))
                {
                    // if we have the PixelData, insure that it matches the BitsStored
                    // this can happen with implicit vrs and OW is the default

                    if (elements.Contains(t.BitsStored))
                    {
                        ushort stored = (ushort)elements[t.BitsStored].Value;
                        Type type = element.Value.GetType().GetElementType();
                        if (stored == 8 && element.Value is ushort[])
                        {
                            ushort[] pixels = element.Value as ushort[];
                            byte[] bytes = new byte[pixels.Length * 2];
                            Buffer.BlockCopy(pixels, 0, bytes, 0, bytes.Length);
                            element.Value = bytes;
                        }
                    }
                }
                else if (element.Tag.Equals(t.ItemDelimitationItem) || element.Tag.Equals(t.SequenceDelimitationItem) || element.Tag.Equals(t.Item))
                {
                    // if we have a sequence or item delimiter, stop scanning
                    if (length == -1)
                    {
                        return current;
                    }
                }

                // now that we are about to add the tag, check and see if it parsed correctly
                if (exception != null)
                {
                    // we have failed to parse, so we create a tag that contains the rest of the data
                    stream.Position = previous;
                    element = new Element("(BAAD,F00D)", reader.ReadBytes((int)(length-previous)));
                }

                string key = Tag.ToString(element);
                // and insert the tag name/value pair in the hash table, allows duplicates
                elements[key] = element;
                //Logging.Log("Adding {0}:{1}", key, element.ToString());

                // now we can throw the exception if there is one
                if (exception != null)
                {
                    throw exception;
                }

                if ((ulong)current >= (ulong)length)
                {
                    //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: current={1} at or exceeds length={2}", stream.Position, current, length));
                    break;
                }
            }
            return current;
        }

        /// <summary>
        /// Attempts to scan a stream for a valid Dicom header.
        /// </summary>
        /// <param name="stream">The stream to Read.</param>
        /// <param name="length">The total length of the stream.</param>
        /// <returns>The final position in the stream Read to.</returns>
        /// <remarks>This method attempts to guess at the TransferSyntax, if one is not found in the header.</remarks>
        private long ScanHeader(Stream stream, long length)
        {
            long result = stream.Position;

            try
            {
                // look for a header
                if (length > 128 + 4)
                {
                    // a dicom 3.0 file will have a 128 byte preamble with a DICM signature after
                    // go past the header
                    long current = stream.Seek(128, SeekOrigin.Begin);
                    // and read the signature, if any
                    byte[] temp = new byte[4];
                    stream.Read(temp, 0, temp.Length);
                    string signature = System.Text.Encoding.ASCII.GetString(temp);

                    if (signature == "DICM")
                    {
                        // a dicom 3.0 file will have a series of 0x0002 group elements
                        // and 0x0002 elements are always ExplicitVrLittleEndian
                        result = Scan(null, stream, metadata, 128 + 4, length, 0x0002, Syntax.ExplicitVrLittleEndian, encoding);

                        // look for the Transfer Syntax UID
                        if (metadata.Contains(t.TransferSyntaxUID))
                        {
                            Element element = metadata[t.TransferSyntaxUID] as Element;
                            String value = element.Value as String;
                            switch (value)
                            {
                                case Syntax.ExplicitVrLittleEndian:
                                case Syntax.ExplicitVrBigEndian:
                                case Syntax.ImplicitVrLittleEndian:
                                case Syntax.JPEGBaselineProcess1:
                                case Syntax.JPEGExtendedProcess2n4:
                                case Syntax.JPEGProgressiveProcess10n12:
                                case Syntax.JPEGLosslessProcess14:
                                case Syntax.JPEGLosslessProcess15:
                                case Syntax.JPEGLosslessProcess14SelectionValue1:
                                case Syntax.JPEGLSLossless:
                                case Syntax.JPEGLSNearlossless:
                                case Syntax.JPEG2000Lossless:
                                case Syntax.JPEG2000:
                                case Syntax.RLELossless:
                                    syntax = value;
                                    break;
                                default:
                                    throw new Exception("Unsupported Transfer Syntax.");
                            }
                        }
                        else
                        {
                            stream.Position = result;
                            syntax = GuessAtSyntax(stream);
                        }
                        // this will flesh out any required part two tags not found in Scan
                        this.Part10Header = true;
                    }
                    else
                    {
                        // restore the stream position to where we started
                        stream.Position = result;
                        syntax = GuessAtSyntax(stream);
                    }
                }
                else
                {
                    this.Part10Header = false;
                    stream.Position = result;
                    if (syntax == Syntax.Unknown)
                    {
                        syntax = GuessAtSyntax(stream);
                    }
                }
            }
            catch
            {
                throw;
            }
            //Logging.Log(LogLevel.Verbose, "Reading {0}", Reflection.GetName(typeof(Syntax), syntax));
            return result;
        }

        /// <summary>
        /// Guess the transfer syntax by peeking into the stream
        /// </summary>
        /// <param name="stream">The stream to peek into.</param>
        /// <returns>The transfer syntax as a string.</returns>
        /// <remarks>To determine the byte ordering we assume that the stream is positioned
        /// at the beginning of a tag and extract the group and element,  first using a little 
        /// endian reader then a big endian reader.  If that element is in the dictionary, that 
        /// fixes the byte ordering.  Then we compare the next two bytes to the vr from 
        /// the dictionary, if there is a match, then we assume vrs are explicit.
        /// </remarks>
        private string GuessAtSyntax(Stream stream)
        {
            EndianBinaryReader reader;
            short group, element;
            string vr = String.Empty;
            bool big = false, little = false, @explicit = false;

            long start = stream.Position;

            // first try little endian
            reader = new EndianBinaryReader(stream, Endian.Little);

            group = reader.ReadInt16();
            element = reader.ReadInt16();
            Tag tag = new Tag(group, element);
            // did we get something that we recognize
            little = Dictionary.Contains(tag.ToString());
            if (little)
            {
                // now see if we can find an explicit vr in the next two bytes
                byte[] bt = reader.ReadBytes(2);
                vr = System.Text.Encoding.ASCII.GetString(bt);
                @explicit = (vr == tag.VR);
            }

            if (!little)
            {
                // rewind our stream to start over 
                stream.Position = start;

                // now try big endian
                reader = new EndianBinaryReader(stream, Endian.Big);

                group = reader.ReadInt16();
                element = reader.ReadInt16();
                tag = new Tag(group, element);
                // did we get something that we recognize
                big = Dictionary.Contains(tag.ToString());
                if (big)
                {
                    // now see if we can find an explicit vr in the next two bytes
                    byte[] bt = reader.ReadBytes(2);
                    vr = System.Text.Encoding.ASCII.GetString(bt);
                    @explicit = (vr == tag.VR);
                }
            }

            string guess = Syntax.ImplicitVrLittleEndian;
            if (little)
            {
                guess = (@explicit) ? Syntax.ExplicitVrLittleEndian : Syntax.ImplicitVrLittleEndian;
            }
            else if (big)
            {
                guess = (@explicit) ? Syntax.ExplicitVrBigEndian : Syntax.ImplicitVrLittleEndian;
            }
            else
            {
                throw new Exception("No DICOM header. Unable to guess at syntax. Probably not a DICOM DataSet.");
            }

            // put our stream position back to where we started
            stream.Position = start;
            Logging.Log(LogLevel.Verbose, "Guessing at syntax, {0}", Reflection.GetName(typeof(Syntax), syntax));
            return guess;
        }

        /// <summary>
        /// Just prior to writing, this sets the length of the Group Length tags in the DataSet
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="encoding"></param>
        private void SetGroupLengths(Elements tags, SpecificCharacterSet encoding)
        {
            uint size = 0;
            Element group = null;
            foreach (Element element in tags)
            {
                if (element.element == 0x0000)
                {
                    if (group != null)
                    {
                        //Logging.Log("Setting GroupLength {0} to {1}.", group, size);
                        group.Value = size;
                    }
                    group = element;
                    size = 0;
                }
                else
                {
                    size += element.GetSize((group != null && group.element == 0x0000) ? Syntax.ExplicitVrLittleEndian : syntax, encoding);
                }
            }
            if (group != null)
            {
                //Logging.Log("Setting GroupLength {0} to {1}.", group, size);
                group.Value = size;
            }
        }
        
        /// <summary>
        /// Provide a list of the Elements.
        /// </summary>
        /// <returns>A concatenated string of Elements.</returns>
        /// <remarks>The string does not contain the File Meta Elements. See <see cref="Element.Dump"./></remarks>
        public string Dump()
        {
            StringBuilder text = new StringBuilder();
            foreach (Element element in elements)
            {
                text.Append(element.Dump());
            }
            return text.ToString();
        }

        public void DeIdentify()
        {
            try
            {

                string target = "CR";
                if (elements.ValueExists(t.SOPClassUID))
                {
                    switch ((string)elements[t.SOPClassUID].Value)
                    {
                        case SOPClass.ComputedRadiographyImageStorage:
                            target = "CR";
                            break;
                        case SOPClass.DigitalXRayImageStorageForPresentation:
                        case SOPClass.DigitalXRayImageStorageForProcessing:
                            target = "DX";
                            break;
                        case SOPClass.DigitalMammographyImageStorageForPresentation:
                        case SOPClass.DigitalMammographyImageStorageForProcessing:
                            target = "MG";
                            break;
                    }
                }
                Dictionary<string, Tag2> iod = Iod.GetTags(target);

                // TODO change loop to go through elements rather than Protected tags
                DateTime now = DateTime.Now;
                foreach (Tag tag in from s in Dictionary.Instance where s.Protected == true select s)
                {
                    string key = tag.ToString();

                    // de-identify some tags by the type of tag
                    bool matched = true;
                    switch (key)
                    {
                        case "(0008,1030)":  // Study Description
                            if(elements.Contains(key))
                                elements[key].Value = "De-Identified Study";
                            break;
                        case "(0010,0040)": // Patient's Sex
                            if (elements.Contains(key))
                                elements[key].Value = "O";
                            break;
                        default:
                            matched = false;
                            break;
                    }

                    // de-identify all other tags based on VR
                    if (!matched && elements.Contains(key))
                    {
                        switch (elements[key].VR)
                        {
                            case "CS":  // Code String, 16 bytes maximum
                            case "AE":  // Application Entity, 16 bytes maximum
                            case "PN":  // Person's Name, 64 character maximum per component group
                            case "LO":  // Long String, 64 characters maximum
                            case "SH":  // Short String, 16 characters maximum
                            case "ST":  // Short Text, 1024 characters maximum
                            case "LT":  // Long Text, 10240 characters maximum
                            case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                                elements[key].Value = "PHI";
                                break;
                            case "AS":  // Age string, 4 bytes fixed
                                elements[key].Value = "000Y";
                                break;
                            case "DA":  // Date, 8 bytes fixed
                                elements[key].Value = now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "DT":  // Date Time, 26 bytes maximum
                                elements[key].Value = now.ToString("yyyyMMddHHmmss", System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "TM":  // Time, 16 bytes maximum
                                elements[key].Value = now.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                                break;
                            case "UI":  // Unique Identifier, 64 bytes maximum
                                elements[key].Value = Element.NewUid().Replace(Element.UidRoot, "1.2.840.123");
                                break;
                            case "DS":  // Decimal String, 16 bytes maximum
                            case "IS":  // Integer String, 12 bytes maximum
                            case "FL":  // Floating Point Single, 4 bytes fixed
                            case "FD":  // Floating Point Double, 8 bytes fixed
                            case "SL":  // Signed Long, 4 bytes fixed
                            case "SS":  // Signed Short, 2 bytes fixed
                            case "UL":  // Unsigned Long, 4 bytes fixed
                            case "US":  // Unsigned short, 2 bytes fixed
                            case "AT":  // Attribute tag, 4 bytes fixed
                                elements[key].Value = 0;
                                break;
                            case "SQ":  // Sequence
                            case "OF":  // Other Float String
                            case "OW":  // Other Word String
                            case "OB":  // Other Byte String
                            case "UN":  // Unknown, any length valid for any other VR
                                break;
                        }
                    }
                    //else if (elements.Contains(key))
                    //{
                    //    elements[key].Value = null;
                    //}
                }
            }
            catch(Exception ex)
            {
                Logging.Log(LogLevel.Error, ex.Message);
                throw ex;
            }
        }
    }

    /// <summary>
    /// A structure meant to track certain options available for writing DataSets.
    /// </summary>
    public struct DataSetOptions
    {
        /// <summary>Whether or not Group Length tags will be written.</summary>
        private bool groupLengths;
        /// <summary>Whether or not Sequence lengths will be written as undefined.</summary>
        private bool undefinedSequenceLength;
        /// <summary>Whether or not Item lengths will be written as undefined.</summary>
        private bool undefinedItemLength;

        public DataSetOptions(bool group, bool sequence, bool item)
        {
            groupLengths = group;
            undefinedSequenceLength = sequence;
            undefinedItemLength = item;
        }

        /// <summary>
        /// Whether or not Group Length tags will be written.
        /// </summary>
        public bool GroupLengths
        {
            get
            {
                return groupLengths;
            }
            set
            {
                groupLengths = value;
            }
        }

        /// <summary>
        /// Whether or not Sequence lengths will be written as undefined.
        /// </summary>
        public bool UndefinedSequenceLength
        {
            get
            {
                return undefinedSequenceLength;
            }
            set
            {
                undefinedSequenceLength = value;
            }
        }

        /// <summary>
        /// Whether or not Item lengths will be written as undefined.
        /// </summary>
        public bool UndefinedItemLength
        {
            get
            {
                return undefinedItemLength;
            }
            set
            {
                undefinedItemLength = value;
            }
        }

    }
}
