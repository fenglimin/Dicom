using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using EK.Capture.Dicom.DicomToolKit;

namespace DicomPipe
{
    // an object used to pass arguments to a background thread
    internal class Connection
    {
        private string name = null;
        private Socket read = null;
        private Socket write = null;
        private object sentry = new object();
        private bool log = false;

        public Connection(string name, Socket read, Socket write, bool log)
        {
            this.name = name;
            this.read = read;
            this.write = write;
            this.log = log;
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public Socket ReadSocket
        {
            get
            {
                return read;
            }
        }

        public Socket WriteSocket
        {
            get
            {
                return write;
            }
        }

        public bool LogEnabled
        {
            get
            {
                return log;
            }
        }

        public void Close()
        {
            lock (sentry)
            {
                Close(read);
                Close(write);
            }
        }

        private void Close(Socket socket)
        {
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
        }
    }

    public class Pipe
    {
        private Socket client;
        private Socket server;
        private ApplicationEntity scp;
        private Thread scuthread;
        private Thread scpthread;
        private System.Threading.AutoResetEvent threadEvent = new AutoResetEvent(false);
        private int timeout = 10000;
        private int MaxPduSize = 262144;
        private Assembly assembly = null;
        private Dictionary<byte, string> syntaxes = new Dictionary<byte, string>();
        private bool log = false;

        public Pipe(Socket client, ApplicationEntity scp, ApplicationEntity scu, Assembly assembly, bool log)
        {
            this.client = client;
            this.scp = scp;
            this.assembly = assembly;
            this.log = log;

            try
            {
                // connect to the SCP
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server.Blocking = true;
                server.Connect(new IPEndPoint(scp.Address, scp.Port));

                // get the SCP thread up and running
                scpthread = new Thread(PipeThread);
                scpthread.Name = "Pipe.Scp.PipeThread";
                scpthread.Start(new Connection(scp.Title, server, client, log));

                if (!threadEvent.WaitOne(timeout, false))
                {
                    string message = "Pipe.Scp.Thread not started.";
                    Logging.Log(message);
                    throw new Exception(message);
                }

                threadEvent.Reset();

                // get the SCU thread up and running
                scuthread = new Thread(PipeThread);
                scuthread.Name = "Pipe.Scu.PipeThread";
                scuthread.Start(new Connection(scu.Title, client, server, log));

                if (!threadEvent.WaitOne(timeout, false))
                {
                    string message = "Pipe.Scu.Thread not started.";
                    Logging.Log(message);
                    throw new Exception(message);
                }
            }
            catch
            {
                client = server = null;
                throw;
            }
        }

        public bool IsOpen()
        {
            return client != null && server != null;
        }

        private void PipeThread(object arg)
        {
            threadEvent.Set();

            Connection connection = arg as Connection;
            byte[] buffer = new byte[MaxPduSize];
            int position = 0;

            MethodInfo OnPdu = null;

            if (assembly != null)
            {
                //Use reflection to call the static Main function
                Module[] mods = assembly.GetModules(false);
                Type[] types = mods[0].GetTypes();

                Type type = mods[0].GetType("Script");
                
                // get a hold of Script
                OnPdu = type.GetMethod("OnPdu");
            }

            try
            {
                NetworkStream input = new NetworkStream(connection.ReadSocket, FileAccess.Read, false);
                NetworkStream output = new NetworkStream(connection.WriteSocket, FileAccess.Write, false);

                while (true)
                {
                    byte[] bytes = Receive(input, buffer, ref position);
                    if (bytes == null)
                    {
                        break;
                    }

                    // if we have some data and a valid script, execute the script
                    MemoryStream memory = new MemoryStream(bytes, 0, bytes.Length);
                    ProtocolDataUnit pdu = null;
                    switch ((ProtocolDataUnit.Type)bytes[0])
                    {
                        case ProtocolDataUnit.Type.A_ASSOCIATE_RQ:
                            pdu = new AssociateRequestPdu();
                            pdu.Read(memory);
                            break;
                        case ProtocolDataUnit.Type.A_ASSOCIATE_AC:
                            pdu = new AssociateRequestPdu();
                            pdu.Read(memory);
                            // we will need to know the transfer syntaxes of presentation data
                            ExtractSyntaxes((AssociateRequestPdu)pdu);
                            break;
                        case ProtocolDataUnit.Type.A_ASSOCIATE_RJ:
                            pdu = new AssociateRejectPdu();
                            pdu.Read(memory);
                            break;
                        case ProtocolDataUnit.Type.P_DATA_TF:
                            // find the presentation's transfer syntax as negotiated earlier in the association
                            string syntax = syntaxes[bytes[10]];
                            pdu = new PresentationDataPdu(syntax);
                            pdu.Read(memory);
                            break;
                        case ProtocolDataUnit.Type.A_RELEASE_RQ:
                            pdu = new AssociationReleasePdu((ProtocolDataUnit.Type)bytes[0]);
                            pdu.Read(memory);
                            break;
                        case ProtocolDataUnit.Type.A_RELEASE_RP:
                            pdu = new AssociationReleasePdu();
                            break;
                        case ProtocolDataUnit.Type.A_ABORT:
                            pdu = new AssociateAbortPdu();
                            pdu.Read(memory);
                            break;
                        default:
                            Logging.Log(LogLevel.Error, "Unsupported ProtocolDataUnit.Type={0}", bytes[0]);
                            break;
                    }
                    if (connection.LogEnabled)
                    {
                        Logging.Log(LogLevel.Verbose, pdu.Name);
                        string text = String.Format("{0} bytes.", pdu.Length);
                        if (pdu.Length < 8192)
                        {
                            text = pdu.ToText();
                        }
                        Logging.Log(LogLevel.Verbose, text);
                    }
                    if (assembly != null && pdu != null && OnPdu != null)
                    {
                        // if the script returns true, assume the pdu was changed
                        if (RunScript(OnPdu, pdu))
                        {
                            // so we re-write the contents into the byte array
                            MemoryStream stream = new MemoryStream();
                            pdu.Write(stream);
                            bytes = stream.ToArray();
                        }
                    }

                    // we pass the bytes, altered or not, on to the other side
                    output.Write(bytes, 0, bytes.Length);
                }
            }
            finally
            {
                threadEvent.Reset();
                connection.Close();
            }
        }

        // record the transfer syntax for all accepted presentation context items
        private void ExtractSyntaxes(AssociateRequestPdu pdu)
        {
            foreach (Item item in pdu.fields)
            {
                if (item is PresentationContextItem)
                {
                    PresentationContextItem pci = item as PresentationContextItem;
                    if (pci.PciReason == PCIReason.Accepted)
                    {
                        syntaxes.Add(pci.PresentationContextId, ((SyntaxItem)pci.fields[0]).Syntax);
                    }
                }
            }
        }

        // run a method and return the result
        private bool RunScript(MethodInfo method, ProtocolDataUnit pdu)
        {
            object s = method.Invoke(null, new object[] { pdu });
            return (bool)s;
        }

        // read from the socket and return complete PDUs
        byte[] Receive(NetworkStream input, byte[] buffer, ref int position)
        {
            //Logging.Log(LogLevel.Verbose, "entering Association.Receive");

            byte[] result = null;
            try
            {
                int length = 0;
                if (position >= 6)
                {
                    //Logging.Log(LogLevel.Verbose, "read enough to tell length");
                    length = Bits.Swap(BitConverter.ToInt32(buffer, 2)) + 6;
                    if (position >= length)
                    {
                        //Logging.Log(LogLevel.Verbose, "read entire pdu");
                        //Dump(String.Format("buffer before {0} {1}", position, length), buffer);
                        result = new byte[length];
                        Array.Copy(buffer, 0, result, 0, length);
                    }
                }
                if (result == null)
                {
                    //Logging.Log(LogLevel.Verbose, "nothing read yet");
                    while (true)
                    {
                        //Logging.Log("read from socket");
                        int count = input.Read(buffer, position, buffer.Length - position);
                        //Logging.Log("Receive {0} bytes", count);
                        position += count;
                        //Logging.Log("position is {0}", position);
                        if (count == 0)
                        {
                            Logging.Log("nothing returned");
                            break;
                        }
                        if (position < 6)
                        {
                            //Logging.Log(LogLevel.Verbose, "not enough to tell length");
                            continue;
                        }
                        length = Bits.Swap(BitConverter.ToInt32(buffer, 2)) + 6;
                        //Logging.Log("length is {0}", length);
                        if (position >= length)
                        {
                            //Logging.Log(LogLevel.Verbose, "read entire pdu");
                            //Dump(String.Format("buffer before {0} {1}", position, length), buffer);
                            result = new byte[length];
                            Array.Copy(buffer, 0, result, 0, length);
                            break;
                        }
                    }
                }
                if (result != null)
                {
                    //Logging.Log(LogLevel.Verbose, "we have data");
                    if (position > length)
                    {
                        //Logging.Log(LogLevel.Verbose, "we have enough");
                        Array.Copy(buffer, length, buffer, 0, position - length);
                        position -= length;
                    }
                    else
                    {
                        //Logging.Log(LogLevel.Verbose, "we start over");
                        position = 0;
                    }
                    //Array.Clear(buffer, position, buffer.Length - position);
                }
            }
            catch(Exception e)
            {
                result = null;
                Logging.Log("Exception caught in Receive.");
                Logging.Log(e.Message);
            }

            //Logging.Log(LogLevel.Verbose, "leaving Association.Receive");

            //Dump(String.Format("result {0}", (result!=null)?result.Length:0), result);
            //Dump(String.Format("buffer {0}", buffer.Length), buffer);
            return result;
        }

    }
}
