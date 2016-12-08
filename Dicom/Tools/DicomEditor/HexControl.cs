using System;
using System.Drawing;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    public partial class HexControl : UserControl
    {

                    
        private const int lines = 8;
        private const string sample = " 00000000 : FF FF FF FF FF FF FF FF - FF FF FF FF FF FF FF FF  ................ ";
        private byte[] bytes = null;

        public HexControl()
        {
            InitializeComponent();
            SetDimensions();
        }

        public byte[] Bytes
        {
            set
            {
                bytes = value;
                ScrollBar.Minimum = ScrollBar.Value = 0;
                ScrollBar.Maximum = bytes.Length / 16 + 1;
                SetText();
            }
        }

        private void SetText()
        {
            if (bytes != null)
            {
                int position = ScrollBar.Value;
                TextBox.Text = DicomObject.ToText(bytes, 16 * position, 16 * 8);
            }
        }

        private void SetDimensions()
        {
            String text = String.Empty;
            Graphics graphics = this.CreateGraphics();
            graphics.PageUnit = GraphicsUnit.Pixel;

            // get the size of the whole block of text
            for(int n = 0; n < lines; n++)
                text += sample + "\r\n";
            SizeF size = graphics.MeasureString(text, TextBox.Font);

            this.Size = new Size((int)size.Width + ScrollBar.Width + 20, (int)size.Height + 20);
            TextBox.Size = new Size((int)size.Width + 20, (int)size.Height + 20);
            ScrollBar.LargeChange = lines;
        }

        private void ScrollBar_ValueChanged(object sender, EventArgs e)
        {
            SetText();
        }

        private void HexControl_Leave(object sender, EventArgs e)
        {
            this.Visible = false;
        }

        private void HexControl_VisibleChanged(object sender, EventArgs e)
        {
            if (this.Visible)
            {
                SetDimensions();
                BringToFront();
                Focus();
                TextBox.Select(0, 0);
            }
            else
            {
                SendToBack();
            }
        }

        protected void TextBox_OnMouseWheel(object sender, MouseEventArgs e)
        {
            int value = ScrollBar.Value;
            if( e.Delta < 0 && value < ScrollBar.Maximum)
            {
                ScrollBar.Value += 1;
            }
            else if (e.Delta > 0 && value > ScrollBar.Minimum)
            {
                ScrollBar.Value -= 1;
            }
        }
    }
}
