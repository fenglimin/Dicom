using System;
using System.Collections.Generic;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{

    /// <summary>
    /// Represents a Standard Dicom Tag stored with the internal Dictionary.
    /// </summary>
    public struct StandardTag
    {
        #region Fields

        /// <summary>
        /// The Dicom Tag as a string.
        /// </summary>
        private string tag;

        /// <summary>
        /// The Dicom Value Representation.
        /// </summary>
        private string vr;

        /// <summary>
        /// The Dicom Value Multiplicity.
        /// </summary>
        private string vm;

        /// <summary>
        /// The Dicom Tag Description.
        /// </summary>
        private string description;

        #endregion Fields

        #region Constructor

        /// <summary>
        /// Initializes a new DictionaryEntry instance with the specified properties.
        /// </summary>
        /// <param name="tag">The Dicom Tag.</param>
        /// <param name="vr">The Dicom Value Representation.</param>
        /// <param name="vm">The Duicom Value Multiplicity.</param>
        /// <param name="description">The Dicom Description.</param>
        public StandardTag(string tag, string vr, string vm, string description)
        {
            this.tag = tag;
            this.vr = vr;
            this.vm = vm;
            this.description = description;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// The Dicom Tag as a string.
        /// </summary>
        /// <exception cref="System.Exception">Illegal group and element combination or bad format.</exception>
        public string Tag
        {
            get
            {
                return tag;
            }
            set
            {
                Tag tag = EK.Capture.Dicom.DicomToolKit.Tag.Parse(value);
                this.tag = value;
            }
        }

        /// <summary>
        /// The Dicom Value Representation.
        /// </summary>
        public string Vr
        {
            get
            {
                return vr;
            }
            set
            {
                this.vr = value;
            }
        }

        /// <summary>
        /// The Dicom Value Multiplicity.
        /// </summary>
        public string Vm
        {
            get
            {
                return vm;
            }
            set
            {
                this.vm = value;
            }
        }

        /// <summary>
        /// The description of the Dicom Tag.
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.description = value;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3}", this.description, this.tag, this.vr,  this.vm);
        }

        #endregion Properties
    }
}
