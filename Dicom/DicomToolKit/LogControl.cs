using System;
using System.Windows.Forms;

namespace EK.Capture.Dicom.DicomToolKit
{
    public partial class LogControl : UserControl
    {
        private int maxlines = 0;

        public LogControl()
        {
            InitializeComponent();
        }

        private void LogControl_Load(object sender, EventArgs e)
        {
            Log.LogMessage += new LoggingEventHandler(OnLogMessage);
            Log.Start(LogLevel.Verbose);
            ScrollBar.Maximum = ScrollBar.Minimum = ScrollBar.Value = 0;
        }

        private void SetText()
        {
            LogTextBox.Lines = Log.GetLines(ScrollBar.Value, maxlines);
        }

        private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            SetText();
        }

        private int MaxLines
        {
            get
            {
                return maxlines;
            }
        }

        public string GetText()
        {
            return Log.GetText();
        }

        public void OnLogMessage(object sender, LoggingEventArgs e)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new LoggingEventHandler(OnLogMessage), new object[] { sender, e });
            }
            else
            {
                ScrollBar.Maximum = (int)Log.Count;
                SetText();
            }
        }

        private void LogTextBox_SizeChanged(object sender, EventArgs e)
        {
            // get the height of 1 line averaged over 10 lines
            float height = TextRenderer.MeasureText("1y\n2y\n3y\n4y\n5y\n6y\n7y\n8y\n9y\n10y", LogTextBox.Font).Height / 10.0f;
            maxlines = (int)((1.0f * LogTextBox.ClientRectangle.Height) / height);
            ScrollBar.LargeChange = maxlines;
        }

        private void ClearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Log.Stop();
            Log.Start(LogLevel.Verbose);
            SetText();
        }
    }
}
