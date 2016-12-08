using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for CFindTest
    /// </summary>
    [TestClass]
    public class CFindTest
    {
        static Server server = null;

        public CFindTest()
        {
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void ClassInitialize(TestContext testContext)
        {
            Logging.LogLevel = LogLevel.Verbose;
        }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [Ignore]
        public void ServerTest()
        {
            string title = "Worklist_SCP";
            int port = 6104;

            Server server = new Server(title, port);

            CFindServiceSCP ris = new CFindServiceSCP(SOPClass.ModalityWorklistInformationModelFIND);
            ris.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            ris.Query += new QueryEventHandler(OnQuery);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(ris);
            server.AddService(echo);

            server.Start();

            System.Windows.Forms.MessageBox.Show("Click OK to stop Server.");

            server.Stop();
        }

        [TestMethod]
        public void InternalQueryTest()
        {
            string title = "QUERY";
            int port = 2104;

            Server server = new Server(title, port);

            CFindServiceSCP ris = new CFindServiceSCP(SOPClass.ModalityWorklistInformationModelFIND);
            ris.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            ris.Query += new QueryEventHandler(OnQuery);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(ris);
            server.AddService(echo);

            server.Start();

            Dictionary<string, string> filter = new Dictionary<string, string>();

            filter.Add(t.PatientName, "*");
            filter.Add(t.ScheduledProcedureStepSequence + t.Modality, "CR");
            filter.Add(t.ScheduledProcedureStepSequence + t.ScheduledProcedureStepStartDate, DateTime.Now.ToString("yyyyMMdd"));

            mwl(title, IPAddress.Parse("127.0.0.1"), port, filter);

            server.Stop();
        }

        [TestMethod]
        public void InternalQueryTest2()
        {
            DicomDir dir = new DicomDir(".");
            dir.Empty();

            ApplicationEntity storage = new ApplicationEntity("ImageServer", IPAddress.Parse("127.0.0.1"), 2000);
            StorageTest.Start(storage);

            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\Y2ASNFDS.dcm", storage, false);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm", storage, false);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", storage, false);

            StorageTest.Stop();

            ApplicationEntity host = new ApplicationEntity("IMAGESERVER", 2190);
            CFindTest.Start(host);

            CFindServiceSCU find = new CFindServiceSCU(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            find.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(find);

            if (association.Open(host.Title, host.Address, host.Port))
            {
                Series series = new Series();
                series[t.StudyInstanceUID].Value = "1.2.840.113564.109517115.2009111711190101521";

                DataSet filter = new DataSet();
                filter.Elements = series.Elements;

                filter.Set(t.QueryRetrieveLevel, "SERIES");

                RecordCollection records = find.CFind(filter);
                RecordCollectionTest.WriteRecords(records);
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            association.Close();
            dir.Empty();

            CFindTest.Stop();
        }

        [Ignore]
        public void ExternalMWLQueryTest()
        {
            string title = "plutoFIR";
            IPAddress address = IPAddress.Parse("10.95.16.141");
            int port = 109;

            Dictionary<string, string> filter = new Dictionary<string, string>();

            filter.Add(t.PatientName, "*");
            filter.Add(t.ScheduledProcedureStepSequence + t.Modality, "CR");
            filter.Add(t.ScheduledProcedureStepSequence + t.ScheduledProcedureStepStartDate, DateTime.Now.ToString("yyyyMMdd"));

            mwl(title, address, port, filter);
        }

        ManualResetEvent stop;

        [TestMethod]
        public void ExternalMWLQueryStressTest()
        {
            int count = 100;

            stop = new ManualResetEvent(false);
            System.Threading.Thread[] threads = new System.Threading.Thread[count];
            for(int n = 0; n < threads.Length; n++)
            {
                threads[n] = new Thread(new ThreadStart(BackgroundThread));
                threads[n].Name = "BackgroundThread";
                threads[n].Start();
            }
            System.Threading.Thread.Sleep(30000);
            stop.Set();
            foreach(System.Threading.Thread thread in threads)
            {
                if (!thread.Join(1000))
                {
                    System.Diagnostics.Debug.WriteLine("thread did not die!");
                }
            }
        }

        public void BackgroundThread()
        {
            string title = "DICOMWORKLIST";
            IPAddress address = IPAddress.Parse("127.0.01");
            int port = 6104;

            Dictionary<string, string> filter = new Dictionary<string, string>();

            filter.Add(t.PatientName, "*");
            filter.Add(t.ScheduledProcedureStepSequence + t.Modality, "CR");
            filter.Add(t.ScheduledProcedureStepSequence + t.ScheduledProcedureStepStartDate, DateTime.Now.ToString("yyyyMMdd"));

            for (; ; )
            {
                mwl(title, address, port, filter);
                if (stop.WaitOne(0))
                    break;
            }
        }

        [Ignore]
        public void ExternalPatientQueryTest()
        {
            ApplicationEntity host = new ApplicationEntity("plutoFIR", IPAddress.Parse("10.95.16.141"), 2104);

            Dictionary<string, string> filter = new Dictionary<string, string>();

            Query(host, "PATIENT", filter);
        }


        [Ignore]
        public void ExternalStudyQueryTest()
        {
            ApplicationEntity host = new ApplicationEntity("plutoFIR", IPAddress.Parse("10.95.16.141"), 2104);

            Dictionary<string, string> filter = new Dictionary<string, string>();

            Query(host, "STUDY", filter);
        }

        [TestMethod]
        public void BaseQueryTest()
        {
            DicomDir dir = new DicomDir(".");

            DateTime now = DateTime.Now;

            // create a patient with two studies, each with one series, 
            // the first series with one image, the second with two
            Patient patient = dir.NewPatient("Patient^Test", "10");
            Study study = patient.NewStudy(now, now, "10", "10");
            Series series = study.NewSeries("CR", "10");
            Image image = series.NewImage("10"); image.ReferencedSOPInstanceUIDinFile = "10";
            study = patient.NewStudy(now, now, "20", "20");
            series = study.NewSeries("CR", "20");
            image = series.NewImage("20"); image.ReferencedSOPInstanceUIDinFile = "20";
            image = series.NewImage("30"); image.ReferencedSOPInstanceUIDinFile = "30";

            // create a patient with one study, containing two series, 
            // the first series with two images, the second with one
            patient = dir.NewPatient("Subject^Test", "20");
            study = patient.NewStudy(now, now, "30", "30");
            series = study.NewSeries("DX", "30");
            image = series.NewImage("40"); image.ReferencedSOPInstanceUIDinFile = "40";
            image = series.NewImage("50"); image.ReferencedSOPInstanceUIDinFile = "50";
            series = study.NewSeries("CR", "40");
            image = series.NewImage("60"); image.ReferencedSOPInstanceUIDinFile = "60";

            // create a patient with a single study and series containing thrww images
            patient = dir.NewPatient("Patietn^Pink", "30");
            study = patient.NewStudy(now, now, "40", "40");
            series = study.NewSeries("MG", "50");
            image = series.NewImage("70"); image.ReferencedSOPInstanceUIDinFile = "70";
            image = series.NewImage("80"); image.ReferencedSOPInstanceUIDinFile = "80";
            image = series.NewImage("90"); image.ReferencedSOPInstanceUIDinFile = "90";

            dir.Save();

            /*  P   ST  SE  IM
             *  10
             *      10
             *          10
             *              10
             *      20
             *          20
             *              20
             *              30
             *  20
             *      30
             *          30
             *              40
             *              50
             *          40
             *              60
             *  30
             *      40
             *          50
             *              70
             *              80
             *              90
             *      
            */

            patient = new Patient();
            RecordCollection records = query("PATIENT", patient.Elements);
            Assert.AreEqual(3, records.Count);

            study = new Study();
            records = query("STUDY", study.Elements);
            Assert.AreEqual(4, records.Count);

            series = new Series();
            records = query("SERIES", series.Elements);
            Assert.AreEqual(5, records.Count);

            image = new Image();
            records = query("IMAGE", image.Elements);
            Assert.AreEqual(9, records.Count);

            dir.Empty();
        }

        public void OnQuery(object sender, QueryEventArgs args)
        {
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Mwl");
            args.Records = new RecordCollection(path, true);
            args.Records.Load();
        }

        static RecordCollection query(string level, Elements filter)
        {
            CFindServiceSCP service = new CFindServiceSCP(SOPClass.PatientRootQueryRetrieveInformationModelFIND);

            filter.Set(t.QueryRetrieveLevel, level);

            return service.InternalQuery(filter);
        }

        public static RecordCollection Query(ApplicationEntity host, string level, Dictionary<string, string> filter)
        {
            RecordCollection records = null;

            CFindServiceSCU study = new CFindServiceSCU(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            study.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            study.Syntaxes.Add(Syntax.ExplicitVrBigEndian);
            study.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(study);

            if (association.Open(host))
            {
                DataSet query = null;
                switch(level)
                {
                    case "PATIENT":
                        query = GetPatientQuery(filter);
                        break;
                    case "STUDY":
                        query = GetStudyQuery(filter);
                        break;
                    case "SERIES":
                        query = GetSeriesQuery(filter);
                        break;
                    case "IMAGE":
                        query = GetImageQuery(filter);
                        break;
                }
                records = study.CFind(query);
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            association.Close();

            return records;
        }

        void mwl(string title, IPAddress address, int port, Dictionary<string, string> filter)
        {
            CFindServiceSCU mwl = new CFindServiceSCU(SOPClass.ModalityWorklistInformationModelFIND);
            mwl.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            mwl.Syntaxes.Add(Syntax.ExplicitVrBigEndian);
            mwl.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mwl);

            if (association.Open(title, address, port))
            {
                DataSet query = GetQuery(filter);

                RecordCollection records = mwl.CFind(query);
                RecordCollectionTest.WriteRecords(records);
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            association.Close();
        }

        public static DataSet GetPatientQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.QueryRetrieveLevel, "PATIENT");
            dicom.Add(t.PatientName, null);
            dicom.Add(t.PatientID, null);
            dicom.Add(t.PatientBirthDate, null);
            dicom.Add(t.PatientBirthTime, null);
            dicom.Add(t.PatientSex, null);
            dicom.Add(t.OtherPatientIDs, null);
            dicom.Add(t.OtherPatientNames, null);
            dicom.Add(t.PatientAge, null);
            dicom.Add(t.PatientSize, null);
            dicom.Add(t.PatientWeight, null);
            dicom.Add(t.EthnicGroup, null);
            dicom.Add(t.Occupation, null);
            dicom.Add(t.AdditionalPatientHistory, null);
            dicom.Add(t.PatientComments, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if (entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        public static DataSet GetStudyQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.StudyDate, null);
            dicom.Add(t.StudyTime, null);
            dicom.Add(t.AccessionNumber, null);
            dicom.Add(t.QueryRetrieveLevel, "STUDY");
            dicom.Add(t.ModalitiesinStudy, null);
            //dicom.Add(t.ReferringPhysiciansName, null);
            dicom.Add(t.StudyDescription, null);
            //dicom.Add(t.PhysiciansofRecord, null);
            //dicom.Add(t.NameofPhysiciansReadingStudy, null);
            //dicom.Add(t.AdmittingDiagnosesDescription, null);
            dicom.Add(t.PatientName, null);
            dicom.Add(t.PatientID, null);
            //dicom.Add(t.PatientsBirthDate, null);
            //dicom.Add(t.PatientsBirthTime, null);
            //dicom.Add(t.PatientsSex, null);
            //dicom.Add(t.OtherPatientIDs, null);
            //dicom.Add(t.OtherPatientNames, null);
            //dicom.Add(t.PatientsAge, null);
            //dicom.Add(t.PatientsSize, null);
            //dicom.Add(t.PatientsWeight, null);
            //dicom.Add(t.EthnicGroup, null);
            //dicom.Add(t.Occupation, null);
            //dicom.Add(t.AdditionalPatientHistory, null);
            //dicom.Add(t.PatientComments, null);
            dicom.Add(t.StudyInstanceUID, null);
            dicom.Add(t.StudyID, null);
            //dicom.Add(t.NumberofPatientRelatedSeries, null);
            //dicom.Add(t.NumberofPatientRelatedInstances, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if (entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        public static DataSet GetSeriesQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.QueryRetrieveLevel, "SERIES");
            dicom.Add(t.Modality, null);
            dicom.Add(t.SeriesDescription, null);
            dicom.Add(t.BodyPartExamined, null);
            dicom.Add(t.PatientPosition, null);
            dicom.Add(t.ViewPosition, null);
            dicom.Add(t.StudyInstanceUID, null);
            dicom.Add(t.SeriesInstanceUID, null);
            dicom.Add(t.SeriesNumber, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if (entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        public static DataSet GetImageQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.SOPClassUID, null);
            dicom.Add(t.SOPInstanceUID, null);
            dicom.Add(t.AcquisitionDate, null);
            dicom.Add(t.AcquisitionTime, null);
            dicom.Add(t.QueryRetrieveLevel, "IMAGE");
            dicom.Add(t.StudyInstanceUID, null);
            dicom.Add(t.SeriesInstanceUID, null);
            dicom.Add(t.InstanceNumber, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if (entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        public static DataSet GetQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            Sequence sequence;
            Elements item;

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.AccessionNumber, null);
            dicom.Add(t.ReferringPhysicianName, null);
            dicom.Add(t.AdmittingDiagnosesDescription, null);
            sequence = new Sequence(t.ReferencedStudySequence);
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.ReferencedSOPClassUID, null);
            item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(t.PatientName, null);
            dicom.Add(t.PatientID, null);
            dicom.Add(t.PatientBirthDate, null);
            dicom.Add(t.PatientBirthTime, null);
            dicom.Add(t.PatientSex, null);
            dicom.Add(t.OtherPatientIDs, null);
            dicom.Add(t.OtherPatientNames, null);
            dicom.Add(t.PatientAge, null);
            dicom.Add(t.PatientSize, null);
            dicom.Add(t.PatientWeight, null);
            dicom.Add(t.MedicalAlerts, null);
            dicom.Add(t.EthnicGroup, null);
            dicom.Add(t.Occupation, null);
            dicom.Add(t.AdditionalPatientHistory, null);
            dicom.Add(t.PregnancyStatus, null);
            dicom.Add(t.PatientComments, null);
            dicom.Add(t.StudyInstanceUID, null);
            dicom.Add(t.RequestingService, null);
            dicom.Add(t.RequestedProcedureDescription, null);
            sequence = new Sequence(t.RequestedProcedureCodeSequence);
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.CodeValue, null);
            item.Add(t.CodingSchemeDesignator, null);
            item.Add(t.CodingSchemeVersion, null);
            item.Add(t.CodeMeaning, null);
            dicom.Add(t.VisitStatusID, null);
            dicom.Add(t.PatientInstitutionResidence, null);
            sequence = new Sequence(t.ScheduledProcedureStepSequence);
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.Modality, null);
            item.Add(t.RequestedContrastAgent, null);
            item.Add(t.ScheduledProcedureStepStartDate, null);
            item.Add(t.ScheduledProcedureStepStartTime, null);
            item.Add(t.ScheduledProcedureStepDescription, null);
            item.Add(t.ScheduledProcedureStepID, null);
            sequence = new Sequence(t.ScheduledProtocolCodeSequence);
            item.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.CodeValue, null);
            item.Add(t.CodingSchemeDesignator, null);
            item.Add(t.CodingSchemeVersion, null);
            item.Add(t.CodeMeaning, null);
            dicom.Add(t.RequestedProcedureID, null);
            dicom.Add(t.RequestedProcedurePriority, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if (entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        public static void Start(ApplicationEntity host)
        {
            if (server != null)
            {
                throw new Exception("CFindTest.Server in use.");
            }

            server = new Server(host.Title, host.Port);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CFindServiceSCP study = new CFindServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            study.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            study.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CFindServiceSCP patient = new CFindServiceSCP(SOPClass.PatientRootQueryRetrieveInformationModelFIND);
            patient.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            patient.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(study);
            server.AddService(patient);

            server.Start();
        }

        public static void Stop()
        {
            server.Stop();
            server = null;
        }

    }
}
