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
    /// Summary description for StorageTest
    /// </summary>
    [TestClass]
    public class StorageTest
    {
        static Server server = null;

        public StorageTest()
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
        public void InternalStorageTest()
        {
            // start the SCP
            ApplicationEntity host = new ApplicationEntity("STORAGE", 2011);
            Start(host, null);

            // create an SCU and store an image
            store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\Y2ASNFDS.dcm", host, true);

            Stop();
        }

        [TestMethod]
        public void ExternalStorageTest()
        {
            ApplicationEntity host = new ApplicationEntity("HARVEST", IPAddress.Parse("127.0.0.1"), 2009);
            //ApplicationEntity host = new ApplicationEntity("plutoFIR", IPAddress.Parse("10.95.16.141"), 2104);
            //ApplicationEntity host = new ApplicationEntity("MESA_IMG_MGR", IPAddress.Parse("10.95.17.121"), 2350);
            //ApplicationEntity host = new ApplicationEntity("DICOMVIEWER", IPAddress.Parse("127.0.0.1"), 2009);
            //ApplicationEntity host = new ApplicationEntity("srkwfmFIR", IPAddress.Parse("10.95.63.11"), 2104);

            store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", host, true);
            //store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm", host, false);
            //store(@"..\..\..\..\forwards.dcm", host, true);
        }

        [TestMethod]
        public void ServerTest()
        {
            ApplicationEntity host = new ApplicationEntity("ROCD4CZ14307CW", 5040);
            StorageTest.Start(host);

            System.Windows.Forms.MessageBox.Show("Click OK to stop Server.");

            StorageTest.Stop();
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

            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", editor, true);

            System.Windows.Forms.MessageBox.Show("Click OK to stop Server.");

            StorageTest.Stop();

            pacs.Kill();
        }

        [TestMethod]
        public void StorageCommitTest()
        {
            ApplicationEntity editor = new ApplicationEntity("DICOMEDITOR", IPAddress.Parse("127.0.0.1"), 2009);
            ApplicationEntity console = new ApplicationEntity("CONSOLE", IPAddress.Parse("127.0.0.1"), 5040);

            StorageTest.Start(console);

            Thread.Sleep(1000);

            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm", editor, true);

            System.Windows.Forms.MessageBox.Show("Click OK to stop Server.");

            StorageTest.Stop();
        }


        public static void Start(ApplicationEntity host)
        {
            Start(host, null);
        }

        public static void Start(ApplicationEntity host, Dictionary<string, ApplicationEntity> stations)
        {
            if (server != null)
            {
                throw new Exception("StorageTest.Server in use.");
            }

            server = new Server(host.Title, host.Port);

            server.Hosts = stations;

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP cr = new StorageServiceSCP(SOPClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            cr.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP dx1 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForPresentation);
            dx1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            dx1.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP dx2 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForProcessing);
            dx2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            dx2.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP mg1 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForPresentation);
            mg1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            mg1.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP mg2 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForProcessing);
            mg2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            mg2.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP gsps = new StorageServiceSCP(SOPClass.GrayscaleSoftcopyPresentationStateStorageSOPClass);
            gsps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            gsps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP sc = new StorageServiceSCP(SOPClass.SecondaryCaptureImageStorage);
            sc.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            sc.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageCommitServiceSCP commit = new StorageCommitServiceSCP();
            commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            commit.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            ImageStoredEventHandler handler = new ImageStoredEventHandler(OnImageStored);
            cr.ImageStored += handler;
            dx1.ImageStored += handler;
            dx2.ImageStored += handler;
            mg1.ImageStored += handler;
            mg2.ImageStored += handler;
            gsps.ImageStored += handler;
            sc.ImageStored += handler;

            commit.StorageCommitReport += new StorageCommitEventHandler(OnStorageCommit);

            server.AddService(echo);
            server.AddService(cr);
            server.AddService(dx1);
            server.AddService(dx2);
            server.AddService(mg1);
            server.AddService(mg2);
            server.AddService(gsps);
            server.AddService(sc);
            server.AddService(commit);

            server.Start();
        }

        public static void Stop()
        {
            server.Stop();
            server = null;
        }

        public static void OnStorageCommit(object sender, StorageCommitEventArgs e)
        {
            if (e.Type == StorageCommitEventType.Report)
            {
                string transaction = (string)e.DataSet[t.TransactionUID].Value;
                e.DataSet.Write(transaction);
            }
            else if (e.Type == StorageCommitEventType.Request)
            {
                string transaction = (string)e.DataSet[t.TransactionUID].Value;
                e.DataSet.Write(transaction);
            }
        }

        public static void OnImageStored(object sender, ImageStoredEventArgs e)
        {
            DataSet dicom = e.DataSet;

            DicomDir dir = new DicomDir(".");
            dir.Add(dicom);
            dir.Save();
        }


        public static void store(string path, ApplicationEntity host, bool request_commit)
        {
            StorageServiceSCU cr = new StorageServiceSCU(SOPClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCU dx = new StorageServiceSCU(SOPClass.DigitalXRayImageStorageForPresentation);
            dx.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            dx.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCU dx2 = new StorageServiceSCU(SOPClass.DigitalXRayImageStorageForProcessing);
            dx2.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            dx2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCU sc = new StorageServiceSCU(SOPClass.SecondaryCaptureImageStorage);
            sc.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            sc.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageCommitServiceSCU commit = new StorageCommitServiceSCU();
            commit.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(cr);
            association.AddService(dx);
            association.AddService(dx2);
            association.AddService(sc);
            association.AddService(commit);

            if (association.Open(host))
            {
                if (cr.Active)
                {
                    DataSet dicom = DataSetTest.GetDataSet(Path.Combine(Tools.RootFolder, path));

                    // insure that the SOPClassUID matches what negotiated and are using
                    dicom.Set(t.SOPClassUID, cr.SOPClassUId);
                    string instance = (string)dicom[t.SOPInstanceUID].Value;

                    try
                    {
                        cr.Store(dicom);
                        Debug.WriteLine("store done!");
                        if (request_commit && commit.Active)
                        {
                            commit.Request(dicom);
                            Debug.WriteLine("storage commit requested!");
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
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
