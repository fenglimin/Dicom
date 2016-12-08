using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EK.Capture.Dicom.DicomToolKit
{
    public partial class LogForm : Form
    {
        public LogLevel LogLevel
        {
            get
            {
                return Logging.LogLevel;
            }
            set
            {
                Logging.LogLevel = value;
            }
        }

        public LogForm()
        {
            InitializeComponent();
        }

        private void Log_SizeChanged(object sender, EventArgs e)
        {
            LogControl.Size = this.ClientSize;
        }

        public void Save(string filename)
        {
            FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            string text = LogControl.GetText();
            stream.Write(Encoding.ASCII.GetBytes(text), 0, text.Length);
        }
    }
}