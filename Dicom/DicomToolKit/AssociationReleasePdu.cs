using System;
using System.IO;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class AssociationReleasePdu : ProtocolDataUnit
    {
        internal int reserved2;

        public AssociationReleasePdu()
            : this(ProtocolDataUnit.Type.Unknown)
        {
        }

        public AssociationReleasePdu(ProtocolDataUnit.Type type) :
            base(type)
        {
        }

        public override long Size
        {
            get
            {
                // reserved2
                length = sizeof(int);
                return (long)length;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                type = (ProtocolDataUnit.Type)reader.ReadByte();
                reserved1 = reader.ReadByte();
                length = reader.ReadInt32();
                reserved2 = reader.ReadInt16();
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override long Write(Stream stream)
        {
            long bytes = 0;
            if (stream.CanWrite)
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                long start = stream.Position;
                writer.Write((byte)type);
                reserved1 = 0;
                writer.Write(reserved1);
                length = (int)Size;
                writer.Write(length);
                reserved2 = 0;
                writer.Write(reserved2);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("AssociationReleasePdu: type={0} reserved1={1 length={2} reserved2={3}",
                type, reserved1, length, reserved2);
        }

    }
}
