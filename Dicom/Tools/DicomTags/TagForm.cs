using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomTags
{
    public partial class TagForm : Form
    {
        List<string> choices = new List<string>();

        public TagForm()
        {
            InitializeComponent();
        }

        private void TagForm_Load(object sender, EventArgs e)
        {
            foreach (Tag entry in Dictionary.Instance)
            {
                choices.Add(entry.ToString() + " " + entry.Description);
            }
            LoadListBox(String.Empty);
        }

        private void LoadListBox(string filter)
        {
            ResultsListBox.Items.Clear();
            filter = filter.ToLower();
            foreach (string choice in choices)
            {
                if (filter == String.Empty || choice.ToLower().Contains(filter))
                {
                    ResultsListBox.Items.Add(choice);
                }
            }
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            TagTextBox.Text = String.Empty;
            LoadListBox(FilterTextBox.Text);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ResultsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Tag tag = Dictionary.Instance[Key(ResultsListBox.Text)];
            TagTextBox.Text = String.Format("{0}\r\n{1} vr={2} vm={3}", tag.Description, tag.ToString(), tag.VR, tag.VM); ;
        }

        private string Key(string name)
        {
            int position = name.IndexOf(' ');
            return (position >= 0) ? name.Substring(0, position) : String.Empty;
        }


    }
}