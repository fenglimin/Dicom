using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class PixelData : Element, IEnumerable<byte[]>
    {

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public PixelData() :
            base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public PixelData(Element parent) :
            base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public PixelData(string key) :
            base(key, "OW")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        public PixelData(short group, short element) :
            base(group, element, "OW")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public PixelData(string key, object value) :
            base(key, value)
        {
            this.VR = "OB";
            this.length = 0xFFFFFFFF;
        }


        #endregion Constructors

        #region Frames

        /// <summary>
        /// The collection of frames
        /// </summary>
        public List<byte[]> Frames
        {
            get
            {
                if (!(value is List<byte[]>))
                {
                    throw new Exception("PixelData is not Encapsulted.");
                }
                return value as List<byte[]>;
            }
        }

        /// <summary>
        /// Adds an frame to the collection.
        /// </summary>
        /// <returns></returns>
        public byte[] AddFrame(byte[] frame)
        {
            Frames.Add(frame);
            return frame;
        }

        /// <summary>
        /// Removes an Item at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] RemoveFrameAt(int index)
        {
            byte[] frame = Frames[index];
            Frames.RemoveAt(index);
            return frame;
        }

        /// <summary>
        /// Inserts an Item at the specicified index.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public byte[] InsertFrame(byte[] frame, int index)
        {
            Frames.Insert(index, frame);
            return frame;
        }

        public void ReplaceFrame(byte[] frame, int index)
        {
            byte[] removed = Frames[index];
            Frames.RemoveAt(index);
            InsertFrame(frame, index);
        }

        #endregion Items

        #region Properties

        /// <summary>
        /// True if the PixelData is in Native format.
        /// </summary>
        public bool IsNative
        {
            get
            {
                return !IsEncapsulated;
            }
        }

        /// <summary>
        /// True is the PixelData is Encapsulated.
        /// </summary>
        public bool IsEncapsulated
        {
            get
            {
                return (value != null && value is List<byte[]>);
            }
        }

        #endregion Properties

        #region IEnumerable and etc.

        /// <summary>
        /// An enumerator for all frames.
        /// </summary>
        /// The enumerator iteerates all Items in a sequence.
        IEnumerator<byte[]> IEnumerable<byte[]>.GetEnumerator()
        {
            foreach (byte[] frame in Frames)
            {
                yield return frame;
            }
       }

        /// <summary>
        /// An enumerator for the collection of frames.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<uint[]>)this).GetEnumerator();
        }

        #endregion IEnumerable and etc.

        #region Overrides

        public override object Value
        {
            get
            {
                return value;
            }
            set
            {
                // if we are setting a List of frames, set the value directly
                if (value is List<byte[]>)
                {
                    this.value = value;
                }
                else
                {
                    // otherwise the base version is good
                    base.Value = value;
                }
            }
        }

        /// <summary>
        /// This method reads the value from the stream and sets the vm for string types
        /// </summary>
        /// <param name="stream">The stream to read the data from.</param>
        /// <param name="syntax">The transfer syntax used to interpret binary data.</param>
        /// <param name="encoding">The encoding to use to decode string data.</param>
        /// <param name="length">The length, in bytes, of the data to read.</param>
        /// <remarks>The stream is assumed to be positioned at the beginning of the encoded tag value.</remarks>
        protected override void ReadValueFromStream(Stream stream, string syntax, SpecificCharacterSet encoding, uint length)
        {
            this.length = length;

            EndianBinaryReader reader = new EndianBinaryReader(stream, Syntax.GetEndian(syntax));
            if (length == 0xFFFFFFFF)
            {
                if (!Syntax.CanEncapsulatePixelData(syntax))
                {
                    throw new Exception(String.Format("Current syntax {0} does not support Encapsulated Pixel Data.", syntax, this.tag.Name));
                }
                ReadEncapsulatedPixelData(reader);
            }
            else
            {
                ReadNativePixelData(reader);
            }
        }

        /// <summary>
        /// Writes the element value to the stream.
        /// </summary>
        /// <param name="stream">The stream to write that positioned at the proper location to write.</param>
        /// <param name="syntax">The tranfer syntax to use if the value is binary.</param>
        /// <param name="encoding">The specific character set to use if the value is text.</param>
        /// <param name="options">The options used to encode tags.</param>
        protected override void WriteValueOnStream(Stream stream, string syntax, SpecificCharacterSet encoding, DataSetOptions options)
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Syntax.GetEndian(syntax));
            if (Syntax.CanEncapsulatePixelData(syntax))
            {
                WriteEncapsulatedPixelData(writer);
            }
            else
            {
                if(vr == "OW")
                {
                    short[] words = Value as short[];
                    foreach (short word in words)
                    {
                        writer.Write(word);
                    }
                }
                else if (vr == "OB")
                {
                    byte[] value = Value as byte[];
                    writer.Write((byte[])value);
                    if (value.Length % 2 != 0)
                    {
                        writer.Write('\0');
                    }
                }
                else
                {
                    throw new Exception(String.Format("Unexpected vr={0} in WriteValueOnStream", vr));
                }
            }
        }

        /// <summary>
        /// Produce a string represetnation of the element.
        /// </summary>
        /// <returns>A string representation of the element.</returns>
        public override string Dump()
        {
            string text = null;
            if (IsNative)
            {
                text = String.Format("{0}:{1}:{2}:{3}:{4} bytes\n", this.Tag.ToString(), vr, this.VM, this.Description, (value!=null) ? ((short[])value).Length : 0);
            }
            else
            {
                text = String.Format("{0}:{1}:{2}:{3}:{4} frames\n", this.Tag.ToString(), vr, this.VM, this.Description, (value!=null) ? Frames.Count : 0);
            }
            return text;
        }

        #endregion Overrides

        /// <summary>
        /// Reads Native (un-encapsulated) PixelData
        /// </summary>
        /// <param name="reader"></param>
        private void ReadNativePixelData(EndianBinaryReader reader)
        {
            if (vr == "OW")
            {
                int size = (int)length / sizeof(short);
                value = reader.ReadWords(size);
            }
            else if (vr == "OB")
            {
                value = reader.ReadBytes((int)length);
            }
            else
            {
                throw new Exception(String.Format("Invalid vr {0}, expecting \"OW\" or \"OB\".", vr));
            }
        }

        /// <summary>
        /// Reads Encapsulated PixelData
        /// </summary>
        /// <param name="reader"></param>
        /// <remarks>PS 3.5-2009 A.4 TRANSFER SYNTAXES FOR ENCAPSULATION OF ENCODED PIXEL DATA</remarks>
        private void ReadEncapsulatedPixelData(EndianBinaryReader reader)
        {
            Stream stream = reader.BaseStream;

            value = new List<byte[]>();
            uint[] offsets = null;

            // encapsulated data is writen similarly to a sequence
            // each frame of the image is stored in an item
            // the offset of each frame is written in the first item

            // one or more Items (FFFE,E000) of even length, may be padded
            // first item is a Basic Offset Table of zero length if not present
            // terminated by a Sequence Delimiter Item (FFFE,E0DD)

            // not supported: each frame can be split into multiple fragments, each in its own item.
            // not supported: if multiple frames, the basic offset table contains offsets to the beginning of each item containing the first fragment of a frame,
            //      the offset is the length from the first item following the Basic Offset Table

            uint length;
            // get the first tag, expecting an Item
            Tag tag = new Tag(reader.ReadInt16(), reader.ReadInt16());
            if (!tag.Equals(t.Item))
            {
                throw new Exception("ReadEncapsulatedPixelData: expecting encapsulated data.");
            }
            int fragment = 0;
            // while we have items
            while (tag.Equals(t.Item))
            {
                // first item is the Basic Offset Table
                if (fragment++ == 0)
                {
                    length = reader.ReadUInt32();
                    // if we have a zero length, we create at least one offset
                    uint count = Math.Max(length / 4, 1);
                    offsets = new uint[count];
                    // no offsets implies one frame
                    if (length == 0)
                    {
                        offsets[0] = 0;
                    }
                    else
                    {
                        // read in as many actual offsets as we have
                        for (int n = 0; n < count; n++)
                        {
                            offsets[n] = reader.ReadUInt32();
                        }
                    }
                }
                else
                {
                    // anything beyond the first Item is a Frame
                    this.length = reader.ReadUInt32();
                    position = stream.Position;
                    Frames.Add(reader.ReadBytes((int)this.length));
                }
                tag = new Tag(reader.ReadInt16(), reader.ReadInt16());
            }
            // Basic Offset Table counts as a fragment
            if ((offsets != null) && (fragment - 1 != offsets.Length))
            {
                throw new Exception("ReadEncapsulatedPixelData: only supports unfragmented frames");
            }

            if (!tag.Equals(t.SequenceDelimitationItem))
            {
                throw new Exception("ReadEncapsulatedPixelData: missing Sequence Delimiter Item");
            }
            length = reader.ReadUInt32();
            // length is expected to be zero, but we will not test
        }

        /// <summary>
        /// Writes the Encapsulated PixelData
        /// </summary>
       /// <param name="writer"></param>
        /// <remarks>PS 3.5-2009 A.4 TRANSFER SYNTAXES FOR ENCAPSULATION OF ENCODED PIXEL DATA</remarks>
        private void WriteEncapsulatedPixelData(EndianBinaryWriter writer)
        {
            WriteBasicOffsetTable(writer);

            foreach (byte[] frame in Frames)
            {
                WriteEncapsulatedItem(writer, frame);
            }

            // write the sequence delimeter
            writer.Write((ushort)0xFFFE);
            writer.Write((ushort)0xE0DD);
            writer.Write((int)0x00000000);
        }

        /// <summary>
        /// Writes the Basic Offset Table for Encapsulated PixelData
        /// </summary>
        /// <param name="writer"></param>
        private void WriteBasicOffsetTable(EndianBinaryWriter writer)
        {
            if (Frames.Count != 1)
            {
                // write out the Item tag and length
                writer.Write((ushort)0xFFFE);
                writer.Write((ushort)0xE000);
                // each offset is an int
                writer.Write(sizeof(int) * Frames.Count);

                // write out each offset
                int offset = 0;
                foreach (byte[] frame in Frames)
                {
                    writer.Write(offset);
                    offset += sizeof(ushort) + sizeof(ushort) + sizeof(int) + frame.Length;
                }
            }
            else
            {
                // write an empty basic offset table
                WriteEncapsulatedItem(writer, null);
            }
        }

        /// <summary>
        /// Writes an Item for Encapsulated PixelData
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="value"></param>
        private void WriteEncapsulatedItem(EndianBinaryWriter writer, byte[] value)
        {
            // write Item tag
            writer.Write((ushort)0xFFFE);
            writer.Write((ushort)0xE000);
            if (value != null && value.Length != 0)
            {
                // get the length to write, make sure it is even
                int length = value.Length;
                if (value.Length % 2 != 0)
                {
                    length++;
                }
                writer.Write(length);
                writer.Write((byte[])value);
                // if the value was odd, write a padding byte
                if (value.Length % 2 != 0)
                {
                    writer.Write('\0');
                }
            }
            else
            {
                writer.Write((int)0x00000000);
            }
        }
    }
}
