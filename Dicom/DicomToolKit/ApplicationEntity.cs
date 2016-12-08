using System;
using System.Net;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents a Dicom Application Entity, which is at one end of a DICOM conversation.
    /// </summary>
    public struct ApplicationEntity
    {
        #region Fields

        /// <summary>
        /// The Dicom AETitle.
        /// </summary>
        private string title;

        /// <summary>
        /// The TCP/IP address.
        /// </summary>
        private IPAddress address;

        /// <summary>
        /// The TCP/IP port.
        /// </summary>
        private int port;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Intializes a new ApplicaitonEntity instance with the specified properties and an adrress of Loopback.
        /// </summary>
        /// <param name="title">The AETitle.</param>
        /// <param name="port">The TCP/IP port.</param>
        public ApplicationEntity(string title, int port) :
            this(title, IPAddress.Loopback, port)
        {
        }

        /// <summary>
        /// Intializes a new ApplicaitonEntity instance with the specified properties.
        /// </summary>
        /// <param name="title">The AETitle.</param>
        /// <param name="address">The TCP/IP address.</param>
        /// <param name="port">The TCP/IP port.</param>
        public ApplicationEntity(string title, IPAddress address, int port)
        {
            this.title = title;
            this.address = address;
            this.port = port;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// The Dicom AETitle.
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                title = value;
            }
        }

        /// <summary>
        /// The TCP/IP address.
        /// </summary>
        public IPAddress Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
            }
        }

        /// <summary>
        /// The TCP/IP port.
        /// </summary>
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        #endregion Properties

        public override string ToString()
        {
            return String.Format("ApplicationEntity={0},{1}:{2}", title, address, port);
        }
    }
}
