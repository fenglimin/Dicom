using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class PresentationDataPdu : ProtocolDataUnit, IEnumerable<PresentationDataValue>
    {
        List<PresentationDataValue> pdvs;
        string syntax;

        public PresentationDataPdu(string syntax) :
            base(ProtocolDataUnit.Type.P_DATA_TF)
        {
            this.syntax = syntax;
            this.reserved1 = 0;
            this.length = -1;
            this.pdvs = new List<PresentationDataValue>();
        }

        public List<PresentationDataValue> Values
        {
            get
            {
                return pdvs;
            }
        }

        IEnumerator<PresentationDataValue> IEnumerable<PresentationDataValue>.GetEnumerator()
        {
            foreach (PresentationDataValue pdv in pdvs)
            {
                yield return pdv;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PresentationDataValue>)this).GetEnumerator();
        }

        public override long Size
        {
            get
            {
                long size = 0;
                foreach (PresentationDataValue pdv in pdvs)
                {
                    size += pdv.Size;
                }
                length = (short)size;
                return size;
            }
        }

        public override string Name
        {
            get
            {
                string name = base.Name;
                foreach (PresentationDataValue pdv in pdvs)
                {
                    name += ":";
                    DataSet temp = pdv.Dicom;   // only make the pdv parse to DataSet once
                    if (temp != null && temp.Contains(t.CommandField))
                    {
                        name += ((CommandType)temp[t.CommandField].Value).ToString();
                    }
                }
                return name;
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
                while (stream.Position <= start + length)
                {
                    // it is important to get the correct syntax
                    PresentationDataValue pdv = new PresentationDataValue(syntax);
                    pdvs.Add(pdv);
                    pdv.Read(stream);
                }
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
                foreach (PresentationDataValue pdv in pdvs)
                {
                    pdv.Write(stream);
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("PresentationDataPdu: type={0} reserved1={1} length={2} syntax={3}\n", type, reserved1, length, Reflection.GetName(typeof(Syntax), syntax)));
            foreach (PresentationDataValue pdv in pdvs)
            {
                text.Append("\t"+pdv.Dump());
            }
            return text.ToString();
        }

    }
}
