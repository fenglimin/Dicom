using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml;

// validate adherance to standard
// compare VM to standard from dictionary
// create a reasonable error handling scheme for parsing, etc.
// Code review type changes
// Support variable length sequences

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents a Dicom Data Element and the encoded Value(s).
    /// </summary>
    public class Element : DicomObject, ICloneable
    {

        protected Tag tag;
        protected uint vm;
        protected string vr;
        protected Element parent;
        protected uint length;
        protected long position;
        protected Stream stream;
        protected object value;
        protected string syntax;

        /// <summary>
        /// Identifies Carestream Health, Inc. and the DicomToolkit (.26)
        /// </summary>
        public static readonly string UidRoot = "1.2.840.113564.26";

        /// <summary>
        /// Constructs an empty Element.
        /// </summary>
        /// <remarks>The VR is UN.</remarks>
        public Element()
        {
            tag = new Tag();
            syntax = Syntax.ImplicitVrLittleEndian;
            value = null;
            parent = null;
        }

        /// <summary>
        /// Constructs an empty Element in a sequence item.
        /// </summary>
        /// <remarks>The VR is UN.</remarks>
        internal Element(Element parent)
            : this()
        {
            this.parent = parent;
            tag.Parent = (parent != null) ? parent.Tag : null;
        }

        internal Element(Element parent, Tag tag)
            : this(parent)
        {
            Initialize(tag.Group, tag.Element, tag.VR, null);
        }

        /// <summary>
        /// Constructs an Element an with initial value.
        /// </summary>
        /// <remarks>The VR is UN if not found in the Dictionary.</remarks>
        public Element(string key, object value)
            : this()
        {
            Tag temp = Tag.Parse(key);
            Initialize(temp.Group, temp.Element, null, value);
        }

        /// <summary>
        /// Constructs an Element a null value.
        /// </summary>
        /// <remarks>The VR is UN if not found in the Dictionary.</remarks>
        public Element(short group, short element)
            : this()
        {
            Initialize(group, element, null, null);
        }

        /// <summary>
        /// Constructs an Element with a VR and a null value.
        /// </summary>
        /// <remarks>The VR is UN if not found in the Dictionary.</remarks>
        public Element(short group, short element, string vr)
            : this()
        {
            Initialize(group, element, vr, null);
        }

        /// <summary>
        /// Constructs an Element with a VR and an initial value.
        /// </summary>
        /// <remarks>The VR is UN if not found in the Dictionary.</remarks>
        public Element(short group, short element, string vr, object value)
            : this()
        {
            Initialize(group, element, vr, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static Element Factory(Element parent, Tag tag)
        {
            if (tag.VR == "SQ")
            {
                return new Sequence(parent);
            }
            else if (tag.Equals(t.PixelData))
            {
                return new PixelData(parent);
            }
            return new Element(parent, tag);
        }

        /// <summary>
        /// Initializes the Element with information from the Dictionary and Value.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        /// <param name="type"></param>
        /// <param name="value"></param>
        protected void Initialize(short group, short element, string type, object value)
        {
            tag.Group = group;
            tag.Element = element;

            string key = Tag.ToString(group, element);
            // if we have the tag in the Dictionary, fill in what we can
            if (Dictionary.Contains(key))
            {
                // get the tag from the Dictionary
                Tag temp = Dictionary.Instance[key];
                // if the tag is Standard, use the VR in the Dictionary to set VR
                if (tag.IsStandard)
                {
                    tag.VR = vr = temp.VR;
                }
                // if a VR has been specified
                if (type != null)
                {
                    tag.VR = vr = type;
                }
                else
                {
                    // default to the first, if multiple, but do not change the tag definition
                    vr = temp.VR.Split(",".ToCharArray())[0];
                }
                tag.Description = temp.Description;
            }
            else
            {
                if (type == null)
                {
                    if (tag.IsGroupLength) type = "UL";
                    if (tag.IsPrivateCreator) type = "LO";
                }
                tag.VR = vr = (type == null) ? "UN" : type;
            }

            if (tag.VR != "SQ")
            {
                this.Value = value;
            }
        }

        /// <summary>
        ///  Not Implemented.
        /// </summary>
        public override long Size
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        /// <summary>
        /// Gets or Sets the Dicom Group Number this instance.
        /// </summary>
        /// <exception cref="System.Exception">Illegal group and element combination.</exception>
        public short Group
        {
            get
            {
                return tag.Group;
            }
            set
            {
                //AssertValid(value, element);
                tag.Group = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Dicom Element Number this instance.
        /// </summary>
        /// <exception cref="System.Exception">Illegal group and element combination.</exception>
        public short element
        {
            get
            {
                return tag.Element;
            }
            set
            {
                //AssertValid(group, value);
                tag.Element = value;
            }
        }

        /// <summary>
        /// Returns the Parent Element
        /// </summary>
        public Element Parent
        {
            get
            {
                return parent;
            }
            set
            {
                this.parent = value;
                tag.Parent = (value != null) ? value.Tag : null;
            }
        }

        /// <summary>
        /// The Dicom Value Representation.
        /// </summary>
        public string VR
        {
            get
            {
                return vr;
            }
            set
            {
                vr = value;
            }
        }

        /// <summary>
        /// The Dicom Value Multiplicity.
        /// </summary>
        public uint VM
        {
            get
            {
                return vm;
            }
            set
            {
                this.vm = value;
            }
        }

        /// <summary>
        /// The description of the Dicom Tag.
        /// </summary>
        public string Description
        {
            get
            {
                return tag.Description;
            }
            set
            {
                tag.Description = value;
            }
        }

        /// <summary>
        /// The Tag of this Element.
        /// </summary>
        public Tag Tag
        {
            get
            {
                return tag;
            }
            set
            {
                tag = value;
            }
        }

        /// <summary>
        /// The current length of this Element.   The length could be expressed in terms of bytes or unicode characters.
        /// </summary>
        public uint Length
        {
            get
            {
                return length;
            }
        }

        /// <summary>
        ///  The location in the stream that this Element was read from.
        /// </summary>
        public long Position
        {
            // TODO : get rid of this
            get
            {
                return position;
            }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override long Read(Stream stream)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        /// <summary>
        /// Produce a string represenation of the collection.
        /// </summary>
        /// <returns>A string represenation of the collection.</returns>
        public override string Dump()
        {
            return (vr == "US" || vr == "SL" || vr == "SS" || vr == "UL") ? 
                String.Format("{0}:{1}:{2}:{3}:0x{4:x4}({5})\n", this.Tag.ToString(), vr, this.VM, this.Description, this.Value, this.Value) :
                String.Format("{0}:{1}:{2}:{3}:{4}\n", this.Tag.ToString(), vr, this.VM, this.Description, this.Value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            Stack<Element> temp = new Stack<Element>();
            Element parent = this;
            while (parent != null)
            {
                temp.Push(parent);
                parent = parent.Parent;
            }
            Element[] levels = temp.ToArray();

            StringBuilder text = new StringBuilder();
            for (int l = 0; l < levels.Length; l++)
            {
                Element level = levels[l];
                text.Append(String.Format("({0:x4},{1:x4})", level.Group, level.element));
                if (level.vr == "SQ" && l < levels.Length - 1)
                {
                    Element next = levels[l + 1];
                    string tag = String.Format("({0:x4},{1:x4})", next.Group, next.element);
                    for (int n = 0; n < ((Sequence)level).Items.Count; n++)
                    {
                        Elements item = ((Sequence)level).Items[n];
                        if (item.Contains(tag) && object.ReferenceEquals(item[tag], next))
                        {
                            text.Append(n);
                            break;
                        }
                    }
                }
            }

            return text.ToString();
        }

        /// <summary>
        /// A string that represents the value of the current object.
        /// </summary>
        /// <returns>The Value to string.</returns>
        public override string ToString()
        {
            if (vr == "SQ")
            {
                return "SQ";
            }
            else
            {
                if (this.Value == null)
                    return "(null)";
                if (this.Value is Array)
                {
                    StringBuilder text = new StringBuilder();
                    foreach (object entry in this.Value as Array)
                    {
                        if (text.Length > 0)
                        {
                            text.Append(@"\");
                        }
                        if (text.Length > 2048)
                        {
                            return Value.ToString();
                        }
                        text.Append(entry.ToString());
                    }
                    return text.ToString();
                }
            }
            return this.Value.ToString();
        }

        public static string GetRepeatingGroup(string tag, int group)
        {
            return tag.Substring(0, 3) + String.Format("{0:X2}", group * 2) + tag.Substring(5);
        }

        /// <summary>
        /// Generate a unique Uid.
        /// </summary>
        /// <returns>A unique DICOM UID.</returns>
        public static string NewUid()
        {
            // from dicom standard: UID is a character string composed of a series of numeric
            // components separated by a period.  Each component of a UID is a number and shall consist 
            // of one or more digits (0-9). The first digit of each component shall not be zero unless the 
            // component is a single digit. UID's, shall not exceed 64 total characters.

            string result = String.Empty;
            try
            {
                MemoryStream stream = new MemoryStream(Guid.NewGuid().ToByteArray());
                BinaryReader reader = new BinaryReader(stream);

                uint data1 = reader.ReadUInt32();       // 10 characters max
                ushort data2 = reader.ReadUInt16();     //  5 characters max 
                ushort data3 = reader.ReadUInt16();     //  5 characters max
                ulong data4 = reader.ReadUInt64();      // 20 characters max

                // add "1.2.840.113564.10.1." and that makes 61 characters max
                // String.Format will not format a number with leading zeros unless asked to do so
                result = String.Format("{0}.{1}.{2}.{3}.{4}", UidRoot, data1, data2, data3, data4);

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return result;
        }

        /// <summary>
        /// Parses the Element from the stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="syntax"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public long Read(Stream stream, string syntax, SpecificCharacterSet encoding)
        {
            EndianBinaryReader reader = new EndianBinaryReader(stream, Syntax.GetEndian(syntax));
            this.stream = stream;
            this.syntax = syntax;
            long start = stream.Position;

            tag = new Tag(reader.ReadInt16(), reader.ReadInt16(), (parent != null) ? parent.Tag : null);

            ValueRepresentationAndLength(reader);

            // this only sets the vm for all non-string datatypes
            ValueMultiplicity();

            if (length > 0)
            {
                stream.Position = position;
                // this sets the vm for string datatypes
                ReadValueFromStream(stream, syntax, encoding, length);
            }

            return stream.Position - start;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        private void ValueRepresentationAndLength(EndianBinaryReader reader)
        {
            if (Syntax.IsExplicit(syntax))
            {
                long lg = stream.Position;
                byte[] bt = reader.ReadBytes(2);
                //vr = System.Text.Encoding.ASCII.GetString(reader.ReadBytes(2));
                vr = System.Text.Encoding.ASCII.GetString(bt);

                // we do not know if we will be dealing with a 2 or 4 byte value length
                // switch on the current VR
                switch (vr)
                {
                    // the following use the 4 byte value length
                    case "OB":
                    case "OF":
                    case "OW":
                    case "UT":
                    case "UN":
                        {
                            // we have 2 reserved bytes
                            short reserved = reader.ReadInt16();
                            // we have a 4 byte value length
                            length = reader.ReadUInt32();
                            // record the start of our value
                            position = stream.Position;
                            // position ourselves at the end of the element/start of the next
                            stream.Position += (long)length;
                        }
                        break;
                    case "SQ":
                        // sequences use the 4 byte value length, but have no value field
                        // and have item tags
                        {
                            short reserved = reader.ReadInt16();
                            length = reader.ReadUInt32();
                            // record the start of our value
                            position = stream.Position;
                        }
                        break;
                    // the remainder have a 2 byte 
                    case "AE":  // Application Entity, 16 bytes maximum
                    case "AS":  // Age string, 4 bytes fixed
                    case "AT":  // Attribute tag, 4 bytes fixed
                    case "CS":  // Code String, 16 bytes maximum
                    case "DA":  // Date, 8 bytes fixed
                    case "DS":  // Decimal String, 16 bytes maximum
                    case "DT":  // Date Time, 26 bytes maximum
                    case "FD":  // Floating Point Double, 8 bytes fixed
                    case "FL":  // Floating Point Single, 4 bytes fixed
                    case "IS":  // Integer String, 12 bytes maximum
                    case "LO":  // Long String, 64 characters maximum
                    case "LT":  // Long Text, 10240 characters maximum, vm = 1 always
                    case "PN":  // Person's Name, 64 character maximum per component group
                    case "SH":  // Short String, 16 characters maximum
                    case "SL":  // Signed Long, 4 bytes fixed
                    case "SS":  // Signed Short, 2 bytes fixed
                    case "ST":  // Short Text, 1024 characters maximum
                    case "TM":  // Time, 16 bytes maximum
                    case "UI":  // Unique Identifier, 64 bytes maximum
                    case "UL":  // Unsigned Long, 4 bytes fixed
                    case "US":  // Unsigned short, 2 bytes fixed
                        {
                            // we have a 2 byte value length
                            length = reader.ReadUInt16();
                            // record the start of our value
                            position = stream.Position;
                            // position ourselves at the end of the element/start of the next
                            stream.Position += (long)length;
                        }
                        break;
                    default:
                        throw new Exception(String.Format("Invalid vr at tag={0}", this.tag.Name));
                }
                if (length > 0x7FFFFFFF && length < 0xFFFFFFFF)
                {
                    throw new Exception("Encountered negative length.");
                }
            }
            else if (syntax == Syntax.ImplicitVrLittleEndian)
            {
                // all tags have a 4 byte length
                length = reader.ReadUInt32();
                if (length > 0x7FFFFFFF && length < 0xFFFFFFFF)
                {
                    throw new Exception("Encountered negative length.");
                }

                // record the start of our value
                position = stream.Position;

                vr = tag.VR.Split(",".ToCharArray())[0];

                if ("SQ" != vr)
                {
                    // position ourselves at the end of the element/start of the next
                    stream.Position += (long)length;
                }
            }
            else
            {
                Debug.Assert(false, "Unsupported syntax {0}.", syntax);
            }
            if (length != 0xffffffff && length % 2 != 0)
            {
                Logging.Log(LogLevel.Warning, "Odd length of {0} for {1}", length, tag.ToString());
            }
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override long Write(Stream stream)
        {
            return Write(stream, Syntax.ImplicitVrLittleEndian, SpecificCharacterSet.Default, new DataSetOptions());
        }

        /// <summary>
        /// Writes the Element to the stream with a syntax and character encoding.
        /// </summary>
        /// <param name="stream">The stream to write to.</param>
        /// <param name="syntax">The TransferSyntax to use when writing to the stream.</param>
        /// <param name="encoding">The character encoding to use when writing to the stream.</param>
        /// <options></options>
        /// <returns></returns>
        public long Write(Stream stream, string syntax, SpecificCharacterSet encoding, DataSetOptions options)
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Syntax.GetEndian(syntax));
            long start = stream.Position;

            writer.Write(tag.Group);
            writer.Write(tag.Element);

            if (syntax == Syntax.Unknown)
            {
                syntax = Syntax.ImplicitVrLittleEndian;
            }
            if (Syntax.IsExplicit(syntax))
            {
                writer.Write(vr.ToCharArray());
            }
            else if (syntax != Syntax.ImplicitVrLittleEndian)
            {
                Debug.Assert(false, "Unsupported syntax {0}.", syntax);
            }

            // PS 3.5-2009, Page 39
            // 7.1.2 DATA ELEMENT STRUCTURE WITH EXPLICIT VR and
            // 7.1.3 DATA ELEMENT STRUCTURE WITH IMPLICIT VR
            if (Syntax.IsExplicit(syntax))
            {
                uint l;
                // TODO allow OB, OW, OF, SQ and UN to have undefined lengths
                switch (vr)
                {
                    case "OB":  // Other Byte String
                        if (tag.Equals(t.PixelData) && Syntax.CanEncapsulatePixelData(syntax))
                        {
                            // write out 2 reserved bytes
                            writer.Write((short)0);
                            // uses the 4 byte value length
                            writer.Write(0xFFFFFFFF);
                            break;
                        }
                        goto case "UN";
                    case "OF":  // Other Float String
                    case "OW":  // Other Word String
                    case "UN":  // Unknown, any length valid for any other VR
                    case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                        // write out 2 reserved bytes
                        writer.Write((short)0);
                        // uses the 4 byte value length
                        l = GetEncodedLength(syntax, encoding);
                        writer.Write(l);
                        break;
                    case "SQ":  // Sequence
                        {
                            writer.Write((short)0);
                            // uses the 4 byte value length
                            length = GetEncodedLength(syntax, encoding);
                            writer.Write(length);
                        }
                        break;
                    default:
                        l = GetEncodedLength(syntax, encoding);
                        writer.Write((short)l);
                        break;
                }
            }
            else
            {
                // TODO allow undefined lengths
                // uses a 2 byte value length
                uint count = GetEncodedLength(syntax, encoding);
                writer.Write(count);
                if (vr == "SQ")
                {
                    length = count;
                }
            }

            if (length > 0)
            {
                WriteValueOnStream(writer.BaseStream, syntax, encoding, options);
            }

            return stream.Position - start;
        }

        /// <summary>
        /// Writes the element value to a file.
        /// </summary>
        /// <param name="path">The path to the file to create or overwrite.</param>
        /// <remarks>If text, the element value is written in the default SpecificCharaterSet.  If binary, the
        /// value is written as little endian.</remarks>
        public void WriteValueOnStream(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                WriteValueOnStream(stream, Syntax.ImplicitVrLittleEndian, SpecificCharacterSet.Default, new DataSetOptions());
                stream.Flush();
            }
        }

        /// <summary>
        /// Writes the element value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write that positioned at the proper location to write.</param>
        /// <param name="syntax">The tranfer syntax to use if the value is binary.</param>
        /// <param name="encoding">The specific character set to use if the value is text.</param>
        /// <param name="options">The options used to encode tags.</param>
        protected virtual void WriteValueOnStream(Stream stream, string syntax, SpecificCharacterSet encoding, DataSetOptions options)
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Syntax.GetEndian(syntax));
            switch (vr)
            {
                case "AE":  // Application Entity, 16 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "AS":  // Age string, 4 bytes fixed
                    {
                        // I have seen no instances of vm > 1 for this type
                        // but I am doing this any way.
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "AT":  // Attribute tag, 4 bytes fixed
                    {
                        if (Value is Array)
                        {
                            Tag[] tags = value as Tag[];
                            foreach (Tag temp in tags)
                            {
                                writer.Write(temp.Group);
                                writer.Write(temp.Element);
                            }
                        }
                        else
                        {
                            Tag temp = (Tag)Value;
                            writer.Write(temp.Group);
                            writer.Write(temp.Element);
                        }
                    }
                    break;
                case "CS":  // Code String, 16 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "DA":  // Date, 8 bytes fixed
                    // TODO the only reason that I am padding this is
                    // for MWL date, a plain date would not need to be padded.
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "DS":  // Decimal String, 16 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "DT":  // Date Time, 26 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "FD":  // Floating Point Double, 8 bytes fixed
                    if (Value is Array)
                    {
                        double[] temp = Value as double[];
                        foreach (double d in temp)
                        {
                            writer.Write(d);
                        }
                    }
                    else
                    {
                        writer.Write((Double)Value);
                    }
                    break;
                case "FL":  // Floating Point Single, 4 bytes fixed
                    if (Value is Array)
                    {
                        float[] temp = Value as float[];
                        foreach (float f in temp)
                        {
                            writer.Write(f);
                        }
                    }
                    else
                    {
                        writer.Write((float)Value);
                    }
                    break;
                case "IS":  // Integer String, 12 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "OB":  // Other Byte String
                    {
                        byte[] value = Value as byte[];
                        writer.Write((byte[])value);
                        if (value.Length % 2 != 0)
                        {
                            writer.Write('\0');
                        }
                    }
                    break;
                case "OF":  // Other Float String
                    {
                        float[] floats = Value as float[];
                        foreach (float single in floats)
                        {
                            writer.Write(single);
                        }
                    }
                    break;
                case "OW":  // Other Word String
                    {
                        short[] words = Value as short[];
                        foreach (short word in words)
                        {
                            writer.Write(word);
                        }
                    }
                    break;
                case "SL":  // Signed Long, 4 bytes fixed
                    if (Value is Array)
                    {
                        int[] temp = Value as int[];
                        foreach (int f in temp)
                        {
                            writer.Write(f);
                        }
                    }
                    else
                    {
                        writer.Write((int)Value);
                    }
                    break;
                case "SS":  // Signed Short, 2 bytes fixed
                    if (Value is Array)
                    {
                        short[] temp = Value as short[];
                        foreach (short f in temp)
                        {
                            writer.Write(f);
                        }
                    }
                    else
                    {
                        writer.Write((short)Value);
                    }
                    break;
                case "TM":  // Time, 16 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                case "UI":  // Unique Identifier, 64 bytes maximum
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(temp[n].ToCharArray());
                            }
                        }
                        else
                        {
                            string temp = Value as String;
                            count = temp.Length;
                            writer.Write(temp.ToCharArray());
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write('\0');
                        }
                    }
                    break;
                case "UL":  // Unsigned Long, 4 bytes fixed
                    if (Value is Array)
                    {
                        uint[] temp = Value as uint[];
                        foreach (uint u in temp)
                        {
                            writer.Write(u);
                        }
                    }
                    else
                    {
                        writer.Write((uint)Value);
                    }
                    break;
                case "UN":  // Unknown, any length valid for any other VR
                    {
                        byte[] value = Value as byte[];
                        writer.Write((byte[])value);
                        // TODO should this be padded?
                        if (value.Length % 2 != 0)
                        {
                            writer.Write('\0');
                        }
                    }
                    break;
                case "US":  // Unsigned short, 2 bytes fixed
                    if (Value is Array)
                    {
                        // TODO there may be a quicker way for big arrays
                        ushort[] temp = value as ushort[];
                        foreach (ushort us in temp)
                        {
                            writer.Write((ushort)us);
                        }
                    }
                    else
                    {
                        writer.Write((ushort)Value);
                    }
                    break;
                case "LO":  // Long String, 64 characters maximum
                case "LT":  // Long Text, 10240 characters maximum, vm = 1 always
                case "PN":  // Person's Name, 64 character maximum per component group
                case "SH":  // Short String, 16 characters maximum
                case "ST":  // Short Text, 1024 characters maximum
                case "UT":  // Unlimited Text, 2^32-2 bytes maximum, vm = 1 always
                    {
                        int count = 0;
                        if (Value is Array)
                        {
                            string[] temp = value as string[];
                            for (int n = 0; n < temp.Length; n++)
                            {
                                if (n > 0)
                                {
                                    count++;
                                    writer.Write('\\');
                                }
                                count += temp[n].Length;
                                writer.Write(encoding.GetBytes(temp[n], vr));
                            }
                        }
                        else
                        {
                            string temp = value.ToString();
                            count = encoding.GetByteCount(temp, vr);
                            writer.Write(encoding.GetBytes(temp, vr));
                        }
                        if (count % 2 != 0)
                        {
                            writer.Write(' ');
                        }
                    }
                    break;
                default:
                    throw new Exception("");
            }
        }

        private uint GetEncodedLength(string syntax, SpecificCharacterSet encoding)
        {
            uint count = 0;
            if (vr == "SQ")
            {
                count = GetSize(syntax, encoding) - GetFront(syntax, encoding);
            }
            else if (vr == "LO" || vr == "LT" || vr == "PN" || vr == "SH" || vr == "ST" || vr == "UT")
            {
                if (Value is Array)
                {
                    string[] temp = value as string[];
                    for (int n = 0; n < temp.Length; n++)
                    {
                        if (n > 0)
                        {
                            count++;
                        }
                        count += (uint)encoding.GetByteCount(temp[n], vr);
                    }
                }
                else
                {
                    if (value != null)
                    {
                        string temp = value.ToString();
                        count = (uint)encoding.GetByteCount(temp, vr);
                    }
                }
            }
            else
            {
                count = length;
            }
            if (count % 2 != 0)
            {
                count++;
            }
            return count;
        }

        /// <summary>
        /// The size in bytes of the Element if it were to be written.
        /// </summary>
        /// <param name="syntax">The TransferSyntax that will be used to write to the stream.</param>
        /// <param name="encoding">The encoding with which this element will be written.</param>
        /// <returns>The size in bytes of the Element.</returns>
        public virtual uint GetSize(string syntax, SpecificCharacterSet encoding)
        {
            return GetFront(syntax, encoding);
        }

        /// <summary>
        /// This method attempts to change the VR based on the input only for tags that could have more than one VR
        /// </summary>
        /// <param name="value"></param>
        private void SelectValueType(object value)
        {
            // if the tag is not in the dictionary, or does not have a more than one value, do not change the vr
            if (Dictionary.Contains(tag.Name))
            {
                if (tag.VR.Contains(","))
                {
                    Array array = value as Array;
                    // TODO does this cover all variable vrs
                    if (array is byte[])
                    {
                        vr = "OB";
                    }
                    else if (array is ushort[])
                    {
                        vr = "OW";
                    }
                }
            }
        }

        /// <summary>
        /// The Value of the Element.
        /// </summary>
        /// <remarks>If a tag has a VM > 1 as defined by Dicom, the Value will be an Array.</remarks>
        public virtual object Value
        {
            get
            {
                return value;
                /*object result = value;
                switch (vr)
                {
                    case "AE":  // Application Entity, 16 bytes maximum
                        result = StripStringValue(value, TrimType.Both);
                        break;
                    case "AS":  // Age string, 4 bytes fixed
                        result = StripStringValue(value, TrimType.Both);
                        break;
                    case "CS":  // Code String, 16 bytes maximum
                        result = StripStringValue(value, TrimType.Both);
                        break;
                    case "DA":  // Date, 8 bytes fixed
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "DS":  // Decimal String, 16 bytes maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "DT":  // Date Time, 26 bytes maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "IS":  // Integer String, 12 bytes maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "LO":  // Long String, 64 characters maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "LT":  // Long Text, 10240 characters maximum
                        result = StripStringValue(value, TrimType.Trailing);
                        break;
                    case "PN":  // Person's Name, 64 character maximum per component group
                        result = StripStringValue(value, TrimType.Trailing);
                        break;
                    case "SH":  // Short String, 16 characters maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "ST":  // Short Text, 1024 characters maximum
                        result = StripStringValue(value, TrimType.Trailing);
                        break;
                    case "TM":  // Time, 16 bytes maximum
                        result = StripStringValue(value, TrimType.None);
                        break;
                    case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                        result = StripStringValue(value, TrimType.Trailing);
                        break;

                    case "AT":  // Attribute tag, 4 bytes fixed
                    case "FL":  // Floating Point Single, 4 bytes fixed
                    case "FD":  // Floating Point Double, 8 bytes fixed
                    case "OB":  // Other Byte String
                    case "OF":  // Other Float String
                    case "OW":  // Other Word String
                    case "SL":  // Signed Long, 4 bytes fixed
                    case "SQ":  // Sequence
                    case "SS":  // Signed Short, 2 bytes fixed
                    case "UI":  // Unique Identifier, 64 bytes maximum
                    case "UL":  // Unsigned Long, 4 bytes fixed
                    case "UN":  // Unknown, any length valid for any other VR
                    case "US":  // Unsigned short, 2 bytes fixed
                    default:
                        break;
                }
                return result;/**/
            }

            set
            {
                if (value == null)
                {
                    this.value = value;
                    // set the length
                    length = 0;
                    return;
                }
                object temp = (value is string) ? ParseString(value as string) : value;
                SelectValueType(value);
                switch (vr)
                {
                    case "AS":  // Age string, 4 bytes fixed
                    case "AE":  // Application Entity, 16 bytes maximum
                    case "CS":  // Code String, 16 bytes maximum
                    case "DA":  // Date, 8 bytes fixed
                    case "DS":  // Decimal String, 16 bytes maximum
                    case "DT":  // Date Time, 26 bytes maximum
                    case "IS":  // Integer String, 12 bytes maximum
                    case "TM":  // Time, 16 bytes maximum
                        SetStringValue(temp, TrimType.Both);
                        break;
                    case "LO":  // Long String, 64 characters maximum
                    case "SH":  // Short String, 16 characters maximum
                        SetStringValue(temp, TrimType.None);
                        break;
                    case "ST":  // Short Text, 1024 characters maximum
                        SetStringValue(temp, TrimType.Trailing);
                        break;
                    case "UI":  // Unique Identifier, 64 bytes maximum
                    case "PN":  // Person's Name, 64 character maximum per component group
                    case "LT":  // Long Text, 10240 characters maximum
                    case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                        SetStringValue(temp, TrimType.Both);
                        break;
                    case "FL":  // Floating Point Single, 4 bytes fixed
                        {
                            float[] array = temp as float[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(float) * array.Length);
                                this.value = array;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(float);
                                this.value = Convert.ToSingle(temp.ToString());
                            }
                        }
                        break;
                    case "FD":  // Floating Point Double, 8 bytes fixed
                        {
                            double[] array = temp as double[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(double) * array.Length);
                                this.value = array;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(double);
                                this.value = Convert.ToDouble(temp.ToString());
                            }
                        }
                        break;
                    case "SL":  // Signed Long, 4 bytes fixed
                        {
                            int[] array = temp as int[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(int) * array.Length);
                                this.value = temp;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(int);
                                this.value = Convert.ToInt32(temp.ToString());
                            }
                        }
                        break;
                    case "SS":  // Signed Short, 2 bytes fixed
                        {
                            short[] array = temp as short[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(short) * array.Length);
                                this.value = temp;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(short);
                                this.value = Convert.ToInt16(temp.ToString());
                            }
                        }
                        break;
                    case "UL":  // Unsigned Long, 4 bytes fixed
                        {
                            uint[] array = temp as uint[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(uint) * array.Length);
                                this.value = temp;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(uint);
                                this.value = Convert.ToUInt32(temp.ToString());
                            }
                        }
                        break;
                    case "US":  // Unsigned short, 2 bytes fixed
                        {
                            ushort[] array = temp as ushort[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(ushort) * array.Length);
                                this.value = array;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(ushort);
                                this.value = Convert.ToUInt16(temp.ToString());
                            }
                        }
                        break;
                    case "AT":  // Attribute tag, 4 bytes fixed
                        {
                            Tag[] array = temp as Tag[];
                            if (array != null)
                            {
                                vm = (uint)array.Length;
                                length = (uint)(sizeof(short) * 2 * array.Length);
                                this.value = array;
                            }
                            else
                            {
                                vm = 1;
                                length = sizeof(short) * 2;
                                this.value = Tag.Parse(temp.ToString());
                            }
                        }
                        break;
                    case "SQ":  // Sequence
                        throw new Exception("A Sequence cannot have a Value.");
                    case "OB":  // Other Byte String
                        SetArrayValue(temp as byte[]);
                        break;
                    case "OF":  // Other Float String
                        SetArrayValue(temp as float[]);
                        break;
                    case "OW":  // Other Word String
                        SetArrayValue(temp as short[]);
                        break;
                    case "UN":  // Unknown, any length valid for any other VR
                        SetArrayValue(temp as byte[]);
                        break;
                    default:
                        throw new Exception(String.Format("Unknown vr {0}.", vr));
                }
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return new Element(this.Group, this.element, this.vr, this.value);
        }

        #endregion

        #region Private Methods

        // sets the value multiplicity for each non-string type
        // to set the vm for string types you have to call GetValueFromStream
        protected ushort ValueMultiplicity()
        {
            ushort count = 1;
            switch (vr)
            {
                case "AE":  // Application Entity, 16 bytes maximum
                case "AS":  // Age string, 4 bytes fixed
                case "CS":  // Code String, 16 bytes maximum
                case "DA":  // Date, 8 bytes fixed
                case "DS":  // Decimal String, 16 bytes maximum
                case "DT":  // Date Time, 26 bytes maximum
                case "IS":  // Integer String, 12 bytes maximum
                case "LO":  // Long String, 64 characters maximum
                case "PN":  // Person's Name, 64 character maximum per component group
                case "SH":  // Short String, 16 characters maximum
                case "ST":  // Short Text, 1024 characters maximum
                case "TM":  // Time, 16 bytes maximum
                case "UI":  // Unique Identifier, 64 bytes maximum
                    // TODO we have to read the tag to figure out the vm for strings
                    vm = 1;
                    break;

                case "FL":  // Floating Point Single, 4 bytes fixed
                    vm = length / sizeof(float);
                    break;
                case "FD":  // Floating Point Double, 8 bytes fixed
                    vm = length / sizeof(double);
                    break;
                case "SL":  // Signed Long, 4 bytes fixed
                    vm = length / sizeof(int);
                    break;
                case "SS":  // Signed Short, 2 bytes fixed
                    vm = length / sizeof(short);
                    break;
                case "UL":  // Unsigned Long, 4 bytes fixed
                    vm = length / sizeof(uint);
                    break;
                case "US":  // Unsigned short, 2 bytes fixed
                    vm = length / sizeof(ushort);
                    break;
                case "AT":  // Attribute tag, 4 bytes fixed
                    vm = length / (sizeof(short) + sizeof(short));
                    break;
                case "SQ":  // Sequence
                    vm = 1;
                    break;
                case "OB":  // Other Byte String
                case "OF":  // Other Float String
                case "OW":  // Other Word String
                case "LT":  // Long Text, 10240 characters maximum
                case "UN":  // Unknown, any length valid for any other VR
                case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                    vm = 1;
                    break;
                default:
                    //throw new Exception(String.Format("Unknown vr {0}.", tag.VR));
                    break;
            }
            return count;
        }

        private object ConvertToArray(object value)
        {
            if (value == null)
            {
                return null;
            }
            System.Type type = value.GetType();
            switch (type.Name)
            {
                case "Float":
                    value = new float[] { (float)value };
                    break;
                case "Double":
                    value = new double[] { (double)value };
                    break;
                case "Int32":
                    value = new int[] { (int)value };
                    break;
                case "Int16":
                    value = new short[] { (short)value };
                    break;
                case "UInt32":
                    value = new uint[] { (uint)value };
                    break;
                case "UShort":
                    value = new ushort[] { (ushort)value };
                    break;
                case "String":
                    value = new string[] { (string)value };
                    break;
                case "Tag":
                    value = new Tag[] { (Tag)value };
                    break;
                default:
                    throw new Exception(String.Format("{0}, Unsupported type in ConvertToArray", type.Name));
            }
            return value;
        }

        /// <summary>
        /// Returns the number of bytes, excluding the actual value, that could be written to a stream.
        /// </summary>
        /// <param name="syntax">The TransferSyntax that will be used to write to the stream.</param>
        /// <param name="encoding">The encoding with which this element will be written.</param>
        /// <returns></returns>
        protected uint GetFront(string syntax, SpecificCharacterSet encoding)
        {
            uint size = 0;
            if (Syntax.IsExplicit(syntax))
            {
                switch (vr)
                {
                    case "OB":  // Other Byte String
                    case "OF":  // Other Float String
                    case "OW":  // Other Word String
                    case "UN":  // Unknown, any length valid for any other VR
                    case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                        // group, element, vr, reserved, and a 4 byte value length
                        size = sizeof(short) + sizeof(short) + sizeof(byte) * 2 + sizeof(short) + sizeof(int) + GetEncodedLength(syntax, encoding);
                        break;
                    case "SQ":
                        // group, element, vr, 2 reserved and a 4 byte value length
                        // the size will be calculated below
                        size = sizeof(short) + sizeof(short) + sizeof(byte) * 2 + sizeof(short) + sizeof(int);
                        break;
                    default:
                        // group, element, vr, and a 2 byte value length
                        size = sizeof(short) + sizeof(short) + sizeof(byte) * 2 + sizeof(short) + GetEncodedLength(syntax, encoding);
                        break;
                }
            }
            else
            {
                switch (vr)
                {
                    case "SQ":
                        // group, element, and a 4 byte value length
                        size = sizeof(short) + sizeof(short) + sizeof(int);
                        // the size will be further calculated below
                        break;
                    default:
                        // group, element, and a 4 byte value length
                        size = sizeof(short) + sizeof(short) + sizeof(int) + GetEncodedLength(syntax, encoding);
                        break;
                }
            }
            return size;
        }

        /// <summary>
        /// Sets the element value from the contents of a file.
        /// </summary>
        /// <param name="path">The path to the file to read.</param>
        /// <remarks>Assumes the element has been properly initialized with the proper VR, etc. If the value is text, 
        /// the default SpecificCharacterSet is used to decode the string.  If the value is bainry, the bianry data is 
        /// interpreted as little endian.</remarks>
        public void ReadValueFromStream(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                stream.Seek(0, SeekOrigin.End);
                uint length = (uint)stream.Length;
                stream.Seek(0, SeekOrigin.Begin);
                ReadValueFromStream(stream, Syntax.ImplicitVrLittleEndian, SpecificCharacterSet.Default, length);
            }
        }

        /// <summary>
        /// TODO this is too much like multiplicity(), it seems to take both methods to set vm
        /// perhaps one should figure vm while the other determines if the result should be an array or single item
        /// </summary>
        /// <returns></returns>
        private uint InferValueMultiplicity()
        {
            switch (vr)
            {
                case "AE":  // Application Entity, 16 bytes maximum
                case "CS":  // Code String, 16 bytes maximum
                case "DS":  // Decimal String, 16 bytes maximum
                case "DT":  // Date Time, 26 bytes maximum
                case "IS":  // Integer String, 12 bytes maximum
                case "TM":  // Time, 16 bytes maximum
                case "PN":  // Person's Name, 64 character maximum per component group
                case "LO":  // Long String, 64 characters maximum
                case "SH":  // Short String, 16 characters maximum
                case "UI":  // Unique Identifier, 64 bytes maximum
                    vm = 2; // number of \;
                    break;
                case "DA":  // Date, 8 bytes fixed
                case "FD":  // Floating Point Double, 8 bytes fixed
                    vm = length / 8;
                    break;
                case "AS":  // Age string, 4 bytes fixed, no examples of vm > 1 in spec
                case "AT":  // Attribute tag, 4 bytes fixed
                case "FL":  // Floating Point Single, 4 bytes fixed
                case "SL":  // Signed Long, 4 bytes fixed
                case "UL":  // Unsigned Long, 4 bytes fixed
                    vm = length / 4;
                    break;
                case "SS":  // Signed Short, 2 bytes fixed
                case "US":  // Unsigned short, 2 bytes fixed
                    vm = length / 2;
                    break;
                case "LT":  // Long Text, 10240 characters maximum
                case "OB":  // Other Byte String
                case "OF":  // Other Float String
                case "OW":  // Other Word String
                case "SQ":  // Sequence
                case "ST":  // Short Text, 1024 characters maximum
                case "UN":  // Unknown, any length valid for any other VR
                case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                default:
                    vm = 1;
                    break;
            }
            return vm;
       }

        /// <summary>
        /// This method reads the value from the stream and sets the vm for string types
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <param name="syntax">The transfer syntax used to interpret binary data.</param>
        /// <param name="encoding">The encoding to use to decode string data.</param>
        /// <param name="length">The length, in bytes, of the data to read.</param>
        /// <remarks>The stream is assumed to be positioned at the beginning of the encoded tag value.</remarks>
        protected virtual void ReadValueFromStream(Stream stream, string syntax, SpecificCharacterSet encoding, uint length)
        {
            this.length = length;

            EndianBinaryReader reader = new EndianBinaryReader(stream, Syntax.GetEndian(syntax));
            bool multiple = (vm != 1 || tag.VM != "1");
            // if the tag is not in the dictionary, tag.VM = "?"
            if (tag.VM == "?")
            {
                vm = InferValueMultiplicity();
                multiple = (vm != 1);
            }
            switch (vr)
            {
                case "CS":  // Code String, 16 bytes maximum
                    value = ReadString(reader, vr, SpecificCharacterSet.Default, TrimType.Both, multiple);
                    break;
                case "AE":  // Application Entity, 16 bytes maximum
                case "AS":  // Age string, 4 bytes fixed
                case "DA":  // Date, 8 bytes fixed
                case "DS":  // Decimal String, 16 bytes maximum
                case "DT":  // Date Time, 26 bytes maximum
                case "IS":  // Integer String, 12 bytes maximum
                case "TM":  // Time, 16 bytes maximum
                    value = ReadString(reader, vr, SpecificCharacterSet.Default, TrimType.None, multiple);
                    break;
                case "PN":  // Person's Name, 64 character maximum per component group
                    value = ReadString(reader, vr, encoding, TrimType.None, multiple);
                    break;
                case "LO":  // Long String, 64 characters maximum
                case "SH":  // Short String, 16 characters maximum
                    value = ReadString(reader, vr, encoding, TrimType.Both, multiple);
                    break;
                case "ST":  // Short Text, 1024 characters maximum
                    value = ReadString(reader, vr, encoding, TrimType.None, multiple);
                    break;
                case "LT":  // Long Text, 10240 characters maximum
                case "UT":  // Unlimited Text, 2^32-2 bytes maximum
                    value = ReadString(reader, vr, encoding, TrimType.None, multiple);
                    break;
                case "UI":  // Unique Identifier, 64 bytes maximum
                    {
                        value = ReadString(reader, vr, SpecificCharacterSet.Default, TrimType.Both, multiple);
                        string[] strings = value as string[];
                        if (strings != null)
                        {
                            for (int n = 0; n < strings.Length; n++)
                            {
                                strings[n] = strings[n].TrimEnd("\0".ToCharArray());
                            }
                        }
                        else
                        {
                            value = ((string)value).TrimEnd("\0".ToCharArray());
                        }
                    }
                    break;
                case "FL":  // Floating Point Single, 4 bytes fixed
                    if (multiple)
                    {
                        float[] temp = new float[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadSingle();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadSingle();
                    }
                    break;
                case "FD":  // Floating Point Double, 8 bytes fixed
                    if (multiple)
                    {
                        double[] temp = new double[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadDouble();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadDouble();
                    }
                    break;
                case "SL":  // Signed Long, 4 bytes fixed
                    if (multiple)
                    {
                        int[] temp = new int[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadInt32();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadInt32();
                    }
                    break;
                case "SS":  // Signed Short, 2 bytes fixed
                    if (multiple)
                    {
                        short[] temp = new short[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadInt16();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadInt16();
                    }
                    break;
                case "UL":  // Unsigned Long, 4 bytes fixed
                    if (multiple)
                    {
                        uint[] temp = new uint[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadUInt32();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadUInt32();
                    }
                    break;
                case "US":  // Unsigned short, 2 bytes fixed
                    if (multiple)
                    {
                        ushort[] temp = new ushort[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            temp[n] = reader.ReadUInt16();
                        }
                        value = temp;
                    }
                    else
                    {
                        value = reader.ReadUInt16();
                    }
                    break;
                case "AT":  // Attribute tag, 4 bytes fixed
                    if (multiple)
                    {
                        Tag[] tags = new Tag[vm];
                        for (int n = 0; n < vm; n++)
                        {
                            short group, element;
                            group = reader.ReadInt16();
                            element = reader.ReadInt16();
                            tags[n] = new Tag(group, element);
                        }
                        value = tags;
                    }
                    else
                    {
                        short group, element;
                        group = reader.ReadInt16();
                        element = reader.ReadInt16();
                        value = new Tag(group, element);
                    }
                    break;
                case "OF":  // Other Float String
                    {
                        int size = (int)length / sizeof(float);
                        float[] floats = new float[size];
                        for (int n = 0; n < size; n++)
                        {
                            floats[n] = reader.ReadSingle();
                        }
                        value = floats;
                    }
                    break;
                case "OW":  // Other Word String
                    {
                        int size = (int)length / sizeof(short);
                        value = reader.ReadWords(size);
                    }
                    break;
                case "OB":  // Other Byte String
                    {
                        value = reader.ReadBytes((int)length);
                    }
                    break;
                case "UN":  // Unknown, any length valid for any other VR
                    {
                        value = reader.ReadBytes((int)length);
                    }
                    break;
            }
        }

        private object ParseString(string value)
        {
            object result = null;
            string[] text = value.Split("\\".ToCharArray());

            // first we will take a look at vrs that we don't support
            // and those that cannot have vm > 1
            switch (vr)
            {
                // these are currently unsupported
                case "AT":
                case "OB":
                case "OF":
                case "OW":
                case "UN":
                case "SQ":
                    throw new Exception("Parsing this type is unsupported.");

                // these cannot have vm > 1
                case "UT":
                case "LT":
                case "ST":
                    result = value;
                    break;
                // the remaining vrs can have multiple values delimited with '\'
                // these can have vm > 1
                case "AE":
                case "CS":
                case "AS":  // "0"-"9","D","W","M","Y"
                case "DA":  // "0"-"9","-"
                case "DT":  // "0"-"9","+","-","."," "
                case "LO":
                case "PN":
                case "SH":
                case "TM":  // "0"-"9",".","-"
                case "UI":  // "0"-"9","."
                    {
                        result = text;
                    }
                    break;

                // these are strings but can be validated further
                case "DS":  // "0"-"9","+","-","E","e","."
                    {
                        for (int n = 0; n < text.Length; n++)
                        {
                            double temp = Double.Parse(text[n]);
                        }
                        result = text;
                    }
                    break;
                case "IS":  // "0"-"9","+","-"
                    {
                        for (int n = 0; n < text.Length; n++)
                        {
                            int temp = Int32.Parse(text[n]);
                        }
                        result = text;
                    }
                    break;
                case "FD":
                    {
                        result = new double[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((double[])result)[n] = double.Parse(text[n]);
                        }
                    }
                    break;
                case "FL":
                    {
                        result = new float[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((float[])result)[n] = float.Parse(text[n]);
                        }
                    }
                    break;
                case "SL":
                    {
                        result = new int[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((int[])result)[n] = int.Parse(text[n]);
                        }
                    }
                    break;
                case "SS":
                    {
                        result = new short[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((short[])result)[n] = short.Parse(text[n]);
                        }
                    }
                    break;
                case "UL":
                    {
                        result = new uint[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((uint[])result)[n] = uint.Parse(text[n]);
                        }
                    }
                    break;
                case "US":
                    {
                        result = new ushort[text.Length];
                        for (int n = 0; n < text.Length; n++)
                        {
                            ((ushort[])result)[n] = ushort.Parse(text[n]);
                        }
                    }
                    break;
            }
            // if we have an array of one thing, just return that one thing
            if (result is Array)
            {
                if (((Array)result).Length == 1)
                    result = ((Array)result).GetValue(0);
            }
            return result;
        }

        private void SetArrayValue<T>(T[] value)
        {
            if (value == null)
            {
                throw new Exception("Value must be set to an array.");
            }
            vm = (uint)value.Length;
            length = (uint)(value.Length * Marshal.SizeOf(default(T)));
            this.value = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="trim"></param>
        /// <returns></returns>
        private object StripStringValue(object value, TrimType trim)
        {
            if (trim == TrimType.None)
            {
                return value;
            }
            object result;
            string[] temp = value as string[];
            if (temp != null)
            {
                result = temp.Clone();
                vm = (uint)temp.Length;
                length = 0;
                for (int n = 0; n < vm; n++)
                {
                    string text = ((string[])result)[n];
                    switch (trim)
                    {
                        case TrimType.Both:
                            text = text.Trim();
                            break;
                        case TrimType.Leading:
                            text = text.TrimStart();
                            break;
                        case TrimType.Trailing:
                            text = text.TrimEnd();
                            break;
                    }
                    if (length > 0)
                    {
                        length++;
                    }
                    length += (uint)text.Length;
                }
            }
            else
            {
                vm = 1;
                result = value.ToString();
                switch (trim)
                {
                    case TrimType.Both:
                        result = ((string)result).Trim();
                        break;
                    case TrimType.Leading:
                        result = ((string)result).TrimStart();
                        break;
                    case TrimType.Trailing:
                        result = ((string)result).TrimEnd();
                        break;
                }
            }
            return result;
        }

        private void SetStringValue(object value, TrimType trim)
        {
            string[] temp = value as string[];
            if (temp != null)
            {
                vm = (uint)temp.Length;
                length = 0;
                for (int n = 0; n < vm; n++)
                {
                    string text = temp[n];
                    //switch (trim)
                    //{
                    //    case TrimType.Both:
                    //        text = text.Trim();
                    //        break;
                    //    case TrimType.Leading:
                    //        text = text.TrimStart();
                    //        break;
                    //    case TrimType.Trailing:
                    //        text = text.TrimEnd();
                    //        break;
                    //}
                    if (length > 0)
                    {
                        length++;
                    }
                    length += (uint)text.Length;
                }
                this.value = value;
            }
            else
            {
                vm = 1;
                string text = value.ToString();
                //switch (trim)
                //{
                //    case TrimType.Both:
                //        text = text.Trim();
                //        break;
                //    case TrimType.Leading:
                //        text = text.TrimStart();
                //        break;
                //    case TrimType.Trailing:
                //        text = text.TrimEnd();
                //        break;
                //}
                this.value = text;
                length = (uint)text.Length;
            }
            if (length % 2 > 0)
            {
                length++;
            }
        }

        /// <summary>
        /// Returns the value as either a string or an array of strings
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="vr"></param>
        /// <param name="encoding"></param>
        /// <param name="trim"></param>
        /// <param name="multiple">Determines whether the answer will be an array of strings or a single string.</param>
        /// <returns></returns>
        /// <remarks>Also sets the length and vm based on what was returned.</remarks>
        private object ReadString(BinaryReader reader, String vr, SpecificCharacterSet encoding, TrimType trim, bool multiple)
        {
            byte[] bytes = reader.ReadBytes((int)length);
            string text = encoding.GetString(bytes, vr);
            length = (uint)text.Length;
            object result = text;
            if (!multiple)
            {
                switch (trim)
                {
                    case TrimType.Both:
                        result = text.Trim();
                        break;
                    case TrimType.Leading:
                        result = text.TrimStart();
                        break;
                    case TrimType.Trailing:
                        result = text.TrimEnd();
                        break;
                    default:
                        result = text;
                        break;
                }
                // recalculate the length based on trimming
                length = (uint)((string)result).Length;
                vm = 1;
            }
            else
            {
                string[] strings = text.Split('\\');
                if (trim > TrimType.None)
                {
                    // recalulate the length based on trimming
                    length = 0;
                    for (int n = 0; n < strings.Length; n++)
                    {
                        switch (trim)
                        {
                            case TrimType.Both:
                                strings[n] = strings[n].Trim();
                                break;
                            case TrimType.Leading:
                                strings[n] = strings[n].TrimStart();
                                break;
                            case TrimType.Trailing:
                                strings[n] = strings[n].TrimEnd();
                                break;
                        }
                        // accumulate the length of each string
                        length += (uint)strings[n].Length;
                    }
                    // account for each '\\' separator
                    length += (uint)strings.Length - 1;
                }
                vm = (uint)strings.Length;
                result = (object)strings;
            }
            return result;
        }

        protected string ToXmlTag()
        {
            string temp = Description;
            if (temp == String.Empty)
            {
                temp = "private";
            }
            else if (temp.IndexOfAny(",' /()".ToCharArray()) != -1)
            {
                temp = temp.Replace(",", String.Empty);
                temp = temp.Replace("'", String.Empty);
                temp = temp.Replace(" ", String.Empty);
                temp = temp.Replace("/", String.Empty);
                temp = temp.Replace("(", String.Empty);
                temp = temp.Replace(")", String.Empty);
            }
            return temp;
        }

        internal void Write(XmlTextWriter writer)
        {
            Write(writer, 0);
        }

        protected virtual void Write(XmlTextWriter writer, int depth)
        {
            string description = ToXmlTag();

            writer.WriteWhitespace(new String('\t', depth + 1));
            writer.WriteStartElement(description);
            writer.WriteAttributeString("tag", Tag.ToString());
            writer.WriteAttributeString("vr", vr.ToString());
            writer.WriteAttributeString("vm", Tag.VM.ToString());

            if (VR == "SQ")
            {
                int count = ((Sequence)this).Items.Count;
                for (int n = 0; n < count; n++)
                {
                    writer.WriteWhitespace("\r\n");
                    writer.WriteWhitespace(new String('\t', depth + 1));
                    writer.WriteStartElement("Item");
                    writer.WriteWhitespace("\r\n");

                    Elements item = ((Sequence)this).Items[n];
                    foreach (Element child in item)
                    {
                        child.Write(writer, depth + 1);
                    }

                    writer.WriteWhitespace(new String('\t', depth + 1));
                    writer.WriteEndElement();
                    writer.WriteWhitespace("\r\n");
                }

                writer.WriteWhitespace(new String('\t', depth + 1));
                writer.WriteEndElement();
                writer.WriteWhitespace("\r\n");
            }
            else
            {
                if (Value is Array)
                {
                    if (vr == "OW")
                    {
                        byte[] bytes = new byte[Length];
                        Buffer.BlockCopy((short[])Value, 0, bytes, 0, (int)Length);
                        writer.WriteBase64(bytes, (int)0, (int)Length);
                    }
                    else if (vr == "OB")
                    {
                        writer.WriteBase64((byte[])Value, (int)0, (int)Length);
                    }
                    else
                    {
                        StringBuilder temp = new StringBuilder();
                        foreach (object entry in Value as Array)
                        {
                            if (temp.Length > 0)
                            {
                                temp.Append("\\");
                            }
                            temp.Append(entry.ToString());
                        }
                        writer.WriteValue((value == null) ? String.Empty : temp.ToString());
                    }
                }
                else
                {
                    object value = Value;
                    writer.WriteValue((value == null) ? String.Empty : value.ToString());
                }
                writer.WriteEndElement();
                writer.WriteWhitespace("\r\n");
            }
        }

        #endregion Private Methods
    }

    public enum TrimType
    {
        None,
        Leading,
        Trailing,
        Both,
    }

}