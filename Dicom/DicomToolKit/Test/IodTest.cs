using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for IodTest
    /// </summary>
    [TestClass]
    public class IodTest
    {
        public IodTest()
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
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }

        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize() 
        {
            Iod.Xml = null;
        }

        //
        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void MyTestCleanup() 
        {
            Iod.Xml = null;
        }

        #endregion

        [TestMethod]
        public void BuildTest()
        {
            DataSet dicom = Iod.Build("MG");

            // maybe build an IOD from a set of source tags ???
            // have yet to figure out how this can be tested, here for debugging
        }

        [TestMethod]
        public void VerifyIodTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm");
            dicom.Read(path);

            Elements missing = new Elements();
            bool success = Iod.Verify(dicom.Elements, "CR", missing);
            Assert.IsTrue(success, "Expected that this would verify.");

            // just check other signature
            success = Iod.Verify(dicom.Elements, "CR");
            Assert.IsTrue(success, "Expected that this would verify.");
        }

        [TestMethod]
        public void ImplicitVerifyIodTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\WNGVU1P1.dcm");
            dicom.Read(path);

            Assert.IsTrue(Iod.Verify(dicom.Elements), "Expected that this would verify.");
        }

        [TestMethod]
        public void Type1ExistsTest()
        {
            // create a DataSet with a certain single non-null tag 
            DataSet dicom = new DataSet();
            dicom.Add(t.SpecificCharacterSet, "ISO_IR 6");

            // specify an IOD that requires a certain tag to be required
            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0008,0005)' vr='CS' vt='1'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            // verify that they match up
            Assert.IsTrue(Iod.Verify(dicom.Elements, "ME"), "Expected that this would verify because the required tags exists.");

        }

        [TestMethod]
        public void Type1DoesNotExistTest()
        {
            DataSet dicom = new DataSet();

            // create a DataSet with one non-null tag 
            dicom.Add(t.PatientName, "Sadler^Michael");

            // specify an IOD that requires another tag to be required
            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0008,0005)' vr='CS' vt='1'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            // verify that they do not match up
            Assert.IsFalse(Iod.Verify(dicom.Elements, "ME"), "Expected that this would not verify because the required tag does not exist.");

        }

        [TestMethod]
        public void Type1CInRangeExistsTest()
        {
            DataSet dicom = new DataSet();

            // create a DataSet with a first non-null tag and a second tag with a value in range
            dicom.Add(t.PatientName, "Sadler^Michael");
            dicom.Add(t.PatientSize, "128");

            // specify an IOD that requires a first tag if a second tag's value falls in a range
            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0010,0010)' vr='PN' vt='1C' dependency='(0010,1020)=6:150'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            // verify that they match up
            Assert.IsTrue(Iod.Verify(dicom.Elements, "ME"), "Expected that this would verify because the dependency value is in range and the tag exists.");
        }

        [TestMethod]
        public void Type1COutOfRangeNotExistTest()
        {
            DataSet dicom = new DataSet();

            // create a DataSet with a tag value out of range
            dicom.Add(t.PatientSize, "250");

            // specify an IOD that requires first tag if a second tag's value falls in a range
            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0010,0010)' vr='PN' vt='1C' dependency='(0010,1020)=6:150'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            Assert.IsTrue(Iod.Verify(dicom.Elements, "ME"), "Expected that this would verify because the dependency value is out of range, so the tag does not have to exist.");
        }

        [TestMethod]
        public void Type1CInRangeNotExistTest()
        {
            DataSet dicom = new DataSet();

            // create a DataSet with a tag value in range
            dicom.Add(t.PatientSize, "128");

            // specify an IOD that requires a tag if a second tag's value falls in a range
            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0010,0010)' vr='PN' vt='1C' dependency='(0010,1020)=6:150'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            Assert.IsFalse(Iod.Verify(dicom.Elements, "ME"), "Expected that this would not verify because the dependency value is in range and the tag does not exist.");
        }

        [TestMethod]
        public void MixOfDependencyTest()
        {
            DataSet dicom = new DataSet();

            dicom.Add(t.SpecificCharacterSet, "ISO_IR 6");
            dicom.Add("(0010,0010)", "Sadler^Michael");
            dicom.Add("(0010,0020)", "123456");
            dicom.Add("(0010,1010)", "49Y");
            dicom.Add("(0010,1020)", "127");
            dicom.Add("(0010,1030)", "260");


            Iod.Xml = @"
                <dicom>
                    <module name='Module'>
                        <element tag='(0010,0010)' vr='PN' vt='1'></element>
                        <element tag='(0010,0020)' vr='LO' vt='1C' dependency='(0008,0005)=ISO_IR 100|ISO_IR 6'></element>
                        <element tag='(0010,1010)' vr='AS' vt='1C' dependency='(0010,0040)=!'></element>
                        <element tag='(0010,1020)' vr='DS' vt='1C' dependency='(0010,1030)=...'></element>
                    </module>
                    <iod name='ME'>
                        <include name='Module'></include>
                    </iod>
                </dicom>
            ";

            Assert.IsTrue(Iod.Verify(dicom.Elements, "ME"), "Expected that this would verify.");

        }

    }
}
