using System;
using System.Collections.Generic;
using System.Reflection;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class Reflection
    {
        public static string GetName(Type classes, string text)
        {
            string field = String.Empty;
            foreach (FieldInfo info in classes.GetFields())
            {
                if (text == info.GetValue(null).ToString())
                {
                    field = info.Name;
                    break;
                }
            }
            return field;
        }
    }

    public class SOPClass
    {
        // Part 6 Data Dictionary : Annex A Registry of DICOM unique identifiers (UID)

        public const string BasicFilmSessionSOPClass = "1.2.840.10008.5.1.1.1";
        public const string BasicFilmBoxSOPClass = "1.2.840.10008.5.1.1.2";
        public const string BasicGrayscaleImageBoxSOPClass = "1.2.840.10008.5.1.1.4";
        public const string BasicGrayscalePrintManagementMetaSOPClass = "1.2.840.10008.5.1.1.9";
        public const string PrinterSOPClass = "1.2.840.10008.5.1.1.16";
        public const string PrinterSOPInstance = "1.2.840.10008.5.1.1.17";
        public const string PresentationLUTSOPClass = "1.2.840.10008.5.1.1.23";
        public const string BasicAnnotationBoxSOPClass = "1.2.840.10008.5.1.1.15";

        public const string VerificationSOPClass = "1.2.840.10008.1.1";

        public const string StorageCommitmentPushModelSOPClass = "1.2.840.10008.1.20.1";
        public const string StorageCommitmentPushModelSOPInstance = "1.2.840.10008.1.20.1.1";

        public const string MediaStorageDirectoryStorage = "1.2.840.10008.1.3.10";

        public const string ComputedRadiographyImageStorage = "1.2.840.10008.5.1.4.1.1.1";
        public const string DigitalXRayImageStorageForPresentation = "1.2.840.10008.5.1.4.1.1.1.1";
        public const string DigitalXRayImageStorageForProcessing = "1.2.840.10008.5.1.4.1.1.1.1.1";
        public const string DigitalMammographyImageStorageForPresentation = "1.2.840.10008.5.1.4.1.1.1.2";
        public const string DigitalMammographyImageStorageForProcessing = "1.2.840.10008.5.1.4.1.1.1.2.1";
        public const string CTImageStorage = "1.2.840.10008.5.1.4.1.1.2";
        public const string EnhancedCTImageStorage = "1.2.840.10008.5.1.4.1.1.2.1";
        public const string MRImageStorage = "1.2.840.10008.5.1.4.1.1.4";
        public const string EnhancedMRImageStorage = "1.2.840.10008.5.1.4.1.1.4.1";
        public const string SecondaryCaptureImageStorage = "1.2.840.10008.5.1.4.1.1.7";
        public const string GrayscaleSoftcopyPresentationStateStorageSOPClass = "1.2.840.10008.5.1.4.1.1.11.1";
        public const string XRayAngiographicImageStorage = "1.2.840.10008.5.1.4.1.1.12.1";
        public const string EnhancedXAImageStorage = "1.2.840.10008.5.1.4.1.1.12.1.1";

        public const string XRayRadiationDoseSRStorage = "1.2.840.10008.5.1.4.1.1.88.67";

        public const string DetachedStudyManagementSOPClass = "1.2.840.10008.3.1.2.3.1";
        public const string ModalityPerformedProcedureStepSOPClass = "1.2.840.10008.3.1.2.3.3";
        public const string ModalityWorklistInformationModelFIND = "1.2.840.10008.5.1.4.31";

        public const string PatientRootQueryRetrieveInformationModelFIND = "1.2.840.10008.5.1.4.1.2.1.1";
        public const string PatientRootQueryRetrieveInformationModelMOVE = "1.2.840.10008.5.1.4.1.2.1.2";
        public const string PatientRootQueryRetrieveInformationModelGET = "1.2.840.10008.5.1.4.1.2.1.3";
        public const string StudyRootQueryRetrieveInformationModelFIND = "1.2.840.10008.5.1.4.1.2.2.1";
        public const string StudyRootQueryRetrieveInformationModelMOVE = "1.2.840.10008.5.1.4.1.2.2.2";
        public const string StudyRootQueryRetrieveInformationModelGET = "1.2.840.10008.5.1.4.1.2.2.3";

    }

    public class Status
    {
        private ServiceStatus type;
        private int code;
        private string description;

        public Status(ServiceStatus type, int code, string description)
        {
            this.type = type;
            this.code = code;
            this.description = description;
        }

        public ServiceStatus ServiceStatus
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
            }
        }

        public int Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                description = value;
            }
        }

    }

    public class ServiceClass : ICloneable
    {
        internal Association association;
        private string uid;
        internal List<string> syntaxes = new List<string>();
        internal System.Threading.AutoResetEvent completeEvent;
        private byte id = 0xff;
        private Message message;
        private PCIReason reason = PCIReason.Undefined;
        private Role role;

        static private Dictionary<string, string> common;

        static ServiceClass()
        {
            common = new Dictionary<string, string>();
            common.Add("0122","Refused: SOP class not supported");
            common.Add("0119","Class-instance conflict");
            common.Add("0111","Duplicate SOP instance");
            common.Add("0210","Duplicate invocation");
            common.Add("0115","Invalid argument value");
            common.Add("0106","Invalid attribute value");
            common.Add("0117","Invalid object instance");
            common.Add("0120","Missing attribute");
            common.Add("0121","Missing attribute value");
            common.Add("0212","Mistyped argument");
            common.Add("0114","No such argument");
            common.Add("0105","No such attribute");
            common.Add("0113","No suc hevent type");
            common.Add("0112","No Such object instance");
            common.Add("0118","No Such SOP class");
            common.Add("0110","Processing failure");
            common.Add("0213","Resource limitation");
            common.Add("0211","Unrecognized operation");
            common.Add("0123","No such action type");
        }

        protected ServiceClass(string uid)
        {
            this.uid = uid;
            this.id = 0xff;
            this.reason = PCIReason.Undefined;
            completeEvent = new System.Threading.AutoResetEvent(false);
            message = null;
            role = Role.Scu;    // this is only sent in an A_ASSOCIATE_RQ pdu, so this default is OK
        }

        protected ServiceClass(ServiceClass other)
        {
            this.association = null;
            this.uid = other.uid;
            foreach(string syntax in other.syntaxes)
            {
                this.syntaxes.Add((string)syntax.Clone());
            }
            completeEvent = new System.Threading.AutoResetEvent(false);
            this.id = 0xff;
            this.reason = PCIReason.Undefined;
            this.role = other.role;
        }

        public string SOPClassUId
        {
            get
            {
                return uid;
            }
        }

        public List<string> Syntaxes
        {
            get
            {
                return syntaxes;
            }
        }

        public Association Association
        {
            get
            {
                return association;
            }
            internal set
            {
                association = value;
            }
        }

        public bool Active
        {
            get
            {
                return (reason == PCIReason.Accepted);
            }
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

        public Message LastMessage
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
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
        public Role Role
        {
            get
            {
                return role;
            }
            set
            {
                role = value;
            }
        }

        public virtual object Clone()
        {
            return new ServiceClass(this);
        }

        #region delete me

        public bool SendPdu(string message, DicomObject pdu)
        {
            return association.SendPdu(this, message, pdu);
        }

        public bool SendDataPdu(string message, DataSet dicom)
        {
            return association.SendDataPdu(this, message, dicom);
        }

        #endregion delete me

        public bool SendCommand(string message, DataSet dicom)
        {
            return association.SendPdu(this, MessageType.Command, message, dicom);
        }

        public bool SendData(string message, DataSet dicom)
        {
            return association.SendPdu(this, MessageType.DataSet, message, dicom);
        }

        private void ProcessCodes(string text, Dictionary<string, string> codes)
        {
            // if the device has sent no ErrorComment, create one, otherwise use what they send
            if (!LastMessage.Dicom.ValueExists(t.ErrorComment))
            {
                if (codes != null)
                {
                    // see if we can come up with one based on code
                    foreach (KeyValuePair<string, string> kvp in codes)
                    {
                        if (message.Status.StartsWith(kvp.Key))
                        {
                            text = kvp.Value;
                        }
                    }
                }
                // finally add the comment
                LastMessage.Dicom.Add(t.ErrorComment, text);
            }
        }

        public void ProcessResponse(string command, int milliseconds, Dictionary<string, string> errors, Dictionary<string, string> warnings)
        {
            // first wait the request amount of time
            if (!completeEvent.WaitOne(milliseconds, false))
            {
                // in the case of a timeout throw an exception
                message = Message.TimeOut;
                throw new TimeoutException(String.Format("Timeout waiting for a {0} response.", command));
            }
            // if we have been aborted, throw an exception
            if (association.State == State.Aborted)
            {
                throw association.AbortException;
            }
            if (message.Dicom.Contains(t.Status))
            {
                // if have an error, update ErrorComment and throw an exception
                if (message.CommandStatus == ServiceStatus.Error)
                {
                    ProcessCodes(command + " error.", errors);
                    throw new DicomException(message, message.ErrorComment);
                }
                // if we have a warning, update ErrorComment
                else if (message.CommandStatus == ServiceStatus.Warning)
                {
                    ProcessCodes(command + " warning.", warnings);
                }
                // if we have a cancel, update ErrorComment
                else if (message.CommandStatus == ServiceStatus.Cancel)
                {
                    Dictionary<string, string> codes = new Dictionary<string, string>();
                    codes.Add("FE00", "Matching terminated due to Cancel request");

                    ProcessCodes(command + " cancelled.", codes);
                }
            }
        }
    }
    /// <summary>
    /// The priority of the Command.
    /// </summary>
    public enum Priority : ushort
    {
        Medium = 0x0000,
        High = 0x0001,
        Low = 0x0002
    }

    /// <summary>
    /// The status of the Service Command.
    /// </summary>
    public enum ServiceStatus
    {
        Success,
        Warning,
        Error,

        Cancel,
        Pending,
        TimeOut,
    }

    // TODO a Message can either be a command fragment or data fragment, only command fragments will have Status.
    public class Message
    {
        private DataSet dicom;

        private static readonly string SuccessMessage = "Success.";
        private static readonly string SuccessStatus = "0000";
        private static readonly string TimeOutMessage = "TimeOut waiting for response.";
        private static readonly string TimeOutStatus = "6666";  // not DICOM

        public Message()
        {
            dicom = new DataSet();
            dicom.Part10Header = false;
        }

        public Message(DataSet dicom)
        {
            Dicom = dicom;
        }

        public ServiceStatus ParseCode(ushort code)
        {
            ServiceStatus result = ServiceStatus.Success;
            if (code != 0x0000)
            {
                if (code == 0x001)
                {
                    result = ServiceStatus.Warning;
                }
                else
                {
                    string text = String.Format("{0:X4}", code);
                    switch (text[0])
                    {
                        case '6':       // not DICOM
                            result = ServiceStatus.TimeOut;
                            break;
                        case 'B':
                            result = ServiceStatus.Warning;
                            break;
                        case '0':       // some of these are Status.Warning, see PS 3.7 Annex C
                            result = ServiceStatus.Error;
                            break;
                        case 'F':
                            result = (text[1] == 'E') ? ServiceStatus.Cancel : ServiceStatus.Pending;
                            break;
                        case 'A':
                        case 'C':
                        default:
                            result = ServiceStatus.Error;
                            break;
                    }
                }
            }
            return result;
        }

        public DataSet Dicom
        {
            get
            {
                return dicom;
            }
            internal set
            {
                dicom = value;
            }
        }

        public ServiceStatus CommandStatus
        {
            get
            {
                ushort code = (dicom != null && dicom.Contains(t.Status)) ? (ushort)dicom[t.Status].Value : (ushort)0;
                return ParseCode(code);
            }
        }

        public string ErrorComment
        {
            get
            {
                return (dicom != null && dicom.Contains(t.Status)) ? (string)dicom[t.ErrorComment].Value : SuccessMessage;
            }
            internal set
            {
                // message may be a command message with a status, or a data message without
                dicom.Set(t.ErrorComment, value);
            }
        }

        public string Status
        {
            get
            {
                return (dicom != null && dicom.Contains(t.Status)) ? String.Format("{0:X4}", (ushort)dicom[t.Status].Value) : SuccessStatus;
            }
            internal set
            {
                // message may be a command message with a status, or a data message without
                dicom.Set(t.ErrorComment, ushort.Parse(value.ToUpper(), System.Globalization.NumberStyles.AllowHexSpecifier));
            }
        }

        public static Message Success
        {
            get
            {
                return new Message();
            }
        }

        public static Message TimeOut
        {
            get
            {
                Message message = new Message();
                message.Status = TimeOutStatus;
                message.ErrorComment = TimeOutMessage;
                return message;
            }
        }
    }

    public class DicomException : Exception
    {
        private Message dicom;

        public DicomException(Message dicom) :
            base()
        {
            this.dicom = dicom;
        }

        public DicomException(Message dicom, string message) :
            base(message)
        {
            this.dicom = dicom;
        }

        public DicomException(Message dicom, string message, Exception inner) :
            base(message, inner)
        {
            this.dicom = dicom;
        }

        public Message Dicom
        {
            get
            {
                return dicom;
            }
            set
            {
                dicom = value;
            }
        }
    }

    public interface IPresentationDataSink
    {
        void OnData(MessageType control, Message message);
    }

}
