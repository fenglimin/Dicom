using System;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Writes primitive types in binary to a stream and supports writing strings in a specific encoding
    /// and endian-ness.
    /// </summary>
    public class EndianBinaryWriter : BinaryWriter
    {
        #region Fields

        /// <summary>
        /// The endian-ness of primitive integer datatypes written to the stream.
        /// </summary>
        private Endian endian = Endian.Little;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EndianBinaryWriter class based on the supplied stream and 
        /// using UTF-8 as the encoding for strings.  Primitive integer datatypes will be written using
        /// the supplied endian orientation.
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="endian">How this instance will interpret binary data on the stream.</param>
        public EndianBinaryWriter(Stream output, Endian endian)
            : base(output)
        {
            Endian = endian;
        }

        /// <summary>
        /// Initializes a new instance of the EndianBinaryWriter class based on the supplied stream and a 
        /// specific character encoding.  Primitive integer datatypes will be written using
        /// the supplied endian orientation. 
        /// </summary>
        /// <param name="output">The output stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="endian">How this instance will interpret binary data on the stream.</param>
        public EndianBinaryWriter(Stream output, Encoding encoding, Endian endian)
            : base(output, encoding)
        {
            Endian = endian;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or Sets the current endian orientation of this instance.
        /// </summary>
        public Endian Endian
        {
            get
            {
                return endian;
            }
            set
            {
                this.endian = value;
            }
        }

        #endregion Properties

        #region Primitive Integer Datatype Overrides

        /// <summary>
        /// Writes a four-byte signed integer to the current stream and advances the stream position 
        /// by four bytes.
        /// </summary>
        /// <param name="value">The four-byte signed integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(int value)
        {
            if(Endian == Endian.Big)
            {
                value = Bits.Swap(value);
            }
            base.Write(value);
        }

        /// <summary>
        /// Writes an eight-byte signed integer to the current stream and advances the
        /// stream position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte signed integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(long value)
        {
            if (Endian == Endian.Big)
            {
                value = Bits.Swap(value);
            }
            base.Write(value);
        }

        /// <summary>
        /// Writes a two-byte signed integer to the current stream and advances the stream position 
        /// by two bytes.
        /// </summary>
        /// <param name="value">The two-byte signed integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(short value)
        {
            if (Endian == Endian.Big)
            {
                value = Bits.Swap(value);
            }
            base.Write(value);
        }

        /// <summary>
        /// Writes a four-byte unsigned integer to the current stream and advances the stream 
        /// position by four bytes.
        /// </summary>
        /// <param name="value">The four-byte unsigned integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(uint value)
        {
            if (Endian == Endian.Big)
            {
                value = (uint)Bits.Swap((int)value);
            }
            base.Write(value);
        }

        /// <summary>
        /// Writes an eight-byte unsigned integer to the current stream and advances the stream 
        /// position by eight bytes.
        /// </summary>
        /// <param name="value">The eight-byte unsigned integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(ulong value)
        {
            if (Endian == Endian.Big)
            {
                value = (ulong)Bits.Swap((long)value);
            }
            base.Write(value);
        }

        /// <summary>
        /// Writes a two-byte unsigned integer to the current stream and advances the stream 
        /// position by two bytes.
        /// </summary>
        /// <param name="value">The two-byte unsigned integer to write.</param>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        public override void Write(ushort value)
        {
            if (Endian == Endian.Big)
            {
                value = (ushort)Bits.Swap((short)value);
            }
            base.Write(value);
        }

        #endregion Primitive Integer Datatype Overrides

        #region Extentions

        public virtual void WriteWords(short[] words)
        {
            byte [] bytes = new byte[words.Length*2];
            Buffer.BlockCopy(words, 0, bytes, 0, words.Length*2);
            if (Endian == Endian.Big)
            {
                for (int n = 0; n < bytes.Length - 1; n += 2)
                {
                    byte temp = bytes[n];
                    bytes[n] = bytes[n + 1];
                    bytes[n + 1] = temp;
                }
            }
            base.Write(bytes);
        }

        #endregion Extensions

    }
}
