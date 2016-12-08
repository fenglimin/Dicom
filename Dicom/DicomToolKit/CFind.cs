using System;
using System.Collections.Generic;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class CFindServiceSCU : ServiceClass, IPresentationDataSink
    {
        RecordCollection records = null;

        public CFindServiceSCU(string uid)
            : base(uid)
        {
        }

        public CFindServiceSCU(CFindServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new CFindServiceSCU(this);
        }

        public RecordCollection CFind(DataSet dicom)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)CommandType.C_FIND_RQ);
            command.Add(t.MessageId, (ushort)1);
            command.Add(t.Priority, (ushort)Priority.Medium);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            records = new RecordCollection();

            SendPdu("C-FIND-RQ", pdu);
            SendDataPdu("C-FIND-RQ DATA", dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("A700","Refused: Out of Resources");
            errors.Add("A900","Identifier does not match SOP Class");
            errors.Add("C","Unable to process");
 
            ProcessResponse("C-FIND-RQ", 30000, errors, null);
            // it is figured that Status.Pending would not bee seen here

            return records;
        }

        public void OnData(MessageType control, Message message)
        {
            // TODO put a lock on the pdu array
            // TODO interpret the commands

            Logging.Log("CFindServiceSCU({0}).OnData", Reflection.GetName(typeof(SOPClass), this.SOPClassUId));

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                ushort status = (ushort)dicom[t.Status].Value;
                bool last = !(status == 0xFF00 || status == 0xFF01);

                //if (!last && status == 0xFF01)
                //{
                //    Logging.Log("Warning that one or more Optional Keys were not supported for existence and/or matching for this Identifier.");
                //}

                DataSetType present = (DataSetType)dicom[t.CommandDataSetType].Value;

                Logging.Log("<< C-FIND-RSP {0},{1}", (last) ? "LAST FRAGMENT" : "NOT-LAST FRAGMENT", present.ToString());
                //dicom.Dump();

                if (last)
                {
                    // TODO do we reset the state to Open here
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< P-DATA-TF");
                if (records == null)
                {
                    records = new RecordCollection();
                }
                records.Add(dicom.Elements);
            }
        }
    }

    /// <summary>
    /// EventArgs sent with a QueryEventHandler that contains the query and the recordcollection to fill.
    /// Set the Cancel property to True to reject the query.
    /// </summary>
    public class QueryEventArgs : EventArgs
    {
        private DataSet query;
        private bool cancel;
        private RecordCollection records;

        public QueryEventArgs(DataSet query)
        {
            this.cancel = false;
            this.query = query;
            this.records = new RecordCollection();
        }

        public DataSet Query
        {
            get
            {
                return query;
            }
        }

        public RecordCollection Records
        {
            get
            {
                return records;
            }
            set
            {
                records = value;
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

    /// <summary>
    /// Called whenever a StorageServiceSCP instance has an image ready from a C-STORE request.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void QueryEventHandler(object sender, QueryEventArgs e);

    public class CFindServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort MessageId;
        public event QueryEventHandler Query;

        public CFindServiceSCP(string uid)
            : base(uid)
        {
        }

        public CFindServiceSCP(CFindServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            CFindServiceSCP temp = new CFindServiceSCP(this);
            temp.Query = this.Query;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("\nCFindServiceSCP({0}).OnData", Reflection.GetName(typeof(SOPClass), this.SOPClassUId));

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                PresentationDataValue pdv;
                PresentationDataPdu command;

                DataSet fragment = new DataSet();


                RecordCollection worklist;
                if (Query != null)
                {
                    QueryEventArgs args = new QueryEventArgs(dicom);
                    Query(this, args);
                    if (args.Cancel)
                    {
                        association.Abort();
                        return;
                    }
                    worklist = args.Records;
                }
                else
                {
                    worklist = InternalQuery(dicom.Elements);
                }

                foreach (Elements procedure in worklist)
                {
                    fragment = new DataSet();
                    pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                    fragment.Add(t.GroupLength(0), (uint)0);
                    fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);
                    fragment.Add(t.CommandField, (ushort)CommandType.C_FIND_RSP);
                    fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                    fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
                    fragment.Add(t.Status, (ushort)0xff00);

                    pdv.Dicom = fragment;

                    command = new PresentationDataPdu(syntaxes[0]);
                    command.Values.Add(pdv);

                    PresentationDataValue response = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastDataSet);

                    DataSet temp = new DataSet();
                    temp.Elements = procedure;

                    temp.Part10Header = false;
                    temp.TransferSyntaxUID = syntaxes[0];

                    response.Dicom = temp;
                    command.Values.Add(response);

                    Logging.Log(command.Dump());
                    SendPdu("C-FIND-RSP", command);

                }

                pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);
                fragment.Add(t.AffectedSOPClassUID, this.SOPClassUId);
                fragment.Add(t.CommandField, (ushort)CommandType.C_FIND_RSP);
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                fragment.Add(t.Status, (ushort)0x0000);

                pdv.Dicom = fragment;
                
                command = new PresentationDataPdu(syntaxes[0]);
                command.Values.Add(pdv);

                Logging.Log(command.Dump());
                SendPdu("C-FIND-RSP", command);
            }
        }

        public RecordCollection InternalQuery(Elements query)
        {
            RecordCollection records = new RecordCollection();
            if (this.SOPClassUId != SOPClass.ModalityWorklistInformationModelFIND)
            {
                DicomDir dir = new DicomDir(".");

                string level = (string)query[t.QueryRetrieveLevel].Value;

                foreach (Patient patient in dir.Patients)
                {
                    if (level == "PATIENT")
                    {
                        if (Compare(query, patient.Elements))
                        {
                            records.Add(patient.Elements);
                        }
                    }
                    else
                    {
                        foreach (Study study in patient)
                        {
                            if (level == "STUDY")
                            {
                                if (Compare(query, study.Elements))
                                {
                                    records.Add(study.Elements);
                                }
                            }
                            else
                            {
                                foreach (Series series in study)
                                {
                                    if (level == "SERIES")
                                    {
                                        if (Compare(query, series.Elements))
                                        {
                                            records.Add(series.Elements);
                                        }
                                    }
                                    else
                                    {
                                        foreach (Image image in series)
                                        {
                                            if (Compare(query, image.Elements))
                                            {
                                                records.Add(image.Elements);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return records;
        }

        public bool Compare(Elements filter, Elements record)
        {
            bool result = true;
            foreach (Element element in filter.InOrder)
            {
                if (element.Group < 8 || element.Tag.Equals(t.SpecificCharacterSet))
                    continue;
                string key = element.Tag.ToString();
                if(!filter.ValueExists(key) || "*" == key)
                    continue;
                if(record.Contains(key))
                {
                    if (element.Value != record[element.Tag.ToString()].Value)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }

    }
}
