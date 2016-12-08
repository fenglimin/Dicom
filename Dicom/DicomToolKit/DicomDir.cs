using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace EK.Capture.Dicom.DicomToolKit
{

    // do not like the lack of random access and inability to get a count and navigate tree


    #region DirectoryRecord

    public class DirectoryRecord
    {
        internal Elements dicom = new Elements();
        /// <summary>
        /// Just that.
        /// </summary>
        public static readonly string DefaultCharacterSet = "ISO_IR 100";

        internal DirectoryRecord()
        {
            this.dicom = new Elements();
        }

        /// <summary>
        /// Called by NewXXX
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="type"></param>
        internal DirectoryRecord(string type)
            : this(new Elements(), type)
        {
        }

        /// <summary>
        /// Called by Load and NewXXX
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="type"></param>
        internal DirectoryRecord(Elements elements, string type)
        {
            this.dicom = elements;
            Initialize(type);
        }

        /// <summary>
        ///  Called by Add
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="type"></param>
        internal DirectoryRecord(DataSet elements, string type)
        {
            Initialize(type);
            if (elements.Contains(t.OffsetoftheNextDirectoryRecord))
                dicom[t.OffsetoftheNextDirectoryRecord].Value = elements[t.OffsetoftheNextDirectoryRecord].Value;
            if (elements.Contains(t.RecordInuseFlag))
                dicom[t.RecordInuseFlag].Value = elements[t.RecordInuseFlag].Value;
            if (elements.Contains(t.OffsetofReferencedLowerLevelDirectoryEntity))
                dicom[t.OffsetofReferencedLowerLevelDirectoryEntity].Value = elements[t.OffsetofReferencedLowerLevelDirectoryEntity].Value;
            if (elements.Contains(t.SpecificCharacterSet))
                dicom[t.SpecificCharacterSet].Value = elements[t.SpecificCharacterSet].Value;
        }

        private void Initialize(string type)
        {
            if (!dicom.Contains(t.OffsetoftheNextDirectoryRecord))
                dicom.Add(t.OffsetoftheNextDirectoryRecord, 0);
            if (!dicom.Contains(t.RecordInuseFlag))
                dicom.Add(t.RecordInuseFlag, 0xffff);
            if (!dicom.Contains(t.OffsetofReferencedLowerLevelDirectoryEntity))
                dicom.Add(t.OffsetofReferencedLowerLevelDirectoryEntity, 0);
            if (!dicom.Contains(t.DirectoryRecordType))
                dicom.Add(t.DirectoryRecordType, type);
            if (!dicom.Contains(t.SpecificCharacterSet))
                dicom.Add(t.SpecificCharacterSet, DefaultCharacterSet);
        }

        public bool Contains(string key)
        {
            return dicom.Contains(key);
        }

        public Element this[string key]
        {
            get
            {
                return dicom[key];
            }
            set
            {
                dicom.Set(key, value.Value);
            }
        }

        public Elements Elements
        {
            get
            {
                return dicom;
            }
        }

        public uint OffsetNextRecord
        {
            get
            {
                return (uint)dicom[t.OffsetoftheNextDirectoryRecord].Value;
            }
            internal set
            {
                dicom[t.OffsetoftheNextDirectoryRecord].Value = value;
            }
        }

        public uint OffsetFirstChild
        {
            get
            {
                return (uint)dicom[t.OffsetofReferencedLowerLevelDirectoryEntity].Value;
            }
            internal set
            {
                dicom[t.OffsetofReferencedLowerLevelDirectoryEntity].Value = value;
            }
        }

        public bool RecordInuseFlag
        {
            get
            {
                ushort flag = (ushort)dicom[t.RecordInuseFlag].Value;
                return !(flag == 0);
            }
            internal set
            {
                dicom[t.RecordInuseFlag].Value = (value) ? 0xffff : 0x0000;
            }
        }

        public string DirectoryRecordType
        {
            get
            {
                return (string)dicom[t.DirectoryRecordType].Value;
            }
        }

        public string SpecificCharacterSet
        {
            get
            {
                return (string)dicom[t.SpecificCharacterSet].Value;
            }
            set
            {
                dicom[t.SpecificCharacterSet].Value = value;
            }
        }
    }

    public class DirectoryParent : DirectoryRecord
    {
        internal List<DirectoryRecord> children = new List<DirectoryRecord>();

        internal DirectoryParent()
        {
        }

        internal DirectoryParent(string type) : base(type)
        {
        }

        internal DirectoryParent(Elements elements, string type) : base(elements, type)
        {
        }

        internal DirectoryParent(DataSet elements, string type) : base(elements, type)
        {
        }

        public List<DirectoryRecord> Children
        {
            get
            {
                return children;
            }
        }
    }

    #endregion DirectoryRecord

    #region Patient

    public class Patient : DirectoryParent, IEnumerable<Study>
    {
        public Patient()
        {
            Initialize(true);
        }

        public Patient(Elements elements) : base(elements, "PATIENT")
        {
            Initialize(false);
        }

        public Patient(string name, string id) : base("PATIENT")
        {
            Initialize(false);
            PatientsName = name;
            PatientID = id;
        }

        public Patient(DataSet elements) : base(elements, "PATIENT")
        {
            Initialize(false);
            if (elements.Contains(t.PatientName))
                dicom[t.PatientName].Value = elements[t.PatientName].Value;
            if (elements.Contains(t.PatientID))
                dicom[t.PatientID].Value = elements[t.PatientID].Value;
            if (elements.Contains(t.PatientSex))
                dicom[t.PatientSex].Value = elements[t.PatientSex].Value;
            if (elements.Contains(t.PatientBirthDate))
                dicom[t.PatientBirthDate].Value = elements[t.PatientBirthDate].Value;
            if (elements.Contains(t.AdditionalPatientHistory))
                dicom[t.AdditionalPatientHistory].Value = elements[t.AdditionalPatientHistory].Value;
        }

        private void Initialize(bool query)
        {
            if (!dicom.Contains(t.PatientName))
                dicom.Add(t.PatientName, null);
            if (!dicom.Contains(t.PatientID))
                dicom.Add(t.PatientID, null);
            if (!dicom.Contains(t.PatientBirthDate))
                dicom.Add(t.PatientBirthDate, null);
            if (!dicom.Contains(t.PatientBirthTime))
                dicom.Add(t.PatientBirthTime, null);
            if (!dicom.Contains(t.PatientSex))
                dicom.Add(t.PatientSex, null);
            if (!dicom.Contains(t.OtherPatientIDs))
                dicom.Add(t.OtherPatientIDs, null);
            if (!dicom.Contains(t.OtherPatientNames))
                dicom.Add(t.OtherPatientNames, null);
            if (!dicom.Contains(t.PatientAge))
                dicom.Add(t.PatientAge, null);
            if (!dicom.Contains(t.PatientSize))
                dicom.Add(t.PatientSize, null);
            if (!dicom.Contains(t.PatientWeight))
                dicom.Add(t.PatientWeight, null);
            if (!dicom.Contains(t.EthnicGroup))
                dicom.Add(t.EthnicGroup, null);
            if (!dicom.Contains(t.Occupation))
                dicom.Add(t.Occupation, null);
            if (!dicom.Contains(t.AdditionalPatientHistory))
                dicom.Add(t.AdditionalPatientHistory, null);
            if (!dicom.Contains(t.PatientComments))
                dicom.Add(t.PatientComments, null);
        }

        public string PatientsName
        {
            get
            {
                return (string)dicom[t.PatientName].Value;
            }
            set
            {
                dicom[t.PatientName].Value = value;
            }
        }

        public string PatientID
        {
            get
            {
                return (string)dicom[t.PatientID].Value;
            }
            set
            {
                dicom[t.PatientID].Value = value;
            }
        }

        public string PatientsSex
        {
            get
            {
                return (string)dicom[t.PatientSex].Value;
            }
            set
            {
                dicom[t.PatientSex].Value = value;
            }
        }

        public string PatientsBirthDate
        {
            get
            {
                return (string)dicom[t.PatientBirthDate].Value;
            }
            set
            {
                dicom[t.PatientBirthDate].Value = value;
            }
        }

        public string AdditionalPatientHistory
        {
            get
            {
                return (string)dicom[t.AdditionalPatientHistory].Value;
            }
            set
            {
                dicom[t.AdditionalPatientHistory].Value = value;
            }
        }


        IEnumerator<Study> IEnumerable<Study>.GetEnumerator()
        {
            foreach (Study temp in children)
            {
                yield return temp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Study>)this).GetEnumerator();
        }

        public Study NewStudy(DateTime date, DateTime time, string uid, string id)
        {
            Study temp = new Study(date, time, uid, id);
            children.Add(temp);
            return temp;
        }
    }

    #endregion Patient

    #region Study

    public class Study : DirectoryParent, IEnumerable<Series>
    {
        private int counter = 0;

        public Study()
        {
            Initialize(true);
        }

        public Study(Elements elements) : base(elements, "STUDY")
        {
            Initialize(false);
        }

        public Study(DateTime date, DateTime time, string uid, string id) : base("STUDY")
        {
            Initialize(false);
            StudyDate = date;
            StudyTime = time;
            StudyInstanceUID = uid;
            StudyID = id;
        }

        public Study(DataSet elements) : base(elements, "STUDY")
        {
            Initialize(false);
            if (elements.Contains(t.StudyDate))
                dicom[t.StudyDate].Value = elements[t.StudyDate].Value;
            if (elements.Contains(t.StudyTime))
                dicom[t.StudyTime].Value = elements[t.StudyTime].Value;
            if (elements.Contains(t.StudyDescription))
                dicom[t.StudyDescription].Value = elements[t.StudyDescription].Value;
            if (elements.Contains(t.StudyInstanceUID))
                dicom[t.StudyInstanceUID].Value = elements[t.StudyInstanceUID].Value;
            if (elements.Contains(t.StudyID))
                dicom[t.StudyID].Value = elements[t.StudyID].Value;
            if (elements.Contains(t.PerformedProcedureStepID))
                dicom[t.PerformedProcedureStepID].Value = elements[t.PerformedProcedureStepID].Value;
            if (elements.Contains(t.AccessionNumber))
                dicom[t.AccessionNumber].Value = elements[t.AccessionNumber].Value;
            if (elements.Contains(t.Manufacturer))
                dicom[t.Manufacturer].Value = elements[t.Manufacturer].Value;
            if (elements.Contains(t.ManufacturerModelName))
                dicom[t.ManufacturerModelName].Value = elements[t.ManufacturerModelName].Value;
        }

        private void Initialize(bool query)
        {
            if (!dicom.Contains(t.StudyDate))
                dicom.Add(t.StudyDate, null);
            if (!dicom.Contains(t.StudyTime))
                dicom.Add(t.StudyTime, null);
            if (!dicom.Contains(t.StudyDescription))
                dicom.Add(t.StudyDescription, null);
            if (!dicom.Contains(t.StudyInstanceUID))
                dicom.Add(t.StudyInstanceUID, null);
            if (!dicom.Contains(t.StudyID))
                dicom.Add(t.StudyID, null);
            if (!dicom.Contains(t.PerformedProcedureStepID))
                dicom.Add(t.PerformedProcedureStepID, null);
            if (!dicom.Contains(t.AccessionNumber))
                dicom.Add(t.AccessionNumber, null);
            if (!dicom.Contains(t.Manufacturer))
                dicom.Add(t.Manufacturer, null);
            if (!dicom.Contains(t.ManufacturerModelName))
                dicom.Add(t.ManufacturerModelName, null);

            if(query)
            {
                if (!dicom.Contains(t.ModalitiesinStudy))
                    dicom.Add(t.ModalitiesinStudy, null);
                if (!dicom.Contains(t.ReferringPhysicianName))
                    dicom.Add(t.ReferringPhysicianName, null);
                if (!dicom.Contains(t.PhysiciansofRecord))
                    dicom.Add(t.PhysiciansofRecord, null);
                if (!dicom.Contains(t.NameofPhysiciansReadingStudy))
                    dicom.Add(t.NameofPhysiciansReadingStudy, null);
                if (!dicom.Contains(t.AdmittingDiagnosesDescription))
                    dicom.Add(t.AdmittingDiagnosesDescription, null);
                if (!dicom.Contains(t.NumberofPatientRelatedSeries))
                    dicom.Add(t.NumberofPatientRelatedSeries, null);
                if (!dicom.Contains(t.NumberofPatientRelatedInstances))
                    dicom.Add(t.NumberofPatientRelatedInstances, null);

                if (!dicom.Contains(t.PatientName))
                    dicom.Add(t.PatientName, null);
                if (!dicom.Contains(t.PatientID))
                    dicom.Add(t.PatientID, null);
                if (!dicom.Contains(t.PatientBirthDate))
                    dicom.Add(t.PatientBirthDate, null);
                if (!dicom.Contains(t.PatientBirthTime))
                    dicom.Add(t.PatientBirthTime, null);
                if (!dicom.Contains(t.PatientSex))
                    dicom.Add(t.PatientSex, null);
                if (!dicom.Contains(t.OtherPatientIDs))
                    dicom.Add(t.OtherPatientIDs, null);
                if (!dicom.Contains(t.OtherPatientNames))
                    dicom.Add(t.OtherPatientNames, null);
                if (!dicom.Contains(t.PatientAge))
                    dicom.Add(t.PatientAge, null);
                if (!dicom.Contains(t.PatientSize))
                    dicom.Add(t.PatientSize, null);
                if (!dicom.Contains(t.PatientWeight))
                    dicom.Add(t.PatientWeight, null);
                if (!dicom.Contains(t.EthnicGroup))
                    dicom.Add(t.EthnicGroup, null);
                if (!dicom.Contains(t.Occupation))
                    dicom.Add(t.Occupation, null);
                if (!dicom.Contains(t.AdditionalPatientHistory))
                    dicom.Add(t.AdditionalPatientHistory, null);
                if (!dicom.Contains(t.PatientComments))
                    dicom.Add(t.PatientComments, null);
            }
        }

        public DateTime StudyDate
        {
            get
            {
                string date = (string)dicom[t.StudyDate].Value;
                return DateTime.ParseExact(date, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                dicom[t.StudyDate].Value = value.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public DateTime StudyTime
        {
            get
            {
                string time = (string)dicom[t.StudyTime].Value;
                return DateTime.ParseExact(time, "HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
            set
            {
                dicom[t.StudyTime].Value = value.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        public string StudyDescription
        {
            get
            {
                return (string)dicom[t.StudyDescription].Value;
            }
            set
            {
                dicom[t.StudyDescription].Value = value;
            }
        }

        public string StudyInstanceUID
        {
            get
            {
                return (string)dicom[t.StudyInstanceUID].Value;
            }
            set
            {
                dicom[t.StudyInstanceUID].Value = value;
            }
        }

        public string StudyID
        {
            get
            {
                return (string)dicom[t.StudyID].Value;
            }
            set
            {
                dicom[t.StudyID].Value = value;
            }
        }

        public string PerformedProcedureStepID
        {
            get
            {
                return (string)dicom[t.PerformedProcedureStepID].Value;
            }
            set
            {
                dicom[t.PerformedProcedureStepID].Value = value;
            }
        }

        public string AccessionNumber
        {
            get
            {
                return (string)dicom[t.AccessionNumber].Value;
            }
            set
            {
                dicom[t.AccessionNumber].Value = value;
            }
        }

        public string Manufacturer
        {
            get
            {
                return (string)dicom[t.Manufacturer].Value;
            }
            set
            {
                dicom[t.Manufacturer].Value = value;
            }
        }

        public string ManufacturerModelName
        {
            get
            {
                return (string)dicom[t.ManufacturerModelName].Value;
            }
            set
            {
                dicom[t.ManufacturerModelName].Value = value;
            }
        }

        IEnumerator<Series> IEnumerable<Series>.GetEnumerator()
        {
            foreach (Series temp in children)
            {
                yield return temp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Series>)this).GetEnumerator();
        }

        public Series NewSeries(string modality, string uid)
        {
            Series temp = new Series(modality, uid, ++counter);
            children.Add(temp);
            return temp;
        }
    }

    #endregion Study

    #region Series

    public class Series : DirectoryParent, IEnumerable<Image>
    {
        public Series()
        {
            Initialize(true);
        }

        public Series(Elements elements) : base(elements, "SERIES")
        {
            Initialize(false);
        }

        public Series(string modality, string uid, int number) : base("SERIES")
        {
            Initialize(false);
            Modality = modality;
            SeriesInstanceUID = uid;
            SeriesNumber = number;
        }

        public Series(DataSet elements) : base(elements, "SERIES")
        {
            Initialize(false);
            if (elements.Contains(t.Modality))
                dicom[t.Modality].Value = elements[t.Modality].Value;
            if (elements.Contains(t.SeriesInstanceUID))
                dicom[t.SeriesInstanceUID].Value = elements[t.SeriesInstanceUID].Value;
            if (elements.Contains(t.SeriesNumber))
                dicom[t.SeriesNumber].Value = elements[t.SeriesNumber].Value;
            if (elements.Contains(t.OperatorsName))
                dicom[t.OperatorsName].Value = elements[t.OperatorsName].Value;
            if (elements.Contains(t.BodyPartExamined))
                dicom[t.BodyPartExamined].Value = elements[t.BodyPartExamined].Value;
            if (elements.Contains(t.PatientPosition))
                dicom[t.PatientPosition].Value = elements[t.PatientPosition].Value;
            if (elements.Contains(t.ViewPosition))
                dicom[t.ViewPosition].Value = elements[t.ViewPosition].Value;

            if (elements.Contains(t.SeriesDescription))
                dicom[t.SeriesDescription].Value = elements[t.SeriesDescription].Value;
        }

        private void Initialize(bool query)
        {
            if (!dicom.Contains(t.Modality))
                dicom.Add(t.Modality, null);
            if (!dicom.Contains(t.SeriesInstanceUID))
                dicom.Add(t.SeriesInstanceUID, null);
            if (!dicom.Contains(t.SeriesNumber))
                dicom.Add(t.SeriesNumber, null);
            if (!dicom.Contains(t.OperatorsName))
                dicom.Add(t.OperatorsName, null);
            if (!dicom.Contains(t.BodyPartExamined))
                dicom.Add(t.BodyPartExamined, null);
            if (!dicom.Contains(t.PatientPosition))
                dicom.Add(t.PatientPosition, null);
            if (!dicom.Contains(t.ViewPosition))
                dicom.Add(t.ViewPosition, null);

            if (!dicom.Contains(t.SeriesDescription))
                dicom.Add(t.SeriesDescription, null);

            if (query)
            {
                if (!dicom.Contains(t.StudyInstanceUID))
                    dicom.Add(t.StudyInstanceUID, null);
            }
        }

        public string Modality
        {
            get
            {
                return (string)dicom[t.Modality].Value;
            }
            set
            {
                dicom[t.Modality].Value = value;
            }
        }

        public string SeriesInstanceUID
        {
            get
            {
                return (string)dicom[t.SeriesInstanceUID].Value;
            }
            set
            {
                dicom[t.SeriesInstanceUID].Value = value;
            }
        }

        public int SeriesNumber
        {
            get
            {
                string number = (string)dicom[t.SeriesNumber].Value;
                return Int32.Parse(number);
            }
            private set
            {
                dicom[t.SeriesNumber].Value = value.ToString();
            }
        }

        public string OperatorsName
        {
            get
            {
                return (string)dicom[t.OperatorsName].Value;
            }
            set
            {
                dicom[t.OperatorsName].Value = value;
            }
        }

        public string BodyPartExamined
        {
            get
            {
                return (string)dicom[t.BodyPartExamined].Value;
            }
            set
            {
                dicom[t.BodyPartExamined].Value = value;
            }
        }

        public string PatientPosition
        {
            get
            {
                return (string)dicom[t.PatientPosition].Value;
            }
            set
            {
                dicom[t.PatientPosition].Value = value;
            }
        }

        public string ViewPosition
        {
            get
            {
                return (string)dicom[t.ViewPosition].Value;
            }
            set
            {
                dicom[t.ViewPosition].Value = value;
            }
        }

        public string SeriesDescription
        {
            get
            {
                return (string)dicom[t.SeriesDescription].Value;
            }
            private set
            {
                dicom[t.SeriesDescription].Value = value.ToString();
            }
        }

        IEnumerator<Image> IEnumerable<Image>.GetEnumerator()
        {
            foreach (Image temp in children)
            {
                yield return temp;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Image>)this).GetEnumerator();
        }

        public Image NewImage(string path)
        {
            Image temp = new Image(path);
            children.Add(temp);
            return temp;
        }

    }

    #endregion Series

    #region Image

    public class Image : DirectoryRecord
    {
        public Image()
        {
            Initialize(true);
        }

        public Image(string path) : this(new Elements())
        {
            Initialize(false);
            ReferencedFileID = path;
        }

        public Image(Elements elements) : base(elements, "IMAGE")
        {
            Initialize(false);
        }

        public Image(string path, DataSet elements) : base(elements, "IMAGE")
        {
            Initialize(false);
            if (elements.Contains(t.SOPInstanceUID))
                dicom[t.ReferencedSOPInstanceUIDinFile].Value = elements[t.SOPInstanceUID].Value;
            if (elements.Contains(t.SOPClassUID))
                dicom[t.ReferencedSOPClassUIDinFile].Value = elements[t.SOPClassUID].Value;
            if (elements.Contains(t.InstanceNumber))
                dicom[t.InstanceNumber].Value = elements[t.InstanceNumber].Value;
            if (elements.Contains(t.Rows))
                dicom[t.Rows].Value = elements[t.Rows].Value;
            if (elements.Contains(t.Columns))
                dicom[t.Columns].Value = elements[t.Columns].Value;
            if (elements.Contains(t.BitsStored))
                dicom[t.BitsStored].Value = elements[t.BitsStored].Value;
            if (elements.Contains(t.PlateID))
                dicom[t.PlateID].Value = elements[t.PlateID].Value;
            ReferencedFileID = path;
        }

        internal void Initialize(bool query)
        {
            if (query)
            {
                if (!dicom.Contains(t.SOPInstanceUID))
                    dicom.Add(t.SOPInstanceUID, null);
                if (!dicom.Contains(t.StudyInstanceUID))
                    dicom.Add(t.StudyInstanceUID, null);
                if (!dicom.Contains(t.SeriesInstanceUID))
                    dicom.Add(t.SeriesInstanceUID, null);

            }
            else
            {
                if (!dicom.Contains(t.ReferencedSOPInstanceUIDinFile))
                    dicom.Add(t.ReferencedSOPInstanceUIDinFile, null);
                if (!dicom.Contains(t.ReferencedSOPClassUIDinFile))
                    dicom.Add(t.ReferencedSOPClassUIDinFile, null);
            }
            if (!dicom.Contains(t.InstanceNumber))
                dicom.Add(t.InstanceNumber, null);
            if (!dicom.Contains(t.Rows))
                dicom.Add(t.Rows, null);
            if (!dicom.Contains(t.Columns))
                dicom.Add(t.Columns, null);
            if (!dicom.Contains(t.BitsStored))
                dicom.Add(t.BitsStored, null);
            if (!dicom.Contains(t.PlateID))
                dicom.Add(t.PlateID, null);
            if (!dicom.Contains(t.ReferencedFileID))
                dicom.Add(t.ReferencedFileID, null);
        }

        public string ReferencedSOPInstanceUIDinFile
        {
            get
            {
                return (string)dicom[t.ReferencedSOPInstanceUIDinFile].Value;
            }
            set
            {
                dicom[t.ReferencedSOPInstanceUIDinFile].Value = value;
            }
        }

        public string ReferencedSOPClassUIDinFile
        {
            get
            {
                return (string)dicom[t.ReferencedSOPClassUIDinFile].Value;
            }
            set
            {
                dicom[t.ReferencedSOPClassUIDinFile].Value = value;
            }
        }

        public int InstanceNumber
        {
            get
            {
                string number = (string)dicom[t.InstanceNumber].Value;
                return Int32.Parse(number);
            }
            private set
            {
                dicom[t.InstanceNumber].Value = value.ToString();
            }
        }

        public ushort Rows
        {
            get
            {
                return (ushort)dicom[t.Rows].Value;
            }
            set
            {
                dicom[t.Rows].Value = value;
            }
        }

        public ushort Columns
        {
            get
            {
                return (ushort)dicom[t.Columns].Value;
            }
            set
            {
                dicom[t.Columns].Value = value;
            }
        }

        public ushort BitsStored
        {
            get
            {
                return (ushort)dicom[t.BitsStored].Value;
            }
            set
            {
                dicom[t.BitsStored].Value = value;
            }
        }

        public string PlateID
        {
            get
            {
                return (string)dicom[t.PlateID].Value;
            }
            set
            {
                dicom[t.PlateID].Value = value;
            }
        }

        public string ReferencedFileID
        {
            get
            {
                return dicom[t.ReferencedFileID].ToString();
            }
            set
            {
                dicom[t.ReferencedFileID].Value = value;
            }
        }

        public DataSet ReferencedDataSet
        {
            get
            {
                DataSet temp = new DataSet();
                temp.Read(ReferencedFileID);
                return temp;
            }
        }
    }

    #endregion Image

    #region DicomDir

    public class DicomDir : IEnumerable<DataSet>
    {
        #region Fields

        /// <summary>
        /// The working folder and location of the DICOMDIR
        /// </summary>
        private string root;
        /// <summary>
        /// The DICOM representing the patient/study/series/images
        /// </summary>
        private DataSet dicom = new DataSet();
        /// <summary>
        /// The collection of Patients with the DICOMDIR
        /// </summary>
        private List<DirectoryRecord> patients = new List<DirectoryRecord>();
        /// <summary>
        /// The reserved DICOMDIR file id
        /// </summary>
        public static readonly string DicomDirFileId = "DICOMDIR";
        /// <summary>
        /// Just that.
        /// </summary>
        public static readonly string DefaultCharacterSet = "ISO_IR 100";
        /// <summary>
        /// The name of the folder in which the images will be saved.
        /// </summary>
        public static readonly string ImagesFolder = "IMAGES";
        /// <summary>
        /// Whether or not the object is in append mode.
        /// </summary>
        /// <remarks>See <see cref="AppendMode">AppendMode</see></remarks>
        private bool append = false;

        #endregion Fields

        #region Construction and Initialization

        /// <summary>
        /// Construct a new DicomDir with the specified path.
        /// </summary>
        /// <param name="root"></param>
        /// <remarks>If the DICOMDIR already exists at the specified path, the 
        /// isntance is initialized with thcontents of the DICOMDIR.  If the DICOMDIR does
        /// not already exist on will be created at path when <see cref="Save"/> is called.</remarks>
        public DicomDir(string root)
        {
            // we will write a part 10 file with meta-data
            dicom.Part10Header = true;
            dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;

            this.root = root;
            // the root is just the folder that the DICOMDIR is in
            string path = Path.Combine(root, DicomDirFileId);

            // if the DICOMDIR already exists, load it into memory
            if (File.Exists(path))
            {
                Load(path);
            }
            else
            {
                // otherwise setup our empty DICOMDIR with some useful information
                Initialize();
            }

        }

        /// <summary>
        /// Add some required number of tags, including part 10 meta data.
        /// </summary>
        void Initialize()
        {
            dicom.Set(t.GroupLength(2), (ulong)0);
            dicom.Set(t.FileMetaInformationVersion, new byte[] { 0, 1 });
            dicom.Set(t.MediaStorageSOPClassUID, SOPClass.MediaStorageDirectoryStorage);
            dicom.Set(t.MediaStorageSOPInstanceUID, Element.NewUid());
            dicom.Set(t.TransferSyntaxUID, Syntax.ExplicitVrLittleEndian);
            dicom.Set(t.ImplementationClassUID, Element.NewUid());
            dicom.Set(t.ImplementationVersionName, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
            dicom.Add(t.FilesetID, "CARESTREAM");
            dicom.Add(t.OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity, 0);
            dicom.Add(t.OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity, 0);
            dicom.Add(t.FilesetConsistencyFlag, 0);
            dicom.Add(new Element(t.DirectoryRecordSequence, "SQ"));

            patients = new List<DirectoryRecord>();
        }

        /// <summary>
        /// Loads the DICOMDIR at the specified path.
        /// </summary>
        /// <param name="path"></param>
        void Load(string path)
        {
            dicom.Read(path);

            // when we say items, we mean the contents of sequences.
            // we assume that each item in a sequence is written serially in the stream
            // and that the contents of each item is also serial, so if we take the offset
            // of some element within an item, that that offset could be used to order
            // the items.

            // each DirectoryRecord is an item on the DirectoryRecordSequence sequence.
            // we create a collection that associates each item with an offset within the item
            Sequence sequence = (Sequence)dicom[t.DirectoryRecordSequence];
            Dictionary<uint, Elements> records = new Dictionary<uint, Elements>();
            foreach (Elements item in sequence.Items)
            {
                //Console.WriteLine("{0}:{1} NextSibling={2} FirstChild={3}", item[t.DirectoryRecordType].Value, item[t.OffsetoftheNextDirectoryRecord].position - 16,
                ////    item[t.OffsetoftheNextDirectoryRecord].Value, item[t.OffsetofReferencedLowerLevelDirectoryEntity].Value);
                // we figure that OffsetoftheNextDirectoryRecord will be there, and be close to the start
                records.Add((uint)item[t.OffsetoftheNextDirectoryRecord].Position, item);
            }

            // then we walk through the set of items, going to each sub-item based on the 
            // offset specified.

            Elements elements = null;
            // we start off with the first patient
            uint pa = (uint)dicom[t.OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity].Value;
            do
            {
                // we find the closest matching item, and associate that item with this patient.
                elements = Seek(records, pa);
                if (elements != null)
                {
                    // we look for the next patient
                    pa = (uint)elements[t.OffsetoftheNextDirectoryRecord].Value;
                    // creating a patient for this item
                    Patient patient = new Patient(elements);
                    patients.Add(patient);
                    // and then do something similar for each study, series, image
                    uint st = (uint)elements[t.OffsetofReferencedLowerLevelDirectoryEntity].Value;
                    do
                    {
                        elements = Seek(records, st);
                        if (elements != null)
                        {
                            st = (uint)elements[t.OffsetoftheNextDirectoryRecord].Value;
                            Study study = new Study(elements);
                            patient.children.Add(study);
                            uint se = (uint)elements[t.OffsetofReferencedLowerLevelDirectoryEntity].Value;
                            do
                            {
                                elements = Seek(records, se);
                                if (elements != null)
                                {
                                    se = (uint)elements[t.OffsetoftheNextDirectoryRecord].Value;
                                    Series series = new Series(elements);
                                    study.children.Add(series);
                                    uint im = (uint)elements[t.OffsetofReferencedLowerLevelDirectoryEntity].Value;
                                    do
                                    {
                                        elements = Seek(records, im);
                                        if (elements != null)
                                        {
                                            im = (uint)elements[t.OffsetoftheNextDirectoryRecord].Value;
                                            Image image = new Image(elements);
                                            series.children.Add(image);
                                        }
                                    } while (elements != null && im > 0);
                                }
                            } while (elements != null && se > 0);
                        }
                    } while (elements != null && st > 0);
                }
            } while (elements != null && pa > 0);

        }

        #endregion Construction and Initialization

        #region Public Properties

        public List<DirectoryRecord> Patients
        {
            get
            {
                return patients;
            }
        }

        public string MediaStorageSOPClassUID
        {
            get
            {
                return (string)dicom[t.MediaStorageSOPClassUID].Value;
            }
            set
            {
                dicom[t.MediaStorageSOPClassUID].Value = value;
            }
        }

        public string MediaStorageSOPInstanceUID
        {
            get
            {
                return (string)dicom[t.MediaStorageSOPInstanceUID].Value;
            }
            set
            {
                dicom[t.MediaStorageSOPInstanceUID].Value = value;
            }
        }

        public string ImplementationClassUID
        {
            get
            {
                return (string)dicom[t.ImplementationClassUID].Value;
            }
            set
            {
                dicom[t.ImplementationClassUID].Value = value;
            }
        }

        public string ImplementationVersionName
        {
            get
            {
                return (string)dicom[t.ImplementationVersionName].Value;
            }
            set
            {
                dicom[t.ImplementationVersionName].Value = value;
            }
        }

        public string FilesetID
        {
            get
            {
                return (string)dicom[t.FilesetID].Value;
            }
            set
            {
                dicom[t.FilesetID].Value = value;
            }
        }

        public string FilesetDescriptorFileID
        {
            get
            {
                return (string)dicom[t.FilesetDescriptorFileID].Value;
            }
            set
            {
                dicom[t.FilesetDescriptorFileID].Value = value;
            }
        }

        public string SpecificCharacterSetofFilesetDescriptorFile
        {
            get
            {
                return (string)dicom[t.SpecificCharacterSetofFilesetDescriptorFile].Value;
            }
            set
            {
                dicom[t.SpecificCharacterSetofFilesetDescriptorFile].Value = value;
            }
        }

        public uint OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity
        {
            get
            {
                return (uint)dicom[t.OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity].Value;
            }
            set
            {
                dicom[t.OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity].Value = value;
            }
        }

        public uint OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity
        {
            get
            {
                return (uint)dicom[t.OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity].Value;
            }
            internal set
            {
                dicom[t.OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity].Value = value;
            }
        }

        public bool FilesetConsistencyFlag
        {
            get
            {
                ushort flag = (ushort)dicom[t.FilesetConsistencyFlag].Value;
                return (flag == 0);
            }
            internal set
            {
                dicom[t.FilesetConsistencyFlag].Value = (value) ? 0x0000 : 0xffff;
            }
        }

        /// <summary>
        /// Whether or not the DicomDir is in append mode.
        /// </summary>
        /// <remarks>When True, Images added to the same Series are assigned a unique SOPInstanceUID and 
        /// InstanceNumber regardless of the supplied value.  The default is False.
        /// This makes it possible to add different renderings of an image without worrying about unique identifiers.</remarks>
        public bool AppendMode
        {
            get
            {
                return append;
            }
            set
            {
                append = value;
            }
        }

        #endregion Public Properties

        #region Public Methods

        /// <summary>
        /// Adds a new image to the DICOMDIR
        /// </summary>
        /// <param name="path">The path to the DICOM file containing the new image to add.</param>
        /// <remarks>Based on the value of tags found in the dicom file, 
        /// this method adds an image into the patient/study/series hierarchy.  Patients are
        /// identified by thier PatientId, or PatientsName if PatientId is blank.  Studies are
        /// identified by StudyInstanceUid.  Series are identified by SeriesInstanceUID.  Images
        /// are identified by SOPInstanceUID.  If the dicom file contains a Patient, Study or Series
        /// that is already in the DICOMDIR, the image will be inserted into the proper place. 
        /// See <see cref="AppendMode">AppendMode</see></remarks>
        public void Add(string path)
        {
            // parse the file into a collection of tags.
            DataSet dicom = new DataSet();
            // if we are in AppendMode, we have to modify the source dicom file, so we read the entire dataset
            // if we are not in AppendMode we can skip reading the pixel data for speed.
            if (append)
            {
                dicom.Read(path);
            }
            else
            {
                dicom.Read(path, 0x7e00);
            }
            // generate a path for the new image
            string foldering = GetFolderName(dicom);
            Add(dicom, path, foldering);
        }

        /// <summary>
        /// Adds a new image to the DICOMDIR
        /// </summary>
        /// <param name="dicom">A DICOM DataSet containing the new image to add.</param>
        /// <remarks>Based on the value of tags found in the dicom file, 
        /// this method adds an image into the patient/study/series hierarchy.  Patients are
        /// identified by thier PatientId, or PatientsName if PatientId is blank.  Studies are
        /// identified by StudyInstanceUid.  Series are identified by SeriesInstanceUID.  Images
        /// are identified by SOPInstanceUID.  If the dicom file contains a Patient, Study or Series
        /// that is already in the DICOMDIR, the image will be inserted into the proper place. 
        /// See <see cref="AppendMode">AppendMode</see></remarks>
        /// <returns>The newly added Image record.</returns>
        public Image Add(DataSet dicom)
        {
            // generate a path for the new image
            string foldering = GetFolderName(dicom);
            return Add(dicom, null, foldering);
        }

        /// <summary>
        /// Adds a new image to the DICOMDIR
        /// </summary>
        /// <param name="path">The path to the DICOM file containing the new image to add.</param>
        /// <param name="foldering">The folder in the File-set to copy the image to.</param>
        /// <remarks>Based on the value of tags found in the dicom file, 
        /// this method adds an image into the patient/study/series hierarchy.  Patients are
        /// identified by thier PatientId, or PatientsName if PatientId is blank.  Studies are
        /// identified by StudyInstanceUid.  Series are identified by SeriesInstanceUID.  Images
        /// are identified by SOPInstanceUID.  If the dicom file contains a Patient, Study or Series
        /// that is already in the DICOMDIR, the image will be inserted into the proper place. 
        /// See <see cref="AppendMode">AppendMode</see></remarks>
        /// <returns>The newly added Image record.</returns>
        public Image Add(string path, string foldering)
        {
            // parse the file into a collection of tags.
            DataSet dicom = new DataSet();
            // if we are in AppendMode, we have to modify the source dicom file, so we read the entire dataset
            // if we are not in AppendMode we can skip reading the pixel data for speed.
            if (append)
            {
                dicom.Read(path);
            }
            else
            {
                dicom.Read(path, 0x7e00);
            }
            return Add(dicom, path, foldering);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dicom"></param>
        /// <param name="foldering"></param>
        /// <returns>The newly added Image record.</returns>
        public Image Add(DataSet dicom, string foldering)
        {
            return Add(dicom, null, foldering);
        }

        /// <summary>
        /// Adds a new image to the DICOMDIR
        /// </summary>
        /// <param name="dicom">A DICOM DataSet containing the new image to add.</param>
        /// <param name="source"></param>
        /// <param name="foldering">The folder in the File-set to copy the image to.</param>
        /// <remarks>Based on the value of tags found in the dicom file, 
        /// this method adds an image into the patient/study/series hierarchy.  Patients are
        /// identified by thier PatientId, or PatientsName if PatientId is blank.  Studies are
        /// identified by StudyInstanceUid.  Series are identified by SeriesInstanceUID.  Images
        /// are identified by SOPInstanceUID.  If the dicom file contains a Patient, Study or Series
        /// that is already in the DICOMDIR, the image will be inserted into the proper place. 
        /// See <see cref="AppendMode">AppendMode</see></remarks>
        /// <returns>The newly added Image record.</returns>
        internal Image Add(DataSet dicom, string source, string foldering)
        {
            /* Through the OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity and
            OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity fields, the DICOMDIR keeps
            track of the first and last patient records.  Then the directory records are listed.
            Each patient has a collection of studies, which has a collection of series, which has
            a collection of Images. Each directory record (patient) keeps track of the 
            next sibling (next patient) through the OffsetoftheNextDirectoryRecord and the first
            child (study) through the OffsetofReferencedLowerLevelDirectoryEntity. */

            Patient patient = null;
            Study study = null;
            Series series = null;
            Image image = null;

            try
            {
                // make sure that the folder exists
                DirectoryInfo folder = Directory.CreateDirectory(Path.Combine(root, foldering));

                string name = GetSequentialFileName(folder.FullName);

                // and put it all together for the full path name
                string path = Path.Combine(foldering, name);

                // first try to find a matching patient based on PatientID
                patient = (Patient)Find(t.PatientID, dicom, t.PatientID, patients);
                // if that fails we will try to match a patient on PatientsName
                if (patient == null)
                {
                    patient = (Patient)Find(t.PatientName, dicom, t.PatientName, patients);
                }
                if (patient != null)
                {
                    // we already have images for this patient, now see if we have an existing study
                    study = (Study)Find(t.StudyInstanceUID, dicom, t.StudyInstanceUID, patient.children);
                    if (study != null)
                    {
                        // see if we have an existing series
                        series = (Series)Find(t.SeriesInstanceUID, dicom, t.SeriesInstanceUID, study.children);
                        if (series != null)
                        {
                            // see if we have an existing image based on SOPInstanceUID
                            image = (Image)Find(t.SOPInstanceUID, dicom, t.ReferencedSOPInstanceUIDinFile, series.children);
                            // if we already have the image and not in append mode, we throw an exception
                            if (image != null && !append)
                            {
                                throw new Exception(String.Format("duplicate SOPInstanceUID={0}.", image.ReferencedSOPInstanceUIDinFile));
                            }
                            else if (append)
                            {
                                // set the SOPInstanceUID to a unique value
                                dicom.Set(t.SOPInstanceUID, Element.NewUid());
                                // find the maximum InstanceNumber in this series
                                int max = 0;
                                foreach (Image temp in series.children)
                                {
                                    max = Math.Max(max, temp.InstanceNumber);
                                }
                                // and set the InstanceNumber to a unique value
                                dicom.Set(t.InstanceNumber, max + 1);
                            }
                            // we have a new image, we just need to add it to the series
                            image = new Image(path, dicom);
                            series.children.Add(image);
                        }
                        else
                        {
                            // we have a new series, we have to update the study
                            image = new Image(path, dicom);
                            series = new Series(dicom);

                            series.children.Add(image);
                            study.children.Add(series);
                         }
                    }
                    else
                    {
                        // we have a new study, we have to update the patient
                        image = new Image(path, dicom);
                        series = new Series(dicom);
                        study = new Study(dicom);

                        series.children.Add(image);
                        study.children.Add(series);
                        patient.children.Add(study);
                    }
                }
                else
                {
                    image = new Image(path, dicom);
                    series = new Series(dicom);
                    study = new Study(dicom);
                    patient = new Patient(dicom);

                    series.children.Add(image);
                    study.children.Add(series);
                    patient.children.Add(study);
                    this.patients.Add(patient);
                }

                // write or copy the file to the images folder.
                string target = Path.Combine(root, path);
                if (source == null || source == String.Empty)
                {
                    dicom.Write(target);
                }
                else
                {
                    File.Copy(source, target);
                    FileInfo info = new FileInfo(target);
                    info.IsReadOnly = false;
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogLevel.Error, ex.Message);
                throw ex;
            }
            return image;
        }

        /// <summary>
        /// Write the DICOMDIR to path
        /// </summary>
        public void Save()
        {

            /* Through the OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity and
            OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity fields, the DICOMDIR keeps
            track of the first and last patient records.  Then the directory records are listed.
            Each patient has a collection of studies, which has a collection of series, which has
            a collection of Images. Each directory record (patient) keeps track of the 
            next sibling (next patient) through the OffsetoftheNextDirectoryRecord and the first
            child (study) through the OffsetofReferencedLowerLevelDirectoryEntity. */

            // whatever our sequence is, we are going to replace it, 
            // and add items for each DirectoryRecord
            Sequence sequence = new Sequence(t.DirectoryRecordSequence);
            dicom[t.DirectoryRecordSequence] = sequence;

            // as we add each item to our sequence we keep a running tab of the size we will
            // take up in the stream in order to set the offsets properly
            // our size in the stream will be the size of our metadata, our elements, the
            // 128 byte header and the DICM preamble that are the normal part of the DICOM header.
            ulong size = dicom.elements.GetSize(dicom.TransferSyntaxUID) + dicom.metadata.GetSize(dicom.TransferSyntaxUID) + 128 + 4;
            // save the offset of the very first patient record
            dicom[t.OffsetoftheFirstDirectoryRecordoftheRootDirectoryEntity].Value = size;
            ulong last = size;
            foreach (Patient patient in patients)
            {
                // we add the patient record
                sequence.Items.Add(patient.dicom);
                // and recalculate the size for the next offset
                last = size;
                size = dicom.elements.GetSize(dicom.TransferSyntaxUID) + dicom.metadata.GetSize(dicom.TransferSyntaxUID) + 128 + 4;
                // the first study will be saved immediatley after the patient, if any
                if (patient.Children.Count > 0)
                {
                    patient.OffsetFirstChild = (uint)size;
                }
                // then we iterate throught he children saving offsets as we go
                foreach (Study study in patient)
                {
                    sequence.Items.Add(study.dicom);
                    last = size;
                    size = dicom.elements.GetSize(dicom.TransferSyntaxUID) + dicom.metadata.GetSize(dicom.TransferSyntaxUID) + 128 + 4;
                    if (study.Children.Count > 0)
                    {
                        study.OffsetFirstChild = (uint)size;
                    }
                    foreach (Series series in study)
                    {
                        sequence.Items.Add(series.dicom);
                        last = size;
                        size = dicom.elements.GetSize(dicom.TransferSyntaxUID) + dicom.metadata.GetSize(dicom.TransferSyntaxUID) + 128 + 4;
                        if (series.Children.Count > 0)
                        {
                            series.OffsetFirstChild = (uint)size;
                        }
                        foreach (Image image in series)
                        {
                            sequence.Items.Add(image.dicom);
                            last = size;
                            size = dicom.elements.GetSize(dicom.TransferSyntaxUID) + dicom.metadata.GetSize(dicom.TransferSyntaxUID) + 128 + 4;
                            // an image has no children, but it does have siblings
                            image.OffsetNextRecord = (uint)size;
                        }
                        // we reset the last image to zero to indicate that it has no siblings.
                        series.children[series.children.Count - 1].OffsetNextRecord = 0;
                        series.OffsetNextRecord = (uint)size;
                    }
                    // we reset the last series to zero to indicate that it has no siblings.
                    study.children[study.children.Count - 1].OffsetNextRecord = 0;
                    study.OffsetNextRecord = (uint)size;
                }
                // we reset the last study to zero to indicate that it has no siblings
                patient.children[patient.children.Count - 1].OffsetNextRecord = 0;
                patient.OffsetNextRecord = (uint)size;
            }
            // we reset the last patient to zero to indicate that it has no siblings, if any
            if (patients.Count > 0)
            {
                patients[patients.Count - 1].OffsetNextRecord = 0;
            }
            dicom[t.OffsetoftheLastDirectoryRecordoftheRootDirectoryEntity].Value = last;

            // now we can write out the whole DataSet with the offsets
            string path = Path.Combine(root, DicomDirFileId);
            dicom.Write(path);

        }

        public void Empty()
        {
            Clean(Path.Combine(root, ImagesFolder));
            FileInfo file = new FileInfo(Path.Combine(root, DicomDirFileId));
            if (file.Exists)
                file.Delete();
            dicom = new DataSet();
            dicom.Part10Header = true;
            dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;
            Initialize();
        }

        void Clean(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (dir.Exists)
            {
                foreach (DirectoryInfo child in dir.GetDirectories())
                {
                    Clean(child.FullName);
                }
                foreach (FileInfo file in dir.GetFiles())
                {
                    if ((file.Attributes & FileAttributes.ReadOnly) != 0)
                        file.Attributes ^= FileAttributes.ReadOnly;
                    file.Delete();
                }
                dir.Delete();
            }
        }

        /// <summary>
        /// Adds a new patient ot the DICOMDIR.
        /// </summary>
        /// <param name="name">The patient name.</param>
        /// <param name="id">The patient id.</param>
        /// <returns>A new patient.</returns>
        /// <remarks>Any other fields besides name and id are defaulted. You must manually
        /// add the associated studies, series, images, etc.  Each type of record has
        /// a method to add a child.</remarks>
        public Patient NewPatient(string name, string id)
        {
            Patient temp = new Patient(name, id);
            patients.Add(temp);
            return temp;
        }

        /// <summary>
        /// Use to iterate over each referenced dicom dataset.
        /// </summary>
        /// <returns></returns>
        IEnumerator<DataSet> IEnumerable<DataSet>.GetEnumerator()
        {
            foreach (Patient patient in patients)
            {
                foreach (Study study in patient)
                {
                    foreach (Series series in study)
                    {
                        foreach (Image image in series)
                        {
                            yield return image.ReferencedDataSet;
                        }
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<DataSet>)this).GetEnumerator();
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Used to find a record that is closest to the specified offset.
        /// </summary>
        /// <param name="records"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private Elements Seek(Dictionary<uint, Elements> records, uint index)
        {
            // the key is the offset, so we iterate the keys
            foreach (uint key in records.Keys)
            {
                // and as soon as we find a key that is greater than or equal to our target
                // we assume that we have found our item
                if (key >= index)
                    return records[key];
            }
            return null;
        }

        /// <summary>
        /// Find a child record that matches on a specific value.
        /// </summary>
        /// <param name="tag1">The tag value in the DataSet that contains the value to search for.</param>
        /// <param name="elements">The DataSet containing the value to search for.</param>
        /// <param name="tag2">The tag value in the DicomDir child records to compare against.</param>
        /// <param name="container">The DicomDir child records to search in for the match.</param>
        /// <returns></returns>
        private DirectoryRecord Find(string tag1, DataSet elements, string tag2, List<DirectoryRecord> siblings)
        {
            string value;
            Element element = null;
            DirectoryRecord record = null;

            try
            {
                element = elements[tag1];
                // first get the value from the MElementList that we will try to match
                if (element == null)
                {
                    return null;
                }
                value = (string)element.Value;

                // now iterate each child and search for the match
                foreach (DirectoryRecord sibling in siblings)
                {
                    // extract the other field from the child record.
                    element = sibling.dicom[tag2];
                    {
                        // only compare if we have something
                        if (element != null)
                        {
                            string text = (string)element.Value;
                            if (text == value)
                            {
                                record = sibling;
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return record;
        }

        /// <summary>
        /// Create a unique, sequential path name for a new image.
        /// </summary>
        /// <param name="elements"></param>
        /// <returns>A pathname relative to root.</returns>
        /// <remarks>The path is IMAGES\modality\date\time, where
        /// modality is the modality, date is the StudyDate, time is the
        /// StudyTime.</remarks>
        private string GetFolderName(DataSet elements)
        {
            string path;
            try
            {
                string modality, date, time;
                System.DateTime now = System.DateTime.Now;

                // Modality
                modality = elements.ValueExists(t.Modality) ? (string)elements[t.Modality].Value : "UN";
                modality = ConvertString(modality);

                // StudyDate
                date = elements.ValueExists(t.StudyDate) ? (string)elements[t.StudyDate].Value :
                    now.ToString("yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                date = ConvertString(date);

                // StudyTime
                time = elements.ValueExists(t.StudyTime) ? (string)elements[t.StudyTime].Value :
                    now.ToString("HHmmss", System.Globalization.CultureInfo.InvariantCulture);
                time = ConvertString(time);

                // put it all together for the folder name
                path = String.Format(@"{0}\{1}\{2}\{3}", ImagesFolder.ToUpper(), modality, date, time);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return path;
        }

        /// <summary>
        /// This method returns the next highest sequential file name for a folder.
        /// </summary>
        /// <param name="path">The context folder.</param>
        /// <returns>The next highest file name that shoul dbe added to the folder.</returns>
        public static string GetSequentialFileName(string path)
        {
            int maximum = 0;
            DirectoryInfo folder = new DirectoryInfo(path);
            if (folder.Exists)
            {
                foreach(FileInfo file in folder.GetFiles())
                {
                    if (file.Extension.ToUpper() == "")
                    {
                        int result;
                        if (Int32.TryParse(file.Name, out result))
                        {
                            maximum = Math.Max(maximum, result);
                        }
                    }
                }
            }
            return String.Format("{0:00000000}", maximum+1);
        }

        /// <summary>
        ///  Converts a string to one that can be used as a DICOMDIR File ID or File-set ID.
        /// </summary>
        /// <param name="text">The string to convert.</param>
        /// <returns>The input string stripped of invalid characters, converted to uppercase and truncated to eight characters.</returns>
        private string ConvertString(string text)
        {
            // restricted by Part 10: Media Storage and File Format for Media Interchange ... DICOM File Service ... CHARACTER SET
            string temp = Regex.Replace(text.ToUpper(), @"[^A-Z0-9_]", "");
            if(temp.Length > 8)
                temp = temp.Substring(0,8);
            return temp;

        }

        /// <summary>
        /// Used for debugging purposes for dumping the current record offsets.
        /// </summary>
        private void DumpOffsets()
        {
            foreach (Patient patient in patients)
            {
                Console.WriteLine("patient next={0} child={1}", patient.OffsetNextRecord, patient.OffsetFirstChild);
                foreach (Study study in patient)
                {
                    Console.WriteLine("\tstudy next={0} child={1}", study.OffsetNextRecord, study.OffsetFirstChild);
                    foreach (Series series in study)
                    {
                        Console.WriteLine("\t\tseries next={0} child={1}", series.OffsetNextRecord, series.OffsetFirstChild);
                        foreach (Image image in series)
                        {
                            Console.WriteLine("\t\t\timage next={0} child={1}", image.OffsetNextRecord, image.OffsetFirstChild);
                        }
                    }
                }
            }
        }

        #endregion Private Methods

    }

    #endregion DicomDir
}
