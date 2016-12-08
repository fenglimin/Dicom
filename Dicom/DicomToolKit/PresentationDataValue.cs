using System;                           // ArrayList
using System.IO;                        // Stream

namespace EK.Capture.Dicom.DicomToolKit
{
    public enum DataSetType : ushort
    {
        DataSetPresent = 0x0102,
        DataSetNotPresent = 0x0101
    }

    public enum MessageType : byte
    {
        DataSet = 0,
        Command = 1,
        LastDataSet = 2,
        LastCommand = 3
    }

    public class MessageControl
    {

        public static bool IsCommand(MessageType control)
        {
            return (control == MessageType.Command || control == MessageType.LastCommand);
        }

        public static bool IsDataSet(MessageType control)
        {
            return (control == MessageType.DataSet || control == MessageType.LastDataSet);
        }

        public static bool IsNotLast(MessageType control)
        {
            return (control == MessageType.DataSet || control == MessageType.Command);
        }

        public static bool IsLast(MessageType control)
        {
            return (control == MessageType.LastDataSet || control == MessageType.LastCommand);
        }
    }

    public class PresentationDataValue : DicomObject
    {
        internal int length;
        internal byte context;
        internal MessageType control;
        internal byte [] data;
        internal int index;
        internal int count;
        internal string syntax;

        public PresentationDataValue(string syntax)
        {
            this.length = -1;
            this.context = 0;
            this.control = 0;
            this.index = 0;
            this.count = 0;
            this.data = null;
            this.syntax = syntax;
        }

        public PresentationDataValue(byte context, string syntax, MessageType control)
            : this(syntax)
        {
            //Logging.Log("PresentationDataValue={0}", context);
            this.context = context;
            this.control = control;
        }

        public PresentationDataValue(byte context, string syntax, MessageType control, byte [] data, int index, int count)
            : this(context, syntax, control)
        {
            this.index = index;
            this.count = count;
            this.data = data;
        }

        public override long Size
        {
            get
            {
                // item length, context id and message control header
                length = sizeof(int) + sizeof(byte) + sizeof(byte) + this.count;
                return length;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                length = reader.ReadInt32();
                context = reader.ReadByte();
                control = (MessageType)reader.ReadByte();
                // length - 2 is because we subtract out the size of context and control
                data = reader.ReadBytes(length - 2);
                index = 0;
                count = data.Length;
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
                length = (int)Size - sizeof(int);
                writer.Write(length);
                writer.Write(context);
                writer.Write((byte)control);
                writer.Write(data, index, count);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public MessageType Control
        {
            get
            {
                return control;
            }
        }
        public Byte Context
        {
            get
            {
                return context;
            }
        }

        /// <summary>
        /// Attempts to parse and return the DataSet associated with this pdv. Some pdvs do not
        /// contain a full DataSet that can be parsed, for instance some pdvs could contain
        /// a fragment of the PixelData.
        /// </summary>
        public DataSet Dicom
        {
            get
            {
                DataSet dicom = null;
                MemoryStream memory = new MemoryStream(data);
                try
                {
                    // a pdv may contain only a fragment of a complete DICOM tag as in the case of a pdv 
                    // containing only a part of the PixelData.  Only Commands and DataSets that fit
                    // into one pdv will always be fully parseable
                    if (MessageControl.IsLast(control))
                    {
                        dicom = new DataSet();
                        // it is important to get the correct syntax
                        DataSet.Scan(null, memory, dicom.elements, 0, data.Length, 0xffff,
                            (MessageControl.IsCommand(control)) ? Syntax.ImplicitVrLittleEndian : syntax, dicom.encoding);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Log(LogLevel.Error, ex.Message);
                    dicom = null;
                }
                return dicom;
            }
            set
            {
                MemoryStream memory = new MemoryStream();
                // it is important to get the correct syntax
                value.TransferSyntaxUID = (MessageControl.IsCommand(control)) ? Syntax.ImplicitVrLittleEndian : syntax;
                value.Write(memory);
                this.data = memory.ToArray();
                this.index = 0;
                this.count = data.Length;
            }
        }

        public override string Dump()
        {
            return String.Format("PresentationDataValue: length={0} context={1} control={2} index={3} count={4} syntax={5}\n", length, context, control, index, count, Reflection.GetName(typeof(Syntax), syntax));
        }

    }
}
