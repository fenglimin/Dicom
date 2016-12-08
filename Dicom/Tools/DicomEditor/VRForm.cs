using System;
using System.Windows.Forms;

namespace DicomEditor
{
    public partial class VRForm : Form
    {
        private string vr = "UN";

        public VRForm()
        {
            InitializeComponent();
        }

        public string VR
        {
            get
            {
                return vr;
            }
            set
            {
                vr = value;
            }

        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            string text = VRComboBox.Text;
            if (text != null && text != String.Empty)
            {
                vr = text.Substring(0, 2);
                DialogResult = DialogResult.OK;
            }
            else
            {
                vr = "UN";
                DialogResult = DialogResult.Cancel;
            }
        }
    }
}
