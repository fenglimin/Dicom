using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    public partial class MainForm : Form
    {
        private delegate bool DelegateOpenFile(String s);           // type
        private DelegateOpenFile OpenFileDelegate;                  // instance
        private Server server;
        private LogForm logging = null;
        private const string aetitle = "DICOMEDITOR";
        private const int port = 2009;
        private bool pacsmode = false;

        public MainForm(string[] args)
        {
            InitializeComponent();

            mruMenu = new MruMenuInline(menuFileRecentFile, new MruMenu.ClickedHandler(OnMenuFileMru), "Software\\Carestream Health\\DicomEditor\\MRU");

            foreach (string filename in args)
            {
                OpenFile(filename);
            }
        }

        private void NewMenuItem_Click(object sender, EventArgs e)
        {
            Browser child = new Browser("");
            child.MdiParent = this;
            child.Show();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (this.ActiveMdiChild is LogForm)
            {
                logging.Close();
                logging.Dispose();
                logging = null;
            }
            else if (this.ActiveMdiChild != null)
            {
                this.ActiveMdiChild.Close();
            }
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "dcm";
            dialog.Filter = "Dicom Files (*.dcm)|*.dcm|All files|*.*";
            dialog.Multiselect = true;
            dialog.ShowDialog();
            if( dialog.FileNames.Length > 0 )
            {
                foreach( string filename in dialog.FileNames )
                {
                    try
                    {
                        OpenFile(filename);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Logging.Log(ex));
                    }
                }
            }
        }

        private bool OpenFile(string filename)
        {
            bool fileOpened = true;

            try
            {
                if (File.Exists(filename))
                {
                    Browser child = new Browser(filename);
                    child.MdiParent = this;
                    child.WindowState = FormWindowState.Maximized;
                    child.Show();
                }
                else
                {
                    throw new Exception("File not found.");
                }
            }
            catch (Exception ex)
            {
                fileOpened = false;
                string errMsg = String.Format("Unable to open \"{0}\":  {1}", filename, ex.Message);
                MessageBox.Show(errMsg, "Dicom Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (fileOpened  &&  !openingFromMRU)
            {
                mruMenu.AddFile(filename);
            }

            return fileOpened;
        }

        private bool openingFromMRU = false;

        private void OnMenuFileMru(int number, String filename)
        {
            bool fileOpened = true;
            Application.DoEvents();
            openingFromMRU = true;

            try
            {
                fileOpened = OpenFile(filename);
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Unable to open \"{0}\":  {1}", filename, ex.Message);
                MessageBox.Show(errMsg, "Dicom Editor", MessageBoxButtons.OK, MessageBoxIcon.Error);

                fileOpened = false;
            }

            openingFromMRU = false;
            if (fileOpened) mruMenu.SetFirstFile(number);
            else mruMenu.RemoveFile(number);
        }

        private void CascadeMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.Cascade);
        }

        private void TileVerticalMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalMenuItem_Click(object sender, EventArgs e)
        {
            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        private void SaveMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild is Browser)
            {
                ((Browser)ActiveMdiChild).Save();
            }
            else
                SaveAs();
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void SaveAs()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.RestoreDirectory = true;
            dialog.AddExtension = true;

            if (ActiveMdiChild is Browser)
            {
                Browser child = (Browser)this.ActiveMdiChild;

                dialog.DefaultExt = "dcm";
                dialog.Filter = "All files|*.*|Dicom Files (*.dcm)|*.dcm|Text Files (*.txt)|*.txt";
                if (child.FileName == null || child.FileName.Length == 0)
                {
                    DataSet dicom = child.Dicom;
                    dialog.FileName = (dicom.Contains(t.SOPInstanceUID)) ? (string)dicom[t.SOPInstanceUID].Value + ".dcm" : "untitled.dcm";
                }
            }
            else if(ActiveMdiChild is LogForm)
            {
                dialog.DefaultExt = "log";
                dialog.Filter = "All files|*.*|Log Files (*.log)|*.log";
                dialog.FileName = String.Format("{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
            else
                return;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                if (ActiveMdiChild is Browser)
                {
                    Browser child = (Browser)this.ActiveMdiChild;
                    FileInfo info = new FileInfo(dialog.FileName);
                    if (info.Extension.ToLower() == ".txt")
                    {
                        BatchEditor.Save(child.Dicom, dialog.FileName);
                    }
                    else
                    {
                        child.FileName = dialog.FileName;
                        child.Save();
                    }
                }
                else if (ActiveMdiChild is LogForm)
                {
                    LogForm child = (LogForm)this.ActiveMdiChild;
                    child.Save(dialog.FileName);
                }
            }
        }

        private void MainForm_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array files = (Array)e.Data.GetData(DataFormats.FileDrop);

                if (files != null)
                {
                    for (int n = 0; n < files.Length; n++)
                    {
                         string s = files.GetValue(n).ToString();

                        // Call OpenFile asynchronously.
                        // Explorer instance from which file is dropped is not responding
                        // all the time when DragDrop handler is active, so we need to return
                        // immidiately (especially if OpenFile shows MessageBox).

                        this.BeginInvoke(OpenFileDelegate, new Object[] { s });
                    }
                    this.Activate();        // in the case Explorer overlaps this form
                }
            }
            catch (Exception)
            {
                //Debug.WriteLine("Error in DragDrop function: " + ex.Message);
                // don't show MessageBox here - Explorer is waiting !
            }
        }

        private void OnImageStored(object sender, ImageStoredEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new ImageStoredEventHandler(OnImageStored), new object[] { sender, e });
            }
            else
            {
                try
                {
                    if (pacsmode)
                    {
                        DataSet dicom = e.DataSet;
                        DicomDir dir = new DicomDir(".");
                        dir.Add(dicom);
                        dir.Save();
                    }
                    else
                    {
                        NewBrowser(e.DataSet);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
        }

        private void OnMpps(object sender, MppsEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MppsEventHandler(OnMpps), new object[] { sender, e });
            }
            else
            {
                try
                {
                    NewBrowser(e.DataSet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
        }

        private void OnJobPrinted(object sender, PrintJobEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new PrintJobEventHandler(OnJobPrinted), new object[] { sender, e });
            }
            else
            {
                FilmSession session = e.Session;
                //if (File.Exists("delete.me"))
                //{
                //    System.IO.FileStream file = null;
                //    System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
                //    try
                //    {
                //        using (file = new System.IO.FileStream("session.xml", System.IO.FileMode.Create))
                //        {
                //            formatter.Serialize(file, session);
                //        }
                //    }
                //    finally
                //    {
                //        if (file != null)
                //        {
                //            file.Close();
                //            file.Dispose();
                //            file = null;
                //        }
                //    }
                //}
                NewBrowser(session.Dicom);
                foreach (FilmBox filmbox in session.FilmBoxes)
                {
                    try
                    {
                        NewBrowser(filmbox.Dicom);
                        foreach (ImageBox image in filmbox.ImageBoxes)
                        {
                            try
                            {
                                NewBrowser(image.Dicom);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(Logging.Log(ex));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(Logging.Log(ex));
                    }
                }
            }
        }

        private void NewBrowser(EK.Capture.Dicom.DicomToolKit.DataSet dicom)
        {
            Browser child = new Browser(dicom);
            child.MdiParent = this;
            child.WindowState = FormWindowState.Maximized;
            child.Show();
        }

        private bool TestServer()
        {
            bool result = false;
            using (Association association = new Association())
            {
                VerificationServiceSCU echo = new VerificationServiceSCU();
                echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                association.AddService(echo);
                if (association.Open(aetitle, IPAddress.Parse("127.0.0.1"), port))
                {
                    result = true;
                }
                echo = null;
            }
            return result;
        }

        private delegate void SetStatusDelegate(string text);
        private void SetStatus(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStatusDelegate(SetStatus), new object[] { text });
            }
            StatusStripStatusLabel.Text = text;
        }

        private void StartServer()
        {
            if (TestServer())
            {
                SetStatus("SCP is already running in another instance.");
                return;
            }

            server = new Server(aetitle, port);

            VerificationServiceSCP echo = new VerificationServiceSCP();
            echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP dx1 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForPresentation);
            dx1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP dx2 = new StorageServiceSCP(SOPClass.DigitalXRayImageStorageForProcessing);
            dx2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP cr = new StorageServiceSCP(SOPClass.ComputedRadiographyImageStorage);
            cr.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP mg1 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForPresentation);
            mg1.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP mg2 = new StorageServiceSCP(SOPClass.DigitalMammographyImageStorageForProcessing);
            mg2.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP gsps = new StorageServiceSCP(SOPClass.GrayscaleSoftcopyPresentationStateStorageSOPClass);
            gsps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP sc = new StorageServiceSCP(SOPClass.SecondaryCaptureImageStorage);
            sc.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

			StorageServiceSCP ct = new StorageServiceSCP(SOPClass.CTImageStorage);
			ct.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

			StorageServiceSCP ctEnhanced = new StorageServiceSCP(SOPClass.EnhancedCTImageStorage);
			ctEnhanced.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageServiceSCP dose = new StorageServiceSCP(SOPClass.XRayRadiationDoseSRStorage);
            dose.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            CFindServiceSCP find = new CFindServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelFIND);
            find.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            CMoveServiceSCP move = new CMoveServiceSCP(SOPClass.StudyRootQueryRetrieveInformationModelMOVE);
            move.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            MppsServiceSCP mpps = new MppsServiceSCP();
            mpps.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            StorageCommitServiceSCP commit = new StorageCommitServiceSCP();
            commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            PrintServiceSCP grayscale = new PrintServiceSCP(SOPClass.BasicGrayscalePrintManagementMetaSOPClass);
            grayscale.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            PresentationLUTServiceSCP plut = new PresentationLUTServiceSCP();
            plut.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            AnnotationServiceSCP annotation = new AnnotationServiceSCP();
            annotation.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

            server.AddService(echo);
            server.AddService(dx1);
            server.AddService(dx2);
            server.AddService(cr);
            server.AddService(mg1);
            server.AddService(mg2);
            server.AddService(gsps);
            server.AddService(sc);
			server.AddService(ct);
			server.AddService(ctEnhanced);
            server.AddService(dose);
            server.AddService(find);
            server.AddService(move);
            server.AddService(mpps);
            server.AddService(commit);
            server.AddService(grayscale);
            server.AddService(plut);
            server.AddService(annotation);

            ImageStoredEventHandler imageHandler = new ImageStoredEventHandler(OnImageStored);
            MppsEventHandler mppsHandler = new MppsEventHandler(OnMpps);
            StorageCommitEventHandler commitHandler = new StorageCommitEventHandler(OnStorageCommitRequest);
            foreach (ServiceClass service in server.Services)
            {
                if (service != null)
                {
                    if (service is StorageServiceSCP)
                    {
                        ((StorageServiceSCP)service).ImageStored += imageHandler;
                    }
                    else if (service is PrintServiceSCP)
                    {
                        ((PrintServiceSCP)service).JobPrinted += new PrintJobEventHandler(OnJobPrinted);
                    }
                    else if (service is MppsServiceSCP)
                    {
                        ((MppsServiceSCP)service).MppsCreate += mppsHandler;
                        ((MppsServiceSCP)service).MppsSet += mppsHandler;
                    }
                    else if (service is StorageCommitServiceSCP)
                    {
                        ((StorageCommitServiceSCP)service).StorageCommitRequest += new StorageCommitEventHandler(OnStorageCommitRequest);
                    }
                    else if (service is CFindServiceSCP)
                    {
                        ((CFindServiceSCP)service).Query += new QueryEventHandler(OnQuery);
                    }
                }
            }

            server.Start();

        }

        private void StopServer()
        {
            if (server != null)
            {
                ImageStoredEventHandler handler = new ImageStoredEventHandler(OnImageStored);
                foreach (ServiceClass store in server.Services)
                {
                    if (store != null && store is StorageServiceSCP)
                        ((StorageServiceSCP)store).ImageStored -= handler;
                }
                server.Stop();
                server = null;
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // create delegate used for asynchronous call
            OpenFileDelegate = new DelegateOpenFile(this.OpenFile);

            // Restore the main form location and state from the registry.
            using (RegistryKey regMainFormPos = Registry.CurrentUser.CreateSubKey("Software\\Carestream Health\\DicomEditor\\Main Form Position"))
            {
                Point formLoc = new Point();
                formLoc.X = (int)regMainFormPos.GetValue("Location.X", 100);
                formLoc.Y = (int)regMainFormPos.GetValue("Location.Y", 100);
                this.DesktopLocation = formLoc;

                Size formSize = new Size();
                formSize.Width = (int)regMainFormPos.GetValue("Size.Width", this.Size.Width);
                formSize.Height = (int)regMainFormPos.GetValue("Size.Height", this.Size.Height);
                this.Size = formSize;

                WindowState = (FormWindowState)regMainFormPos.GetValue("Window State", FormWindowState.Normal);
            }

            StartServer();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServer();

            // Save the main form location and state in the registry.
            using (RegistryKey regMainFormPos = Registry.CurrentUser.CreateSubKey("Software\\Carestream Health\\DicomEditor\\Main Form Position"))
            {
                // We don't want to run the next time in a minimized state, so save only
                // "Normal" or "Maximized".
                FormWindowState formState = WindowState;
                if (FormWindowState.Minimized == formState) formState = FormWindowState.Normal;
                regMainFormPos.SetValue("Window State", (int)formState);

                // If the form is not normal, "restore" it so we save that size and location.
                // When we run again, we will remember to maximize, but restoring first allows us to
                // also restore down properly when we run again.
                if (FormWindowState.Normal != WindowState) WindowState = FormWindowState.Normal;

                regMainFormPos.SetValue("Location.X", DesktopLocation.X);
                regMainFormPos.SetValue("Location.Y", DesktopLocation.Y);
                regMainFormPos.SetValue("Size.Width", Size.Width);
                regMainFormPos.SetValue("Size.Height", Size.Height);
            }

            // Save the MRU to the registry.
            mruMenu.SaveToRegistry();
        }

        private void MainForm_MdiChildActivate(object sender, EventArgs e)
        {
            UpdateMenuItems();
        }

        private void UpdateMenuItems()
        {
            if (this.ActiveMdiChild is Browser)
            {
                this.DeIdentifyMenuItem.Enabled = this.SaveMenuItem.Enabled = this.SaveAsMenuItem.Enabled = this.CloseMenuItem.Enabled = true;
                Browser child = (Browser)this.ActiveMdiChild;
                bool exists = (child != null && child.FileName != null && child.FileName.Length > 0);
                this.SaveMenuItem.Enabled = exists;
                if(exists)
                {
                    FileInfo file = new FileInfo(child.FileName.ToUpper());
                    this.ViewMenuItem.Enabled = file.Name == "DICOMDIR";
                }
            }
            else if (this.ActiveMdiChild is LogForm)
            {
                this.DeIdentifyMenuItem.Enabled = false;
                this.SaveMenuItem.Enabled = this.SaveAsMenuItem.Enabled = this.CloseMenuItem.Enabled = true;
            }
            else
            {
                this.SaveMenuItem.Enabled = this.DeIdentifyMenuItem.Enabled = 
                    this.SaveMenuItem.Enabled = this.SaveAsMenuItem.Enabled = this.CloseMenuItem.Enabled = false;
            }
            this.NewMenuItem.Enabled = this.OpenMenuItem.Enabled = true;

            bool found = false;
            foreach (Form child in MdiChildren)
            {
                if (child is LogForm)
                    found = true;
            }
            if (!found && logging != null)
            {
                logging.Dispose();
                logging = null;
            }
            this.LoggingMenuItem.Checked = (!Object.ReferenceEquals(logging, null));
        }

        private void LoggingMenuItem_Click(object sender, EventArgs e)
        {
            ToggleLogging();
        }

        private void ToggleLogging()
        {
            if (logging == null)
            {
                logging = new LogForm();
                logging.MdiParent = this;
                logging.WindowState = FormWindowState.Maximized;
                logging.Show();
            }
            else
            {
                logging.Close();
                logging.Dispose();
                logging = null;
            }
            LoggingMenuItem.Checked = (logging != null);
        }

        private void DeIdentifyMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                Browser child = (Browser)this.ActiveMdiChild;
                if (child != null)
                {
                    child.DeIdentify();
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void Menu_Popup(object sender, EventArgs e)
        {
            UpdateMenuItems();
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            string version = AssemblyName.GetAssemblyName(Assembly.GetAssembly(this.GetType()).Location).Version.ToString();
            string services = string.Empty;

            if (server != null)
            {
                foreach (ServiceClass service in server.Services)
                {
                    if (services.Length > 0)
                        services += "\n";
                    services += Reflection.GetName(typeof(SOPClass), service.SOPClassUId);
                }
                services += "\n";
            }
            MessageBox.Show(String.Format("DicomEditor, version {0}\n\n{1} on port {2}\n\n{3}\nUsage:{4}", version, aetitle, port, services, BatchEditor.Usage), "About");
        }

        private void FindMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild is Browser)
            {
               ((Browser)ActiveMdiChild).Find();
            }
        }

        private void CheckIODMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                Browser child = (Browser)this.ActiveMdiChild;
                if (child != null)
                {
                    if (logging == null)
                    {
                        ToggleLogging();
                    }
                    logging.Activate();
                    bool passed = child.VerifyIOD();
                    //System.Windows.Forms.MessageBox.Show(String.Format("Verify IOD {0}", passed ? "Passed!" : "Failed! See log for details."));
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void ViewMenuItem_Click(object sender, EventArgs e)
        {
            if (sender is MenuItem)
            {
                if (ActiveMdiChild is Browser)
                {
                    DataSetMenuItem.Checked = (((MenuItem)sender).Name == "DataSetMenuItem");
                    DICOMDIRMenuItem.Checked = !DataSetMenuItem.Checked;

                    ((Browser)ActiveMdiChild).DICOMDIR = DICOMDIRMenuItem.Checked;
                }
            }
        }

        private void ToolsMenuItem_Popup(object sender, EventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(".");
            FileInfo[] files = directory.GetFiles("*.commit");
            StorageCommitMenuItem.Enabled = (files.Length > 0);
            PACSModeMenuItem.Checked = pacsmode;
        }

        private void OnStorageCommitRequest(object sender, StorageCommitEventArgs e)
        {
            if (e.Type == StorageCommitEventType.Request)
            {
                string transaction = (string)e.DataSet[t.TransactionUID].Value;
                e.DataSet.Write(String.Format("{0}.commit", transaction));
            }
        }

        public void OnQuery(object sender, QueryEventArgs args)
        {
            DicomDir dir = new DicomDir(".");

            DataSet query = args.Query;

            string level = (string)query[t.QueryRetrieveLevel].Value;
            bool found = false;
            string value = String.Empty;
            switch (level)
            {
                case "PATIENT":
                   value = (string)query[t.PatientID].Value;
                    foreach (Patient patient in dir.Patients)
                    {
                        if (patient.PatientID == value)
                        {
                            found = true;
                            args.Records.Add(patient.Elements);
                        }
                        if (found) break;
                    }
                    break;
                case "STUDY":
                    //value = (string)query[t.StudyInstanceUID].Value;
                    value = (string)query[t.PatientID].Value;
                    foreach (Patient patient in dir.Patients)
                    {
                        if (patient.PatientID == value)
                        {
                            found = true;
                            foreach (Study study in patient)
                            {
                                args.Records.Add(study.Elements);
                            }
                        }
                        if (found) break;
                    }
                    break;
                case "SERIES":
                    //value = (string)query[t.SeriesInstanceUID].Value;
                    value = (string)query[t.StudyInstanceUID].Value;
                    foreach (Patient patient in dir.Patients)
                    {
                        foreach (Study study in patient)
                        {
                            if (study.StudyInstanceUID == value)
                            {
                                found = true;
                                foreach (Series series in study)
                                {
                                    args.Records.Add(series.Elements);
                                }
                            }
                            if (found) break;
                        }
                        if (found) break;
                    }
                    break;
                case "IMAGE":
                    //value = (string)query[t.SOPInstanceUID].Value;
                    value = (string)query[t.SeriesInstanceUID].Value;
                    foreach (Patient patient in dir.Patients)
                    {
                        foreach (Study study in patient)
                        {
                            foreach(Series series in study)
                            {
                                if (series.SeriesInstanceUID == value)
                                {
                                    found = true;
                                    foreach (EK.Capture.Dicom.DicomToolKit.Image image in series)
                                    {
                                        image.Elements.Add(t.SOPClassUID, image.Elements[t.ReferencedSOPClassUIDinFile].Value);
                                        image.Elements.Add(t.SOPInstanceUID, image.Elements[t.ReferencedSOPInstanceUIDinFile].Value);

                                        // added to support prior retrieval via Carestream Modality. 
                                        if (query.ValueExists(t.StudyInstanceUID))
                                            image.Elements.Add(t.StudyInstanceUID, study.StudyInstanceUID);
                                        if (query.ValueExists(t.SeriesInstanceUID))
                                            image.Elements.Add(t.SeriesInstanceUID, series.SeriesInstanceUID);

                                        args.Records.Add(image.Elements);
                                    }
                                }
                                if (found) break;
                            }
                            if (found) break;
                        }
                        if (found) break;
                    }
                    break;
            }
        }

        private void PACSModeMenuItemClick(object sender, EventArgs e)
        {
            pacsmode = !pacsmode;
            PACSModeMenuItem.Checked = pacsmode;
        }

        private void StorageCommitMenuItem_Click(object sender, EventArgs e)
        {
			server.Hosts.Add("127.0.0.1", new ApplicationEntity("CONSOLE", IPAddress.Parse("127.0.0.1"), 5040));
            StorageCommitForm dialog = new StorageCommitForm(server.Hosts);
            dialog.ShowDialog();
        }
    }
}
