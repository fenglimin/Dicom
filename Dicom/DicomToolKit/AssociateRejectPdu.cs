using System;
using System.IO;

namespace EK.Capture.Dicom.DicomToolKit
{

    internal enum RejectResult
    {
        Undefined = 0,
        RejectedPermanent = 1,
        RejectedTransient = 2,
    }

    internal enum RejectSource
    {
        Undefined = 0,
        User = 1,
        ProviderAcse = 2,
        ProviderPresentation = 3,
    }

    internal enum RejectReason
    {
        Undefined = 0,
        NoReasonGiven = 1,
        ApplicationContextNameNotSupported = 2,
        CallingAETitleNotRecognized = 3,
        Reserved4 = 4,
        Reserved5 = 5,
        Reserved6 = 6,
        CalledAETitleNotRecognized = 7,
        Reserved8 = 8,
        Reserved9 = 9,
        Reserved10 = 10,
    }

    public class AssociateRejectPdu : ProtocolDataUnit
    {
        internal byte reserved2;
        internal byte result;
        internal byte source;
        internal byte reason;

        public AssociateRejectPdu() :
            base(ProtocolDataUnit.Type.A_ASSOCIATE_RJ)
        {
        }

        public override long Size
        {
            get
            {
                // reserved2, result, source and reason
                length = sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte);
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
                reserved2 = reader.ReadByte();
                result = reader.ReadByte();
                source = reader.ReadByte();
                reason = reader.ReadByte();
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
                writer.Write(result);
                writer.Write(source);
                writer.Write(reason);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("AssociateRejectPdu: type={0} reserved1={1} length={2} reserved2={3} result={4} source={5} reason={6}",
                type, reserved1, length, reserved2, result, source, reason);
        }
    }
}
