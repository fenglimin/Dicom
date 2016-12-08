using System;
using System.IO;

namespace EK.Capture.Dicom.DicomToolKit
{

    public enum AbortSource
    {
        ServiceUser = 0,
        Reserved1 = 1,
        ServiceProvider = 2
    }

    public enum AbortReason
    {
        NoReasonGiven = 0,
        UnrecognizedPdu = 1,
        UnexpectedPdu = 2,
        Reserved3 = 3,
        UnrecognizedPduParameter = 4,
        UnexpectedPduParameter = 5,
        InvalidPduParameterValue = 6
    }

    public class AssociateAbortPdu : ProtocolDataUnit
    {
        internal byte reserved2;
        internal byte reserved3;
        internal AbortSource source;
        internal AbortReason reason;

        public AssociateAbortPdu() :
            base(ProtocolDataUnit.Type.A_ABORT)
        {
        }

        public override long Size
        {
            get
            {
                // reserved2, reserved3, source and reason
                length = sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte);
                return (long)length;
            }
        }

        public AbortReason Reason
        {
            get
            {
                return reason;
            }
            set
            {
                reason = value;
            }
        }

        public AbortSource Source
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
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
                reserved2 = reader.ReadByte();
                reserved3 = reader.ReadByte();
                source = (AbortSource)reader.ReadByte();
                reason = (AbortReason)reader.ReadByte();
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
                reserved3 = 0;
                writer.Write(reserved3);
                writer.Write((byte)source);
                writer.Write((byte)reason);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("AssociateAbortPdu\ntype={0} reserved1={1} length={2} reserved2={3} reserved3={4} source={5} reason={6}",
                type, reserved1, length, reserved2, reserved3, (AbortSource)source, (AbortReason)reason);
        }
    }
}
