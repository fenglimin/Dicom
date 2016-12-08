using System;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public abstract class DicomObject
    {
        public abstract long Size
        {
            get;
        }

        public abstract long Read(Stream stream);
        public abstract long Write(Stream stream);
        public abstract string Dump();

        public string ToText()
        {
           return ToText(ToArray());
        }

        public byte[] ToArray()
        {
            MemoryStream stream = new MemoryStream();
            Write(stream);
            return stream.ToArray();
        }

        public static string ToText(byte[] bytes)
        {
            return ToText(bytes, 0, bytes.Length);
        }

        public static string ToText(byte[] bytes, int start, int length)
        {
            if (bytes == null)
            {
                return String.Empty;
            }
            if (length < 0)
            {
                throw new ArgumentException("Out of bounds", "length");
            }
            if (start > bytes.Length)
            {
                throw new ArgumentException("Out of bounds", "start");
            }

            if (bytes.Length < start + length)
            {
                length = bytes.Length - start;
            }

            StringBuilder text = new StringBuilder();
            int total = (int)Math.Ceiling(length / 16.0);
            for (int n = 0; n < total; n++)
            {
                if (text.Length > 0)
                    text.Append("\r\n");

                int offset = start + 16 * n;
                int count = (n == total - 1) ? length - (n * 16) : 16;
                StringBuilder left = new StringBuilder();
                StringBuilder right = new StringBuilder();
                for (int m = 0; m < count; m++)
                {
                    if (m == 8)
                    {
                        left.Append("- ");
                    }
                    left.Append(String.Format("{0:X2} ", bytes[offset + m]));
                    char c = Convert.ToChar(bytes[offset + m]);
                    right.Append((Char.IsLetterOrDigit(c) || c == ' ' || Char.IsPunctuation(c)) ? (char)bytes[offset + m] : '.');
                }
                string address = String.Format("{0:x8} : ", start + n * 16);
                text.Append(address + left.ToString() + new String(' ', 51 - left.Length) + right.ToString());
            }
            return text.ToString();
        }

        protected static string pad(string text, int size)
        {
            if (text.Length > size)
            {
                return text.Substring(0, size);
            }
            return text + new String(' ', size - text.Length);
        }
    }

    public class ProtocolDataUnit : DicomObject
    {
        protected ProtocolDataUnit.Type type;
        protected byte reserved1;
        protected int length;

        public enum Type : byte
        {
            Unknown = 0x00,
            A_ASSOCIATE_RQ = 0x01,
            A_ASSOCIATE_AC = 0x02,
            A_ASSOCIATE_RJ = 0x03,
            P_DATA_TF = 0x04,
            A_RELEASE_RQ = 0x05,
            A_RELEASE_RP = 0x06,
            A_ABORT = 0x07
        }

        public ProtocolDataUnit() :
            this(ProtocolDataUnit.Type.Unknown)
        {
        }

        public ProtocolDataUnit(ProtocolDataUnit.Type type)
        {
            this.type = type;
        }

        public ProtocolDataUnit.Type PduType
        {
            get
            {
                return type;
            }
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        public override long Size
        {
            get 
            { 
                throw new NotImplementedException(); 
            }
        }

        public virtual string Name
        {
            get
            {
                return type.ToString();
            }
        }

        public override string Dump()
        {
            throw new NotImplementedException();
        }

        public override long Read(Stream stream)
        {
            throw new NotImplementedException();
        }

        public override long Write(Stream stream)
        {
            throw new NotImplementedException();
        }
    }

}
