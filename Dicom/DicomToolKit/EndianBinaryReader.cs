using System;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Bit swaps big and little endian datatypes.
    /// </summary>
    public class Bits
    {
        /// <summary>
        /// Swaps the endian-ness of a 2-byte integer.
        /// </summary>
        /// <param name="value">The 2-byte integer to swap.</param>
        /// <returns>A 2-byte integer with the opposite endianess</returns>
        public static short Swap(short value)
        {
            int b1 = value & 0xff;
            int b2 = (value >> 8) & 0xff;

            return (short)(b1 << 8 | b2 << 0);
        }

        /// <summary>
        /// Swaps the endian-ness of a 4-byte integer.
        /// </summary>
        /// <param name="value">The 4-byte integer to swap.</param>
        /// <returns>A 4-byte integer with the opposite endianess</returns>
        public static int Swap(int value)
        {
            int b1 = (value >> 0) & 0xff;
            int b2 = (value >> 8) & 0xff;
            int b3 = (value >> 16) & 0xff;
            int b4 = (value >> 24) & 0xff;

            return b1 << 24 | b2 << 16 | b3 << 8 | b4 << 0;
        }

        /// <summary>
        /// Swaps the endian-ness of an 8-byte integer.
        /// </summary>
        /// <param name="value">The 8-byte integer to swap.</param>
        /// <returns>An 8-byte integer with the opposite endianess</returns>
        public static long Swap(long value)
        {
            long b1 = (value >> 0) & 0xff;
            long b2 = (value >> 8) & 0xff;
            long b3 = (value >> 16) & 0xff;
            long b4 = (value >> 24) & 0xff;
            long b5 = (value >> 32) & 0xff;
            long b6 = (value >> 40) & 0xff;
            long b7 = (value >> 48) & 0xff;
            long b8 = (value >> 56) & 0xff;

            return b1 << 56 | b2 << 48 | b3 << 40 | b4 << 32 |
                   b5 << 24 | b6 << 16 | b7 << 8 | b8 << 0;
        }

    }

    /// <summary>
    /// Reads primitive data types as binary values from a stream in a specific 
    /// encoding and endian-ness.
    /// </summary>
    public class EndianBinaryReader : BinaryReader
    {
        #region Fields

        /// <summary>
        /// The endian-ness of primitive integer datatypes read from the stream.
        /// </summary>
        private Endian endian = Endian.Little;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the EndianBinaryReader class based on the supplied stream 
        /// and using <see cref="UTF8Encoding"/>.  Primitive integer datatypes will be read using the 
        /// supplied endian orientation.
        /// </summary>
        /// <param name="input">A stream.</param>
        /// <param name="endian">How this instance will interpret binary data from the stream.</param>
        public EndianBinaryReader(Stream input, Endian endian)
            : base(input)
        {
            Endian = endian;
        }

        /// <summary>
        /// Initializes a new instance of the EndianBinaryReader class based on the supplied stream 
        /// and a specific character encoding.  Primitive integer datatypes will be read using the 
        /// supplied endian orientation.
        /// </summary>
        /// <param name="input">A stream.</param>
        /// <param name="encoding">The character encoding.</param>
        /// <param name="endian">How this instance will interpret binary data from the stream.</param>
        public EndianBinaryReader(Stream input, Encoding encoding, Endian endian)
            : base(input, encoding)
        {
            Endian = endian;
        }

        #endregion Constrcutors

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
        /// Reads a single-byte signed integer from the current stream and advances the current position 
        /// of the stream by one byte.
        /// </summary>
        /// <returns>A 1-byte signed integer read from the current stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public virtual byte PeekByte()
        {
            return (byte)(base.PeekChar());
        }
                
        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position 
        /// of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte signed integer read from the current stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override short ReadInt16()
        {
            short value = base.ReadInt16();
            return (Endian == Endian.Little) ? value : Bits.Swap(value);
        }

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current position 
        /// of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte signed integer read from the current stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override int ReadInt32()
        {
            int value = base.ReadInt32();
            return (Endian == Endian.Little) ? value : Bits.Swap(value);
        }

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current position 
        /// of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte signed integer read from the current stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override long ReadInt64()
        {
            long value = base.ReadInt64();
            return (Endian == Endian.Little) ? value : Bits.Swap(value);
        }

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream and advances the position 
        /// of the stream by two bytes.
        /// </summary>
        /// <returns>A 2-byte unsigned integer read from this stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override ushort ReadUInt16()
        {
            ushort value = base.ReadUInt16();
            return (Endian == Endian.Little) ? value : (ushort)Bits.Swap((short)value);
        }

        /// <summary>
        /// Reads a 4-byte unsigned integer from the current stream and advances the position 
        /// of the stream by four bytes.
        /// </summary>
        /// <returns>A 4-byte unsigned integer read from this stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override uint ReadUInt32()
        {
            uint value = base.ReadUInt32();
            return (Endian == Endian.Little) ? value : (uint)Bits.Swap((int)value);
        }

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position 
        /// of the stream by eight bytes.
        /// </summary>
        /// <returns>An 8-byte unsigned integer read from this stream.</returns>
        /// <exception cref="System.ObjectDisposedException">The stream is closed.</exception>
        /// <exception cref="System.IO.IOException">An I/O error occurs.</exception>
        /// <exception cref="System.IO.EndOfStreamException">The end of the stream is reached.</exception>
        public override ulong ReadUInt64()
        {
            ulong value = base.ReadUInt64();
            return (Endian == Endian.Little) ? value : (ulong)Bits.Swap((long)value);
        }

        #endregion Primitive Integer Datatype Overrides

        #region Extentions

        public virtual short[] ReadWords(int count)
        {
            byte [] bytes = base.ReadBytes(count*2);
            short[] words = new short[count];
            if (Endian == Endian.Big)
            {
                for (int n = 0; n < count*2-1; n += 2)
                {
                    byte temp = bytes[n];
                    bytes[n] = bytes[n + 1];
                    bytes[n + 1] = temp;
                }
            }
            Buffer.BlockCopy(bytes, 0, words, 0, bytes.Length);
            return words;
        }

        #endregion Extensions
    }
}
