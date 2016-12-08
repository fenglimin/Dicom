using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEmpty
{
    class Program
    {
        static void Main(string[] args)
        {
            ApplicationEntity host = Initialize(args);

            Server server = StartServer(host);

            System.Console.Write("Press <Enter> to Close ");
            System.Console.ReadLine();

            server.Stop();

        }

        private static ApplicationEntity Initialize(string[] args)
        {
            ApplicationEntity host = new ApplicationEntity("DicomEmpty", 2012);
            if (args.Length >= 1)
            {
                host.Port = Int32.Parse(args[0]);
            }
            Console.WriteLine(host.ToString());
            return host;
        }

        private static Server StartServer(ApplicationEntity host)
        {
            Server server = new Server(host.Title, host.Port);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            PrintServiceSCP grayscale = new PrintServiceSCP(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            grayscale.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            grayscale.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            PresentationLUTServiceSCP plut = new PresentationLUTServiceSCP();
            plut.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            plut.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            AnnotationServiceSCP annotation = new AnnotationServiceSCP();
            annotation.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            annotation.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(grayscale);
            server.AddService(plut);
            server.AddService(annotation);

            PrintJobEventHandler print_handler = new PrintJobEventHandler(OnPagePrinted);
            grayscale.JobPrinted += print_handler;

            StorageServiceSCP cr = new StorageServiceSCP(SOPClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            cr.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP dx1 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForPresentation);
            dx1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            dx1.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP dx2 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForProcessing);
            dx2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            dx2.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP mg1 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForPresentation);
            mg1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            mg1.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP mg2 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForProcessing);
            mg2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            mg2.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP gsps = new StorageServiceSCP(SOPClass.GrayscaleSoftcopyPresentationStateStorageSOPClass);
            gsps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            gsps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP sc = new StorageServiceSCP(SOPClass.SecondaryCaptureImageStorage);
            sc.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            sc.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            StorageServiceSCP dose = new StorageServiceSCP(SOPClass.XRayRadiationDoseSRStorage);
            dose.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
            dose.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            ImageStoredEventHandler store_handler = new ImageStoredEventHandler(OnImageStored);
            cr.ImageStored += store_handler;
            dx1.ImageStored += store_handler;
            dx2.ImageStored += store_handler;
            mg1.ImageStored += store_handler;
            mg2.ImageStored += store_handler;
            gsps.ImageStored += store_handler;
            sc.ImageStored += store_handler;
            dose.ImageStored += store_handler;

            server.AddService(cr);
            server.AddService(dx1);
            server.AddService(dx2);
            server.AddService(mg1);
            server.AddService(mg2);
            server.AddService(gsps);
            server.AddService(sc);
            server.AddService(dose);

            MppsServiceSCP mpps = new MppsServiceSCP();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            MppsEventHandler mpps_handler = new MppsEventHandler(OnMpps);
            mpps.MppsCreate += mpps_handler;
            mpps.MppsSet += mpps_handler;

            server.AddService(mpps);

            server.Start();

            return server;
        }

        private static void OnPagePrinted(object sender, PrintJobEventArgs e)
        {
            System.Console.Write("p");
        }

        private static void OnImageStored(object sender, ImageStoredEventArgs e)
        {
            System.Console.Write("s");
        }

        private static void OnMpps(object sender, MppsEventArgs e)
        {
            System.Console.Write("m");
        }

    }
}
