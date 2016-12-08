using System;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEcho
{
    public partial class MainForm : Form
    {
        private Server server;
        private string aetitle = "DICOMECHO";
        private int port = 2007;
        private Settings settings = new Settings("DicomEcho");

        public MainForm()
        {
            InitializeComponent();
            RefreshComboBox();
            SetHostName();
            StartServer();
        }

        private void StartServer()
        {
            server = new Server(aetitle, port);

            VerificationServiceSCP service = new VerificationServiceSCP();
            service.Syntaxes.Add(Syntax.ExplicitVrLittleEndian);
            service.Syntaxes.Add(Syntax.ExplicitVrBigEndian);
            service.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(service);

            server.Start();
        }

        private void StopServer()
        {
            server.Stop();
            server = null;
        }

        private void RefreshComboBox()
        {
            string current = settings["mru"];

            this.CalledAETitleComboBox.Items.Clear();
            foreach (String key in settings)
            {
                string value = settings[key];
                // skip the mru setting, etc.
                if (value.Contains(":"))
                {
                    this.CalledAETitleComboBox.Items.Add(key);
                }
            }

            if (this.CalledAETitleComboBox.Items.Count > 0)
            {
                if (current != null && current != String.Empty)
                {
                    this.CalledAETitleComboBox.SelectedIndex = this.CalledAETitleComboBox.Items.IndexOf(current);
                }
                else
                {
                    this.CalledAETitleComboBox.SelectedIndex = 0;
                }
            }
        }

        private void RunButton_Click(object sender, EventArgs e)
        {
            Cursor cursor;
            cursor = Cursor.Current;
            Cursor.Current = Cursors.WaitCursor;

            try
            {
                // set up the called and the calling
                ApplicationEntity scp = new ApplicationEntity();
                ApplicationEntity scu = new ApplicationEntity();

                scu.Title = this.CallingAETitleTextBox.Text;

                scp.Title = this.CalledAETitleComboBox.Text;
                scp.Address = IPAddress.Parse(this.IPAddressTextBox.Text);
                scp.Port = Int32.Parse(this.PortTextBox.Text);

                settings[scp.Title] = String.Format("{0}:{1}:{2}", scp.Title, scp.Address, scp.Port);
                settings["mru"] = scp.Title;

                // create a background thread to do the Echo so that we can pump messages and have the logging control updated
                Thread thread = new Thread(Run);
                thread.Name = "";
                thread.Start(new Arguments(scu, scp));

                // pump messages until the thread dies.
                while (thread.IsAlive)
                {
                    Application.DoEvents();
                }

                RefreshComboBox();
            }
            catch (Exception ex)
            {
                SetStatusBarText(ex.Message);
            }
            finally
            {
                Cursor.Current = cursor;
            }
        }

        private void Run(object args)
        {
            Arguments arguments = (Arguments)args;

            VerificationServiceSCU echo = new VerificationServiceSCU();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            Association association = new Association();
            association.AddService(echo);

            SetStatusBarText("Running ...");
            if (association.Open(arguments.Scp, arguments.Scu))
            {
                ServiceStatus status = echo.Echo();
                SetStatusBarText(status.ToString());
            }
            else
            {
                SetStatusBarText("Can't establish association");
            }

            association.Close();
            association.Dispose();

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CalledAETitleComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string title = this.CalledAETitleComboBox.SelectedItem.ToString();
            string info = settings[title];
            if (info != null)
            {
                string [] parts = info.Split(":".ToCharArray());
                this.IPAddressTextBox.Text = parts[1];
                this.PortTextBox.Text = parts[2];
            }
        }

        delegate void SetStatusBarTextHandler(string message);

        private void SetStatusBarText(string message)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new SetStatusBarTextHandler(SetStatusBarText), new object[] { message });
            }
            else
            {
                this.StatusBar.Text = message;
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            string title = this.CalledAETitleComboBox.SelectedItem.ToString();
            DialogResult result = System.Windows.Forms.MessageBox.Show(String.Format("Delete {0}", title), "Confirm Delete", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                settings.Remove(title);
                RefreshComboBox();
            }
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            string version = AssemblyName.GetAssemblyName(Assembly.GetAssembly(this.GetType()).Location).Version.ToString();
            string services = string.Empty;

            foreach (ServiceClass service in server.Services)
            {
                if (services.Length > 0)
                    services += "\n";
                services += Reflection.GetName(typeof(SOPClass), service.SOPClassUId);
            }

            MessageBox.Show(String.Format("DicomEditor, version {0}\n\n{1} on port {2}\n\n{3}", version, aetitle, port, services), "About");
        }

        private void HostNameButton_Click(object sender, EventArgs e)
        {
            SetHostName();
        }

        void SetHostName()
        {
            this.CallingAETitleTextBox.Text = Dns.GetHostName();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();
        }

    }

    /// <summary>
    /// This is used to pass arguments to the background thread
    /// </summary>
    internal class Arguments
    {
        ApplicationEntity scp;
        ApplicationEntity scu;

        public Arguments(ApplicationEntity scu, ApplicationEntity scp)
        {
            this.scu = scu;
            this.scp = scp;
        }

        public ApplicationEntity Scu
        {
            get
            {
                return scu;
            }
            set
            {
                scu = value;
            }
        }

        public ApplicationEntity Scp
        {
            get
            {
                return scp;
            }
            set
            {
                scp = value;
            }
        }
    }

}