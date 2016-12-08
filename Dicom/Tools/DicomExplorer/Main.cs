using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomExplorer
{
    public partial class Main : Form
    {
        private LogForm logging = null;
        private Settings settings = new Settings("DicomExplorer");

        public Main()
        {
            InitializeComponent();
            NewExplorer();
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            NewExplorer();
        }

        private void NewExplorer()
        {
            this.Cursor = Cursors.WaitCursor;

            string path = settings["Path"];

            Explorer child = new Explorer(path);
            child.MdiParent = this;
            child.Show();

            this.Cursor = Cursors.Default;
        }

        private void LoggingToolStripMenuItem_Click(object sender, EventArgs e)
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
            LoggingToolStripMenuItem.Checked = (logging != null);
        }

        private void SelectColumnsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveMdiChild is Explorer)
            {
                Explorer child = (Explorer)this.ActiveMdiChild;
                List<string> mapping = child.Mapping;

                TagForm dialog = new TagForm(mapping);
                DialogResult result = dialog.ShowDialog();
                if (DialogResult.OK == result)
                {
                    child.Mapping = dialog.Selection;
                }
            }
        }

     }
}
