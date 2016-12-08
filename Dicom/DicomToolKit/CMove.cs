using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class CMoveServiceSCU : ServiceClass, IPresentationDataSink
    {
        DataSet results = null;
        public event CMoveEventHandler ImageMoved;

        public CMoveServiceSCU(string uid)
            : base(uid)
        {
        }

        public CMoveServiceSCU(CMoveServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new CMoveServiceSCU(this);
        }

        public DataSet CGet(Element element)
        {
            return Execute(CommandType.C_GET_RQ, null, element);
        }

        public DataSet CMove(string destination, Element element)
        {
            return Execute(CommandType.C_MOVE_RQ, destination, element);
        }

        private DataSet Execute(CommandType type, string destination, Element element)
        {

            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)type);
            command.Add(t.MessageId, (ushort)1);
            if (destination != null)
            {
                command.Add(t.MoveDestination, destination);
            }
            command.Add(t.Priority, (ushort)Priority.Medium);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            results = new DataSet();

            SendPdu(type.ToString(), pdu);

            DataSet dicom = new DataSet();
            dicom.Part10Header = false;

            if (element.Tag.Equals(t.PatientID))
            {
                dicom.Set(t.QueryRetrieveLevel, "PATIENT");
            }
            else if (element.Tag.Equals(t.StudyInstanceUID))
            {
                dicom.Set(t.QueryRetrieveLevel, "STUDY");
            }
            else if (element.Tag.Equals(t.SeriesInstanceUID))
            {
                dicom.Set(t.QueryRetrieveLevel, "SERIES");
            }
            else if (element.Tag.Equals(t.SOPInstanceUID))
            {
                dicom.Set(t.QueryRetrieveLevel, "IMAGE");
            }
            else
            {
                throw new Exception("Unsupported Unique Key Attribute.");
            }
            dicom.Add(element);

            SendDataPdu(type.ToString() + " DATA", dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("A701","Refused: Out of Resources ?Unable to calculate number of matches");
            errors.Add("A702","Refused: Out of Resources ?Unable to perform sub-operations");
            errors.Add("A801","Refused: Move Destination unknown");
            errors.Add("A900","Identifier does not match SOP Class");

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B000","Sub-operations Complete ?One or more Failures");

            ProcessResponse(type.ToString(), 30000, errors, warnings);

            return results;
        }

        public void OnData(MessageType control, Message message)
        {
            // TODO put a lock on the pdu array
            // TODO interpret the commands

            Logging.Log("CMoveServiceSCU({0}).OnData", Reflection.GetName(typeof(SOPClass), this.SOPClassUId));

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                ushort status = (ushort)dicom[t.Status].Value;
                bool last = !(status == 0xFF00);

                DataSetType present = (DataSetType)dicom[t.CommandDataSetType].Value;

                Logging.Log("<< {0} {1},{2}", Enum.Parse(typeof(CommandType), dicom[t.CommandField].Value.ToString()), (last) ? "LAST FRAGMENT" : "NOT-LAST FRAGMENT", present.ToString());
                //dicom.Dump();

                string text = String.Empty;
                if (dicom.Contains(t.NumberofRemainingSuboperations))
                {
                    text += String.Format("remaining={0} ", dicom[t.NumberofRemainingSuboperations].Value);
                    results.Set(t.NumberofRemainingSuboperations, dicom[t.NumberofRemainingSuboperations].Value);
                }
                if (dicom.Contains(t.NumberofCompletedSuboperations))
                {
                    text += String.Format("completed={0} ", dicom[t.NumberofCompletedSuboperations].Value);
                    results.Set(t.NumberofCompletedSuboperations, dicom[t.NumberofCompletedSuboperations].Value);
                }
                if (dicom.Contains(t.NumberofFailedSuboperations))
                {
                    text += String.Format("failed={0} ", dicom[t.NumberofFailedSuboperations].Value);
                    results.Set(t.NumberofFailedSuboperations, dicom[t.NumberofFailedSuboperations].Value);
                }
                if (dicom.Contains(t.NumberofWarningSuboperations))
                {
                    text += String.Format("warning={0} ", dicom[t.NumberofWarningSuboperations].Value);
                    results.Set(t.NumberofWarningSuboperations, dicom[t.NumberofWarningSuboperations].Value);
                }

                if (this.ImageMoved != null)
                {
                    ImageMoved(this, new CMoveEventArgs(dicom));
                }

                Logging.Log(text);

                if (last)
                {
                    dicom.Remove(t.NumberofRemainingSuboperations);
                    // TODO do we reset the state to Open here
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< P-DATA-TF");
            }
        }
    }

    public class CMoveServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort MessageId;
        string destination;

        public CMoveServiceSCP(string uid)
            : base(uid)
        {
        }

        public CMoveServiceSCP(CMoveServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new CMoveServiceSCP(this);
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("\nCMoveServiceSCP({0}).OnData", Reflection.GetName(typeof(SOPClass), this.SOPClassUId));

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                MessageId = (ushort)dicom[t.MessageId].Value;
                // TODO accomodate trailing spaces
                destination = ((string)dicom[t.MoveDestination].Value).TrimEnd();
            }
            else
            {
                PresentationDataValue pdv;
                PresentationDataPdu command;
                DataSet fragment = new DataSet();

                if (!this.Association.Hosts.ContainsKey(destination))
                {
                    pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                    fragment.Part10Header = false;
                    fragment.TransferSyntaxUID = syntaxes[0];

                    fragment.Add(t.GroupLength(0), (uint)0);
                    fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);
                    fragment.Add(t.CommandField, (ushort)CommandType.C_MOVE_RSP);
                    fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                    fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                    fragment.Add(t.Status, (ushort)0xA801);

                    pdv.Dicom = fragment;

                    command = new PresentationDataPdu(syntaxes[0]);
                    command.Values.Add(pdv);

                    SendPdu("C-MOVE-RSP", command);
                    return;
                }

                ApplicationEntity host = this.Association.Hosts[destination];

                List<string> imagelist = Query(dicom);

                int failed = 0;
                for( int n = 0; n < imagelist.Count; n++)
                {
                    if (!SendImage(host, imagelist[n]))
                    {
                        failed++;
                    }

                    pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                    fragment = new DataSet();

                    fragment.Part10Header = false;
                    fragment.TransferSyntaxUID = syntaxes[0];

                    fragment.Add(t.GroupLength(0), (uint)0);
                    fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);
                    fragment.Add(t.CommandField, (ushort)CommandType.C_MOVE_RSP);
                    fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                    fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
                    fragment.Add(t.Status, (ushort)0xff00);

                    fragment.Set(t.NumberofRemainingSuboperations, imagelist.Count - n - 1);
                    fragment.Set(t.NumberofCompletedSuboperations, n + 1 - failed);
                    fragment.Set(t.NumberofFailedSuboperations, failed);
                    fragment.Set(t.NumberofWarningSuboperations, 0);

                    pdv.Dicom = fragment;

                    command = new PresentationDataPdu(syntaxes[0]);
                    command.Values.Add(pdv);

                    SendPdu("C-MOVE-RSP", command);

                }

                int status = (failed > 0) ? 0xB000 : 0x0000;

                pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                fragment = new DataSet();

                fragment.Part10Header = false;
                fragment.TransferSyntaxUID = syntaxes[0];

                fragment.Add(t.GroupLength(0), (uint)0);
                fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);
                fragment.Add(t.CommandField, (ushort)CommandType.C_FIND_RSP);
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                fragment.Add(t.Status, (ushort)status);

                fragment.Set(t.NumberofCompletedSuboperations, imagelist.Count);
                fragment.Set(t.NumberofFailedSuboperations, 0);
                fragment.Set(t.NumberofWarningSuboperations, 0);

                pdv.Dicom = fragment;

                command = new PresentationDataPdu(syntaxes[0]);
                command.Values.Add(pdv);

                SendPdu("C-MOVE-RSP", command);
            }
        }

        private bool SendImage(ApplicationEntity host, string path)
        {
            bool result = true;
            try
            {
                DataSet dicom = new DataSet();
                dicom.Read(path);

                string uid = (string)dicom[t.SOPClassUID].Value;
                StorageServiceSCU storage = new StorageServiceSCU(uid);
                storage.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                Association association = new Association();
                association.AddService(storage);

                if (association.Open(host))
                {
                    if (storage.Active)
                    {
                        try
                        {
                            storage.Store(dicom);
                        }
                        catch (Exception ex)
                        {
                            result = false;
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                else
                {
                    result = false;
                    //Debug.WriteLine("\ncan't Open.");
                }

                association.Close();
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        public virtual List<string> Query(DataSet query)
        {
            DicomDir dir = new DicomDir(".");

            const int Patient = 0;
            const int Study = 1;
            const int Series = 2;
            const int Image = 3;

            string level = (string)query[t.QueryRetrieveLevel].Value;
            int type = Patient;
            string value = String.Empty;
            switch (level)
            {
                case "PATIENT":
                    type = Patient;
                    value = (string)query[t.PatientID].Value;
                    break;
                case "STUDY":
                    type = Study;
                    value = (string)query[t.StudyInstanceUID].Value;
                    break;
                case "SERIES":
                    type = Series;
                    value = (string)query[t.SeriesInstanceUID].Value;
                    break;
                case "IMAGE":
                    type = Image;
                    value = (string)query[t.SOPInstanceUID].Value;
                    break;
            }

            List<string> results = new List<string>();

            bool match = false;
            foreach (Patient patient in dir.Patients)
            {
                match = match || (type == Patient && patient.PatientID == value);
                if (match || type > Patient)
                {
                    foreach (Study study in patient)
                    {
                        match = match || (type == Study && study.StudyInstanceUID == value);
                        if (match || type > Study)
                        {
                            foreach (Series series in study)
                            {
                                match = match || (type == Series && series.SeriesInstanceUID == value);
                                if (match || type > Series)
                                {
                                    foreach (Image image in series)
                                    {
                                        match = match || (type == Image && image.ReferencedSOPInstanceUIDinFile == value);
                                        if (match)
                                        {
                                            results.Add(image.ReferencedFileID);
                                            if (type == Image && image.ReferencedSOPInstanceUIDinFile == value)
                                                break;
                                        }
                                    }
                                    if ((type == Series && series.SeriesInstanceUID == value) || (match && type > Series))
                                        break;
                                }
                            }
                            if ((type == Study && study.StudyInstanceUID == value) || (match && type > Study))
                                break;
                        }
                    }
                    if ((type == Patient && patient.PatientID == value) || (match && type > Patient))
                        break;
                }
            }
            return results;
        }
    }

    public class CMoveEventArgs : EventArgs
    {
        private DataSet dicom;
        private bool cancel;

        public ushort Undefined = ushort.MaxValue;

        public CMoveEventArgs(DataSet dicom)
        {
            this.cancel = false;
            this.dicom = dicom;
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

        public ushort Remaining
        {
            get
            {
                if(dicom.Contains(t.NumberofRemainingSuboperations))
                    return (ushort)dicom[t.NumberofRemainingSuboperations].Value;
                else
                    return Undefined;
            }
        }

        public ushort Completed
        {
            get
            {
                if(dicom.Contains(t.NumberofCompletedSuboperations))
                    return (ushort)dicom[t.NumberofCompletedSuboperations].Value;
                else
                    return Undefined;
            }
        }

        public ushort Failed
        {
            get
            {
                if(dicom.Contains(t.NumberofFailedSuboperations))
                    return (ushort)dicom[t.NumberofFailedSuboperations].Value;
                else
                    return Undefined;
            }
        }

        public ushort Warning
        {
            get
            {
                if(dicom.Contains(t.NumberofWarningSuboperations))
                    return (ushort)dicom[t.NumberofWarningSuboperations].Value;
                else
                    return Undefined;
            }
        }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            if (Remaining != Undefined)
                text.Append(String.Format("Remaining={0} ", Remaining));
            if (Completed != Undefined)
                text.Append(String.Format("Completed={0} ", Completed));
            if (Failed != Undefined)
                text.Append(String.Format("Failed={0} ", Failed));
            if (Warning != Undefined)
                text.Append(String.Format("Warning={0} ", Warning));
            text.Append(String.Format("Cancel={0} ", Cancel));
            return text.ToString();
        }
    }

    public delegate void CMoveEventHandler(object sender, CMoveEventArgs e);
}
