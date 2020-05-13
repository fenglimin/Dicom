using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Windows.Forms;
using DicomViewer;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest.CustomControl;
using ExtendedListTest.Image;
using ExtendedListTest.Service;

namespace ExtendedListTest
{
	public partial class TinyPACS : Form, IDicomServiceWorkerUser
	{
		public delegate void DicomElementsReceivedHandler(ReceivedDicomElements receivedDicomElements);
        public delegate bool CallBackReturnBoolHandler();
        public delegate string CallBackReturnStringHandler();
	    public delegate void ShowMessageHandler(string message, bool isError, bool isShowMessageBox);
		public delegate void DicomDirSavedHandler(ReceivedDicomElements receivedDicomElements, string dicomDir);
		

	    private DicomServiceWorker dicomServiceWorker;
        //private CheckBox cbOpenWhenReceived = new CheckBox();

	    private bool saveWhenReceived = false;
	    private bool openWhenReceived = false;
	    private string selectedDicomDir = string.Empty;

	    private Size prevSize;

		public TinyPACS()
		{
			InitializeComponent();

		    prevSize = Size;

            tcDicomFiles.DrawMode = TabDrawMode.OwnerDrawFixed;
            tcDicomFiles.DrawItem += tcDicomFiles_DrawItem;

            //cbOpenWhenReceived.Text = "Open Image When Received";
            //cbOpenWhenReceived.Checked = true;

            //var controlHost = new ToolStripControlHost(cbOpenWhenReceived);
            //toolStrip1.Items.Add(new ToolStripSeparator());
            //toolStrip1.Items.Add(controlHost);

			//this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		}

	    //private void SetTabHeader(TabPage page, Color color)
        //{
        //    TabColors[page] = color;
        //    tcDicomFiles.Invalidate();
        //}

		private void Form2_Load(object sender, EventArgs e)
		{
		    StartService();

		    LoadAllDicomDir();
		}

		private void LoadAllDicomDir()
		{
			var oldSelect = toolStripComboBox_DicomDir.Text;
			if (string.IsNullOrEmpty(oldSelect))
				oldSelect = "Main";

			toolStripComboBox_DicomDir.Items.Clear();
			foreach (var dicomDir in dicomServiceWorker.GetAllDicomDir())
			{
				toolStripComboBox_DicomDir.Items.Add(dicomDir);
			}

			toolStripComboBox_DicomDir.Text = oldSelect;
		}

        private void tcDicomFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Find tab control uncer mouse
                var mouseRect = new Rectangle(e.X, e.Y, 1, 1);
                for (var i = 0; i < tcDicomFiles.TabCount; i++)
                {
                    if (tcDicomFiles.GetTabRect(i).IntersectsWith(mouseRect))
                    {
                        tcDicomFiles.SelectedIndex = i;
                        break;
                    }
                }

                var receivedDicomElements = GetReceivedDicomElementsByTabPage(tcDicomFiles.SelectedTab);
                if (receivedDicomElements != null)
				{
					receivedDicomElements.ImageStatus = ImageMemoryStatus.CachedInMemory;
					RefreshControl();
				}
				
				var oldSelectedIndex = tcDicomFiles.SelectedIndex;
                tcDicomFiles.TabPages.Remove(tcDicomFiles.SelectedTab);


                if (oldSelectedIndex >= tcDicomFiles.TabCount)
                    oldSelectedIndex--;
                if (oldSelectedIndex < 0)
                    oldSelectedIndex = 0;

                if (tcDicomFiles.TabCount > 0)
                    tcDicomFiles.SelectTab(oldSelectedIndex);    

                GC.Collect();
            }
        }

        private TabPage GetTabPageByReceivedDicomElements(ReceivedDicomElements receivedDicomElements)
        {
            return tcDicomFiles.TabPages.Cast<TabPage>().FirstOrDefault(tabPage => GetReceivedDicomElementsByTabPage(tabPage) == receivedDicomElements);
        }

	    private ReceivedDicomElements GetReceivedDicomElementsByTabPage(TabPage tabPage)
	    {
	        if (tabPage == null)
	            return null;

            var customCtrl = tabPage.Controls[0] as IElementsBase;
            return (customCtrl != null) ? customCtrl.GetReceivedDicomElements() : null;
        }

        private void LoadFile(string fileName)
	    {
            try
            {
                var elements = OtherImageFormats.Read(fileName);
				var isDicomDir = elements.GetSafeStringValue(t.MediaStorageSOPClassUID) ==
	                             SOPClass.MediaStorageDirectoryStorage;

                var receivedDicom = new ReceivedDicomElements
                {
                    CallingAeTitle = "Localhost",
                    FileName = fileName,
                    Elements = elements,
                    ImageSource = isDicomDir? ImageSource.LocalDicomDir : ImageSource.LocalDicomFile,
                    SavedToDisk = true
                };

                ShowElements(receivedDicom);

                ShowMessage(fileName + " opened successfully!", false, false);
            }
            catch (Exception ex)
            {
                ShowMessage(fileName + " opened failed! " + ex.Message, true, true);
            }
	    }

	    public string GetActiveStorage()
	    {
            if (InvokeRequired)
            {
                Invoke(new CallBackReturnStringHandler(GetActiveStorage));
            }
            else
            {
                selectedDicomDir = toolStripComboBox_DicomDir.Text;
            }

            return selectedDicomDir;
	    }

	    public bool OpenWhenReceived()
		{
			return toolStripButton_OpenWhenReceived.Checked;
		}

	    public bool SaveWhenReceived()
	    {
	        if (InvokeRequired)
	        {
                Invoke(new CallBackReturnBoolHandler(SaveWhenReceived));
	        }
	        else
	        {
	            saveWhenReceived = toolStripButton_SaveWhenReceived.Checked;
	        }

            return saveWhenReceived;
	    }

	    public void ShowElements(ReceivedDicomElements receivedDicomElements)
	    {
	        var tabPage = GetTabPageByReceivedDicomElements(receivedDicomElements);
	        if (tabPage != null)
	        {
	            tcDicomFiles.SelectTab(tabPage);
	            return;
	        }

		    var modality = receivedDicomElements.Elements.GetSafeStringValue(t.Modality).ToUpper();

		    var title = receivedDicomElements.CallingAeTitle + " : " + receivedDicomElements.ImageSource + "  ";
		    if (string.IsNullOrEmpty(receivedDicomElements.FileName))
			    title += receivedDicomElements.ReceivedDateTime.ToString("HH:mm:ss.fff");
		    else
			    title += receivedDicomElements.FileName;

			tabPage = new TabPage { Text = title };
            UserControl userControl;

		    if (receivedDicomElements.ImageSource == ImageSource.LocalDicomDir)
		    {
				userControl = new ucDircomDir(receivedDicomElements, this) { Dock = DockStyle.Fill };
		    }
		    else
		    {
				if (modality == "SR")
				{
					userControl = new ucDoseReport(receivedDicomElements) { Dock = DockStyle.Fill };
				}
				else if (modality == "PR" || receivedDicomElements.ImageSource == ImageSource.StorageCommitment || receivedDicomElements.ImageSource == ImageSource.Mpps)
				{
					userControl = new ucTag(receivedDicomElements) { Dock = DockStyle.Fill };
				}
				else
				{
					userControl = new ucTagAndImage(receivedDicomElements) { Dock = DockStyle.Fill };
				}    
		    }


            tabPage.Controls.Add(userControl);
            tcDicomFiles.Controls.Add(tabPage);
            tcDicomFiles.SelectTab(tabPage);

			receivedDicomElements.ImageStatus = ImageMemoryStatus.OpenedInWindow;
			RefreshControl();

	        var elementBase = userControl as IElementsBase;
	        if (string.IsNullOrEmpty(elementBase.GetLastError()))
	            return;

            throw new Exception(elementBase.GetLastError());
	    }

		public void OnDicomElementsReceived(ReceivedDicomElements receivedDicomElements)
		{
			if (InvokeRequired)
			{
				Invoke(new DicomElementsReceivedHandler(OnDicomElementsReceived), new object[] { receivedDicomElements });
			}
			else
			{
				try
				{
					if (receivedDicomElements.ImageStatus == ImageMemoryStatus.OpenedInWindow)
						ShowElements(receivedDicomElements);
					else
						RefreshControl();
				}
				catch (Exception ex)
				{
					MessageBox.Show(Logging.Log(ex));
				}
			}
		}

		public void OnDicomDirSaved(ReceivedDicomElements receivedDicomElements, string dicomDir)
		{
			if (InvokeRequired)
			{
				Invoke(new DicomDirSavedHandler(OnDicomDirSaved), new object[] { receivedDicomElements, dicomDir });
			}
			else
			{
				try
				{
					LoadAllDicomDir();
				}
				catch (Exception ex)
				{
					MessageBox.Show(Logging.Log(ex));
				}
			}
		}

		public void ShowMessage(string message, bool hasError, bool isShowMessageBox)
	    {
            if (InvokeRequired)
            {
                Invoke(new ShowMessageHandler(ShowMessage), new object[] { message, hasError, isShowMessageBox });
            }
            else
            {
                try
                {
                    var temp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "   " + message;
                    toolStripComboBox_Message.Items.Insert(0, temp);
                    toolStripComboBox_Message.SelectedIndex = 0;
                    toolStripComboBox_Message.ForeColor = hasError ? Color.Red : Color.Black;

                    if (isShowMessageBox)
                        MessageBox.Show(message);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(Logging.Log(ex));
                }
            }
	    }

	    private void RefreshControl()
		{
		    var cachedCount = dicomServiceWorker.CachedCount();
            toolStripButton_Cached.ForeColor = (cachedCount == 0)? Color.Black : Color.Red;
			toolStripButton_Cached.Text = string.Format("Received Image ({0})", dicomServiceWorker.CachedCount());
		    toolStripButton_Cached.Enabled = dicomServiceWorker.CachedCount() > 0;

		    var receivedDicomElemens = GetReceivedDicomElementsByTabPage(tcDicomFiles.SelectedTab);
		    if (receivedDicomElemens == null)
		    {
		        toolStripButton_Save.Enabled = false;
		        toolStripButton_SaveAs.Enabled = false;
		        toolStripButton_SaveDicomDir.Enabled = false;
		        return;
		    }

		    toolStripButton_SaveDicomDir.Enabled = !receivedDicomElemens.IsSavedToDicomDir(toolStripComboBox_DicomDir.Text) && receivedDicomElemens.ImageSource != ImageSource.LocalDicomDir;
            toolStripButton_Save.Enabled = !receivedDicomElemens.SavedToDisk;
            toolStripButton_SaveAs.Enabled = !receivedDicomElemens.SavedToDisk; ;

		}

		private void toolStripButton_Open_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                RestoreDirectory = true,
                AddExtension = true,
                DefaultExt = "dcm",
				Filter = "Dicom Files (*.dcm)|*.dcm|All files|*.*"
            };

		    if (dialog.ShowDialog() == DialogResult.OK)
            {
                LoadFile(dialog.FileName);
                RefreshControl();
            }
        }

        private void tcDicomFiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            //e.DrawBackground();
            using (Brush br = new SolidBrush(Color.Azure))
            {
                e.Graphics.FillRectangle(br, e.Bounds);

                var font = new Font(e.Font.FontFamily, e.Font.Size - (float)0.3);
                var sz = e.Graphics.MeasureString(tcDicomFiles.TabPages[e.Index].Text, font);
                sz.Width -= 20;

                var brush = Brushes.Black;
                if (tcDicomFiles.SelectedIndex == e.Index)
                    brush = Brushes.Red;
                e.Graphics.DrawString(tcDicomFiles.TabPages[e.Index].Text, font, brush, e.Bounds.Left + (e.Bounds.Width - sz.Width) / 2 - 10, e.Bounds.Top + (e.Bounds.Height - sz.Height) / 2 + 2);

                Rectangle rect = e.Bounds;
                rect.Offset(0, 1);
                rect.Inflate(0, -1);
                e.Graphics.DrawRectangle(Pens.DarkGray, rect);
                e.DrawFocusRectangle();
            }
        }

	    private bool StartService()
	    {
	        dicomServiceWorker = new DicomServiceWorker(this);
	        return dicomServiceWorker.StartService();
	    }

	    public string GetAtTitle()
	    {
	        return "TinyPACS";
	    }

	    public int GetPort()
	    {
	        return 12345;
	    }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (dicomServiceWorker != null)
                dicomServiceWorker.StopService();
        }

        private void toolStripButton_Cached_Click(object sender, EventArgs e)
        {
            var cachedForm = new CachedForm(this, dicomServiceWorker);
            cachedForm.ShowDialog();
        }

		private void toolStripButton_Test_Click(object sender, EventArgs e)
		{
		    var i = 0;
		    toolStripButton_Test.Checked = !toolStripButton_Test.Checked;
		    //Form frmShowPic = new Form();
		    //frmShowPic.Width = 234;
		    //frmShowPic.Height = 332;
		    //frmShowPic.MinimizeBox = false;
		    //frmShowPic.MaximizeBox = false;
		    //frmShowPic.ShowIcon = false;
		    //frmShowPic.StartPosition = FormStartPosition.CenterScreen;

		    //frmShowPic.Text = "Show Picture";

		    ///* add panel */
		    //Panel panPic = new Panel();
		    //panPic.AutoSize = false;
		    //panPic.AutoScroll = true;
		    //panPic.Dock = DockStyle.Fill;

		    ///* add picture box */
		    //PictureBox pbPic = new PictureBox();
		    //pbPic.SizeMode = PictureBoxSizeMode.AutoSize;
		    //pbPic.Location = new Point(0, 0);

		    //panPic.Controls.Add(pbPic);
		    //frmShowPic.Controls.Add(panPic);

		    ///* define image */
		    //pbPic.ImageLocation = @"d:\Untitled1.png";

		    //frmShowPic.ShowDialog();
		}

        private void toolStripButton_Save_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var receivedDicomElements = GetReceivedDicomElementsByTabPage(tcDicomFiles.SelectedTab);
                if (receivedDicomElements != null)
                {
                    receivedDicomElements.Elements.Write(saveFileDialog1.FileName);
                    tcDicomFiles.SelectedTab.Text = "Local  " + saveFileDialog1.FileName;

                    receivedDicomElements.OnDiskSaved(saveFileDialog1.FileName);
                    RefreshControl();

                    ShowMessage(saveFileDialog1.FileName + "   saved successfully!", false, false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(saveFileDialog1.FileName + "   saved failed! " + ex.Message, false, false);
            }
            
        }

        private void toolStripButton_SaveAs_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton_OpenWhenReceived_Click(object sender, EventArgs e)
        {
            toolStripButton_OpenWhenReceived.Checked = !toolStripButton_OpenWhenReceived.Checked;
        }

        private void toolStripButton_SaveWhenReceived_Click(object sender, EventArgs e)
        {
            toolStripButton_SaveWhenReceived.Checked = !toolStripButton_SaveWhenReceived.Checked;
        }

        private void tcDicomFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshControl();
        }

        private void toolStripButton_SaveDicomDir_Click(object sender, EventArgs e)
        {
            dicomServiceWorker.SaveToDicomDir(GetReceivedDicomElementsByTabPage(tcDicomFiles.SelectedTab), toolStripComboBox_DicomDir.Text);
			LoadAllDicomDir();
            RefreshControl();
        }

        private void toolStripComboBox_DicomDir_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(toolStripComboBox_DicomDir.Text))
            {
                 MessageBox.Show("Empty dicom dir name is not allowed!");
                toolStripComboBox_DicomDir.Text = selectedDicomDir;
            }
            else
            {
                const string notAllowed = "/\\\":?*<>|";
                if (toolStripComboBox_DicomDir.Text.IndexOfAny(notAllowed.ToCharArray()) != -1)
                {
                    MessageBox.Show(notAllowed + " are not allowed!");
                    toolStripComboBox_DicomDir.Text = selectedDicomDir;
                }   
            }

            RefreshControl();

        }

	    private void toolStripComboBox_DicomDir_KeyDown(object sender, KeyEventArgs e)
        {
            selectedDicomDir = toolStripComboBox_DicomDir.Text;
        }


        private void TinyPACS_SizeChanged(object sender, EventArgs e)
        {
			//toolStrip1.SuspendLayout();

			//var widthChanged = Size.Width - prevSize.Width;
			//toolStripComboBox_Message.Size = new Size(toolStripComboBox_Message.Width+widthChanged, toolStripComboBox_Message.Height);
			//prevSize = Size;

			//toolStrip1.ResumeLayout(false);
			//toolStrip1.PerformLayout();
        }

		private void toolStripButton_OpenDicomDir_Click(object sender, EventArgs e)
		{
			var dicomDirPath = Path.Combine(dicomServiceWorker.StorageRootPath, toolStripComboBox_DicomDir.Text);
			if (!Directory.Exists(dicomDirPath))
			{
				Directory.CreateDirectory(dicomDirPath);
				var dicomDir = new DicomDir(dicomDirPath);
				dicomDir.Save();
			}

			LoadFile(Path.Combine(dicomDirPath, "DICOMDIR"));
			LoadAllDicomDir();
			RefreshControl();
		}
	}
}
