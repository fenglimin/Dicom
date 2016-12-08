using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using System.Reflection;
using System.CodeDom;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

using EK.Capture.Dicom.DicomToolKit;

namespace DicomPipe
{
    public partial class MainForm : Form
    {
        private Socket serversocket;
        private Thread mainthread;
        private System.Threading.ManualResetEvent started = new ManualResetEvent(false);
        private const int Timeout = 30000;
        private List<Pipe> associations = new List<Pipe>();
        private Settings settings = new Settings("DicomPipe");
        private Assembly assembly;
        private System.Windows.Forms.Timer timer;
        private string script;

        public MainForm()
        {
            InitializeComponent();

            timer = new System.Windows.Forms.Timer();
            timer.Interval = 5000;
            timer.Tick += new EventHandler(OnTick);

            if (settings["PipePortTextBox"] == null || settings["PipePortTextBox"] == String.Empty)
            {
                settings["PipePortTextBox"] = "2010";
            }

            UpdateControls(true);
            Start();
            this.StopStartButton.Text = (IsStarted) ? "Stop" : "Start";
            this.StopStartButton.Focus();

            timer.Start();

        }

        private void OnTick(object sender, EventArgs args)
        {
            timer.Stop();
            StatusStripLabel.Text = "";
        }

        private Assembly GenerateAssembly(string name)
        {
            if (!File.Exists(name))
            {
                return null;
            }

            CompilerParameters parameters = new CompilerParameters();
            parameters.CompilerOptions = "/target:library /optimize";
            parameters.GenerateExecutable = false;
            parameters.GenerateInMemory = true;
            parameters.IncludeDebugInformation = false;
            parameters.ReferencedAssemblies.Add("mscorlib.dll");
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");

            // the list of assemblies could come from the config store
            // the assemblies need to be in the same folder as the executable
            parameters.ReferencedAssemblies.Add(@"EK.Capture.Dicom.DicomToolKit.dll");

            // TODO scan the script for assembly references and Add them

            string source = System.IO.File.ReadAllText(name);

            // compile the code
            CodeDomProvider provider = CodeDomProvider.CreateProvider("CSharp");
            CompilerResults results = provider.CompileAssemblyFromSource(parameters, source);

            // throw any compiler errors
            if (results.Errors.Count > 0)
            {
                StringBuilder text = new StringBuilder();
                foreach (CompilerError error in results.Errors)
                {
                    text.Append("Compile Error:  " + error.ErrorText + "\r\n");
                }
                throw new Exception(text.ToString());
            }

            SetStatus(String.Format("{0} loaded", name));
            script = name;
            return results.CompiledAssembly;
        }


        private void UpdateControls(bool loading)
        {
            if (loading)
            {
                this.PipePortTextBox.Text = settings["PipePortTextBox"];
                this.LoggingCheckBox.Checked = bool.Parse((settings["LoggingCheckBox"] !=null) ? settings["LoggingCheckBox"] : false.ToString());
                RefreshHostComboBox(ScpTitleComboBox);
                RefreshHostComboBox(ScuTitleComboBox);
                RefreshScriptComboBox();
            }
            else
            {
                settings["PipePortTextBox"] = this.PipePortTextBox.Text;
                settings["LoggingCheckBox"] = LoggingCheckBox.Checked.ToString();
                settings["scp"] = ScpTitleComboBox.Text;
                settings[ScpTitleComboBox.Text] = String.Format("{0}:{1}:{2}", ScpTitleComboBox.Text, ScpAddressTextBox.Text, ScpPortTextBox.Text);
                settings["scu"] = ScuTitleComboBox.Text;
                settings[ScuTitleComboBox.Text] = String.Format("{0}:{1}:{2}", ScuTitleComboBox.Text, ScuAddressTextBox.Text, ScuPortTextBox.Text);
                settings["script"] = ScriptComboBox.Text.ToLower();
            }
        }

        private void RefreshScriptComboBox()
        {
            this.ScriptComboBox.Items.Clear();
            DirectoryInfo dir = new DirectoryInfo(".");
            foreach (FileInfo info in dir.GetFiles("*.cs"))
            {
                this.ScriptComboBox.Items.Add(info.Name.ToLower());
            }

            string current = settings["script"];
            if (this.ScriptComboBox.Items.Count > 0)
            {
                if (current != null && current != String.Empty)
                {
                    this.ScriptComboBox.SelectedIndex = this.ScriptComboBox.Items.IndexOf(current);
                }
                else
                {
                    this.ScriptComboBox.SelectedIndex = 0;
                }
            }
        }

        private void RefreshHostComboBox(ComboBox combo)
        {
           combo.Items.Clear();
            foreach (String key in settings)
            {
                string value = settings[key];
                // skip the mru setting, etc.
                if (value.Contains(":"))
                {
                    combo.Items.Add(key);
                }
            }
            // "scp" or "scu"
            string host = combo.Name.Substring(0, 3).ToLower();
            string current = settings[host];
            if (combo.Items.Count > 0)
            {
                if (current != null && current != String.Empty)
                {
                    combo.SelectedIndex = combo.Items.IndexOf(current);
                }
                else
                {
                    combo.SelectedIndex = 0;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer.Stop();
            Stop();
        }

        public int ActiveConnections
        {
            get
            {
                return UpdateConnections();
            }
        }

        public bool IsStarted
        {
            get
            {
                return started.WaitOne(0, false);
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
                    Pipe connection = associations[n - 1];
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

        public void Start()
        {
            if (IsStarted)
            {
                string message = "Already Started!";
                Logging.Log(message);
            }

            if (ScpTitleComboBox.Text == String.Empty || ScpAddressTextBox.Text == String.Empty 
                || ScpPortTextBox.Text == String.Empty && PipePortTextBox.Text == String.Empty)
            {
                SetStatus("Missing required data");
                return;
            }

            try
            {
                assembly = GenerateAssembly(ScriptComboBox.Text);
            }
            catch (Exception ex)
            {
                SetStatus("Compiler error, not Started");
                MessageBox.Show(ex.Message);
                assembly = null;
                return;
            }

            mainthread = new Thread(new ThreadStart(ListenForConnections));
            mainthread.Name = "Server.ListenForConnections";
            mainthread.Start();

            if (!started.WaitOne(10000, false))
            {
                Logging.Log("Timeout waiting for Server to Start.");
            }

            UpdateControls(false);

            SetStatus("Started");
            Logging.Log("Started.");
        }

        public void Stop()
        {
            if (!IsStarted)
            {
                string message = "Not started!";
                Logging.Log(message);
                return;
            }

            assembly = null;

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
            SetStatus("Stopped");
        }

         private void ListenForConnections()
        {
            try
            {
                // create a blocking TCP/IP stream based server socket
                serversocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                serversocket.Blocking = true;


                // bind and listen on the wildcard address
                IPEndPoint ipepServer = new IPEndPoint(IPAddress.Any, Int32.Parse(settings["PipePortTextBox"]));
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
                            ApplicationEntity scp = GetApplicationEntity("scp");
                            ApplicationEntity scu = GetApplicationEntity("scu");
                            Pipe pipe = new Pipe(clientsock, scp, scu, assembly, LoggingCheckBox.Checked);

                            associations.Add(pipe);
                            SetStatus("connection");
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

        private delegate void SetStatusDelegate(string message);

        public void SetStatus(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStatusDelegate(SetStatus), new object[] { message });
            }
            else
            {
                string text = StatusStripLabel.Text;
                if (text.Length > 0)
                {
                    text += ", ";
                }
                text += message;
                StatusStripLabel.Text = text;
                timer.Start();
            }
        }

        private delegate ApplicationEntity GetGetApplicationEntityDelegate(string host);

        private ApplicationEntity GetApplicationEntity(string host)
        {
            if (this.InvokeRequired)
            {
                return (ApplicationEntity)this.Invoke(new GetGetApplicationEntityDelegate(GetApplicationEntity), new object[] { host });
            }
            else
            {
                if (host == "scu")
                {
                    return new ApplicationEntity(ScuTitleComboBox.Text, IPAddress.Parse(ScuAddressTextBox.Text), Int32.Parse(ScuPortTextBox.Text));
                }
                return new ApplicationEntity(ScpTitleComboBox.Text, IPAddress.Parse(ScpAddressTextBox.Text), Int32.Parse(ScpPortTextBox.Text));
            }
        }

        private void StopStartButton_Click(object sender, EventArgs e)
        {
            if (!IsStarted)
            {
                Start();
            }
            else
            {
                Stop();
            }
            this.StopStartButton.Text = (IsStarted) ? "Stop" : "Start";
        }

        private void TitleComboBox_SelectedValueChanged(object sender, EventArgs e)
        {
            ComboBox combo = sender as ComboBox;
            TextBox address = ScpAddressTextBox;
            TextBox port = ScpPortTextBox;
            if (combo.Name == "ScuTitleComboBox")
            {
                address = ScuAddressTextBox;
                port = ScuPortTextBox;
            }
            string title = combo.SelectedItem.ToString();
            string info = settings[title];
            if (info != null)
            {
                string[] parts = info.Split(":".ToCharArray());
                address.Text = parts[1];
                port.Text = parts[2];
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            ComboBox combo = (sender is Button && ((Button)sender).Name == "ScpDeleteButton") ? 
                ScpTitleComboBox : ScuTitleComboBox;

            string title = combo.SelectedItem.ToString();
            DialogResult result = System.Windows.Forms.MessageBox.Show(String.Format("Delete {0}", title), "Confirm Delete", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                settings.Remove(title);
                RefreshHostComboBox(combo);
            }
        }

    }
}
