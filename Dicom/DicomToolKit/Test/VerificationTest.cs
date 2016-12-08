using System;
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
    /// Summary description for VerificationTest
    /// </summary>
    [TestClass]
    public class VerificationTest
    {
        public VerificationTest()
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
        public void InternalEchoTest()
        {
            // start an SCP
            Server server = new Server("ECHO", 1234);

            VerificationServiceSCP service = new VerificationServiceSCP();
            service.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(service);

            server.Start();

            // create an SCU and echo
            echo("ECHO", IPAddress.Parse("127.0.0.1"), 1234);

            server.Stop();
        }

        static void echo(string title, IPAddress address, int port)
        {
            VerificationServiceSCU echo = new VerificationServiceSCU();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(echo);

            if (association.Open(title, address, port))
            {
                ServiceStatus status = echo.Echo();
                Debug.WriteLine(String.Format("\necho {0}.", status));
            }
            else
            {
                Debug.WriteLine(String.Format("\ncan't Open."));
            }

            echo = null;
            association.Dispose();
            association = null;
        }

    }
}
