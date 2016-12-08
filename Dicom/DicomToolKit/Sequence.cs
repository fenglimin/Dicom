using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class Sequence : Element, IEnumerable<Element>
    {
        //TODO make items the Value
        /// <summary></summary>
        private List<Elements> items = new List<Elements>();

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public Sequence() :
            base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        public Sequence(Element parent) :
            base(parent)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        public Sequence(string key) :
            base(key, "SQ")
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        public Sequence(short group, short element) :
            base(group, element, "SQ")
        {
        }

        #endregion Constructors

        #region Items

        /// <summary>
        /// The collection of Items for tags that are sequences.
        /// </summary>
        public List<Elements> Items
        {
            get
            {
                return items;
            }
        }

        /// <summary>
        /// Adds a new empty Item to the sequence.
        /// </summary>
        /// <returns></returns>
        public Elements NewItem()
        {
            if (this.VR != "SQ")
            {
                throw new Exception(String.Format("You can only add Items to a sequence.  This tag has a VR of {0}", this.VR));
            }
            Elements item = new Elements(this);
            items.Add(item);
            return item;
        }

        /// <summary>
        /// Adds an Item to the sequence.
        /// </summary>
        /// <returns></returns>
        public Elements AddItem(Elements item)
        {
            if (this.VR != "SQ")
            {
                throw new Exception(String.Format("You can only add Items to a sequence.  This tag has a VR of {0}", this.VR));
            }
            item.Parent = this;
            items.Add(item);
            return item;
        }

        /// <summary>
        /// Removes an Item at the specified index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Elements RemoveItemAt(int index)
        {
            Elements item = items[index];
            items.RemoveAt(index);
            return item;
        }

        /// <summary>
        /// Inserts an Item at the specicified index.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public Elements InsertItem(Elements item, int index)
        {
            items.Insert(index, item);
            return item;
        }

        public void ReplaceItem(Elements item, int index)
        {
            Elements removed = items[index];
            items.RemoveAt(index);
            InsertItem(item, index);
        }

        #endregion Items

        #region IEnumerable and etc.

        /// <summary>
        /// An enumerator for the collection of Elements in ascending tag order.
        /// </summary>
        public IEnumerable<Element> InOrder
        {
            get
            {
                return ScanInOrder(this);
            }
        }

        /// <summary>
        /// An enumerator for all Elements for this Element.
        /// </summary>
        /// The enumerator iteerates all Items in a sequence.
        IEnumerator<Element> IEnumerable<Element>.GetEnumerator()
        {
            foreach (Elements item in items)
            {
                foreach (Element element in item)
                {
                    if (element is Sequence)
                    {
                        foreach (Elements child in ((Sequence)element).items)
                        {
                            foreach (Element temp in child)
                            {
                                yield return temp;
                            }
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
        /// An enumerator for the collection of Elements in no particular order.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            // TODO: resolve all this scan in order stuff for Element, Elements and DataSet
            return ((IEnumerable<Element>)this).GetEnumerator();
        }

        private IEnumerable<Element> ScanInOrder(Sequence root)
        {
            foreach (Elements item in items)
            {
                foreach (Element element in item)
                {
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

        #endregion IEnumerable and etc.

        #region Overrides

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
            value = null;
            if (length != 0)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Syntax.GetEndian(syntax));

                //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Reading sequence of length={1}", stream.Position, (length == UInt32.MaxValue) ? "undefined" : length.ToString()));
                ReadSequence(reader, encoding, reader.BaseStream.Position);
                //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Finished reading sequence", stream.Position));
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
            unchecked
            {
                foreach (Elements elements in items)
                {
                    writer.Write((short)0xFFFE);
                    writer.Write((short)0xE000);

                    // each nested element list can have its own SpecificCharacterSet, 
                    // and it is valid for all nested elements, unless overridden again further down
                    SpecificCharacterSet scoped = encoding;
                    if (elements.Contains(t.SpecificCharacterSet))
                    {
                        scoped = new SpecificCharacterSet(elements[t.SpecificCharacterSet].Value);
                    }

                    uint size = 0;
                    // and account for the size of each child element
                    foreach (Element child in elements)
                    {
                        size += child.GetSize(syntax, scoped);
                    }
                    writer.Write(size);

                    foreach (Element child in elements)
                    {
                        child.Write(writer.BaseStream, syntax, scoped, options);
                    }
                }
            }
        }

        /// <summary>
        /// The size in bytes of the Element if it were to be written.
        /// </summary>
        /// <param name="syntax">The TransferSyntax that will be used to write to the stream.</param>
        /// <param name="encoding">The encoding with which this element will be written.</param>
        /// <returns>The size in bytes of the Element.</returns>
        public override uint GetSize(string syntax, SpecificCharacterSet encoding)
        {
            uint size = GetFront(syntax, encoding);
            foreach (Elements elements in items)
            {
                // account for the size of each item tag and item value length
                size += sizeof(short) + sizeof(short) + sizeof(int);
                // each nested element list can have its own SpecificCharacterSet, 
                // it is valid for all nested elements, unless overridden again further down
                SpecificCharacterSet scoped = encoding;
                if (elements.Contains(t.SpecificCharacterSet))
                {
                    scoped = new SpecificCharacterSet(elements[t.SpecificCharacterSet].Value);
                }
                // and then account for the size of each child element
                foreach (Element element in elements)
                {
                    size += element.GetSize(syntax, scoped);
                }
            }
            return size;
        }

        public override object Value
        {
            get
            {
                // A Sequence does not have a value
                return null;
            }
            set
            {
                throw new Exception("A Sequence cannot have a Value.");
            }
        }

        /// <summary>
        /// Produce a string represenation of the collection.
        /// </summary>
        /// <returns>A string represenation of the collection.</returns>
        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("{0}:{1}:{2}\n", this.Tag.ToString(), this.VR, this.Description));
            foreach (Elements elements in items)
            {
                text.Append(elements.Dump());
            }
            return text.ToString();
        }

        #endregion Overrides

        private void ReadSequence(EndianBinaryReader reader, SpecificCharacterSet encoding, long start)
        {
            Tag lookahead = null;
            // if we have somethign to read and we are not at the end of the stream
            if (length != 0 && position != stream.Length)
            {
                // look ahead and see if we can find an item delimiter tag
                lookahead = DataSet.PeekTag(reader, this.syntax);
                //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Peek {1} in ReadSequence", stream.Position, lookahead.Description));
 
                // as long as we have item tags
                // TODO if we do not find an item tag that is a problem
                while (lookahead.Equals(t.Item))
                {
                    // position past the tag group and element
                    stream.Position += sizeof(ushort) + sizeof(ushort);
                    // find out the length of the item
                    uint count = reader.ReadUInt32();

                    //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Reading item of length={1}", stream.Position, (count == UInt32.MaxValue) ? "undefined" : count.ToString()));

                    Elements elements = new Elements();
                    items.Add(elements);

                    long place = stream.Position;
                    // call scan recursively, passing in the length limit which is the size of the
                    // entire item along with the sequence key name
                    long temp = (count != UInt32.MaxValue) ? (place + count) : -1;
                    place = DataSet.Scan(this, stream, elements, place, temp, UInt16.MaxValue, syntax, encoding);

                    //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Finished reading item", stream.Position));

                    if (stream.Position < stream.Length)
                    {
                        lookahead = DataSet.PeekTag(reader, this.syntax);
                        //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: Peek {1} in ReadSequence", stream.Position, lookahead.Name));
                    }

                    // the next tag is either another item in the sequence, a sequence delimiter, or the next tag 
                    // after the sequence. we only get the next tag if we are not at the end of the file and 
                    // we have an undefined sequence length
                    if (length == UInt32.MaxValue)
                    {
                        if(lookahead.Equals(t.SequenceDelimitationItem))
                        {
                            //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: undefined length, found {1}:{2}", stream.Position, lookahead.Description, lookahead.Name));
                            break;
                        }
                    }
                    else
                    {
                        if (stream.Position >= start + length)
                        {
                            break;
                        }
                        if (!lookahead.Equals(t.Item))
                        {
                        }
                    }
                }
                if (length == UInt32.MaxValue)
                {
                    if (lookahead.Equals(t.SequenceDelimitationItem))
                    {
                        //System.Diagnostics.Debug.WriteLine(String.Format("0x{0:X8}:{0}: undefined length, found {1}:{2}", stream.Position, lookahead.Description, lookahead.Name));
                        // position stream beyond delimitation tag
                        stream.Position += sizeof(ushort) * 2;
                        // no need to check zero
                        int zero = reader.ReadInt32();
                    }
                }
            }
        }

    }
}
