using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for CMoveTest
    /// </summary>
    [TestClass]
    public class CMoveTest
    {
        static Server server = null;

        public CMoveTest()
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

        [TestMethod]
        public void InternalCMoveTest()
        {
            // make sure DICOM at "." is empty
            DicomDir dir = new DicomDir(".");
            dir.Empty();

            ApplicationEntity storage = new ApplicationEntity("ImageServer", IPAddress.Parse("127.0.0.1"), 2000);
            ApplicationEntity server = new ApplicationEntity("ImageServer", IPAddress.Parse("127.0.0.1"), 5104);

            Dictionary<string, ApplicationEntity> stations = new Dictionary<string, ApplicationEntity>();
            stations.Add(storage.Title, storage);

            StorageTest.Start(storage);
            CMoveTest.Start(server, stations);

            // add three images to a DICOMDIR at "."
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\Y2ASNFDS.dcm", storage, false);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm", storage, false);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", storage, false);

            // ask for an image from a DICOMDIR at "." be delivered to a DICOMDIR at "."
            // should create a duplicate SOPInstanceUID, which results in a WARNING
            try
            {
                move(storage.Title, server);
            }
            catch(Exception)
            {
            }

            CMoveTest.Stop();
            StorageTest.Stop();
        }

        [Ignore]
        public void ExternalCGetTest()
        {
            ApplicationEntity host = new ApplicationEntity("COMMON", IPAddress.Parse("127.0.0.1"), 104);
            get(host);
        }

        [TestMethod]
        public void EditorTest()
        {
            Process pacs = new Process();
            pacs.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomEditor\bin\Debug\DicomEditor.exe";

            ApplicationEntity editor = new ApplicationEntity("DICOMEDITOR", IPAddress.Parse("127.0.0.1"), 2009);
            ApplicationEntity console = new ApplicationEntity("CONSOLE", IPAddress.Parse("127.0.0.1"), 2010);

            try
            {
                pacs.Start();
                StorageTest.Start(console);
            }
            catch
            {
                pacs.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomEditor\bin\Debug\DicomEditor.exe";
                pacs.Start();
            }
            Thread.Sleep(1000);

            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", editor, false);

            RecordCollection records;
            Dictionary<string, string> filter;
            
            filter = new Dictionary<string,string>();
            filter.Add(t.PatientID, "759275");
            records = CFindTest.Query(editor, "STUDY", filter);
            if (records != null)
            {
                filter = new Dictionary<string,string>();
                filter.Add(t.StudyInstanceUID, (string)records[0][t.StudyInstanceUID].Value);
                records = CFindTest.Query(editor, "SERIES", filter);
                if (records != null)
                {
                    filter = new Dictionary<string, string>();
                    filter.Add(t.SeriesInstanceUID, (string)records[0][t.SeriesInstanceUID].Value);
                    records = CFindTest.Query(editor, "IMAGE", filter);
                    if (records != null)
                    {

                        CMoveServiceSCU move = new CMoveServiceSCU(SOPClass.StudyRootQueryRetrieveInformationModelMOVE);
                        move.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
                        move.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                        Association association = new Association();
                        association.AddService(move);

                        if (association.Open(editor))
                        {
                            if (move.Active)
                            {
                                move.ImageMoved += new CMoveEventHandler(OnImageMoved);
                                try
                                {
                                    //Element element = new Element(t.SOPInstanceUID, records[0][t.ReferencedSOPInstanceUIDinFile].Value);
                                    Element element = new Element(t.SOPInstanceUID, records[0][t.SOPInstanceUID].Value);
                                    DataSet results = move.CMove(console.Title, element);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine(ex.Message);
                                    throw;
                                }
                                finally
                                {
                                    move.ImageMoved -= new CMoveEventHandler(OnImageMoved);
                                }
                            }
                        }
                        else
                        {
                            Debug.WriteLine("\ncan't Open.");
                        }

                        association.Close();
                    }
                }
            }
            pacs.Kill();
        }

        [TestMethod]
        public void BaseQueryTest()
        {
            DicomDir dir = new DicomDir(".");

            DateTime now = DateTime.Now;

            // create a patient with two studies, each with one series, 
            // the first series with one image, the second with two
            Patient patient = dir.NewPatient("10", "10");
            Study study = patient.NewStudy(now, now, "10", "10");
            Series series = study.NewSeries("CR", "10");
            Image image = series.NewImage("10"); image.ReferencedSOPInstanceUIDinFile = "10";
            study = patient.NewStudy(now, now, "20", "20");
            series = study.NewSeries("CR", "20");
            image = series.NewImage("20"); image.ReferencedSOPInstanceUIDinFile = "20";
            image = series.NewImage("30"); image.ReferencedSOPInstanceUIDinFile = "30";

            // create a patient with one study, containing two series, 
            // the first series with two images, the second with one
            patient = dir.NewPatient("20", "20");
            study = patient.NewStudy(now, now, "30", "30");
            series = study.NewSeries("CR", "30");
            image = series.NewImage("40"); image.ReferencedSOPInstanceUIDinFile = "40";
            image = series.NewImage("50"); image.ReferencedSOPInstanceUIDinFile = "50";
            series = study.NewSeries("CR", "40");
            image = series.NewImage("60"); image.ReferencedSOPInstanceUIDinFile = "60";

            // create a patient with a single study and series containing thrww images
            patient = dir.NewPatient("30", "30");
            study = patient.NewStudy(now, now, "40", "40");
            series = study.NewSeries("CR", "50");
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

            Assert.AreEqual("10,20,30", query("PATIENT", "10"));
            Assert.AreEqual("10", query("STUDY", "10"));
            Assert.AreEqual("10", query("SERIES", "10"));
            Assert.AreEqual("20,30", query("STUDY", "20"));
            Assert.AreEqual("20,30", query("SERIES", "20"));

            Assert.AreEqual("40,50,60", query("PATIENT", "20"));
            Assert.AreEqual("40,50,60", query("STUDY", "30"));
            Assert.AreEqual("40,50", query("SERIES", "30"));
            Assert.AreEqual("60", query("SERIES", "40"));

            Assert.AreEqual("70,80,90", query("PATIENT", "30"));
            Assert.AreEqual("70,80,90", query("STUDY", "40"));
            Assert.AreEqual("70,80,90", query("SERIES", "50"));

            Assert.AreEqual("10", query("IMAGE", "10"));
            Assert.AreEqual("20", query("IMAGE", "20"));
            Assert.AreEqual("30", query("IMAGE", "30"));
            Assert.AreEqual("40", query("IMAGE", "40"));
            Assert.AreEqual("50", query("IMAGE", "50"));
            Assert.AreEqual("60", query("IMAGE", "60"));
            Assert.AreEqual("70", query("IMAGE", "70"));
            Assert.AreEqual("80", query("IMAGE", "80"));
            Assert.AreEqual("90", query("IMAGE", "90"));

            dir.Empty();
        }

        static string query(string level, string id)
        {
            CMoveServiceSCP service = new CMoveServiceSCP(SOPClass.PatientRootQueryRetrieveInformationModelGET);

            DataSet dicom = new DataSet();

            dicom.Set(t.QueryRetrieveLevel, level);
            if (level == "PATIENT")
            {
                dicom.Set(t.PatientID, id);
            }
            else if (level == "STUDY")
            {
                dicom.Set(t.StudyInstanceUID, id);
            }
            else if (level == "SERIES")
            {
                dicom.Set(t.SeriesInstanceUID, id);
            }
            else if (level == "IMAGE")
            {
                dicom.Set(t.SOPInstanceUID, id);
            }

            string text = string.Empty;
            foreach (string image in service.Query(dicom))
            {
                if (text.Length > 1)
                    text += ",";
                text += image;
            }
            return text;
        }

        static void get(ApplicationEntity host)
        {
            CMoveServiceSCU get = new CMoveServiceSCU(SOPClass.PatientRootQueryRetrieveInformationModelGET);
            get.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP store = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForPresentation);
            store.Role = Role.Scp;
            store.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            store.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            VerificationServiceSCU echo = new VerificationServiceSCU();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(store);
            association.AddService(get);
            association.AddService(echo);

            if (association.Open(host))
            {
                if (get.Active && store.Active)
                {
                    store.ImageStored += new ImageStoredEventHandler(OnImageStored);
                    try
                    {
                        Element element = new Element(t.PatientID, "759274");
                        DataSet results = get.CGet(element);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                    finally
                    {
                        store.ImageStored -= new ImageStoredEventHandler(OnImageStored);
                    }
                }
            }
            else
            {
                Debug.WriteLine("\ncan't Open.");
            }

            association.Close();
        }

        static void move(string destination, ApplicationEntity host)
        {
            CMoveServiceSCU pm = new CMoveServiceSCU(SOPClass.PatientRootQueryRetrieveInformationModelMOVE);
            pm.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            pm.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            CFindServiceSCU pf = new CFindServiceSCU(SOPClass.PatientRootQueryRetrieveInformationModelFIND);
            pf.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            pf.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            CMoveServiceSCU sm = new CMoveServiceSCU(SOPClass.StudyRootQueryRetrieveInformationModelMOVE);
            sm.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            sm.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            CFindServiceSCU sf = new CFindServiceSCU(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            sf.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            sf.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(pm);
            association.AddService(pf);
            association.AddService(sm);
            association.AddService(sf);

            if (association.Open(host))
            {
                if (sm.Active)
                {
                    sm.ImageMoved += new CMoveEventHandler(OnImageMoved);
                    try
                    {
                        Element element = new Element(t.StudyInstanceUID, "1.2.840.113564.109517115.2009111711190101521");
                        DataSet results = sm.CMove(destination, element);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        throw;
                    }
                    finally
                    {
                        sm.ImageMoved -= new CMoveEventHandler(OnImageMoved);
                    }
                }
            }
            else
            {
                Debug.WriteLine("\ncan't Open.");
            }

            association.Close();
        }

        public static void Start(ApplicationEntity host)
        {
            Start(host, null);
        }

        public static void Start(ApplicationEntity host, Dictionary<string, ApplicationEntity> stations)
        {
            if (server != null)
            {
                throw new Exception("CMoveTest.Server in use.");
            }

            server = new Server(host.Title, host.Port);

            server.Hosts = stations;

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CMoveServiceSCP pm = new CMoveServiceSCP(SOPClass.PatientRootQueryRetrieveInformationModelMOVE);
            pm.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            pm.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CMoveServiceSCP pf = new CMoveServiceSCP(SOPClass.PatientRootQueryRetrieveInformationModelFIND);
            pf.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            pf.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CMoveServiceSCP sm = new CMoveServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelMOVE);
            sm.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            sm.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            CMoveServiceSCP sf = new CMoveServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            sf.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            sf.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(pm);
            server.AddService(pf);
            server.AddService(sm);
            server.AddService(sf);

            server.Start();
        }

        public static void Stop()
        {
            server.Stop();
            server = null;
        }

        public static void OnImageStored(object sender, ImageStoredEventArgs e)
        {
            DataSet dicom = e.DataSet;
            dicom.Write("finky.dcm");
       }

        public static void OnImageMoved(object sender, CMoveEventArgs e)
        {
            Logging.Log(e.ToString());
        }

    }
}
