using System;
using System.Collections.Generic;

namespace EK.Capture.Dicom.DicomToolKit
{

    public class MppsServiceSCU : ServiceClass, IPresentationDataSink
    {
        public MppsServiceSCU()
            : base(SOPClass.ModalityPerformedProcedureStepSOPClass)
        {
        }

        public MppsServiceSCU(MppsServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new MppsServiceSCU(this);
        }

        public bool Begin(string uid, DataSet dicom)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)CommandType.N_CREATE_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.Priority, (ushort)Priority.Medium);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.AffectedSOPInstanceUID, uid);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-CREATE-RQ", pdu);
            SendDataPdu("N-CREATE-DATA", dicom);

            ProcessResponse("N-CREATE-RQ", 8000, null, null);

            return true;
        }

        public bool End(string uid, DataSet dicom)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)CommandType.N_SET_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.Priority, (ushort)Priority.Medium);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.RequestedSOPInstanceUID, uid);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-SET-RQ", pdu);
            SendDataPdu("N-SET-DATA", dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("0110", "Performed Procedure Step Object may no longer be updated");

            ProcessResponse("N-SET-RQ", 8000, errors, null);
            return true;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("MppsServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            ushort command = (ushort)dicom[t.CommandField].Value;
            if (command == (ushort)CommandType.N_CREATE_RSP)
            {
                Logging.Log("<< N-CREATE-RSP");
            }
            else if (command == (ushort)CommandType.N_SET_RSP)
            {
                Logging.Log("<< N-SET-RSP");
            }
            ushort status = (ushort)dicom[t.Status].Value;
            if (status != 0)
            {
                Logging.Log("N-CREATE/N-SET status of {0:x4}", status);
            }
            completeEvent.Set();
        }
    }

	public class MppsEventArgs : DicomEventArgs
    {
        private string uid;
        private ushort command;
        private DataSet dicom;
        private bool cancel;

        public MppsEventArgs(string uid, ushort command, DataSet dicom)
        {
            this.uid = uid;
            this.cancel = false;
            this.command = command;
            this.dicom = dicom;
        }

        public String InstanceUid
        {
            get
            {
                return uid;
            }
        }

        public ushort Command
        {
            get
            {
                return command;
            }
        }

        public DataSet DataSet
        {
            get
            {
                return dicom;
            }
        }

        public bool Cancel
        {
            get
            {
                return cancel;
            }
            set
            {
                cancel = value;
            }
        }
    }

    public delegate void MppsEventHandler(object sender, MppsEventArgs e);

    public class MppsServiceSCP : ServiceClass, IPresentationDataSink
    {
        string SOPInstanceUID;
        ushort MessageId;
        ushort command;
        public event MppsEventHandler MppsCreate;
        public event MppsEventHandler MppsSet;

        public MppsServiceSCP()
            : base(SOPClass.ModalityPerformedProcedureStepSOPClass)
        {
        }

        public MppsServiceSCP(MppsServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            MppsServiceSCP temp = new MppsServiceSCP(this);
            temp.MppsCreate = this.MppsCreate;
            temp.MppsSet = this.MppsSet;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("MppsServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
                SOPInstanceUID = (command == (ushort)CommandType.N_CREATE_RQ) ? 
                    (string)dicom[t.AffectedSOPInstanceUID].Value :
                    (string)dicom[t.RequestedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                dicom.Add(t.GroupLength(2), (ulong)0);
                dicom.Add(t.FileMetaInformationVersion, new byte[] { 0, 1 });
                dicom.Add(t.MediaStorageSOPClassUID, this.SOPClassUId);
                dicom.Add(t.MediaStorageSOPInstanceUID, this.SOPInstanceUID);
                dicom.Add(t.TransferSyntaxUID, this.syntaxes[0]);
                dicom.Add(t.ImplementationClassUID, "1.2.3.4");
                dicom.Add(t.ImplementationVersionName, "not specified");

                dicom.Part10Header = true;
                dicom.TransferSyntaxUID = syntaxes[0];

                MppsEventArgs mpps = new MppsEventArgs(SOPInstanceUID, command, dicom);
				mpps.CallingAeTitle = association.CallingAeTitle;
				mpps.CallingAeIpAddress = association.CallingAeIpAddress;
                if (command == (ushort)CommandType.N_CREATE_RQ && MppsCreate != null)
                {
                    MppsCreate(this, mpps);
                }
                else if (command == (ushort)CommandType.N_SET_RQ && MppsSet != null)
                {
                    MppsSet(this, mpps);
                }
                else
                {
                    string uid = (string)dicom[t.SOPInstanceUID].Value;
                    dicom.Write(uid + ".dcm");
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)144);//
                fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);//
                fragment.Add(t.CommandField, (ushort)command | 0x8000);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
                fragment.Add(t.Status, (int)((mpps.Cancel) ? 0xC000 : 0));
                fragment.Add(t.AffectedSOPInstanceUID, SOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu((command == (ushort)CommandType.N_CREATE_RQ) ? "N-CREATE-RSP" : ">> N-SET-RSP", request);
            }
        }
    }
}
