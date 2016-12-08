using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
//using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for DataSetTest
    /// </summary>
    [TestClass]
    public class DataSetTest
    {
        public DataSetTest()
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
        public void ImplicitVrLittleEndianTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\Syntax\ImplicitVrLittleEndian.dcm");
            dicom.Read(path);

            Assert.AreEqual(dicom.TransferSyntaxUID, Syntax.ImplicitVrLittleEndian);
        }

        [TestMethod]
        public void AdhocTest()
        {
            DataSet dicom = new DataSet();
            dicom.Part10Header = true;
            dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;

            Element element = dicom.Add("(0029,1011)", "LO", "stinky");

            dicom.Write("output.dcm");

            dicom = new DataSet();
            dicom.Read("output.dcm");

            dicom["(0029,1011)"].Value = "really stinky";

        }

        [TestMethod]
        public void SetTest()
        {
            DataSet dicom = new DataSet();

            dicom.Set("(0008,0005)", null);
            Assert.IsTrue(dicom.Contains("(0008,0005)"));

            dicom.Set("(0040,0100)(0040,0001)", @"AA32\AA33");
            Assert.IsTrue(dicom.Contains("(0040,0100)(0040,0001)"));

            dicom.Set("(0008,2218)(0008,2220)(0008,0100)", null);
            Assert.IsTrue(dicom.Contains("(0008,2218)(0008,2220)(0008,0100)"));

        }


        [TestMethod]
        public void ExplicitVrLittleEndianTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\Syntax\ExplicitVrLittleEndian.dcm");
            dicom.Read(path);

            Assert.AreEqual(dicom.TransferSyntaxUID, Syntax.ExplicitVrLittleEndian);
        }

        [TestMethod]
        public void ExplicitVrBigEndianTest()
        {
            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\Syntax\ExplicitVrBigEndian.dcm");
            dicom.Read(path);

            Assert.AreEqual(dicom.TransferSyntaxUID, Syntax.ExplicitVrBigEndian);
        }

        [TestMethod]
        public void EmptySequenceAsLastTagTest()
        {
            DataSet dicom = new DataSet();

            // tag=(0008,0082) vr=SQ length=0
            byte[] bytes = { 0x08, 0x00, 0x82, 0x00, 0x53, 0x51, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            MemoryStream stream = new MemoryStream(bytes);

            dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;
            dicom.Part10Header = false;

            dicom.Read(stream);

        }

        [TestMethod]
        public void EmptyNestedSequenceTest()
        {
            DataSet dicom = new DataSet();

            byte[] bytes = { 0x40, 0x00, 0x55, 0x05, 0x53, 0x51, 0x00, 0x00, 0x1c, 0x00, 0x00, 0x00,    // tag=(0040,0555) vr=SQ length=28
                               0xfe, 0xff, 0x00, 0xe0, 0xff, 0xff, 0xff, 0xff,                          // item tag=(fffe,e000) length=undefined
                               0x40, 0x00, 0x43, 0xa0, 0x53, 0x51, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,  // tag=(0040,ao43) vr=SQ length=0 
                               0xfe, 0xff, 0x0d, 0xe0, 0x00, 0x00, 0x00, 0x00 };                        // item tag=(fffe,e00d) length=0
            MemoryStream stream = new MemoryStream(bytes);

            dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;
            dicom.Part10Header = false;

            dicom.Write(stream);

            DataSet result = new DataSet();
            stream.Seek(0, SeekOrigin.Begin);
            result.Read(stream);

        }

        [TestMethod]
        public void SequenceTest()
        {
            MemoryStream stream = null;
            DataSet dicom = null;
            long count = 0;

            byte[] bytes1 = {   
                        0x08, 0x00, 0x40, 0x11, 0x20, 0x00, 0x00, 0x00,  // sequence (0008,1140) length=32
                        0xfe, 0xff, 0x00, 0xe0, 0x08, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=8
                        0x08, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,  //          (0008,0005) length=0
                        0xfe, 0xff, 0x00, 0xe0, 0x08, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=8
                        0x08, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,  //          (0008,0005) length=0
                };
            stream = new MemoryStream(bytes1);
            stream.Seek(0, SeekOrigin.Begin);
 
            dicom = new DataSet();
            dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;
            dicom.Part10Header = false;

            count = dicom.Read(stream);

            Assert.IsTrue(((Sequence)dicom[t.ReferencedImageSequence]).Items.Count == 2);
            Assert.IsTrue(count == stream.Length);


            byte[] bytes2 = {   
                        0x08, 0x00, 0x40, 0x11, 0x20, 0x00, 0x00, 0x00,  // sequence (0008,1140) length=32
                        0xfe, 0xff, 0x00, 0xe0, 0x00, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=0
                        0xfe, 0xff, 0x00, 0xe0, 0x08, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=8
                        0x08, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,  //          (0008,0005) length=0
                        0xfe, 0xff, 0x00, 0xe0, 0x00, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=0
                };
            stream = new MemoryStream(bytes2);
            stream.Seek(0, SeekOrigin.Begin);

            dicom = new DataSet();
            dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;
            dicom.Part10Header = false;

            count = dicom.Read(stream);

            Assert.IsTrue(((Sequence)dicom[t.ReferencedImageSequence]).Items.Count == 3);
            Assert.IsTrue(count == stream.Length);


            byte[] bytes3 = {   
                        0x08, 0x00, 0x40, 0x11, 0xff, 0xff, 0xff, 0xff,  // sequence (0008,1140) length=undefined
                        0xfe, 0xff, 0x00, 0xe0, 0x08, 0x00, 0x00, 0x00,  // item     (fffe,e000) length=8
                        0x08, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,  //          (0008,0005) length=0
                        0xfe, 0xff, 0x00, 0xe0, 0xff, 0xff, 0xff, 0xff,  // item     (fffe,e000) length=undefined
                        0x08, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00,  //          (0008,0005) length=0
                        0xfe, 0xff, 0x0d, 0xe0, 0x00, 0x00, 0x00, 0x00,  // (fffe,e00d) length=0
                        0xfe, 0xff, 0x00, 0xe0, 0xff, 0xff, 0xff, 0xff,  // item     (fffe,e000) length=undefined
                        0xfe, 0xff, 0x0d, 0xe0, 0x00, 0x00, 0x00, 0x00,  // (fffe,e00d) length=0
                        0xfe, 0xff, 0xdd, 0xe0, 0x00, 0x00, 0x00, 0x00,  // (fffe,e0dd) length=0
                };
            stream = new MemoryStream(bytes3);
            stream.Seek(0, SeekOrigin.Begin);

            dicom = new DataSet();
            dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;
            dicom.Part10Header = false;

            count = dicom.Read(stream);

            Assert.IsTrue(((Sequence)dicom[t.ReferencedImageSequence]).Items.Count == 3);
            Assert.IsTrue(count == stream.Length);

        }


        [TestMethod]
        public void IndexerTest()
        {
            DataSet dicom = new DataSet();

            Sequence sequence = (Sequence)dicom.Add(new Sequence(t.SourceImageSequence));
            Elements item = sequence.NewItem();
            item.Add(new Element(t.ReferencedSOPInstanceUID, "1.2.840.1"));
            item = sequence.NewItem();
            item.Add(new Element(t.ReferencedSOPInstanceUID, "1.2.840.2"));

            Assert.IsTrue(dicom.Contains("(0008,2112)1(0008,1155)"));
            Assert.AreEqual("1.2.840.2", dicom["(0008,2112)1(0008,1155)"].Value);
        }

        [TestMethod]
        public void Example()
        {
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\DicomDir\THGLUZ5J.dcm");

            // sorry, I called it DataSet
            DataSet dicom = new DataSet();

            // you can read a dicom file or fragment from a stream or provide a path
            dicom.Read(path);

            // a DataSet is a collection of Elements

            // using DataSet's indexer, you get an Element object
            Element element = dicom[t.SpecificCharacterSet];

            // each Element has a Value and Tag property.

            // each element has a value (which is a System.Object)
            object temp = element.Value;
            // if an element has a VM > 1, it is always an array of whatever type
            string[] value = (string[])element.Value;

            // each element has a tag
            Tag tag = element.Tag;

            // Element has a VM property that represents the actual number of values for that element
            uint count = element.VM;
            // The Tag property on Element also has a VM which represents the VM defined by the standard, it is a string, e.g, "1-n".
            temp = element.Tag.VM;

            // Element.ToString returns a string represenstation of the value, Element.Tag.ToString gets you the tag
            Debug.WriteLine(String.Format("{0}:{1}:{2}:{3}:{4}\n", element.Tag.ToString(), element.VR, element.VM, element.Description, element.Value.ToString()));

            // t.ImageType == "(0008,0008)";
            element = dicom["(0008,0008)"];

            // you can always ask if a tag exists
            if (dicom.Contains(t.ImageType))
            {
                Element temp2 = dicom[t.ImageType];
                // you have to know what datatype you are going after
                if (temp2.Tag.VM != "1")
                {
                    string[] values = (string[])element.Value;

                    // you can set VM > 1 by this means
                    element.Value = @"ORIGINAL\PRIMARY\OTHER";
                }
                // otherwise it would be a string
                else
                {
                    string value2 = (string)element.Value;
                }
            }

            // there is a method that tells you if a tag has a non-null value or not.
            if (dicom.ValueExists(t.ImageType))
            {
                dicom[t.ImageType].Value = "ORIGINAL";
            }

            // there is a Set method that can be used to set a Value, even if there is no Element in the DataSet
            // so this
            dicom.Set(t.ImageType, "ORIGINAL");
            // is equivalent to 
            if (dicom.Contains(t.ImageType))
            {
                dicom[t.ImageType].Value = "ORIGINAL";
            }
            else
            {
                dicom.Add(t.ImageType, "ORIGINAL");
            }

            // a sequence is an Element with zero or more Items
            Sequence sequence = dicom[t.SourceImageSequence] as Sequence;
            if (sequence.Items.Count > 0)
            {
                // each Item is a collection of Elements
                Elements item = sequence.Items[0];
                element = item[t.ReferencedSOPInstanceUID];
            }

            // you can get to a tag in a sequence like this, if you know that there is only one item
            // otherwise you have to iterate the Items
            element = dicom[t.SourceImageSequence + t.ReferencedSOPInstanceUID];
            
            // there is also a syntax that can get to specific items as long as you know they exist
            // for instance the ReferencedSOPInstanceUID on the third item in the SourceImageSequence
            if(dicom.Contains("(0008,2112)2(0008,1155)"))
            {
                element = dicom["(0008,2112)2(0008,1155)"];
            }

            // PixelData is a short array.  This is because the VR is OW, and dicom says signed 16 bit
            short [] pixels = (short[])dicom[t.PixelData].Value;
            // strangley, this works too, so you can also get it as a ushort which is what you expect
            ushort[] pixels2 = (ushort[])dicom[t.PixelData].Value;
        }

        [Ignore]
        public void ReadWriteLengthTest()
        {
            File.Delete("ReadWriteLengthTest.tmp");

            DataSet dicom = new DataSet();

            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\Syntax\ExplicitVrLittleEndian.dcm");
            FileInfo info = new FileInfo(path);
            long bytes = info.Length;

            long read = dicom.Read(path);
            Assert.AreEqual(bytes, read);

            long written = dicom.Write("ReadWriteLengthTest.tmp");
            Assert.AreEqual(bytes, written);

            File.Delete("ReadWriteLengthTest.tmp");
        }

        [TestMethod]
        public void ReadUpToTest()
        {
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\DicomDir\THGLUZ5J.dcm");

            DataSet dicom = new DataSet();
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            long bytes = dicom.Read(stream, (ushort)0x7FDF);

            Assert.IsFalse(dicom.Contains(t.PixelData));
        }

        [TestMethod]
        public void PrivateTagTest()
        {
            DataSet @explicit = new DataSet();
            @explicit.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;

            @explicit.Add(t.SpecificCharacterSet, "ISO_IR 100");    // add this tag so that GuessAtSyntax will find a tag
            Element before = @explicit.Add("(0029,1011)", "UT", "<xml></xml>");

            @explicit.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;
            @explicit.Write("output.dcm");

            DataSet @implicit = new DataSet();
            @implicit.Read("output.dcm");

            Element after = @implicit["(0029,1011)"];

            Assert.IsTrue(after.VR == "UN");
        }

        [TestMethod]
        public void DoranTest()
        {
            try
            {
                string inputFilePath = @"C:\Temp\31456.dcm";
                string inputRawPixelsFilePath = @"C:\Temp\DecompressedPixelDataTemp.raw";
                string outputFilePath = @"C:\Temp\output.dcm";

                // Read the data set.
                DataSet dicomDS = new DataSet();
                dicomDS.Read(inputFilePath);
                int height = Convert.ToInt32(dicomDS[t.Rows].ToString());
                int width = Convert.ToInt32(dicomDS[t.Columns].ToString());

                // Read the raw pixels.
                int rawPixelFileSize = width * height * 2;
                byte[] rawPixels = new byte[rawPixelFileSize];

                using (BinaryReader binReader = new BinaryReader(new FileStream(inputRawPixelsFilePath, FileMode.Open, FileAccess.Read)))
                {
                    binReader.Read(rawPixels, 0, rawPixelFileSize);
                }

                // Convert the pixels to ushort, while swapping the bytes.
                ushort tmp = 0;
                int j = 0;
                int numPixels = width * height;
                ushort[] pixelsUshort = new ushort[numPixels];

                for (int i = 0; i < numPixels; i++)
                {
                    pixelsUshort[i] = rawPixels[j + 1];
                    tmp = (ushort)(rawPixels[j] << 8);
                    pixelsUshort[i] |= tmp;
                    j += 2;
                }

                // Change the transfer syntax to explicit VR littl endian.
                dicomDS.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;

                // Delete the existing pixel data from the data set.
                dicomDS.Remove(t.PixelData);

                // Replace the pixels in the data set with the decoded pixels.
                dicomDS.Set(t.PixelData, pixelsUshort);

                // Write out the data set.
                dicomDS.Write(outputFilePath);
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Unable to replace pixel data and change transfer syntax:  {0}", ex.Message);
                Console.WriteLine(errMsg);
            }
        }

        [TestMethod]
        public void AnotherAdHocTest()
        {
            string path = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\\Data\DicomDir\THGLUZ5J.dcm");

            DataSet dicom = new DataSet();
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            long bytes = dicom.Read(stream);

            dicom.Remove(t.PixelData);

            // Copy pixels into the output DICOM dataset
            ushort[] newBuf = new ushort[2048 * 2500];

            dicom.Add(t.PixelData, newBuf);
        }

        [TestMethod]
        public void DecodeTest()
        {
/*            string text = @"
00000000 : 04 00 00 00 02 0A 00 00 - 00 48 01 03 00 00 02 00  .........H......
00000010 : 16 00 00 00 31 2E 32 2E - 38 34 30 2E 31 30 30 30  ....1.2.840.1000
00000020 : 38 2E 35 2E 31 2E 34 2E - 33 31 00 00 00 01 02 00  8.5.1.4.31......
00000030 : 00 00 20 80 00 00 20 01 - 02 00 00 00 00 00 00 00  .. ... .........
00000040 : 00 08 02 00 00 00 00 00 - 00 00 00 09 02 00 00 00  ................
00000050 : 00 FF 00 00 01 BA 01 02 - 08 00 05 00 0A 00 00 00  .ÿ...º..........
*/
            string text = @"
00000050 : 08 00 05 00 0A 00 00 00 -                          ................
00000060 : 49 53 4F 5F 49 52 20 31 - 30 30 08 00 50 00 06 00  ISO_IR 100..P...
00000070 : 00 00 30 30 30 30 34 20 - 10 00 10 00 0E 00 00 00  ..00004 ........
00000080 : 48 61 79 64 6E 5E 46 72 - FC 6E 7A 5E 30 34 10 00  Haydn.Frünz.04..
00000090 : 20 00 02 00 00 00 48 46 - 10 00 30 00 08 00 00 00   .....HF..0.....
000000a0 : 31 37 33 32 30 34 33 31 - 10 00 40 00 02 00 00 00  17320431..@.....
000000b0 : 4D 20 10 00 00 20 06 00 - 00 00 41 42 5A 45 53 53  M ... ....ABZESS
000000c0 : 10 00 10 21 06 00 00 00 - 54 41 4E 54 41 4C 20 00  ...!....TANTAL .
000000d0 : 0D 00 1A 00 00 00 31 2E - 32 2E 32 37 36 2E 30 2E  ......1.2.276.0.
000000e0 : 37 32 33 30 30 31 30 2E - 33 2E 32 2E 31 30 34 00  7230010.3.2.104.
000000f0 : 32 00 32 10 06 00 00 00 - 4D 49 4C 4C 45 52 32 00  2.2.....MILLER2.
00000100 : 60 10 06 00 00 00 45 58 - 41 4D 36 37 32 00 64 10  ......EXAM672.d.
00000110 : 34 00 00 00 FE FF 00 E0 - 2C 00 00 00 08 00 00 01  4...þÿ.à,.......
00000120 : 06 00 00 00 42 52 DC 53 - 54 20 08 00 02 01 06 00  ....BRÜST ......
00000130 : 00 00 39 39 52 50 52 20 - 08 00 04 01 08 00 00 00  ..99RPR ........
00000140 : 42 52 45 41 53 54 20 34 - 40 00 00 01 AA 00 00 00  BREAST 4@...ª...
00000150 : FE FF 00 E0 9E 00 00 00 - 08 00 60 00 02 00 00 00  þÿ.à............
00000160 : 55 53 32 00 70 10 00 00 - 00 00 40 00 01 00 04 00  US2.p.....@.....
00000170 : 00 00 41 41 33 32 40 00 - 02 00 08 00 00 00 32 30  ..AA32@.......20
00000180 : 31 32 30 32 32 38 40 00 - 03 00 06 00 00 00 31 34  120228@.......14
00000190 : 35 37 30 39 40 00 06 00 - 06 00 00 00 4D 45 59 45  5709@.......MEYE
000001a0 : 52 20 40 00 07 00 06 00 - 00 00 45 58 41 4D 39 38  R @.......EXAM98
000001b0 : 40 00 08 00 00 00 00 00 - 40 00 09 00 08 00 00 00  @.......@.......
000001c0 : 53 50 44 37 33 38 34 33 - 40 00 10 00 08 00 00 00  SPD73843@.......
000001d0 : 53 54 4E 33 34 37 32 33 - 40 00 11 00 06 00 00 00  STN34723@.......
000001e0 : 42 30 34 46 35 37 40 00 - 12 00 00 00 00 00 40 00  B04F57@.......@.
000001f0 : 00 04 00 00 00 00 40 00 - 01 10 06 00 00 00 42 52  ......@.......BR
00000200 : DC 53 54 20 40 00 03 10 - 04 00 00 00 4C 4F 57 20  ÜST @.......LOW 
";
            MemoryStream stream = new MemoryStream();

            DataSet dicom = new DataSet();
            dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;
            dicom.Part10Header = true;

            dicom.Write(stream);

            using (StringReader reader = new StringReader(text))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length < 8)
                    {
                        continue;
                    }
                    string temp = line.Substring(11,24) + line.Substring(37,24);
                    string[] bytes = temp.Split(" ".ToCharArray());
                    foreach (string b in bytes)
                    {
                        if (b.Length == 0)
                        {
                            continue;
                        }
                        stream.WriteByte(byte.Parse(b, System.Globalization.NumberStyles.AllowHexSpecifier));
                    }
                }
            }

            stream.Seek(0L, SeekOrigin.Begin);

            //PresentationDataPdu response = new PresentationDataPdu(Syntax.ImplicitVrLittleEndian);
            //response.Read(stream);

            //System.Diagnostics.Debug.WriteLine(response.Dump());

            FileStream file = new FileStream("decoded.dcm", FileMode.Create, FileAccess.Write);
            file.Write(stream.ToArray(), 0, (int)stream.Length);
        }

        public static DataSet GetDataSet(string file)
        {
            DataSet dicom = new DataSet();
            if (file != null)
            {
                dicom.Read(file);
                dicom.Part10Header = false;
            }
            else
            {
                dicom.TransferSyntaxUID = Syntax.ImplicitVrLittleEndian;

                Sequence sequence;
                Elements item;

                dicom.Add(t.SpecificCharacterSet, "ISO_IR 100");
                dicom.Add(t.ImageType, @"DERIVED\PRIMARY");
                dicom.Add(t.SOPClassUID, SOPClass.DigitalXRayImageStorageForPresentation);
                dicom.Add(t.SOPInstanceUID, "1.2.840.113564.15010295186.2007070913382770320.4003001025002");
                dicom.Add(t.StudyDate, "20080718");
                dicom.Add(t.SeriesDate, "20080718");
                dicom.Add(t.AcquisitionDate, "20080718");
                dicom.Add(t.ContentDate, "20070709");
                dicom.Add(t.StudyTime, "125235.718");
                dicom.Add(t.SeriesTime, "133827.859");
                dicom.Add(t.AcquisitionTime, "133911.062");
                dicom.Add(t.ContentTime, "133911.062");
                dicom.Add(t.AccessionNumber, "1000704101");
                dicom.Add(t.Modality, "DX");
                dicom.Add(t.PresentationIntentType, "FOR PRESENTATION");
                dicom.Add(t.Manufacturer, "KODAK");
                dicom.Add(t.ReferringPhysicianName, "Smith^Bob");
                dicom.Add(t.StationName, "sadler");
                dicom.Add(t.StudyDescription, "XR Chest");
                dicom.Add(t.ManufacturerModelName, "CRxxx");

                sequence = new Sequence(t.ReferencedStudySequence);
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, SOPClass.DetachedStudyManagementSOPClass);
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.9.1.2005121220021252.20070709124146.21000704101");
                dicom.Add(sequence);

                sequence = new Sequence(t.ReferencedPerformedProcedureStepSequence);
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.3.1.2.3.3");
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.9.1.2005121220021252.20070709124146.21000704101");
                dicom.Add(sequence);

                sequence = new Sequence(t.ReferencedImageSequence);
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, SOPClass.DigitalXRayImageStorageForPresentation);
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.15010295186.2007070913382770320.4003001025002");
                dicom.Add(sequence);

                sequence = new Sequence(t.SourceImageSequence);
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, SOPClass.DigitalXRayImageStorageForProcessing);
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.15010295186.2007070913382770320.1000000000003");
                dicom.Add(sequence);

                sequence = new Sequence(t.AnatomicRegionSequence);
                item = sequence.NewItem();
                item.Add(t.CodeValue, "T-D3000");
                item.Add(t.CodingSchemeDesignator, "SNM3");
                item.Add(t.CodeMeaning, "Chest");
                item.Add(t.MappingResource, "DCMR");
                item.Add(t.ContextGroupVersion, "20020904");
                item.Add(t.ContextIdentifier, "4031");
                dicom.Add(sequence);

                dicom.Add(t.PatientName, "Sadler^Michael");
                dicom.Add(t.PatientID, "489720009");
                dicom.Add(t.PatientBirthDate, "19601226");
                dicom.Add(t.PatientBirthTime, "000000.000");
                dicom.Add(t.PatientSex, "O");
                dicom.Add(t.PatientAge, "046Y");
                dicom.Add(t.ContrastBolusAgent, "");
                dicom.Add(t.BodyPartExamined, "CHEST");
                dicom.Add(t.DeviceSerialNumber, "0000");
                dicom.Add(t.SoftwareVersions, "1.0.10.2");
                dicom.Add(t.ImagerPixelSpacing, @"0.168\0.168");
                dicom.Add(t.RelativeXRayExposure, "2607");
                dicom.Add(t.PositionerType, "NONE");
                dicom.Add(t.ShutterShape, "POLYGONAL");
                dicom.Add(t.VerticesofthePolygonalShutter, @"2500\1\2500\2048\1\2048\1\1");
                dicom.Add(t.ShutterPresentationValue, 0);
                dicom.Add(t.ViewPosition, "AP");
                dicom.Add(t.DetectorType, "");
                dicom.Add(t.StudyInstanceUID, "1.2.840.113564.9.1.2005121220021252.20070709124146.21000704101");
                dicom.Add(t.SeriesInstanceUID, "1.2.840.113564.15010295186.2007070913382762510");
                dicom.Add(t.StudyID, "");
                dicom.Add(t.SeriesNumber, "1");
                dicom.Add(t.AcquisitionNumber, "1");
                dicom.Add(t.InstanceNumber, "1");
                dicom.Add(t.PatientOrientation, @"L\F");
                dicom.Add(t.ImageLaterality, "U");
                dicom.Add(t.ImagesinAcquisition, "1");
                dicom.Add(t.SamplesperPixel, 1);
                dicom.Add(t.PhotometricInterpretation, "MONOCHROME2");
                dicom.Add(t.Rows, 2500);
                dicom.Add(t.Columns, 2048);
                dicom.Add(t.PixelSpacing, @"0.168\0.168");
                dicom.Add(t.PixelAspectRatio, @"1\1");
                dicom.Add(t.BitsAllocated, 16);
                dicom.Add(t.BitsStored, 12);
                dicom.Add(t.HighBit, 11);
                dicom.Add(t.PixelRepresentation, 0);
                dicom.Add(t.SmallestImagePixelValue, 0);
                dicom.Add(t.LargestImagePixelValue, 4095);
                dicom.Add(t.BurnedInAnnotation, "NO");
                dicom.Add(t.PixelIntensityRelationship, "LOG");
                dicom.Add(t.PixelIntensityRelationshipSign, 1);
                dicom.Add(t.RescaleIntercept, "0");
                dicom.Add(t.RescaleSlope, "1");
                dicom.Add(t.RescaleType, null);
                dicom.Add(t.LossyImageCompression, "00");

                dicom.Add(t.PixelData, new short[2048 * 2500]);
            }
            return dicom;
        }
    }
}
