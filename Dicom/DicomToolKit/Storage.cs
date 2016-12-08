using System;
using System.Collections.Generic;
using System.Net;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// An SCU service that can perform C-STORE requests for a given SOP class.
    /// </summary>
    public class StorageServiceSCU : ServiceClass, IPresentationDataSink
    {
        public StorageServiceSCU(string uid)
            : base(uid)
        {
        }

        public StorageServiceSCU(StorageServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            return new StorageServiceSCU(this);
        }

        /// <summary>
        /// Perform a C-STORE request over an established association
        /// </summary>
        /// <returns></returns>
        public bool Store(DataSet dicom)
        {
            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClassUId);
            command.Add(t.CommandField, (ushort)CommandType.C_STORE_RQ);
            command.Add(t.MessageId, 1);
            command.Add(t.Priority, (ushort)Priority.Medium);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            string uid = (string)dicom[t.SOPInstanceUID].Value;
            command.Add(t.AffectedSOPInstanceUID, uid);

            SendCommand("C-STORE-RQ", command);
            SendData("C-STORE-RQ DATA", dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("A7","Refused: Out of Resources");
            errors.Add("A9","Error: Data Set does not match SOP Class");
            errors.Add("C","Error: Cannot understand");

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B000","Warning Coercion of Data Elements");
            warnings.Add("B007","Data Set does not match SOP Class");
            warnings.Add("B006","Elements Discarded");

            ProcessResponse("C-STORE-RQ", 30000, errors, warnings);
            return true;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("StorageServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            ushort command = (ushort)dicom[t.CommandField].Value;
            if (command == (ushort)CommandType.C_STORE_RSP)
            {
                ushort status = (ushort)dicom[t.Status].Value;
                Logging.Log("<< C-STORE-RSP with status of {0:x4}", status);
            }
            else
            {
                Logging.Log(LogLevel.Error, "C-STORE received unexpected CommandField of {0:x4}", command);
            }
            completeEvent.Set();
        }
    }

    /// <summary>
    /// EventArgs sent with a ImageStoredEventHandler that contain the image.
    /// Set the Cancel property to True to reject the storage.
    /// </summary>
	public class ImageStoredEventArgs : DicomEventArgs
    {
        private DataSet dicom;
        private bool cancel;

        public ImageStoredEventArgs(DataSet dicom) 
        {
            this.cancel = false;
            this.dicom = dicom;
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

		public string CallingAeTitle { get; set; }
		public IPAddress CallingAeIpAddress { get; set; }
    }

    /// <summary>
    /// Called whenever a StorageServiceSCP instance has an image ready from a C-STORE request.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void ImageStoredEventHandler(object sender, ImageStoredEventArgs e);

    /// <summary>
    /// An SCP service that responds to C-STORE requests for a given SOP class.
    /// </summary>
    public class StorageServiceSCP : ServiceClass, IPresentationDataSink
    {
        string AffectedSOPInstanceUID;
        ushort MessageId;
        public event ImageStoredEventHandler ImageStored;

        public StorageServiceSCP(string uid)
            : base(uid)
        {
        }

        public StorageServiceSCP(StorageServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            StorageServiceSCP temp = new StorageServiceSCP(this);
            temp.ImageStored = this.ImageStored;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("StorageServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                AffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                dicom.Add(t.GroupLength(2), (ulong)0);
                dicom.Add(t.FileMetaInformationVersion, new byte[] { 0, 1 });
                dicom.Add(t.MediaStorageSOPClassUID, this.SOPClassUId);
                dicom.Add(t.MediaStorageSOPInstanceUID, this.AffectedSOPInstanceUID);
                dicom.Add(t.TransferSyntaxUID, this.syntaxes[0]);
                dicom.Add(t.ImplementationClassUID, "1.2.3.4");
                dicom.Add(t.ImplementationVersionName, "not specified");

                dicom.Part10Header = true;
                dicom.TransferSyntaxUID = syntaxes[0];

                int status = 0x0000;
                ImageStoredEventArgs image = new ImageStoredEventArgs(dicom);
				image.CallingAeTitle = association.CallingAeTitle;
	            image.CallingAeIpAddress = association.CallingAeIpAddress;

                if (ImageStored != null)
                {
                    try
                    {
                        ImageStored(this, image);
                        if (image.Cancel)
                        {
                            status = 0xC000;
                        }
                    }
                    catch (Exception ex)
                    {
                        status = 0xA700;
                        Logging.Log(LogLevel.Error, String.Format("Exception caught from ImageStored, {0}", ex.Message));
                    }
                }

#if DEBUG
                else
                {
                    string uid = (string)dicom[t.SOPInstanceUID].Value;
                    dicom.Write(uid + ".dcm");
                }
#endif

                DataSet response = new DataSet();

                response.Add(t.GroupLength(0), (uint)144); // the number is calculated later anyway
                response.Add(t.AffectedSOPClassUID, this.SOPClassUId);//
                response.Add(t.CommandField, (ushort)CommandType.C_STORE_RSP);//
                response.Add(t.MessageIdBeingRespondedTo, MessageId);
                response.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
                response.Add(t.Status, status);
                response.Add(t.AffectedSOPInstanceUID, AffectedSOPInstanceUID);

                SendCommand("C-STORE-RSP", response);
            }
        }
    }
}
