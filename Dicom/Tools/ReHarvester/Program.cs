using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using EK.Capture.Dicom.DicomToolKit;
using EK.Capture.Imaging.Eclipse.Interop;

namespace ReHarvester
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    Console.WriteLine("usage:ReHarvester filename, where filename is the path of a DICOM part 10 file.");
                    return;
                }

                Program program = new Program();
                program.Run(args[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(String.Format("Error: {0}", ex.Message));
            }
        }

        private void Run(string filename)
        {

            // create a stream out of the Dicom file
            FileStream ifs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);

            // parse the dicom file
            DataSet dicom = new DataSet();
            if (dicom.Read(ifs))
            {
                // get the Eclipse tag
                Element tag = dicom["(0029,1018)"];

                string state = tag.Value as String;

                // do nothing if already fixed
                if (!state.Contains("schemas.microsoft.com".ToUpper()))
                {
                    Console.WriteLine("Nothing to do.");
                    return;
                }

                // fix the problem
                state = Transform(state);

                // only continue if we can de-serialize at this point
                Verify(state);

                // re-assign the value to the Eclipse tag
                tag.Value = state;

                // write out the results
                FileInfo info = new FileInfo(filename);
                string output = info.DirectoryName + @"\" + "re." + info.Name;
                FileStream ofs = new FileStream(output, FileMode.Create);

                dicom.Write(ofs);
                ofs.Flush();
                ofs.Dispose();
            }
            else
            {
                throw new Exception("Unable to parse the dicom file.");
            }
            ifs.Dispose();
        }

        private void Verify(string state)
        {
            // create an Eclipse logging object
            EK.Capture.Imaging.Eclipse.Interop.Logging logging = 
                (EK.Capture.Imaging.Eclipse.Interop.Logging)Factory.Create("logging");

            // hook-up to logging notifications
            logging.ErrorMessageEvent += new LoggingEventHandler(OnErrorMessage);
            //logging.InfoMessageEvent += new LoggingEventHandler(OnMessage);
            //logging.DebugMessageEvent += new LoggingEventHandler(OnMessage);

            try
            {
                using (IState eclipse = SerializationHelper.Deserialize(state)) { }
            }
            catch
            {
                throw;
            }
            finally
            {
                logging.ErrorMessageEvent -= new LoggingEventHandler(OnErrorMessage);
                //logging.InfoMessageEvent -= new LoggingEventHandler(OnMessage);
                //logging.DebugMessageEvent -= new LoggingEventHandler(OnMessage);
            }
        }

        private string Transform(string state)
        {
            /*{
                // write out a debug file
                FileStream ofs = new FileStream("before.xml", FileMode.Create);
                BinaryWriter writer = new BinaryWriter(ofs);

                writer.Write(state.ToCharArray());
                ofs.Flush();
                ofs.Dispose();
            }/**/

            string[] replacements = {
            "xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" xmlns:clr=\"http://schemas.microsoft.com/soap/encoding/clr/1.0\" SOAP-ENV:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\"",
            "xmlns:a1=\"http://schemas.microsoft.com/clr/nsassem/EK.Capture.Imaging.Eclipse.Interop/EK.Capture.Imaging.Eclipse.Interop%2C%20Version%3D",
            "%2C%20Culture%3Dneutral%2C%20PublicKeyToken%3Dddf0b8c12962ac51\"",
            "xsi:type",
            "SOAP-ENV:Envelope",
            "SOAP-ENV:Body",
            "a1:State",
            "AppliedBlackSurround",
            "AppliedLUT",
            "DatabaseContentVersion",
            "DatabaseFormatVersion",
            "LibraryVersion",
            "LUTBurnIn",
            "GridFrequency",
            "GridType",
            "ProcessAsSingleExposure",
            "BlackSurroundBurnIn",
            "BlackSurroundEnabled",
            "EuroFlag",
            "Look",
            "ProcessingMode",
            "GridSuppression",
            "ImagingSystemType",
            "PlateOrientation",
            "ExamType",
            "Projection",
            "BodyPart",
            "SkinLineHighPixel",
            "SkinLineLowPixel",
            "SkinLineShift",
            "SkinLineEnlarge_mm",
            "SkinLineBackgroundDelta",
            "SkinLine",
            "ProcessingLevel",
            "EnhancedFrequencyProcessing",
            "NoiseSuppression",
            "GridDetection",
            "BlackBone",
            "ExposureFieldCount",
            "Deltas",
            "href=", 
            "Exposure0", 
            "Exposure1",
            "Exposure2",
            "Exposure3",
            "Exposure4",
            "a1:Params",
            "ExposureIndex",
            "Asymmetry>",
            "AmbientLuminance",
            "ViewboxLuminance",
            "DMax",
            "DMin",
            "HighExposureNoiseAdjust",
            "LowExposureNoiseAdjust",
            "HighDensitySharpness",
            "LowDensitySharpness",
            "HighDensityDetailContrast",
            "LowDensityDetailContrast",
            "HighDensityBreakpoint",
            "LowDensityBreakpoint",
            "MinExposureDelta",
            "NoiseDensityBreakpoint",
            "MaxNoiseRightExpBreakpoint",
            "LeftNoiseExpBreakpoint",
            "GlobalContrast",
            "ReferenceDensity",
            "KernelSize3",
            "KernelSize2",
            "KernelSize1",
            "Brightness",
            "a1:ExposureField",
            "LutApplied",
            "UnbiasedParams",
            "Deltas",
            "FinalParams",
            "top",
            "a1:BladeAttributes",
            "theta",
            "rho",
            "diag",
            "polarity",
            "xi",
            "yi",
            "xf",
            "yf",
            "x0",
            "y0",
            "radius",
            "left",
            "bottom",
            "right",
            "a1:Params",
            "ExposureIndex",
            "Asymmetry",
            "AmbientLuminance",
            "ViewboxLuminance",
            "DMax",
            "DMin",
            "HighExposureNoiseAdjust",
            "LowExposureNoiseAdjust",
            "HighDensitySharpness",
            "LowDensitySharpness",
            "HighDensityDetailContrast",
            "LowDensityDetailContrast",
            "HighDensityBreakpoint",
            "LowDensityBreakpoint",
            "MinExposureDelta",
            "NoiseDensityBreakpoint",
            "MaxNoiseRightExpBreakpoint",
            "LeftNoiseExpBreakpoint",
            "GlobalContrast",
            "ReferenceDensity",
            "KernelSize3",
            "KernelSize2",
            "KernelSize1",
            "Brightness",

            ">true<",
            ">false<",

            "a1:Enums+GRID_SUPPRESSION",
                ">NO_filter<",
                ">nbw1_6<",
                ">nbw2_12<",
                ">nbw3_18<",
                ">nbw4_22<",
                ">nbw4_25<",
                ">nbw4_29<",
                ">nbw4_32<",
                ">nbw4_34<",
                ">nbw4_36<",
                ">nbw5_38<",
                ">nbw5_40<",
                ">nbw6_44<",
                ">nbw6_47<",
                ">nbw6_50<",
                ">nbw6_54<",
                ">nbw6_58<",
                ">lpbw1_64<",
                ">lpf_nq2<",

            "a1:Enums+LUTBurnIn",
                ">BURN_IN_LUT_NEVER<",
                ">BURN_IN_LUT_ALWAYS<",
                ">BURN_IN_LUT_MEP_ONLY<",
                ">BURN_IN_LUT_SEP_ONLY<",

            "a1:Enums+BlackSurroundBurnIn",
                ">BURN_IN_BLACK_SURROUND_NEVER<",
                ">BURN_IN_BLACK_SURROUND_ALWAYS<",
                ">BURN_IN_BLACK_SURROUND_MEP_ONLY<",
                ">BURN_IN_BLACK_SURROUND_SEP_ONLY<",

            "a1:Enums+ImagingSystemType",
                ">Kodak_CR<",
                ">Orex_CR<",
                ">DRC_DR<",
                ">Trixell_DR<",

            "a1:Enums+PlateOrientation",
                ">LONGITUDINAL<",
                ">TRANSVERSAL<",

            "a1:Enums+EclipseStateStatus",
                ">Uninitialized<",
                ">EclipseSuccess<",
                ">EclipseFallback<",
                ">EclipseFail<",
                ">WindowLevelSuccess<",
                ">WindowLevelFail<",

            "a1:Enums+ProcessingLevel",
                ">Baseline<",
                ">Premium<",

            ">CRPrem-Alt-E<",
            ">CRPrem-Fixed1<",
            ">CRPrem-Fixed2<",
            ">CRPrem-Fixed3<",
            ">CRPrem-US-Alt-A<",
            ">CRPrem-US-Alt-B<",
            ">CRPrem-US-Alt-C<",
            ">CRPrem-US-Alt-D<",
            ">CRPrem-US-Alt1<",
            ">CRPrem-US-Alt2<",
            ">CRPrem-US-Alt3<",
            ">CRPrem-US-Alt4<",
            ">CR_Baseline<",
            ">CR_BaselineNoise<",
            ">CR_Premium<",
            ">CR_PremNoise<",
            ">DR_Baseline<",
            ">DR_Premium<",
            ">Medium<",
            ">Sharp<",
            ">Soft<",

            "a1:Enums+ProcessingMode",
                ">ExamIndependent<",
                ">Global<",
                ">ExamDependent<",
                ">Aim<",
                ">WindowLevel<",

            "Window",

            "Params",
            "Status",
            "Level",
            "a1:Enums+",
            "a1:",
            "id=\"ref-",
            "id=",
            "<id",
            "id>",
            "#ref-",

            };

            foreach (string text in replacements)
            {
                state = state.Replace(text.ToUpper(), text);
            }

            //state = state.Replace("1.0.10.3", "1.0.10.0");
            //state = state.Replace("1, 0, 10, 31", "1, 0, 10, 00");

            /*{
                // write out a debug file
                FileStream ofs = new FileStream("after.xml", FileMode.Create);
                BinaryWriter writer = new BinaryWriter(ofs);

                writer.Write(state.ToCharArray());
                ofs.Flush();
                ofs.Dispose();
            }
            /**/

            return state;

            /*
            StringReader input = new StringReader(state);
            XmlReader reader = XmlReader.Create(input);

            StringWriter output = new StringWriter();
            XmlWriter writer = XmlWriter.Create(output);

            XslCompiledTransform xslt = new XslCompiledTransform();
            xslt.Load("tolower.xsl");
            xslt.Transform(reader, writer);

            return writer.ToString();
            */
        }

        private static void OnErrorMessage(System.Object sender, LogEventArgs args)
        {
            System.Console.Out.WriteLine("ERROR:{0}", args.Message);
        }

        private static void OnMessage(System.Object sender, LogEventArgs args)
        {
            System.Console.Out.WriteLine("{0}", args.Message);
        }

    }
}
