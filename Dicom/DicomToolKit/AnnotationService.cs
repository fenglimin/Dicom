using System;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class AnnotationServiceSCU : ServiceClass, IPresentationDataSink
    {
        ushort command;

        public AnnotationServiceSCU() 
            : base(SOPClass.BasicAnnotationBoxSOPClass)
        {
        }

        public AnnotationServiceSCU(AnnotationServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new AnnotationServiceSCU(this);
        }
    
        #region IPresentationDataSink Members

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("AnnotationServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            Logging.Log(String.Format("AnnotationServiceSCU.OnData: command={0:x4}", command));
            switch ((CommandType)command)
            {
                case CommandType.N_SET_RSP:
                    NSet(control, dicom);
                    break;
                case CommandType.N_DELETE_RSP:
                    NDelete(control, dicom);
                    break;
            }
        }

        private void NSet(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-SET-RSP");
                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-SET-RSP-DATA");
                completeEvent.Set();
            }
        }

        private void NDelete(MessageType control, DataSet dicom)
        {
            Logging.Log("<< N-DELETE-RSP");
            completeEvent.Set();
        }

        #endregion

    }

    public class AnnotationServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string RequestedSOPClassUID;
        string RequestedSOPInstanceUID;
        ushort MessageId;

        public AnnotationServiceSCP()
            : base(SOPClass.BasicAnnotationBoxSOPClass)
        {
        }

        public AnnotationServiceSCP(AnnotationServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new AnnotationServiceSCP(this);
        }

        #region IPresentationDataSink Members

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("AnnotationServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            switch ((CommandType)command)
            {
                case CommandType.N_SET_RQ:
                    NSet(control, dicom);
                    break;
                case CommandType.N_DELETE_RQ:
                    NDelete(control, dicom);
                    break;
            }
        }

        private void NSet(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-SET-RQ");

                RequestedSOPClassUID = (string)dicom[t.RequestedSOPClassUID].Value;
                RequestedSOPInstanceUID = (string)dicom[t.RequestedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-SET-DATA");

                if (RequestedSOPClassUID == SOPClass.BasicAnnotationBoxSOPClass)
                {
                    //Annotation annotation = annotations[RequestedSOPInstanceUID];
                    //annotation.Dicom = dicom;
                }
                else
                {
                    Logging.Log("N-SET: Unknown SOP class {0}", RequestedSOPClassUID);
                    return;
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, RequestedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_SET_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
                fragment.Add(t.Status, (ushort)0);
                fragment.Add(t.AffectedSOPInstanceUID, RequestedSOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-SET-RSP", request);
            }
        }

        private void NDelete(MessageType control, DataSet dicom)
        {
            Logging.Log("<< N-DELETE-RQ");
            //N-DELETE-RSP
        }

        #endregion

    }

}
