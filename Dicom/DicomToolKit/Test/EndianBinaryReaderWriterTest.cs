using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for EndianBinaryReaderWriterTest
    /// </summary>
    [TestClass]
    public class EndianBinaryReaderWriterTest
    {
        public EndianBinaryReaderWriterTest()
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
        public void ReadWordsTest()
        {
            unchecked
            {
                byte[] bytes = { 0x01, 0xfe, 0xff, 0x04 };
                MemoryStream stream = new MemoryStream();
                stream.Write(bytes, 0, bytes.Length);

                stream.Seek(0, SeekOrigin.Begin);
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Little);
                short[] little = reader.ReadWords(bytes.Length / 2);

                Assert.IsTrue(little[0] == (short)0xfe01 && little[1] == 0x04ff);


                stream.Seek(0, SeekOrigin.Begin);
                reader = new EndianBinaryReader(stream, Endian.Big);
                short[] big = reader.ReadWords(bytes.Length / 2);

                Assert.IsTrue(big[0] == 0x01fe && big[1] == (short)0xff04);
            }
        }

        [TestMethod]
        public void WriteWordsTest()
        {
            unchecked
            {
                short[] words = { 0x01fe, (short)0xff04 };
                MemoryStream stream = new MemoryStream();

                EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Little);
                writer.WriteWords(words);
                stream.Seek(0, SeekOrigin.Begin);
                byte[] bytes = stream.ToArray();
                Assert.IsTrue(bytes[0] == 0xfe && bytes[1] == 0x01 && bytes[2] == 0x04 && bytes[3] == 0xff);

                writer = new EndianBinaryWriter(stream, Endian.Big);
                writer.WriteWords(words);
                stream.Seek(0, SeekOrigin.Begin);
                bytes = stream.ToArray();
                Assert.IsTrue(bytes[0] == 0x01 && bytes[1] == 0xfe && bytes[2] == 0xff && bytes[3] == 0x04);

            }
        }
    }
}
