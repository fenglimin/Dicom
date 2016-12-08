using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomViewer
{
    public partial class MainForm : Form
    {
        private FileSystemWatcher watcher = null;
        private delegate bool DelegateOpenFile(String s);           // type
        private DelegateOpenFile openFileDelegate;                  // instance
        private bool loaded;
        private Server server;
        private const string AeTitle = "DICOMVIEWER";
        private const int Port = 1234;
        private LogForm logging;
        private bool openingFromMru;

        public MainForm(string[] args)
        {
            InitializeComponent();

            mruMenu = new MruMenuInline(menuFileRecentFile, OnMenuFileMru, "Software\\Carestream Health\\DicomViewer\\MRU");

            foreach (var filename in args)
            {
                OpenFile(filename);
            }
        }

        private void NewMenuItem_Click(object sender, EventArgs e)
        {
            Viewer child = new Viewer();
            child.MdiParent = this;
            child.Show();
        }

        private void OnMenuFileMru(int number, String filename)
        {
            bool fileOpened = true;
            Application.DoEvents();
            openingFromMru = true;

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

            openingFromMru = false;
            if (fileOpened) mruMenu.SetFirstFile(number);
            else mruMenu.RemoveFile(number);
        }

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.ActiveMdiChild is LogForm)
                {
                    logging.Close();
                    logging.Dispose();
                    logging = null;
                }
                else
                {
                    using (Viewer child = (Viewer)this.ActiveMdiChild)
                    {
                        child.Close();
                    }
                    System.GC.Collect();
                }
            }
            catch
            {
            }
        }

        private void OpenMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = Viewer.Filter;
            // try and use the last file type used
            Settings settings = new Settings("DicomViewer");
            if (settings["DefaultExt"] != null)
            {
                string extension = settings["DefaultExt"];
                //"Dicom Files (*.dcm)|*.dcm|Raw Files (*.raw)|*.raw|Mask Files (*.mask)|*.mask|Bit Files (*.bit)|*.bit|PGM Files (*.pgm)|*.pgm|Bitmap Files (*.bmp)|*.bmp|Jpeg Files (*.jpg)|*.jpg|Png Files (*.png)|*.png|Gif Files (*.gif)|*.gif|Tiff Files (*.tiff;*.tif)|*.tiff;*.tif|Emf Files (*.emf)|*.emf|Exif Files (*.exif)|*.exif|Wmf Files (*.wmf)|*.wmf|Filmboxes (*.xml)|*.xml|All Image Files |*.dcm;*.raw;*.mask;*.pgm;*.bmp;*.jpg;*.png;*.gif;*.tiff;*.tif;*.emf;*.exif;*.wmf;*.xml|All files|*.*"
                string[] choices = dialog.Filter.Split("|".ToCharArray());
                int n;
                // try and find the extension in the Filter
                for(n = 0; n < choices.Length; n++)
                {
                    if (choices[n] == extension)
                        break;
                }
                // if found, the FilterIndex is 1 based.
                if (n < choices.Length)
                {
                    n = (n + 1) / 2;
                    dialog.FilterIndex = n;
                }
            }
            dialog.Multiselect = true;
            dialog.ShowDialog();
            if (dialog.FileNames.Length > 0)
            {
                // save tha last file type opened
                if (dialog.FileNames.Length == 1)
                {
                    // save it as *.something because that is how the formatted
                    FileInfo info = new FileInfo(dialog.FileName);
                    settings["DefaultExt"] = "*"+info.Extension.ToLower();
                }
                foreach (string filename in dialog.FileNames)
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
            FileInfo info = new FileInfo(filename);
            if (info.Extension.ToLower() == ".uint16")
            {
                if (ActiveMdiChild is Viewer)
                {
                    Viewer child = (Viewer)this.ActiveMdiChild;
                    child.ApplyLut(filename);
                }
            }
            else if (info.Extension.ToLower() == ".xml")
            {
                if (ActiveMdiChild is Viewer)
                {
                    Viewer child = (Viewer)this.ActiveMdiChild;
                    child.ApplyBsm(filename);
                }
            }
            else
            {
                Settings settings = new Settings("DicomViewer");
                settings["filename"] = filename;
                Viewer child = new Viewer(filename);
                child.MdiParent = this;
                child.Show();
            }
            }
            catch
            {
                fileOpened = false;
            }

            if (fileOpened && !openingFromMru)
            {
                mruMenu.AddFile(filename);
            }

            return fileOpened;
        }

        private void NewViewer(EK.Capture.Dicom.DicomToolKit.DataSet dicom)
        {
            Viewer child = new Viewer(dicom);
            child.MdiParent = this;
            child.WindowState = FormWindowState.Maximized;
            child.Show();
        }

        private void NewViewer(FilmBox page)
        {
            Viewer child = new Viewer(page);
            child.MdiParent = this;
            child.WindowState = FormWindowState.Maximized;
            child.Show();
        }

        private bool Loaded
        {
            get
            {
                return loaded;
            }
            set
            {
                loaded = value;
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

                        this.BeginInvoke(openFileDelegate, new Object[] { s });
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

        private void MainForm_Load(object sender, EventArgs e)
        {
            // create delegate used for asynchronous call
            openFileDelegate = new DelegateOpenFile(this.OpenFile);

            watcher = new FileSystemWatcher(@"c:\", "*.*");
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName;

            watcher.Created += new FileSystemEventHandler(OnCreated);

            watcher.EnableRaisingEvents = false;
            Loaded = true;

            StartServer();
        }

        private void OnCreated(object source, FileSystemEventArgs e)
        {
            if (this.InvokeRequired)
            {
                // we should not call Invoke or access the form until the form is loaded.
                if (this.Loaded)
                {
                    this.Invoke(new FileSystemEventHandler(OnCreated), new object[] { source, e });
                    System.Threading.Thread.Sleep(1000);
                }
            }
            else
            {
                try
                {
                    Viewer child = new Viewer(e.FullPath);
                    child.MdiParent = this;
                    child.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            mruMenu.SaveToRegistry();
            StopServer();
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

        private bool TestServer()
        {
            bool result = false;
            using (Association association = new Association())
            {
                VerificationServiceSCU echo = new VerificationServiceSCU();
                echo.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);

                association.AddService(echo);
                if (association.Open(AeTitle, IPAddress.Parse("127.0.0.1"), Port))
                {
                    result = true;
                }
                echo = null;
            }
            return result;
        }

        private delegate void SetStatusDelegate(string text);
        public void SetStatus(string text)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new SetStatusDelegate(SetStatus), new object[] { text });
            }
            StatusStripStatusLabel.Text = text;
            Application.DoEvents();
        }

        private void StartServer()
        {
            if (TestServer())
            {
                SetStatus("SCP is already running in another instance.");
                return;
            }

            server = new Server(AeTitle, Port);

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
            server.AddService(grayscale);
            server.AddService(plut);
            server.AddService(annotation);

            ImageStoredEventHandler imageHandler = new ImageStoredEventHandler(OnImageStored);
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
                        ((PrintServiceSCP)service).JobPrinted += new PrintJobEventHandler(OnPagePrinted);
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
                foreach (ServiceClass service in server.Services)
                {
                    if (service != null && service is StorageServiceSCP)
                    {
                        ((StorageServiceSCP)service).ImageStored -= handler;
                    }
                    if (service != null && service is PrintServiceSCP)
                    {
                        ((PrintServiceSCP)service).JobPrinted -= new PrintJobEventHandler(OnPagePrinted);
                    }
                }
                server.Stop();
                server = null;
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
                //try
                //{
                //    if (File.Exists("delete.me"))
                //    {
                //        ushort columns = (ushort)e.DataSet[t.Columns].Value;
                //        ushort rows = (ushort)e.DataSet[t.Rows].Value;
                //        string filename = String.Format("{0}.{1}.{2}.raw", accession++, columns, rows);
                //        using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                //        {
                //            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Little);
                //            writer.WriteWords((short[])e.DataSet[t.PixelData].Value);
                //        }
                //        GC.Collect();
                //        return;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(Logging.Log(ex));
                //}
                try
                {
                    NewViewer(e.DataSet);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
        }

        private void OnPagePrinted(object sender, PrintJobEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new PrintJobEventHandler(OnPagePrinted), new object[] { sender, e });
            }
            else
            {

                FilmBox page = e.Session.FilmBoxes[0];
                //try
                //{
                //    if (File.Exists("delete.me"))
                //    {
                //        int temp = accession++;
                //        page.Dicom.Write(String.Format("filmbox{0}.dcm", temp));
                //        foreach (ImageBox image in page.ImageBoxes)
                //        {
                //            ushort position = (ushort)image.Dicom[t.ImageBoxPosition].Value;
                //            image.Dicom.Write(String.Format("imagebox{0}.{1}.dcm", temp, position));
                //        }
                //        return;
                //    }
                //}
                //catch (Exception ex)
                //{
                //    MessageBox.Show(Logging.Log(ex));
                //}
                try
                {
                    NewViewer(page);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
        }

        private void SaveAsMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        public void SaveAs()
        {
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.RestoreDirectory = true;
                dialog.AddExtension = true;

                if (ActiveMdiChild is Viewer)
                {
                    Viewer child = (Viewer)this.ActiveMdiChild;

                    dialog.DefaultExt = "dcm";
                    dialog.Filter = Viewer.Filter;
                    if (child.FileName == null || child.FileName.Length == 0)
                    {
                        DataSet dicom = child.Dicom;
                        dialog.FileName = (dicom.Contains(t.SOPInstanceUID)) ? (string)dicom[t.SOPInstanceUID].Value + ".dcm" : "untitled.dcm";
                    }
                }
                else if (ActiveMdiChild is LogForm)
                {
                    dialog.DefaultExt = "log";
                    dialog.Filter = "All files|*.*|Log Files (*.log)|*.log";
                    dialog.FileName = String.Format("{0}", DateTime.Now.ToString("yyyyMMddHHmmss"));
                }
                else
                    return;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    if (ActiveMdiChild is Viewer)
                    {
                        Viewer child = (Viewer)this.ActiveMdiChild;
                        child.SaveAs(dialog.FileName);
                    }
                    else if (ActiveMdiChild is LogForm)
                    {
                        LogForm child = (LogForm)this.ActiveMdiChild;
                        child.Save(dialog.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
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
            MessageBox.Show(String.Format("DicomViewer, version {0}\n\n{1} on port {2}\n\n{3}\nUsage:{4}", version, AeTitle, Port, services, BatchProcessor.Usage), "About");
        }

        private void CompareMenuItem_Click(object sender, EventArgs e)
        {
            string first = null;
            string second = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "dcm";
            dialog.Filter = "Dicom Files (*.dcm)|*.dcm|All files|*.*";
            dialog.Multiselect = true;
            dialog.Title = "Select one or both files to compare.";
            dialog.ShowDialog();

            if (dialog.FileNames.Length > 0)
            {
                first = dialog.FileNames[0];
                if (dialog.FileNames.Length > 1)
                {
                    second = dialog.FileNames[1];
                }
                if (second == null)
                {
                    dialog.Multiselect = false;
                    dialog.Title = "Select the second file to compare.";
                    dialog.ShowDialog();

                    if (dialog.FileNames.Length > 0)
                    {
                        second = dialog.FileNames[0];
                    }
                }
            }
            if (first != null & second != null)
            {
                DataSet left = new DataSet();
                left.Read(first);
                DataSet right = new DataSet();
                right.Read(second);

                Compare(left, right);
            }
        }

        private unsafe void Compare(DataSet left, DataSet right)
        {
            ImageAttributes first = Utilities.GetAttributes(left.Elements);
            ImageAttributes second = Utilities.GetAttributes(right.Elements);

            if (first.width == second.width && first.height == second.height)
            {
                ImageAttributes difference = first;
                difference.buffer = new ushort[first.width * first.height];
                fixed (UInt16* pleft = (UInt16[])first.buffer, pright = (UInt16[])second.buffer, presult = (UInt16[])difference.buffer)
                {
                    int o1 = 0, o2 = 0;
                    for (int r = 0; r < first.height; r++)
                    {
                        for (int c = 0; c < first.width; c++)
                        {
                            presult[r*first.width+c] = (ushort)(pleft[o1+c] - pright[o2+c]);
                        }
                        o1 += first.stride;
                        o2 += second.stride;
                    }
                }
            }
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
    }
}