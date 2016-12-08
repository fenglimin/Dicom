using System;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class PresentationLUTServiceSCU : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string AffectedSOPClassUID;
        string AffectedSOPInstanceUID;

        public PresentationLUTServiceSCU()
            : base(SOPClass.PresentationLUTSOPClass)
        {
        }

        public PresentationLUTServiceSCU(PresentationLUTServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new PresentationLUTServiceSCU(this);
        }

        #region IPresentationDataSink Members

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("PLUTServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            Logging.Log(String.Format("PLUTServiceSCU.OnData: command={0:x4}", command));
            switch ((CommandType)command)
            {
                case CommandType.N_CREATE_RSP:
                    NCreate(control, dicom);
                    break;
            }
        }

        private void NCreate(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-CREATE-RSP");

                AffectedSOPClassUID = (string)dicom[t.AffectedSOPClassUID].Value;
                AffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;

                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-CREATE-RSP-DATA");

                if (AffectedSOPClassUID == SOPClass.PresentationLUTSOPClass)
                {
                    // update the plut 
                }
                else
                {
                    Logging.Log("N-CREATE:Unknown SOP class {0}", AffectedSOPClassUID);
                    return;
                }
                completeEvent.Set();
            }
        }

        #endregion
    }

    public class PresentationLUTServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string AffectedSOPClassUID;
        string AffectedSOPInstanceUID;
        ushort MessageId;

        public PresentationLUTServiceSCP()
            : base(SOPClass.PresentationLUTSOPClass)
        {
        }

        public PresentationLUTServiceSCP(PresentationLUTServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new PresentationLUTServiceSCP(this);
        }

        #region IPresentationDataSink Members

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("PLUTServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            Logging.Log(String.Format("PLUTServiceSCP.OnData: command={0:x4}", command));
            switch ((CommandType)command)
            {
                case CommandType.N_CREATE_RQ:
                    NCreate(control, dicom);
                    break;
            }
        }

        private void NCreate(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-CREATE-RQ");

                AffectedSOPClassUID = (string)dicom[t.AffectedSOPClassUID].Value;
                AffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-CREATE-DATA");

                if (AffectedSOPClassUID == SOPClass.PresentationLUTSOPClass)
                {
                    //PresentationLUT plut = new PresentationLUT();
                    //plut.Dicom = dicom;
                }
                else
                {
                    Logging.Log("N-SET: Unknown SOP class {0}", AffectedSOPClassUID);
                    return;
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, AffectedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_CREATE_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
                fragment.Add(t.Status, (ushort)0);
                fragment.Add(t.AffectedSOPInstanceUID, AffectedSOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-CREATE-RSP", request);
            }
        }

        #endregion
    }
}
