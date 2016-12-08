using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represnts a Dicom Tag.
    /// </summary>
    /// <remarks>Contains static methods to format and parse Dicom tag strings.</remarks>
    public class Tag
    {
        #region Fields

        /// <summary>
        /// Rpresetns the parent tag of this tag or null.
        /// </summary>
        private Tag parent = null;

        /// <summary>
        /// Represents the Dicom Group Number of a Dicom Tag.
        /// </summary>
        private short group;

        /// <summary>
        /// Represents the Dicom Element Number of a Dicom Tag.
        /// </summary>
        private short element;

        /// <summary>
        /// The Dicom Value Representation.
        /// </summary>
        private string vr = "UN";

        /// <summary>
        /// The Dicom Value Multiplicity.
        /// </summary>
        private string vm = "?";

        /// <summary>
        /// The Dicom Tag Description.
        /// </summary>
        private string description = "unknown";

        /// <summary>
        /// Whether or not this tag has been retired from the standard.
        /// </summary>
        private bool retired = false;

        /// <summary>
        /// Whether or not this tag contains protected health information
        /// </summary>
        private bool phi = false;

        public static string SearchPattern = @"\((?<group>[0-9a-fA-f]{4}),(?<element>[0-9a-fA-f]{4})\)(?<item>\d+)?";
        
        #endregion Fields

        #region Constructor

        public Tag(string key) : this(Tag.Parse(key))
        {
        }

        /// <summary>
        /// Initializes a new DictionaryEntry instance with the specified properties.
        /// </summary>
        /// <param name="group">The Dicom group number</param>
        /// <param name="element">The Dicom element number</param>
        /// <param name="vr">The Dicom Value Representation.</param>
        /// <param name="vm">The Dicom Value Multiplicity.</param>
        /// <param name="description">The Dicom Description.</param>
        /// <param name="retired">Whether or not the tag is retired form the standard.</param>
        public Tag(short group, short element, string vr, string vm, string description, bool retired, bool phi)
        {
            this.group = group;
            this.element = element;
            this.parent = null;
            this.vr = vr;
            this.vm = vm;
            this.description = description;
            this.retired = retired;
            this.phi = phi;
        }

        internal Tag()
        {
        }

        /// <summary>
        /// Initializes a new instance of the Tag class with the specified Dicom Group and 
        /// Element numbers.
        /// </summary>
        /// <param name="group">The Dicom Group Number.</param>
        /// <param name="element">The Dicom Element Number.</param>
        /// <exception cref="System.Exception">Illegal group and element combination.</exception>
        internal Tag(short group, short element)
        {
            this.group = group;
            this.element = element;
            this.parent = null;

            if (Dictionary.Contains(this.ToString()))
            {
                Tag temp = Dictionary.Instance[this.ToString()];
                this.vm = temp.VM;
                this.vr = temp.VR;
                this.description = temp.Description;
                this.phi = temp.phi;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="group"></param>
        /// <param name="element"></param>
        /// <param name="parent"></param>
        internal Tag(short group, short element, Tag parent) :
            this(group, element)
        {
            this.parent = parent;
        }

        internal Tag(Tag other)
        {
            this.group = other.group;
            this.element = other.element;
            this.description = other.description;
            this.retired = other.retired;
            this.phi = other.phi;
            if (other.parent != null)
                this.parent = new Tag(other.parent);
            else
                this.parent = null;
            this.vm = other.vm;
            this.vr = other.vr;
        }

        #endregion Constructor

        #region Properties

        /// <summary>
        /// Gets or Sets the Dicom Group Number this instance.
        /// </summary>
        /// <exception cref="System.Exception">Illegal group and element combination.</exception>
        public short Group
        {
            get
            {
                return group;
            }
            set
            {
                //AssertValid(value, element);
                this.group = value;
            }
        }

        /// <summary>
        /// Gets or Sets the Dicom Element Number this instance.
        /// </summary>
        /// <exception cref="System.Exception">Illegal group and element combination.</exception>
        public short Element
        {
            get
            {
                return element;
            }
            set
            {
                //AssertValid(group, value);
                this.element = value;
            }
        }

        /// <summary>
        /// Returns the Parent tag, if any
        /// </summary>
        public Tag Parent
        {
            get
            {
                return parent;
            }
            set
            {
                this.parent = value;
            }
        }

        /// <summary>
        /// The Dicom Value Representation.
        /// </summary>
        public string VR
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
        public string VM
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
        ///  Returns the (group,element) tag string of the tag.
        /// </summary>
        /// <remarks>If you want the tag string including any parent tags, use the <see cref="ToString"/> method</remarks>
        public string Name
        {
            get
            {
                return ToString(this.group, this.element);
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

        /// <summary>
        /// Whether or not this tag has been retired from the standard.
        /// </summary>
        public bool Retired
        {
            get
            {
                return retired;
            }
            set
            {
                this.retired = value;
            }
        }

        /// <summary>
        /// Whether or not this tag contains protected health information.
        /// </summary>
        public bool Protected
        {
            get
            {
                return phi;
            }
            set
            {
                this.phi = value;
            }
        }

        #endregion Properties

        #region ToString

        /// <summary>
        /// Returns the tag string, including the parent tags if this tag is within a sequence
        /// </summary>
        /// <returns></returns>
        /// <remarks>To get just the tag string without parent tags, use the <see cref="Name"/> property</remarks>
        public override string ToString()
        {
            string temp = (this.parent != null) ? this.parent.ToString() : String.Empty;
            // formatted in hexadecimal (xxxx,xxxx)
            return temp + String.Format("({0:x4},{1:x4})", this.group, this.element);
        }

        #endregion ToString

        #region Static Methods

        /// <summary>
        /// Splits a formatted Dicom tag string into individual Dicom Tags.
        /// </summary>
        /// <param name="key">The string to split.</param>
        /// <returns>An array of Tags.</returns>
        /// <exception cref="System.Exception">Illegal group and element combination or bad format.</exception>
        public static Tag[] Split(string key)
        {
            Tag[] tags = InternalParse(key);
            for (int n = 0; n < tags.Length; n++)
            {
                Tag temp = tags[n];
                tags[n] = new Tag(temp.Group, temp.Element);
            }
            return tags;
        }

        public static Tag Head(string key)
        {
            Tag[] tags = Split(key);
            return tags[0];
        }

        public static Tag Tail(string key)
        {
            Tag[] tags = Split(key);
            return tags[tags.Length - 1];
        }

        internal static Tag[] InternalParse(string key)
        {
            // match something like (0000,0000)(0000,0000)(0000,0000)
            Match match = Regex.Match(key.ToLower(), SearchPattern);

            if (!match.Success)
            {
                throw new Exception(String.Format("{0} is not a properly formatted dicom tag.", key));
            }

            // put each match into an array
            List<Tag> list = new List<Tag>();
            while (match.Success)
            {
                // we do not want to access the dictionary
                Tag temp = new Tag();
                temp.Group = short.Parse(match.Groups["group"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);
                temp.Element = short.Parse(match.Groups["element"].ToString(), System.Globalization.NumberStyles.AllowHexSpecifier);

                list.Add(temp);
                match = match.NextMatch();
            }

            return list.ToArray();
        }

        public static bool TryParse(string key)
        {
            // match something like (0000,0000)(0000,0000)(0000,0000)
            Match match = Regex.Match(key.ToLower(), SearchPattern);
            return match.Success;
        }

        /// <summary>
        /// Converts a formatted Dicom tag string into a Tag instance.
        /// </summary>
        /// <param name="key">The formatted Dicom tag string to parse.</param>
        /// <returns>A Tag instance.</returns>
        /// <exception cref="System.Exception">Illegal group and element combination or bad format.</exception>
        public static Tag Parse(string key)
        {
            if (key == null || key == String.Empty)
                return null;

            Tag[] tags = Split(key);

            if (tags.Length > 1)
            {
                for (int n = 1; n < tags.Length; n++)
                {
                    tags[n].parent = tags[n - 1]; 
                }
                return tags[tags.Length - 1];
            }
            return tags[0];
        }

        public static string ToString(Element element)
        {
            return ToString(element.Group, element.element);
        }

        public static string ToString(short group, short element)
        {
            // formatted in hexadecimal (xxxx,xxxx)
            return String.Format("({0:x4},{1:x4})", group, element);
        }

        #endregion Static Methods

        #region Private Methods

        /*
        /// <summary>
        /// Tests the validity of the group and element combination.
        /// </summary>
        /// <param name="group">The Dicom Group Number.</param>
        /// <param name="element">The Dicom Element Number.</param>
        /// <returns>Whether the group and element combination is valid or not.</returns>
        public static bool IsValid(short group, short element)
        {
            unchecked
            {
                if (element == (short)0xEEEE)
                {
                    if (group == 0x001 || group == 0x0003 || group == 0x0005 || group == 0x0007 ||
                        // TODO find out if the following groups are reserved for DIMSE commands
                        group == 0x000 || group == 0x0002 || group == 0x0004 || group == 0x0006 ||
                        group == (short)0xFFFF)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Used to insure that illegal group and element combinations are not formed.  An exception
        /// is thrown if the supplied combination is illegal or reserved.
        /// </summary>
        /// <param name="group">The Dicom Group Number.</param>
        /// <param name="element">The Dicom Element Number.</param>
        private static void AssertValid(short group, short element)
        {
            if (!IsValid(group, element))
            {
                throw new Exception("Illegal group and element combination.");
            }
        }
 */

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }
            Tag t = obj as Tag;
            if ((System.Object)t == null)
            {
                return false;
            }
            return (group == t.group) && (element == t.element);
        }

        public bool Equals(Tag t)
        {
            if ((object)t == null)
            {
                return false;
            }
            return (group == t.group) && (element == t.element);
        }

        public bool Equals(string tag)
        {
            return (tag.ToLower() == ToString(this.group, this.element));
        }

        public override int GetHashCode()
        {
            return (int)(((ushort)element) | (uint)(group << 16));
        }

        public bool IsGroupLength
        {
            get
            {
                return (element == 0);
            }
        }

        public bool IsStandard
        {
            get
            {
                return (group % 2 == 0);
            }
        }

        public bool IsPrivate
        {
            get
            {
                return (group % 2 == 1);
            }
        }

        public bool IsPrivateCreator
        {
            get
            {
                return IsPrivate && (element >= 0x10 && element <= 0xff);
            }
        }

        #endregion Private Methods
    }

    public class t
    {
        public static string GroupLength(short group)
        {
            return String.Format("({0:x4},0000)", group);
        }
        public const string AffectedSOPClassUID = "(0000,0002)";
        public const string RequestedSOPClassUID = "(0000,0003)";
        public const string CommandField = "(0000,0100)";
        public const string MessageId = "(0000,0110)";
        public const string MessageIdBeingRespondedTo = "(0000,0120)";
        public const string MoveDestination = "(0000,0600)";
        public const string Priority = "(0000,0700)";
        public const string CommandDataSetType = "(0000,0800)";
        public const string Status = "(0000,0900)";
        public const string OffendingElement = "(0000,0901)";
        public const string ErrorComment = "(0000,0902)";
        public const string ErrorID = "(0000,0903)";
        public const string AffectedSOPInstanceUID = "(0000,1000)";
        public const string RequestedSOPInstanceUID = "(0000,1001)";
        public const string EventTypeID = "(0000,1002)";
        public const string AttributeIdentifierList = "(0000,1005)";
        public const string ActionTypeID = "(0000,1008)";
        public const string NumberofRemainingSuboperations = "(0000,1020)";
        public const string NumberofCompletedSuboperations = "(0000,1021)";
        public const string NumberofFailedSuboperations = "(0000,1022)";
        public const string NumberofWarningSuboperations = "(0000,1023)";
        public const string MoveOriginator = "(0000,1030)";
        public const string MoveOriginatorMessageID = "(0000,1031)";
        public const string FileMetaInformationGroupLength = "(0002,0000)";
        public const string FileMetaInformationVersion = "(0002,0001)";
        public const string MediaStorageSOPClassUID = "(0002,0002)";
        public const string MediaStorageSOPInstanceUID = "(0002,0003)";
        public const string TransferSyntaxUID = "(0002,0010)";
        public const string ImplementationClassUID = "(0002,0012)";
        public const string ImplementationVersionName = "(0002,0013)";
        public const string SourceApplicationEntityTitle = "(0002,0016)";
        public const string PrivateInformationCreatorUID = "(0002,0100)";
        public const string PrivateInformation = "(0002,0102)";
        public const string FilesetID = "(0004,1130)";
        public const string FilesetDescriptorFileID = "(0004,1141)";
        public const string SpecificCharacterSetofFilesetDescriptorFile = "(0004,1142)";
        public const string OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity = "(0004,1200)";
        public const string OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity = "(0004,1202)";
        public const string FilesetConsistencyFlag = "(0004,1212)";
        public const string DirectoryRecordSequence = "(0004,1220)";
        public const string OffsetoftheNextDirectoryRecord = "(0004,1400)";
        public const string RecordInuseFlag = "(0004,1410)";
        public const string OffsetofReferencedLowerLevelDirectoryEntity = "(0004,1420)";
        public const string DirectoryRecordType = "(0004,1430)";
        public const string PrivateRecordUID = "(0004,1432)";
        public const string ReferencedFileID = "(0004,1500)";
        public const string MRDRDirectoryRecordOffset = "(0004,1504)";
        public const string ReferencedSOPClassUIDinFile = "(0004,1510)";
        public const string ReferencedSOPInstanceUIDinFile = "(0004,1511)";
        public const string ReferencedTransferSyntaxUIDinFile = "(0004,1512)";
        public const string ReferencedRelatedGeneralSOPClassUIDinFile = "(0004,151A)";
        public const string NumberofReferences = "(0004,1600)";
        public const string LengthtoEnd = "(0008,0001)";
        public const string SpecificCharacterSet = "(0008,0005)";
        public const string LanguageCodeSequence = "(0008,0006)";
        public const string ImageType = "(0008,0008)";
        public const string RecognitionCode = "(0008,0010)";
        public const string InstanceCreationDate = "(0008,0012)";
        public const string InstanceCreationTime = "(0008,0013)";
        public const string InstanceCreatorUID = "(0008,0014)";
        public const string SOPClassUID = "(0008,0016)";
        public const string SOPInstanceUID = "(0008,0018)";
        public const string RelatedGeneralSOPClassUID = "(0008,001A)";
        public const string OriginalSpecializedSOPClassUID = "(0008,001B)";
        public const string StudyDate = "(0008,0020)";
        public const string SeriesDate = "(0008,0021)";
        public const string AcquisitionDate = "(0008,0022)";
        public const string ContentDate = "(0008,0023)";
        public const string OverlayDate = "(0008,0024)";
        public const string CurveDate = "(0008,0025)";
        public const string AcquisitionDateTime = "(0008,002A)";
        public const string StudyTime = "(0008,0030)";
        public const string SeriesTime = "(0008,0031)";
        public const string AcquisitionTime = "(0008,0032)";
        public const string ContentTime = "(0008,0033)";
        public const string OverlayTime = "(0008,0034)";
        public const string CurveTime = "(0008,0035)";
        public const string DataSetType = "(0008,0040)";
        public const string DataSetSubtype = "(0008,0041)";
        public const string NuclearMedicineSeriesType = "(0008,0042)";
        public const string AccessionNumber = "(0008,0050)";
        public const string IssuerofAccessionNumberSequence = "(0008,0051)";
        public const string QueryRetrieveLevel = "(0008,0052)";
        public const string RetrieveAETitle = "(0008,0054)";
        public const string InstanceAvailability = "(0008,0056)";
        public const string FailedSOPInstanceUIDList = "(0008,0058)";
        public const string Modality = "(0008,0060)";
        public const string ModalitiesinStudy = "(0008,0061)";
        public const string SOPClassesinStudy = "(0008,0062)";
        public const string ConversionType = "(0008,0064)";
        public const string PresentationIntentType = "(0008,0068)";
        public const string Manufacturer = "(0008,0070)";
        public const string InstitutionName = "(0008,0080)";
        public const string InstitutionAddress = "(0008,0081)";
        public const string InstitutionCodeSequence = "(0008,0082)";
        public const string ReferringPhysicianName = "(0008,0090)";
        public const string ReferringPhysicianAddress = "(0008,0092)";
        public const string ReferringPhysicianTelephoneNumbers = "(0008,0094)";
        public const string ReferringPhysicianIdentificationSequence = "(0008,0096)";
        public const string CodeValue = "(0008,0100)";
        public const string CodingSchemeDesignator = "(0008,0102)";
        public const string CodingSchemeVersion = "(0008,0103)";
        public const string CodeMeaning = "(0008,0104)";
        public const string MappingResource = "(0008,0105)";
        public const string ContextGroupVersion = "(0008,0106)";
        public const string ContextGroupLocalVersion = "(0008,0107)";
        public const string ContextGroupExtensionFlag = "(0008,010B)";
        public const string CodingSchemeUID = "(0008,010C)";
        public const string ContextGroupExtensionCreatorUID = "(0008,010D)";
        public const string ContextIdentifier = "(0008,010F)";
        public const string CodingSchemeIdentificationSequence = "(0008,0110)";
        public const string CodingSchemeRegistry = "(0008,0112)";
        public const string CodingSchemeExternalID = "(0008,0114)";
        public const string CodingSchemeName = "(0008,0115)";
        public const string CodingSchemeResponsibleOrganization = "(0008,0116)";
        public const string ContextUID = "(0008,0117)";
        public const string TimezoneOffsetFromUTC = "(0008,0201)";
        public const string NetworkID = "(0008,1000)";
        public const string StationName = "(0008,1010)";
        public const string StudyDescription = "(0008,1030)";
        public const string ProcedureCodeSequence = "(0008,1032)";
        public const string SeriesDescription = "(0008,103E)";
        public const string SeriesDescriptionCodeSequence = "(0008,103F)";
        public const string InstitutionalDepartmentName = "(0008,1040)";
        public const string PhysiciansofRecord = "(0008,1048)";
        public const string PhysiciansofRecordIdentificationSequence = "(0008,1049)";
        public const string PerformingPhysicianName = "(0008,1050)";
        public const string PerformingPhysicianIdentificationSequence = "(0008,1052)";
        public const string NameofPhysiciansReadingStudy = "(0008,1060)";
        public const string PhysiciansReadingStudyIdentificationSequence = "(0008,1062)";
        public const string OperatorsName = "(0008,1070)";
        public const string OperatorIdentificationSequence = "(0008,1072)";
        public const string AdmittingDiagnosesDescription = "(0008,1080)";
        public const string AdmittingDiagnosesCodeSequence = "(0008,1084)";
        public const string ManufacturerModelName = "(0008,1090)";
        public const string ReferencedResultsSequence = "(0008,1100)";
        public const string ReferencedStudySequence = "(0008,1110)";
        public const string ReferencedPerformedProcedureStepSequence = "(0008,1111)";
        public const string ReferencedSeriesSequence = "(0008,1115)";
        public const string ReferencedPatientSequence = "(0008,1120)";
        public const string ReferencedVisitSequence = "(0008,1125)";
        public const string ReferencedOverlaySequence = "(0008,1130)";
        public const string ReferencedStereometricInstanceSequence = "(0008,1134)";
        public const string ReferencedWaveformSequence = "(0008,113A)";
        public const string ReferencedImageSequence = "(0008,1140)";
        public const string ReferencedCurveSequence = "(0008,1145)";
        public const string ReferencedInstanceSequence = "(0008,114A)";
        public const string ReferencedRealWorldValueMappingInstanceSequence = "(0008,114B)";
        public const string ReferencedSOPClassUID = "(0008,1150)";
        public const string ReferencedSOPInstanceUID = "(0008,1155)";
        public const string SOPClassesSupported = "(0008,115A)";
        public const string ReferencedFrameNumber = "(0008,1160)";
        public const string SimpleFrameList = "(0008,1161)";
        public const string CalculatedFrameList = "(0008,1162)";
        public const string TimeRange = "(0008,1163)";
        public const string FrameExtractionSequence = "(0008,1164)";
        public const string MultiFrameSourceSOPInstanceUID = "(0008,1167)";
        public const string TransactionUID = "(0008,1195)";
        public const string FailureReason = "(0008,1197)";
        public const string FailedSOPSequence = "(0008,1198)";
        public const string ReferencedSOPSequence = "(0008,1199)";
        public const string StudiesContainingOtherReferencedInstancesSequence = "(0008,1200)";
        public const string RelatedSeriesSequence = "(0008,1250)";
        public const string LossyImageCompressionRetired = "(0008,2110)";
        public const string DerivationDescription = "(0008,2111)";
        public const string SourceImageSequence = "(0008,2112)";
        public const string StageName = "(0008,2120)";
        public const string StageNumber = "(0008,2122)";
        public const string NumberofStages = "(0008,2124)";
        public const string ViewName = "(0008,2127)";
        public const string ViewNumber = "(0008,2128)";
        public const string NumberofEventTimers = "(0008,2129)";
        public const string NumberofViewsinStage = "(0008,212A)";
        public const string EventElapsedTimes = "(0008,2130)";
        public const string EventTimerNames = "(0008,2132)";
        public const string EventTimerSequence = "(0008,2133)";
        public const string EventTimeOffset = "(0008,2134)";
        public const string EventCodeSequence = "(0008,2135)";
        public const string StartTrim = "(0008,2142)";
        public const string StopTrim = "(0008,2143)";
        public const string RecommendedDisplayFrameRate = "(0008,2144)";
        public const string TransducerPosition = "(0008,2200)";
        public const string TransducerOrientation = "(0008,2204)";
        public const string AnatomicStructure = "(0008,2208)";
        public const string AnatomicRegionSequence = "(0008,2218)";
        public const string AnatomicRegionModifierSequence = "(0008,2220)";
        public const string PrimaryAnatomicStructureSequence = "(0008,2228)";
        public const string AnatomicStructureSpaceorRegionSequence = "(0008,2229)";
        public const string PrimaryAnatomicStructureModifierSequence = "(0008,2230)";
        public const string TransducerPositionSequence = "(0008,2240)";
        public const string TransducerPositionModifierSequence = "(0008,2242)";
        public const string TransducerOrientationSequence = "(0008,2244)";
        public const string TransducerOrientationModifierSequence = "(0008,2246)";
        public const string AnatomicStructureSpaceOrRegionCodeSequenceTrial = "(0008,2251)";
        public const string AnatomicPortalOfEntranceCodeSequenceTrial = "(0008,2253)";
        public const string AnatomicApproachDirectionCodeSequenceTrial = "(0008,2255)";
        public const string AnatomicPerspectiveDescriptionTrial = "(0008,2256)";
        public const string AnatomicPerspectiveCodeSequenceTrial = "(0008,2257)";
        public const string AnatomicLocationOfExaminingInstrumentDescriptionTrial = "(0008,2258)";
        public const string AnatomicLocationOfExaminingInstrumentCodeSequenceTrial = "(0008,2259)";
        public const string AnatomicStructureSpaceOrRegionModifierCodeSequenceTrial = "(0008,225A)";
        public const string OnAxisBackgroundAnatomicStructureCodeSequenceTrial = "(0008,225C)";
        public const string AlternateRepresentationSequence = "(0008,3001)";
        public const string IrradiationEventUID = "(0008,3010)";
        public const string IdentifyingComments = "(0008,4000)";
        public const string FrameType = "(0008,9007)";
        public const string ReferencedImageEvidenceSequence = "(0008,9092)";
        public const string ReferencedRawDataSequence = "(0008,9121)";
        public const string CreatorVersionUID = "(0008,9123)";
        public const string DerivationImageSequence = "(0008,9124)";
        public const string SourceImageEvidenceSequence = "(0008,9154)";
        public const string PixelPresentation = "(0008,9205)";
        public const string VolumetricProperties = "(0008,9206)";
        public const string VolumeBasedCalculationTechnique = "(0008,9207)";
        public const string ComplexImageComponent = "(0008,9208)";
        public const string AcquisitionContrast = "(0008,9209)";
        public const string DerivationCodeSequence = "(0008,9215)";
        public const string ReferencedPresentationStateSequence = "(0008,9237)";
        public const string ReferencedOtherPlaneSequence = "(0008,9410)";
        public const string FrameDisplaySequence = "(0008,9458)";
        public const string RecommendedDisplayFrameRateinFloat = "(0008,9459)";
        public const string SkipFrameRangeFlag = "(0008,9460)";
        public const string PatientName = "(0010,0010)";
        public const string PatientID = "(0010,0020)";
        public const string IssuerofPatientID = "(0010,0021)";
        public const string TypeofPatientID = "(0010,0022)";
        public const string IssuerofPatientIDQualifiersSequence = "(0010,0024)";
        public const string PatientBirthDate = "(0010,0030)";
        public const string PatientBirthTime = "(0010,0032)";
        public const string PatientSex = "(0010,0040)";
        public const string PatientInsurancePlanCodeSequence = "(0010,0050)";
        public const string PatientPrimaryLanguageCodeSequence = "(0010,0101)";
        public const string PatientPrimaryLanguageModifierCodeSequence = "(0010,0102)";
        public const string OtherPatientIDs = "(0010,1000)";
        public const string OtherPatientNames = "(0010,1001)";
        public const string OtherPatientIDsSequence = "(0010,1002)";
        public const string PatientBirthName = "(0010,1005)";
        public const string PatientAge = "(0010,1010)";
        public const string PatientSize = "(0010,1020)";
        public const string PatientSizeCodeSequence = "(0010,1021)";
        public const string PatientWeight = "(0010,1030)";
        public const string PatientAddress = "(0010,1040)";
        public const string InsurancePlanIdentification = "(0010,1050)";
        public const string PatientMotherBirthName = "(0010,1060)";
        public const string MilitaryRank = "(0010,1080)";
        public const string BranchofService = "(0010,1081)";
        public const string MedicalRecordLocator = "(0010,1090)";
        public const string MedicalAlerts = "(0010,2000)";
        public const string Allergies = "(0010,2110)";
        public const string CountryofResidence = "(0010,2150)";
        public const string RegionofResidence = "(0010,2152)";
        public const string PatientTelephoneNumbers = "(0010,2154)";
        public const string EthnicGroup = "(0010,2160)";
        public const string Occupation = "(0010,2180)";
        public const string SmokingStatus = "(0010,21A0)";
        public const string AdditionalPatientHistory = "(0010,21B0)";
        public const string PregnancyStatus = "(0010,21C0)";
        public const string LastMenstrualDate = "(0010,21D0)";
        public const string PatientReligiousPreference = "(0010,21F0)";
        public const string PatientSpeciesDescription = "(0010,2201)";
        public const string PatientSpeciesCodeSequence = "(0010,2202)";
        public const string PatientSexNeutered = "(0010,2203)";
        public const string AnatomicalOrientationType = "(0010,2210)";
        public const string PatientBreedDescription = "(0010,2292)";
        public const string PatientBreedCodeSequence = "(0010,2293)";
        public const string BreedRegistrationSequence = "(0010,2294)";
        public const string BreedRegistrationNumber = "(0010,2295)";
        public const string BreedRegistryCodeSequence = "(0010,2296)";
        public const string ResponsiblePerson = "(0010,2297)";
        public const string ResponsiblePersonRole = "(0010,2298)";
        public const string ResponsibleOrganization = "(0010,2299)";
        public const string PatientComments = "(0010,4000)";
        public const string ExaminedBodyThickness = "(0010,9431)";
        public const string ClinicalTrialSponsorName = "(0012,0010)";
        public const string ClinicalTrialProtocolID = "(0012,0020)";
        public const string ClinicalTrialProtocolName = "(0012,0021)";
        public const string ClinicalTrialSiteID = "(0012,0030)";
        public const string ClinicalTrialSiteName = "(0012,0031)";
        public const string ClinicalTrialSubjectID = "(0012,0040)";
        public const string ClinicalTrialSubjectReadingID = "(0012,0042)";
        public const string ClinicalTrialTimePointID = "(0012,0050)";
        public const string ClinicalTrialTimePointDescription = "(0012,0051)";
        public const string ClinicalTrialCoordinatingCenterName = "(0012,0060)";
        public const string PatientIdentityRemoved = "(0012,0062)";
        public const string DeidentificationMethod = "(0012,0063)";
        public const string DeidentificationMethodCodeSequence = "(0012,0064)";
        public const string ClinicalTrialSeriesID = "(0012,0071)";
        public const string ClinicalTrialSeriesDescription = "(0012,0072)";
        public const string ClinicalTrialProtocolEthicsCommitteeName = "(0012,0081)";
        public const string ClinicalTrialProtocolEthicsCommitteeApprovalNumber = "(0012,0082)";
        public const string ConsentforClinicalTrialUseSequence = "(0012,0083)";
        public const string DistributionType = "(0012,0084)";
        public const string ConsentforDistributionFlag = "(0012,0085)";
        public const string CADFileFormat = "(0014,0023)";
        public const string ComponentReferenceSystem = "(0014,0024)";
        public const string ComponentManufacturingProcedure = "(0014,0025)";
        public const string ComponentManufacturer = "(0014,0028)";
        public const string MaterialThickness = "(0014,0030)";
        public const string MaterialPipeDiameter = "(0014,0032)";
        public const string MaterialIsolationDiameter = "(0014,0034)";
        public const string MaterialGrade = "(0014,0042)";
        public const string MaterialPropertiesFileID = "(0014,0044)";
        public const string MaterialPropertiesFileFormat = "(0014,0045)";
        public const string MaterialNotes = "(0014,0046)";
        public const string ComponentShape = "(0014,0050)";
        public const string CurvatureType = "(0014,0052)";
        public const string OuterDiameter = "(0014,0054)";
        public const string InnerDiameter = "(0014,0056)";
        public const string ActualEnvironmentalConditions = "(0014,1010)";
        public const string ExpiryDate = "(0014,1020)";
        public const string EnvironmentalConditions = "(0014,1040)";
        public const string EvaluatorSequence = "(0014,2002)";
        public const string EvaluatorNumber = "(0014,2004)";
        public const string EvaluatorName = "(0014,2006)";
        public const string EvaluationAttempt = "(0014,2008)";
        public const string IndicationSequence = "(0014,2012)";
        public const string IndicationNumber = "(0014,2014)";
        public const string IndicationLabel = "(0014,2016)";
        public const string IndicationDescription = "(0014,2018)";
        public const string IndicationType = "(0014,201A)";
        public const string IndicationDisposition = "(0014,201C)";
        public const string IndicationROISequence = "(0014,201E)";
        public const string IndicationPhysicalPropertySequence = "(0014,2030)";
        public const string PropertyLabel = "(0014,2032)";
        public const string CoordinateSystemNumberofAxes = "(0014,2202)";
        public const string CoordinateSystemAxesSequence = "(0014,2204)";
        public const string CoordinateSystemAxisDescription = "(0014,2206)";
        public const string CoordinateSystemDataSetMapping = "(0014,2208)";
        public const string CoordinateSystemAxisNumber = "(0014,220A)";
        public const string CoordinateSystemAxisType = "(0014,220C)";
        public const string CoordinateSystemAxisUnits = "(0014,220E)";
        public const string CoordinateSystemAxisValues = "(0014,2210)";
        public const string CoordinateSystemTransformSequence = "(0014,2220)";
        public const string TransformDescription = "(0014,2222)";
        public const string TransformNumberofAxes = "(0014,2224)";
        public const string TransformOrderofAxes = "(0014,2226)";
        public const string TransformedAxisUnits = "(0014,2228)";
        public const string CoordinateSystemTransformRotationandScaleMatrix = "(0014,222A)";
        public const string CoordinateSystemTransformTranslationMatrix = "(0014,222C)";
        public const string InternalDetectorFrameTime = "(0014,3011)";
        public const string NumberofFramesIntegrated = "(0014,3012)";
        public const string DetectorTemperatureSequence = "(0014,3020)";
        public const string SensorName = "(0014,3022)";
        public const string HorizontalOffsetofSensor = "(0014,3024)";
        public const string VerticalOffsetofSensor = "(0014,3026)";
        public const string SensorTemperature = "(0014,3028)";
        public const string DarkCurrentSequence = "(0014,3040)";
        public const string DarkCurrentCounts = "(0014,3050)";
        public const string GainCorrectionReferenceSequence = "(0014,3060)";
        public const string AirCounts = "(0014,3070)";
        public const string KVUsedinGainCalibration = "(0014,3071)";
        public const string MAUsedinGainCalibration = "(0014,3072)";
        public const string NumberofFramesUsedforIntegration = "(0014,3073)";
        public const string FilterMaterialUsedinGainCalibration = "(0014,3074)";
        public const string FilterThicknessUsedinGainCalibration = "(0014,3075)";
        public const string DateofGainCalibration = "(0014,3076)";
        public const string TimeofGainCalibration = "(0014,3077)";
        public const string BadPixelImage = "(0014,3080)";
        public const string CalibrationNotes = "(0014,3099)";
        public const string PulserEquipmentSequence = "(0014,4002)";
        public const string PulserType = "(0014,4004)";
        public const string PulserNotes = "(0014,4006)";
        public const string ReceiverEquipmentSequence = "(0014,4008)";
        public const string AmplifierType = "(0014,400A)";
        public const string ReceiverNotes = "(0014,400C)";
        public const string PreAmplifierEquipmentSequence = "(0014,400E)";
        public const string PreAmplifierNotes = "(0014,400F)";
        public const string TransmitTransducerSequence = "(0014,4010)";
        public const string ReceiveTransducerSequence = "(0014,4011)";
        public const string NumberofElements = "(0014,4012)";
        public const string ElementShape = "(0014,4013)";
        public const string ElementDimensionA = "(0014,4014)";
        public const string ElementDimensionB = "(0014,4015)";
        public const string ElementPitch = "(0014,4016)";
        public const string MeasuredBeamDimensionA = "(0014,4017)";
        public const string MeasuredBeamDimensionB = "(0014,4018)";
        public const string LocationofMeasuredBeamDiameter = "(0014,4019)";
        public const string NominalFrequency = "(0014,401A)";
        public const string MeasuredCenterFrequency = "(0014,401B)";
        public const string MeasuredBandwidth = "(0014,401C)";
        public const string PulserSettingsSequence = "(0014,4020)";
        public const string PulseWidth = "(0014,4022)";
        public const string ExcitationFrequency = "(0014,4024)";
        public const string ModulationType = "(0014,4026)";
        public const string Damping = "(0014,4028)";
        public const string ReceiverSettingsSequence = "(0014,4030)";
        public const string AcquiredSoundpathLength = "(0014,4031)";
        public const string AcquisitionCompressionType = "(0014,4032)";
        public const string AcquisitionSampleSize = "(0014,4033)";
        public const string RectifierSmoothing = "(0014,4034)";
        public const string DACSequence = "(0014,4035)";
        public const string DACType = "(0014,4036)";
        public const string DACGainPoints = "(0014,4038)";
        public const string DACTimePoints = "(0014,403A)";
        public const string DACAmplitude = "(0014,403C)";
        public const string PreAmplifierSettingsSequence = "(0014,4040)";
        public const string TransmitTransducerSettingsSequence = "(0014,4050)";
        public const string ReceiveTransducerSettingsSequence = "(0014,4051)";
        public const string IncidentAngle = "(0014,4052)";
        public const string CouplingTechnique = "(0014,4054)";
        public const string CouplingMedium = "(0014,4056)";
        public const string CouplingVelocity = "(0014,4057)";
        public const string CrystalCenterLocationX = "(0014,4058)";
        public const string CrystalCenterLocationZ = "(0014,4059)";
        public const string SoundPathLength = "(0014,405A)";
        public const string DelayLawIdentifier = "(0014,405C)";
        public const string GateSettingsSequence = "(0014,4060)";
        public const string GateThreshold = "(0014,4062)";
        public const string VelocityofSound = "(0014,4064)";
        public const string CalibrationSettingsSequence = "(0014,4070)";
        public const string CalibrationProcedure = "(0014,4072)";
        public const string ProcedureVersion = "(0014,4074)";
        public const string ProcedureCreationDate = "(0014,4076)";
        public const string ProcedureExpirationDate = "(0014,4078)";
        public const string ProcedureLastModifiedDate = "(0014,407A)";
        public const string CalibrationTime = "(0014,407C)";
        public const string CalibrationDate = "(0014,407E)";
        public const string LINACEnergy = "(0014,5002)";
        public const string LINACOutput = "(0014,5004)";
        public const string ContrastBolusAgent = "(0018,0010)";
        public const string ContrastBolusAgentSequence = "(0018,0012)";
        public const string ContrastBolusAdministrationRouteSequence = "(0018,0014)";
        public const string BodyPartExamined = "(0018,0015)";
        public const string ScanningSequence = "(0018,0020)";
        public const string SequenceVariant = "(0018,0021)";
        public const string ScanOptions = "(0018,0022)";
        public const string MRAcquisitionType = "(0018,0023)";
        public const string SequenceName = "(0018,0024)";
        public const string AngioFlag = "(0018,0025)";
        public const string InterventionDrugInformationSequence = "(0018,0026)";
        public const string InterventionDrugStopTime = "(0018,0027)";
        public const string InterventionDrugDose = "(0018,0028)";
        public const string InterventionDrugCodeSequence = "(0018,0029)";
        public const string AdditionalDrugSequence = "(0018,002A)";
        public const string Radionuclide = "(0018,0030)";
        public const string Radiopharmaceutical = "(0018,0031)";
        public const string EnergyWindowCenterline = "(0018,0032)";
        public const string EnergyWindowTotalWidth = "(0018,0033)";
        public const string InterventionDrugName = "(0018,0034)";
        public const string InterventionDrugStartTime = "(0018,0035)";
        public const string InterventionSequence = "(0018,0036)";
        public const string TherapyType = "(0018,0037)";
        public const string InterventionStatus = "(0018,0038)";
        public const string TherapyDescription = "(0018,0039)";
        public const string InterventionDescription = "(0018,003A)";
        public const string CineRate = "(0018,0040)";
        public const string InitialCineRunState = "(0018,0042)";
        public const string SliceThickness = "(0018,0050)";
        public const string KVP = "(0018,0060)";
        public const string CountsAccumulated = "(0018,0070)";
        public const string AcquisitionTerminationCondition = "(0018,0071)";
        public const string EffectiveDuration = "(0018,0072)";
        public const string AcquisitionStartCondition = "(0018,0073)";
        public const string AcquisitionStartConditionData = "(0018,0074)";
        public const string AcquisitionTerminationConditionData = "(0018,0075)";
        public const string RepetitionTime = "(0018,0080)";
        public const string EchoTime = "(0018,0081)";
        public const string InversionTime = "(0018,0082)";
        public const string NumberofAverages = "(0018,0083)";
        public const string ImagingFrequency = "(0018,0084)";
        public const string ImagedNucleus = "(0018,0085)";
        public const string EchoNumbers = "(0018,0086)";
        public const string MagneticFieldStrength = "(0018,0087)";
        public const string SpacingBetweenSlices = "(0018,0088)";
        public const string NumberofPhaseEncodingSteps = "(0018,0089)";
        public const string DataCollectionDiameter = "(0018,0090)";
        public const string EchoTrainLength = "(0018,0091)";
        public const string PercentSampling = "(0018,0093)";
        public const string PercentPhaseFieldofView = "(0018,0094)";
        public const string PixelBandwidth = "(0018,0095)";
        public const string DeviceSerialNumber = "(0018,1000)";
        public const string DeviceUID = "(0018,1002)";
        public const string DeviceID = "(0018,1003)";
        public const string PlateID = "(0018,1004)";
        public const string GeneratorID = "(0018,1005)";
        public const string GridID = "(0018,1006)";
        public const string CassetteID = "(0018,1007)";
        public const string GantryID = "(0018,1008)";
        public const string SecondaryCaptureDeviceID = "(0018,1010)";
        public const string HardcopyCreationDeviceID = "(0018,1011)";
        public const string DateofSecondaryCapture = "(0018,1012)";
        public const string TimeofSecondaryCapture = "(0018,1014)";
        public const string SecondaryCaptureDeviceManufacturer = "(0018,1016)";
        public const string HardcopyDeviceManufacturer = "(0018,1017)";
        public const string SecondaryCaptureDeviceManufacturerModelName = "(0018,1018)";
        public const string SecondaryCaptureDeviceSoftwareVersions = "(0018,1019)";
        public const string HardcopyDeviceSoftwareVersion = "(0018,101A)";
        public const string HardcopyDeviceManufacturerModelName = "(0018,101B)";
        public const string SoftwareVersions = "(0018,1020)";
        public const string VideoImageFormatAcquired = "(0018,1022)";
        public const string DigitalImageFormatAcquired = "(0018,1023)";
        public const string ProtocolName = "(0018,1030)";
        public const string ContrastBolusRoute = "(0018,1040)";
        public const string ContrastBolusVolume = "(0018,1041)";
        public const string ContrastBolusStartTime = "(0018,1042)";
        public const string ContrastBolusStopTime = "(0018,1043)";
        public const string ContrastBolusTotalDose = "(0018,1044)";
        public const string SyringeCounts = "(0018,1045)";
        public const string ContrastFlowRate = "(0018,1046)";
        public const string ContrastFlowDuration = "(0018,1047)";
        public const string ContrastBolusIngredient = "(0018,1048)";
        public const string ContrastBolusIngredientConcentration = "(0018,1049)";
        public const string SpatialResolution = "(0018,1050)";
        public const string TriggerTime = "(0018,1060)";
        public const string TriggerSourceorType = "(0018,1061)";
        public const string NominalInterval = "(0018,1062)";
        public const string FrameTime = "(0018,1063)";
        public const string CardiacFramingType = "(0018,1064)";
        public const string FrameTimeVector = "(0018,1065)";
        public const string FrameDelay = "(0018,1066)";
        public const string ImageTriggerDelay = "(0018,1067)";
        public const string MultiplexGroupTimeOffset = "(0018,1068)";
        public const string TriggerTimeOffset = "(0018,1069)";
        public const string SynchronizationTrigger = "(0018,106A)";
        public const string SynchronizationChannel = "(0018,106C)";
        public const string TriggerSamplePosition = "(0018,106E)";
        public const string RadiopharmaceuticalRoute = "(0018,1070)";
        public const string RadiopharmaceuticalVolume = "(0018,1071)";
        public const string RadiopharmaceuticalStartTime = "(0018,1072)";
        public const string RadiopharmaceuticalStopTime = "(0018,1073)";
        public const string RadionuclideTotalDose = "(0018,1074)";
        public const string RadionuclideHalfLife = "(0018,1075)";
        public const string RadionuclidePositronFraction = "(0018,1076)";
        public const string RadiopharmaceuticalSpecificActivity = "(0018,1077)";
        public const string RadiopharmaceuticalStartDateTime = "(0018,1078)";
        public const string RadiopharmaceuticalStopDateTime = "(0018,1079)";
        public const string BeatRejectionFlag = "(0018,1080)";
        public const string LowRRValue = "(0018,1081)";
        public const string HighRRValue = "(0018,1082)";
        public const string IntervalsAcquired = "(0018,1083)";
        public const string IntervalsRejected = "(0018,1084)";
        public const string PVCRejection = "(0018,1085)";
        public const string SkipBeats = "(0018,1086)";
        public const string HeartRate = "(0018,1088)";
        public const string CardiacNumberofImages = "(0018,1090)";
        public const string TriggerWindow = "(0018,1094)";
        public const string ReconstructionDiameter = "(0018,1100)";
        public const string DistanceSourcetoDetector = "(0018,1110)";
        public const string DistanceSourcetoPatient = "(0018,1111)";
        public const string EstimatedRadiographicMagnificationFactor = "(0018,1114)";
        public const string GantryDetectorTilt = "(0018,1120)";
        public const string GantryDetectorSlew = "(0018,1121)";
        public const string TableHeight = "(0018,1130)";
        public const string TableTraverse = "(0018,1131)";
        public const string TableMotion = "(0018,1134)";
        public const string TableVerticalIncrement = "(0018,1135)";
        public const string TableLateralIncrement = "(0018,1136)";
        public const string TableLongitudinalIncrement = "(0018,1137)";
        public const string TableAngle = "(0018,1138)";
        public const string TableType = "(0018,113A)";
        public const string RotationDirection = "(0018,1140)";
        public const string AngularPosition = "(0018,1141)";
        public const string RadialPosition = "(0018,1142)";
        public const string ScanArc = "(0018,1143)";
        public const string AngularStep = "(0018,1144)";
        public const string CenterofRotationOffset = "(0018,1145)";
        public const string RotationOffset = "(0018,1146)";
        public const string FieldofViewShape = "(0018,1147)";
        public const string FieldofViewDimensions = "(0018,1149)";
        public const string ExposureTime = "(0018,1150)";
        public const string XRayTubeCurrent = "(0018,1151)";
        public const string Exposure = "(0018,1152)";
        public const string ExposureinuAs = "(0018,1153)";
        public const string AveragePulseWidth = "(0018,1154)";
        public const string RadiationSetting = "(0018,1155)";
        public const string RectificationType = "(0018,1156)";
        public const string RadiationMode = "(0018,115A)";
        public const string ImageandFluoroscopyAreaDoseProduct = "(0018,115E)";
        public const string FilterType = "(0018,1160)";
        public const string TypeofFilters = "(0018,1161)";
        public const string IntensifierSize = "(0018,1162)";
        public const string ImagerPixelSpacing = "(0018,1164)";
        public const string Grid = "(0018,1166)";
        public const string GeneratorPower = "(0018,1170)";
        public const string CollimatorgridName = "(0018,1180)";
        public const string CollimatorType = "(0018,1181)";
        public const string FocalDistance = "(0018,1182)";
        public const string XFocusCenter = "(0018,1183)";
        public const string YFocusCenter = "(0018,1184)";
        public const string FocalSpots = "(0018,1190)";
        public const string AnodeTargetMaterial = "(0018,1191)";
        public const string BodyPartThickness = "(0018,11A0)";
        public const string CompressionForce = "(0018,11A2)";
        public const string DateofLastCalibration = "(0018,1200)";
        public const string TimeofLastCalibration = "(0018,1201)";
        public const string ConvolutionKernel = "(0018,1210)";
        public const string UpperLowerPixelValues = "(0018,1240)";
        public const string ActualFrameDuration = "(0018,1242)";
        public const string CountRate = "(0018,1243)";
        public const string PreferredPlaybackSequencing = "(0018,1244)";
        public const string ReceiveCoilName = "(0018,1250)";
        public const string TransmitCoilName = "(0018,1251)";
        public const string PlateType = "(0018,1260)";
        public const string PhosphorType = "(0018,1261)";
        public const string ScanVelocity = "(0018,1300)";
        public const string WholeBodyTechnique = "(0018,1301)";
        public const string ScanLength = "(0018,1302)";
        public const string AcquisitionMatrix = "(0018,1310)";
        public const string InplanePhaseEncodingDirection = "(0018,1312)";
        public const string FlipAngle = "(0018,1314)";
        public const string VariableFlipAngleFlag = "(0018,1315)";
        public const string SAR = "(0018,1316)";
        public const string dBdt = "(0018,1318)";
        public const string AcquisitionDeviceProcessingDescription = "(0018,1400)";
        public const string AcquisitionDeviceProcessingCode = "(0018,1401)";
        public const string CassetteOrientation = "(0018,1402)";
        public const string CassetteSize = "(0018,1403)";
        public const string ExposuresonPlate = "(0018,1404)";
        public const string RelativeXRayExposure = "(0018,1405)";
        public const string ExposureIndex = "(0018,1411)";
        public const string TargetExposureIndex = "(0018,1412)";
        public const string DeviationIndex = "(0018,1413)";
        public const string ColumnAngulation = "(0018,1450)";
        public const string TomoLayerHeight = "(0018,1460)";
        public const string TomoAngle = "(0018,1470)";
        public const string TomoTime = "(0018,1480)";
        public const string TomoType = "(0018,1490)";
        public const string TomoClass = "(0018,1491)";
        public const string NumberofTomosynthesisSourceImages = "(0018,1495)";
        public const string PositionerMotion = "(0018,1500)";
        public const string PositionerType = "(0018,1508)";
        public const string PositionerPrimaryAngle = "(0018,1510)";
        public const string PositionerSecondaryAngle = "(0018,1511)";
        public const string PositionerPrimaryAngleIncrement = "(0018,1520)";
        public const string PositionerSecondaryAngleIncrement = "(0018,1521)";
        public const string DetectorPrimaryAngle = "(0018,1530)";
        public const string DetectorSecondaryAngle = "(0018,1531)";
        public const string ShutterShape = "(0018,1600)";
        public const string ShutterLeftVerticalEdge = "(0018,1602)";
        public const string ShutterRightVerticalEdge = "(0018,1604)";
        public const string ShutterUpperHorizontalEdge = "(0018,1606)";
        public const string ShutterLowerHorizontalEdge = "(0018,1608)";
        public const string CenterofCircularShutter = "(0018,1610)";
        public const string RadiusofCircularShutter = "(0018,1612)";
        public const string VerticesofthePolygonalShutter = "(0018,1620)";
        public const string ShutterPresentationValue = "(0018,1622)";
        public const string ShutterOverlayGroup = "(0018,1623)";
        public const string ShutterPresentationColorCIELabValue = "(0018,1624)";
        public const string CollimatorShape = "(0018,1700)";
        public const string CollimatorLeftVerticalEdge = "(0018,1702)";
        public const string CollimatorRightVerticalEdge = "(0018,1704)";
        public const string CollimatorUpperHorizontalEdge = "(0018,1706)";
        public const string CollimatorLowerHorizontalEdge = "(0018,1708)";
        public const string CenterofCircularCollimator = "(0018,1710)";
        public const string RadiusofCircularCollimator = "(0018,1712)";
        public const string VerticesofthePolygonalCollimator = "(0018,1720)";
        public const string AcquisitionTimeSynchronized = "(0018,1800)";
        public const string TimeSource = "(0018,1801)";
        public const string TimeDistributionProtocol = "(0018,1802)";
        public const string NTPSourceAddress = "(0018,1803)";
        public const string PageNumberVector = "(0018,2001)";
        public const string FrameLabelVector = "(0018,2002)";
        public const string FramePrimaryAngleVector = "(0018,2003)";
        public const string FrameSecondaryAngleVector = "(0018,2004)";
        public const string SliceLocationVector = "(0018,2005)";
        public const string DisplayWindowLabelVector = "(0018,2006)";
        public const string NominalScannedPixelSpacing = "(0018,2010)";
        public const string DigitizingDeviceTransportDirection = "(0018,2020)";
        public const string RotationofScannedFilm = "(0018,2030)";
        public const string IVUSAcquisition = "(0018,3100)";
        public const string IVUSPullbackRate = "(0018,3101)";
        public const string IVUSGatedRate = "(0018,3102)";
        public const string IVUSPullbackStartFrameNumber = "(0018,3103)";
        public const string IVUSPullbackStopFrameNumber = "(0018,3104)";
        public const string LesionNumber = "(0018,3105)";
        public const string AcquisitionComments = "(0018,4000)";
        public const string OutputPower = "(0018,5000)";
        public const string TransducerData = "(0018,5010)";
        public const string FocusDepth = "(0018,5012)";
        public const string ProcessingFunction = "(0018,5020)";
        public const string PostprocessingFunction = "(0018,5021)";
        public const string MechanicalIndex = "(0018,5022)";
        public const string BoneThermalIndex = "(0018,5024)";
        public const string CranialThermalIndex = "(0018,5026)";
        public const string SoftTissueThermalIndex = "(0018,5027)";
        public const string SoftTissuefocusThermalIndex = "(0018,5028)";
        public const string SoftTissuesurfaceThermalIndex = "(0018,5029)";
        public const string DynamicRange = "(0018,5030)";
        public const string TotalGain = "(0018,5040)";
        public const string DepthofScanField = "(0018,5050)";
        public const string PatientPosition = "(0018,5100)";
        public const string ViewPosition = "(0018,5101)";
        public const string ProjectionEponymousNameCodeSequence = "(0018,5104)";
        public const string ImageTransformationMatrix = "(0018,5210)";
        public const string ImageTranslationVector = "(0018,5212)";
        public const string Sensitivity = "(0018,6000)";
        public const string SequenceofUltrasoundRegions = "(0018,6011)";
        public const string RegionSpatialFormat = "(0018,6012)";
        public const string RegionDataType = "(0018,6014)";
        public const string RegionFlags = "(0018,6016)";
        public const string RegionLocationMinX0 = "(0018,6018)";
        public const string RegionLocationMinY0 = "(0018,601A)";
        public const string RegionLocationMaxX1 = "(0018,601C)";
        public const string RegionLocationMaxY1 = "(0018,601E)";
        public const string ReferencePixelX0 = "(0018,6020)";
        public const string ReferencePixelY0 = "(0018,6022)";
        public const string PhysicalUnitsXDirection = "(0018,6024)";
        public const string PhysicalUnitsYDirection = "(0018,6026)";
        public const string ReferencePixelPhysicalValueX = "(0018,6028)";
        public const string ReferencePixelPhysicalValueY = "(0018,602A)";
        public const string PhysicalDeltaX = "(0018,602C)";
        public const string PhysicalDeltaY = "(0018,602E)";
        public const string TransducerFrequency = "(0018,6030)";
        public const string TransducerType = "(0018,6031)";
        public const string PulseRepetitionFrequency = "(0018,6032)";
        public const string DopplerCorrectionAngle = "(0018,6034)";
        public const string SteeringAngle = "(0018,6036)";
        public const string DopplerSampleVolumeXPositionRetired = "(0018,6038)";
        public const string DopplerSampleVolumeXPosition = "(0018,6039)";
        public const string DopplerSampleVolumeYPositionRetired = "(0018,603A)";
        public const string DopplerSampleVolumeYPosition = "(0018,603B)";
        public const string TMLinePositionX0Retired = "(0018,603C)";
        public const string TMLinePositionX0 = "(0018,603D)";
        public const string TMLinePositionY0Retired = "(0018,603E)";
        public const string TMLinePositionY0 = "(0018,603F)";
        public const string TMLinePositionX1Retired = "(0018,6040)";
        public const string TMLinePositionX1 = "(0018,6041)";
        public const string TMLinePositionY1Retired = "(0018,6042)";
        public const string TMLinePositionY1 = "(0018,6043)";
        public const string PixelComponentOrganization = "(0018,6044)";
        public const string PixelComponentMask = "(0018,6046)";
        public const string PixelComponentRangeStart = "(0018,6048)";
        public const string PixelComponentRangeStop = "(0018,604A)";
        public const string PixelComponentPhysicalUnits = "(0018,604C)";
        public const string PixelComponentDataType = "(0018,604E)";
        public const string NumberofTableBreakPoints = "(0018,6050)";
        public const string TableofXBreakPoints = "(0018,6052)";
        public const string TableofYBreakPoints = "(0018,6054)";
        public const string NumberofTableEntries = "(0018,6056)";
        public const string TableofPixelValues = "(0018,6058)";
        public const string TableofParameterValues = "(0018,605A)";
        public const string RWaveTimeVector = "(0018,6060)";
        public const string DetectorConditionsNominalFlag = "(0018,7000)";
        public const string DetectorTemperature = "(0018,7001)";
        public const string DetectorType = "(0018,7004)";
        public const string DetectorConfiguration = "(0018,7005)";
        public const string DetectorDescription = "(0018,7006)";
        public const string DetectorMode = "(0018,7008)";
        public const string DetectorID = "(0018,700A)";
        public const string DateofLastDetectorCalibration = "(0018,700C)";
        public const string TimeofLastDetectorCalibration = "(0018,700E)";
        public const string ExposuresonDetectorSinceLastCalibration = "(0018,7010)";
        public const string ExposuresonDetectorSinceManufactured = "(0018,7011)";
        public const string DetectorTimeSinceLastExposure = "(0018,7012)";
        public const string DetectorActiveTime = "(0018,7014)";
        public const string DetectorActivationOffsetFromExposure = "(0018,7016)";
        public const string DetectorBinning = "(0018,701A)";
        public const string DetectorElementPhysicalSize = "(0018,7020)";
        public const string DetectorElementSpacing = "(0018,7022)";
        public const string DetectorActiveShape = "(0018,7024)";
        public const string DetectorActiveDimensions = "(0018,7026)";
        public const string DetectorActiveOrigin = "(0018,7028)";
        public const string DetectorManufacturerName = "(0018,702A)";
        public const string DetectorManufacturerModelName = "(0018,702B)";
        public const string FieldofViewOrigin = "(0018,7030)";
        public const string FieldofViewRotation = "(0018,7032)";
        public const string FieldofViewHorizontalFlip = "(0018,7034)";
        public const string PixelDataAreaOriginRelativeToFOV = "(0018,7036)";
        public const string PixelDataAreaRotationAngleRelativeToFOV = "(0018,7038)";
        public const string GridAbsorbingMaterial = "(0018,7040)";
        public const string GridSpacingMaterial = "(0018,7041)";
        public const string GridThickness = "(0018,7042)";
        public const string GridPitch = "(0018,7044)";
        public const string GridAspectRatio = "(0018,7046)";
        public const string GridPeriod = "(0018,7048)";
        public const string GridFocalDistance = "(0018,704C)";
        public const string FilterMaterial = "(0018,7050)";
        public const string FilterThicknessMinimum = "(0018,7052)";
        public const string FilterThicknessMaximum = "(0018,7054)";
        public const string FilterBeamPathLengthMinimum = "(0018,7056)";
        public const string FilterBeamPathLengthMaximum = "(0018,7058)";
        public const string ExposureControlMode = "(0018,7060)";
        public const string ExposureControlModeDescription = "(0018,7062)";
        public const string ExposureStatus = "(0018,7064)";
        public const string PhototimerSetting = "(0018,7065)";
        public const string ExposureTimeinuS = "(0018,8150)";
        public const string XRayTubeCurrentinuA = "(0018,8151)";
        public const string ContentQualification = "(0018,9004)";
        public const string PulseSequenceName = "(0018,9005)";
        public const string MRImagingModifierSequence = "(0018,9006)";
        public const string EchoPulseSequence = "(0018,9008)";
        public const string InversionRecovery = "(0018,9009)";
        public const string FlowCompensation = "(0018,9010)";
        public const string MultipleSpinEcho = "(0018,9011)";
        public const string MultiplanarExcitation = "(0018,9012)";
        public const string PhaseContrast = "(0018,9014)";
        public const string TimeofFlightContrast = "(0018,9015)";
        public const string Spoiling = "(0018,9016)";
        public const string SteadyStatePulseSequence = "(0018,9017)";
        public const string EchoPlanarPulseSequence = "(0018,9018)";
        public const string TagAngleFirstAxis = "(0018,9019)";
        public const string MagnetizationTransfer = "(0018,9020)";
        public const string T2Preparation = "(0018,9021)";
        public const string BloodSignalNulling = "(0018,9022)";
        public const string SaturationRecovery = "(0018,9024)";
        public const string SpectrallySelectedSuppression = "(0018,9025)";
        public const string SpectrallySelectedExcitation = "(0018,9026)";
        public const string SpatialPresaturation = "(0018,9027)";
        public const string Tagging = "(0018,9028)";
        public const string OversamplingPhase = "(0018,9029)";
        public const string TagSpacingFirstDimension = "(0018,9030)";
        public const string GeometryofkSpaceTraversal = "(0018,9032)";
        public const string SegmentedkSpaceTraversal = "(0018,9033)";
        public const string RectilinearPhaseEncodeReordering = "(0018,9034)";
        public const string TagThickness = "(0018,9035)";
        public const string PartialFourierDirection = "(0018,9036)";
        public const string CardiacSynchronizationTechnique = "(0018,9037)";
        public const string ReceiveCoilManufacturerName = "(0018,9041)";
        public const string MRReceiveCoilSequence = "(0018,9042)";
        public const string ReceiveCoilType = "(0018,9043)";
        public const string QuadratureReceiveCoil = "(0018,9044)";
        public const string MultiCoilDefinitionSequence = "(0018,9045)";
        public const string MultiCoilConfiguration = "(0018,9046)";
        public const string MultiCoilElementName = "(0018,9047)";
        public const string MultiCoilElementUsed = "(0018,9048)";
        public const string MRTransmitCoilSequence = "(0018,9049)";
        public const string TransmitCoilManufacturerName = "(0018,9050)";
        public const string TransmitCoilType = "(0018,9051)";
        public const string SpectralWidth = "(0018,9052)";
        public const string ChemicalShiftReference = "(0018,9053)";
        public const string VolumeLocalizationTechnique = "(0018,9054)";
        public const string MRAcquisitionFrequencyEncodingSteps = "(0018,9058)";
        public const string Decoupling = "(0018,9059)";
        public const string DecoupledNucleus = "(0018,9060)";
        public const string DecouplingFrequency = "(0018,9061)";
        public const string DecouplingMethod = "(0018,9062)";
        public const string DecouplingChemicalShiftReference = "(0018,9063)";
        public const string kspaceFiltering = "(0018,9064)";
        public const string TimeDomainFiltering = "(0018,9065)";
        public const string NumberofZeroFills = "(0018,9066)";
        public const string BaselineCorrection = "(0018,9067)";
        public const string ParallelReductionFactorInplane = "(0018,9069)";
        public const string CardiacRRIntervalSpecified = "(0018,9070)";
        public const string AcquisitionDuration = "(0018,9073)";
        public const string FrameAcquisitionDateTime = "(0018,9074)";
        public const string DiffusionDirectionality = "(0018,9075)";
        public const string DiffusionGradientDirectionSequence = "(0018,9076)";
        public const string ParallelAcquisition = "(0018,9077)";
        public const string ParallelAcquisitionTechnique = "(0018,9078)";
        public const string InversionTimes = "(0018,9079)";
        public const string MetaboliteMapDescription = "(0018,9080)";
        public const string PartialFourier = "(0018,9081)";
        public const string EffectiveEchoTime = "(0018,9082)";
        public const string MetaboliteMapCodeSequence = "(0018,9083)";
        public const string ChemicalShiftSequence = "(0018,9084)";
        public const string CardiacSignalSource = "(0018,9085)";
        public const string Diffusionbvalue = "(0018,9087)";
        public const string DiffusionGradientOrientation = "(0018,9089)";
        public const string VelocityEncodingDirection = "(0018,9090)";
        public const string VelocityEncodingMinimumValue = "(0018,9091)";
        public const string VelocityEncodingAcquisitionSequence = "(0018,9092)";
        public const string NumberofkSpaceTrajectories = "(0018,9093)";
        public const string CoverageofkSpace = "(0018,9094)";
        public const string SpectroscopyAcquisitionPhaseRows = "(0018,9095)";
        public const string ParallelReductionFactorInplaneRetired = "(0018,9096)";
        public const string TransmitterFrequency = "(0018,9098)";
        public const string ResonantNucleus = "(0018,9100)";
        public const string FrequencyCorrection = "(0018,9101)";
        public const string MRSpectroscopyFOVGeometrySequence = "(0018,9103)";
        public const string SlabThickness = "(0018,9104)";
        public const string SlabOrientation = "(0018,9105)";
        public const string MidSlabPosition = "(0018,9106)";
        public const string MRSpatialSaturationSequence = "(0018,9107)";
        public const string MRTimingandRelatedParametersSequence = "(0018,9112)";
        public const string MREchoSequence = "(0018,9114)";
        public const string MRModifierSequence = "(0018,9115)";
        public const string MRDiffusionSequence = "(0018,9117)";
        public const string CardiacSynchronizationSequence = "(0018,9118)";
        public const string MRAveragesSequence = "(0018,9119)";
        public const string MRFOVGeometrySequence = "(0018,9125)";
        public const string VolumeLocalizationSequence = "(0018,9126)";
        public const string SpectroscopyAcquisitionDataColumns = "(0018,9127)";
        public const string DiffusionAnisotropyType = "(0018,9147)";
        public const string FrameReferenceDateTime = "(0018,9151)";
        public const string MRMetaboliteMapSequence = "(0018,9152)";
        public const string ParallelReductionFactoroutofplane = "(0018,9155)";
        public const string SpectroscopyAcquisitionOutofplanePhaseSteps = "(0018,9159)";
        public const string BulkMotionStatus = "(0018,9166)";
        public const string ParallelReductionFactorSecondInplane = "(0018,9168)";
        public const string CardiacBeatRejectionTechnique = "(0018,9169)";
        public const string RespiratoryMotionCompensationTechnique = "(0018,9170)";
        public const string RespiratorySignalSource = "(0018,9171)";
        public const string BulkMotionCompensationTechnique = "(0018,9172)";
        public const string BulkMotionSignalSource = "(0018,9173)";
        public const string ApplicableSafetyStandardAgency = "(0018,9174)";
        public const string ApplicableSafetyStandardDescription = "(0018,9175)";
        public const string OperatingModeSequence = "(0018,9176)";
        public const string OperatingModeType = "(0018,9177)";
        public const string OperatingMode = "(0018,9178)";
        public const string SpecificAbsorptionRateDefinition = "(0018,9179)";
        public const string GradientOutputType = "(0018,9180)";
        public const string SpecificAbsorptionRateValue = "(0018,9181)";
        public const string GradientOutput = "(0018,9182)";
        public const string FlowCompensationDirection = "(0018,9183)";
        public const string TaggingDelay = "(0018,9184)";
        public const string RespiratoryMotionCompensationTechniqueDescription = "(0018,9185)";
        public const string RespiratorySignalSourceID = "(0018,9186)";
        public const string ChemicalShiftMinimumIntegrationLimitinHz = "(0018,9195)";
        public const string ChemicalShiftMaximumIntegrationLimitinHz = "(0018,9196)";
        public const string MRVelocityEncodingSequence = "(0018,9197)";
        public const string FirstOrderPhaseCorrection = "(0018,9198)";
        public const string WaterReferencedPhaseCorrection = "(0018,9199)";
        public const string MRSpectroscopyAcquisitionType = "(0018,9200)";
        public const string RespiratoryCyclePosition = "(0018,9214)";
        public const string VelocityEncodingMaximumValue = "(0018,9217)";
        public const string TagSpacingSecondDimension = "(0018,9218)";
        public const string TagAngleSecondAxis = "(0018,9219)";
        public const string FrameAcquisitionDuration = "(0018,9220)";
        public const string MRImageFrameTypeSequence = "(0018,9226)";
        public const string MRSpectroscopyFrameTypeSequence = "(0018,9227)";
        public const string MRAcquisitionPhaseEncodingStepsinplane = "(0018,9231)";
        public const string MRAcquisitionPhaseEncodingStepsoutofplane = "(0018,9232)";
        public const string SpectroscopyAcquisitionPhaseColumns = "(0018,9234)";
        public const string CardiacCyclePosition = "(0018,9236)";
        public const string SpecificAbsorptionRateSequence = "(0018,9239)";
        public const string RFEchoTrainLength = "(0018,9240)";
        public const string GradientEchoTrainLength = "(0018,9241)";
        public const string ArterialSpinLabelingContrast = "(0018,9250)";
        public const string MRArterialSpinLabelingSequence = "(0018,9251)";
        public const string ASLTechniqueDescription = "(0018,9252)";
        public const string ASLSlabNumber = "(0018,9253)";
        public const string ASLSlabThickness = "(0018,9254)";
        public const string ASLSlabOrientation = "(0018,9255)";
        public const string ASLMidSlabPosition = "(0018,9256)";
        public const string ASLContext = "(0018,9257)";
        public const string ASLPulseTrainDuration = "(0018,9258)";
        public const string ASLCrusherFlag = "(0018,9259)";
        public const string ASLCrusherFlow = "(0018,925A)";
        public const string ASLCrusherDescription = "(0018,925B)";
        public const string ASLBolusCutoffFlag = "(0018,925C)";
        public const string ASLBolusCutoffTimingSequence = "(0018,925D)";
        public const string ASLBolusCutoffTechnique = "(0018,925E)";
        public const string ASLBolusCutoffDelayTime = "(0018,925F)";
        public const string ASLSlabSequence = "(0018,9260)";
        public const string ChemicalShiftMinimumIntegrationLimitinppm = "(0018,9295)";
        public const string ChemicalShiftMaximumIntegrationLimitinppm = "(0018,9296)";
        public const string CTAcquisitionTypeSequence = "(0018,9301)";
        public const string AcquisitionType = "(0018,9302)";
        public const string TubeAngle = "(0018,9303)";
        public const string CTAcquisitionDetailsSequence = "(0018,9304)";
        public const string RevolutionTime = "(0018,9305)";
        public const string SingleCollimationWidth = "(0018,9306)";
        public const string TotalCollimationWidth = "(0018,9307)";
        public const string CTTableDynamicsSequence = "(0018,9308)";
        public const string TableSpeed = "(0018,9309)";
        public const string TableFeedperRotation = "(0018,9310)";
        public const string SpiralPitchFactor = "(0018,9311)";
        public const string CTGeometrySequence = "(0018,9312)";
        public const string DataCollectionCenterPatient = "(0018,9313)";
        public const string CTReconstructionSequence = "(0018,9314)";
        public const string ReconstructionAlgorithm = "(0018,9315)";
        public const string ConvolutionKernelGroup = "(0018,9316)";
        public const string ReconstructionFieldofView = "(0018,9317)";
        public const string ReconstructionTargetCenterPatient = "(0018,9318)";
        public const string ReconstructionAngle = "(0018,9319)";
        public const string ImageFilter = "(0018,9320)";
        public const string CTExposureSequence = "(0018,9321)";
        public const string ReconstructionPixelSpacing = "(0018,9322)";
        public const string ExposureModulationType = "(0018,9323)";
        public const string EstimatedDoseSaving = "(0018,9324)";
        public const string CTXRayDetailsSequence = "(0018,9325)";
        public const string CTPositionSequence = "(0018,9326)";
        public const string TablePosition = "(0018,9327)";
        public const string ExposureTimeinms = "(0018,9328)";
        public const string CTImageFrameTypeSequence = "(0018,9329)";
        public const string XRayTubeCurrentinmA = "(0018,9330)";
        public const string ExposureinmAs = "(0018,9332)";
        public const string ConstantVolumeFlag = "(0018,9333)";
        public const string FluoroscopyFlag = "(0018,9334)";
        public const string DistanceSourcetoDataCollectionCenter = "(0018,9335)";
        public const string ContrastBolusAgentNumber = "(0018,9337)";
        public const string ContrastBolusIngredientCodeSequence = "(0018,9338)";
        public const string ContrastAdministrationProfileSequence = "(0018,9340)";
        public const string ContrastBolusUsageSequence = "(0018,9341)";
        public const string ContrastBolusAgentAdministered = "(0018,9342)";
        public const string ContrastBolusAgentDetected = "(0018,9343)";
        public const string ContrastBolusAgentPhase = "(0018,9344)";
        public const string CTDIvol = "(0018,9345)";
        public const string CTDIPhantomTypeCodeSequence = "(0018,9346)";
        public const string CalciumScoringMassFactorPatient = "(0018,9351)";
        public const string CalciumScoringMassFactorDevice = "(0018,9352)";
        public const string EnergyWeightingFactor = "(0018,9353)";
        public const string CTAdditionalXRaySourceSequence = "(0018,9360)";
        public const string ProjectionPixelCalibrationSequence = "(0018,9401)";
        public const string DistanceSourcetoIsocenter = "(0018,9402)";
        public const string DistanceObjecttoTableTop = "(0018,9403)";
        public const string ObjectPixelSpacinginCenterofBeam = "(0018,9404)";
        public const string PositionerPositionSequence = "(0018,9405)";
        public const string TablePositionSequence = "(0018,9406)";
        public const string CollimatorShapeSequence = "(0018,9407)";
        public const string PlanesinAcquisition = "(0018,9410)";
        public const string XAXRFFrameCharacteristicsSequence = "(0018,9412)";
        public const string FrameAcquisitionSequence = "(0018,9417)";
        public const string XRayReceptorType = "(0018,9420)";
        public const string AcquisitionProtocolName = "(0018,9423)";
        public const string AcquisitionProtocolDescription = "(0018,9424)";
        public const string ContrastBolusIngredientOpaque = "(0018,9425)";
        public const string DistanceReceptorPlanetoDetectorHousing = "(0018,9426)";
        public const string IntensifierActiveShape = "(0018,9427)";
        public const string IntensifierActiveDimensions = "(0018,9428)";
        public const string PhysicalDetectorSize = "(0018,9429)";
        public const string PositionofIsocenterProjection = "(0018,9430)";
        public const string FieldofViewSequence = "(0018,9432)";
        public const string FieldofViewDescription = "(0018,9433)";
        public const string ExposureControlSensingRegionsSequence = "(0018,9434)";
        public const string ExposureControlSensingRegionShape = "(0018,9435)";
        public const string ExposureControlSensingRegionLeftVerticalEdge = "(0018,9436)";
        public const string ExposureControlSensingRegionRightVerticalEdge = "(0018,9437)";
        public const string ExposureControlSensingRegionUpperHorizontalEdge = "(0018,9438)";
        public const string ExposureControlSensingRegionLowerHorizontalEdge = "(0018,9439)";
        public const string CenterofCircularExposureControlSensingRegion = "(0018,9440)";
        public const string RadiusofCircularExposureControlSensingRegion = "(0018,9441)";
        public const string VerticesofthePolygonalExposureControlSensingRegion = "(0018,9442)";
        public const string ColumnAngulationPatient = "(0018,9447)";
        public const string BeamAngle = "(0018,9449)";
        public const string FrameDetectorParametersSequence = "(0018,9451)";
        public const string CalculatedAnatomyThickness = "(0018,9452)";
        public const string CalibrationSequence = "(0018,9455)";
        public const string ObjectThicknessSequence = "(0018,9456)";
        public const string PlaneIdentification = "(0018,9457)";
        public const string FieldofViewDimensionsinFloat = "(0018,9461)";
        public const string IsocenterReferenceSystemSequence = "(0018,9462)";
        public const string PositionerIsocenterPrimaryAngle = "(0018,9463)";
        public const string PositionerIsocenterSecondaryAngle = "(0018,9464)";
        public const string PositionerIsocenterDetectorRotationAngle = "(0018,9465)";
        public const string TableXPositiontoIsocenter = "(0018,9466)";
        public const string TableYPositiontoIsocenter = "(0018,9467)";
        public const string TableZPositiontoIsocenter = "(0018,9468)";
        public const string TableHorizontalRotationAngle = "(0018,9469)";
        public const string TableHeadTiltAngle = "(0018,9470)";
        public const string TableCradleTiltAngle = "(0018,9471)";
        public const string FrameDisplayShutterSequence = "(0018,9472)";
        public const string AcquiredImageAreaDoseProduct = "(0018,9473)";
        public const string CarmPositionerTabletopRelationship = "(0018,9474)";
        public const string XRayGeometrySequence = "(0018,9476)";
        public const string IrradiationEventIdentificationSequence = "(0018,9477)";
        public const string XRayThreeDFrameTypeSequence = "(0018,9504)";
        public const string ContributingSourcesSequence = "(0018,9506)";
        public const string XRayThreeDAcquisitionSequence = "(0018,9507)";
        public const string PrimaryPositionerScanArc = "(0018,9508)";
        public const string SecondaryPositionerScanArc = "(0018,9509)";
        public const string PrimaryPositionerScanStartAngle = "(0018,9510)";
        public const string SecondaryPositionerScanStartAngle = "(0018,9511)";
        public const string PrimaryPositionerIncrement = "(0018,9514)";
        public const string SecondaryPositionerIncrement = "(0018,9515)";
        public const string StartAcquisitionDateTime = "(0018,9516)";
        public const string EndAcquisitionDateTime = "(0018,9517)";
        public const string ApplicationName = "(0018,9524)";
        public const string ApplicationVersion = "(0018,9525)";
        public const string ApplicationManufacturer = "(0018,9526)";
        public const string AlgorithmType = "(0018,9527)";
        public const string AlgorithmDescription = "(0018,9528)";
        public const string XRayThreeDReconstructionSequence = "(0018,9530)";
        public const string ReconstructionDescription = "(0018,9531)";
        public const string PerProjectionAcquisitionSequence = "(0018,9538)";
        public const string DiffusionbmatrixSequence = "(0018,9601)";
        public const string DiffusionbvalueXX = "(0018,9602)";
        public const string DiffusionbvalueXY = "(0018,9603)";
        public const string DiffusionbvalueXZ = "(0018,9604)";
        public const string DiffusionbvalueYY = "(0018,9605)";
        public const string DiffusionbvalueYZ = "(0018,9606)";
        public const string DiffusionbvalueZZ = "(0018,9607)";
        public const string DecayCorrectionDateTime = "(0018,9701)";
        public const string StartDensityThreshold = "(0018,9715)";
        public const string StartRelativeDensityDifferenceThreshold = "(0018,9716)";
        public const string StartCardiacTriggerCountThreshold = "(0018,9717)";
        public const string StartRespiratoryTriggerCountThreshold = "(0018,9718)";
        public const string TerminationCountsThreshold = "(0018,9719)";
        public const string TerminationDensityThreshold = "(0018,9720)";
        public const string TerminationRelativeDensityThreshold = "(0018,9721)";
        public const string TerminationTimeThreshold = "(0018,9722)";
        public const string TerminationCardiacTriggerCountThreshold = "(0018,9723)";
        public const string TerminationRespiratoryTriggerCountThreshold = "(0018,9724)";
        public const string DetectorGeometry = "(0018,9725)";
        public const string TransverseDetectorSeparation = "(0018,9726)";
        public const string AxialDetectorDimension = "(0018,9727)";
        public const string RadiopharmaceuticalAgentNumber = "(0018,9729)";
        public const string PETFrameAcquisitionSequence = "(0018,9732)";
        public const string PETDetectorMotionDetailsSequence = "(0018,9733)";
        public const string PETTableDynamicsSequence = "(0018,9734)";
        public const string PETPositionSequence = "(0018,9735)";
        public const string PETFrameCorrectionFactorsSequence = "(0018,9736)";
        public const string RadiopharmaceuticalUsageSequence = "(0018,9737)";
        public const string AttenuationCorrectionSource = "(0018,9738)";
        public const string NumberofIterations = "(0018,9739)";
        public const string NumberofSubsets = "(0018,9740)";
        public const string PETReconstructionSequence = "(0018,9749)";
        public const string PETFrameTypeSequence = "(0018,9751)";
        public const string TimeofFlightInformationUsed = "(0018,9755)";
        public const string ReconstructionType = "(0018,9756)";
        public const string DecayCorrected = "(0018,9758)";
        public const string AttenuationCorrected = "(0018,9759)";
        public const string ScatterCorrected = "(0018,9760)";
        public const string DeadTimeCorrected = "(0018,9761)";
        public const string GantryMotionCorrected = "(0018,9762)";
        public const string PatientMotionCorrected = "(0018,9763)";
        public const string CountLossNormalizationCorrected = "(0018,9764)";
        public const string RandomsCorrected = "(0018,9765)";
        public const string NonuniformRadialSamplingCorrected = "(0018,9766)";
        public const string SensitivityCalibrated = "(0018,9767)";
        public const string DetectorNormalizationCorrection = "(0018,9768)";
        public const string IterativeReconstructionMethod = "(0018,9769)";
        public const string AttenuationCorrectionTemporalRelationship = "(0018,9770)";
        public const string PatientPhysiologicalStateSequence = "(0018,9771)";
        public const string PatientPhysiologicalStateCodeSequence = "(0018,9772)";
        public const string DepthsofFocus = "(0018,9801)";
        public const string ExcludedIntervalsSequence = "(0018,9803)";
        public const string ExclusionStartDatetime = "(0018,9804)";
        public const string ExclusionDuration = "(0018,9805)";
        public const string USImageDescriptionSequence = "(0018,9806)";
        public const string ImageDataTypeSequence = "(0018,9807)";
        public const string DataType = "(0018,9808)";
        public const string TransducerScanPatternCodeSequence = "(0018,9809)";
        public const string AliasedDataType = "(0018,980B)";
        public const string PositionMeasuringDeviceUsed = "(0018,980C)";
        public const string TransducerGeometryCodeSequence = "(0018,980D)";
        public const string TransducerBeamSteeringCodeSequence = "(0018,980E)";
        public const string TransducerApplicationCodeSequence = "(0018,980F)";
        public const string ContributingEquipmentSequence = "(0018,A001)";
        public const string ContributionDateTime = "(0018,A002)";
        public const string ContributionDescription = "(0018,A003)";
        public const string StudyInstanceUID = "(0020,000D)";
        public const string SeriesInstanceUID = "(0020,000E)";
        public const string StudyID = "(0020,0010)";
        public const string SeriesNumber = "(0020,0011)";
        public const string AcquisitionNumber = "(0020,0012)";
        public const string InstanceNumber = "(0020,0013)";
        public const string IsotopeNumber = "(0020,0014)";
        public const string PhaseNumber = "(0020,0015)";
        public const string IntervalNumber = "(0020,0016)";
        public const string TimeSlotNumber = "(0020,0017)";
        public const string AngleNumber = "(0020,0018)";
        public const string ItemNumber = "(0020,0019)";
        public const string PatientOrientation = "(0020,0020)";
        public const string OverlayNumber = "(0020,0022)";
        public const string CurveNumber = "(0020,0024)";
        public const string LUTNumber = "(0020,0026)";
        public const string ImagePosition = "(0020,0030)";
        public const string ImagePositionPatient = "(0020,0032)";
        public const string ImageOrientation = "(0020,0035)";
        public const string ImageOrientationPatient = "(0020,0037)";
        public const string Location = "(0020,0050)";
        public const string FrameofReferenceUID = "(0020,0052)";
        public const string Laterality = "(0020,0060)";
        public const string ImageLaterality = "(0020,0062)";
        public const string ImageGeometryType = "(0020,0070)";
        public const string MaskingImage = "(0020,0080)";
        public const string ReportNumber = "(0020,00AA)";
        public const string TemporalPositionIdentifier = "(0020,0100)";
        public const string NumberofTemporalPositions = "(0020,0105)";
        public const string TemporalResolution = "(0020,0110)";
        public const string SynchronizationFrameofReferenceUID = "(0020,0200)";
        public const string SOPInstanceUIDofConcatenationSource = "(0020,0242)";
        public const string SeriesinStudy = "(0020,1000)";
        public const string AcquisitionsinSeries = "(0020,1001)";
        public const string ImagesinAcquisition = "(0020,1002)";
        public const string ImagesinSeries = "(0020,1003)";
        public const string AcquisitionsinStudy = "(0020,1004)";
        public const string ImagesinStudy = "(0020,1005)";
        public const string Reference = "(0020,1020)";
        public const string PositionReferenceIndicator = "(0020,1040)";
        public const string SliceLocation = "(0020,1041)";
        public const string OtherStudyNumbers = "(0020,1070)";
        public const string NumberofPatientRelatedStudies = "(0020,1200)";
        public const string NumberofPatientRelatedSeries = "(0020,1202)";
        public const string NumberofPatientRelatedInstances = "(0020,1204)";
        public const string NumberofStudyRelatedSeries = "(0020,1206)";
        public const string NumberofStudyRelatedInstances = "(0020,1208)";
        public const string NumberofSeriesRelatedInstances = "(0020,1209)";
        public const string ModifyingDeviceID = "(0020,3401)";
        public const string ModifiedImageID = "(0020,3402)";
        public const string ModifiedImageDate = "(0020,3403)";
        public const string ModifyingDeviceManufacturer = "(0020,3404)";
        public const string ModifiedImageTime = "(0020,3405)";
        public const string ModifiedImageDescription = "(0020,3406)";
        public const string ImageComments = "(0020,4000)";
        public const string OriginalImageIdentification = "(0020,5000)";
        public const string OriginalImageIdentificationNomenclature = "(0020,5002)";
        public const string StackID = "(0020,9056)";
        public const string InStackPositionNumber = "(0020,9057)";
        public const string FrameAnatomySequence = "(0020,9071)";
        public const string FrameLaterality = "(0020,9072)";
        public const string FrameContentSequence = "(0020,9111)";
        public const string PlanePositionSequence = "(0020,9113)";
        public const string PlaneOrientationSequence = "(0020,9116)";
        public const string TemporalPositionIndex = "(0020,9128)";
        public const string NominalCardiacTriggerDelayTime = "(0020,9153)";
        public const string NominalCardiacTriggerTimePriorToRPeak = "(0020,9154)";
        public const string ActualCardiacTriggerTimePriorToRPeak = "(0020,9155)";
        public const string FrameAcquisitionNumber = "(0020,9156)";
        public const string DimensionIndexValues = "(0020,9157)";
        public const string FrameComments = "(0020,9158)";
        public const string ConcatenationUID = "(0020,9161)";
        public const string InconcatenationNumber = "(0020,9162)";
        public const string InconcatenationTotalNumber = "(0020,9163)";
        public const string DimensionOrganizationUID = "(0020,9164)";
        public const string DimensionIndexPointer = "(0020,9165)";
        public const string FunctionalGroupPointer = "(0020,9167)";
        public const string DimensionIndexPrivateCreator = "(0020,9213)";
        public const string DimensionOrganizationSequence = "(0020,9221)";
        public const string DimensionIndexSequence = "(0020,9222)";
        public const string ConcatenationFrameOffsetNumber = "(0020,9228)";
        public const string FunctionalGroupPrivateCreator = "(0020,9238)";
        public const string NominalPercentageofCardiacPhase = "(0020,9241)";
        public const string NominalPercentageofRespiratoryPhase = "(0020,9245)";
        public const string StartingRespiratoryAmplitude = "(0020,9246)";
        public const string StartingRespiratoryPhase = "(0020,9247)";
        public const string EndingRespiratoryAmplitude = "(0020,9248)";
        public const string EndingRespiratoryPhase = "(0020,9249)";
        public const string RespiratoryTriggerType = "(0020,9250)";
        public const string RRIntervalTimeNominal = "(0020,9251)";
        public const string ActualCardiacTriggerDelayTime = "(0020,9252)";
        public const string RespiratorySynchronizationSequence = "(0020,9253)";
        public const string RespiratoryIntervalTime = "(0020,9254)";
        public const string NominalRespiratoryTriggerDelayTime = "(0020,9255)";
        public const string RespiratoryTriggerDelayThreshold = "(0020,9256)";
        public const string ActualRespiratoryTriggerDelayTime = "(0020,9257)";
        public const string ImagePositionVolume = "(0020,9301)";
        public const string ImageOrientationVolume = "(0020,9302)";
        public const string UltrasoundAcquisitionGeometry = "(0020,9307)";
        public const string ApexPosition = "(0020,9308)";
        public const string VolumetoTransducerMappingMatrix = "(0020,9309)";
        public const string VolumetoTableMappingMatrix = "(0020,930A)";
        public const string PatientFrameofReferenceSource = "(0020,930C)";
        public const string TemporalPositionTimeOffset = "(0020,930D)";
        public const string PlanePositionVolumeSequence = "(0020,930E)";
        public const string PlaneOrientationVolumeSequence = "(0020,930F)";
        public const string TemporalPositionSequence = "(0020,9310)";
        public const string DimensionOrganizationType = "(0020,9311)";
        public const string VolumeFrameofReferenceUID = "(0020,9312)";
        public const string TableFrameofReferenceUID = "(0020,9313)";
        public const string DimensionDescriptionLabel = "(0020,9421)";
        public const string PatientOrientationinFrameSequence = "(0020,9450)";
        public const string FrameLabel = "(0020,9453)";
        public const string AcquisitionIndex = "(0020,9518)";
        public const string ContributingSOPInstancesReferenceSequence = "(0020,9529)";
        public const string ReconstructionIndex = "(0020,9536)";
        public const string LightPathFilterPassThroughWavelength = "(0022,0001)";
        public const string LightPathFilterPassBand = "(0022,0002)";
        public const string ImagePathFilterPassThroughWavelength = "(0022,0003)";
        public const string ImagePathFilterPassBand = "(0022,0004)";
        public const string PatientEyeMovementCommanded = "(0022,0005)";
        public const string PatientEyeMovementCommandCodeSequence = "(0022,0006)";
        public const string SphericalLensPower = "(0022,0007)";
        public const string CylinderLensPower = "(0022,0008)";
        public const string CylinderAxis = "(0022,0009)";
        public const string EmmetropicMagnification = "(0022,000A)";
        public const string IntraOcularPressure = "(0022,000B)";
        public const string HorizontalFieldofView = "(0022,000C)";
        public const string PupilDilated = "(0022,000D)";
        public const string DegreeofDilation = "(0022,000E)";
        public const string StereoBaselineAngle = "(0022,0010)";
        public const string StereoBaselineDisplacement = "(0022,0011)";
        public const string StereoHorizontalPixelOffset = "(0022,0012)";
        public const string StereoVerticalPixelOffset = "(0022,0013)";
        public const string StereoRotation = "(0022,0014)";
        public const string AcquisitionDeviceTypeCodeSequence = "(0022,0015)";
        public const string IlluminationTypeCodeSequence = "(0022,0016)";
        public const string LightPathFilterTypeStackCodeSequence = "(0022,0017)";
        public const string ImagePathFilterTypeStackCodeSequence = "(0022,0018)";
        public const string LensesCodeSequence = "(0022,0019)";
        public const string ChannelDescriptionCodeSequence = "(0022,001A)";
        public const string RefractiveStateSequence = "(0022,001B)";
        public const string MydriaticAgentCodeSequence = "(0022,001C)";
        public const string RelativeImagePositionCodeSequence = "(0022,001D)";
        public const string CameraAngleofView = "(0022,001E)";
        public const string StereoPairsSequence = "(0022,0020)";
        public const string LeftImageSequence = "(0022,0021)";
        public const string RightImageSequence = "(0022,0022)";
        public const string AxialLengthoftheEye = "(0022,0030)";
        public const string OphthalmicFrameLocationSequence = "(0022,0031)";
        public const string ReferenceCoordinates = "(0022,0032)";
        public const string DepthSpatialResolution = "(0022,0035)";
        public const string MaximumDepthDistortion = "(0022,0036)";
        public const string AlongscanSpatialResolution = "(0022,0037)";
        public const string MaximumAlongscanDistortion = "(0022,0038)";
        public const string OphthalmicImageOrientation = "(0022,0039)";
        public const string DepthofTransverseImage = "(0022,0041)";
        public const string MydriaticAgentConcentrationUnitsSequence = "(0022,0042)";
        public const string AcrossscanSpatialResolution = "(0022,0048)";
        public const string MaximumAcrossscanDistortion = "(0022,0049)";
        public const string MydriaticAgentConcentration = "(0022,004E)";
        public const string IlluminationWaveLength = "(0022,0055)";
        public const string IlluminationPower = "(0022,0056)";
        public const string IlluminationBandwidth = "(0022,0057)";
        public const string MydriaticAgentSequence = "(0022,0058)";
        public const string OphthalmicAxialMeasurementsRightEyeSequence = "(0022,1007)";
        public const string OphthalmicAxialMeasurementsLeftEyeSequence = "(0022,1008)";
        public const string OphthalmicAxialLengthMeasurementsType = "(0022,1010)";
        public const string OphthalmicAxialLength = "(0022,1019)";
        public const string LensStatusCodeSequence = "(0022,1024)";
        public const string VitreousStatusCodeSequence = "(0022,1025)";
        public const string IOLFormulaCodeSequence = "(0022,1028)";
        public const string IOLFormulaDetail = "(0022,1029)";
        public const string KeratometerIndex = "(0022,1033)";
        public const string SourceofOphthalmicAxialLengthCodeSequence = "(0022,1035)";
        public const string TargetRefraction = "(0022,1037)";
        public const string RefractiveProcedureOccurred = "(0022,1039)";
        public const string RefractiveSurgeryTypeCodeSequence = "(0022,1040)";
        public const string OphthalmicUltrasoundAxialMeasurementsTypeCodeSequence = "(0022,1044)";
        public const string OphthalmicAxialLengthMeasurementsSequence = "(0022,1050)";
        public const string IOLPower = "(0022,1053)";
        public const string PredictedRefractiveError = "(0022,1054)";
        public const string OphthalmicAxialLengthVelocity = "(0022,1059)";
        public const string LensStatusDescription = "(0022,1065)";
        public const string VitreousStatusDescription = "(0022,1066)";
        public const string IOLPowerSequence = "(0022,1090)";
        public const string LensConstantSequence = "(0022,1092)";
        public const string IOLManufacturer = "(0022,1093)";
        public const string LensConstantDescription = "(0022,1094)";
        public const string KeratometryMeasurementTypeCodeSequence = "(0022,1096)";
        public const string ReferencedOphthalmicAxialMeasurementsSequence = "(0022,1100)";
        public const string OphthalmicAxialLengthMeasurementsSegmentNameCodeSequence = "(0022,1101)";
        public const string RefractiveErrorBefore = "(0022,1103)";
        public const string IOLPowerForExactEmmetropia = "(0022,1121)";
        public const string IOLPowerForExactTargetRefraction = "(0022,1122)";
        public const string AnteriorChamberDepthDefinitionCodeSequence = "(0022,1125)";
        public const string LensThickness = "(0022,1130)";
        public const string AnteriorChamberDepth = "(0022,1131)";
        public const string SourceofLensThicknessDataCodeSequence = "(0022,1132)";
        public const string SourceofAnteriorChamberDepthDataCodeSequence = "(0022,1133)";
        public const string SourceofRefractiveErrorDataCodeSequence = "(0022,1135)";
        public const string OphthalmicAxialLengthMeasurementModified = "(0022,1140)";
        public const string OphthalmicAxialLengthDataSourceCodeSequence = "(0022,1150)";
        public const string OphthalmicAxialLengthAcquisitionMethodCodeSequence = "(0022,1153)";
        public const string SignaltoNoiseRatio = "(0022,1155)";
        public const string OphthalmicAxialLengthDataSourceDescription = "(0022,1159)";
        public const string OphthalmicAxialLengthMeasurementsTotalLengthSequence = "(0022,1210)";
        public const string OphthalmicAxialLengthMeasurementsSegmentalLengthSequence = "(0022,1211)";
        public const string OphthalmicAxialLengthMeasurementsLengthSummationSequence = "(0022,1212)";
        public const string UltrasoundOphthalmicAxialLengthMeasurementsSequence = "(0022,1220)";
        public const string OpticalOphthalmicAxialLengthMeasurementsSequence = "(0022,1225)";
        public const string UltrasoundSelectedOphthalmicAxialLengthSequence = "(0022,1230)";
        public const string OphthalmicAxialLengthSelectionMethodCodeSequence = "(0022,1250)";
        public const string OpticalSelectedOphthalmicAxialLengthSequence = "(0022,1255)";
        public const string SelectedSegmentalOphthalmicAxialLengthSequence = "(0022,1257)";
        public const string SelectedTotalOphthalmicAxialLengthSequence = "(0022,1260)";
        public const string OphthalmicAxialLengthQualityMetricSequence = "(0022,1262)";
        public const string OphthalmicAxialLengthQualityMetricTypeDescription = "(0022,1273)";
        public const string IntraocularLensCalculationsRightEyeSequence = "(0022,1300)";
        public const string IntraocularLensCalculationsLeftEyeSequence = "(0022,1310)";
        public const string ReferencedOphthalmicAxialLengthMeasurementQCImageSequence = "(0022,1330)";
        public const string VisualFieldHorizontalExtent = "(0024,0010)";
        public const string VisualFieldVerticalExtent = "(0024,0011)";
        public const string VisualFieldShape = "(0024,0012)";
        public const string ScreeningTestModeCodeSequence = "(0024,0016)";
        public const string MaximumStimulusLuminance = "(0024,0018)";
        public const string BackgroundLuminance = "(0024,0020)";
        public const string StimulusColorCodeSequence = "(0024,0021)";
        public const string BackgroundIlluminationColorCodeSequence = "(0024,0024)";
        public const string StimulusArea = "(0024,0025)";
        public const string StimulusPresentationTime = "(0024,0028)";
        public const string FixationSequence = "(0024,0032)";
        public const string FixationMonitoringCodeSequence = "(0024,0033)";
        public const string VisualFieldCatchTrialSequence = "(0024,0034)";
        public const string FixationCheckedQuantity = "(0024,0035)";
        public const string PatientNotProperlyFixatedQuantity = "(0024,0036)";
        public const string PresentedVisualStimuliDataFlag = "(0024,0037)";
        public const string NumberofVisualStimuli = "(0024,0038)";
        public const string ExcessiveFixationLossesDataFlag = "(0024,0039)";
        public const string ExcessiveFixationLosses = "(0024,0040)";
        public const string StimuliRetestingQuantity = "(0024,0042)";
        public const string CommentsonPatientPerformanceofVisualField = "(0024,0044)";
        public const string FalseNegativesEstimateFlag = "(0024,0045)";
        public const string FalseNegativesEstimate = "(0024,0046)";
        public const string NegativeCatchTrialsQuantity = "(0024,0048)";
        public const string FalseNegativesQuantity = "(0024,0050)";
        public const string ExcessiveFalseNegativesDataFlag = "(0024,0051)";
        public const string ExcessiveFalseNegatives = "(0024,0052)";
        public const string FalsePositivesEstimateFlag = "(0024,0053)";
        public const string FalsePositivesEstimate = "(0024,0054)";
        public const string CatchTrialsDataFlag = "(0024,0055)";
        public const string PositiveCatchTrialsQuantity = "(0024,0056)";
        public const string TestPointNormalsDataFlag = "(0024,0057)";
        public const string TestPointNormalsSequence = "(0024,0058)";
        public const string GlobalDeviationProbabilityNormalsFlag = "(0024,0059)";
        public const string FalsePositivesQuantity = "(0024,0060)";
        public const string ExcessiveFalsePositivesDataFlag = "(0024,0061)";
        public const string ExcessiveFalsePositives = "(0024,0062)";
        public const string VisualFieldTestNormalsFlag = "(0024,0063)";
        public const string ResultsNormalsSequence = "(0024,0064)";
        public const string AgeCorrectedSensitivityDeviationAlgorithmSequence = "(0024,0065)";
        public const string GlobalDeviationFromNormal = "(0024,0066)";
        public const string GeneralizedDefectSensitivityDeviationAlgorithmSequence = "(0024,0067)";
        public const string LocalizedDeviationfromNormal = "(0024,0068)";
        public const string PatientReliabilityIndicator = "(0024,0069)";
        public const string VisualFieldMeanSensitivity = "(0024,0070)";
        public const string GlobalDeviationProbability = "(0024,0071)";
        public const string LocalDeviationProbabilityNormalsFlag = "(0024,0072)";
        public const string LocalizedDeviationProbability = "(0024,0073)";
        public const string ShortTermFluctuationCalculated = "(0024,0074)";
        public const string ShortTermFluctuation = "(0024,0075)";
        public const string ShortTermFluctuationProbabilityCalculated = "(0024,0076)";
        public const string ShortTermFluctuationProbability = "(0024,0077)";
        public const string CorrectedLocalizedDeviationFromNormalCalculated = "(0024,0078)";
        public const string CorrectedLocalizedDeviationFromNormal = "(0024,0079)";
        public const string CorrectedLocalizedDeviationFromNormalProbabilityCalculated = "(0024,0080)";
        public const string CorrectedLocalizedDeviationFromNormalProbability = "(0024,0081)";
        public const string GlobalDeviationProbabilitySequence = "(0024,0083)";
        public const string LocalizedDeviationProbabilitySequence = "(0024,0085)";
        public const string FovealSensitivityMeasured = "(0024,0086)";
        public const string FovealSensitivity = "(0024,0087)";
        public const string VisualFieldTestDuration = "(0024,0088)";
        public const string VisualFieldTestPointSequence = "(0024,0089)";
        public const string VisualFieldTestPointXCoordinate = "(0024,0090)";
        public const string VisualFieldTestPointYCoordinate = "(0024,0091)";
        public const string AgeCorrectedSensitivityDeviationValue = "(0024,0092)";
        public const string StimulusResults = "(0024,0093)";
        public const string SensitivityValue = "(0024,0094)";
        public const string RetestStimulusSeen = "(0024,0095)";
        public const string RetestSensitivityValue = "(0024,0096)";
        public const string VisualFieldTestPointNormalsSequence = "(0024,0097)";
        public const string QuantifiedDefect = "(0024,0098)";
        public const string AgeCorrectedSensitivityDeviationProbabilityValue = "(0024,0100)";
        public const string GeneralizedDefectCorrectedSensitivityDeviationFlag = "(0024,0102)";
        public const string GeneralizedDefectCorrectedSensitivityDeviationValue = "(0024,0103)";
        public const string GeneralizedDefectCorrectedSensitivityDeviationProbabilityValue = "(0024,0104)";
        public const string MinimumSensitivityValue = "(0024,0105)";
        public const string BlindSpotLocalized = "(0024,0106)";
        public const string BlindSpotXCoordinate = "(0024,0107)";
        public const string BlindSpotYCoordinate = "(0024,0108)";
        public const string VisualAcuityMeasurementSequence = "(0024,0110)";
        public const string RefractiveParametersUsedonPatientSequence = "(0024,0112)";
        public const string MeasurementLaterality = "(0024,0113)";
        public const string OphthalmicPatientClinicalInformationLeftEyeSequence = "(0024,0114)";
        public const string OphthalmicPatientClinicalInformationRightEyeSequence = "(0024,0115)";
        public const string FovealPointNormativeDataFlag = "(0024,0117)";
        public const string FovealPointProbabilityValue = "(0024,0118)";
        public const string ScreeningBaselineMeasured = "(0024,0120)";
        public const string ScreeningBaselineMeasuredSequence = "(0024,0122)";
        public const string ScreeningBaselineType = "(0024,0124)";
        public const string ScreeningBaselineValue = "(0024,0126)";
        public const string AlgorithmSource = "(0024,0202)";
        public const string DataSetName = "(0024,0306)";
        public const string DataSetVersion = "(0024,0307)";
        public const string DataSetSource = "(0024,0308)";
        public const string DataSetDescription = "(0024,0309)";
        public const string VisualFieldTestReliabilityGlobalIndexSequence = "(0024,0317)";
        public const string VisualFieldGlobalResultsIndexSequence = "(0024,0320)";
        public const string DataObservationSequence = "(0024,0325)";
        public const string IndexNormalsFlag = "(0024,0338)";
        public const string IndexProbability = "(0024,0341)";
        public const string IndexProbabilitySequence = "(0024,0344)";
        public const string SamplesperPixel = "(0028,0002)";
        public const string SamplesperPixelUsed = "(0028,0003)";
        public const string PhotometricInterpretation = "(0028,0004)";
        public const string ImageDimensions = "(0028,0005)";
        public const string PlanarConfiguration = "(0028,0006)";
        public const string NumberofFrames = "(0028,0008)";
        public const string FrameIncrementPointer = "(0028,0009)";
        public const string FrameDimensionPointer = "(0028,000A)";
        public const string Rows = "(0028,0010)";
        public const string Columns = "(0028,0011)";
        public const string Planes = "(0028,0012)";
        public const string UltrasoundColorDataPresent = "(0028,0014)";
        public const string PixelSpacing = "(0028,0030)";
        public const string ZoomFactor = "(0028,0031)";
        public const string ZoomCenter = "(0028,0032)";
        public const string PixelAspectRatio = "(0028,0034)";
        public const string ImageFormat = "(0028,0040)";
        public const string ManipulatedImage = "(0028,0050)";
        public const string CorrectedImage = "(0028,0051)";
        public const string CompressionRecognitionCode = "(0028,005F)";
        public const string CompressionCode = "(0028,0060)";
        public const string CompressionOriginator = "(0028,0061)";
        public const string CompressionLabel = "(0028,0062)";
        public const string CompressionDescription = "(0028,0063)";
        public const string CompressionSequence = "(0028,0065)";
        public const string CompressionStepPointers = "(0028,0066)";
        public const string RepeatInterval = "(0028,0068)";
        public const string BitsGrouped = "(0028,0069)";
        public const string PerimeterTable = "(0028,0070)";
        public const string PerimeterValue = "(0028,0071)";
        public const string PredictorRows = "(0028,0080)";
        public const string PredictorColumns = "(0028,0081)";
        public const string PredictorConstants = "(0028,0082)";
        public const string BlockedPixels = "(0028,0090)";
        public const string BlockRows = "(0028,0091)";
        public const string BlockColumns = "(0028,0092)";
        public const string RowOverlap = "(0028,0093)";
        public const string ColumnOverlap = "(0028,0094)";
        public const string BitsAllocated = "(0028,0100)";
        public const string BitsStored = "(0028,0101)";
        public const string HighBit = "(0028,0102)";
        public const string PixelRepresentation = "(0028,0103)";
        public const string SmallestValidPixelValue = "(0028,0104)";
        public const string LargestValidPixelValue = "(0028,0105)";
        public const string SmallestImagePixelValue = "(0028,0106)";
        public const string LargestImagePixelValue = "(0028,0107)";
        public const string SmallestPixelValueinSeries = "(0028,0108)";
        public const string LargestPixelValueinSeries = "(0028,0109)";
        public const string SmallestImagePixelValueinPlane = "(0028,0110)";
        public const string LargestImagePixelValueinPlane = "(0028,0111)";
        public const string PixelPaddingValue = "(0028,0120)";
        public const string PixelPaddingRangeLimit = "(0028,0121)";
        public const string ImageLocation = "(0028,0200)";
        public const string QualityControlImage = "(0028,0300)";
        public const string BurnedInAnnotation = "(0028,0301)";
        public const string RecognizableVisualFeatures = "(0028,0302)";
        public const string LongitudinalTemporalInformationModified = "(0028,0303)";
        public const string TransformLabel = "(0028,0400)";
        public const string TransformVersionNumber = "(0028,0401)";
        public const string NumberofTransformSteps = "(0028,0402)";
        public const string SequenceofCompressedData = "(0028,0403)";
        public const string DetailsofCoefficients = "(0028,0404)";
        public const string DCTLabel = "(0028,0700)";
        public const string DataBlockDescription = "(0028,0701)";
        public const string DataBlock = "(0028,0702)";
        public const string NormalizationFactorFormat = "(0028,0710)";
        public const string ZonalMapNumberFormat = "(0028,0720)";
        public const string ZonalMapLocation = "(0028,0721)";
        public const string ZonalMapFormat = "(0028,0722)";
        public const string AdaptiveMapFormat = "(0028,0730)";
        public const string CodeNumberFormat = "(0028,0740)";
        public const string PixelSpacingCalibrationType = "(0028,0A02)";
        public const string PixelSpacingCalibrationDescription = "(0028,0A04)";
        public const string PixelIntensityRelationship = "(0028,1040)";
        public const string PixelIntensityRelationshipSign = "(0028,1041)";
        public const string WindowCenter = "(0028,1050)";
        public const string WindowWidth = "(0028,1051)";
        public const string RescaleIntercept = "(0028,1052)";
        public const string RescaleSlope = "(0028,1053)";
        public const string RescaleType = "(0028,1054)";
        public const string WindowCenterWidthExplanation = "(0028,1055)";
        public const string VOILUTFunction = "(0028,1056)";
        public const string GrayScale = "(0028,1080)";
        public const string RecommendedViewingMode = "(0028,1090)";
        public const string GrayLookupTableDescriptor = "(0028,1100)";
        public const string RedPaletteColorLookupTableDescriptor = "(0028,1101)";
        public const string GreenPaletteColorLookupTableDescriptor = "(0028,1102)";
        public const string BluePaletteColorLookupTableDescriptor = "(0028,1103)";
        public const string AlphaPaletteColorLookupTableDescriptor = "(0028,1104)";
        public const string LargeRedPaletteColorLookupTableDescriptor = "(0028,1111)";
        public const string LargeGreenPaletteColorLookupTableDescriptor = "(0028,1112)";
        public const string LargeBluePaletteColorLookupTableDescriptor = "(0028,1113)";
        public const string PaletteColorLookupTableUID = "(0028,1199)";
        public const string GrayLookupTableData = "(0028,1200)";
        public const string RedPaletteColorLookupTableData = "(0028,1201)";
        public const string GreenPaletteColorLookupTableData = "(0028,1202)";
        public const string BluePaletteColorLookupTableData = "(0028,1203)";
        public const string AlphaPaletteColorLookupTableData = "(0028,1204)";
        public const string LargeRedPaletteColorLookupTableData = "(0028,1211)";
        public const string LargeGreenPaletteColorLookupTableData = "(0028,1212)";
        public const string LargeBluePaletteColorLookupTableData = "(0028,1213)";
        public const string LargePaletteColorLookupTableUID = "(0028,1214)";
        public const string SegmentedRedPaletteColorLookupTableData = "(0028,1221)";
        public const string SegmentedGreenPaletteColorLookupTableData = "(0028,1222)";
        public const string SegmentedBluePaletteColorLookupTableData = "(0028,1223)";
        public const string BreastImplantPresent = "(0028,1300)";
        public const string PartialView = "(0028,1350)";
        public const string PartialViewDescription = "(0028,1351)";
        public const string PartialViewCodeSequence = "(0028,1352)";
        public const string SpatialLocationsPreserved = "(0028,135A)";
        public const string DataFrameAssignmentSequence = "(0028,1401)";
        public const string DataPathAssignment = "(0028,1402)";
        public const string BitsMappedtoColorLookupTable = "(0028,1403)";
        public const string BlendingLUT1Sequence = "(0028,1404)";
        public const string BlendingLUT1TransferFunction = "(0028,1405)";
        public const string BlendingWeightConstant = "(0028,1406)";
        public const string BlendingLookupTableDescriptor = "(0028,1407)";
        public const string BlendingLookupTableData = "(0028,1408)";
        public const string EnhancedPaletteColorLookupTableSequence = "(0028,140B)";
        public const string BlendingLUT2Sequence = "(0028,140C)";
        public const string BlendingLUT2TransferFunction = "(0028,140D)";
        public const string DataPathID = "(0028,140E)";
        public const string RGBLUTTransferFunction = "(0028,140F)";
        public const string AlphaLUTTransferFunction = "(0028,1410)";
        public const string ICCProfile = "(0028,2000)";
        public const string LossyImageCompression = "(0028,2110)";
        public const string LossyImageCompressionRatio = "(0028,2112)";
        public const string LossyImageCompressionMethod = "(0028,2114)";
        public const string ModalityLUTSequence = "(0028,3000)";
        public const string LUTDescriptor = "(0028,3002)";
        public const string LUTExplanation = "(0028,3003)";
        public const string ModalityLUTType = "(0028,3004)";
        public const string LUTData = "(0028,3006)";
        public const string VOILUTSequence = "(0028,3010)";
        public const string SoftcopyVOILUTSequence = "(0028,3110)";
        public const string ImagePresentationComments = "(0028,4000)";
        public const string BiPlaneAcquisitionSequence = "(0028,5000)";
        public const string RepresentativeFrameNumber = "(0028,6010)";
        public const string FrameNumbersofInterestFOI = "(0028,6020)";
        public const string FrameofInterestDescription = "(0028,6022)";
        public const string FrameofInterestType = "(0028,6023)";
        public const string MaskPointers = "(0028,6030)";
        public const string RWavePointer = "(0028,6040)";
        public const string MaskSubtractionSequence = "(0028,6100)";
        public const string MaskOperation = "(0028,6101)";
        public const string ApplicableFrameRange = "(0028,6102)";
        public const string MaskFrameNumbers = "(0028,6110)";
        public const string ContrastFrameAveraging = "(0028,6112)";
        public const string MaskSubpixelShift = "(0028,6114)";
        public const string TIDOffset = "(0028,6120)";
        public const string MaskOperationExplanation = "(0028,6190)";
        public const string PixelDataProviderURL = "(0028,7FE0)";
        public const string DataPointRows = "(0028,9001)";
        public const string DataPointColumns = "(0028,9002)";
        public const string SignalDomainColumns = "(0028,9003)";
        public const string LargestMonochromePixelValue = "(0028,9099)";
        public const string DataRepresentation = "(0028,9108)";
        public const string PixelMeasuresSequence = "(0028,9110)";
        public const string FrameVOILUTSequence = "(0028,9132)";
        public const string PixelValueTransformationSequence = "(0028,9145)";
        public const string SignalDomainRows = "(0028,9235)";
        public const string DisplayFilterPercentage = "(0028,9411)";
        public const string FramePixelShiftSequence = "(0028,9415)";
        public const string SubtractionItemID = "(0028,9416)";
        public const string PixelIntensityRelationshipLUTSequence = "(0028,9422)";
        public const string FramePixelDataPropertiesSequence = "(0028,9443)";
        public const string GeometricalProperties = "(0028,9444)";
        public const string GeometricMaximumDistortion = "(0028,9445)";
        public const string ImageProcessingApplied = "(0028,9446)";
        public const string MaskSelectionMode = "(0028,9454)";
        public const string LUTFunction = "(0028,9474)";
        public const string MaskVisibilityPercentage = "(0028,9478)";
        public const string PixelShiftSequence = "(0028,9501)";
        public const string RegionPixelShiftSequence = "(0028,9502)";
        public const string VerticesoftheRegion = "(0028,9503)";
        public const string MultiframePresentationSequence = "(0028,9505)";
        public const string PixelShiftFrameRange = "(0028,9506)";
        public const string LUTFrameRange = "(0028,9507)";
        public const string ImagetoEquipmentMappingMatrix = "(0028,9520)";
        public const string EquipmentCoordinateSystemIdentification = "(0028,9537)";
        public const string DirectviewGroup = "(0029,0010)";
        public const string PtoneWindowCenter = "(0029,1002)";
        public const string PtoneWindowWidth = "(0029,1003)";
        public const string EclipseBodyPart = "(0029,1004)";
        public const string EclipsePosition = "(0029,1005)";
        public const string ReprocessedImage = "(0029,1006)";
        public const string SensitivityLabel = "(0029,1007)";
        public const string ImagingData1 = "(0029,1008)";
        public const string ImagingData2 = "(0029,1009)";
        public const string ImagingData3 = "(0029,1010)";
        public const string CompressedDefectMap = "(0029,1011)";
        public const string CCViewName = "(0029,1015)";
        public const string CCViewGuid = "(0029,1016)";
        public const string ProcessingViewGuid = "(0029,1017)";
        public const string EclipseState = "(0029,1018)";
        public const string Flip = "(0029,1019)";
        public const string DetectorCropTop = "(0029,1020)";
        public const string DetectorCropBottom = "(0029,1021)";
        public const string DetectorCropLeft = "(0029,1022)";
        public const string DetectorCropRight = "(0029,1023)";
        public const string LinearToLogLowOffset = "(0029,1026)";
        public const string LinearToLogHighOffset = "(0029,1027)";
        public const string LinearToLogScale = "(0029,1029)";
        public const string Rotation = "(0029,101A)";
        public const string HarvestType = "(0029,101B)";
        public const string HarvestParameters = "(0029,101C)";
        public const string ZEncoderPosition = "(0029,101D)";
        public const string PreviewBitsAllocated = "(0029,10FA)";
        public const string PreviewBitsStored = "(0029,10FB)";
        public const string PreviewRows = "(0029,10FC)";
        public const string PreviewColumns = "(0029,10FD)";
        public const string PreviewPixelData = "(0029,10FE)";
        public const string ExamClass = "(0029,10FF)";
        public const string StudyStatusID = "(0032,000A)";
        public const string StudyPriorityID = "(0032,000C)";
        public const string StudyIDIssuer = "(0032,0012)";
        public const string StudyVerifiedDate = "(0032,0032)";
        public const string StudyVerifiedTime = "(0032,0033)";
        public const string StudyReadDate = "(0032,0034)";
        public const string StudyReadTime = "(0032,0035)";
        public const string ScheduledStudyStartDate = "(0032,1000)";
        public const string ScheduledStudyStartTime = "(0032,1001)";
        public const string ScheduledStudyStopDate = "(0032,1010)";
        public const string ScheduledStudyStopTime = "(0032,1011)";
        public const string ScheduledStudyLocation = "(0032,1020)";
        public const string ScheduledStudyLocationAETitle = "(0032,1021)";
        public const string ReasonforStudy = "(0032,1030)";
        public const string RequestingPhysicianIdentificationSequence = "(0032,1031)";
        public const string RequestingPhysician = "(0032,1032)";
        public const string RequestingService = "(0032,1033)";
        public const string RequestingServiceCodeSequence = "(0032,1034)";
        public const string StudyArrivalDate = "(0032,1040)";
        public const string StudyArrivalTime = "(0032,1041)";
        public const string StudyCompletionDate = "(0032,1050)";
        public const string StudyCompletionTime = "(0032,1051)";
        public const string StudyComponentStatusID = "(0032,1055)";
        public const string RequestedProcedureDescription = "(0032,1060)";
        public const string RequestedProcedureCodeSequence = "(0032,1064)";
        public const string RequestedContrastAgent = "(0032,1070)";
        public const string StudyComments = "(0032,4000)";
        public const string ReferencedPatientAliasSequence = "(0038,0004)";
        public const string VisitStatusID = "(0038,0008)";
        public const string AdmissionID = "(0038,0010)";
        public const string IssuerofAdmissionID = "(0038,0011)";
        public const string IssuerofAdmissionIDSequence = "(0038,0014)";
        public const string RouteofAdmissions = "(0038,0016)";
        public const string ScheduledAdmissionDate = "(0038,001A)";
        public const string ScheduledAdmissionTime = "(0038,001B)";
        public const string ScheduledDischargeDate = "(0038,001C)";
        public const string ScheduledDischargeTime = "(0038,001D)";
        public const string ScheduledPatientInstitutionResidence = "(0038,001E)";
        public const string AdmittingDate = "(0038,0020)";
        public const string AdmittingTime = "(0038,0021)";
        public const string DischargeDate = "(0038,0030)";
        public const string DischargeTime = "(0038,0032)";
        public const string DischargeDiagnosisDescription = "(0038,0040)";
        public const string DischargeDiagnosisCodeSequence = "(0038,0044)";
        public const string SpecialNeeds = "(0038,0050)";
        public const string ServiceEpisodeID = "(0038,0060)";
        public const string IssuerofServiceEpisodeID = "(0038,0061)";
        public const string ServiceEpisodeDescription = "(0038,0062)";
        public const string IssuerofServiceEpisodeIDSequence = "(0038,0064)";
        public const string PertinentDocumentsSequence = "(0038,0100)";
        public const string CurrentPatientLocation = "(0038,0300)";
        public const string PatientInstitutionResidence = "(0038,0400)";
        public const string PatientState = "(0038,0500)";
        public const string PatientClinicalTrialParticipationSequence = "(0038,0502)";
        public const string VisitComments = "(0038,4000)";
        public const string WaveformOriginality = "(003A,0004)";
        public const string NumberofWaveformChannels = "(003A,0005)";
        public const string NumberofWaveformSamples = "(003A,0010)";
        public const string SamplingFrequency = "(003A,001A)";
        public const string MultiplexGroupLabel = "(003A,0020)";
        public const string ChannelDefinitionSequence = "(003A,0200)";
        public const string WaveformChannelNumber = "(003A,0202)";
        public const string ChannelLabel = "(003A,0203)";
        public const string ChannelStatus = "(003A,0205)";
        public const string ChannelSourceSequence = "(003A,0208)";
        public const string ChannelSourceModifiersSequence = "(003A,0209)";
        public const string SourceWaveformSequence = "(003A,020A)";
        public const string ChannelDerivationDescription = "(003A,020C)";
        public const string ChannelSensitivity = "(003A,0210)";
        public const string ChannelSensitivityUnitsSequence = "(003A,0211)";
        public const string ChannelSensitivityCorrectionFactor = "(003A,0212)";
        public const string ChannelBaseline = "(003A,0213)";
        public const string ChannelTimeSkew = "(003A,0214)";
        public const string ChannelSampleSkew = "(003A,0215)";
        public const string ChannelOffset = "(003A,0218)";
        public const string WaveformBitsStored = "(003A,021A)";
        public const string FilterLowFrequency = "(003A,0220)";
        public const string FilterHighFrequency = "(003A,0221)";
        public const string NotchFilterFrequency = "(003A,0222)";
        public const string NotchFilterBandwidth = "(003A,0223)";
        public const string WaveformDataDisplayScale = "(003A,0230)";
        public const string WaveformDisplayBackgroundCIELabValue = "(003A,0231)";
        public const string WaveformPresentationGroupSequence = "(003A,0240)";
        public const string PresentationGroupNumber = "(003A,0241)";
        public const string ChannelDisplaySequence = "(003A,0242)";
        public const string ChannelRecommendedDisplayCIELabValue = "(003A,0244)";
        public const string ChannelPosition = "(003A,0245)";
        public const string DisplayShadingFlag = "(003A,0246)";
        public const string FractionalChannelDisplayScale = "(003A,0247)";
        public const string AbsoluteChannelDisplayScale = "(003A,0248)";
        public const string MultiplexedAudioChannelsDescriptionCodeSequence = "(003A,0300)";
        public const string ChannelIdentificationCode = "(003A,0301)";
        public const string ChannelMode = "(003A,0302)";
        public const string ScheduledStationAETitle = "(0040,0001)";
        public const string ScheduledProcedureStepStartDate = "(0040,0002)";
        public const string ScheduledProcedureStepStartTime = "(0040,0003)";
        public const string ScheduledProcedureStepEndDate = "(0040,0004)";
        public const string ScheduledProcedureStepEndTime = "(0040,0005)";
        public const string ScheduledPerformingPhysicianName = "(0040,0006)";
        public const string ScheduledProcedureStepDescription = "(0040,0007)";
        public const string ScheduledProtocolCodeSequence = "(0040,0008)";
        public const string ScheduledProcedureStepID = "(0040,0009)";
        public const string StageCodeSequence = "(0040,000A)";
        public const string ScheduledPerformingPhysicianIdentificationSequence = "(0040,000B)";
        public const string ScheduledStationName = "(0040,0010)";
        public const string ScheduledProcedureStepLocation = "(0040,0011)";
        public const string PreMedication = "(0040,0012)";
        public const string ScheduledProcedureStepStatus = "(0040,0020)";
        public const string OrderPlacerIdentifierSequence = "(0040,0026)";
        public const string OrderFillerIdentifierSequence = "(0040,0027)";
        public const string LocalNamespaceEntityID = "(0040,0031)";
        public const string UniversalEntityID = "(0040,0032)";
        public const string UniversalEntityIDType = "(0040,0033)";
        public const string IdentifierTypeCode = "(0040,0035)";
        public const string AssigningFacilitySequence = "(0040,0036)";
        public const string AssigningJurisdictionCodeSequence = "(0040,0039)";
        public const string AssigningAgencyorDepartmentCodeSequence = "(0040,003A)";
        public const string ScheduledProcedureStepSequence = "(0040,0100)";
        public const string ReferencedNonImageCompositeSOPInstanceSequence = "(0040,0220)";
        public const string PerformedStationAETitle = "(0040,0241)";
        public const string PerformedStationName = "(0040,0242)";
        public const string PerformedLocation = "(0040,0243)";
        public const string PerformedProcedureStepStartDate = "(0040,0244)";
        public const string PerformedProcedureStepStartTime = "(0040,0245)";
        public const string PerformedProcedureStepEndDate = "(0040,0250)";
        public const string PerformedProcedureStepEndTime = "(0040,0251)";
        public const string PerformedProcedureStepStatus = "(0040,0252)";
        public const string PerformedProcedureStepID = "(0040,0253)";
        public const string PerformedProcedureStepDescription = "(0040,0254)";
        public const string PerformedProcedureTypeDescription = "(0040,0255)";
        public const string PerformedProtocolCodeSequence = "(0040,0260)";
        public const string PerformedProtocolType = "(0040,0261)";
        public const string ScheduledStepAttributesSequence = "(0040,0270)";
        public const string RequestAttributesSequence = "(0040,0275)";
        public const string CommentsonthePerformedProcedureStep = "(0040,0280)";
        public const string PerformedProcedureStepDiscontinuationReasonCodeSequence = "(0040,0281)";
        public const string QuantitySequence = "(0040,0293)";
        public const string Quantity = "(0040,0294)";
        public const string MeasuringUnitsSequence = "(0040,0295)";
        public const string BillingItemSequence = "(0040,0296)";
        public const string TotalTimeofFluoroscopy = "(0040,0300)";
        public const string TotalNumberofExposures = "(0040,0301)";
        public const string EntranceDose = "(0040,0302)";
        public const string ExposedArea = "(0040,0303)";
        public const string DistanceSourcetoEntrance = "(0040,0306)";
        public const string DistanceSourcetoSupport = "(0040,0307)";
        public const string ExposureDoseSequence = "(0040,030E)";
        public const string CommentsonRadiationDose = "(0040,0310)";
        public const string XRayOutput = "(0040,0312)";
        public const string HalfValueLayer = "(0040,0314)";
        public const string OrganDose = "(0040,0316)";
        public const string OrganExposed = "(0040,0318)";
        public const string BillingProcedureStepSequence = "(0040,0320)";
        public const string FilmConsumptionSequence = "(0040,0321)";
        public const string BillingSuppliesandDevicesSequence = "(0040,0324)";
        public const string ReferencedProcedureStepSequence = "(0040,0330)";
        public const string PerformedSeriesSequence = "(0040,0340)";
        public const string CommentsontheScheduledProcedureStep = "(0040,0400)";
        public const string ProtocolContextSequence = "(0040,0440)";
        public const string ContentItemModifierSequence = "(0040,0441)";
        public const string ScheduledSpecimenSequence = "(0040,0500)";
        public const string SpecimenAccessionNumber = "(0040,050A)";
        public const string ContainerIdentifier = "(0040,0512)";
        public const string IssueroftheContainerIdentifierSequence = "(0040,0513)";
        public const string AlternateContainerIdentifierSequence = "(0040,0515)";
        public const string ContainerTypeCodeSequence = "(0040,0518)";
        public const string ContainerDescription = "(0040,051A)";
        public const string ContainerComponentSequence = "(0040,0520)";
        public const string SpecimenSequence = "(0040,0550)";
        public const string SpecimenIdentifier = "(0040,0551)";
        public const string SpecimenDescriptionSequenceTrial = "(0040,0552)";
        public const string SpecimenDescriptionTrial = "(0040,0553)";
        public const string SpecimenUID = "(0040,0554)";
        public const string AcquisitionContextSequence = "(0040,0555)";
        public const string AcquisitionContextDescription = "(0040,0556)";
        public const string SpecimenDescriptionSequence = "(0040,0560)";
        public const string IssueroftheSpecimenIdentifierSequence = "(0040,0562)";
        public const string SpecimenTypeCodeSequence = "(0040,059A)";
        public const string SpecimenShortDescription = "(0040,0600)";
        public const string SpecimenDetailedDescription = "(0040,0602)";
        public const string SpecimenPreparationSequence = "(0040,0610)";
        public const string SpecimenPreparationStepContentItemSequence = "(0040,0612)";
        public const string SpecimenLocalizationContentItemSequence = "(0040,0620)";
        public const string SlideIdentifier = "(0040,06FA)";
        public const string ImageCenterPointCoordinatesSequence = "(0040,071A)";
        public const string XOffsetinSlideCoordinateSystem = "(0040,072A)";
        public const string YOffsetinSlideCoordinate = "(0040,073A)";
        public const string ZOffsetinSlideCoordinateSystem = "(0040,074A)";
        public const string PixelSpacingSequence = "(0040,08D8)";
        public const string CoordinateSystemAxisCodeSequence = "(0040,08DA)";
        public const string MeasurementUnitsCodeSequence = "(0040,08EA)";
        public const string VitalStainCodeSequenceTrial = "(0040,09F8)";
        public const string RequestedProcedureID = "(0040,1001)";
        public const string ReasonfortheRequestedProcedure = "(0040,1002)";
        public const string RequestedProcedurePriority = "(0040,1003)";
        public const string PatientTransportArrangements = "(0040,1004)";
        public const string RequestedProcedureLocation = "(0040,1005)";
        public const string PlacerOrderNumberProcedure = "(0040,1006)";
        public const string FillerOrderNumberProcedure = "(0040,1007)";
        public const string ConfidentialityCode = "(0040,1008)";
        public const string ReportingPriority = "(0040,1009)";
        public const string ReasonforRequestedProcedureCodeSequence = "(0040,100A)";
        public const string NamesofIntendedRecipientsofResults = "(0040,1010)";
        public const string IntendedRecipientsofResultsIdentificationSequence = "(0040,1011)";
        public const string ReasonForPerformedProcedureCodeSequence = "(0040,1012)";
        public const string RequestedProcedureDescriptionTrial = "(0040,1060)";
        public const string PersonIdentificationCodeSequence = "(0040,1101)";
        public const string PersonAddress = "(0040,1102)";
        public const string PersonTelephoneNumbers = "(0040,1103)";
        public const string RequestedProcedureComments = "(0040,1400)";
        public const string ReasonfortheImagingServiceRequest = "(0040,2001)";
        public const string IssueDateofImagingServiceRequest = "(0040,2004)";
        public const string IssueTimeofImagingServiceRequest = "(0040,2005)";
        public const string PlacerOrderNumberImagingServiceRequestRetired = "(0040,2006)";
        public const string FillerOrderNumberImagingServiceRequestRetired = "(0040,2007)";
        public const string OrderEnteredBy = "(0040,2008)";
        public const string OrderEntererLocation = "(0040,2009)";
        public const string OrderCallbackPhoneNumber = "(0040,2010)";
        public const string PlacerOrderNumberImagingServiceRequest = "(0040,2016)";
        public const string FillerOrderNumberImagingServiceRequest = "(0040,2017)";
        public const string ImagingServiceRequestComments = "(0040,2400)";
        public const string ConfidentialityConstraintonPatientDataDescription = "(0040,3001)";
        public const string GeneralPurposeScheduledProcedureStepStatus = "(0040,4001)";
        public const string GeneralPurposePerformedProcedureStepStatus = "(0040,4002)";
        public const string GeneralPurposeScheduledProcedureStepPriority = "(0040,4003)";
        public const string ScheduledProcessingApplicationsCodeSequence = "(0040,4004)";
        public const string ScheduledProcedureStepStartDateTime = "(0040,4005)";
        public const string MultipleCopiesFlag = "(0040,4006)";
        public const string PerformedProcessingApplicationsCodeSequence = "(0040,4007)";
        public const string HumanPerformerCodeSequence = "(0040,4009)";
        public const string ScheduledProcedureStepModificationDateTime = "(0040,4010)";
        public const string ExpectedCompletionDateTime = "(0040,4011)";
        public const string ResultingGeneralPurposePerformedProcedureStepsSequence = "(0040,4015)";
        public const string ReferencedGeneralPurposeScheduledProcedureStepSequence = "(0040,4016)";
        public const string ScheduledWorkitemCodeSequence = "(0040,4018)";
        public const string PerformedWorkitemCodeSequence = "(0040,4019)";
        public const string InputAvailabilityFlag = "(0040,4020)";
        public const string InputInformationSequence = "(0040,4021)";
        public const string RelevantInformationSequence = "(0040,4022)";
        public const string ReferencedGeneralPurposeScheduledProcedureStepTransactionUID = "(0040,4023)";
        public const string ScheduledStationNameCodeSequence = "(0040,4025)";
        public const string ScheduledStationClassCodeSequence = "(0040,4026)";
        public const string ScheduledStationGeographicLocationCodeSequence = "(0040,4027)";
        public const string PerformedStationNameCodeSequence = "(0040,4028)";
        public const string PerformedStationClassCodeSequence = "(0040,4029)";
        public const string PerformedStationGeographicLocationCodeSequence = "(0040,4030)";
        public const string RequestedSubsequentWorkitemCodeSequence = "(0040,4031)";
        public const string NonDICOMOutputCodeSequence = "(0040,4032)";
        public const string OutputInformationSequence = "(0040,4033)";
        public const string ScheduledHumanPerformersSequence = "(0040,4034)";
        public const string ActualHumanPerformersSequence = "(0040,4035)";
        public const string HumanPerformerOrganization = "(0040,4036)";
        public const string HumanPerformerName = "(0040,4037)";
        public const string RawDataHandling = "(0040,4040)";
        public const string InputReadinessState = "(0040,4041)";
        public const string PerformedProcedureStepStartDateTime = "(0040,4050)";
        public const string PerformedProcedureStepEndDateTime = "(0040,4051)";
        public const string ProcedureStepCancellationDateTime = "(0040,4052)";
        public const string EntranceDoseinmGy = "(0040,8302)";
        public const string ReferencedImageRealWorldValueMappingSequence = "(0040,9094)";
        public const string RealWorldValueMappingSequence = "(0040,9096)";
        public const string PixelValueMappingCodeSequence = "(0040,9098)";
        public const string LUTLabel = "(0040,9210)";
        public const string RealWorldValueLastValueMapped = "(0040,9211)";
        public const string RealWorldValueLUTData = "(0040,9212)";
        public const string RealWorldValueFirstValueMapped = "(0040,9216)";
        public const string RealWorldValueIntercept = "(0040,9224)";
        public const string RealWorldValueSlope = "(0040,9225)";
        public const string FindingsFlagTrial = "(0040,A007)";
        public const string RelationshipType = "(0040,A010)";
        public const string FindingsSequenceTrial = "(0040,A020)";
        public const string FindingsGroupUIDTrial = "(0040,A021)";
        public const string ReferencedFindingsGroupUIDTrial = "(0040,A022)";
        public const string FindingsGroupRecordingDateTrial = "(0040,A023)";
        public const string FindingsGroupRecordingTimeTrial = "(0040,A024)";
        public const string FindingsSourceCategoryCodeSequenceTrial = "(0040,A026)";
        public const string VerifyingOrganization = "(0040,A027)";
        public const string DocumentingOrganizationIdentifierCodeSequenceTrial = "(0040,A028)";
        public const string VerificationDateTime = "(0040,A030)";
        public const string ObservationDateTime = "(0040,A032)";
        public const string ValueType = "(0040,A040)";
        public const string ConceptNameCodeSequence = "(0040,A043)";
        public const string MeasurementPrecisionDescriptionTrial = "(0040,A047)";
        public const string ContinuityOfContent = "(0040,A050)";
        public const string UrgencyorPriorityAlertsTrial = "(0040,A057)";
        public const string SequencingIndicatorTrial = "(0040,A060)";
        public const string DocumentIdentifierCodeSequenceTrial = "(0040,A066)";
        public const string DocumentAuthorTrial = "(0040,A067)";
        public const string DocumentAuthorIdentifierCodeSequenceTrial = "(0040,A068)";
        public const string IdentifierCodeSequenceTrial = "(0040,A070)";
        public const string VerifyingObserverSequence = "(0040,A073)";
        public const string ObjectBinaryIdentifierTrial = "(0040,A074)";
        public const string VerifyingObserverName = "(0040,A075)";
        public const string DocumentingObserverIdentifierCodeSequenceTrial = "(0040,A076)";
        public const string AuthorObserverSequence = "(0040,A078)";
        public const string ParticipantSequence = "(0040,A07A)";
        public const string CustodialOrganizationSequence = "(0040,A07C)";
        public const string ParticipationType = "(0040,A080)";
        public const string ParticipationDateTime = "(0040,A082)";
        public const string ObserverType = "(0040,A084)";
        public const string ProcedureIdentifierCodeSequenceTrial = "(0040,A085)";
        public const string VerifyingObserverIdentificationCodeSequence = "(0040,A088)";
        public const string ObjectDirectoryBinaryIdentifierTrial = "(0040,A089)";
        public const string EquivalentCDADocumentSequence = "(0040,A090)";
        public const string ReferencedWaveformChannels = "(0040,A0B0)";
        public const string DateofDocumentorVerbalTransactionTrial = "(0040,A110)";
        public const string TimeofDocumentCreationorVerbalTransactionTrial = "(0040,A112)";
        public const string DateTime = "(0040,A120)";
        public const string Date = "(0040,A121)";
        public const string Time = "(0040,A122)";
        public const string PersonName = "(0040,A123)";
        public const string UID = "(0040,A124)";
        public const string ReportStatusIDTrial = "(0040,A125)";
        public const string TemporalRangeType = "(0040,A130)";
        public const string ReferencedSamplePositions = "(0040,A132)";
        public const string ReferencedFrameNumbers = "(0040,A136)";
        public const string ReferencedTimeOffsets = "(0040,A138)";
        public const string ReferencedDateTime = "(0040,A13A)";
        public const string TextValue = "(0040,A160)";
        public const string ObservationCategoryCodeSequenceTrial = "(0040,A167)";
        public const string ConceptCodeSequence = "(0040,A168)";
        public const string BibliographicCitationTrial = "(0040,A16A)";
        public const string PurposeofReferenceCodeSequence = "(0040,A170)";
        public const string ObservationUIDTrial = "(0040,A171)";
        public const string ReferencedObservationUIDTrial = "(0040,A172)";
        public const string ReferencedObservationClassTrial = "(0040,A173)";
        public const string ReferencedObjectObservationClassTrial = "(0040,A174)";
        public const string AnnotationGroupNumber = "(0040,A180)";
        public const string ObservationDateTrial = "(0040,A192)";
        public const string ObservationTimeTrial = "(0040,A193)";
        public const string MeasurementAutomationTrial = "(0040,A194)";
        public const string ModifierCodeSequence = "(0040,A195)";
        public const string IdentificationDescriptionTrial = "(0040,A224)";
        public const string CoordinatesSetGeometricTypeTrial = "(0040,A290)";
        public const string AlgorithmCodeSequenceTrial = "(0040,A296)";
        public const string AlgorithmDescriptionTrial = "(0040,A297)";
        public const string PixelCoordinatesSetTrial = "(0040,A29A)";
        public const string MeasuredValueSequence = "(0040,A300)";
        public const string NumericValueQualifierCodeSequence = "(0040,A301)";
        public const string CurrentObserverTrial = "(0040,A307)";
        public const string NumericValue = "(0040,A30A)";
        public const string ReferencedAccessionSequenceTrial = "(0040,A313)";
        public const string ReportStatusCommentTrial = "(0040,A33A)";
        public const string ProcedureContextSequenceTrial = "(0040,A340)";
        public const string VerbalSourceTrial = "(0040,A352)";
        public const string AddressTrial = "(0040,A353)";
        public const string TelephoneNumberTrial = "(0040,A354)";
        public const string VerbalSourceIdentifierCodeSequenceTrial = "(0040,A358)";
        public const string PredecessorDocumentsSequence = "(0040,A360)";
        public const string ReferencedRequestSequence = "(0040,A370)";
        public const string PerformedProcedureCodeSequence = "(0040,A372)";
        public const string CurrentRequestedProcedureEvidenceSequence = "(0040,A375)";
        public const string ReportDetailSequenceTrial = "(0040,A380)";
        public const string PertinentOtherEvidenceSequence = "(0040,A385)";
        public const string HL7StructuredDocumentReferenceSequence = "(0040,A390)";
        public const string ObservationSubjectUIDTrial = "(0040,A402)";
        public const string ObservationSubjectClassTrial = "(0040,A403)";
        public const string ObservationSubjectTypeCodeSequenceTrial = "(0040,A404)";
        public const string CompletionFlag = "(0040,A491)";
        public const string CompletionFlagDescription = "(0040,A492)";
        public const string VerificationFlag = "(0040,A493)";
        public const string ArchiveRequested = "(0040,A494)";
        public const string PreliminaryFlag = "(0040,A496)";
        public const string ContentTemplateSequence = "(0040,A504)";
        public const string IdenticalDocumentsSequence = "(0040,A525)";
        public const string ObservationSubjectContextFlagTrial = "(0040,A600)";
        public const string ObserverContextFlagTrial = "(0040,A601)";
        public const string ProcedureContextFlagTrial = "(0040,A603)";
        public const string ContentSequence = "(0040,A730)";
        public const string RelationshipSequenceTrial = "(0040,A731)";
        public const string RelationshipTypeCodeSequenceTrial = "(0040,A732)";
        public const string LanguageCodeSequenceTrial = "(0040,A744)";
        public const string UniformResourceLocatorTrial = "(0040,A992)";
        public const string WaveformAnnotationSequence = "(0040,B020)";
        public const string TemplateIdentifier = "(0040,DB00)";
        public const string TemplateVersion = "(0040,DB06)";
        public const string TemplateLocalVersion = "(0040,DB07)";
        public const string TemplateExtensionFlag = "(0040,DB0B)";
        public const string TemplateExtensionOrganizationUID = "(0040,DB0C)";
        public const string TemplateExtensionCreatorUID = "(0040,DB0D)";
        public const string ReferencedContentItemIdentifier = "(0040,DB73)";
        public const string HL7InstanceIdentifier = "(0040,E001)";
        public const string HL7DocumentEffectiveTime = "(0040,E004)";
        public const string HL7DocumentTypeCodeSequence = "(0040,E006)";
        public const string DocumentClassCodeSequence = "(0040,E008)";
        public const string RetrieveURI = "(0040,E010)";
        public const string RetrieveLocationUID = "(0040,E011)";
        public const string TypeofInstances = "(0040,E020)";
        public const string DICOMRetrievalSequence = "(0040,E021)";
        public const string DICOMMediaRetrievalSequence = "(0040,E022)";
        public const string WADORetrievalSequence = "(0040,E023)";
        public const string XDSRetrievalSequence = "(0040,E024)";
        public const string RepositoryUniqueID = "(0040,E030)";
        public const string HomeCommunityID = "(0040,E031)";
        public const string DocumentTitle = "(0042,0010)";
        public const string EncapsulatedDocument = "(0042,0011)";
        public const string MIMETypeofEncapsulatedDocument = "(0042,0012)";
        public const string SourceInstanceSequence = "(0042,0013)";
        public const string ListofMIMETypes = "(0042,0014)";
        public const string ProductPackageIdentifier = "(0044,0001)";
        public const string SubstanceAdministrationApproval = "(0044,0002)";
        public const string ApprovalStatusFurtherDescription = "(0044,0003)";
        public const string ApprovalStatusDateTime = "(0044,0004)";
        public const string ProductTypeCodeSequence = "(0044,0007)";
        public const string ProductName = "(0044,0008)";
        public const string ProductDescription = "(0044,0009)";
        public const string ProductLotIdentifier = "(0044,000A)";
        public const string ProductExpirationDateTime = "(0044,000B)";
        public const string SubstanceAdministrationDateTime = "(0044,0010)";
        public const string SubstanceAdministrationNotes = "(0044,0011)";
        public const string SubstanceAdministrationDeviceID = "(0044,0012)";
        public const string ProductParameterSequence = "(0044,0013)";
        public const string SubstanceAdministrationParameterSequence = "(0044,0019)";
        public const string LensDescription = "(0046,0012)";
        public const string RightLensSequence = "(0046,0014)";
        public const string LeftLensSequence = "(0046,0015)";
        public const string UnspecifiedLateralityLensSequence = "(0046,0016)";
        public const string CylinderSequence = "(0046,0018)";
        public const string PrismSequence = "(0046,0028)";
        public const string HorizontalPrismPower = "(0046,0030)";
        public const string HorizontalPrismBase = "(0046,0032)";
        public const string VerticalPrismPower = "(0046,0034)";
        public const string VerticalPrismBase = "(0046,0036)";
        public const string LensSegmentType = "(0046,0038)";
        public const string OpticalTransmittance = "(0046,0040)";
        public const string ChannelWidth = "(0046,0042)";
        public const string PupilSize = "(0046,0044)";
        public const string CornealSize = "(0046,0046)";
        public const string AutorefractionRightEyeSequence = "(0046,0050)";
        public const string AutorefractionLeftEyeSequence = "(0046,0052)";
        public const string DistancePupillary = "(0046,0060)";
        public const string NearPupillaryDistance = "(0046,0062)";
        public const string IntermediatePupillaryDistance = "(0046,0063)";
        public const string OtherPupillaryDistance = "(0046,0064)";
        public const string KeratometryRightEyeSequence = "(0046,0070)";
        public const string KeratometryLeftEyeSequence = "(0046,0071)";
        public const string SteepKeratometricAxisSequence = "(0046,0074)";
        public const string RadiusofCurvature = "(0046,0075)";
        public const string KeratometricPower = "(0046,0076)";
        public const string KeratometricAxis = "(0046,0077)";
        public const string FlatKeratometricAxisSequence = "(0046,0080)";
        public const string BackgroundColor = "(0046,0092)";
        public const string Optotype = "(0046,0094)";
        public const string OptotypePresentation = "(0046,0095)";
        public const string SubjectiveRefractionRightEyeSequence = "(0046,0097)";
        public const string SubjectiveRefractionLeftEyeSequence = "(0046,0098)";
        public const string AddNearSequence = "(0046,0100)";
        public const string AddIntermediateSequence = "(0046,0101)";
        public const string AddOtherSequence = "(0046,0102)";
        public const string AddPower = "(0046,0104)";
        public const string ViewingDistance = "(0046,0106)";
        public const string VisualAcuityTypeCodeSequence = "(0046,0121)";
        public const string VisualAcuityRightEyeSequence = "(0046,0122)";
        public const string VisualAcuityLeftEyeSequence = "(0046,0123)";
        public const string VisualAcuityBothEyesOpenSequence = "(0046,0124)";
        public const string ViewingDistanceType = "(0046,0125)";
        public const string VisualAcuityModifiers = "(0046,0135)";
        public const string DecimalVisualAcuity = "(0046,0137)";
        public const string OptotypeDetailedDefinition = "(0046,0139)";
        public const string ReferencedRefractiveMeasurementsSequence = "(0046,0145)";
        public const string SpherePower = "(0046,0146)";
        public const string CylinderPower = "(0046,0147)";
        public const string ImagedVolumeWidth = "(0048,0001)";
        public const string ImagedVolumeHeight = "(0048,0002)";
        public const string ImagedVolumeDepth = "(0048,0003)";
        public const string TotalPixelMatrixColumns = "(0048,0006)";
        public const string TotalPixelMatrixRows = "(0048,0007)";
        public const string TotalPixelMatrixOriginSequence = "(0048,0008)";
        public const string SpecimenLabelinImage = "(0048,0010)";
        public const string FocusMethod = "(0048,0011)";
        public const string ExtendedDepthofField = "(0048,0012)";
        public const string NumberofFocalPlanes = "(0048,0013)";
        public const string DistanceBetweenFocalPlanes = "(0048,0014)";
        public const string RecommendedAbsentPixelCIELabValue = "(0048,0015)";
        public const string IlluminatorTypeCodeSequence = "(0048,0100)";
        public const string ImageOrientationSlide = "(0048,0102)";
        public const string OpticalPathSequence = "(0048,0105)";
        public const string OpticalPathIdentifier = "(0048,0106)";
        public const string OpticalPathDescription = "(0048,0107)";
        public const string IlluminationColorCodeSequence = "(0048,0108)";
        public const string SpecimenReferenceSequence = "(0048,0110)";
        public const string CondenserLensPower = "(0048,0111)";
        public const string ObjectiveLensPower = "(0048,0112)";
        public const string ObjectiveLensNumericalAperture = "(0048,0113)";
        public const string PaletteColorLookupTableSequence = "(0048,0120)";
        public const string ReferencedImageNavigationSequence = "(0048,0200)";
        public const string TopLeftHandCornerofLocalizerArea = "(0048,0201)";
        public const string BottomRightHandCornerofLocalizerArea = "(0048,0202)";
        public const string OpticalPathIdentificationSequence = "(0048,0207)";
        public const string PlanePositionSlideSequence = "(0048,021A)";
        public const string RowPositionInTotalImagePixelMatrix = "(0048,021E)";
        public const string ColumnPositionInTotalImagePixelMatrix = "(0048,021F)";
        public const string PixelOriginInterpretation = "(0048,0301)";
        public const string CalibrationImage = "(0050,0004)";
        public const string DeviceSequence = "(0050,0010)";
        public const string ContainerComponentTypeCodeSequence = "(0050,0012)";
        public const string ContainerComponentThickness = "(0050,0013)";
        public const string DeviceLength = "(0050,0014)";
        public const string ContainerComponentWidth = "(0050,0015)";
        public const string DeviceDiameter = "(0050,0016)";
        public const string DeviceDiameterUnits = "(0050,0017)";
        public const string DeviceVolume = "(0050,0018)";
        public const string InterMarkerDistance = "(0050,0019)";
        public const string ContainerComponentMaterial = "(0050,001A)";
        public const string ContainerComponentID = "(0050,001B)";
        public const string ContainerComponentLength = "(0050,001C)";
        public const string ContainerComponentDiameter = "(0050,001D)";
        public const string ContainerComponentDescription = "(0050,001E)";
        public const string DeviceDescription = "(0050,0020)";
        public const string ContrastBolusIngredientPercentbyVolume = "(0052,0001)";
        public const string OCTFocalDistance = "(0052,0002)";
        public const string BeamSpotSize = "(0052,0003)";
        public const string EffectiveRefractiveIndex = "(0052,0004)";
        public const string OCTAcquisitionDomain = "(0052,0006)";
        public const string OCTOpticalCenterWavelength = "(0052,0007)";
        public const string AxialResolution = "(0052,0008)";
        public const string RangingDepth = "(0052,0009)";
        public const string AlineRate = "(0052,0011)";
        public const string AlinesPerFrame = "(0052,0012)";
        public const string CatheterRotationalRate = "(0052,0013)";
        public const string AlinePixelSpacing = "(0052,0014)";
        public const string ModeofPercutaneousAccessSequence = "(0052,0016)";
        public const string IntravascularOCTFrameTypeSequence = "(0052,0025)";
        public const string OCTZOffsetApplied = "(0052,0026)";
        public const string IntravascularFrameContentSequence = "(0052,0027)";
        public const string IntravascularLongitudinalDistance = "(0052,0028)";
        public const string IntravascularOCTFrameContentSequence = "(0052,0029)";
        public const string OCTZOffsetCorrection = "(0052,0030)";
        public const string CatheterDirectionofRotation = "(0052,0031)";
        public const string SeamLineLocation = "(0052,0033)";
        public const string FirstAlineLocation = "(0052,0034)";
        public const string SeamLineIndex = "(0052,0036)";
        public const string NumberofPaddedAlines = "(0052,0038)";
        public const string InterpolationType = "(0052,0039)";
        public const string RefractiveIndexApplied = "(0052,003A)";
        public const string EnergyWindowVector = "(0054,0010)";
        public const string NumberofEnergyWindows = "(0054,0011)";
        public const string EnergyWindowInformationSequence = "(0054,0012)";
        public const string EnergyWindowRangeSequence = "(0054,0013)";
        public const string EnergyWindowLowerLimit = "(0054,0014)";
        public const string EnergyWindowUpperLimit = "(0054,0015)";
        public const string RadiopharmaceuticalInformationSequence = "(0054,0016)";
        public const string ResidualSyringeCounts = "(0054,0017)";
        public const string EnergyWindowName = "(0054,0018)";
        public const string DetectorVector = "(0054,0020)";
        public const string NumberofDetectors = "(0054,0021)";
        public const string DetectorInformationSequence = "(0054,0022)";
        public const string PhaseVector = "(0054,0030)";
        public const string NumberofPhases = "(0054,0031)";
        public const string PhaseInformationSequence = "(0054,0032)";
        public const string NumberofFramesinPhase = "(0054,0033)";
        public const string PhaseDelay = "(0054,0036)";
        public const string PauseBetweenFrames = "(0054,0038)";
        public const string PhaseDescription = "(0054,0039)";
        public const string RotationVector = "(0054,0050)";
        public const string NumberofRotations = "(0054,0051)";
        public const string RotationInformationSequence = "(0054,0052)";
        public const string NumberofFramesinRotation = "(0054,0053)";
        public const string RRIntervalVector = "(0054,0060)";
        public const string NumberofRRIntervals = "(0054,0061)";
        public const string GatedInformationSequence = "(0054,0062)";
        public const string DataInformationSequence = "(0054,0063)";
        public const string TimeSlotVector = "(0054,0070)";
        public const string NumberofTimeSlots = "(0054,0071)";
        public const string TimeSlotInformationSequence = "(0054,0072)";
        public const string TimeSlot = "(0054,0073)";
        public const string SliceVector = "(0054,0080)";
        public const string NumberofSlices = "(0054,0081)";
        public const string AngularViewVector = "(0054,0090)";
        public const string TimeSliceVector = "(0054,0100)";
        public const string NumberofTimeSlices = "(0054,0101)";
        public const string StartAngle = "(0054,0200)";
        public const string TypeofDetectorMotion = "(0054,0202)";
        public const string TriggerVector = "(0054,0210)";
        public const string NumberofTriggersinPhase = "(0054,0211)";
        public const string ViewCodeSequence = "(0054,0220)";
        public const string ViewModifierCodeSequence = "(0054,0222)";
        public const string RadionuclideCodeSequence = "(0054,0300)";
        public const string AdministrationRouteCodeSequence = "(0054,0302)";
        public const string RadiopharmaceuticalCodeSequence = "(0054,0304)";
        public const string CalibrationDataSequence = "(0054,0306)";
        public const string EnergyWindowNumber = "(0054,0308)";
        public const string ImageID = "(0054,0400)";
        public const string PatientOrientationCodeSequence = "(0054,0410)";
        public const string PatientOrientationModifierCodeSequence = "(0054,0412)";
        public const string PatientGantryRelationshipCodeSequence = "(0054,0414)";
        public const string SliceProgressionDirection = "(0054,0500)";
        public const string SeriesType = "(0054,1000)";
        public const string Units = "(0054,1001)";
        public const string CountsSource = "(0054,1002)";
        public const string ReprojectionMethod = "(0054,1004)";
        public const string SUVType = "(0054,1006)";
        public const string RandomsCorrectionMethod = "(0054,1100)";
        public const string AttenuationCorrectionMethod = "(0054,1101)";
        public const string DecayCorrection = "(0054,1102)";
        public const string ReconstructionMethod = "(0054,1103)";
        public const string DetectorLinesofResponseUsed = "(0054,1104)";
        public const string ScatterCorrectionMethod = "(0054,1105)";
        public const string AxialAcceptance = "(0054,1200)";
        public const string AxialMash = "(0054,1201)";
        public const string TransverseMash = "(0054,1202)";
        public const string DetectorElementSize = "(0054,1203)";
        public const string CoincidenceWindowWidth = "(0054,1210)";
        public const string SecondaryCountsType = "(0054,1220)";
        public const string FrameReferenceTime = "(0054,1300)";
        public const string PrimaryPromptsCountsAccumulated = "(0054,1310)";
        public const string SecondaryCountsAccumulated = "(0054,1311)";
        public const string SliceSensitivityFactor = "(0054,1320)";
        public const string DecayFactor = "(0054,1321)";
        public const string DoseCalibrationFactor = "(0054,1322)";
        public const string ScatterFractionFactor = "(0054,1323)";
        public const string DeadTimeFactor = "(0054,1324)";
        public const string ImageIndex = "(0054,1330)";
        public const string CountsIncluded = "(0054,1400)";
        public const string DeadTimeCorrectionFlag = "(0054,1401)";
        public const string HistogramSequence = "(0060,3000)";
        public const string HistogramNumberofBins = "(0060,3002)";
        public const string HistogramFirstBinValue = "(0060,3004)";
        public const string HistogramLastBinValue = "(0060,3006)";
        public const string HistogramBinWidth = "(0060,3008)";
        public const string HistogramExplanation = "(0060,3010)";
        public const string HistogramData = "(0060,3020)";
        public const string SegmentationType = "(0062,0001)";
        public const string SegmentSequence = "(0062,0002)";
        public const string SegmentedPropertyCategoryCodeSequence = "(0062,0003)";
        public const string SegmentNumber = "(0062,0004)";
        public const string SegmentLabel = "(0062,0005)";
        public const string SegmentDescription = "(0062,0006)";
        public const string SegmentAlgorithmType = "(0062,0008)";
        public const string SegmentAlgorithmName = "(0062,0009)";
        public const string SegmentIdentificationSequence = "(0062,000A)";
        public const string ReferencedSegmentNumber = "(0062,000B)";
        public const string RecommendedDisplayGrayscaleValue = "(0062,000C)";
        public const string RecommendedDisplayCIELabValue = "(0062,000D)";
        public const string MaximumFractionalValue = "(0062,000E)";
        public const string SegmentedPropertyTypeCodeSequence = "(0062,000F)";
        public const string SegmentationFractionalType = "(0062,0010)";
        public const string DeformableRegistrationSequence = "(0064,0002)";
        public const string SourceFrameofReferenceUID = "(0064,0003)";
        public const string DeformableRegistrationGridSequence = "(0064,0005)";
        public const string GridDimensions = "(0064,0007)";
        public const string GridResolution = "(0064,0008)";
        public const string VectorGridData = "(0064,0009)";
        public const string PreDeformationMatrixRegistrationSequence = "(0064,000F)";
        public const string PostDeformationMatrixRegistrationSequence = "(0064,0010)";
        public const string NumberofSurfaces = "(0066,0001)";
        public const string SurfaceSequence = "(0066,0002)";
        public const string SurfaceNumber = "(0066,0003)";
        public const string SurfaceComments = "(0066,0004)";
        public const string SurfaceProcessing = "(0066,0009)";
        public const string SurfaceProcessingRatio = "(0066,000A)";
        public const string SurfaceProcessingDescription = "(0066,000B)";
        public const string RecommendedPresentationOpacity = "(0066,000C)";
        public const string RecommendedPresentationType = "(0066,000D)";
        public const string FiniteVolume = "(0066,000E)";
        public const string Manifold = "(0066,0010)";
        public const string SurfacePointsSequence = "(0066,0011)";
        public const string SurfacePointsNormalsSequence = "(0066,0012)";
        public const string SurfaceMeshPrimitivesSequence = "(0066,0013)";
        public const string NumberofSurfacePoints = "(0066,0015)";
        public const string PointCoordinatesData = "(0066,0016)";
        public const string PointPositionAccuracy = "(0066,0017)";
        public const string MeanPointDistance = "(0066,0018)";
        public const string MaximumPointDistance = "(0066,0019)";
        public const string PointsBoundingBoxCoordinates = "(0066,001A)";
        public const string AxisofRotation = "(0066,001B)";
        public const string CenterofRotation = "(0066,001C)";
        public const string NumberofVectors = "(0066,001E)";
        public const string VectorDimensionality = "(0066,001F)";
        public const string VectorAccuracy = "(0066,0020)";
        public const string VectorCoordinateData = "(0066,0021)";
        public const string TrianglePointIndexList = "(0066,0023)";
        public const string EdgePointIndexList = "(0066,0024)";
        public const string VertexPointIndexList = "(0066,0025)";
        public const string TriangleStripSequence = "(0066,0026)";
        public const string TriangleFanSequence = "(0066,0027)";
        public const string LineSequence = "(0066,0028)";
        public const string PrimitivePointIndexList = "(0066,0029)";
        public const string SurfaceCount = "(0066,002A)";
        public const string ReferencedSurfaceSequence = "(0066,002B)";
        public const string ReferencedSurfaceNumber = "(0066,002C)";
        public const string SegmentSurfaceGenerationAlgorithmIdentificationSequence = "(0066,002D)";
        public const string SegmentSurfaceSourceInstanceSequence = "(0066,002E)";
        public const string AlgorithmFamilyCodeSequence = "(0066,002F)";
        public const string AlgorithmNameCodeSequence = "(0066,0030)";
        public const string AlgorithmVersion = "(0066,0031)";
        public const string AlgorithmParameters = "(0066,0032)";
        public const string FacetSequence = "(0066,0034)";
        public const string SurfaceProcessingAlgorithmIdentificationSequence = "(0066,0035)";
        public const string AlgorithmName = "(0066,0036)";
        public const string ImplantSize = "(0068,6210)";
        public const string ImplantTemplateVersion = "(0068,6221)";
        public const string ReplacedImplantTemplateSequence = "(0068,6222)";
        public const string ImplantType = "(0068,6223)";
        public const string DerivationImplantTemplateSequence = "(0068,6224)";
        public const string OriginalImplantTemplateSequence = "(0068,6225)";
        public const string EffectiveDateTime = "(0068,6226)";
        public const string ImplantTargetAnatomySequence = "(0068,6230)";
        public const string InformationFromManufacturerSequence = "(0068,6260)";
        public const string NotificationFromManufacturerSequence = "(0068,6265)";
        public const string InformationIssueDateTime = "(0068,6270)";
        public const string InformationSummary = "(0068,6280)";
        public const string ImplantRegulatoryDisapprovalCodeSequence = "(0068,62A0)";
        public const string OverallTemplateSpatialTolerance = "(0068,62A5)";
        public const string HPGLDocumentSequence = "(0068,62C0)";
        public const string HPGLDocumentID = "(0068,62D0)";
        public const string HPGLDocumentLabel = "(0068,62D5)";
        public const string ViewOrientationCodeSequence = "(0068,62E0)";
        public const string ViewOrientationModifier = "(0068,62F0)";
        public const string HPGLDocumentScaling = "(0068,62F2)";
        public const string HPGLDocument = "(0068,6300)";
        public const string HPGLContourPenNumber = "(0068,6310)";
        public const string HPGLPenSequence = "(0068,6320)";
        public const string HPGLPenNumber = "(0068,6330)";
        public const string HPGLPenLabel = "(0068,6340)";
        public const string HPGLPenDescription = "(0068,6345)";
        public const string RecommendedRotationPoint = "(0068,6346)";
        public const string BoundingRectangle = "(0068,6347)";
        public const string ImplantTemplateThreeDModelSurfaceNumber = "(0068,6350)";
        public const string SurfaceModelDescriptionSequence = "(0068,6360)";
        public const string SurfaceModelLabel = "(0068,6380)";
        public const string SurfaceModelScalingFactor = "(0068,6390)";
        public const string MaterialsCodeSequence = "(0068,63A0)";
        public const string CoatingMaterialsCodeSequence = "(0068,63A4)";
        public const string ImplantTypeCodeSequence = "(0068,63A8)";
        public const string FixationMethodCodeSequence = "(0068,63AC)";
        public const string MatingFeatureSetsSequence = "(0068,63B0)";
        public const string MatingFeatureSetID = "(0068,63C0)";
        public const string MatingFeatureSetLabel = "(0068,63D0)";
        public const string MatingFeatureSequence = "(0068,63E0)";
        public const string MatingFeatureID = "(0068,63F0)";
        public const string MatingFeatureDegreeofFreedomSequence = "(0068,6400)";
        public const string DegreeofFreedomID = "(0068,6410)";
        public const string DegreeofFreedomType = "(0068,6420)";
        public const string TwoDMatingFeatureCoordinatesSequence = "(0068,6430)";
        public const string ReferencedHPGLDocumentID = "(0068,6440)";
        public const string TwoDMatingPoint = "(0068,6450)";
        public const string TwoDMatingAxes = "(0068,6460)";
        public const string TwoDDegreeofFreedomSequence = "(0068,6470)";
        public const string ThreeDDegreeofFreedomAxis = "(0068,6490)";
        public const string RangeofFreedom = "(0068,64A0)";
        public const string ThreeDMatingPoint = "(0068,64C0)";
        public const string ThreeDMatingAxesThreeDMatingAxesFD9 = "(0068,64D0)";
        public const string TwoDDegreeofFreedomAxis = "(0068,64F0)";
        public const string PlanningLandmarkPointSequence = "(0068,6500)";
        public const string PlanningLandmarkLineSequence = "(0068,6510)";
        public const string PlanningLandmarkPlaneSequence = "(0068,6520)";
        public const string PlanningLandmarkID = "(0068,6530)";
        public const string PlanningLandmarkDescription = "(0068,6540)";
        public const string PlanningLandmarkIdentificationCodeSequence = "(0068,6545)";
        public const string TwoDPointCoordinatesSequence = "(0068,6550)";
        public const string TwoDPointCoordinates = "(0068,6560)";
        public const string ThreeDPointCoordinates = "(0068,6590)";
        public const string TwoDLineCoordinatesSequence = "(0068,65A0)";
        public const string TwoDLineCoordinates = "(0068,65B0)";
        public const string ThreeDLineCoordinates = "(0068,65D0)";
        public const string TwoDPlaneCoordinatesSequence = "(0068,65E0)";
        public const string TwoDPlaneIntersection = "(0068,65F0)";
        public const string ThreeDPlaneOrigin = "(0068,6610)";
        public const string ThreeDPlaneNormal = "(0068,6620)";
        public const string GraphicAnnotationSequence = "(0070,0001)";
        public const string GraphicLayer = "(0070,0002)";
        public const string BoundingBoxAnnotationUnits = "(0070,0003)";
        public const string AnchorPointAnnotationUnits = "(0070,0004)";
        public const string GraphicAnnotationUnits = "(0070,0005)";
        public const string UnformattedTextValue = "(0070,0006)";
        public const string TextObjectSequence = "(0070,0008)";
        public const string GraphicObjectSequence = "(0070,0009)";
        public const string BoundingBoxTopLeftHandCorner = "(0070,0010)";
        public const string BoundingBoxBottomRightHandCorner = "(0070,0011)";
        public const string BoundingBoxTextHorizontalJustification = "(0070,0012)";
        public const string AnchorPoint = "(0070,0014)";
        public const string AnchorPointVisibility = "(0070,0015)";
        public const string GraphicDimensions = "(0070,0020)";
        public const string NumberofGraphicPoints = "(0070,0021)";
        public const string GraphicData = "(0070,0022)";
        public const string GraphicType = "(0070,0023)";
        public const string GraphicFilled = "(0070,0024)";
        public const string ImageRotationRetired = "(0070,0040)";
        public const string ImageHorizontalFlip = "(0070,0041)";
        public const string ImageRotation = "(0070,0042)";
        public const string DisplayedAreaTopLeftHandCornerTrial = "(0070,0050)";
        public const string DisplayedAreaBottomRightHandCornerTrial = "(0070,0051)";
        public const string DisplayedAreaTopLeftHandCorner = "(0070,0052)";
        public const string DisplayedAreaBottomRightHandCorner = "(0070,0053)";
        public const string DisplayedAreaSelectionSequence = "(0070,005A)";
        public const string GraphicLayerSequence = "(0070,0060)";
        public const string GraphicLayerOrder = "(0070,0062)";
        public const string GraphicLayerRecommendedDisplayGrayscaleValue = "(0070,0066)";
        public const string GraphicLayerRecommendedDisplayRGBValue = "(0070,0067)";
        public const string GraphicLayerDescription = "(0070,0068)";
        public const string ContentLabel = "(0070,0080)";
        public const string ContentDescription = "(0070,0081)";
        public const string PresentationCreationDate = "(0070,0082)";
        public const string PresentationCreationTime = "(0070,0083)";
        public const string ContentCreatorName = "(0070,0084)";
        public const string ContentCreatorIdentificationCodeSequence = "(0070,0086)";
        public const string AlternateContentDescriptionSequence = "(0070,0087)";
        public const string PresentationSizeMode = "(0070,0100)";
        public const string PresentationPixelSpacing = "(0070,0101)";
        public const string PresentationPixelAspectRatio = "(0070,0102)";
        public const string PresentationPixelMagnificationRatio = "(0070,0103)";
        public const string GraphicGroupLabel = "(0070,0207)";
        public const string GraphicGroupDescription = "(0070,0208)";
        public const string CompoundGraphicSequence = "(0070,0209)";
        public const string CompoundGraphicInstanceID = "(0070,0226)";
        public const string FontName = "(0070,0227)";
        public const string FontNameType = "(0070,0228)";
        public const string CSSFontName = "(0070,0229)";
        public const string RotationAngle = "(0070,0230)";
        public const string TextStyleSequence = "(0070,0231)";
        public const string LineStyleSequence = "(0070,0232)";
        public const string FillStyleSequence = "(0070,0233)";
        public const string GraphicGroupSequence = "(0070,0234)";
        public const string TextColorCIELabValue = "(0070,0241)";
        public const string HorizontalAlignment = "(0070,0242)";
        public const string VerticalAlignment = "(0070,0243)";
        public const string ShadowStyle = "(0070,0244)";
        public const string ShadowOffsetX = "(0070,0245)";
        public const string ShadowOffsetY = "(0070,0246)";
        public const string ShadowColorCIELabValue = "(0070,0247)";
        public const string Underlined = "(0070,0248)";
        public const string Bold = "(0070,0249)";
        public const string Italic = "(0070,0250)";
        public const string PatternOnColorCIELabValue = "(0070,0251)";
        public const string PatternOffColorCIELabValue = "(0070,0252)";
        public const string LineThickness = "(0070,0253)";
        public const string LineDashingStyle = "(0070,0254)";
        public const string LinePattern = "(0070,0255)";
        public const string FillPattern = "(0070,0256)";
        public const string FillMode = "(0070,0257)";
        public const string ShadowOpacity = "(0070,0258)";
        public const string GapLength = "(0070,0261)";
        public const string DiameterofVisibility = "(0070,0262)";
        public const string RotationPoint = "(0070,0273)";
        public const string TickAlignment = "(0070,0274)";
        public const string ShowTickLabel = "(0070,0278)";
        public const string TickLabelAlignment = "(0070,0279)";
        public const string CompoundGraphicUnits = "(0070,0282)";
        public const string PatternOnOpacity = "(0070,0284)";
        public const string PatternOffOpacity = "(0070,0285)";
        public const string MajorTicksSequence = "(0070,0287)";
        public const string TickPosition = "(0070,0288)";
        public const string TickLabel = "(0070,0289)";
        public const string CompoundGraphicType = "(0070,0294)";
        public const string GraphicGroupID = "(0070,0295)";
        public const string ShapeType = "(0070,0306)";
        public const string RegistrationSequence = "(0070,0308)";
        public const string MatrixRegistrationSequence = "(0070,0309)";
        public const string MatrixSequence = "(0070,030A)";
        public const string FrameofReferenceTransformationMatrixType = "(0070,030C)";
        public const string RegistrationTypeCodeSequence = "(0070,030D)";
        public const string FiducialDescription = "(0070,030F)";
        public const string FiducialIdentifier = "(0070,0310)";
        public const string FiducialIdentifierCodeSequence = "(0070,0311)";
        public const string ContourUncertaintyRadius = "(0070,0312)";
        public const string UsedFiducialsSequence = "(0070,0314)";
        public const string GraphicCoordinatesDataSequence = "(0070,0318)";
        public const string FiducialUID = "(0070,031A)";
        public const string FiducialSetSequence = "(0070,031C)";
        public const string FiducialSequence = "(0070,031E)";
        public const string GraphicLayerRecommendedDisplayCIELabValue = "(0070,0401)";
        public const string BlendingSequence = "(0070,0402)";
        public const string RelativeOpacity = "(0070,0403)";
        public const string ReferencedSpatialRegistrationSequence = "(0070,0404)";
        public const string BlendingPosition = "(0070,0405)";
        public const string HangingProtocolName = "(0072,0002)";
        public const string HangingProtocolDescription = "(0072,0004)";
        public const string HangingProtocolLevel = "(0072,0006)";
        public const string HangingProtocolCreator = "(0072,0008)";
        public const string HangingProtocolCreationDateTime = "(0072,000A)";
        public const string HangingProtocolDefinitionSequence = "(0072,000C)";
        public const string HangingProtocolUserIdentificationCodeSequence = "(0072,000E)";
        public const string HangingProtocolUserGroupName = "(0072,0010)";
        public const string SourceHangingProtocolSequence = "(0072,0012)";
        public const string NumberofPriorsReferenced = "(0072,0014)";
        public const string ImageSetsSequence = "(0072,0020)";
        public const string ImageSetSelectorSequence = "(0072,0022)";
        public const string ImageSetSelectorUsageFlag = "(0072,0024)";
        public const string SelectorAttribute = "(0072,0026)";
        public const string SelectorValueNumber = "(0072,0028)";
        public const string TimeBasedImageSetsSequence = "(0072,0030)";
        public const string ImageSetNumber = "(0072,0032)";
        public const string ImageSetSelectorCategory = "(0072,0034)";
        public const string RelativeTime = "(0072,0038)";
        public const string RelativeTimeUnits = "(0072,003A)";
        public const string AbstractPriorValue = "(0072,003C)";
        public const string AbstractPriorCodeSequence = "(0072,003E)";
        public const string ImageSetLabel = "(0072,0040)";
        public const string SelectorAttributeVR = "(0072,0050)";
        public const string SelectorSequencePointer = "(0072,0052)";
        public const string SelectorSequencePointerPrivateCreator = "(0072,0054)";
        public const string SelectorAttributePrivateCreator = "(0072,0056)";
        public const string SelectorATValue = "(0072,0060)";
        public const string SelectorCSValue = "(0072,0062)";
        public const string SelectorISValue = "(0072,0064)";
        public const string SelectorLOValue = "(0072,0066)";
        public const string SelectorLTValue = "(0072,0068)";
        public const string SelectorPNValue = "(0072,006A)";
        public const string SelectorSHValue = "(0072,006C)";
        public const string SelectorSTValue = "(0072,006E)";
        public const string SelectorUTValue = "(0072,0070)";
        public const string SelectorDSValue = "(0072,0072)";
        public const string SelectorFDValue = "(0072,0074)";
        public const string SelectorFLValue = "(0072,0076)";
        public const string SelectorULValue = "(0072,0078)";
        public const string SelectorUSValue = "(0072,007A)";
        public const string SelectorSLValue = "(0072,007C)";
        public const string SelectorSSValue = "(0072,007E)";
        public const string SelectorCodeSequenceValue = "(0072,0080)";
        public const string NumberofScreens = "(0072,0100)";
        public const string NominalScreenDefinitionSequence = "(0072,0102)";
        public const string NumberofVerticalPixels = "(0072,0104)";
        public const string NumberofHorizontalPixels = "(0072,0106)";
        public const string DisplayEnvironmentSpatialPosition = "(0072,0108)";
        public const string ScreenMinimumGrayscaleBitDepth = "(0072,010A)";
        public const string ScreenMinimumColorBitDepth = "(0072,010C)";
        public const string ApplicationMaximumRepaintTime = "(0072,010E)";
        public const string DisplaySetsSequence = "(0072,0200)";
        public const string DisplaySetNumber = "(0072,0202)";
        public const string DisplaySetLabel = "(0072,0203)";
        public const string DisplaySetPresentationGroup = "(0072,0204)";
        public const string DisplaySetPresentationGroupDescription = "(0072,0206)";
        public const string PartialDataDisplayHandling = "(0072,0208)";
        public const string SynchronizedScrollingSequence = "(0072,0210)";
        public const string DisplaySetScrollingGroup = "(0072,0212)";
        public const string NavigationIndicatorSequence = "(0072,0214)";
        public const string NavigationDisplaySet = "(0072,0216)";
        public const string ReferenceDisplaySets = "(0072,0218)";
        public const string ImageBoxesSequence = "(0072,0300)";
        public const string ImageBoxNumber = "(0072,0302)";
        public const string ImageBoxLayoutType = "(0072,0304)";
        public const string ImageBoxTileHorizontalDimension = "(0072,0306)";
        public const string ImageBoxTileVerticalDimension = "(0072,0308)";
        public const string ImageBoxScrollDirection = "(0072,0310)";
        public const string ImageBoxSmallScrollType = "(0072,0312)";
        public const string ImageBoxSmallScrollAmount = "(0072,0314)";
        public const string ImageBoxLargeScrollType = "(0072,0316)";
        public const string ImageBoxLargeScrollAmount = "(0072,0318)";
        public const string ImageBoxOverlapPriority = "(0072,0320)";
        public const string CineRelativetoRealTime = "(0072,0330)";
        public const string FilterOperationsSequence = "(0072,0400)";
        public const string FilterbyCategory = "(0072,0402)";
        public const string FilterbyAttributePresence = "(0072,0404)";
        public const string FilterbyOperator = "(0072,0406)";
        public const string StructuredDisplayBackgroundCIELabValue = "(0072,0420)";
        public const string EmptyImageBoxCIELabValue = "(0072,0421)";
        public const string StructuredDisplayImageBoxSequence = "(0072,0422)";
        public const string StructuredDisplayTextBoxSequence = "(0072,0424)";
        public const string ReferencedFirstFrameSequence = "(0072,0427)";
        public const string ImageBoxSynchronizationSequence = "(0072,0430)";
        public const string SynchronizedImageBoxList = "(0072,0432)";
        public const string TypeofSynchronization = "(0072,0434)";
        public const string BlendingOperationType = "(0072,0500)";
        public const string ReformattingOperationType = "(0072,0510)";
        public const string ReformattingThickness = "(0072,0512)";
        public const string ReformattingInterval = "(0072,0514)";
        public const string ReformattingOperationInitialViewDirection = "(0072,0516)";
        public const string ThreeDRenderingType = "(0072,0520)";
        public const string SortingOperationsSequence = "(0072,0600)";
        public const string SortbyCategory = "(0072,0602)";
        public const string SortingDirection = "(0072,0604)";
        public const string DisplaySetPatientOrientation = "(0072,0700)";
        public const string VOIType = "(0072,0702)";
        public const string PseudoColorType = "(0072,0704)";
        public const string PseudoColorPaletteInstanceReferenceSequence = "(0072,0705)";
        public const string ShowGrayscaleInverted = "(0072,0706)";
        public const string ShowImageTrueSizeFlag = "(0072,0710)";
        public const string ShowGraphicAnnotationFlag = "(0072,0712)";
        public const string ShowPatientDemographicsFlag = "(0072,0714)";
        public const string ShowAcquisitionTechniquesFlag = "(0072,0716)";
        public const string DisplaySetHorizontalJustification = "(0072,0717)";
        public const string DisplaySetVerticalJustification = "(0072,0718)";
        public const string ContinuationStartMeterset = "(0074,0120)";
        public const string ContinuationEndMeterset = "(0074,0121)";
        public const string ProcedureStepState = "(0074,1000)";
        public const string ProcedureStepProgressInformationSequence = "(0074,1002)";
        public const string ProcedureStepProgress = "(0074,1004)";
        public const string ProcedureStepProgressDescription = "(0074,1006)";
        public const string ProcedureStepCommunicationsURISequence = "(0074,1008)";
        public const string ContactURI = "(0074,100A)";
        public const string ContactDisplayName = "(0074,100C)";
        public const string ProcedureStepDiscontinuationReasonCodeSequence = "(0074,100E)";
        public const string BeamTaskSequence = "(0074,1020)";
        public const string BeamTaskType = "(0074,1022)";
        public const string BeamOrderIndexTrial = "(0074,1024)";
        public const string TableTopVerticalAdjustedPosition = "(0074,1026)";
        public const string TableTopLongitudinalAdjustedPosition = "(0074,1027)";
        public const string TableTopLateralAdjustedPosition = "(0074,1028)";
        public const string PatientSupportAdjustedAngle = "(0074,102A)";
        public const string TableTopEccentricAdjustedAngle = "(0074,102B)";
        public const string TableTopPitchAdjustedAngle = "(0074,102C)";
        public const string TableTopRollAdjustedAngle = "(0074,102D)";
        public const string DeliveryVerificationImageSequence = "(0074,1030)";
        public const string VerificationImageTiming = "(0074,1032)";
        public const string DoubleExposureFlag = "(0074,1034)";
        public const string DoubleExposureOrdering = "(0074,1036)";
        public const string DoubleExposureMetersetTrial = "(0074,1038)";
        public const string DoubleExposureFieldDeltaTrial = "(0074,103A)";
        public const string RelatedReferenceRTImageSequence = "(0074,1040)";
        public const string GeneralMachineVerificationSequence = "(0074,1042)";
        public const string ConventionalMachineVerificationSequence = "(0074,1044)";
        public const string IonMachineVerificationSequence = "(0074,1046)";
        public const string FailedAttributesSequence = "(0074,1048)";
        public const string OverriddenAttributesSequence = "(0074,104A)";
        public const string ConventionalControlPointVerificationSequence = "(0074,104C)";
        public const string IonControlPointVerificationSequence = "(0074,104E)";
        public const string AttributeOccurrenceSequence = "(0074,1050)";
        public const string AttributeOccurrencePointer = "(0074,1052)";
        public const string AttributeItemSelector = "(0074,1054)";
        public const string AttributeOccurrencePrivateCreator = "(0074,1056)";
        public const string SelectorSequencePointerItems = "(0074,1057)";
        public const string ScheduledProcedureStepPriority = "(0074,1200)";
        public const string WorklistLabel = "(0074,1202)";
        public const string ProcedureStepLabel = "(0074,1204)";
        public const string ScheduledProcessingParametersSequence = "(0074,1210)";
        public const string PerformedProcessingParametersSequence = "(0074,1212)";
        public const string UnifiedProcedureStepPerformedProcedureSequence = "(0074,1216)";
        public const string RelatedProcedureStepSequence = "(0074,1220)";
        public const string ProcedureStepRelationshipType = "(0074,1222)";
        public const string ReplacedProcedureStepSequence = "(0074,1224)";
        public const string DeletionLock = "(0074,1230)";
        public const string ReceivingAE = "(0074,1234)";
        public const string RequestingAE = "(0074,1236)";
        public const string ReasonforCancellation = "(0074,1238)";
        public const string SCPStatus = "(0074,1242)";
        public const string SubscriptionListStatus = "(0074,1244)";
        public const string UnifiedProcedureStepListStatus = "(0074,1246)";
        public const string BeamOrderIndex = "(0074,1324)";
        public const string DoubleExposureMeterset = "(0074,1338)";
        public const string DoubleExposureFieldDelta = "(0074,133A)";
        public const string ImplantAssemblyTemplateName = "(0076,0001)";
        public const string ImplantAssemblyTemplateIssuer = "(0076,0003)";
        public const string ImplantAssemblyTemplateVersion = "(0076,0006)";
        public const string ReplacedImplantAssemblyTemplateSequence = "(0076,0008)";
        public const string ImplantAssemblyTemplateType = "(0076,000A)";
        public const string OriginalImplantAssemblyTemplateSequence = "(0076,000C)";
        public const string DerivationImplantAssemblyTemplateSequence = "(0076,000E)";
        public const string ImplantAssemblyTemplateTargetAnatomySequence = "(0076,0010)";
        public const string ProcedureTypeCodeSequence = "(0076,0020)";
        public const string SurgicalTechnique = "(0076,0030)";
        public const string ComponentTypesSequence = "(0076,0032)";
        public const string ComponentTypeCodeSequence = "(0076,0034)";
        public const string ExclusiveComponentType = "(0076,0036)";
        public const string MandatoryComponentType = "(0076,0038)";
        public const string ComponentSequence = "(0076,0040)";
        public const string ComponentID = "(0076,0055)";
        public const string ComponentAssemblySequence = "(0076,0060)";
        public const string Component1ReferencedID = "(0076,0070)";
        public const string Component1ReferencedMatingFeatureSetID = "(0076,0080)";
        public const string Component1ReferencedMatingFeatureID = "(0076,0090)";
        public const string Component2ReferencedID = "(0076,00A0)";
        public const string Component2ReferencedMatingFeatureSetID = "(0076,00B0)";
        public const string Component2ReferencedMatingFeatureID = "(0076,00C0)";
        public const string ImplantTemplateGroupName = "(0078,0001)";
        public const string ImplantTemplateGroupDescription = "(0078,0010)";
        public const string ImplantTemplateGroupIssuer = "(0078,0020)";
        public const string ImplantTemplateGroupVersion = "(0078,0024)";
        public const string ReplacedImplantTemplateGroupSequence = "(0078,0026)";
        public const string ImplantTemplateGroupTargetAnatomySequence = "(0078,0028)";
        public const string ImplantTemplateGroupMembersSequence = "(0078,002A)";
        public const string ImplantTemplateGroupMemberID = "(0078,002E)";
        public const string ThreeDImplantTemplateGroupMemberMatchingPoint = "(0078,0050)";
        public const string ThreeDImplantTemplateGroupMemberMatchingAxes = "(0078,0060)";
        public const string ImplantTemplateGroupMemberMatchingTwoDCoordinatesSequence = "(0078,0070)";
        public const string TwoDImplantTemplateGroupMemberMatchingPoint = "(0078,0090)";
        public const string TwoDImplantTemplateGroupMemberMatchingAxes = "(0078,00A0)";
        public const string ImplantTemplateGroupVariationDimensionSequence = "(0078,00B0)";
        public const string ImplantTemplateGroupVariationDimensionName = "(0078,00B2)";
        public const string ImplantTemplateGroupVariationDimensionRankSequence = "(0078,00B4)";
        public const string ReferencedImplantTemplateGroupMemberID = "(0078,00B6)";
        public const string ImplantTemplateGroupVariationDimensionRank = "(0078,00B8)";
        public const string StorageMediaFilesetID = "(0088,0130)";
        public const string StorageMediaFilesetUID = "(0088,0140)";
        public const string IconImageSequence = "(0088,0200)";
        public const string TopicTitle = "(0088,0904)";
        public const string TopicSubject = "(0088,0906)";
        public const string TopicAuthor = "(0088,0910)";
        public const string TopicKeywords = "(0088,0912)";
        public const string SOPInstanceStatus = "(0100,0410)";
        public const string SOPAuthorizationDateTime = "(0100,0420)";
        public const string SOPAuthorizationComment = "(0100,0424)";
        public const string AuthorizationEquipmentCertificationNumber = "(0100,0426)";
        public const string MACIDNumber = "(0400,0005)";
        public const string MACCalculationTransferSyntaxUID = "(0400,0010)";
        public const string MACAlgorithm = "(0400,0015)";
        public const string DataElementsSigned = "(0400,0020)";
        public const string DigitalSignatureUID = "(0400,0100)";
        public const string DigitalSignatureDateTime = "(0400,0105)";
        public const string CertificateType = "(0400,0110)";
        public const string CertificateofSigner = "(0400,0115)";
        public const string Signature = "(0400,0120)";
        public const string CertifiedTimestampType = "(0400,0305)";
        public const string CertifiedTimestamp = "(0400,0310)";
        public const string DigitalSignaturePurposeCodeSequence = "(0400,0401)";
        public const string ReferencedDigitalSignatureSequence = "(0400,0402)";
        public const string ReferencedSOPInstanceMACSequence = "(0400,0403)";
        public const string MAC = "(0400,0404)";
        public const string EncryptedAttributesSequence = "(0400,0500)";
        public const string EncryptedContentTransferSyntaxUID = "(0400,0510)";
        public const string EncryptedContent = "(0400,0520)";
        public const string ModifiedAttributesSequence = "(0400,0550)";
        public const string OriginalAttributesSequence = "(0400,0561)";
        public const string AttributeModificationDateTime = "(0400,0562)";
        public const string ModifyingSystem = "(0400,0563)";
        public const string SourceofPreviousValues = "(0400,0564)";
        public const string ReasonfortheAttributeModification = "(0400,0565)";
        public const string NumberofCopies = "(2000,0010)";
        public const string PrinterConfigurationSequence = "(2000,001E)";
        public const string PrintPriority = "(2000,0020)";
        public const string MediumType = "(2000,0030)";
        public const string FilmDestination = "(2000,0040)";
        public const string FilmSessionLabel = "(2000,0050)";
        public const string MemoryAllocation = "(2000,0060)";
        public const string MaximumMemoryAllocation = "(2000,0061)";
        public const string ColorImagePrintingFlag = "(2000,0062)";
        public const string CollationFlag = "(2000,0063)";
        public const string AnnotationFlag = "(2000,0065)";
        public const string ImageOverlayFlag = "(2000,0067)";
        public const string PresentationLUTFlag = "(2000,0069)";
        public const string ImageBoxPresentationLUTFlag = "(2000,006A)";
        public const string MemoryBitDepth = "(2000,00A0)";
        public const string PrintingBitDepth = "(2000,00A1)";
        public const string MediaInstalledSequence = "(2000,00A2)";
        public const string OtherMediaAvailableSequence = "(2000,00A4)";
        public const string SupportedImageDisplayFormatsSequence = "(2000,00A8)";
        public const string ReferencedFilmBoxSequence = "(2000,0500)";
        public const string ReferencedStoredPrintSequence = "(2000,0510)";
        public const string ImageDisplayFormat = "(2010,0010)";
        public const string AnnotationDisplayFormatID = "(2010,0030)";
        public const string FilmOrientation = "(2010,0040)";
        public const string FilmSizeID = "(2010,0050)";
        public const string PrinterResolutionID = "(2010,0052)";
        public const string DefaultPrinterResolutionID = "(2010,0054)";
        public const string MagnificationType = "(2010,0060)";
        public const string SmoothingType = "(2010,0080)";
        public const string DefaultMagnificationType = "(2010,00A6)";
        public const string OtherMagnificationTypesAvailable = "(2010,00A7)";
        public const string DefaultSmoothingType = "(2010,00A8)";
        public const string OtherSmoothingTypesAvailable = "(2010,00A9)";
        public const string BorderDensity = "(2010,0100)";
        public const string EmptyImageDensity = "(2010,0110)";
        public const string MinDensity = "(2010,0120)";
        public const string MaxDensity = "(2010,0130)";
        public const string Trim = "(2010,0140)";
        public const string ConfigurationInformation = "(2010,0150)";
        public const string ConfigurationInformationDescription = "(2010,0152)";
        public const string MaximumCollatedFilms = "(2010,0154)";
        public const string Illumination = "(2010,015E)";
        public const string ReflectedAmbientLight = "(2010,0160)";
        public const string PrinterPixelSpacing = "(2010,0376)";
        public const string ReferencedFilmSessionSequence = "(2010,0500)";
        public const string ReferencedImageBoxSequence = "(2010,0510)";
        public const string ReferencedBasicAnnotationBoxSequence = "(2010,0520)";
        public const string ImageBoxPosition = "(2020,0010)";
        public const string Polarity = "(2020,0020)";
        public const string RequestedImageSize = "(2020,0030)";
        public const string RequestedDecimateCropBehavior = "(2020,0040)";
        public const string RequestedResolutionID = "(2020,0050)";
        public const string RequestedImageSizeFlag = "(2020,00A0)";
        public const string DecimateCropResult = "(2020,00A2)";
        public const string BasicGrayscaleImageSequence = "(2020,0110)";
        public const string BasicColorImageSequence = "(2020,0111)";
        public const string ReferencedImageOverlayBoxSequence = "(2020,0130)";
        public const string ReferencedVOILUTBoxSequence = "(2020,0140)";
        public const string AnnotationPosition = "(2030,0010)";
        public const string TextString = "(2030,0020)";
        public const string ReferencedOverlayPlaneSequence = "(2040,0010)";
        public const string ReferencedOverlayPlaneGroups = "(2040,0011)";
        public const string OverlayPixelDataSequence = "(2040,0020)";
        public const string OverlayMagnificationType = "(2040,0060)";
        public const string OverlaySmoothingType = "(2040,0070)";
        public const string OverlayorImageMagnification = "(2040,0072)";
        public const string MagnifytoNumberofColumns = "(2040,0074)";
        public const string OverlayForegroundDensity = "(2040,0080)";
        public const string OverlayBackgroundDensity = "(2040,0082)";
        public const string OverlayMode = "(2040,0090)";
        public const string ThresholdDensity = "(2040,0100)";
        public const string ReferencedImageBoxSequenceRetired = "(2040,0500)";
        public const string PresentationLUTSequence = "(2050,0010)";
        public const string PresentationLUTShape = "(2050,0020)";
        public const string ReferencedPresentationLUTSequence = "(2050,0500)";
        public const string PrintJobID = "(2100,0010)";
        public const string ExecutionStatus = "(2100,0020)";
        public const string ExecutionStatusInfo = "(2100,0030)";
        public const string CreationDate = "(2100,0040)";
        public const string CreationTime = "(2100,0050)";
        public const string Originator = "(2100,0070)";
        public const string DestinationAE = "(2100,0140)";
        public const string OwnerID = "(2100,0160)";
        public const string NumberofFilms = "(2100,0170)";
        public const string ReferencedPrintJobSequencePullStoredPrint = "(2100,0500)";
        public const string PrinterStatus = "(2110,0010)";
        public const string PrinterStatusInfo = "(2110,0020)";
        public const string PrinterName = "(2110,0030)";
        public const string PrintQueueID = "(2110,0099)";
        public const string QueueStatus = "(2120,0010)";
        public const string PrintJobDescriptionSequence = "(2120,0050)";
        public const string ReferencedPrintJobSequence = "(2120,0070)";
        public const string PrintManagementCapabilitiesSequence = "(2130,0010)";
        public const string PrinterCharacteristicsSequence = "(2130,0015)";
        public const string FilmBoxContentSequence = "(2130,0030)";
        public const string ImageBoxContentSequence = "(2130,0040)";
        public const string AnnotationContentSequence = "(2130,0050)";
        public const string ImageOverlayBoxContentSequence = "(2130,0060)";
        public const string PresentationLUTContentSequence = "(2130,0080)";
        public const string ProposedStudySequence = "(2130,00A0)";
        public const string OriginalImageSequence = "(2130,00C0)";
        public const string LabelUsingInformationExtractedFromInstances = "(2200,0001)";
        public const string LabelText = "(2200,0002)";
        public const string LabelStyleSelection = "(2200,0003)";
        public const string MediaDisposition = "(2200,0004)";
        public const string BarcodeValue = "(2200,0005)";
        public const string BarcodeSymbology = "(2200,0006)";
        public const string AllowMediaSplitting = "(2200,0007)";
        public const string IncludeNonDICOMObjects = "(2200,0008)";
        public const string IncludeDisplayApplication = "(2200,0009)";
        public const string PreserveCompositeInstancesAfterMediaCreation = "(2200,000A)";
        public const string TotalNumberofPiecesofMediaCreated = "(2200,000B)";
        public const string RequestedMediaApplicationProfile = "(2200,000C)";
        public const string ReferencedStorageMediaSequence = "(2200,000D)";
        public const string FailureAttributes = "(2200,000E)";
        public const string AllowLossyCompression = "(2200,000F)";
        public const string RequestPriority = "(2200,0020)";
        public const string RTImageLabel = "(3002,0002)";
        public const string RTImageName = "(3002,0003)";
        public const string RTImageDescription = "(3002,0004)";
        public const string ReportedValuesOrigin = "(3002,000A)";
        public const string RTImagePlane = "(3002,000C)";
        public const string XRayImageReceptorTranslation = "(3002,000D)";
        public const string XRayImageReceptorAngle = "(3002,000E)";
        public const string RTImageOrientation = "(3002,0010)";
        public const string ImagePlanePixelSpacing = "(3002,0011)";
        public const string RTImagePosition = "(3002,0012)";
        public const string RadiationMachineName = "(3002,0020)";
        public const string RadiationMachineSAD = "(3002,0022)";
        public const string RadiationMachineSSD = "(3002,0024)";
        public const string RTImageSID = "(3002,0026)";
        public const string SourcetoReferenceObjectDistance = "(3002,0028)";
        public const string FractionNumber = "(3002,0029)";
        public const string ExposureSequence = "(3002,0030)";
        public const string MetersetExposure = "(3002,0032)";
        public const string DiaphragmPosition = "(3002,0034)";
        public const string FluenceMapSequence = "(3002,0040)";
        public const string FluenceDataSource = "(3002,0041)";
        public const string FluenceDataScale = "(3002,0042)";
        public const string PrimaryFluenceModeSequence = "(3002,0050)";
        public const string FluenceMode = "(3002,0051)";
        public const string FluenceModeID = "(3002,0052)";
        public const string DVHType = "(3004,0001)";
        public const string DoseUnits = "(3004,0002)";
        public const string DoseType = "(3004,0004)";
        public const string DoseComment = "(3004,0006)";
        public const string NormalizationPoint = "(3004,0008)";
        public const string DoseSummationType = "(3004,000A)";
        public const string GridFrameOffsetVector = "(3004,000C)";
        public const string DoseGridScaling = "(3004,000E)";
        public const string RTDoseROISequence = "(3004,0010)";
        public const string DoseValue = "(3004,0012)";
        public const string TissueHeterogeneityCorrection = "(3004,0014)";
        public const string DVHNormalizationPoint = "(3004,0040)";
        public const string DVHNormalizationDoseValue = "(3004,0042)";
        public const string DVHSequence = "(3004,0050)";
        public const string DVHDoseScaling = "(3004,0052)";
        public const string DVHVolumeUnits = "(3004,0054)";
        public const string DVHNumberofBins = "(3004,0056)";
        public const string DVHData = "(3004,0058)";
        public const string DVHReferencedROISequence = "(3004,0060)";
        public const string DVHROIContributionType = "(3004,0062)";
        public const string DVHMinimumDose = "(3004,0070)";
        public const string DVHMaximumDose = "(3004,0072)";
        public const string DVHMeanDose = "(3004,0074)";
        public const string StructureSetLabel = "(3006,0002)";
        public const string StructureSetName = "(3006,0004)";
        public const string StructureSetDescription = "(3006,0006)";
        public const string StructureSetDate = "(3006,0008)";
        public const string StructureSetTime = "(3006,0009)";
        public const string ReferencedFrameofReferenceSequence = "(3006,0010)";
        public const string RTReferencedStudySequence = "(3006,0012)";
        public const string RTReferencedSeriesSequence = "(3006,0014)";
        public const string ContourImageSequence = "(3006,0016)";
        public const string StructureSetROISequence = "(3006,0020)";
        public const string ROINumber = "(3006,0022)";
        public const string ReferencedFrameofReferenceUID = "(3006,0024)";
        public const string ROIName = "(3006,0026)";
        public const string ROIDescription = "(3006,0028)";
        public const string ROIDisplayColor = "(3006,002A)";
        public const string ROIVolume = "(3006,002C)";
        public const string RTRelatedROISequence = "(3006,0030)";
        public const string RTROIRelationship = "(3006,0033)";
        public const string ROIGenerationAlgorithm = "(3006,0036)";
        public const string ROIGenerationDescription = "(3006,0038)";
        public const string ROIContourSequence = "(3006,0039)";
        public const string ContourSequence = "(3006,0040)";
        public const string ContourGeometricType = "(3006,0042)";
        public const string ContourSlabThickness = "(3006,0044)";
        public const string ContourOffsetVector = "(3006,0045)";
        public const string NumberofContourPoints = "(3006,0046)";
        public const string ContourNumber = "(3006,0048)";
        public const string AttachedContours = "(3006,0049)";
        public const string ContourData = "(3006,0050)";
        public const string RTROIObservationsSequence = "(3006,0080)";
        public const string ObservationNumber = "(3006,0082)";
        public const string ReferencedROINumber = "(3006,0084)";
        public const string ROIObservationLabel = "(3006,0085)";
        public const string RTROIIdentificationCodeSequence = "(3006,0086)";
        public const string ROIObservationDescription = "(3006,0088)";
        public const string RelatedRTROIObservationsSequence = "(3006,00A0)";
        public const string RTROIInterpretedType = "(3006,00A4)";
        public const string ROIInterpreter = "(3006,00A6)";
        public const string ROIPhysicalPropertiesSequence = "(3006,00B0)";
        public const string ROIPhysicalProperty = "(3006,00B2)";
        public const string ROIPhysicalPropertyValue = "(3006,00B4)";
        public const string ROIElementalCompositionSequence = "(3006,00B6)";
        public const string ROIElementalCompositionAtomicNumber = "(3006,00B7)";
        public const string ROIElementalCompositionAtomicMassFraction = "(3006,00B8)";
        public const string FrameofReferenceRelationshipSequence = "(3006,00C0)";
        public const string RelatedFrameofReferenceUID = "(3006,00C2)";
        public const string FrameofReferenceTransformationType = "(3006,00C4)";
        public const string FrameofReferenceTransformationMatrix = "(3006,00C6)";
        public const string FrameofReferenceTransformationComment = "(3006,00C8)";
        public const string MeasuredDoseReferenceSequence = "(3008,0010)";
        public const string MeasuredDoseDescription = "(3008,0012)";
        public const string MeasuredDoseType = "(3008,0014)";
        public const string MeasuredDoseValue = "(3008,0016)";
        public const string TreatmentSessionBeamSequence = "(3008,0020)";
        public const string TreatmentSessionIonBeamSequence = "(3008,0021)";
        public const string CurrentFractionNumber = "(3008,0022)";
        public const string TreatmentControlPointDate = "(3008,0024)";
        public const string TreatmentControlPointTime = "(3008,0025)";
        public const string TreatmentTerminationStatus = "(3008,002A)";
        public const string TreatmentTerminationCode = "(3008,002B)";
        public const string TreatmentVerificationStatus = "(3008,002C)";
        public const string ReferencedTreatmentRecordSequence = "(3008,0030)";
        public const string SpecifiedPrimaryMeterset = "(3008,0032)";
        public const string SpecifiedSecondaryMeterset = "(3008,0033)";
        public const string DeliveredPrimaryMeterset = "(3008,0036)";
        public const string DeliveredSecondaryMeterset = "(3008,0037)";
        public const string SpecifiedTreatmentTime = "(3008,003A)";
        public const string DeliveredTreatmentTime = "(3008,003B)";
        public const string ControlPointDeliverySequence = "(3008,0040)";
        public const string IonControlPointDeliverySequence = "(3008,0041)";
        public const string SpecifiedMeterset = "(3008,0042)";
        public const string DeliveredMeterset = "(3008,0044)";
        public const string MetersetRateSet = "(3008,0045)";
        public const string MetersetRateDelivered = "(3008,0046)";
        public const string ScanSpotMetersetsDelivered = "(3008,0047)";
        public const string DoseRateDelivered = "(3008,0048)";
        public const string TreatmentSummaryCalculatedDoseReferenceSequence = "(3008,0050)";
        public const string CumulativeDosetoDoseReference = "(3008,0052)";
        public const string FirstTreatmentDate = "(3008,0054)";
        public const string MostRecentTreatmentDate = "(3008,0056)";
        public const string NumberofFractionsDelivered = "(3008,005A)";
        public const string OverrideSequence = "(3008,0060)";
        public const string ParameterSequencePointer = "(3008,0061)";
        public const string OverrideParameterPointer = "(3008,0062)";
        public const string ParameterItemIndex = "(3008,0063)";
        public const string MeasuredDoseReferenceNumber = "(3008,0064)";
        public const string ParameterPointer = "(3008,0065)";
        public const string OverrideReason = "(3008,0066)";
        public const string CorrectedParameterSequence = "(3008,0068)";
        public const string CorrectionValue = "(3008,006A)";
        public const string CalculatedDoseReferenceSequence = "(3008,0070)";
        public const string CalculatedDoseReferenceNumber = "(3008,0072)";
        public const string CalculatedDoseReferenceDescription = "(3008,0074)";
        public const string CalculatedDoseReferenceDoseValue = "(3008,0076)";
        public const string StartMeterset = "(3008,0078)";
        public const string EndMeterset = "(3008,007A)";
        public const string ReferencedMeasuredDoseReferenceSequence = "(3008,0080)";
        public const string ReferencedMeasuredDoseReferenceNumber = "(3008,0082)";
        public const string ReferencedCalculatedDoseReferenceSequence = "(3008,0090)";
        public const string ReferencedCalculatedDoseReferenceNumber = "(3008,0092)";
        public const string BeamLimitingDeviceLeafPairsSequence = "(3008,00A0)";
        public const string RecordedWedgeSequence = "(3008,00B0)";
        public const string RecordedCompensatorSequence = "(3008,00C0)";
        public const string RecordedBlockSequence = "(3008,00D0)";
        public const string TreatmentSummaryMeasuredDoseReferenceSequence = "(3008,00E0)";
        public const string RecordedSnoutSequence = "(3008,00F0)";
        public const string RecordedRangeShifterSequence = "(3008,00F2)";
        public const string RecordedLateralSpreadingDeviceSequence = "(3008,00F4)";
        public const string RecordedRangeModulatorSequence = "(3008,00F6)";
        public const string RecordedSourceSequence = "(3008,0100)";
        public const string SourceSerialNumber = "(3008,0105)";
        public const string TreatmentSessionApplicationSetupSequence = "(3008,0110)";
        public const string ApplicationSetupCheck = "(3008,0116)";
        public const string RecordedBrachyAccessoryDeviceSequence = "(3008,0120)";
        public const string ReferencedBrachyAccessoryDeviceNumber = "(3008,0122)";
        public const string RecordedChannelSequence = "(3008,0130)";
        public const string SpecifiedChannelTotalTime = "(3008,0132)";
        public const string DeliveredChannelTotalTime = "(3008,0134)";
        public const string SpecifiedNumberofPulses = "(3008,0136)";
        public const string DeliveredNumberofPulses = "(3008,0138)";
        public const string SpecifiedPulseRepetitionInterval = "(3008,013A)";
        public const string DeliveredPulseRepetitionInterval = "(3008,013C)";
        public const string RecordedSourceApplicatorSequence = "(3008,0140)";
        public const string ReferencedSourceApplicatorNumber = "(3008,0142)";
        public const string RecordedChannelShieldSequence = "(3008,0150)";
        public const string ReferencedChannelShieldNumber = "(3008,0152)";
        public const string BrachyControlPointDeliveredSequence = "(3008,0160)";
        public const string SafePositionExitDate = "(3008,0162)";
        public const string SafePositionExitTime = "(3008,0164)";
        public const string SafePositionReturnDate = "(3008,0166)";
        public const string SafePositionReturnTime = "(3008,0168)";
        public const string CurrentTreatmentStatus = "(3008,0200)";
        public const string TreatmentStatusComment = "(3008,0202)";
        public const string FractionGroupSummarySequence = "(3008,0220)";
        public const string ReferencedFractionNumber = "(3008,0223)";
        public const string FractionGroupType = "(3008,0224)";
        public const string BeamStopperPosition = "(3008,0230)";
        public const string FractionStatusSummarySequence = "(3008,0240)";
        public const string TreatmentDate = "(3008,0250)";
        public const string TreatmentTime = "(3008,0251)";
        public const string RTPlanLabel = "(300A,0002)";
        public const string RTPlanName = "(300A,0003)";
        public const string RTPlanDescription = "(300A,0004)";
        public const string RTPlanDate = "(300A,0006)";
        public const string RTPlanTime = "(300A,0007)";
        public const string TreatmentProtocols = "(300A,0009)";
        public const string PlanIntent = "(300A,000A)";
        public const string TreatmentSites = "(300A,000B)";
        public const string RTPlanGeometry = "(300A,000C)";
        public const string PrescriptionDescription = "(300A,000E)";
        public const string DoseReferenceSequence = "(300A,0010)";
        public const string DoseReferenceNumber = "(300A,0012)";
        public const string DoseReferenceUID = "(300A,0013)";
        public const string DoseReferenceStructureType = "(300A,0014)";
        public const string NominalBeamEnergyUnit = "(300A,0015)";
        public const string DoseReferenceDescription = "(300A,0016)";
        public const string DoseReferencePointCoordinates = "(300A,0018)";
        public const string NominalPriorDose = "(300A,001A)";
        public const string DoseReferenceType = "(300A,0020)";
        public const string ConstraintWeight = "(300A,0021)";
        public const string DeliveryWarningDose = "(300A,0022)";
        public const string DeliveryMaximumDose = "(300A,0023)";
        public const string TargetMinimumDose = "(300A,0025)";
        public const string TargetPrescriptionDose = "(300A,0026)";
        public const string TargetMaximumDose = "(300A,0027)";
        public const string TargetUnderdoseVolumeFraction = "(300A,0028)";
        public const string OrganatRiskFullvolumeDose = "(300A,002A)";
        public const string OrganatRiskLimitDose = "(300A,002B)";
        public const string OrganatRiskMaximumDose = "(300A,002C)";
        public const string OrganatRiskOverdoseVolumeFraction = "(300A,002D)";
        public const string ToleranceTableSequence = "(300A,0040)";
        public const string ToleranceTableNumber = "(300A,0042)";
        public const string ToleranceTableLabel = "(300A,0043)";
        public const string GantryAngleTolerance = "(300A,0044)";
        public const string BeamLimitingDeviceAngleTolerance = "(300A,0046)";
        public const string BeamLimitingDeviceToleranceSequence = "(300A,0048)";
        public const string BeamLimitingDevicePositionTolerance = "(300A,004A)";
        public const string SnoutPositionTolerance = "(300A,004B)";
        public const string PatientSupportAngleTolerance = "(300A,004C)";
        public const string TableTopEccentricAngleTolerance = "(300A,004E)";
        public const string TableTopPitchAngleTolerance = "(300A,004F)";
        public const string TableTopRollAngleTolerance = "(300A,0050)";
        public const string TableTopVerticalPositionTolerance = "(300A,0051)";
        public const string TableTopLongitudinalPositionTolerance = "(300A,0052)";
        public const string TableTopLateralPositionTolerance = "(300A,0053)";
        public const string RTPlanRelationship = "(300A,0055)";
        public const string FractionGroupSequence = "(300A,0070)";
        public const string FractionGroupNumber = "(300A,0071)";
        public const string FractionGroupDescription = "(300A,0072)";
        public const string NumberofFractionsPlanned = "(300A,0078)";
        public const string NumberofFractionPatternDigitsPerDay = "(300A,0079)";
        public const string RepeatFractionCycleLength = "(300A,007A)";
        public const string FractionPattern = "(300A,007B)";
        public const string NumberofBeams = "(300A,0080)";
        public const string BeamDoseSpecificationPoint = "(300A,0082)";
        public const string BeamDose = "(300A,0084)";
        public const string BeamMeterset = "(300A,0086)";
        public const string BeamDosePointDepth = "(300A,0088)";
        public const string BeamDosePointEquivalentDepth = "(300A,0089)";
        public const string BeamDosePointSSD = "(300A,008A)";
        public const string NumberofBrachyApplicationSetups = "(300A,00A0)";
        public const string BrachyApplicationSetupDoseSpecificationPoint = "(300A,00A2)";
        public const string BrachyApplicationSetupDose = "(300A,00A4)";
        public const string BeamSequence = "(300A,00B0)";
        public const string TreatmentMachineName = "(300A,00B2)";
        public const string PrimaryDosimeterUnit = "(300A,00B3)";
        public const string SourceAxisDistance = "(300A,00B4)";
        public const string BeamLimitingDeviceSequence = "(300A,00B6)";
        public const string RTBeamLimitingDeviceType = "(300A,00B8)";
        public const string SourcetoBeamLimitingDeviceDistance = "(300A,00BA)";
        public const string IsocentertoBeamLimitingDeviceDistance = "(300A,00BB)";
        public const string NumberofLeafJawPairs = "(300A,00BC)";
        public const string LeafPositionBoundaries = "(300A,00BE)";
        public const string BeamNumber = "(300A,00C0)";
        public const string BeamName = "(300A,00C2)";
        public const string BeamDescription = "(300A,00C3)";
        public const string BeamType = "(300A,00C4)";
        public const string RadiationType = "(300A,00C6)";
        public const string HighDoseTechniqueType = "(300A,00C7)";
        public const string ReferenceImageNumber = "(300A,00C8)";
        public const string PlannedVerificationImageSequence = "(300A,00CA)";
        public const string ImagingDeviceSpecificAcquisitionParameters = "(300A,00CC)";
        public const string TreatmentDeliveryType = "(300A,00CE)";
        public const string NumberofWedges = "(300A,00D0)";
        public const string WedgeSequence = "(300A,00D1)";
        public const string WedgeNumber = "(300A,00D2)";
        public const string WedgeType = "(300A,00D3)";
        public const string WedgeID = "(300A,00D4)";
        public const string WedgeAngle = "(300A,00D5)";
        public const string WedgeFactor = "(300A,00D6)";
        public const string TotalWedgeTrayWaterEquivalentThickness = "(300A,00D7)";
        public const string WedgeOrientation = "(300A,00D8)";
        public const string IsocentertoWedgeTrayDistance = "(300A,00D9)";
        public const string SourcetoWedgeTrayDistance = "(300A,00DA)";
        public const string WedgeThinEdgePosition = "(300A,00DB)";
        public const string BolusID = "(300A,00DC)";
        public const string BolusDescription = "(300A,00DD)";
        public const string NumberofCompensators = "(300A,00E0)";
        public const string MaterialID = "(300A,00E1)";
        public const string TotalCompensatorTrayFactor = "(300A,00E2)";
        public const string CompensatorSequence = "(300A,00E3)";
        public const string CompensatorNumber = "(300A,00E4)";
        public const string CompensatorID = "(300A,00E5)";
        public const string SourcetoCompensatorTrayDistance = "(300A,00E6)";
        public const string CompensatorRows = "(300A,00E7)";
        public const string CompensatorColumns = "(300A,00E8)";
        public const string CompensatorPixelSpacing = "(300A,00E9)";
        public const string CompensatorPosition = "(300A,00EA)";
        public const string CompensatorTransmissionData = "(300A,00EB)";
        public const string CompensatorThicknessData = "(300A,00EC)";
        public const string NumberofBoli = "(300A,00ED)";
        public const string CompensatorType = "(300A,00EE)";
        public const string NumberofBlocks = "(300A,00F0)";
        public const string TotalBlockTrayFactor = "(300A,00F2)";
        public const string TotalBlockTrayWaterEquivalentThickness = "(300A,00F3)";
        public const string BlockSequence = "(300A,00F4)";
        public const string BlockTrayID = "(300A,00F5)";
        public const string SourcetoBlockTrayDistance = "(300A,00F6)";
        public const string IsocentertoBlockTrayDistance = "(300A,00F7)";
        public const string BlockType = "(300A,00F8)";
        public const string AccessoryCode = "(300A,00F9)";
        public const string BlockDivergence = "(300A,00FA)";
        public const string BlockMountingPosition = "(300A,00FB)";
        public const string BlockNumber = "(300A,00FC)";
        public const string BlockName = "(300A,00FE)";
        public const string BlockThickness = "(300A,0100)";
        public const string BlockTransmission = "(300A,0102)";
        public const string BlockNumberofPoints = "(300A,0104)";
        public const string BlockData = "(300A,0106)";
        public const string ApplicatorSequence = "(300A,0107)";
        public const string ApplicatorID = "(300A,0108)";
        public const string ApplicatorType = "(300A,0109)";
        public const string ApplicatorDescription = "(300A,010A)";
        public const string CumulativeDoseReferenceCoefficient = "(300A,010C)";
        public const string FinalCumulativeMetersetWeight = "(300A,010E)";
        public const string NumberofControlPoints = "(300A,0110)";
        public const string ControlPointSequence = "(300A,0111)";
        public const string ControlPointIndex = "(300A,0112)";
        public const string NominalBeamEnergy = "(300A,0114)";
        public const string DoseRateSet = "(300A,0115)";
        public const string WedgePositionSequence = "(300A,0116)";
        public const string WedgePosition = "(300A,0118)";
        public const string BeamLimitingDevicePositionSequence = "(300A,011A)";
        public const string LeafJawPositions = "(300A,011C)";
        public const string GantryAngle = "(300A,011E)";
        public const string GantryRotationDirection = "(300A,011F)";
        public const string BeamLimitingDeviceAngle = "(300A,0120)";
        public const string BeamLimitingDeviceRotationDirection = "(300A,0121)";
        public const string PatientSupportAngle = "(300A,0122)";
        public const string PatientSupportRotationDirection = "(300A,0123)";
        public const string TableTopEccentricAxisDistance = "(300A,0124)";
        public const string TableTopEccentricAngle = "(300A,0125)";
        public const string TableTopEccentricRotationDirection = "(300A,0126)";
        public const string TableTopVerticalPosition = "(300A,0128)";
        public const string TableTopLongitudinalPosition = "(300A,0129)";
        public const string TableTopLateralPosition = "(300A,012A)";
        public const string IsocenterPosition = "(300A,012C)";
        public const string SurfaceEntryPoint = "(300A,012E)";
        public const string SourcetoSurfaceDistance = "(300A,0130)";
        public const string CumulativeMetersetWeight = "(300A,0134)";
        public const string TableTopPitchAngle = "(300A,0140)";
        public const string TableTopPitchRotationDirection = "(300A,0142)";
        public const string TableTopRollAngle = "(300A,0144)";
        public const string TableTopRollRotationDirection = "(300A,0146)";
        public const string HeadFixationAngle = "(300A,0148)";
        public const string GantryPitchAngle = "(300A,014A)";
        public const string GantryPitchRotationDirection = "(300A,014C)";
        public const string GantryPitchAngleTolerance = "(300A,014E)";
        public const string PatientSetupSequence = "(300A,0180)";
        public const string PatientSetupNumber = "(300A,0182)";
        public const string PatientSetupLabel = "(300A,0183)";
        public const string PatientAdditionalPosition = "(300A,0184)";
        public const string FixationDeviceSequence = "(300A,0190)";
        public const string FixationDeviceType = "(300A,0192)";
        public const string FixationDeviceLabel = "(300A,0194)";
        public const string FixationDeviceDescription = "(300A,0196)";
        public const string FixationDevicePosition = "(300A,0198)";
        public const string FixationDevicePitchAngle = "(300A,0199)";
        public const string FixationDeviceRollAngle = "(300A,019A)";
        public const string ShieldingDeviceSequence = "(300A,01A0)";
        public const string ShieldingDeviceType = "(300A,01A2)";
        public const string ShieldingDeviceLabel = "(300A,01A4)";
        public const string ShieldingDeviceDescription = "(300A,01A6)";
        public const string ShieldingDevicePosition = "(300A,01A8)";
        public const string SetupTechnique = "(300A,01B0)";
        public const string SetupTechniqueDescription = "(300A,01B2)";
        public const string SetupDeviceSequence = "(300A,01B4)";
        public const string SetupDeviceType = "(300A,01B6)";
        public const string SetupDeviceLabel = "(300A,01B8)";
        public const string SetupDeviceDescription = "(300A,01BA)";
        public const string SetupDeviceParameter = "(300A,01BC)";
        public const string SetupReferenceDescription = "(300A,01D0)";
        public const string TableTopVerticalSetupDisplacement = "(300A,01D2)";
        public const string TableTopLongitudinalSetupDisplacement = "(300A,01D4)";
        public const string TableTopLateralSetupDisplacement = "(300A,01D6)";
        public const string BrachyTreatmentTechnique = "(300A,0200)";
        public const string BrachyTreatmentType = "(300A,0202)";
        public const string TreatmentMachineSequence = "(300A,0206)";
        public const string SourceSequence = "(300A,0210)";
        public const string SourceNumber = "(300A,0212)";
        public const string SourceType = "(300A,0214)";
        public const string SourceManufacturer = "(300A,0216)";
        public const string ActiveSourceDiameter = "(300A,0218)";
        public const string ActiveSourceLength = "(300A,021A)";
        public const string SourceEncapsulationNominalThickness = "(300A,0222)";
        public const string SourceEncapsulationNominalTransmission = "(300A,0224)";
        public const string SourceIsotopeName = "(300A,0226)";
        public const string SourceIsotopeHalfLife = "(300A,0228)";
        public const string SourceStrengthUnits = "(300A,0229)";
        public const string ReferenceAirKermaRate = "(300A,022A)";
        public const string SourceStrength = "(300A,022B)";
        public const string SourceStrengthReferenceDate = "(300A,022C)";
        public const string SourceStrengthReferenceTime = "(300A,022E)";
        public const string ApplicationSetupSequence = "(300A,0230)";
        public const string ApplicationSetupType = "(300A,0232)";
        public const string ApplicationSetupNumber = "(300A,0234)";
        public const string ApplicationSetupName = "(300A,0236)";
        public const string ApplicationSetupManufacturer = "(300A,0238)";
        public const string TemplateNumber = "(300A,0240)";
        public const string TemplateType = "(300A,0242)";
        public const string TemplateName = "(300A,0244)";
        public const string TotalReferenceAirKerma = "(300A,0250)";
        public const string BrachyAccessoryDeviceSequence = "(300A,0260)";
        public const string BrachyAccessoryDeviceNumber = "(300A,0262)";
        public const string BrachyAccessoryDeviceID = "(300A,0263)";
        public const string BrachyAccessoryDeviceType = "(300A,0264)";
        public const string BrachyAccessoryDeviceName = "(300A,0266)";
        public const string BrachyAccessoryDeviceNominalThickness = "(300A,026A)";
        public const string BrachyAccessoryDeviceNominalTransmission = "(300A,026C)";
        public const string ChannelSequence = "(300A,0280)";
        public const string ChannelNumber = "(300A,0282)";
        public const string ChannelLength = "(300A,0284)";
        public const string ChannelTotalTime = "(300A,0286)";
        public const string SourceMovementType = "(300A,0288)";
        public const string NumberofPulses = "(300A,028A)";
        public const string PulseRepetitionInterval = "(300A,028C)";
        public const string SourceApplicatorNumber = "(300A,0290)";
        public const string SourceApplicatorID = "(300A,0291)";
        public const string SourceApplicatorType = "(300A,0292)";
        public const string SourceApplicatorName = "(300A,0294)";
        public const string SourceApplicatorLength = "(300A,0296)";
        public const string SourceApplicatorManufacturer = "(300A,0298)";
        public const string SourceApplicatorWallNominalThickness = "(300A,029C)";
        public const string SourceApplicatorWallNominalTransmission = "(300A,029E)";
        public const string SourceApplicatorStepSize = "(300A,02A0)";
        public const string TransferTubeNumber = "(300A,02A2)";
        public const string TransferTubeLength = "(300A,02A4)";
        public const string ChannelShieldSequence = "(300A,02B0)";
        public const string ChannelShieldNumber = "(300A,02B2)";
        public const string ChannelShieldID = "(300A,02B3)";
        public const string ChannelShieldName = "(300A,02B4)";
        public const string ChannelShieldNominalThickness = "(300A,02B8)";
        public const string ChannelShieldNominalTransmission = "(300A,02BA)";
        public const string FinalCumulativeTimeWeight = "(300A,02C8)";
        public const string BrachyControlPointSequence = "(300A,02D0)";
        public const string ControlPointRelativePosition = "(300A,02D2)";
        public const string ControlPointThreeDPosition = "(300A,02D4)";
        public const string CumulativeTimeWeight = "(300A,02D6)";
        public const string CompensatorDivergence = "(300A,02E0)";
        public const string CompensatorMountingPosition = "(300A,02E1)";
        public const string SourcetoCompensatorDistance = "(300A,02E2)";
        public const string TotalCompensatorTrayWaterEquivalentThickness = "(300A,02E3)";
        public const string IsocentertoCompensatorTrayDistance = "(300A,02E4)";
        public const string CompensatorColumnOffset = "(300A,02E5)";
        public const string IsocentertoCompensatorDistances = "(300A,02E6)";
        public const string CompensatorRelativeStoppingPowerRatio = "(300A,02E7)";
        public const string CompensatorMillingToolDiameter = "(300A,02E8)";
        public const string IonRangeCompensatorSequence = "(300A,02EA)";
        public const string CompensatorDescription = "(300A,02EB)";
        public const string RadiationMassNumber = "(300A,0302)";
        public const string RadiationAtomicNumber = "(300A,0304)";
        public const string RadiationChargeState = "(300A,0306)";
        public const string ScanMode = "(300A,0308)";
        public const string VirtualSourceAxisDistances = "(300A,030A)";
        public const string SnoutSequence = "(300A,030C)";
        public const string SnoutPosition = "(300A,030D)";
        public const string SnoutID = "(300A,030F)";
        public const string NumberofRangeShifters = "(300A,0312)";
        public const string RangeShifterSequence = "(300A,0314)";
        public const string RangeShifterNumber = "(300A,0316)";
        public const string RangeShifterID = "(300A,0318)";
        public const string RangeShifterType = "(300A,0320)";
        public const string RangeShifterDescription = "(300A,0322)";
        public const string NumberofLateralSpreadingDevices = "(300A,0330)";
        public const string LateralSpreadingDeviceSequence = "(300A,0332)";
        public const string LateralSpreadingDeviceNumber = "(300A,0334)";
        public const string LateralSpreadingDeviceID = "(300A,0336)";
        public const string LateralSpreadingDeviceType = "(300A,0338)";
        public const string LateralSpreadingDeviceDescription = "(300A,033A)";
        public const string LateralSpreadingDeviceWaterEquivalentThickness = "(300A,033C)";
        public const string NumberofRangeModulators = "(300A,0340)";
        public const string RangeModulatorSequence = "(300A,0342)";
        public const string RangeModulatorNumber = "(300A,0344)";
        public const string RangeModulatorID = "(300A,0346)";
        public const string RangeModulatorType = "(300A,0348)";
        public const string RangeModulatorDescription = "(300A,034A)";
        public const string BeamCurrentModulationID = "(300A,034C)";
        public const string PatientSupportType = "(300A,0350)";
        public const string PatientSupportID = "(300A,0352)";
        public const string PatientSupportAccessoryCode = "(300A,0354)";
        public const string FixationLightAzimuthalAngle = "(300A,0356)";
        public const string FixationLightPolarAngle = "(300A,0358)";
        public const string MetersetRate = "(300A,035A)";
        public const string RangeShifterSettingsSequence = "(300A,0360)";
        public const string RangeShifterSetting = "(300A,0362)";
        public const string IsocentertoRangeShifterDistance = "(300A,0364)";
        public const string RangeShifterWaterEquivalentThickness = "(300A,0366)";
        public const string LateralSpreadingDeviceSettingsSequence = "(300A,0370)";
        public const string LateralSpreadingDeviceSetting = "(300A,0372)";
        public const string IsocentertoLateralSpreadingDeviceDistance = "(300A,0374)";
        public const string RangeModulatorSettingsSequence = "(300A,0380)";
        public const string RangeModulatorGatingStartValue = "(300A,0382)";
        public const string RangeModulatorGatingStopValue = "(300A,0384)";
        public const string RangeModulatorGatingStartWaterEquivalentThickness = "(300A,0386)";
        public const string RangeModulatorGatingStopWaterEquivalentThickness = "(300A,0388)";
        public const string IsocentertoRangeModulatorDistance = "(300A,038A)";
        public const string ScanSpotTuneID = "(300A,0390)";
        public const string NumberofScanSpotPositions = "(300A,0392)";
        public const string ScanSpotPositionMap = "(300A,0394)";
        public const string ScanSpotMetersetWeights = "(300A,0396)";
        public const string ScanningSpotSize = "(300A,0398)";
        public const string NumberofPaintings = "(300A,039A)";
        public const string IonToleranceTableSequence = "(300A,03A0)";
        public const string IonBeamSequence = "(300A,03A2)";
        public const string IonBeamLimitingDeviceSequence = "(300A,03A4)";
        public const string IonBlockSequence = "(300A,03A6)";
        public const string IonControlPointSequence = "(300A,03A8)";
        public const string IonWedgeSequence = "(300A,03AA)";
        public const string IonWedgePositionSequence = "(300A,03AC)";
        public const string ReferencedSetupImageSequence = "(300A,0401)";
        public const string SetupImageComment = "(300A,0402)";
        public const string MotionSynchronizationSequence = "(300A,0410)";
        public const string ControlPointOrientation = "(300A,0412)";
        public const string GeneralAccessorySequence = "(300A,0420)";
        public const string GeneralAccessoryID = "(300A,0421)";
        public const string GeneralAccessoryDescription = "(300A,0422)";
        public const string GeneralAccessoryType = "(300A,0423)";
        public const string GeneralAccessoryNumber = "(300A,0424)";
        public const string ApplicatorGeometrySequence = "(300A,0431)";
        public const string ApplicatorApertureShape = "(300A,0432)";
        public const string ApplicatorOpening = "(300A,0433)";
        public const string ApplicatorOpeningX = "(300A,0434)";
        public const string ApplicatorOpeningY = "(300A,0435)";
        public const string SourcetoApplicatorMountingPositionDistance = "(300A,0436)";
        public const string ReferencedRTPlanSequence = "(300C,0002)";
        public const string ReferencedBeamSequence = "(300C,0004)";
        public const string ReferencedBeamNumber = "(300C,0006)";
        public const string ReferencedReferenceImageNumber = "(300C,0007)";
        public const string StartCumulativeMetersetWeight = "(300C,0008)";
        public const string EndCumulativeMetersetWeight = "(300C,0009)";
        public const string ReferencedBrachyApplicationSetupSequence = "(300C,000A)";
        public const string ReferencedBrachyApplicationSetupNumber = "(300C,000C)";
        public const string ReferencedSourceNumber = "(300C,000E)";
        public const string ReferencedFractionGroupSequence = "(300C,0020)";
        public const string ReferencedFractionGroupNumber = "(300C,0022)";
        public const string ReferencedVerificationImageSequence = "(300C,0040)";
        public const string ReferencedReferenceImageSequence = "(300C,0042)";
        public const string ReferencedDoseReferenceSequence = "(300C,0050)";
        public const string ReferencedDoseReferenceNumber = "(300C,0051)";
        public const string BrachyReferencedDoseReferenceSequence = "(300C,0055)";
        public const string ReferencedStructureSetSequence = "(300C,0060)";
        public const string ReferencedPatientSetupNumber = "(300C,006A)";
        public const string ReferencedDoseSequence = "(300C,0080)";
        public const string ReferencedToleranceTableNumber = "(300C,00A0)";
        public const string ReferencedBolusSequence = "(300C,00B0)";
        public const string ReferencedWedgeNumber = "(300C,00C0)";
        public const string ReferencedCompensatorNumber = "(300C,00D0)";
        public const string ReferencedBlockNumber = "(300C,00E0)";
        public const string ReferencedControlPointIndex = "(300C,00F0)";
        public const string ReferencedControlPointSequence = "(300C,00F2)";
        public const string ReferencedStartControlPointIndex = "(300C,00F4)";
        public const string ReferencedStopControlPointIndex = "(300C,00F6)";
        public const string ReferencedRangeShifterNumber = "(300C,0100)";
        public const string ReferencedLateralSpreadingDeviceNumber = "(300C,0102)";
        public const string ReferencedRangeModulatorNumber = "(300C,0104)";
        public const string ApprovalStatus = "(300E,0002)";
        public const string ReviewDate = "(300E,0004)";
        public const string ReviewTime = "(300E,0005)";
        public const string ReviewerName = "(300E,0008)";
        public const string Arbitrary = "(4000,0010)";
        public const string TextComments = "(4000,4000)";
        public const string ResultsID = "(4008,0040)";
        public const string ResultsIDIssuer = "(4008,0042)";
        public const string ReferencedInterpretationSequence = "(4008,0050)";
        public const string ReportProductionStatusTrial = "(4008,00FF)";
        public const string InterpretationRecordedDate = "(4008,0100)";
        public const string InterpretationRecordedTime = "(4008,0101)";
        public const string InterpretationRecorder = "(4008,0102)";
        public const string ReferencetoRecordedSound = "(4008,0103)";
        public const string InterpretationTranscriptionDate = "(4008,0108)";
        public const string InterpretationTranscriptionTime = "(4008,0109)";
        public const string InterpretationTranscriber = "(4008,010A)";
        public const string InterpretationText = "(4008,010B)";
        public const string InterpretationAuthor = "(4008,010C)";
        public const string InterpretationApproverSequence = "(4008,0111)";
        public const string InterpretationApprovalDate = "(4008,0112)";
        public const string InterpretationApprovalTime = "(4008,0113)";
        public const string PhysicianApprovingInterpretation = "(4008,0114)";
        public const string InterpretationDiagnosisDescription = "(4008,0115)";
        public const string InterpretationDiagnosisCodeSequence = "(4008,0117)";
        public const string ResultsDistributionListSequence = "(4008,0118)";
        public const string DistributionName = "(4008,0119)";
        public const string DistributionAddress = "(4008,011A)";
        public const string InterpretationID = "(4008,0200)";
        public const string InterpretationIDIssuer = "(4008,0202)";
        public const string InterpretationTypeID = "(4008,0210)";
        public const string InterpretationStatusID = "(4008,0212)";
        public const string Impressions = "(4008,0300)";
        public const string ResultsComments = "(4008,4000)";
        public const string LowEnergyDetectors = "(4010,0001)";
        public const string HighEnergyDetectors = "(4010,0002)";
        public const string DetectorGeometrySequence = "(4010,0004)";
        public const string ThreatROIVoxelSequence = "(4010,1001)";
        public const string ThreatROIBase = "(4010,1004)";
        public const string ThreatROIExtents = "(4010,1005)";
        public const string ThreatROIBitmap = "(4010,1006)";
        public const string RouteSegmentID = "(4010,1007)";
        public const string GantryType = "(4010,1008)";
        public const string OOIOwnerType = "(4010,1009)";
        public const string RouteSegmentSequence = "(4010,100A)";
        public const string PotentialThreatObjectID = "(4010,1010)";
        public const string ThreatSequence = "(4010,1011)";
        public const string ThreatCategory = "(4010,1012)";
        public const string ThreatCategoryDescription = "(4010,1013)";
        public const string ATDAbilityAssessment = "(4010,1014)";
        public const string ATDAssessmentFlag = "(4010,1015)";
        public const string ATDAssessmentProbability = "(4010,1016)";
        public const string Mass = "(4010,1017)";
        public const string Density = "(4010,1018)";
        public const string ZEffective = "(4010,1019)";
        public const string BoardingPassID = "(4010,101A)";
        public const string CenterofMass = "(4010,101B)";
        public const string CenterofPTO = "(4010,101C)";
        public const string BoundingPolygon = "(4010,101D)";
        public const string RouteSegmentStartLocationID = "(4010,101E)";
        public const string RouteSegmentEndLocationID = "(4010,101F)";
        public const string RouteSegmentLocationIDType = "(4010,1020)";
        public const string AbortReason = "(4010,1021)";
        public const string VolumeofPTO = "(4010,1023)";
        public const string AbortFlag = "(4010,1024)";
        public const string RouteSegmentStartTime = "(4010,1025)";
        public const string RouteSegmentEndTime = "(4010,1026)";
        public const string TDRType = "(4010,1027)";
        public const string InternationalRouteSegment = "(4010,1028)";
        public const string ThreatDetectionAlgorithmandVersion = "(4010,1029)";
        public const string AssignedLocation = "(4010,102A)";
        public const string AlarmDecisionTime = "(4010,102B)";
        public const string AlarmDecision = "(4010,1031)";
        public const string NumberofTotalObjects = "(4010,1033)";
        public const string NumberofAlarmObjects = "(4010,1034)";
        public const string PTORepresentationSequence = "(4010,1037)";
        public const string ATDAssessmentSequence = "(4010,1038)";
        public const string TIPType = "(4010,1039)";
        public const string DICOSVersion = "(4010,103A)";
        public const string OOIOwnerCreationTime = "(4010,1041)";
        public const string OOIType = "(4010,1042)";
        public const string OOISize = "(4010,1043)";
        public const string AcquisitionStatus = "(4010,1044)";
        public const string BasisMaterialsCodeSequence = "(4010,1045)";
        public const string PhantomType = "(4010,1046)";
        public const string OOIOwnerSequence = "(4010,1047)";
        public const string ScanType = "(4010,1048)";
        public const string ItineraryID = "(4010,1051)";
        public const string ItineraryIDType = "(4010,1052)";
        public const string ItineraryIDAssigningAuthority = "(4010,1053)";
        public const string RouteID = "(4010,1054)";
        public const string RouteIDAssigningAuthority = "(4010,1055)";
        public const string InboundArrivalType = "(4010,1056)";
        public const string CarrierID = "(4010,1058)";
        public const string CarrierIDAssigningAuthority = "(4010,1059)";
        public const string SourceOrientation = "(4010,1060)";
        public const string SourcePosition = "(4010,1061)";
        public const string BeltHeight = "(4010,1062)";
        public const string AlgorithmRoutingCodeSequence = "(4010,1064)";
        public const string TransportClassification = "(4010,1067)";
        public const string OOITypeDescriptor = "(4010,1068)";
        public const string TotalProcessingTime = "(4010,1069)";
        public const string DetectorCalibrationData = "(4010,106C)";
        public const string MACParametersSequence = "(4FFE,0001)";
        public const string CurveDimensions = "(5000,0005)";
        public const string NumberofPoints = "(5000,0010)";
        public const string TypeofData = "(5000,0020)";
        public const string CurveDescription = "(5000,0022)";
        public const string AxisUnits = "(5000,0030)";
        public const string AxisLabels = "(5000,0040)";
        public const string DataValueRepresentation = "(5000,0103)";
        public const string MinimumCoordinateValue = "(5000,0104)";
        public const string MaximumCoordinateValue = "(5000,0105)";
        public const string CurveRange = "(5000,0106)";
        public const string CurveDataDescriptor = "(5000,0110)";
        public const string CoordinateStartValue = "(5000,0112)";
        public const string CoordinateStepValue = "(5000,0114)";
        public const string CurveActivationLayer = "(5000,1001)";
        public const string AudioType = "(5000,2000)";
        public const string AudioSampleFormat = "(5000,2002)";
        public const string NumberofChannels = "(5000,2004)";
        public const string NumberofSamples = "(5000,2006)";
        public const string SampleRate = "(5000,2008)";
        public const string TotalTime = "(5000,200A)";
        public const string AudioSampleData = "(5000,200C)";
        public const string AudioComments = "(5000,200E)";
        public const string CurveLabel = "(5000,2500)";
        public const string CurveReferencedOverlaySequence = "(5000,2600)";
        public const string CurveReferencedOverlayGroup = "(5000,2610)";
        public const string CurveData = "(5000,3000)";
        public const string SharedFunctionalGroupsSequence = "(5200,9229)";
        public const string PerframeFunctionalGroupsSequence = "(5200,9230)";
        public const string WaveformSequence = "(5400,0100)";
        public const string ChannelMinimumValue = "(5400,0110)";
        public const string ChannelMaximumValue = "(5400,0112)";
        public const string WaveformBitsAllocated = "(5400,1004)";
        public const string WaveformSampleInterpretation = "(5400,1006)";
        public const string WaveformPaddingValue = "(5400,100A)";
        public const string WaveformData = "(5400,1010)";
        public const string FirstOrderPhaseCorrectionAngle = "(5600,0010)";
        public const string SpectroscopyData = "(5600,0020)";
        public const string OverlayRows = "(6000,0010)";
        public const string OverlayColumns = "(6000,0011)";
        public const string OverlayPlanes = "(6000,0012)";
        public const string NumberofFramesinOverlay = "(6000,0015)";
        public const string OverlayDescription = "(6000,0022)";
        public const string OverlayType = "(6000,0040)";
        public const string OverlaySubtype = "(6000,0045)";
        public const string OverlayOrigin = "(6000,0050)";
        public const string ImageFrameOrigin = "(6000,0051)";
        public const string OverlayPlaneOrigin = "(6000,0052)";
        public const string OverlayCompressionCode = "(6000,0060)";
        public const string OverlayCompressionOriginator = "(6000,0061)";
        public const string OverlayCompressionLabel = "(6000,0062)";
        public const string OverlayCompressionDescription = "(6000,0063)";
        public const string OverlayCompressionStepPointers = "(6000,0066)";
        public const string OverlayRepeatInterval = "(6000,0068)";
        public const string OverlayBitsGrouped = "(6000,0069)";
        public const string OverlayBitsAllocated = "(6000,0100)";
        public const string OverlayBitPosition = "(6000,0102)";
        public const string OverlayFormat = "(6000,0110)";
        public const string OverlayLocation = "(6000,0200)";
        public const string OverlayCodeLabel = "(6000,0800)";
        public const string OverlayNumberofTables = "(6000,0802)";
        public const string OverlayCodeTableLocation = "(6000,0803)";
        public const string OverlayBitsForCodeWord = "(6000,0804)";
        public const string OverlayActivationLayer = "(6000,1001)";
        public const string OverlayDescriptorGray = "(6000,1100)";
        public const string OverlayDescriptorRed = "(6000,1101)";
        public const string OverlayDescriptorGreen = "(6000,1102)";
        public const string OverlayDescriptorBlue = "(6000,1103)";
        public const string OverlaysGray = "(6000,1200)";
        public const string OverlaysRed = "(6000,1201)";
        public const string OverlaysGreen = "(6000,1202)";
        public const string OverlaysBlue = "(6000,1203)";
        public const string ROIArea = "(6000,1301)";
        public const string ROIMean = "(6000,1302)";
        public const string ROIStandardDeviation = "(6000,1303)";
        public const string OverlayLabel = "(6000,1500)";
        public const string OverlayData = "(6000,3000)";
        public const string OverlayComments = "(6000,4000)";
        public const string VariablePixelData = "(7F00,0010)";
        public const string VariableNextDataGroup = "(7F00,0011)";
        public const string VariableCoefficientsSDVN = "(7F00,0020)";
        public const string VariableCoefficientsSDHN = "(7F00,0030)";
        public const string VariableCoefficientsSDDN = "(7F00,0040)";
        public const string PixelData = "(7FE0,0010)";
        public const string CoefficientsSDVN = "(7FE0,0020)";
        public const string CoefficientsSDHN = "(7FE0,0030)";
        public const string CoefficientsSDDN = "(7FE0,0040)";
        public const string DigitalSignaturesSequence = "(FFFA,FFFA)";
        public const string DataSetTrailingPadding = "(FFFC,FFFC)";
        public const string Item = "(FFFE,E000)";
        public const string ItemDelimitationItem = "(FFFE,E00D)";
        public const string SequenceDelimitationItem = "(FFFE,E0DD)";
    }
}
