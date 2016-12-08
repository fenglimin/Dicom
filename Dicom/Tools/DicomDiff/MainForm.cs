using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomDiff
{
    public partial class MainForm : Form
    {
        string first = null;
        string second = null;

        public MainForm(string[] args)
        {
            ParseArgs(args);
            InitializeComponent();
        }

        private void ParseArgs(string[] args)
        {
            for (int n = 0; n < args.Length; n++)
            {
                string arg = args[n];
                if (first == null)
                {
                    first = args[n];
                }
                else
                {
                    second = args[n];
                }
            }
        }

        private void CompareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            first = null;
            second = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.DefaultExt = "dcm";
            dialog.Filter = "Dicom Files (*.dcm)|*.dcm|All files|*.*";
            dialog.Multiselect = true;
            dialog.Title = "Select one or both files to compare.";
            dialog.ShowDialog();

            if (dialog.FileNames.Length > 0)
            {
                first = dialog.FileNames[0];
            }
            else
            {
                return;
            }
            if (dialog.FileNames.Length > 1)
            {
                second = dialog.FileNames[1];
            }
            if (second == null)
            {
                dialog.Multiselect = false;
                dialog.Title = "Select the second file to compare.";
                dialog.ShowDialog();

                if (dialog.FileNames.Length > 0)
                {
                    second = dialog.FileNames[0];
                }
                else
                {
                    return;
                }
            }

            FillListBox();
        }

        private void FillListBox()
        {
            if(first == null || second == null)
            {
                return;
            }

            DataSet left = new DataSet();
            left.Read(first);
            DataSet right = new DataSet();
            right.Read(second);

            ArrayList keys = BatchProcessor.CreateMasterList(left, right);

            try
            {

                DiffListView.Clear();

                int width = (this.ClientRectangle.Width - 5) / 4;

                DiffListView.Columns.Add("Tag", width);
                DiffListView.Columns.Add("Name", width);

                string title = (first != null && first.Length > 0) ? new FileInfo(first).Name : "untitled";

                DiffListView.Columns.Add(title, width);

                title = (second != null && second.Length > 0) ? new FileInfo(second).Name : "untitled";
                DiffListView.Columns.Add(title, width);

                foreach (string key in keys)
                {
                    ListViewItem item = CreateNewItem(key, left, right);
                    DiffListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Problems encountered, " + ex.Message);
            }
        }

        private ListViewItem CreateNewItem(string key, DataSet left, DataSet right)
        {
            ListViewItem item = new ListViewItem();

            Tag tag = EK.Capture.Dicom.DicomToolKit.Tag.Tail(key);

            // the first column is the tag
            item.Text = key;
            // each subsequent column is added by calling item.SubItems.Add
            // the second column is the description
            item.SubItems.Add(tag.Description);

            if (left.Contains(key))
            {
                item.SubItems.Add(left[key].ToString());
                if (right.Contains(key))
                {
                    if (!BatchProcessor.ElementsCompare(left[key], right[key]))
                    {
                        item.BackColor = Color.LightGray;
                    }
                }
                else
                {
                    item.Font = new Font(item.Font, FontStyle.Italic);
                    item.ForeColor = Color.Red;
                }
            }
            else
            {
                item.SubItems.Add("");
            }

            if (right.Contains(key))
            {
                item.SubItems.Add(right[key].ToString());
                if (left.Contains(key))
                {
                    if (!BatchProcessor.ElementsCompare(left[key], right[key]))
                    {
                        item.BackColor = Color.LightGray;
                    }
                }
                else
                {
                    item.Font = new Font(item.Font, FontStyle.Italic);
                    item.ForeColor = Color.Red;
                }
            }
            else
            {
                item.SubItems.Add("");
            }
            return item;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (DiffListView.Columns.Count == 4)
            {
                DiffListView.Columns[0].Width = this.ClientRectangle.Width / 4;
                DiffListView.Columns[1].Width = this.ClientRectangle.Width / 4;
                DiffListView.Columns[2].Width = this.ClientRectangle.Width / 4;
                DiffListView.Columns[3].Width = this.ClientRectangle.Width / 4;
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string version = AssemblyName.GetAssemblyName(Assembly.GetAssembly(this.GetType()).Location).Version.ToString();
            MessageBox.Show(String.Format("DicomDiff, version {0}\n\nUsage:{1}", version, BatchProcessor.Usage), "About");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            if(first != null && second != null)
            {
                FillListBox();
            }
        }
    }
}
