using System;
using System.Windows.Forms;

namespace DicomEditor
{
    public interface IFindable
    {
        void FindNext(string text, bool forward);
    }

    public partial class FindForm : Form
    {
        private static string text = String.Empty;
        private bool forward = true;
        private IFindable target = null;

        public FindForm(IFindable target) 
            : base()
        {
            this.target = target;
            InitializeComponent();
        }

        public string FindText
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
            }
        }

        public bool Forward
        {
            get
            {
                return forward;
            }
            set
            {
                forward = value;
            }
        }

        private void FindButton_Click(object sender, EventArgs e)
        {
            FindText = this.FindTextBox.Text;
            Forward = DownRadioButton.Checked;
            if (target != null)
            {
                ((IFindable)target).FindNext(FindText, Forward);
            }
            DialogResult = DialogResult.OK;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void FindForm_Load(object sender, EventArgs e)
        {
            FindTextBox.Text = FindText;
        }
    }
}
