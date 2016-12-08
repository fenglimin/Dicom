using System;
using System.Collections.Generic;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Contains event data associated with a MwlRecordsEventHandler.
    /// </summary>
    public class MwlEventArgs : EventArgs
    {

        #region Fields

        private RecordCollection records;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new PduDataEventArgs instance with the specified RecordCollection.
        /// </summary>
        /// <param name="records"></param>
        public MwlEventArgs(RecordCollection records)
        {
            this.records = records;
        }

        #endregion Constructor

        #region Properties

        public RecordCollection Records
        {
            get
            {
                return records;
            }
        }

        #endregion Properties
    }

    /// <summary>
    /// Handler called when Modality Worklist Records are received.
    /// </summary>
    /// <param name="sender">The source of the notification.</param>
    /// <param name="e">The event args containing the Mwl records.</param>
    public delegate void MwlEventHandler(object sender, MwlEventArgs e);

    /// <summary>
    /// Contains event data associated with a Pdata response.
    /// </summary>
    public class PduDataEventArgs : EventArgs
    {

        #region Fields

        private PresentationDataPdu pdu;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new PduDataEventArgs instance with the specified PresentationDataPdu.
        /// </summary>
        /// <param name="records"></param>
        internal PduDataEventArgs(PresentationDataPdu pdu)
        {
            this.pdu = pdu;
        }

        #endregion Constructor

        #region Properties

        internal PresentationDataPdu Pdu
        {
            get
            {
                return pdu;
            }
        }

        #endregion Properties
    }

    internal delegate void PduDataEventHandler(object sender, PduDataEventArgs e);

    /// <summary>
    /// Contains event data associated with a Pdata response.
    /// </summary>
    public class AssociateRequestEventArgs : EventArgs
    {

        #region Fields

        private AssociateRequestPdu pdu;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new PduDataEventArgs instance with the specified PresentationDataPdu.
        /// </summary>
        /// <param name="records"></param>
        internal AssociateRequestEventArgs(AssociateRequestPdu pdu)
        {
            this.pdu = pdu;
        }

        #endregion Constructor

        #region Properties

        internal AssociateRequestPdu Pdu
        {
            get
            {
                return pdu;
            }
        }

        #endregion Properties
    }

    internal delegate void AssociateRequestEventHandler(object sender, AssociateRequestEventArgs e);

}
