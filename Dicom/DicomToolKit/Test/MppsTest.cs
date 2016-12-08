using System;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for MppsTest
    /// </summary>
    [TestClass]
    public class MppsTest
    {
        public MppsTest()
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
        public void TestMethod1()
        {
        }

        static void OnMpps(object sender, MppsEventArgs e)
        {
            DataSet dicom = e.DataSet;

            string path = String.Format("{0}.{1}.dcm", e.InstanceUid, (e.Command == 0x0140) ? "n-create" : "n-set");
            dicom.Write(path);
        }

        static void mppsservice(bool wait)
        {
            Server server = new Server("MPPS", 2010);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            MppsServiceSCP mpps = new MppsServiceSCP();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            MppsEventHandler handler = new MppsEventHandler(OnMpps);
            mpps.MppsCreate += handler;
            mpps.MppsSet += handler;

            server.AddService(echo);
            server.AddService(mpps);

            server.Start();

            if (wait)
            {
                System.Console.WriteLine("\nPress <enter> to stop server ...");
                System.Console.ReadLine();
            }
            else
            {
                string uid = Element.NewUid();
                ApplicationEntity host = new ApplicationEntity("MPPS", IPAddress.Parse("10.95.53.106"), 2010);
                Begin(uid, host);
                End(uid, host);
            }

            mpps.MppsCreate -= handler;
            mpps.MppsSet -= handler;

            server.Stop();

        }

        public static void Begin(string uid, ApplicationEntity host)
        {
            MppsServiceSCU mpps = new MppsServiceSCU();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mpps);

            if (association.Open(host))
            {
                DataSet dicom = DataSetTest.GetDataSet(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Mpps\begin.dcm"));

                mpps.Begin(uid, dicom);
                //System.Console.WriteLine("begin done!");
            }
            else
            {
                //System.Console.WriteLine("\ncan't Open.");
            }
            //System.Console.WriteLine("before Close!");
            association.Close();
            //System.Console.WriteLine("after Close!");
        }

        public static void End(string uid, ApplicationEntity host)
        {
            MppsServiceSCU mpps = new MppsServiceSCU();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mpps);

            if (association.Open(host))
            {
                DataSet dicom = DataSetTest.GetDataSet(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Mpps\end.dcm"));

                mpps.End(uid, dicom);
                //System.Console.WriteLine("end done!");
            }
            else
            {
                //System.Console.WriteLine("\ncan't Open.");
            }
            //System.Console.WriteLine("before Close!");
            association.Close();
            //System.Console.WriteLine("after Close!");
        }

    }
}
