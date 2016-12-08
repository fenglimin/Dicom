using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for SpecificCharacterSetTest
    /// </summary>
    [TestClass]
    public class SpecificCharacterSetTest
    {
        public SpecificCharacterSetTest()
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
        public void CheckCharactersTest()
        {
            byte[] bytes = { 0x43, 0xC3, 0xB4, 0x6C, 0x6F, 0x6E, 0x20, 0x64, 0x2E, 0x63, 0x2E, 0x20 };

            Encoding latin1 = Encoding.GetEncoding("iso-8859-1");
            string text = latin1.GetString(bytes);  //wtf
        }

        [TestMethod]
        public void DefaultEncodingTest()
        {
            // from PS 3.5-2009, Page 107
            // H.3.2 Example 2: Value 1 of Attribute Specific Character Set (0008,0005) is ISO 2022 IR 13.

            byte[] bytes = new byte[] { 0xd4, 0xcf, 0xc0, 0xde, 0x5e, 0xc0, 0xdb, 0xb3, 0x3d, 0x1b, 0x24, 0x42, 0x3b, 0x33, 0x45, 0x44,
                                        0x1b, 0x28, 0x4a, 0x5e, 0x1b, 0x24, 0x42, 0x42, 0x40, 0x4f, 0x3a, 0x1b, 0x28, 0x4a, 0x3d, 0x1b,
                                        0x24, 0x42, 0x24, 0x64, 0x24, 0x5e, 0x24, 0x40, 0x1b, 0x28, 0x4a, 0x5e, 0x1b, 0x24, 0x42, 0x24,
                                        0x3f, 0x24, 0x6d, 0x24, 0x26, 0x1b, 0x28, 0x4a };

            DefaultEncoding encoding = new DefaultEncoding();

            string text = "\x00d7d4\x00d7cf\x00d7c0\x00d7de^\x00d7c0\x00d7db\x00d7b3=\x00d71b$B;3ED\x00d71b(J^\x00d71b$BB@O:\x00d71b(J=\x00d71b$B$d$^$@\x00d71b(J^\x00d71b$B$?$m$&\x00d71b(J";

            string decode = encoding.GetString(bytes);
            Assert.AreEqual(decode, text);

            byte[] temp = encoding.GetBytes(text.ToCharArray());

            string second = encoding.GetString(temp);
            Assert.AreEqual(second, text);

        }

        [TestMethod]
        public void JapaneseTest()
        {
            //ISO 2022 IR 6\ISO 2022 IR 87\ISO 2022 IR 100\ISO 2022 IR 159\ISO 2022 IR 13

            byte[] bytes = new byte[] { 0x59, 0x61, 0x6d, 0x61, 0x64, 0x61, 0x5e, 0x54, 0x61, 0x72, 0x6f, 0x75, 0x3d, 0x1b, 0x24, 0x42, 
                                        0x3b, 0x33, 0x45, 0x44, 0x1b, 0x28, 0x42, 0x5e, 0x1b, 0x24, 0x42, 0x42, 0x40, 0x4f, 0x3a, 0x1b,
                                        0x28, 0x42, 0x3d, 0x1b, 0x24, 0x42, 0x24, 0x64, 0x24, 0x5e, 0x24, 0x40, 0x1b, 0x28, 0x42, 0x5e,
                                        0x1b, 0x24, 0x42, 0x24, 0x3f, 0x24, 0x6d, 0x24, 0x26, 0x1b, 0x28, 0x42 };
            string unicode = @"Yamada^Tarou=山田^太郎=やまだ^たろう";

            Encoding encoding = Encoding.GetEncoding("iso-2022-jp");
            string gg = encoding.GetString(bytes);

            Assert.AreEqual(unicode, gg);

            EncodeDecodeTest(@"ISO 2022 IR 87", unicode, bytes);

        }

        [TestMethod]
        public void KoreanTest()
        {
            byte[] bytes = new byte[] { 0x48, 0x6f, 0x6e, 0x67, 0x5e, 0x47, 0x69, 0x6c, 0x64, 0x6f, 0x6e, 0x67, 0x3d, 0x1b, 0x24, 0x29, 
                                        0x43, 0xfb, 0xf3, 0x5e, 0x1b, 0x24, 0x29, 0x43, 0xd1, 0xce, 0xd4, 0xd7, 0x3d, 0x1b, 0x24, 0x29, 
                                        0x43, 0xc8, 0xab, 0x5e, 0x1b, 0x24, 0x29, 0x43, 0xb1, 0xe6, 0xb5, 0xbf };
            string unicode = @"Hong^Gildong=洪^吉洞=홍^길동";

            // first test the default encoding
            DefaultEncoding encoding = new DefaultEncoding();

            string temp = encoding.GetString(bytes);
            Assert.AreEqual(temp, "Hong^Gildong=\x00d71b$)C\x00d7fb\x00d7f3^\x00d71b$)C\x00d7d1\x00d7ce\x00d7d4\x00d7d7=\x00d71b$)C\x00d7c8\x00d7ab^\x00d71b$)C\x00d7b1\x00d7e6\x00d7b5\x00d7bf", "Default decoding failed.");

            EncodeDecodeTest(@"\ISO 2022 IR 149", unicode, bytes);
        }
        

        private void EncodeDecodeTest(string specific, string unicode, byte[] bytes)
        {
            SpecificCharacterSet set = new SpecificCharacterSet(specific.Split(@"\".ToCharArray()));

            string decode = set.GetString(bytes, "PN");
            Assert.AreEqual(decode, unicode, String.Format("Decoding failed for {0}.", specific));

            byte[] encode = set.GetBytes(unicode, "PN");
            Assert.IsTrue(Compare(bytes, encode));

        }

        private bool Compare(byte[] left, byte[] right)
        {
            bool result = true;
            StringBuilder top = new StringBuilder();
            StringBuilder bottom = new StringBuilder();
            StringBuilder errors = new StringBuilder();

            if (left.Length == right.Length)
            {
                for (int n = 0; n < left.Length; n++)
                {
                    top.Append(String.Format("0x{0:x2} ", left[n]));
                    bottom.Append(String.Format("0x{0:x2} ", right[n]));
                    if (left[n] == right[n])
                    {
                        errors.Append("     ");
                    }
                    else
                    {
                        errors.Append("^^^^ ");
                        result = false;
                    }
                }
            }
            else
            {
                return false;
            }

            System.Diagnostics.Debug.WriteLine(top.ToString());
            System.Diagnostics.Debug.WriteLine(bottom.ToString());
            System.Diagnostics.Debug.WriteLine(errors.ToString());

            return result;
        }
    }
}
