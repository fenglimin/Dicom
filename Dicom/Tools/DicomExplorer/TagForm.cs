using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomExplorer
{
    public partial class TagForm : Form
    {
        private bool loading = false;
        private List<string> selection = null;
        private Dictionary<string, bool> choices = new Dictionary<string, bool>();

        public TagForm()
        {
            InitializeComponent();
        }

        public TagForm(List<String> mapping) :
            this()
        {
            this.selection = mapping;
        }

        public List<String> Selection
        {
            get
            {
                return selection;
            }
            set
            {
                selection = value;
            }
        }

        private void TagForm_Load(object sender, EventArgs e)
        {
            foreach (Tag entry in Dictionary.Instance)
            {
                choices.Add(entry.ToString() + " " + entry.Description, (selection != null && Contains(selection, entry.ToString())));
            }
            LoadListBox(String.Empty);
            FilterTextBox.Focus();
        }

        private bool Contains(List<string> collection, string text)
        {
            foreach(String item in collection)
            {
                if(item.ToLower().Contains(text.ToLower()))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadListBox(string filter)
        {
            loading = true;
            try
            {
                ResultsCheckedListBox.Items.Clear();
                filter = filter.ToLower();
                foreach (KeyValuePair<string, bool> choice in choices)
                {
                    if (filter == String.Empty || choice.Key.ToLower().Contains(filter))
                    {
                        ResultsCheckedListBox.Items.Add(choice.Key, choice.Value);
                    }
                }
            }
            finally
            {
                loading = false;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            selection = new List<string>();
            foreach (KeyValuePair<string, bool> choice in choices)
            {
                if (choice.Value)
                {
                    int position = choice.Key.IndexOf(" ");
                    string item = choice.Key.Substring(0, position).Trim();
                    selection.Add(item);
                }
            }
            // if they have entered a tag that is not recognized
            if (ResultsCheckedListBox.Items.Count == 0)
            {
                String text = FilterTextBox.Text;
                try
                {
                    Tag tag = EK.Capture.Dicom.DicomToolKit.Tag.Parse(text);
                    // we reach here if Parse does not throw
                    selection.Add(text);
                }
                catch
                {
                    // ignore any cases where they leave garbage or a partially typed tag
                }
            }
            DialogResult = (selection.Count != 0) ? DialogResult.OK : DialogResult.Cancel;
        }

        private void FilterTextBox_TextChanged(object sender, EventArgs e)
        {
            LoadListBox(FilterTextBox.Text);
        }

        private void ResultsCheckedListBox_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!loading)
            {
                CheckState b = e.NewValue;
                int index = e.Index;
                string name = ResultsCheckedListBox.Items[index] as String;
                choices[name] = (b == CheckState.Checked);
            }
        }

        private void Clear_Click(object sender, EventArgs e)
        {
            loading = true;
            FilterTextBox.Text = String.Empty;
            try
            {
                ResultsCheckedListBox.Items.Clear();
                string[] keys = new string[choices.Keys.Count];
                choices.Keys.CopyTo(keys, 0);
                foreach(string key in keys)
                {
                    choices[key] = false;
                }
            }
            finally
            {
                loading = false;
            }
            LoadListBox(FilterTextBox.Text);
        }

        private void ShowButton_Click(object sender, EventArgs e)
        {
            loading = true;
            try
            {
                ResultsCheckedListBox.Items.Clear();
                foreach (KeyValuePair<string, bool> choice in choices)
                {
                    if (choice.Value)
                    {
                        ResultsCheckedListBox.Items.Add(choice.Key, choice.Value);
                    }
                }
            }
            finally
            {
                loading = false;
            }
        }
    }
}