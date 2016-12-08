using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for DicomDirTest
    /// </summary>
    [TestClass]
    public class DicomDirTest
    {
        static object sentry = new object();

        public DicomDirTest()
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
        public void AddCreateTest()
        {
            lock (sentry)
            {

                string folder = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir");

                DicomDir dir = Setup(folder, "AddCreateTest");

                string before = Dump(dir, "before");

                // then read it in
                dir = new DicomDir(Path.Combine(folder, "AddCreateTest"));
                string after = Dump(dir, "after");

                // compare dumps to see if we got the same thing
                Assert.AreEqual(before, after, "before does not match after");

                // do not like the lack of random access and inability to get a count and navigate tree

                // the next block tests the results, two patients, one with one study/series/image and 
                // one with a single study/series with two images
                Assert.IsTrue(dir.Patients.Count == 2, "Expecting two Patients");
                foreach (Study study in (Patient)dir.Patients[0])
                {
                    int count = 0;
                    foreach (Series series in study)
                    {
                        count++;
                        foreach(Image image in series)
                        {
                            Assert.IsTrue(image.OffsetNextRecord != 0, "Expecting more than one Image");
                            break;
                        }
                    }
                    Assert.IsTrue(count == 1, "Expecting one Series");
                    Assert.IsTrue(study.OffsetNextRecord == 0, "Expecting one Study");
                    break;
                }

            }
        }

        [TestMethod]
        public void ManualCreateTest()
        {
            lock (sentry)
            {
                string folder = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir");
                string path = Path.Combine(folder, "ManualCreateTest");

                Directory.CreateDirectory(path);

                DicomDir dir = new DicomDir(path);
                dir.Empty();

                DateTime now = DateTime.Now;

                Patient patient = dir.NewPatient("Sadler^Michael", "12345");
                Study study = patient.NewStudy(now, now, Element.NewUid(), "1");
                Series series = study.NewSeries("CR", Element.NewUid());
                Image image = series.NewImage(Path.Combine(folder, "THGLUZ5J.dcm"));

                dir.Save();
                string before = Dump(dir, "before");

                dir = new DicomDir(path);
                string after = Dump(dir, "after");

                Assert.AreEqual(before, after, "before does not match after");
            }
        }

        [TestMethod]
        public void FolderingTest()
        {
            string folder = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir");
            string path = Path.Combine(folder, "FolderingTest");

            DicomDir dir = new DicomDir(path);
            dir.Empty();

            DirectoryInfo working = new DirectoryInfo(path);
            working = Directory.CreateDirectory(path);

            int n = 0;
            // add each test image to the DICOMDIR
            foreach (string file in Directory.GetFiles(folder, "*.dcm"))
            {
                dir.Add(file, @"parent\child");
                n++;
            }

            // write it out
            dir.Save();

            string temp = Path.Combine(path, @"parent\child");
            temp = Path.Combine(temp, String.Format("{0:00000000}", n));
            Assert.IsTrue(new FileInfo(temp).Exists);

        }

        [TestMethod]
        public void ManualExtraTagsTest()
        {
            string folder = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir");
            string path = Path.Combine(folder, "ManualExtraTagsTest");

            DicomDir dir = new DicomDir(path);
            dir.Empty();

            DirectoryInfo working = new DirectoryInfo(path);
            working = Directory.CreateDirectory(path);

            int n = 0;
            // add each test image to the DICOMDIR
            foreach (string file in Directory.GetFiles(folder, "*.dcm"))
            {
                DataSet temp = new DataSet();
                temp.Read(file);

                // when you add an Image, you now get back a reference to the image
                Image image = dir.Add(temp, @"parent\child");

                // and you can apply some logic to add tags

                // either hard code them
                image.Elements.Set(t.InstanceNumber, n);
                // or add them if they exist in the original image
                if (temp.Contains(t.ImagePositionPatient))
                {
                    image.Elements.Add(temp[t.ImagePositionPatient]);
                }
                else
                {
                    image.Elements.Add(t.ImagePositionPatient, "1");
                }
                if (temp.Contains(t.ImageOrientationPatient))
                {
                    image.Elements.Add(temp[t.ImageOrientationPatient]);
                }
                else
                {
                    image.Elements.Add(t.ImageOrientationPatient, "2");
                }

                n++;
            }

            // write it out
            dir.Save();
        }

        [TestMethod]
        public void FirstOffsetTest()
        {
        }

        [TestMethod]
        public void GetSequentialFileName()
        {
            DirectoryInfo working = new DirectoryInfo(".");

            bool existing = false;
            foreach (FileInfo file in working.GetFiles())
            {
                if (file.Extension.ToUpper() == ".DCM")
                {
                    existing = true;
                    break;
                }
            }

            Assert.IsFalse(existing, "Did not expect any DICOM files in working folder.");

            Assert.AreEqual("00000001", DicomDir.GetSequentialFileName("."));

            File.Create(Path.Combine(".", "00000001")).Dispose();
            File.Create(Path.Combine(".", "00000003")).Dispose();
            File.Create(Path.Combine(".", "00000097")).Dispose();
            Assert.AreEqual("00000098", DicomDir.GetSequentialFileName("."));

            File.Delete(Path.Combine(".", "00000097"));
            Assert.AreEqual("00000004", DicomDir.GetSequentialFileName("."));

            File.Delete(Path.Combine(".", "00000003"));
            Assert.AreEqual("00000002", DicomDir.GetSequentialFileName("."));

            File.Delete(Path.Combine(".", "00000001"));
            Assert.AreEqual("00000001", DicomDir.GetSequentialFileName("."));
        }


        public static DicomDir Setup(string folder, string path)
        {
            DicomDir dir = new DicomDir(Path.Combine(folder, path));
            dir.Empty();

            // add each test image to the DICOMDIR
            foreach (string file in Directory.GetFiles(folder, "*.dcm"))
            {
                dir.Add(file);
            }

            // write it out
            dir.Save();

            return dir;
        }

        #region Private Methods

        string Dump(DicomDir dir, string title)
        {
            StringBuilder text = new StringBuilder();

            foreach (Patient patient in dir.Patients)
            {
                text.Append(String.Format("patient next={0} child={1}\n", patient.OffsetNextRecord, patient.OffsetFirstChild));
                foreach (Study study in patient)
                {
                    text.Append(String.Format("\tstudy next={0} child={1}\n", study.OffsetNextRecord, study.OffsetFirstChild));
                    foreach (Series series in study)
                    {
                        text.Append(String.Format("\t\tseries next={0} child={1}\n", series.OffsetNextRecord, series.OffsetFirstChild));
                        foreach (Image image in series)
                        {
                            text.Append(String.Format("\t\t\timage next={0} child={1} {2}\n", image.OffsetNextRecord, image.OffsetFirstChild, image.ReferencedFileID));
                        }
                    }
                }
            }

            string result = text.ToString();
            Debug.WriteLine(title + "\n" + result);

            return result;
        }

        #endregion Private Methods

    }
}
