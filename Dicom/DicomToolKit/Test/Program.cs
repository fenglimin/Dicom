using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using EK.Capture.Dicom.DicomToolKit;

namespace Test
{
    class Program
    {

        #region Echo

        static void echoservice(bool wait)
        {
            Server server = new Server("ECHO", 1234);

            VerificationServiceSCP service = new VerificationServiceSCP();
            service.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(service);

            server.Start();

            if (wait)
            {
                System.Console.WriteLine("\nPress <enter> to stop server ...");
                System.Console.ReadLine();
            }
            else
            {
                echo("ECHO", IPAddress.Parse("127.0.0.1"), 1234);
            }

            server.Stop();

        }

        static void echomwl(string title, IPAddress address, int port)
        {
            VerificationServiceSCU echo = new VerificationServiceSCU();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            QueryRetrieveServiceSCU mwl = new QueryRetrieveServiceSCU();
            mwl.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mwl);
            association.AddService(echo);

            if (association.Open(title, address, port))
            {

                bool result = echo.Echo();
                System.Console.WriteLine("\necho {0}.", (result) ? "succeded" : "failed");
                if (result)
                {
                    DataSet query = GetQuery(null);
                    RecordCollection records = mwl.CFind(query);
                    WriteRecords(records);
                }
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            association.Close();
        }

        static void echo(string title, IPAddress address, int port)
        {
            VerificationServiceSCU echo = new VerificationServiceSCU();
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(echo);

            if (association.Open(title, address, port))
            {
                bool result = echo.Echo();
                System.Console.WriteLine("\necho {0}.", (result) ? "succeded" : "failed");
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            association.Close();
        }

        #endregion Echo

        #region QueryRetrieve


        static void mwl2(string[] args)
        {
            Dictionary<string, string> arguments = null;
            try
            {
                arguments = ParseCommandLine(args);

                ApplicationEntity scp = new ApplicationEntity(arguments["scp"], IPAddress.Parse(arguments["address"]), Int32.Parse(arguments["port"]));
                ApplicationEntity scu = new ApplicationEntity(arguments["scu"], IPAddress.Parse("127.0.0.1"), 0);

                QueryRetrieveServiceSCU mwl = new QueryRetrieveServiceSCU();
                mwl.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
                mwl.Syntaxes.Add(Syntax.ExplicitVrBigEndian);
                mwl.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                Association association = new Association();
                association.AddService(mwl);

                if (association.Open(scp, scu))
                {
                    DataSet query = GetQuery(arguments);
                    RecordCollection records = mwl.CFind(query);
                    WriteRecords(records);
                }
                else
                {
                    System.Console.WriteLine("\ncan't Open.");
                }
                association.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("");
                Console.WriteLine("usage: Test scp= address= port= [scu=] [tagname=]*");
                Console.WriteLine("e.g. : Test scp=Worklist_SCP address=127.0.0.1 port=6104");
                Console.WriteLine("");
            }
        }

        #endregion QueryRetrieve

        #region Store

        static void OnImageStored(object sender, ImageStorageEventArgs e)
        {
            DataSet dicom = e.DataSet;

            string uid = (string)dicom[t.SOPInstanceUID].Value;

            FileStream ofs = new FileStream(uid + ".dcm", FileMode.Create);
            dicom.Write(ofs);

            ofs.Flush();
            ofs.Dispose();
        }

        static void storeservice(bool wait)
        {
            Server server = new Server("HARVEST", 2000);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP dx = new StorageServiceSCP(StorageClass.DigitalXRayImageStorageForPresentation);
            dx.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP cr = new StorageServiceSCP(StorageClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            ImageStorageEventHandler handler = new ImageStorageEventHandler(OnImageStored);
            dx.ImageStored += handler;
            cr.ImageStored += handler;

            server.AddService(echo);
            server.AddService(dx);
            server.AddService(cr);

            server.Start();

            if (wait)
            {
                System.Console.WriteLine("\nPress <enter> to stop server ...");
                System.Console.ReadLine();
            }
            else
            {
                store("HARVEST", IPAddress.Parse("127.0.0.1"), 2000);
            }

            dx.ImageStored -= handler;
            cr.ImageStored -= handler;

            //System.Console.WriteLine("before Stop!");
            server.Stop();
            //System.Console.WriteLine("after Stop!");

        }

        static void store(string title, IPAddress address, int port)
        {
            StorageServiceSCU dx = new StorageServiceSCU(StorageClass.DigitalXRayImageStorageForPresentation);
            dx.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCU cr = new StorageServiceSCU(StorageClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(dx);
            association.AddService(cr);

            if (association.Open(title, address, port))
            {
                if (dx.Active)
                {
                    DataSet dicom = GetDataSet(@"..\..\test.dcm");

                    dx.Store(dicom);
                    System.Console.WriteLine("store done!");
                }
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            //System.Console.WriteLine("before Close!");
            association.Close();
            //System.Console.WriteLine("after Close!");
        }

        #endregion Store

        #region Print

        static void printservice(bool wait)
        {
            Server server = new Server("PRINTER", 5000);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            PrintServiceSCP grayscale = new PrintServiceSCP(PrintClass.BasicGrayscalePrintManagementMetaSOPClass);
            grayscale.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(grayscale);

            server.Start();

            if (wait)
            {
                System.Console.WriteLine("\nPress <enter> to stop server ...");
                System.Console.ReadLine();
            }
            else
            {
                print("PRINTER", IPAddress.Parse("127.0.0.1"), 5000);
            }

            server.Stop();

        }

        static void print(string title, IPAddress address, int port)
        {
            PrintServiceSCU print = new PrintServiceSCU(PrintClass.BasicGrayscalePrintManagementMetaSOPClass);
            print.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(print);

            if (association.Open(title, address, port))
            {
                if (print.Active)
                {
                }
            }
        }

        #endregion Print

        #region DicomDir


        static void opendicomdir(string folder)
        {
            DicomDir dir = new DicomDir(folder);
            display(dir);
        }


        #endregion

        #region Mpps

        static void OnMpps(object sender, MppsEventArgs e)
        {
            DataSet dicom = e.DataSet;

            string path = String.Format("{0}.{1}.dcm", e.InstanceUid, (e.Command == 0x0140) ? "n-create" : "n-set");
            FileStream ofs = new FileStream(path, FileMode.Create);
            dicom.Write(ofs);

            ofs.Flush();
            ofs.Dispose();
        }

        static void mppsservice(bool wait)
        {
            Server server = new Server("MPPS", 2010);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            MppsServiceSCP mpps = new MppsServiceSCP();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            MppsEventHandler handler = new MppsEventHandler(OnMpps);
            mpps.MppsCreate += handler;
            mpps.MppsSet += handler;

            server.AddService(echo);
            server.AddService(mpps);

            server.Start();

            if (wait)
            {
                System.Console.WriteLine("\nPress <enter> to stop server ...");
                System.Console.ReadLine();
            }
            else
            {
                string uid = Element.GetNewUid();
                begin(uid, "MPPS", IPAddress.Parse("10.95.53.106"), 2010);
                end(uid, "MPPS", IPAddress.Parse("10.95.53.106"), 2010);
            }

            mpps.MppsCreate -= handler;
            mpps.MppsSet -= handler;

            server.Stop();

        }

        static void begin(string uid, string title, IPAddress address, int port)
        {
            MppsServiceSCU mpps = new MppsServiceSCU();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mpps);

            if (association.Open(title, address, port))
            {
                DataSet dicom = GetDataSet(@"..\..\begin.dcm");

                mpps.Begin(uid, dicom);
                System.Console.WriteLine("begin done!");
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            //System.Console.WriteLine("before Close!");
            association.Close();
            //System.Console.WriteLine("after Close!");
        }

        static void end(string uid, string title, IPAddress address, int port)
        {
            MppsServiceSCU mpps = new MppsServiceSCU();
            mpps.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(mpps);

            if (association.Open(title, address, port))
            {
                DataSet dicom = GetDataSet(@"..\..\end.dcm");

                mpps.End(uid, dicom);
                System.Console.WriteLine("end done!");
            }
            else
            {
                System.Console.WriteLine("\ncan't Open.");
            }
            //System.Console.WriteLine("before Close!");
            association.Close();
            //System.Console.WriteLine("after Close!");
        }

        #endregion Mpps

        static void Main(string[] args)
        {
            DateTime start = DateTime.Now;

            try
            {
                //Tag tag = Tag.Parse("(0008,0100)(0008,0010)(0040,1020)");
                //accessors();

                //echo("MWLSERVER", IPAddress.Parse("150.102.124.17"), 2250);
                //echo("Worklist_SCP", IPAddress.Parse("127.0.0.1"), 6104);
                //echo("HARVEST", IPAddress.Parse("127.0.0.1"), 2000);

                // not an accepted presentation context item
                //echomwl("Worklist_SCP", IPAddress.Parse("10.95.53.106"), 6104);

                //mwl("Worklist_SCP", IPAddress.Parse("10.95.53.106"), 6104);
                //mwl("OFFIS", IPAddress.Parse("127.0.0.1"), 109);
                //mwl("MESA_MWL", IPAddress.Parse("10.95.16.94"), 2250);
                //mwl("BROKER", IPAddress.Parse("150.102.124.7"), 3320);
                //mwl("MWL", IPAddress.Parse("10.95.53.106"), 101);
                //mwl("AMICAS", IPAddress.Parse("127.0.0.1"), 2010);

                //mwl2(args);

                //echoservice(true);
                //storeservice(false);
                //mwlservice(false);
                //printservice(false);
                //mppsservice(false);

                //dicomdir();
                //opendicomdir(@"C:\Documents and Settings\L438125\Desktop\dicom files\DICOMDIR Pixelmed");
                //store("HARVEST", IPAddress.Parse("10.112.14.203"), 2000);
                dump();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            TimeSpan duration = DateTime.Now - start;
            Console.WriteLine((float)duration.Ticks / (float)TimeSpan.TicksPerMillisecond / 1000);

            System.Console.WriteLine("\nPress <enter> to continue ...");
            System.Console.ReadLine();

        }

        private static void dump()
        {
            DataSet dicom = GetDataSet();
            System.Console.WriteLine(dicom.Dump());
        }

        private static Dictionary<string, string> ParseCommandLine(string[] args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            Type type = typeof(t);
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (string item in args)
            {
                string[] elements = item.Split("=".ToCharArray());
                if (elements.Length != 2)
                {
                    throw new Exception(String.Format("Invalid argument '{0}'", item));
                }
                switch (elements[0])
                {
                    case "scp":
                    case "address":
                    case "port":
                    case "scu":
                        elements[0] = elements[0].ToLower();
                        arguments[elements[0]] = elements[1];
                        break;
                    default:
                        {
                            bool found = false;
                            string[] tags = elements[0].Split(".".ToCharArray());
                            string key = String.Empty;
                            foreach (string tag in tags)
                            {
                                found = false;
                                foreach (FieldInfo info in fields)
                                {
                                    if (info.Name == tag)
                                    {
                                        found = true;
                                        key += (string)info.GetValue(info);
                                    }
                                }
                                if (!found)
                                {
                                    throw new Exception(String.Format("No such tag '{0}'", elements[0]));
                                }
                            }

                            if (!found)
                            {
                                throw new Exception(String.Format("No such tag '{0}'", elements[0]));
                            }
                            arguments[key] = elements[1];
                        }
                        break;
                }
            }

            if (!arguments.ContainsKey("scu"))
            {
                arguments["scu"] = Dns.GetHostName();
            }
            if (!arguments.ContainsKey("scp") || !arguments.ContainsKey("address") || !arguments.ContainsKey("port"))
            {
                throw new Exception("You must specify scp, address and port as command line arguments.");
            }

            return arguments;
        }

        #region Stuff

        /*
        Dicom dicom = new Dicom();
        dicom.Part10Header = true;

        dicom.syntax = Syntax.ExplicitVrLittleEndian;

        dicom.Add(t.FileMetaInformationVersion, new byte[] { 0, 1 });
        dicom.Add(t.MediaStorageSOPClassUID, "1.2.840.10008.5.1.4.1.1.1.1");
        dicom.Add(t.MediaStorageSOPInstanceUID, "1.2.840.113564.15010295186.20070613");
        dicom.Add(t.TransferSyntaxUID, Syntax.ExplicitVrLittleEndian);
        dicom.Add(t.ImplementationClassUID, "1.2.840.113564.3.4.1");

        Element sequence = new Element(t.ReferencedImageSequence, "SQ");
        Elements item = sequence.NewItem();

        item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.5.1.4.1.1.1.1");
        item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.15010295186.20070613");

        dicom.Add(sequence);
        dicom.Add(t.PatientsName, "Sadler^Michael");

        FileStream ofs = new FileStream("out.dcm", FileMode.Create);
        dicom.Write(ofs);
        ofs.Flush();
        /**/

        /*
        File.Copy(args[0], "in.dcm", true);
        FileStream ifs = new FileStream("in.dcm", FileMode.Open, FileAccess.Read, FileShare.Read);

        Dicom dcm = new Dicom();
        if (dcm.Read(ifs))
        {
            InOrder(dcm);
            FileStream ofs = new FileStream("out.dcm", FileMode.Create);
            dcm.Write(ofs);
            ofs.Flush();
        }
        /**/

        /*
        DirectoryInfo directory = new DirectoryInfo(args[0]);

        FileInfo[] files = directory.GetFiles();

        string name = String.Empty;
        foreach (FileInfo file in files)
        {
            try
            {
                    name = file.Name;
                    FileStream ifs = new FileStream(file.FullName, FileMode.Open);

                    Dicom dcm = new Dicom();
                    if (dcm.Read(ifs))
                    {
                        Console.WriteLine(dcm[t.PatientsName].Value);
                        Dump(dcm);
                        //InOrder(dcm);
                        break;
                    }
            }
            catch (Exception ex)
            {
                Console.WriteLine(name + ":" + ex);
            }
        }
        /**/

        #endregion

        #region Etc.

        public static void multiplicity()
        {
            Hashtable hash = new Hashtable();
            foreach (EK.Capture.Dicom.DicomToolKit.Tag entry in Dictionary.Instance)
            {
                if (!hash.ContainsKey(entry.VM))
                {
                    hash[entry.VM] = entry.VM;
                }
            }
            ArrayList keys = new ArrayList(hash.Keys);
            keys.Sort();
            // then we iterate over the sorted keys
            foreach (string key in keys)
            {
                Console.WriteLine(key);
            }
        }


        public static void multiplicity2()
        {
            Hashtable types = new Hashtable();
            foreach (EK.Capture.Dicom.DicomToolKit.Tag entry in Dictionary.Instance)
            {
                if (!types.ContainsKey(entry.VR))
                {
                    types[entry.VR] = new Hashtable();
                }
                if (!((Hashtable)types[entry.VR]).ContainsKey(entry.VM))
                {
                    ((Hashtable)types[entry.VR])[entry.VM] = entry.VM;
                }
            }
            ArrayList keys = new ArrayList(types.Keys);
            keys.Sort();
            // then we iterate over the sorted keys
            foreach (string key in keys)
            {
                Console.Write(key + ": ");
                Hashtable temp = types[key] as Hashtable;
                foreach (System.Collections.DictionaryEntry entry in temp)
                {
                    Console.Write(entry.Value + ", ");
                }
                Console.WriteLine();
            }
        }

        static void accessors()
        {
            Tag tag = Dictionary.Instance["(0028,0002)"];

            DataSet dicom = GetDataSet();

            ushort number = (ushort)dicom[t.SamplesperPixel].Value;
            dicom[t.SamplesperPixel].Value = 1;
        }
        
        static void SessionProfiles()
        {
            string path = @"C:\Documents and Settings\l438125\Desktop\SessionProfiles\France\Session2\";
            RecordCollection records = new RecordCollection(path, true);

            using (StreamReader reader = new StreamReader(path + "HisRisServiceCache"))
            {
                String line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] strings = line.Split(" ".ToCharArray());
                    if (strings.Length > 1)
                    {
                        records.Add(strings[1]);
                    }
                }
            }
            WriteRecords(records);
        }

        static DataSet GetDataSet()
        {
            return GetDataSet(null);
        }

        static DataSet GetDataSet(string file)
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

                Element sequence;
                Elements item;

                dicom.Add(t.SpecificCharacterSet, "ISO_IR 100");
                dicom.Add(t.ImageType, @"DERIVED\PRIMARY");
                dicom.Add(t.SOPClassUID, StorageClass.DigitalMammographyImageStorageForPresentation);
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
                dicom.Add(t.ReferringPhysiciansName, "Smith^Bob");
                dicom.Add(t.StationName, "sadler");
                dicom.Add(t.StudyDescription, "XR Chest");
                dicom.Add(t.ManufacturersModelName, "CRxxx");

                sequence = new Element(t.ReferencedStudySequence, "SQ");
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.3.1.2.3.1");
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.9.1.2005121220021252.20070709124146.21000704101");
                dicom.Add(sequence);

                sequence = new Element(t.ReferencedPerformedProcedureStepSequence, "SQ");
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.3.1.2.3.3");
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.9.1.2005121220021252.20070709124146.21000704101");
                dicom.Add(sequence);

                sequence = new Element(t.ReferencedImageSequence, "SQ");
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.5.1.4.1.1.1.1");
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.15010295186.2007070913382770320.4003001025002");
                dicom.Add(sequence);

                sequence = new Element(t.SourceImageSequence, "SQ");
                item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.5.1.4.1.1.1.1.1");
                item.Add(t.ReferencedSOPInstanceUID, "1.2.840.113564.15010295186.2007070913382770320.1000000000003");
                dicom.Add(sequence);

                sequence = new Element(t.AnatomicRegionSequence, "SQ");
                item = sequence.NewItem();
                item.Add(t.CodeValue, "T-D3000");
                item.Add(t.CodingSchemeDesignator, "SNM3");
                item.Add(t.CodeMeaning, "Chest");
                item.Add(t.MappingResource, "DCMR");
                item.Add(t.ContextGroupVersion, "20020904");
                item.Add(t.ContextIdentifier, "4031");
                dicom.Add(sequence);

                dicom.Add(t.PatientsName, "Sadler^Michael");
                dicom.Add(t.PatientID, "489720009");
                dicom.Add(t.PatientsBirthDate, "19601226");
                dicom.Add(t.PatientsBirthTime, "000000.000");
                dicom.Add(t.PatientsSex, "O");
                dicom.Add(t.PatientsAge, "046Y");
                dicom.Add(t.ContrastBolusAgent, "");
                dicom.Add(t.BodyPartExamined, "CHEST");
                dicom.Add(t.DeviceSerialNumber, "0000");
                dicom.Add(t.SoftwareVersions, "1.0.10.2");
                dicom.Add(t.ImagerPixelSpacing, @"0.168\0.168");
                dicom.Add(t.RelativeXrayExposure, "2607");
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

        public static DataSet GetQuery(Dictionary<string, string> args)
        {
            DataSet dicom = new DataSet();

            Element sequence;
            Elements item;

            dicom.Add(t.SpecificCharacterSet, null);
            dicom.Add(t.AccessionNumber, null);
            dicom.Add(t.ReferringPhysiciansName, null);
            dicom.Add(t.AdmittingDiagnosesDescription, null);
            sequence = new Element(t.ReferencedStudySequence, "SQ");
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.ReferencedSOPClassUID, null);
            item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(t.PatientsName, "*");
            dicom.Add(t.PatientID, null);
            dicom.Add(t.PatientsBirthDate, null);
            dicom.Add(t.PatientsBirthTime, null);
            dicom.Add(t.PatientsSex, null);
            dicom.Add(t.OtherPatientIDs, null);
            dicom.Add(t.OtherPatientNames, null);
            dicom.Add(t.PatientsAge, null);
            dicom.Add(t.PatientsSize, null);
            dicom.Add(t.PatientsWeight, null);
            dicom.Add(t.MedicalAlerts, null);
            dicom.Add(t.EthnicGroup, null);
            dicom.Add(t.Occupation, null);
            dicom.Add(t.AdditionalPatientHistory, null);
            dicom.Add(t.PregnancyStatus, null);
            dicom.Add(t.PatientComments, null);
            dicom.Add(t.StudyInstanceUID, null);
            dicom.Add(t.RequestingService, null);
            dicom.Add(t.RequestedProcedureDescription, null);
            sequence = new Element(t.RequestedProcedureCodeSequence, "SQ");
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.CodeValue, null);
            item.Add(t.CodingSchemeDesignator, null);
            item.Add(t.CodingSchemeVersion, null);
            item.Add(t.CodeMeaning, null);
            dicom.Add(t.VisitStatusID, null);
            dicom.Add(t.PatientsInstitutionResidence, null);
            sequence = new Element(t.ScheduledProcedureStepSequence, "SQ");
            dicom.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.Modality, "CR");
            item.Add(t.RequestedContrastAgent, null);
            String format = "yyyyMMdd";
            DateTime today = DateTime.Now;
            //String range = String.Format("{0}-{1}", today.AddDays(-7).ToString(format), today.AddDays(1).ToString(format));
            String range = today.ToString(format);
            item.Add(t.ScheduledProcedureStepStartDate, range);
            item.Add(t.ScheduledProcedureStepStartTime, null);
            item.Add(t.ScheduledProcedureStepDescription, null);
            item.Add(t.ScheduledProcedureStepID, null);
            sequence = new Element(t.ScheduledProtocolCodeSequence, "SQ");
            item.Add(sequence);
            item = sequence.NewItem();
            item.Add(t.CodeValue, null);
            item.Add(t.CodingSchemeDesignator, null);
            item.Add(t.CodingSchemeVersion, null);
            item.Add(t.CodeMeaning, null);
            dicom.Add(t.RequestedProcedureID, null);
            dicom.Add(t.RequestedProcedurePriority, null);

            if (args != null)
            {
                foreach (KeyValuePair<string, string> entry in args)
                {
                    if(entry.Key.Contains("("))
                        dicom[entry.Key].Value = entry.Value;
                }
            }

            return dicom;
        }

        #endregion Etc.
    }
}
