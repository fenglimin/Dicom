using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using EK.Capture.Dicom.DicomToolKit;

namespace cfind
{
    // "..\..\..\5x.dcm" -scp MESA_MWL -a 10.95.17.121 -p 2250 -scu SADLER
    class Program
    {
        private static string scp = String.Empty;
        private static string scu = Dns.GetHostName();
        private static IPAddress address = IPAddress.Parse("127.0.0.1");
        private static int port = 104;
        private static string input;

        static void Main(string[] args)
        {
            try
            {
                if (Setup(args))
                {
                    CFindServiceSCU mwl = new CFindServiceSCU(SOPClass.ModalityWorklistInformationModelFIND);
                    mwl.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
                    mwl.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
                    mwl.Syntaxes.Add(Syntax.ExplicitVrBigEndian);

                    Association association = new Association();
                    association.AddService(mwl);

                    if (association.Open(scu, scp, address, port))
                    {
                        DataSet query = GetQuery();

                        RecordCollection records = mwl.CFind(query);

                        DumpRecords(records);
                    }
                    else
                    {
                        throw new Exception(String.Format("Can't connect to {0}:{1}:{2}", scp, address, port));
                    }
                    association.Close();
                }
            }
            catch (Exception ex)
            {
                System.Console.Out.WriteLine(ex.Message);
            }
        }

        private static void Usage()
        {
            StringBuilder text = new StringBuilder();
            text.Append(String.Format(@"

cfind input -scp title [-a address] [-p port] [-scu title]
where:  input is the filename of a DICOM dataset containing the C-FIND-RQ-DATA to send.
        -scp title is the ae-title of the MWL CFIND SCP, required.
        -a address is the optional IPAddress of the host, default is 127.0.0.1.
        -p port is the optional tcp/ip port of the host, default is 104.
        -scu title is the optional ae-title of the MWL CFIND SCU, default is machine name.
         ? prints this usage."));
            System.Console.WriteLine(text.ToString());
        }

        private static bool Setup(string[] args)
        {
            if (args.Length > 1 && args[0] != "?")
            {
                for (int n = 0; n < args.Length; n++)
                {
                    string arg = args[n];
                    switch (arg.ToLower())
                    {
                        case "-scp":
                            if (n < args.Length)
                            {
                                scp = args[++n];
                            }
                            break;
                        case "-scu":
                            if (n < args.Length)
                            {
                                scu = args[++n];
                            }
                            break;
                        case "-a":
                            if (n < args.Length)
                            {
                                address = IPAddress.Parse(args[++n]);
                            }
                            break;
                        case "-p":
                            if (n < args.Length)
                            {
                                port = Int32.Parse(args[++n]);
                            }
                            break;
                        default:
                            input = args[n];
                            break;
                    }
                }
                if (input != null && !File.Exists(input))
                {
                    throw new Exception(String.Format("{0}, file not found.", input));
                }
                if (scp == null || scp == String.Empty)
                {
                    throw new Exception("You must specify an ae-title.");
                }
            }
            else
            {
                Usage();
                return false;
            }
            return true;
        }

        private static DataSet GetQuery()
        {
            DataSet query = null;
            if (input != null && File.Exists(input))
            {
                query = new DataSet();
                query.Read(input);
            }
            else
            {
            }
            return query;
        }

        private static void DumpRecords(RecordCollection records)
        {
            if (records != null)
            {
                foreach (Elements record in records)
                {
                    foreach (Element element in record.InOrder)
                    {
                        System.Console.Out.WriteLine(String.Format("{0}:{1}:{2}", element.Tag.ToString(), element.Description, element.Value));
                    }
                    System.Console.Out.WriteLine("");
                }
                System.Console.Out.WriteLine(String.Format("\n{0} records returned.", (records == null) ? 0 : records.Count));
            }
        }

    }
}
