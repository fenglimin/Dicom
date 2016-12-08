using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for ToolsTest
    /// </summary>
    [TestClass]
    public class ToolsTest
    {
        public ToolsTest()
        {
            //
            // TODO: Add constructor logic here
            //
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
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
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
        public void DicomEditorTest()
        {
            Process editor = new Process();
            editor.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomEditor\bin\Debug\DicomEditor.exe";

            try
            {
                editor.Start();
            }
            catch
            {
                editor.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomEditor\bin\Release\DicomEditor.exe";
                editor.Start();
            }
            Thread.Sleep(1000);

            ApplicationEntity host = new ApplicationEntity("DICOMEDITOR", 2009);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\Y2ASNFDS.dcm", host, false);

            editor.Kill();
        }

        [TestMethod]
        public void DicomViewerTest()
        {
            Process viewer = new Process();
            viewer.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomViewer\bin\Debug\DicomViewer.exe";

            try
            {
                viewer.Start();
            }
            catch
            {
                viewer.StartInfo.FileName = Tools.RootFolder + @"\EK\Capture\Dicom\Tools\DicomViewer\bin\Release\DicomViewer.exe";
                viewer.Start();
            }
            Thread.Sleep(1000);

            ApplicationEntity host = new ApplicationEntity("DICOMVIEWER", 2008);
            StorageTest.store(@"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\Y2ASNFDS.dcm", host, false);

            viewer.Kill();
        }

    }
}
