using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// A Server is an object that listens on a port for connections and establishes an association for a calling SCU.
    /// </summary>
    public class Server
    {
        private Socket serversocket;
        private Thread mainthread;
        private int port;
        private string aeTitle;
        private System.Threading.ManualResetEvent started;
        private const int Timeout = 30000;
        private List<Association> associations;
        List<ServiceClass> services;
        Dictionary<string, ApplicationEntity> hosts;

        public Server(ApplicationEntity host) :
            this(host.Title, host.Port)
        {
        }

        public Server(string aeTitle, int port)
        {
            this.port = port;
            this.aeTitle = aeTitle;
            started = new ManualResetEvent(false);
            associations = new List<Association>();
            services = new List<ServiceClass>();
            hosts = new Dictionary<string, ApplicationEntity>();
        }

        public bool IsStarted
        {
            get
            {
                return started.WaitOne(0, false);
            }
        }

        public int ActiveConnections
        {
            get
            {
                return UpdateConnections();
            }
        }

        public string AETitle
        {
            get
            {
                return aeTitle;
            }
            set
            {
                aeTitle = value;
            }
        }

        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
            }
        }

        public List<ServiceClass> Services
        {
            get
            {
                return services;
            }
        }

        public Dictionary<string, ApplicationEntity> Hosts
        {
            get
            {
                return hosts;
            }
            set
            {
                hosts = value;
            }
        }

        /// <summary>
        /// Start the Server listening for and establishing associations.
        /// </summary>
        public void Start()
        {
            if (IsStarted)
            {
                string message = "Already Started!";
                Logging.Log(message);
                throw new Exception(message);
            }

            LoadHosts();

            mainthread = new Thread(new ThreadStart(ListenForConnections));
            mainthread.Name = "Server.ListenForConnections";
            mainthread.Start();

            if (!started.WaitOne(10000, false))
            {
                Logging.Log("Timeout waiting for Server to Start.");
            }

            Logging.Log("Started");
        }

        /// <summary>
        /// Stop the Server from establishing connections.
        /// </summary>
        public void Stop()
        {
            if (!IsStarted)
            {
                string message = "Not started!";
                Logging.Log(message);
                return;
            }

            // signal that we want to shutdown our server socket.
            // this should start the process of stopping the background thread
            if (serversocket != null)
            {
                Logging.Log("Socket will be shutdown.");
                serversocket.Close();
            }
            // even if the thread doesn't shutdown, we may be able to start 
            // another mainthread
            started.Reset();
            // if we have a background thread
            if (mainthread != null)
            {
                // wait a little bit
                if (!mainthread.Join(Timeout))
                {
                    // if the thread did not shutdown, be nasty about it.
                    Logging.Log("Timeout waiting for thread to complete!");
                    mainthread.Abort();
                }
            }
        }

        public void Register(ApplicationEntity entity)
        {
            if (hosts.ContainsKey(entity.Title))
            {
                hosts[entity.Title] = entity;
            }
            else
            {
                hosts.Add(entity.Title, entity);
            }
        }

        private void ListenForConnections()
        {
            try
            {
                // create a blocking TCP/IP stream based server socket
                // replaced  AddressFamily.InterNetwork with AddressFamily.InterNetworkV6 to support IPv6.
                serversocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
                serversocket.Blocking = true;

                // 27 is equivalent to IPV6_V6ONLY socket option in the winsock code libary.
                serversocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, 0);

                // bind and listen on the IPv4 and IPv6 wildcard address.
                IPEndPoint ipepServer = new IPEndPoint(IPAddress.IPv6Any, port);
                serversocket.Bind(ipepServer);
                serversocket.Listen(-1);

                // notify the foreground thread that we are listening.
                started.Set();

                Logging.Log("ListenForConnections");

                // until we are told to Stop
                while (true)
                {
                    Socket clientsock = null;
                    try
                    {
                        // block while we accept a new socket connection
                        clientsock = serversocket.Accept();
                    }
                    catch (SocketException)
                    {
                        // it is likely that the exception being caught here
                        // is that Accept has been interupted by the socket being shutdown
                        // during application exit, so no error is reported.
                        break;
                    }
                    catch (ObjectDisposedException e)
                    {
                        Logging.Log("ObjectDisposedException" + e.Message);
                        break;
                    }
                    catch (InvalidOperationException e)
                    {
                        Logging.Log("InvalidOperationException" + e.Message);
                        break;
                    }
                    if (clientsock.Connected)
                    {
                        lock (associations)
                        {
                            // we got one, setup a file server session for this socket
                            Association association = new Association(clientsock, services, hosts);
                           
                            associations.Add(association);
                            UpdateConnections();
                        }
                    }
                }
            }
            catch (SystemException e)
            {
                Logging.Log("SystemException" + e.Message);
            }
            finally
            {
                // do not leave without signaling that we are accepting connections
                started.Reset();
            }
        }

        private int UpdateConnections()
        {
            int count;
            lock (associations)
            {
                count = associations.Count;
                for (int n = count; n > 0; n--)
                {
                    Association connection = associations[n - 1];
                    if (connection.IsOpen())
                    {
                        associations.RemoveAt(n - 1);
                    }
                }
                count = associations.Count;
            }
            Logging.Log("UpdateConnections: {0} active connection(s).", count);
            return count;
        }

        public void AddService(ServiceClass service)
        {
            services.Add(service);
        }

        private void LoadHosts()
        {
            string path = "hosts";
            if (!File.Exists(path))
            {
                FileInfo info = new FileInfo(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                path = Path.Combine(info.DirectoryName, path);
            }
            if (File.Exists(path))
            {
                hosts = new Dictionary<string, ApplicationEntity>();
                using (StreamReader file = new StreamReader(path))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        ParseApplicationEntity(line, hosts);
                    }
                    file.Close();
                }
            }
            else
            {
                Logging.Log("No hosts file found.");
            }
        }

        private void ParseApplicationEntity(string line, Dictionary<string, ApplicationEntity> hosts)
        {
            string[] parts = line.Split(",".ToCharArray());
            if (parts.Length >= 3)
            {
                string title = parts[0].ToUpper().Trim();
                if(!hosts.ContainsKey(title))
                {
                    hosts[title] = new ApplicationEntity(title, IPAddress.Parse(parts[1].Trim()), Int32.Parse(parts[2].Trim()));
                }
                else
                {
                Logging.Log(LogLevel.Warning, String.Format("ignoring duplicate hosts entry, line={0}.", line));
                }
            }
            else
            {
                Logging.Log(LogLevel.Error, String.Format("hosts entry has too few fields, line={0}.", line));
            }
        }
    }
}
