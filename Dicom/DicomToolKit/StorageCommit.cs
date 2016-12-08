using System;
using System.Net;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// An SCU service that can perform C-STORE requests for a given SOP class.
    /// </summary>
    public class StorageCommitServiceSCU : ServiceClass, IPresentationDataSink
    {
        public StorageCommitServiceSCU()
            : base(SOPClass.StorageCommitmentPushModelSOPClass)
        {
        }

        public StorageCommitServiceSCU(StorageCommitServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new StorageCommitServiceSCU(this);
        }

        /// <summary>
        /// Perform an N-ACTION Storage Commitment request over an established association
        /// </summary>
        /// <returns></returns>
        public bool Request(DataSet dicom)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.RequestedSOPClassUID, SOPClass.StorageCommitmentPushModelSOPClass);
            command.Add(t.CommandField, (ushort)CommandType.N_ACTION_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.RequestedSOPInstanceUID, SOPClass.StorageCommitmentPushModelSOPInstance);
            command.Add(t.ActionTypeID, "1");

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-ACTION-RQ", pdu);

            DataSet data = new DataSet();
            data.Part10Header = false;
            dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;

            data.Set(t.SOPClassUID, SOPClass.StorageCommitmentPushModelSOPClass);
            data.Set(t.SOPInstanceUID, Element.UidRoot);
            data.Set(t.TransactionUID, Element.NewUid());

            Sequence sequence = new Sequence(t.ReferencedSOPSequence);
            data.Add(sequence);
            Elements item = sequence.NewItem();
            item.Set(t.ReferencedSOPClassUID, (string)dicom[t.SOPClassUID].Value);
            item.Set(t.ReferencedSOPInstanceUID, (string)dicom[t.SOPInstanceUID].Value);

            SendDataPdu("N-ACTION-RQ DATA", data);

            ProcessResponse("N-ACTION-RQ (Storage Commit)", 8000, null, null);
            return true;
        }

        /// <summary>
        /// Perform an N-EVENT-REPORT Storage Commitment report over an established association with the ScpScuRole of Scp.
        /// </summary>
        /// <returns></returns>
        public bool Report(DataSet dicom)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            // for storage commit, ScpScuRoleSelectionItem is mandatory and N_EVENT_REPORT_RQ generator is the SCP
            // perhaps move Report to StorageCommitServiceSCU, and move responses in here
            Role = Role.Scp;

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClass.StorageCommitmentPushModelSOPClass);
            command.Add(t.CommandField, (ushort)CommandType.N_EVENT_REPORT_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.AffectedSOPInstanceUID, SOPClass.StorageCommitmentPushModelSOPInstance);
            command.Add(t.ActionTypeID, "1");

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N_EVENT_REPORT_RQ", pdu);

            if (dicom.Contains(t.SOPClassUID))
                dicom.Remove(t.SOPClassUID);
            if(dicom.Contains(t.SOPInstanceUID))
                dicom.Remove(t.SOPInstanceUID);

            SendDataPdu("N_EVENT_REPORT_RQ DATA", dicom);

            ProcessResponse("N_EVENT_REPORT_RQ (Storage Commit)", 8000, null, null);
            return true;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("StorageCommitServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            ushort status = (ushort)dicom[t.Status].Value;
            CommandType command = (CommandType)dicom[t.CommandField].Value;

            Logging.Log("<< {0} {1}", command, (status == 0x0000) ? "SUCCESS" : "FAILURE");

            completeEvent.Set();

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum StorageCommitEventType
    {
        Request,
        Report
    }

    /// <summary>
    /// EventArgs sent with an StorageCommitEventHandler that contains the storage commitment.
    /// </summary>
    public class StorageCommitEventArgs : EventArgs
    {
        private DataSet dicom;
        private StorageCommitEventType type;

        public StorageCommitEventArgs(DataSet dicom, StorageCommitEventType type)
        {
            this.dicom = dicom;
            this.type = type;
        }

        public DataSet DataSet
        {
            get
            {
                return dicom;
            }
        }

        public StorageCommitEventType Type
        {
            get
            {
                return type;
            }
        }

		public string CallingAeTitle { get; set; }
		public IPAddress CallingAeIpAddress { get; set; }
    }

    /// <summary>
    /// Called whenever a StorageCommitServiceSCP instance has committed and image.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void StorageCommitEventHandler(object sender, StorageCommitEventArgs e);

    /// <summary>
    /// An SCP service that responds to C-STORE requests for a given SOP class.
    /// </summary>
    public class StorageCommitServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string RequestedSOPClassUID;
        string RequestedSOPInstanceUID;
        ushort MessageId;
        public event StorageCommitEventHandler StorageCommitReport;
        public event StorageCommitEventHandler StorageCommitRequest;

        public StorageCommitServiceSCP()
            : base(SOPClass.StorageCommitmentPushModelSOPClass)
        {
        }

        public StorageCommitServiceSCP(StorageCommitServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            StorageCommitServiceSCP temp = new StorageCommitServiceSCP(this);
            temp.StorageCommitReport = this.StorageCommitReport;
            temp.StorageCommitRequest = this.StorageCommitRequest;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                switch ((CommandType)command)
                {
                    case CommandType.N_EVENT_REPORT_RQ:
                        NEventReport(control, dicom);
                        break;
                    case CommandType.N_ACTION_RQ:
                        NAction(control, dicom);
                        break;
                }
            }
        }

        private void NEventReport(MessageType control, DataSet dicom)
        {
            if (!MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-EVENT-REPORT-DATA");

                if (StorageCommitReport != null)
                {
                    StorageCommitReport(this, new StorageCommitEventArgs(dicom, StorageCommitEventType.Report));
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();
                fragment.Part10Header = false;

                fragment.Add(t.GroupLength(0), (uint)0);
                fragment.Add(t.AffectedSOPClassUID, SOPClass.StorageCommitmentPushModelSOPClass);
                fragment.Add(t.CommandField, (ushort)CommandType.N_EVENT_REPORT_RSP);
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                fragment.Add(t.Status, (ushort)0);
                fragment.Add(t.AffectedSOPInstanceUID, SOPClass.StorageCommitmentPushModelSOPInstance);
                fragment.Add(t.ActionTypeID, "1");

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-EVENT-REPORT-RSP", request);
            }
        }

        private void NAction(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-ACTION-RQ");

                RequestedSOPClassUID = (string)dicom[t.RequestedSOPClassUID].Value;
                RequestedSOPInstanceUID = (string)dicom[t.RequestedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-ACTION-DATA");

                if (StorageCommitRequest != null)
                {
	                var args = new StorageCommitEventArgs(dicom, StorageCommitEventType.Request)
	                {
		                CallingAeIpAddress = association.CallingAeIpAddress,
		                CallingAeTitle = association.CallingAeTitle
	                };
	                StorageCommitRequest(this, args);
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();
                fragment.Part10Header = false;
                fragment.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_ACTION_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                fragment.Add(t.Status, (ushort)0);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-ACTION-RSP", request);
            }
        }

    }
}
