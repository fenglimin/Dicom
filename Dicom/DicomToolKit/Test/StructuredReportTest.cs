using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for StructuredReportTest
    /// </summary>
    [TestClass]
    public class StructuredReportTest
    {
        public StructuredReportTest()
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


        /*
            Node root = (Node)nf.createContainerNode( cf.createCodedEntry(“209068”,”99PMP”,“Report”), null,“SEPARATE”);
            */

        [Ignore]
        public void ReadTest()
        {
            //StructuredReport report = StructuredReport.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Reports\report12.dcm"));
            StructuredReport report = StructuredReport.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Reports\test.dcm"));
            //StructuredReport report = StructuredReport.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Reports\cad.dcm"));
            DumpReport(report);
        }

        [Ignore]
        public void MammographyCADReportCreationTest()
        {
            // MAMMOGRAPHY CAD DOCUMENT ROOT, TID 4000
            StructuredReport report = new StructuredReport("111036", "DCM", "Mammography CAD Report");
            Language(report);
            ImageLibrary(report);
            Recommendations(report);
            Detections(report);
            Analyses(report);

            DumpReport(report);
            report.DataSet.Write("CreationTest.dcm");
        }

        [Ignore]
        public void FindNodeTest()
        {
            StructuredReport report = StructuredReport.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\Reports\cad.dcm"));

            ReferenceNode reference = report.FindNode("1.3.1.2.4.1") as ReferenceNode;

            Assert.IsTrue(reference is ReferenceNode, "Expected a Reference Node.");
            Assert.IsTrue(reference.RelationshipType == RelationshipType.SelectedFrom && reference.Reference == "1.2.1", "Expecting a SELECTED FROM reference to 1.2.1");
        }

        [Ignore]
        public void NumericNodeTest()
        {
            NumericNode number = new NumericNode(new CodedEntry("122322", "DCM", "Calibration Factor"));

            number.Value = "1.3";
            number.Units = new CodedEntry("Gym2", "UCUM", "Gym2");


        }

        #region Internals

        internal void DumpReport(StructuredReport report)
        {
            System.Diagnostics.Debug.WriteLine(report.ToString());
        }

        void Language(StructuredReport report)
        {
            CodeNode language = new CodeNode(new CodedEntry("121049", "DCM", "Language of Content Item and Descendants"));
            language.Value = new CodedEntry("en", "RFC3066", "English");
            report.Root.Add(language, RelationshipType.HasConceptModifier);

            CodeNode country = new CodeNode(new CodedEntry("121046", "DCM", "Country of Language"));
            country.Value = new CodedEntry("US", "ISO3166_1", "UNITED STATES");
            language.Add(country, RelationshipType.HasConceptModifier);
        }

        void ImageLibrary(StructuredReport report)
        {
            ContainerNode library = new ContainerNode(new CodedEntry("111028", "DCM", "Image Library"), ContinuityOfContent.Separate);
            report.Root.Add(library, RelationshipType.Contains);

            // there would be one of these for each image
            ImageNode image = new ImageNode(null, SOPClass.DigitalMammographyImageStorageForProcessing, 
                "1.2.840.113564.10956419.20110513130130750480.1000000000003");
            image.ImageLaterality = Laterality.Right;
            image.ImageView = ViewForMammography.CC;
            image.PatientOrientationRow = "P";
            image.PatientOrientationColumn = "L";
            image.StudyDate = "20110513";
            image.StudyTime = "130157";
            image.ContentDate = "20110513";
            image.ContentTime = "130156";
            image.HorizontalImagerPixelSpacing = "49";
            image.VerticalImagerPixelSpacing = "49";

            library.Add(image, RelationshipType.Contains);
        }

        void Recommendations(StructuredReport report)
        {
            CodeNode findings = new CodeNode(new CodedEntry("111017", "DCM", "CAD Processing and Findings Summary"));
            report.Root.Add(findings, RelationshipType.Contains);
        }

        void Detections(StructuredReport report)
        {
            CodeNode detections = new CodeNode(new CodedEntry("111064", "DCM", "Summary of Detections"));
                detections.Value = new CodedEntry("111222", "DCM", "Succeeded");
            report.Root.Add(detections, RelationshipType.Contains);
        }

        void Analyses(StructuredReport report)
        {
            CodeNode analyses = new CodeNode(new CodedEntry("111065", "DCM", "Summary of Analyses"));
            analyses.Value = new CodedEntry("111225", "DCM", "Not Attempted");
            report.Root.Add(analyses, RelationshipType.Contains);
        }

        #endregion
    }
}
