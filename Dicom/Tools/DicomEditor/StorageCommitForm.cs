using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    public partial class StorageCommitForm : Form
    {
        private Dictionary<string, ApplicationEntity> hosts;

        public StorageCommitForm(Dictionary<string, ApplicationEntity> hosts)
        {
            this.hosts = hosts;
            InitializeComponent();
        }

        private void SendButton_Click(object sender, EventArgs e)
        {
            int status = (SuccessCheckBox.Checked) ? 0 : 1;
            ApplicationEntity host = (ApplicationEntity)((ComboBoxItem)HostComboBox.SelectedItem).Value;
            DirectoryInfo directory = new DirectoryInfo(".");
            foreach (FileInfo file in directory.GetFiles("*.commit"))
            {
                SendStorageCommit(file.FullName, host, status);
            }
            RefreshControls();
        }

        private void EnableControls()
        {
            CancelButton.Enabled = true;
            HostComboBox.Enabled = HostComboBox.Items.Count > 0;

            DirectoryInfo directory = new DirectoryInfo(".");
            FileInfo[] files = directory.GetFiles("*.commit");

            StatusStrip.Text = String.Format("{0} reports to send.", files.Length);

            SendButton.Enabled = ClearButton.Enabled = SendButton.Enabled = files.Length > 0;
        }

        private void StorageCommitForm_Load(object sender, EventArgs e)
        {
            RefreshControls();
        }

        private void RefreshControls()
        {
            LoadHosts();
            EnableControls();
        }

        private void LoadHosts()
        {
            HostComboBox.Items.Clear();
            foreach (KeyValuePair<string, ApplicationEntity> kvp in hosts)
            {
                ApplicationEntity host = (ApplicationEntity)kvp.Value;
                string text = String.Format("{0},{1},{2}", host.Title, host.Address, host.Port);
                HostComboBox.Items.Add(new ComboBoxItem(text, host));
            }
        }

        private bool SendStorageCommit(string path, ApplicationEntity host, int status)
        {
            bool result = true;
            try
            {
                StorageCommitServiceSCU commit = new StorageCommitServiceSCU();
                commit.Syntaxes.Add(Syntax.ImplicitVrLittleEndian);
                commit.Role = Role.Scp;

                Association association = new Association();
                association.AddService(commit);

                if (association.Open(host))
                {
                    if (commit.Active)
                    {
                        try
                        {
                            EK.Capture.Dicom.DicomToolKit.DataSet dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                            dicom.Read(path);

                            if (status != 0)
                            {
                                dicom.Add(t.RetrieveAETitle, host.Title);
                                // add the FailedSOPSequence
                                Sequence sequence = new Sequence(t.FailedSOPSequence);
                                dicom.Add(sequence);
                                Elements item = sequence.NewItem();
                                item.Add(dicom[t.ReferencedSOPSequence + t.ReferencedSOPClassUID]);
                                item.Add(dicom[t.ReferencedSOPSequence + t.ReferencedSOPInstanceUID]);
                                item.Add(t.FailureReason, 274);
                                // remove the ReferencedSOPSequence
                                dicom.Remove(t.ReferencedSOPSequence);
                            }

                            result = commit.Report(dicom);
                            Debug.WriteLine("commit done!");
                        }
                        catch (Exception ex)
                        {
                            result = false;
                            Debug.WriteLine(ex.Message);
                        }
                    }
                }
                else
                {
                    result = false;
                    Debug.WriteLine("\ncan't Open.");
                }
                association.Close();
            }
            catch
            {
            }
            RefreshControls();
            return result;
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            DirectoryInfo directory = new DirectoryInfo(".");
            FileInfo[] files = directory.GetFiles("*.commit");
            foreach (FileInfo file in files)
            {
                file.Delete();
            }
            RefreshControls();
        }
    }

    internal class ComboBoxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }

        public ComboBoxItem(string text, object value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }

}
