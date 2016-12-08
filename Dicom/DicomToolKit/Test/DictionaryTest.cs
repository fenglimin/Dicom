using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for DictionaryTest
    /// </summary>
    [TestClass]
    public class DictionaryTest
    {
        public DictionaryTest()
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
        public void ListVMTest()
        {
            SortedSet<string> choices = new SortedSet<string>();
            foreach (Tag tag in Dictionary.Instance)
            {
                if (!choices.Contains(tag.VM))
                {
                    choices.Add(tag.VM);
                }
            }
            foreach (string vm in choices)
            {
                System.Diagnostics.Debug.Write(vm + ", ");
            }
            System.Diagnostics.Debug.WriteLine("");
            // 1, 1-2, 1-3, 1-32, 16, 1-8, 1-99, 1-n, 2, 2-2n, 2-n, 3, 3-3n, 3-n, 4, 6,
        }

        [TestMethod]
        public void ListVRTest()
        {
            SortedSet<string> choices = new SortedSet<string>();
            foreach (Tag tag in Dictionary.Instance)
            {
                if (!choices.Contains(tag.VR))
                {
                    choices.Add(tag.VR);
                }
            }
            foreach (string vr in choices)
            {
                System.Diagnostics.Debug.Write(vr + " ");
            }
            System.Diagnostics.Debug.WriteLine("");
            // AE AS AT CS DA DS DT FD FL IS LO LT OB OB,OW OF OW OW,OB PN SH SL SQ SS ST TM UI UL UN US US,SS US,SS,OW UT
        }

        [TestMethod]
        public void VRVMTest()
        {
            Dictionary<string, List<string>> vrs = new Dictionary<string, List<string>>();
            foreach (Tag tag in Dictionary.Instance)
            {
                if (!vrs.ContainsKey(tag.VR))
                {
                    List<string> list = new List<string>();
                    list.Add(tag.VM);
                    vrs.Add(tag.VR, list);
                }
                else
                {
                    if(!vrs[tag.VR].Contains(tag.VM))
                        vrs[tag.VR].Add(tag.VM);
                }
            }
            foreach (KeyValuePair<string, List<string>> kvp in vrs)
            {
                System.Diagnostics.Debug.Write(kvp.Key+" ");
                foreach (string vm in kvp.Value)
                {
                    System.Diagnostics.Debug.Write("," + vm);
                }
                System.Diagnostics.Debug.WriteLine("");
            }
        }

        [TestMethod]
        public void DumpTags()
        {
            StreamWriter tags = new StreamWriter("tag.txt");
            foreach (Tag tag in Dictionary.Instance)
            {
                string name = String.Empty;
                string[] words = tag.Description.Split(" ".ToCharArray());
                foreach(string word in words)
                {
                    name += Dictionary.ModifyWordForEnumeration(word);
                }
                string temp = String.Format("        public const string {0} = \"{1}\";", name, tag.ToString().ToUpper());
                tags.WriteLine(temp);
            }
            tags.Close();
            tags.Dispose();
        }

    }
}
