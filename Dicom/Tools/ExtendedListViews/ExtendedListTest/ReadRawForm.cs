using System;
using System.IO;
using System.Windows.Forms;
using DicomViewer;

namespace ExtendedListTest
{
    public partial class ReadRawForm : Form
    {
        private string path;
        private ImageAttributes attributes =
            new ImageAttributes(TypeCode.UInt16, null, 0, 0, 0, 12, 169, true);
        private FileInfo info;

        public ReadRawForm(string path, ImageAttributes attributes)
        {
            this.path = path;
            this.attributes = attributes;
            InitializeComponent();
        }

        /// <summary>
        /// 
        /// </summary>
        public ImageAttributes Attributes 
        {
            get 
            {
                return attributes;
            }
            set
            {
                attributes = value;
                SetControls();
            }
        }

        private void ReadRawForm_Load(object sender, EventArgs e)
        {
            info = new FileInfo(path);
            NameLabel.Text = info.Name;
            SetControls();
        }

        private void SetControls()
        {
            ColumnsTextBox.Text = attributes.width.ToString();
            RowsTextBox.Text = attributes.height.ToString();
            BPPComboBox.Text = attributes.bitsperpixel.ToString();
            Monochrome1CheckBox.Checked = attributes.monochrome1;
            EnableButtons();
        }

        private void GetControls()
        {
            attributes.width = Int32.Parse(ColumnsTextBox.Text);
            attributes.height = Int32.Parse(RowsTextBox.Text);
            attributes.stride = attributes.width;
            attributes.bitsperpixel = Int32.Parse(BPPComboBox.Text);
            attributes.monochrome1 = Monochrome1CheckBox.Checked;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            GetControls();

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Anywhere_TextChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void EnableButtons()
        {
            try
            {
                int columns = Int32.Parse(ColumnsTextBox.Text);
                int rows = Int32.Parse(RowsTextBox.Text);
                int bpp = Int32.Parse(this.BPPComboBox.Text);
                int size = (bpp > 8) ? 2 : 1;
                this.OKButton.Enabled = (columns * rows * size == info.Length);
            }
            catch
            {
            }
        }
    }
}
