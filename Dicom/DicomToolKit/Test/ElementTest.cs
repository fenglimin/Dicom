using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for ElementTest
    /// </summary>
    [TestClass]
    public class ElementTest
    {
        public ElementTest()
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
        public void ReadTest()
        {
            Element element = new Element();

            byte[] bytes = { 0x28, 0x00, 0x10, 0x00, 0x55, 0x53, 0x02, 0x00, 0xC4, 0x09 };
            MemoryStream stream = new MemoryStream(bytes);

            element.Read(stream, Syntax.ExplicitVrLittleEndian, new SpecificCharacterSet());

            Assert.AreEqual(element.Value, (UInt16)2500);
        }

        [TestMethod]
        public void ResolveTest()
        {
            DataSet dicom = new DataSet();

            Sequence sequence = new Sequence(t.ViewCodeSequence);
            dicom.Add(sequence);

            Elements item = sequence.NewItem();
            item.Add(t.CodeValue, "R-10242");
            item.Add(t.CodingSchemeDesignator, "SNM3");
            item.Add(t.CodeMeaning, "cranio-caudal");

            sequence = new Sequence(t.ViewModifierCodeSequence);
            item.Add(sequence);

            Element element = dicom["(0054,0220)(0008,0100)"];
            Assert.AreEqual(element.Value, "R-10242");

            Assert.IsTrue(dicom.Contains(t.ViewCodeSequence));
            Assert.IsTrue(dicom.Contains(t.ViewCodeSequence + t.CodeMeaning));
            Assert.IsTrue(dicom.Contains(t.ViewCodeSequence + t.ViewModifierCodeSequence));

            Assert.IsFalse(dicom.Contains(t.ViewCodeSequence + t.CodingSchemeVersion));
            Assert.IsFalse(dicom.Contains(t.ViewCodeSequence + t.ViewModifierCodeSequence + t.CodeValue));
        }

        [TestMethod]
        public void ParentTest()
        {
            DataSet dicom = new DataSet();
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Mwl\00001.dcm");
            dicom.Read(path);

            foreach (Element element in dicom.InOrder)
            {
                System.Diagnostics.Debug.WriteLine(String.Format("{0}:{1}", (element.Parent != null) ? element.Parent.Tag.Name : "(null)", element.GetPath()));
            }
        }

        [TestMethod]
        public void ShortTextTest()
        {
            Element element = new Element();

            // tag=(2010,0010) vr=ST length=12 value=STANDARD\1,1
            byte[] bytes = { 0x10, 0x20, 0x10, 0x00, 0x53, 0x54, 0x0C, 0x00, 0x53, 0x54, 0x41, 0x4E, 0x44, 0x41, 0x52, 0x44, 0x5C, 0x31, 0x2C, 0x31 };
            MemoryStream stream = new MemoryStream(bytes);

            element.Read(stream, Syntax.ExplicitVrLittleEndian, new SpecificCharacterSet());

            Assert.AreEqual(element.VM, (uint)1, "Expecting VM = 1");
        }

        [TestMethod]
        public void PrivateGroupLengthTest()
        {
            Element element = new Element();

            // tag=(0029,0000) vr=UL length=4 value=3
            byte[] bytes = { 0x29, 0x00, 0x00, 0x00, 0x55, 0x4C, 0x04, 0x00, 0x03, 0x00, 0x00, 0x00 };
            MemoryStream stream = new MemoryStream(bytes);

            // read will throw an exception if private group length tags are not allowed
            element.Read(stream, Syntax.ExplicitVrLittleEndian, new SpecificCharacterSet());

            Assert.AreEqual((uint)element.Value, (uint)3, "Expecting a value of 3");
        }

        [TestMethod]
        public void GetPathTest()
        {
            DataSet dicom = new DataSet();
            string value = "1.1.2.3";

            Sequence sequence = new Sequence(t.ScheduledProcedureStepSequence);
            dicom.Add(sequence);
            Elements item = sequence.NewItem();
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
            item = sequence.NewItem();
            item = sequence.NewItem();
            item.Add(t.CodeValue, null);
            item.Add(t.CodingSchemeDesignator, null);
            Element element = item.Add(t.CodingSchemeVersion, value);
            item.Add(t.CodeMeaning, null);
            dicom.Add(t.RequestedProcedureID, null);
            dicom.Add(t.RequestedProcedurePriority, null);

            string path = element.GetPath();
            Assert.AreEqual(value, dicom[path].Value.ToString());
        }

        [TestMethod]
        public void Resolve2Test()
        {
            DataSet dicom = new DataSet();

            Sequence sequence = new Sequence(t.ViewCodeSequence);
            dicom.Add(sequence);

            Elements item = sequence.NewItem();
            item = sequence.NewItem();

            item.Add(t.CodeValue, "R-10242");
            item.Add(t.CodingSchemeDesignator, "SNM3");
            item.Add(t.CodeMeaning, "cranio-caudal");

            sequence = new Sequence(t.ViewModifierCodeSequence);
            item.Add(sequence);

// the code above generated this dataset
//           (0054,0220)
//              item(0)
//              item(1)
//                  (0008,0100)
//                  (0008,0102)
//                  (0008,0104)
//                  (0054,0222)

            Element element = dicom["(0054,0220)1(0008,0100)"];
            Assert.AreEqual(element.Value, "R-10242");

            Assert.IsTrue(dicom.Contains("(0054,0220)"));
            Assert.IsTrue(dicom.Contains("(0054,0220)1(0054,0222)"));
            Assert.IsTrue(dicom.Contains("(0054,0220)1(0008,0104)"));

            Assert.IsFalse(dicom.Contains("(0054,0220)(0054,0222)"));
            Assert.IsFalse(dicom.Contains("(0054,0220)(0008,0104)"));
            Assert.IsFalse(dicom.Contains("(0054,0220)0(0008,0104)"));
            Assert.IsFalse(dicom.Contains("(0054,0220)2(0008,0103)"));
            Assert.IsFalse(dicom.Contains("(0054,0220)(0054,0222)3(0008,0100)"));
        }

        [TestMethod]
        public void CaseInsensitiveTest()
        {
            DataSet dicom = new DataSet();

            Element element = dicom.Add("(300A,02bA)", null);

            Assert.IsTrue(dicom.Contains("(300a,02ba)"));
            Assert.AreSame(element, dicom["(300a,02ba)"]);
        }

        [Ignore]
        public void ValueMultiplicityTest()
        {
            DataSet output = new DataSet();
            Element @out = output.Add(t.PixelSpacing, "0.168");

            Assert.IsTrue(@out.Value is Array && @out.VM == 1);

            MemoryStream stream = new MemoryStream();
            output.Write(stream);

            stream.Seek(0, SeekOrigin.Begin);

            DataSet input = new DataSet();
            input.Read(stream);

            Element @in = input[t.PixelSpacing];

            Assert.IsTrue(@in.Value is Array && @in.VM == 1);


        }

        [TestMethod]
        public void ToXmlTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm");
            dicom.Read(path);

            XmlDocument document = new XmlDocument();
            document.LoadXml(dicom.Elements.ToXml());

            Assert.IsTrue(document.HasChildNodes);

            XmlNode element = document.DocumentElement;
            
            XmlNode node = element.SelectSingleNode("PatientID");
            Assert.AreEqual("759275", node.InnerText);

            node = element.SelectSingleNode("AnatomicRegionSequence/Item/CodeValue");
            Assert.AreEqual("T-D3000", node.InnerText);
        }

        [TestMethod]
        public void MultipleVRReadTest()
        {
            Element element = new Element();

            // tag=(7FE0,0010) vr=OW reserved=0 length=6 value= 0x0001,0x0002,0x0003
            byte[] words = { 0xE0, 0x7F, 0x10, 0x00, 0x4F, 0x57, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x01, 0x00, 0x02, 0x00, 0x03, 0x00 };
            MemoryStream stream = new MemoryStream(words);

            element.Read(stream, Syntax.ExplicitVrLittleEndian, new SpecificCharacterSet());

            Assert.AreEqual("OW", element.VR);
            Assert.AreEqual(@"1\2\3", element.ToString());

            // tag=(7FE0,0010) vr=OB reserved=0 length=6 value= 0x01,0x02,0x03
            byte[] bytes = { 0xE0, 0x7F, 0x10, 0x00, 0x4F, 0x42, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x01, 0x02, 0x03 };
            stream = new MemoryStream(bytes);

            element.Read(stream, Syntax.ExplicitVrLittleEndian, new SpecificCharacterSet());

            Assert.AreEqual("OB", element.VR);
            Assert.AreEqual(@"1\2\3", element.ToString());
        }

        [TestMethod]
        public void MultipleVRAssignTest()
        {
            Element element = new Element(0x7FE0, 0x0010);

            short[] words = { 0x0001, 0x0002, 0x0003 };
            element.Value = words;

            Assert.AreEqual("OW", element.VR);
            Assert.AreEqual(@"1\2\3", element.ToString());

            byte[] bytes = { 0x01, 0x02, 0x03 };
            element.Value = bytes;

            Assert.AreEqual("OB", element.VR);
            Assert.AreEqual(@"1\2\3", element.ToString());
        }

        [TestMethod]
        public void MultipleValueStringTest()
        {
            Element element = new Element(0x0028, 0x3002, "US");
            element.Value = @"4096\0\12";
            Assert.AreEqual(element.VM, 3u);
        }
    }
}
