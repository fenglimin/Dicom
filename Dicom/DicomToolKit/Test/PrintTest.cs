using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EK.Capture.Dicom.DicomToolKit;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    /// <summary>
    /// Summary description for PrintTest
    /// </summary>
    [TestClass]
    public class PrintTest
    {
        public PrintTest()
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
        public void InternalOneUpPortraitTest()
        {
            ApplicationEntity host = new ApplicationEntity("PRINTER", IPAddress.Parse("127.0.0.1"), 5042);
 
            Server server = StartServer(host.Title, host.Port, null);

            print(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm"), host);

            server.Stop();

        }

        [TestMethod]
        public void InternalFourUpPortraitTest()
        {
            ApplicationEntity host = new ApplicationEntity("NER_8700", IPAddress.Parse("127.0.0.1"), 5041);

            Server server = StartServer(host.Title, host.Port, null);

            FilmSession session = CreateFourUpFilmSession();

            print(session, host);

            server.Stop();
        }

        [TestMethod]
        public void InternalPrinterStatusTest()
        {
            ApplicationEntity host = new ApplicationEntity("PRINTER", IPAddress.Parse("10.95.16.219"), 5042);

            Server server = StartServer(host.Title, host.Port, null);

            PrintServiceSCU print = new PrintServiceSCU(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            print.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(print);

            PrinterStatusEventHandler handler = new PrinterStatusEventHandler(OnPrinterStatus);
            print.PrinterStatus += handler;

            if (association.Open(host))
            {
                if (print.Active)
                {
                    PrinterStatusEventArgs status = print.GetPrinterStatus();
                }
            }
            else
            {
                Debug.WriteLine("\ncan't Open.");
            }

            association.Close();

            server.Stop();
        }

        [Ignore]
        public void ExternalPrinterStatusTest()
        {
            ApplicationEntity host = new ApplicationEntity("NER_8900", IPAddress.Parse("10.95.16.219"), 5040);

            PrintServiceSCU print = new PrintServiceSCU(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            print.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(print);

            PrinterStatusEventHandler handler = new PrinterStatusEventHandler(OnPrinterStatus);
            print.PrinterStatus += handler;

            if (association.Open(host))
            {
                if (print.Active)
                {
                    print.GetPrinterStatus();
                }
            }
            else
            {
                Debug.WriteLine("\ncan't Open.");
            }

            association.Close();
        }

        private void OnJobPrinted(Object sender, PrintJobEventArgs args)
        {
            System.IO.FileStream file = null;
            System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
             using (file = new System.IO.FileStream("session.xml", System.IO.FileMode.Create))
            {
                formatter.Serialize(file, args.Session);
            }
        }

        [TestMethod]
        public void SessionTest()
        {
            ApplicationEntity host = new ApplicationEntity("PRINTER", IPAddress.Parse("127.0.0.1"), 5042);

            Server server = StartServer(host.Title, host.Port, OnJobPrinted);

            print(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm"), host);

            server.Stop();
        }


        [Ignore]
        public void ExternalOneUpPortraitTest()
        {
            ApplicationEntity host = new ApplicationEntity("NER_8900", IPAddress.Parse("10.95.16.219"), 5040);

            print(@"C:\Data\SnapshotViews\l438125_Development\ImageProcessing\EK\Capture\Dicom\DicomToolKit\Test\Data\output.dcm", host);
        }

        [TestMethod]
        public void DicomEmptyTest()
        {
            ApplicationEntity host = new ApplicationEntity("DicomEmpty", 2012);

            string store = Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolKit\Test\Data\DicomDir\THGLUZ5J.dcm");

            for (int n = 0; n < 10; n++)
            {
               FilmSession session = CreateFourUpFilmSession();
                print(session, host);

                StorageTest.store(store, host, false);

                string uid = Element.NewUid();
                MppsTest.Begin(uid, host);
                MppsTest.End(uid, host);
            }
        }

        [Ignore]
        public void ExternalFourUpPortraitTest()
        {
            ApplicationEntity host = new ApplicationEntity("DICOMEDITOR", 2008);

            FilmSession session = CreateFourUpFilmSession();

            print(session, host);
        }

        [TestMethod]
        public void SerializationTest()
        {
            System.IO.FileStream file = null;
            System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
            try
            {
                FilmSession session = CreateFourUpFilmSession();
                using (file = new System.IO.FileStream("session.xml", System.IO.FileMode.Create))
                {
                    formatter.Serialize(file, session);
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                    file.Dispose();
                    file = null;
                }
            }

            try
            {
                using (file = new System.IO.FileStream("session.xml", System.IO.FileMode.Open))
                {
                    ApplicationEntity host = new ApplicationEntity("DICOMEDITOR", 2008);
                    
                    FilmSession session = (FilmSession)formatter.Deserialize(file);

                    print(session, host);
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                    file.Dispose();
                    file = null;
                }
            }
        }

        [TestMethod]
        public void DeserializationTest()
        {
            System.IO.FileStream file = null;
            System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();

            try
            {
                string path = "session.xml";
                using (file = new System.IO.FileStream(path, System.IO.FileMode.Open))
                {
                    object temp = formatter.Deserialize(file);
                    FilmSession session = null;
                    if (temp is FilmSession)
                    {
                        session = (FilmSession)temp;
                    }
                    else
                    {
                        session = new FilmSession();
                        session.AddFilmBox(temp as FilmBox);
                    }
                    print(session, new ApplicationEntity("NER_8900", IPAddress.Parse("127.0.0.1"), 2008));
                }
            }
            finally
            {
                if (file != null)
                {
                    file.Close();
                    file.Dispose();
                    file = null;
                }
            }
        }

        [TestMethod]
        public void PrintFromPartsTest()
        {
            try
            {
                DataSet dicom = new DataSet();
                dicom.Read("FilmSession.dcm");
                string instance = Element.NewUid();
                FilmSession session = new FilmSession(instance, dicom);

                dicom = new DataSet();
                dicom.Read("FilmBox.dcm");
                dicom[t.ReferencedFilmSessionSequence + t.ReferencedSOPInstanceUID].Value = instance;
                FilmBox filmbox = session.NewFilmBox(Element.NewUid(), dicom);

                dicom = new DataSet();
                dicom.Read("ImageBox.dcm");
                ImageBox imagebox = filmbox.NewImageBox(Element.NewUid(), dicom);

                print(session, new ApplicationEntity("NER_8900", IPAddress.Parse("127.0.0.1"), 2008));
            }
            finally
            {
            }
        }

        #region Private Methods

        static void print(FilmSession session, ApplicationEntity host)
        {
            PrintServiceSCU print = new PrintServiceSCU(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            print.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            PresentationLUTServiceSCU plut = new PresentationLUTServiceSCU();
            plut.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            AnnotationServiceSCU annotations = new AnnotationServiceSCU();
            annotations.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(print);
            association.AddService(plut);
            association.AddService(annotations);

            PrinterStatusEventHandler handler = new PrinterStatusEventHandler(OnPrinterStatus);
            print.PrinterStatus += handler;

            if (association.Open(host))
            {
                if (print.Active)
                {
                    print.Print(session);
                }
            }
            else
            {
                Debug.WriteLine("\ncan't Open.");
            }

            association.Close();
        }

        public static void OnPrinterStatus(object sender, PrinterStatusEventArgs e)
        {
            DataSet dicom = e.DataSet;
            Debug.WriteLine("Printer Status:\n" + dicom.Dump());
        }

        static void print(string path, ApplicationEntity host)
        {
            DataSet dicom = new DataSet();
            dicom.Read(path);

            FilmSession session = new FilmSession();

            session[t.NumberofCopies].Value = "1";
            session[t.PrintPriority].Value = "MED";
            session[t.MediumType].Value = "BLUE FILM";
            session[t.FilmDestination].Value = "PROCESSOR";

            PresentationLUT plut = session.NewPresentationLUT();
            plut[t.PresentationLUTShape].Value = "IDENTITIY";

            FilmBox filmbox = session.NewFilmBox();

            filmbox[t.ImageDisplayFormat].Value = @"STANDARD\1,1";
            filmbox[t.AnnotationDisplayFormatID].Value = "COMBINED";
            filmbox[t.FilmOrientation].Value = "PORTRAIT";
            filmbox[t.FilmSizeID].Value = "14INX17IN";
            filmbox[t.MagnificationType].Value = "CUBIC";
            filmbox[t.SmoothingType].Value = "5";
            filmbox[t.BorderDensity].Value = "BLACK";
            filmbox[t.MinDensity].Value = "21";
            filmbox[t.MaxDensity].Value = "360";
            filmbox[t.Trim].Value = "NO";
            filmbox[t.Illumination].Value = "2500";
            filmbox[t.ReflectedAmbientLight].Value = "2";

            Annotation annotation = filmbox.NewAnnotation();
            annotation[t.AnnotationPosition].Value = 0;
            annotation[t.TextString].Value = "bottom";

            annotation = filmbox.NewAnnotation();
            annotation[t.AnnotationPosition].Value = 1;
            annotation[t.TextString].Value = "label";

            ImageBox imagebox = filmbox.NewImageBox();

            imagebox[t.MagnificationType].Value = "CUBIC";
            imagebox[t.SmoothingType].Value = "5";
            imagebox[t.ImageBoxPosition].Value = "1";
            imagebox[t.Polarity].Value = "NORMAL";
            imagebox[t.RequestedDecimateCropBehavior].Value = "DECIMATE";

            imagebox[t.BasicGrayscaleImageSequence + t.SamplesperPixel].Value = dicom[t.SamplesperPixel].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PhotometricInterpretation].Value = dicom[t.PhotometricInterpretation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value = dicom[t.Rows].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value = dicom[t.Columns].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelAspectRatio].Value = dicom[t.PixelAspectRatio].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsAllocated].Value = dicom[t.BitsAllocated].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsStored].Value = dicom[t.BitsStored].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.HighBit].Value = dicom[t.HighBit].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelRepresentation].Value = dicom[t.PixelRepresentation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelData].Value = dicom[t.PixelData].Value;

            imagebox.PresentationLUT = plut;

            print(session, host);
        }

        private static Server StartServer(string title, int port, PrintJobEventHandler handler)
        {
            Server server = new Server(title, port);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            PrintServiceSCP grayscale = new PrintServiceSCP(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            grayscale.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            if (handler != null)
            {
                grayscale.JobPrinted += handler;
            }

            PresentationLUTServiceSCP plut = new PresentationLUTServiceSCP();
            plut.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            AnnotationServiceSCP annotation = new AnnotationServiceSCP();
            annotation.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(grayscale);
            server.AddService(plut);
            server.AddService(annotation);

            server.Start();

            return server;
        }

        static FilmSession CreateFourUpFilmSession()
        {
            DataSet dicom = null;
            FilmSession session = new FilmSession();

            session[t.NumberofCopies].Value = "1";
            session[t.PrintPriority].Value = "MED";
            session[t.MediumType].Value = "BLUE FILM";
            session[t.FilmDestination].Value = "BIN_1";
            session[t.FilmSessionLabel].Value = "";

            FilmBox filmbox = session.NewFilmBox();

            filmbox[t.ImageDisplayFormat].Value = @"STANDARD\2,2";
            filmbox[t.FilmOrientation].Value = "PORTRAIT";
            filmbox[t.FilmSizeID].Value = "14INX17IN";
            filmbox[t.BorderDensity].Value = "330";
            filmbox[t.MinDensity].Value = "23";
            filmbox[t.MaxDensity].Value = "331";
            filmbox[t.Trim].Value = "NO";
            filmbox[t.Illumination].Value = "2007";
            filmbox[t.ReflectedAmbientLight].Value = "9";

            dicom = new DataSet();
            dicom.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolkit\Test\Data\DicomDir\THGLUZ5J.dcm"));

            ImageBox imagebox = filmbox.NewImageBox();

            imagebox[t.MagnificationType].Value = "MAG_UNKNOWN";
            imagebox[t.SmoothingType].Value = "0";
            imagebox[t.ImagePosition].Value = "1";
            imagebox[t.Polarity].Value = "NORMAL";
            imagebox[t.RequestedDecimateCropBehavior].Value = "DECIMATE";

            imagebox[t.BasicGrayscaleImageSequence + t.SamplesperPixel].Value = dicom[t.SamplesperPixel].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PhotometricInterpretation].Value = dicom[t.PhotometricInterpretation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value = dicom[t.Rows].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value = dicom[t.Columns].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelAspectRatio].Value = dicom[t.PixelAspectRatio].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsAllocated].Value = dicom[t.BitsAllocated].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsStored].Value = dicom[t.BitsStored].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.HighBit].Value = dicom[t.HighBit].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelRepresentation].Value = dicom[t.PixelRepresentation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelData].Value = dicom[t.PixelData].Value;

            dicom = new DataSet();
            dicom.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolkit\Test\Data\DicomDir\WNGVU1P1.dcm"));

            imagebox = filmbox.NewImageBox();

            Size truesize = new Size(949, 1210);
            Size original = new Size((ushort)dicom[t.Columns].Value, (ushort)dicom[t.Rows].Value);

            imagebox[t.MagnificationType].Value = "MAG_UNKNOWN";
            imagebox[t.SmoothingType].Value = "0";
            imagebox[t.ImagePosition].Value = "2";
            imagebox[t.RequestedImageSize].Value = "159.431997850537";
            imagebox[t.Polarity].Value = "NORMAL";
            imagebox[t.RequestedDecimateCropBehavior].Value = "FAIL";

            imagebox[t.BasicGrayscaleImageSequence + t.SamplesperPixel].Value = dicom[t.SamplesperPixel].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PhotometricInterpretation].Value = dicom[t.PhotometricInterpretation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value = (ushort)truesize.Height;
            imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value = (ushort)truesize.Width;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelAspectRatio].Value = dicom[t.PixelAspectRatio].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsAllocated].Value = dicom[t.BitsAllocated].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsStored].Value = dicom[t.BitsStored].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.HighBit].Value = dicom[t.HighBit].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelRepresentation].Value = dicom[t.PixelRepresentation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelData].Value = CenterCrop((short[])dicom[t.PixelData].Value, new Size((ushort)dicom[t.Columns].Value, (ushort)dicom[t.Rows].Value), truesize);

            dicom = new DataSet();
            dicom.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolkit\Test\Data\DicomDir\Y2ASNFDS.dcm"));

            imagebox = filmbox.NewImageBox();

            imagebox[t.MagnificationType].Value = "MAG_UNKNOWN";
            imagebox[t.SmoothingType].Value = "0";
            imagebox[t.ImagePosition].Value = "3";
            imagebox[t.RequestedImageSize].Value = "159.431997850537";
            imagebox[t.Polarity].Value = "NORMAL";
            imagebox[t.RequestedDecimateCropBehavior].Value = "FAIL";

            imagebox[t.BasicGrayscaleImageSequence + t.SamplesperPixel].Value = dicom[t.SamplesperPixel].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PhotometricInterpretation].Value = dicom[t.PhotometricInterpretation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value = (ushort)truesize.Height;
            imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value = (ushort)truesize.Width;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelAspectRatio].Value = dicom[t.PixelAspectRatio].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsAllocated].Value = dicom[t.BitsAllocated].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsStored].Value = dicom[t.BitsStored].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.HighBit].Value = dicom[t.HighBit].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelRepresentation].Value = dicom[t.PixelRepresentation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelData].Value = CenterCrop((short[])dicom[t.PixelData].Value, original, truesize);

            dicom = new DataSet();
            dicom.Read(Path.Combine(Tools.RootFolder, @"EK\Capture\Dicom\DicomToolkit\Test\Data\DicomDir\THGLUZ5J.dcm"));

            imagebox = filmbox.NewImageBox();

            imagebox[t.MagnificationType].Value = "MAG_UNKNOWN";
            imagebox[t.SmoothingType].Value = "0";
            imagebox[t.ImagePosition].Value = "4";
            imagebox[t.RequestedImageSize].Value = "159.431997850537";
            imagebox[t.Polarity].Value = "NORMAL";
            imagebox[t.RequestedDecimateCropBehavior].Value = "DECIMATE";

            imagebox[t.BasicGrayscaleImageSequence + t.SamplesperPixel].Value = dicom[t.SamplesperPixel].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PhotometricInterpretation].Value = dicom[t.PhotometricInterpretation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value = dicom[t.Columns].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value = dicom[t.Rows].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelAspectRatio].Value = dicom[t.PixelAspectRatio].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsAllocated].Value = dicom[t.BitsAllocated].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.BitsStored].Value = dicom[t.BitsStored].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.HighBit].Value = dicom[t.HighBit].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelRepresentation].Value = dicom[t.PixelRepresentation].Value;
            imagebox[t.BasicGrayscaleImageSequence + t.PixelData].Value = Rotate((short[])dicom[t.PixelData].Value, original);

            return session;
        }

        static short[] CenterCrop(short[] input, Size before, Size after)
        {
            short[] output = new short[after.Width * after.Height];

            int top = (before.Height - after.Height) / 2;
            int bottom = top + after.Height;
            int left = (before.Width - after.Width) / 2;
            int right = left + after.Width;

            int n = 0;
            for (int r = top; r < bottom; r++)
            {
                for (int c = left; c < right; c++)
                {
                    output[n++] = input[r * before.Width + c];
                }
            }

            return output;
        }

        static short[] Rotate(short[] input, Size before)
        {
            short[] output = new short[input.Length];

            for (int r = 0; r < before.Height; r++)
            {
                for (int c = 0; c < before.Width; c++)
                {
                    output[before.Height - r + before.Height * c - 1] = input[r * before.Width + c];
                }
            }
            return output;
        }

        #endregion Private Methods

    }
}
