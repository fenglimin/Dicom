using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomShift
{
    public partial class MainForm : Form
    {
        private ushort[] lut = null;
        private ushort N;
        private ushort maxvalue;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Shift(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
            {
                EK.Capture.Dicom.DicomToolKit.DataSet dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                dicom.Read(stream);

                stream.Close();

                // apply the shift to the image pixels
                ushort[] pixels = (ushort[])dicom[t.PixelData].Value;
                for (int n = 0; n < pixels.Length; n++)
                {
                    pixels[n] = lut[pixels[n]];
                }

                if (AddPixelValuesCheckBox.Checked)
                {
                    pixels[0] = 0;
                    pixels[1] = (ushort)(maxvalue - 1);
                }

                dicom.Set(t.BitsStored, N);
                dicom.Set(t.HighBit, N-1);
                if (AddPixelValuesCheckBox.Checked)
                {
                    pixels[0] = 0;
                    pixels[1] = (ushort)(maxvalue - 1);
                    dicom.Set(t.SmallestImagePixelValue, (ushort)0);
                    dicom.Set(t.LargestImagePixelValue, (ushort)(maxvalue - 1));
                }
                else
                {
                    dicom.Set(t.SmallestImagePixelValue, (ushort)(maxvalue / 4));
                    dicom.Set(t.LargestImagePixelValue, maxvalue - maxvalue / 4);
                }
                dicom.Set(t.WindowWidth, maxvalue / 2);
                dicom.Set(t.WindowCenter, maxvalue / 2);

                FileInfo info = new FileInfo(filename);
                String text = filename.Replace(info.Extension, "");
                text += "a" + info.Extension;

                dicom.Write(text);
            }
        }

        /// <summary>
        /// Create the lut with the current values
        /// </summary>
        private void SetupLut()
        {
            ushort start = (ushort)(maxvalue / 4);
            ushort delta = (ushort)(maxvalue / 2 / 4096);
            lut = new ushort[4096];
            for (int n = 0; n < lut.Length; n++)
            {
                lut[n] = (ushort)(start + n * delta);
            }
        }

        private void ShiftButton_Click(object sender, EventArgs e)
        {
            this.ShiftButton.Enabled = false;

            try
            {
                SetupLut();

                OpenFileDialog dialog = new OpenFileDialog();
                dialog.DefaultExt = "dcm";
                dialog.Filter = "Dicom Files (*.dcm)|*.dcm";
                dialog.Multiselect = true;
                dialog.ShowDialog();
                if (dialog.FileNames.Length > 0)
                {
                    foreach (string filename in dialog.FileNames)
                    {
                        Shift(filename);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.ShiftButton.Enabled = true;
            }
        }

        private void RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = sender as RadioButton;
            if (radio != null && radio.Checked)
            {
                N = UInt16.Parse(radio.Tag.ToString());
                maxvalue = (ushort)(1 << (int)N);
                this.AddPixelValuesCheckBox.Text = String.Format("Add 0 and {0} Pixel Values", 1 << (int)N);
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            N = 15;
            maxvalue = (ushort)(1 << (int)N);
            this.RadioButton15.Checked = true;
        }
    }
}
