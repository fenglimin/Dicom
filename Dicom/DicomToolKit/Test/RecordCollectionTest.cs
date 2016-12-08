using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for RecordCollectionTest
    /// </summary>
    [TestClass]
    public class RecordCollectionTest
    {
        public RecordCollectionTest()
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
        public void LoadExistingFilesTest()
        {
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data");
            RecordCollection records = new RecordCollection(path, true);
            records.Load();
            WriteRecords(records);
        }

        public static void WriteRecords(RecordCollection records)
        {
            if (records != null)
            {
                foreach (Elements record in records)
                {
                    foreach (Element element in record.InOrder)
                    {
                        Debug.WriteLine(String.Format("{0}:{1}:{2}", element.Tag.ToString(), element.Description, element.Value));
                    }
                    Debug.WriteLine("");
                }
                Debug.WriteLine(String.Format("\n{0} records returned.", (records == null) ? 0 : records.Count));
            }
        }

    }
}
