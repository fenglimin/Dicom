using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EK.Capture.Dicom.DicomToolKit
{
    //TODO: implement State.Requesting and State.Notifiying in fsm, and is State.Waiting redundant.

    /// <summary>
    /// Represents a communication channel between two Dicom devices.
    /// </summary>
    public class Association : IDisposable
    {
        #region Fields

        private ApplicationEntity scp;
        private ApplicationEntity scu;
        private Socket socket = null;
        private byte[] buffer;
        private int position = 0;
        private State state;
        private Thread machineThread;
        private List<ServiceClass> services;
        private byte index = 0;
        private System.Threading.AutoResetEvent completeEvent = null;
        private System.Threading.AutoResetEvent threadEvent = null;
        private int packetSize;
        private int MaxPduSize = 262144;
        private FileStream file = null;
        private int number;
        private bool disposed = false;
        private AssociationAbortedException abortexception = null;
        private int timeout = 5000;
        private Dictionary<string, ApplicationEntity> hosts;

        static readonly string ImplementationClassUid = "1.2.840.113564.3.1.255";
        static string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        static object sentry = new object();
        static int serial;

		public string CallingAeTitle { get; set; }
		public IPAddress CallingAeIpAddress { get; set; }
        #endregion Fields

        #region Constructors and IDisposable

        /// <summary>
        /// Constructs an association used by an SCU.
        /// </summary>
        public Association()
        {
            Initialize();
        }

        /// <summary>
        /// Constructs an association used by an SCP using a socket and supporting a set of services
        /// </summary>
        /// <param name="socket">The server socket to use for the association.</param>
        /// <param name="services">The services that this SCP supports.</param>
        /// <param name="hosts">The workstations that this SCP has registered with it.</param>
        internal Association(Socket socket, List<ServiceClass> services, Dictionary<string, ApplicationEntity> hosts)
        {
            Initialize();

            lock (sentry)
            {
                number = ++serial;
            }
//#if DEBUG
//            Console.WriteLine("socket {0,4} opened", number);
//#endif

            this.hosts = hosts;

            foreach(ServiceClass service in services)
            {
                AddService((ServiceClass)service.Clone());
            }

            this.socket = socket;
            this.State = State.Waiting;

            machineThread = new Thread(new ThreadStart(StateMachine));
            machineThread.Name = "Association.StateMachine";
            machineThread.Start();

            if (!threadEvent.WaitOne(timeout, false))
            {
                Logging.Log("Background thread not started.");
            }
        }

        ~Association()
        {
            Dispose(false);
        }

        private void Initialize()
        {
            index = 1;
            packetSize = 32768;
            buffer = new byte[MaxPduSize];
            State = State.Closed;
            completeEvent = new System.Threading.AutoResetEvent(false);
            threadEvent = new AutoResetEvent(false);
            services = new List<ServiceClass>();
        }

        /// <summary>
        /// Performs tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Conditionally releases managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether or not the method is called from user code.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!disposed)
                {
                    if (disposing)
                    {
                        if (IsOpen())
                        {
                            Close();
                        }
                        // clean up managed resources
                        if (socket != null)
                        {
                            socket.Close();
                            ((IDisposable)socket).Dispose();
                            socket = null;
                        }
                        if (completeEvent != null)
                        {
                            ((IDisposable)completeEvent).Dispose();
                            completeEvent = null;
                        }
                        if (threadEvent != null)
                        {
                            ((IDisposable)threadEvent).Dispose();
                            threadEvent = null;
                        }
                        if (file != null)
                        {
                            file.Close();
                            file.Dispose();
                            file = null;
                        }
                    }
                    // clean up unmanaged resources
                }
            }
            catch(Exception ex)
            {
                Logging.Log(LogLevel.Error, "Exception caught in Dispose, {0}", ex.Message);
            }
        }

        /// <summary>
        /// Insures that the object is not used after being Disposed.
        /// </summary>
        private void AssertValid()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("Association");
            }
        }

        #endregion Constructors and IDisposable

        #region Properties

        /// <summary>
        /// The current state of the association.
        /// </summary>
        public State State
        {
            get
            {
                return state;
            }
            internal set
            {
                //Logging.Log("State transition {0} to {1}", state, value);
                state = value;
            }
        }

        public bool IsOpen()
        {
            return (state == State.Open || state == State.Requesting || state == State.Notifiying || state == State.Waiting);
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Establish an association with an SCP using the current machine name as the calling AE title.
        /// </summary>
        /// <param name="provider">The AE title of the SCP.</param>
        /// <param name="address">The TCP/IP address of the SCP.</param>
        /// <param name="port">The TCP/IP port of the SCP.</param>
        /// <returns></returns>
        public bool Open(ApplicationEntity host)
        {
            return Open(host, new ApplicationEntity(Dns.GetHostName(), IPAddress.Loopback, 0));
        }

        /// <summary>
        /// Establish an association with an SCP using the current machine name as the calling AE title.
        /// </summary>
        /// <param name="provider">The AE title of the SCP.</param>
        /// <param name="address">The TCP/IP address of the SCP.</param>
        /// <param name="port">The TCP/IP port of the SCP.</param>
        /// <returns></returns>
        public bool Open(string provider, IPAddress address, int port)
        {
            return Open(new ApplicationEntity(provider, address, port), new ApplicationEntity(Dns.GetHostName(), IPAddress.Loopback, 0));
        }

        /// <summary>
        /// Establish an association with an SCP using the provided calling AE title.
        /// </summary>
        /// <param name="user">The calling AE title to use.</param>
        /// <param name="provider">The AE title of the SCP.</param>
        /// <param name="address">The TCP/IP address of the SCP.</param>
        /// <param name="port">The TCP/IP port of the SCP.</param>
        /// <returns></returns>
        public bool Open(string user, string provider, IPAddress address, int port)
        {
            return Open(new ApplicationEntity(provider, address, port), new ApplicationEntity(user, IPAddress.Loopback, 0));
        }

        /// <summary>
        /// Establish an association between two Dicom devices.
        /// </summary>
        /// <param name="scp">The called device.</param>
        /// <param name="scu">The calling device.</param>
        /// <returns></returns>
        public bool Open(ApplicationEntity scp, ApplicationEntity scu)
        {
            this.scp = scp;
            this.scu = scu;

            // connect
            if (!Connect(scp.Address, scp.Port))
            {
                return false;
            }

            machineThread = new Thread(new ThreadStart(StateMachine));
            machineThread.Name = "Association.StateMachine";
            machineThread.Start();

            if (!threadEvent.WaitOne(timeout, false))
            {
                Logging.Log("Background thread not started.");
                State = State.Closed;
                return false;
            }

            // get request
            AssociateRequestPdu pdu = GetAssociationRequestPdu();
            // write pdu
            MemoryStream memory = new MemoryStream();
            pdu.Write(memory);

            //pdu.Dump();
            Dump(">> "+"A-ASSOCIATE-RQ", memory.ToArray());

            State = State.Opening;

            NetworkStream output = new NetworkStream(socket, FileAccess.Write, false);
            output.Write(memory.ToArray(), 0, (int)memory.Length);

            if (!completeEvent.WaitOne(timeout, false))
            {
                Logging.Log("A-ASSOCIATE-RQ FAILED");
                State = State.Closed;
                return false;
            }
            return (State == State.Open);
        }

        /// <summary>
        /// Closes the Association.
        /// </summary>
        /// <returns>True if we get a response, False otherwise.</returns>
        public bool Close()
        {
            bool result = true;
            if (!disposed)
            {
                try
                {
                    if (IsOpen())
                    {
                        AssociationReleasePdu release = new AssociationReleasePdu(ProtocolDataUnit.Type.A_RELEASE_RQ);

                        MemoryStream memory = new MemoryStream();
                        release.Write(memory);

                        Dump(">> " + "A-RELEASE-RQ", memory.ToArray());

                        NetworkStream output = new NetworkStream(socket, FileAccess.Write, false);

                        State = State.Closing;
                        output.Write(memory.ToArray(), 0, (int)memory.Length);

                        // get response
                        if (!completeEvent.WaitOne(timeout, false))
                        {
                            result = false;
                        }
                    }
                    // close socket
                    if (socket.Connected)
                    {
                        socket.Close();
                        if (!threadEvent.WaitOne(timeout, false))
                        {
                            Logging.Log("Background thread did not complete.");
                        }
                    }
                }
                catch (Exception e)
                {
                    State = State.Closed;
                    Logging.Log(e.Message);
                    result = false;
                }
            }
            State = State.Closed;
            return result;
        }

        /// <summary>
        /// Aborts the association with no reason given.
        /// </summary>
        public void Abort()
        {
            Abort(AbortSource.ServiceUser, AbortReason.NoReasonGiven);
        }

        /// <summary>
        /// Aborts the association with a reason.
        /// </summary>
        public void Abort(AbortReason reason)
        {
            Abort(AbortSource.ServiceUser, reason);
        }


        /// <summary>
        /// Aborts the association.
        /// </summary>
        internal void Abort(AbortSource source, AbortReason reason)
        {
            AssertValid();
            if (!IsOpen())
            {
                throw new Exception("Association not open.");
            }

            // the exception should be thrown on the foreground service thread
            abortexception = new AssociationAbortedException(source, reason, "Association aborted at the client's request.");
            State = State.Aborted;
            // the association is aborted, free any services awaiting respsonses
            foreach (ServiceClass service in services)
            {
                service.completeEvent.Set();
            }

            AssociateAbortPdu abort = new AssociateAbortPdu();
            abort.Source = source;
            abort.reason = reason;

            Send("A-ABORT", abort);

        }

        internal AssociationAbortedException AbortException
        {
            get
            {
                if (abortexception == null)
                {
                    abortexception = new AssociationAbortedException(AbortSource.ServiceUser, AbortReason.NoReasonGiven);
                }
                return abortexception;
            }
            set
            {
                abortexception = value;
            }
        }

        /// <summary>
        /// The current list of services added to the Association.
        /// </summary>
        internal List<ServiceClass> Services
        {
            get
            {
                return services;
            }
        }

        /// <summary>
        /// Adds a Service to the Association.
        /// </summary>
        /// <param name="service"></param>
        public void AddService(ServiceClass service)
        {
            service.Association = this;
            services.Add(service);
        }

        /// <summary>
        /// The amount of time, in milliseconds, that the Association will use when waiting for responses and threads.
        /// </summary>
        /// <remarks>The value will be constrained between 3 and 30 seconds.  The default is 5 seconds.</remarks>
        public int Timeout
        {
            get
            {
                return timeout;
            }
            set
            {
                if (value < 3000)
                {
                    timeout = 3000;
                }
                else if (value > 30000)
                {
                    timeout = 30000;
                }
                else
                {
                    timeout = value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ApplicationEntity Scu
        {
            get
            {
                return scu;
            }
        }

        internal Dictionary<string, ApplicationEntity> Hosts
        {
            get
            {
                return hosts;
            }
        }

        #endregion Public Methods

        #region Private and Internal Methods

        #region delete me

        internal bool SendPdu(ServiceClass service, string message, DicomObject pdu)
        {
            //Logging.Log("Association.SendPdu");

            try
            {

                if (State == State.Aborted)
                {
                    throw AbortException;
                }

                MemoryStream memory = new MemoryStream();
                pdu.Write(memory);

                NetworkStream output = new NetworkStream(socket, FileAccess.Write, false);

                byte[] bytes = memory.ToArray();
                Dump(">> " + message, bytes);

                if (service != null)
                {
                    service.LastMessage = null;
                }
                output.Write(bytes, 0, (int)memory.Length);

                State = (State != State.Closing) ? State.Waiting : State;
            }
            catch (Exception ex)
            {
                Logging.Log(LogLevel.Error, ex.Message);
                throw;
            }
            return true;
        }

        internal bool SendDataPdu(ServiceClass service, string message, DataSet dicom)
        {
            bool result = true;

            dicom.Part10Header = false;
            dicom.TransferSyntaxUID = service.Syntaxes[0];

            MemoryStream memory = new MemoryStream();
            dicom.Write(memory);
            byte[] bytes = memory.ToArray();

            PresentationDataPdu pdu = null;
            PresentationDataValue pdv = null;

            int index = 0;
            int remaining = (int)memory.Length;
            int size = this.packetSize - 128;

            while (remaining > this.packetSize && result)
            {
                pdu = new PresentationDataPdu(service.Syntaxes[0]);
                pdv = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], MessageType.DataSet, bytes, index, size);
                pdu.Values.Add(pdv);

                result = SendPdu(service, message, pdu);

                index += size;
                remaining -= size;
            }

            if (result)
            {
                pdu = new PresentationDataPdu(service.Syntaxes[0]);
                pdv = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], MessageType.LastDataSet, bytes, index, remaining);
                pdu.Values.Add(pdv);
                result = SendPdu(service, message, pdu);
            }

            return result;
        }

        #endregion delete me

        internal bool SendPdu(ServiceClass service, MessageType type, string message, DataSet dicom)
        {
            bool result = true;

            if (State == State.Aborted)
            {
                throw AbortException;
            }
            if (service != null)
            {
                service.LastMessage = null;
            }

            dicom.Part10Header = false;
            dicom.TransferSyntaxUID = MessageControl.IsCommand(type) ? Syntax.ImplicitVrLittleEndian : service.Syntaxes[0];

            MemoryStream memory = new MemoryStream();
            dicom.Write(memory);
            byte[] bytes = memory.ToArray();

            PresentationDataPdu pdu = null;
            PresentationDataValue pdv = null;

            int index = 0;
            int remaining = (int)memory.Length;
            int size = this.packetSize - 128;

            State = (State != State.Closing) ? State.Waiting : State;
            while (remaining > this.packetSize && result)
            {
                pdu = new PresentationDataPdu(service.Syntaxes[0]);
                pdv = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], type, bytes, index, size);
                pdu.Values.Add(pdv);

                result = Send(message, pdu);

                index += size;
                remaining -= size;
            }

            if (result)
            {
                pdu = new PresentationDataPdu(service.Syntaxes[0]);
                pdv = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], type+2, bytes, index, remaining);
                pdu.Values.Add(pdv);
                result = Send(message, pdu);
            }

            return result;
        }

        internal bool Send(string message, DicomObject fragment)
        {
            //Logging.Log("Association.Send");

            try
            {
                if (fragment.Size > this.packetSize)
                {
                    Logging.Log(LogLevel.Error, "fragment exceeds packet size.");
                    return false;
                }

                MemoryStream memory = new MemoryStream();
                fragment.Write(memory);

                NetworkStream output = new NetworkStream(socket, FileAccess.Write, false);

                byte[] bytes = memory.ToArray();
                Dump(">> " + message, bytes);

                output.Write(bytes, 0, (int)memory.Length);

                State = (State != State.Closing) ? State.Waiting : State;
            }
            catch (Exception ex)
            {
                Logging.Log(LogLevel.Error, ex.Message);
                return false;
            }
            return true;
        }

        private void OnOpening(byte [] pdu)
        {
            // parse response
            MemoryStream memory = new MemoryStream(pdu, 0, pdu.Length);

            switch ((ProtocolDataUnit.Type)pdu[0])
            {
                case ProtocolDataUnit.Type.A_ASSOCIATE_AC:
                    {
                        Dump("<< A-ASSOCIATE-AC", pdu);

                        AssociateRequestPdu response = new AssociateRequestPdu();
                        response.Read(memory);
                        //response.Dump();

                        foreach (Item item in response.fields)
                        {
                            Logging.Log(item.Dump());
                            switch ((ItemType)item.Type)
                            {
                                case ItemType.PresentationContextItemAC:
                                    PresentationContextItem pci = item as PresentationContextItem;
                                    SyntaxItem transfer = ((SyntaxItem)pci.fields[0]);
                                    ServiceClass service = FindServiceClass(pci.PresentationContextId);
                                    if (service != null)
                                    {
                                        service.Syntaxes.Clear();
                                        service.PciReason = pci.PciReason;
                                        if (service.PciReason == PCIReason.Accepted)
                                        {
                                            service.Syntaxes.Add(transfer.Syntax);
                                            Logging.Log("SCP Accept PresentationContext={0} class={1} syntax={2}", pci.PresentationContextId, Reflection.GetName(typeof(SOPClass), service.SOPClassUId), Reflection.GetName(typeof(Syntax), transfer.Syntax));
                                        }
                                        else
                                        {
                                            Logging.Log("SCP Reject PresentationContext={0} class={1} syntax={2}", pci.PresentationContextId, Reflection.GetName(typeof(SOPClass), service.SOPClassUId), Reflection.GetName(typeof(Syntax), transfer.Syntax));
                                        }
                                    }
                                    else
                                    {
                                        Logging.Log(LogLevel.Error, "SCP unknown PresentationContext={0} reason={1} class={2} syntax={3}", pci.PresentationContextId, pci.PciReason, Reflection.GetName(typeof(SOPClass), service.SOPClassUId), Reflection.GetName(typeof(Syntax), transfer.Syntax));
                                    }
                                    break;
                                case ItemType.UserInformationItem:
                                    UserInfoItem uii = item as UserInfoItem;
                                    int count = uii.fields.Count;
                                    for (int n = count - 1; n >= 0; n--)
                                    {
                                        Item field = uii.fields[n];
                                        if ((ItemType)field.Type == ItemType.MaximumLengthSubItem)
                                        {
                                            MaximumLengthItem length = field as MaximumLengthItem;
                                            packetSize = length.PacketSize;
                                        }
                                    }
                                    break;
                            }
                        }
                        
                        State = State.Open;
                    }
                    break;
                case ProtocolDataUnit.Type.A_ASSOCIATE_RJ:
                    {
                        AssociateRejectPdu response = new AssociateRejectPdu();
                        response.Read(memory);
                        State = State.Closed;

                        Dump("<< A-ASSOCIATE-RJ", pdu);
                    }
                    break;
                default:
                    State = State.Closed;
                    Dump(String.Format("<< UNEXPECTED PDU(0x{0:x4})", pdu[0]), pdu);
                    break;
            }

            if (completeEvent != null)
            {
                completeEvent.Set();
            }
        }

        private void OnWaiting(byte [] pdu)
        {

            // don't forget to add Dump if you add to this method
            //Logging.Log("OnWaiting {0}", (pdu!=null)?pdu[0].ToString():"null");


            switch ((ProtocolDataUnit.Type)pdu[0])
            {
                case ProtocolDataUnit.Type.A_ASSOCIATE_RQ:
                    AssociateRequestReceived(pdu);
                    break;
                case ProtocolDataUnit.Type.A_ASSOCIATE_AC:
                    break;
                case ProtocolDataUnit.Type.A_ASSOCIATE_RJ:
                    break;
                case ProtocolDataUnit.Type.P_DATA_TF:
                    PduDataReceived(pdu);
                    break;
                case ProtocolDataUnit.Type.A_RELEASE_RQ:
                    AssociationReleaseReceived(pdu);
                    break;
                case ProtocolDataUnit.Type.A_RELEASE_RP:
                    break;
                case ProtocolDataUnit.Type.A_ABORT:
                    AssociationAbortReceived(pdu);
                    break;
                default:
                    Logging.Log(LogLevel.Error, "Unsupported ProtocolDataUnit.Type={0}", pdu[0]);
                    Dump("<< UNKNOWN", pdu);
                    break;
            }

        }

        private void OnClosing(byte[] pdu)
        {
            // parse response
            MemoryStream memory = new MemoryStream(pdu, 0, pdu.Length);
            AssociationReleasePdu response = new AssociationReleasePdu();
            response.Read(memory);

            switch ((ProtocolDataUnit.Type)response.PduType)
            {
                case ProtocolDataUnit.Type.A_RELEASE_RP:
                    State = State.Closed;
                    Dump("<< A-RELEASE-RP", pdu);
                    break;
                default:
                    State = State.Closed;
                    Dump(String.Format("<< UNEXPECTED PDU(0x{0:x4})", (byte)response.PduType), pdu);
                    break;
            }

            completeEvent.Set();
        }

        private void AssociateRequestReceived(byte[] pdu)
        {
            Dump("<< A-ASSOCIATE-RQ", pdu);
//#if DEBUG
//            if (number != 0) Console.WriteLine("association {0,4} opened", number);
//#endif
            bool reject = true;
            // parse request
            MemoryStream memory = new MemoryStream(pdu, 0, pdu.Length);
            AssociateRequestPdu request = new AssociateRequestPdu();
            request.Read(memory);

            //request.Dump();
	        CallingAeTitle = request.calling.TrimEnd();
            AssociateRequestPdu accept = new AssociateRequestPdu(ProtocolDataUnit.Type.A_ASSOCIATE_AC, request.called, request.calling);

            // record as much as we can about the caller
            IPEndPoint lep = socket.LocalEndPoint as IPEndPoint;
            if (lep != null)
            {
                scp = new ApplicationEntity(request.calling.TrimEnd(" ".ToCharArray()), lep.Address, lep.Port);
            }
            else
            {
                scp = new ApplicationEntity(request.calling.TrimEnd(" ".ToCharArray()), 0);
            }
            IPEndPoint rep = socket.RemoteEndPoint as IPEndPoint;
            if (rep != null)
            {
                scu = new ApplicationEntity(request.calling.TrimEnd(" ".ToCharArray()), rep.Address, rep.Port);
	            CallingAeIpAddress = rep.Address;
            }
            else
            {
                scu = new ApplicationEntity(request.calling.TrimEnd(" ".ToCharArray()), 0);
            }

            foreach (Item item in request.fields)
            {
                switch ((ItemType)item.Type)
                {
                    case ItemType.ApplicationItem:
                        ApplicationContextItem aci = item as ApplicationContextItem;
                        accept.fields.Add(aci);
                        break;
                    case ItemType.PresentationContextItemRQ:
                        PresentationContextItem temp = item as PresentationContextItem;
                        PresentationContextItem response = new PresentationContextItem((byte)ItemType.PresentationContextItemAC, temp.PresentationContextId);
                        
                        SyntaxItem abs = ((SyntaxItem)temp.fields[0]);
                        ServiceClass service = null;
                        SyntaxItem transfer = null;
                        for(int n = 1; n < temp.fields.Count; n++)
                        {
                            transfer = temp.fields[n] as SyntaxItem;
                            service = FindServiceClass(abs.Syntax, transfer.Syntax);
                            if(service != null)
                            {
                                break;
                            }
                        }
                        if (service == null)
                        {
                            response.PciReason = PCIReason.AbstractSyntaxNotSupported;
                            Logging.Log("SCU Reject PresentationContext={0} abstract={1}", temp.PresentationContextId, Reflection.GetName(typeof(SOPClass), abs.Syntax));
                        }
                        else
                        {
                            service.PresentationContextId = temp.PresentationContextId;
                            service.Syntaxes.Clear();
                            service.Syntaxes.Add(transfer.Syntax);

                            service.PciReason = response.PciReason = PCIReason.Accepted;
                            Logging.Log("SCU Accept PresentationContext={0} abstract={1} transfer={2}", service.PresentationContextId, Reflection.GetName(typeof(SOPClass), abs.Syntax), Reflection.GetName(typeof(Syntax), transfer.Syntax));
                            reject = false;
                        }

                        response.fields.Add(transfer);
                        accept.fields.Add(response);

                        break;
                    case ItemType.UserInformationItem:
                        UserInfoItem uii = item as UserInfoItem;
                        int count = uii.fields.Count;
                        // note: this is editing the sent UserInfoItem in place rather
                        // than making a new object, make a new object
                        for(int n = count-1; n >= 0; n--)
                        {
                            Item field = uii.fields[n];
                            switch ((ItemType)field.Type)
                            {
                                case ItemType.MaximumLengthSubItem:
                                    {
                                        MaximumLengthItem length = field as MaximumLengthItem;
                                        packetSize = length.PacketSize;
                                    }
                                    break;
                                case ItemType.ImplementationClassUidSubItem:
                                    {
                                        SyntaxItem implementation = field as SyntaxItem;
                                        implementation.Syntax = ImplementationClassUid;
                                    }
                                    break;
                                case ItemType.ImplementationVersionNameSubItem:
                                    {
                                        SyntaxItem implementation = field as SyntaxItem;
                                        implementation.Syntax = version;
                                    }
                                    break;
                                case ItemType.AsynchronousOperationsWindowSubItem:
                                case ItemType.ScuScpRoleSubItem:
                                default:
                                    uii.fields.RemoveAt(n);
                                    break;
                            }
                        }
                        accept.fields.Add(uii);
                        break;
                    default:
                        break;
                }
            }
            if (reject)
            {
                Abort(AbortSource.ServiceProvider, AbortReason.NoReasonGiven);
            }
            else
            {
                Send("A-ASSOCIATE-AC", accept);
            }
        }

        private void AssociationReleaseReceived(byte [] pdu)
        {
            Dump("<< A-RELEASE-RQ ...", pdu);
//#if DEBUG
//           if (number != 0) Console.WriteLine("association {0,4} closed", number);
//#endif
            AssociationReleasePdu release = new AssociationReleasePdu(ProtocolDataUnit.Type.A_RELEASE_RP);
            State = State.Closed;
            Send("A-RELEASE-RP", release);
        }

        private void AssociationAbortReceived(byte[] pdu)
        {
            Dump("<< A-ABORT", pdu);
//#if DEBUG
//            if (number != 0) Console.WriteLine("association {0,4} aborted", number);
//#endif
            // parse response
            MemoryStream memory = new MemoryStream(pdu, 0, pdu.Length);
            AssociateAbortPdu response = new AssociateAbortPdu();
            response.Read(memory);

            // the exception should be thrown on the foreground service thread
            abortexception = new AssociationAbortedException(response.Source, response.Reason, String.Format("Association aborted by provider ({0}).", response.Reason));
            State = State.Aborted;
            // the association is aborted, free any services awaiting respsonses
            foreach (ServiceClass service in services)
            {
                service.completeEvent.Set();
            }
        }

        private void CheckAborted()
        {
            if (State == State.Aborted && abortexception != null)
            {
                throw abortexception;
            }
        }

        private void PduDataReceived(byte[] pdu)
        {
            //Logging.Log("PduDataReceived {0}", pdu);
            //Dump("<< P-DATA-TF", pdu);

            // TODO this must be able to handle multiple pdvs

            string syntax = Syntax.ImplicitVrLittleEndian;
            ServiceClass service = FindServiceClass(pdu[10]);
            if (service != null)
            {
                syntax = service.Syntaxes[0];
            }

            // TODO currently assuming that a Command comes in a single pdu
            // TODO all DataSet fragments are combined into a single Message

            if (MessageControl.IsDataSet((MessageType)pdu[11]))
            {
                //Logging.Log("dataset");
                if (MessageControl.IsNotLast((MessageType)pdu[11]))
                {
                    //Logging.Log("not last fragment");
                    if (file == null)
                    {
                        file = new FileStream(Guid.NewGuid().ToString()+".tmp", FileMode.Create, FileAccess.ReadWrite);
                        //Dump("PduDataReceived1 ...", pdu);
                    }
                    else
                    {
                        //Logging.Log("Receiving {0} bytes", pdu.Length - 12);
                    }
                    file.Write(pdu, 12, pdu.Length - 12);
                    return;
                }
                else
                {
                    //Logging.Log("last fragment");
                    if (file != null)
                    {
                        Dump("PduDataReceived2 ...", pdu);

                        file.Write(pdu, 12, pdu.Length - 12);
                        file.Flush();

                        DataSet dicom = new DataSet();
                        dicom.TransferSyntaxUID = syntax;

                        file.Seek(0, SeekOrigin.Begin);
                        dicom.Read(file);

                        if (service != null)
                        {
                            try
                            {
                                service.LastMessage = new Message(dicom);
                                ((IPresentationDataSink)service).OnData(MessageType.LastDataSet, service.LastMessage);
                            }
                            catch (Exception ex)
                            {
                                Logging.Log(LogLevel.Error, "Exception in OnData for SOPClassUId={0}, {1}", service.SOPClassUId, ex.Message);
                            }
                        }

                        file.Close();
                        File.Delete(file.Name);
                        file = null;
                        return;
                    }
                }
            }

            // parse response
            MemoryStream memory = new MemoryStream(pdu, 0, pdu.Length);
            PresentationDataPdu response = new PresentationDataPdu(syntax);
            response.Read(memory);

            response.Dump();

            foreach(PresentationDataValue pdv in response.Values)
            {
                service = FindServiceClass(pdv.context);
                if( service != null && service is IPresentationDataSink)
                {
                    service.LastMessage = new Message(pdv.Dicom);
                    ((IPresentationDataSink)service).OnData(pdv.control, service.LastMessage);
                }
            }
        }

        private ServiceClass FindServiceClass(int context)
        {
            foreach (ServiceClass service in services)
            {
                if (service.PresentationContextId == context)
                {
                    return service;
                }
            }
            return null;
        }

        private ServiceClass FindServiceClass(string uid, string transfer)
        {
            foreach (ServiceClass service in services)
            {
                if (service.SOPClassUId == uid)
                {
                    foreach (String temp in service.Syntaxes)
                    {
                        if (temp == transfer)
                        {
                            return service;
                        }
                    }
                }
            }
            return null;
        }

        private AssociateRequestPdu GetAssociationRequestPdu()
        {
            //Logging.Log("Association.GetAssociationRequestPdu");

            AssociateRequestPdu pdu = new AssociateRequestPdu(ProtocolDataUnit.Type.A_ASSOCIATE_RQ, scp.Title, scu.Title);

            ApplicationContextItem ac = new ApplicationContextItem("1.2.840.10008.3.1.1.1");
            pdu.fields.Add(ac);

            foreach (ServiceClass service in services)
            {
                service.PresentationContextId = index;
                index += 2;
                PresentationContextItem pci = new PresentationContextItem((byte)ItemType.PresentationContextItemRQ, service.PresentationContextId);
                pdu.fields.Add(pci);

                SyntaxItem abs = new SyntaxItem((byte)ItemType.AbstractSyntaxSubItem, service.SOPClassUId);
                pci.fields.Add(abs);

                foreach (string syntax in service.Syntaxes)
                {
                    SyntaxItem trx = new SyntaxItem((byte)ItemType.TransferSyntaxSubItem, syntax);
                    pci.fields.Add(trx);
                }
            }

            UserInfoItem user = new UserInfoItem();
            pdu.fields.Add(user);

            MaximumLengthItem ml = new MaximumLengthItem(MaxPduSize);
            user.fields.Add(ml);

            SyntaxItem impl = new SyntaxItem((byte)ItemType.ImplementationClassUidSubItem, "1.2.840.113564.3.2");
            user.fields.Add(impl);

            AsynchronousOperationsWindowItem operations = new AsynchronousOperationsWindowItem();
            user.fields.Add(operations);

            foreach (ServiceClass service in services)
            {
                ScpScuRoleSelectionItem role = new ScpScuRoleSelectionItem(service.SOPClassUId, service.Role);
                user.fields.Add(role);
            }
            
            SyntaxItem implver = new SyntaxItem((byte)ItemType.ImplementationVersionNameSubItem, "not specified");
            user.fields.Add(implver);

            return pdu;
        }

        private bool Connect(IPAddress host, int port)
        {
            bool succeeded = true;
            if (socket != null)
            {
                socket.Close();
                socket = null;
            }
            IPEndPoint ipepServer = null;

            ipepServer = new IPEndPoint(host, port);

            // Use address family so IPv4 and IPv6 are supported.
            socket = new Socket(ipepServer.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Blocking = true;
            succeeded = true;
            try
            {
                socket.Connect(ipepServer);
            }
            catch(Exception ex)
            {
                Logging.Log(LogLevel.Error, ex.Message);
                succeeded = false;
            }
            return succeeded;
        }

        private void StateMachine()
        {
            threadEvent.Set();
            while (true)
            {
                byte [] pdu = Receive();
                if (pdu == null)
                {
                    break;
                }
                //Dump("<< received ...", pdu);
                lock (this)
                {
                    switch (State)
                    {
                        case State.Opening:
                            OnOpening(pdu);
                            break;
                        case State.Waiting:
                            OnWaiting(pdu);
                            break;
                        case State.Closing:
                            OnClosing(pdu);
                            break;
                        default:
                            break;
                    }
                }
            }
            State = State.Closed;
            if (threadEvent != null)
            {
                threadEvent.Set();
            }
        }

        /*
         * weaknesses:
         *  buffers are always copied, do we need to double buffer socket
         *  if pdu length is not good, we wait

        /* loop
         *  if we have enough to determine length
         *      determine length
         *      if we have entire pdu
         *          copy into results
         *  else if we have read nothing
         *      loop
         *          read from socket into buffer
         *          if nothing
         *              socket is closed so break
         *          if not enough to determine length
         *              continue
         *          determine length
         *          if we have entire pdu
         *              copy into results
         *              break
         *          continue
         *  if we have read something
         *      if we have a pdu and then some
         *          move any extra to front of buffer
         *          reposition pointer to end of buffer
         *      else
         *          reposition pointer to start of buffer
         *  continue
        */
        byte[] Receive()
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

                    NetworkStream input = new NetworkStream(socket, FileAccess.Read, false);
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
//#if DEBUG
//                            if(number!=0) Console.WriteLine("socket {0,4} closed", number);
//#endif
                            State = State.Closed;
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
                State = State.Aborted;
                Logging.Log("Exception caught in Receive.");
                Logging.Log(e.Message);
            }

            //Logging.Log(LogLevel.Verbose, "leaving Association.Receive");

            //Dump(String.Format("result {0}", (result!=null)?result.Length:0), result);
            //Dump(String.Format("buffer {0}", buffer.Length), buffer);
            return result;
        }

        private void Dump(string message, byte[] bytes)
        {
            if (bytes.Length > MaxPduSize / 20)
            {
                Logging.Log(LogLevel.Verbose, "Dump {0} bytes.", bytes.Length);
            }
            else
            {
                string text = DicomObject.ToText(bytes);
                text = message + "\r\n" + text;
                Logging.Log(LogLevel.Verbose, text.ToString());
            }
        }

        #endregion Private and Internal Methods
    }

    public class AssociationAbortedException : Exception
    {
        private AbortSource source;
        private AbortReason reason;

        public AssociationAbortedException(AbortSource source, AbortReason reason)
            : base()
        {
            this.source = source;
            this.reason = reason;
        }

        public AssociationAbortedException(AbortSource source, AbortReason reason, string message)
            : base(message)
        {
            this.source = source;
            this.reason = reason;
        }

        public AssociationAbortedException(AbortSource source, AbortReason reason, string message, Exception innerException)
            : base(message, innerException)
        {
            this.source = source;
            this.reason = reason;
        }

        public new AbortSource Source
        {
            get
            {
                return source;
            }
        }

        public AbortReason Reason
        {
            get
            {
                return reason;
            }
        }
    }

    /// <summary>
    /// The states that an Association can be in.
    /// </summary>
    public enum State
    {

        // if you add an enumeration value, please update IsOpen

        /// <summary>The association is unitialized.</summary>
        Undefined,
        /// <summary>The association is open and accepted.</summary>
        Open,
        /// <summary>The association is opening.</summary>
        Opening,
        /// <summary>The association is closing.</summary>
        Closing,
        /// <summary>The association is closed.</summary>
        Closed,
        /// <summary>The association is sending a command or data.</summary>
        Requesting,
        /// <summary>The association is idle and waiting.</summary>
        Waiting,
        /// <summary>The association is in a service notification.</summary>
        Notifiying,
        /// <summary>The association is aborted.</summary>
        Aborted,
    }
}
