using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public enum Role
    {
        Scu,
        Scp,
        Both
    }

    public enum CommandType : ushort
    {
        C_STORE_RQ = 0x0001,
        C_STORE_RSP = 0x8001,
        C_GET_RQ = 0x0010,
        C_GET_RSP = 0x8010,
        C_FIND_RQ = 0x0020,
        C_FIND_RSP = 0x8020,
        C_MOVE_RQ = 0x0021,
        C_MOVE_RSP = 0x8021,
        C_ECHO_RQ = 0x0030,
        C_ECHO_RSP = 0x8030,
        N_EVENT_REPORT_RQ = 0x0100,
        N_EVENT_REPORT_RSP = 0x8100,
        N_GET_RQ = 0x0110,
        N_GET_RSP = 0x8110,
        N_SET_RQ = 0x0120,
        N_SET_RSP = 0x8120,
        N_ACTION_RQ = 0x0130,
        N_ACTION_RSP = 0x8130,
        N_CREATE_RQ = 0x0140,
        N_CREATE_RSP = 0x8140,
        N_DELETE_RQ = 0x0150,
        N_DELETE_RSP = 0x8150,
        C_CANCEL_RQ = 0x0FFF
    }

    public enum ItemType : byte
    {
        ApplicationItem = 0x10,
        PresentationContextItemRQ = 0x20,
        PresentationContextItemAC = 0x21,
        AbstractSyntaxSubItem = 0x30,
        TransferSyntaxSubItem = 0x40,
        UserInformationItem = 0x50,
        MaximumLengthSubItem = 0x51,
        ImplementationClassUidSubItem = 0x52,
        AsynchronousOperationsWindowSubItem = 0x53,
        ScuScpRoleSubItem = 0x54,
        ImplementationVersionNameSubItem = 0x55
    }

    public class Item : DicomObject
    {
        protected byte type;
        protected byte reserved1;
        protected short length;

        public Item(byte type)
        {
            this.type = type;
            this.reserved1 = 0;
            this.length = -1;
        }

        public byte Type
        {
            get
            {
                return type;
            }
        }

        public override long Size
        {
            get
            {
                // type, reserved1 and length
                return sizeof(byte) + sizeof(byte) + sizeof(short);
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                type = reader.ReadByte();
                reserved1 = reader.ReadByte();
                length = reader.ReadInt16();
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
                writer.Write(type);
                writer.Write((byte)0);
                length = (short)(Size - (sizeof(byte) + sizeof(byte) + sizeof(short)));
                writer.Write(length);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("{0}\n\ttype={1} reserved1={2} length={3}", ToText(), type, reserved1, length);
        }
    }

    public class ApplicationContextItem : Item
    {
        // char <= 64
        string context;

        public ApplicationContextItem()
            :this(String.Empty)
        {
        }

        public ApplicationContextItem(string uid)
            : base(0x010)
        {
            this.context = (uid.Length>64)?uid.Substring(0,64):uid;
            this.length = (short)context.Length;
        }

        public override long Size
        {
            get
            {
                return base.Size + context.Length;
            }
        }

        public string Context
        {
            get
            {
                return context;
            }
            set
            {
                context = (value.Length>64)?value.Substring(0,64):value;
                this.length = (short)context.Length;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if(stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                long count = base.Read(stream);
                byte[] temp = reader.ReadBytes(length);
                context = System.Text.Encoding.ASCII.GetString(temp);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override long Write(Stream stream)
        {
             long bytes = 0;
            if(stream.CanWrite)
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                long start = stream.Position;
                length = (short)Size;
                long count = base.Write(stream);
                writer.Write(context.ToCharArray());
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("ApplicationContextItem\n{0}\n\tcontext={1}", base.Dump(), context);
        }

    }

    public enum PCIReason : byte
    {
        Undefined = 0xff,
        Accepted = 0x00,
        UserRejected = 0x01,
        NoReason = 0x02,
        AbstractSyntaxNotSupported = 0x03,
        TransferSyntaxesNotSupported = 0x04
    }

    public class PresentationContextItem : Item
    {
        byte id;
        byte reserved2;
        PCIReason reason;
        byte reserved3;
        public List<Item> fields;

        public PresentationContextItem(byte type, byte id)
            : base(type)
        {
            this.id = id;
            this.reserved2 = 0;
            this.reason = PCIReason.Undefined;
            this.reserved3 = 0;
            fields = new List<Item>();
        }

        public PresentationContextItem(byte type)
            : base(type)
        {
            this.id = 0;
            this.reserved2 = 0;
            this.reason = 0;
            this.reserved3 = 0;
            fields = new List<Item>();
        }

        public byte PresentationContextId
        {
            get
            {
                return id;
            }
            internal set
            {
                id = value;
            }
        }

        public PCIReason PciReason
        {
            get
            {
                return reason;
            }
            internal set
            {
                reason = value;
            }
        }

        public override long Size
        {
            get
            {
                // id, reserved1, reason and reserved3
                long size = base.Size + sizeof(byte) + sizeof(byte) + sizeof(byte) + sizeof(byte);
                foreach(Item item in fields)
                {
                    size += item.Size;
                }
                length = (short)size;
                return size;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if(stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                base.Read(stream);
                id = reader.ReadByte();
                reserved2 = reader.ReadByte();
                reason = (PCIReason)reader.ReadByte();
                reserved3 = reader.ReadByte();
                byte next = reader.PeekByte();
                while(next==0x30 || next ==0x40)
                {
                    Item item = new SyntaxItem(next);
                    fields.Add(item);
                    item.Read(stream);
                    if (stream.Position >= stream.Length)
                        break;
                    next = reader.PeekByte();
                }
                if(fields.Count<1)
                {
                    throw new Exception("ill-formed presentation_context");
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override long Write(Stream stream)
        {
            long bytes = 0;
            if(stream.CanWrite)
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                long start = stream.Position;
                length = (short)Size;
                base.Write(stream);
                writer.Write(id);
                reserved2 = 0;
                writer.Write(reserved2);
                writer.Write((byte)reason);
                reserved3 = 0;
                writer.Write(reserved3);
                foreach (Item item in fields)
                {
                    item.Write(stream);
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("PresentationContextItem\n{0}\n\tid={1} reserved2={2} reason={3} reserved3={4}", base.Dump(), id, reserved2, reason, reserved3));
            foreach (Item item in fields)
            {
                text.Append(item.Dump());
            }
            return text.ToString();

        }

    }

    public class SyntaxItem : Item
    {
        // char <= 64
        string syntax;

        public SyntaxItem(byte type)
            : this(type, String.Empty)
        {
        }

        public SyntaxItem(byte type, string uid)
            : base(type)
        {
            this.syntax = (uid.Length > 64) ? uid.Substring(0, 64) : uid;
            this.length = (short)syntax.Length;
        }

        public override long Size
        {
            get
            {
                return base.Size + syntax.Length;
            }
        }

        public string Syntax
        {
            get
            {
                return syntax;
            }
            internal set
            {
                syntax = (value.Length > 64) ? value.Substring(0, 64) : value;
                this.length = (short)syntax.Length;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if(stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                long count = base.Read(stream);
                byte[] temp = reader.ReadBytes(length);
                syntax = System.Text.Encoding.ASCII.GetString(temp);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override long Write(Stream stream)
        {
             long bytes = 0;
            if(stream.CanWrite)
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                long start = stream.Position;
                length = (short)Size;
                long count = base.Write(stream);
                writer.Write(syntax.ToCharArray());
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("SyntaxItem\n{0}\n\tsyntax={1}", base.Dump(), syntax);
        }
    }

    public class UserInfoItem : Item
    {
        public List<Item> fields;

        public UserInfoItem()
            : base(0x50)
        {
            fields = new List<Item>();
        }

        public override long Size
        {
            get
            {
                long size = base.Size;
                foreach (Item item in fields)
                {
                    size += item.Size;
                }
                length = (short)size;
                return size;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if(stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                base.Read(stream);
                byte next = reader.PeekByte();
                Item item = null;
                while(next>0x50)
                {
                    switch(next)
                    {
                    case (byte)ItemType.MaximumLengthSubItem: // 0x51:
                        item = new MaximumLengthItem();
                        break;
                    case (byte)ItemType.ImplementationClassUidSubItem: // 0x52:
                    case (byte)ItemType.ImplementationVersionNameSubItem: // 0x55:
                        item = new SyntaxItem(next);
                        break;
                    case (byte)ItemType.AsynchronousOperationsWindowSubItem: // 0x53:
                        item = new AsynchronousOperationsWindowItem();
                        break;
                    case (byte)ItemType.ScuScpRoleSubItem: // 0x54:
                        item = new ScpScuRoleSelectionItem();
                        break;
                    default:
                        Debug.Assert(false, String.Format("Unknown User Info Item {0}.", next));
                        break;
                    }
                    fields.Add(item);
                    item.Read(stream);
                    if (stream.Position >= stream.Length)
                        break;
                    next = reader.PeekByte();
                }
                if(fields.Count<1)
                {
                    throw new Exception("Ill-formed User Info Item.");
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override long Write(Stream stream)
        {
            long bytes = 0;
            if(stream.CanRead)
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Big);
                long start = stream.Position;
                length = (short)Size;
                base.Write(stream);
                foreach (Item item in fields)
                {
                    item.Write(stream);
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("UserInfoItem\n{0}\n", base.Dump()));
            foreach (Item item in fields)
            {
                text.Append(item.Dump());
            }
            return text.ToString();

        }
    }

    public class MaximumLengthItem : Item
    {
        int packet;

        public MaximumLengthItem()
            : this(-1)
        {
        }

        public MaximumLengthItem(int packet)
            : base(0x51)
        {
            this.packet = packet;
            this.length = 4;
        }

        public override long Size
        {
            get
            {
                // packet
                return base.Size + sizeof(int);
            }
        }

        public int PacketSize
        {
            get
            {
                return packet;
            }
            internal set
            {
                packet = value;
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                base.Read(stream);
                packet = reader.ReadInt32();
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
                base.Write(stream);
                writer.Write(packet);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("MaximumLengthItem\n{0}\npacket={0}", base.Dump(), packet);
        }
    }

    public class AsynchronousOperationsWindowItem : Item
    {
        protected short invoked;
        protected short performed;

        public AsynchronousOperationsWindowItem()
            : base(0x53)
        {
            this.invoked = 0x01;
            this.performed = 0x01;
            length = 4;
        }

        public override long Size
        {
            get
            {
                // invoked and performed
                return base.Size + sizeof(short) + sizeof(short);
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                base.Read(stream);
                invoked = reader.ReadInt16();
                performed = reader.ReadInt16();
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
                length = 4;
                base.Write(stream);
                writer.Write(invoked);
                writer.Write(performed);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("AsynchronousOperationsWindowItem\n{0}\ninvoked={1} performed={2}", base.Dump(), invoked, performed);
        }
    }

    public class ScpScuRoleSelectionItem : Item
    {
        protected short uidlength;
        protected string uid;
        protected byte scurole;
        protected byte scprole;

        public ScpScuRoleSelectionItem()
            : this(String.Empty, Role.Scu)
        {
        }

        public ScpScuRoleSelectionItem(string uid, Role role)
            : base((byte)ItemType.ScuScpRoleSubItem/*0x54*/)
        {
            this.uid = uid;
            this.uidlength = (short)uid.Length;
            this.scurole = (role == Role.Scu || role == Role.Both) ? (byte)1 : (byte)0;
            this.scprole = (role == Role.Scp || role == Role.Both) ? (byte)1 : (byte)0;
        }

        public override long Size
        {
            get
            {
                // uidlength, uid, scurole and scprole
                return base.Size + sizeof(short) + uid.Length + sizeof(byte) + sizeof(byte);
            }
        }

        public override long Read(Stream stream)
        {
            long bytes = 0;
            if (stream.CanRead)
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);
                long start = stream.Position;
                base.Read(stream);
                uidlength = reader.ReadInt16();
                byte[] temp = reader.ReadBytes(uidlength);
                uid = System.Text.Encoding.ASCII.GetString(temp);
                scurole = reader.ReadByte();
                scprole = reader.ReadByte();
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
                length = (short)Size;
                base.Write(stream);
                uidlength = (short)uid.Length;
                writer.Write(uidlength);
                writer.Write(uid.ToCharArray());
                writer.Write(scurole);
                writer.Write(scprole);
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            return String.Format("ScpScuRoleSelectionItem\n{0}\nuidlength={1} uid={2} scurole={3} scprole={4}", base.Dump(), uidlength, uid, scurole, scprole);
        }
    }

    public class AssociateRequestPdu : ProtocolDataUnit
    {
        public short version;
        public short reserved2;
        public string called;
        public string calling;
        public byte[] reserved3;
        public List<Item> fields;

        public AssociateRequestPdu()
            : this((ProtocolDataUnit.Type)0xff, String.Empty, String.Empty)
        {
        }

        public AssociateRequestPdu(ProtocolDataUnit.Type type, string called, string calling) 
            : base(type)
        {
            this.reserved1 = 0;
            this.length = -1;
            this.version = 0x01;
            this.reserved2 = 0;
            this.called = pad(called, 16);
            this.calling = pad(calling, 16);
            this.reserved3 = new byte[32];
            this.fields = new List<Item>();
        }

        public override long Size
        {
            get
            {
                // version, reserved2, called, calling, and reserved3
                long size = sizeof(short) + sizeof(short) + called.Length + calling.Length + reserved3.Length;
                foreach (Item item in fields)
                {
                    size += item.Size;
                }
                length = (short)size;
                return size;
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
                version = reader.ReadInt16();
                reserved2 = reader.ReadInt16();

                byte[] temp = reader.ReadBytes(16);
                called = System.Text.Encoding.ASCII.GetString(temp);
                temp = reader.ReadBytes(16);
                calling = System.Text.Encoding.ASCII.GetString(temp);
                reserved3 = reader.ReadBytes(32);
                byte next = reader.PeekByte();
                Item item = null;
                while (next == 0x10 || next == 0x20 || next == 0x21 || next == 0x50)
                {
                    switch (next)
                    {
                        case 0x10:
                            item = new ApplicationContextItem();
                            break;
                        case 0x20:
                        case 0x21:
                            item = new PresentationContextItem(next);
                            break;
                        case 0x50:
                            item = new UserInfoItem();
                            break;
                    }
                    fields.Add(item);
                    item.Read(stream);
                    if (stream.Position >= stream.Length)
                        break;
                    next = reader.PeekByte();
                }
                if (fields.Count < 3)
                {
                    throw new Exception("Ill-formed Protocol Data Unit.");
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
                writer.Write(version);
                reserved2 = 0;
                writer.Write(reserved2);
                writer.Write(called.ToCharArray());
                writer.Write(calling.ToCharArray());
                reserved3 = new byte[32];
                writer.Write(reserved3);
                foreach (Item item in fields)
                {
                    item.Write(stream);
                }
                bytes = stream.Position - start;
            }
            return bytes;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format("AssociateRequestPdu\n{0}\n\ttype={1} reserved1={2} length={3} version={4} reserved2={5} called={6} calling={7} reserved3={8}", 
                ToText(), type, reserved1, length, version, reserved2, called, calling, reserved3));
            foreach (Item item in fields)
            {
                text.Append(item.Dump());
                text.Append("\n"); 
            }
            return text.ToString();

        }

    }
}
