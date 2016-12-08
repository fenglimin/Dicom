namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// An SCU service that can perform C-ECHO requests.
    /// </summary>
    public class VerificationServiceSCU : ServiceClass, IPresentationDataSink
    {

        public VerificationServiceSCU() : base(SOPClass.VerificationSOPClass)
        {
        }

        public VerificationServiceSCU(VerificationServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new VerificationServiceSCU(this);
        }

        /// <summary>
        /// Perform a C-ECHO request over an established association
        /// </summary>
        /// <returns></returns>
        public ServiceStatus Echo()
        {

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)CommandType.C_ECHO_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);

            SendCommand("C-ECHO-RQ", command);

            ProcessResponse("C-ECHO-RQ", 8000, null, null);

            return LastMessage.CommandStatus;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("VerificationServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            ushort status = (ushort)dicom[t.Status].Value;
            Logging.Log("<< C-ECHO-RSP {0}", (status == 0x0000) ? "SUCCESS" : "FAILURE");

            completeEvent.Set();
        }
    }

    /// <summary>
    /// An SCP service that responds to C-ECHO requests.
    /// </summary>
    public class VerificationServiceSCP : ServiceClass, IPresentationDataSink
    {
        public VerificationServiceSCP() 
            : base(SOPClass.VerificationSOPClass)
        {
        }

        public VerificationServiceSCP(VerificationServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new VerificationServiceSCP(this);
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("VerificationServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            PresentationDataPdu pdu = new PresentationDataPdu(syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet response = new DataSet();

            response.Add(t.GroupLength(0), (uint)0);//
            response.Add(t.AffectedSOPClassUID, this.SOPClassUId);
            response.Add(t.CommandField, (ushort)CommandType.C_ECHO_RSP);
            ushort messageId = (ushort)dicom[t.MessageId].Value;
            response.Add(t.MessageIdBeingRespondedTo, messageId);
            response.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
            response.Add(t.Status, (ushort)0);

            pdv.Dicom = response;
            pdu.Values.Add(pdv);

            SendPdu("C-ECHO-RSP", pdu);
        }

    }

}
