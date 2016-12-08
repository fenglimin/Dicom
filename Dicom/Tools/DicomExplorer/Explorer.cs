using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomExplorer
{
    public partial class Explorer : Form
    {
        private FileInfo[] files;
        private String[] contents;
        private object sentry = new object();
        private List<string> mapping;
        private int index;
        private ListViewItem item = null;

        public Explorer(string path)
        {
            InitializeComponent();

            PopulateTreeView(path);
            ConfigureListView();
            PopulateListView(path);
        }

        private void PopulateTreeView(string path)
        {
            this.Cursor = Cursors.WaitCursor;
            PopulateDriveList();
            ExpandPath(path);
            this.Cursor = Cursors.Default;
        }

        private void ExpandPath(string path)
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo info = new DirectoryInfo(path);
                if (info.Exists)
                {
                    string[] folders = path.Split(@"\".ToCharArray());
                    TreeNode parent = FolderTreeView.Nodes[0];
                    foreach (string folder in folders)
                    {
                        string root = (parent != null && parent.Tag != null) ? ((DirectoryInfo)parent.Tag).FullName : "";
                        if ((parent = FindChildNode(parent, folder)) != null)
                        {
                            PopulateFolder(parent, Path.Combine(root, Pad(StripProvider(parent.Text))));
                            FolderTreeView.SelectedNode = parent;
                        }
                    }
                }
            }
        }

        TreeNode FindChildNode(TreeNode parent, string name)
        {
            TreeNode node = null;
            foreach (TreeNode child in parent.Nodes)
            {
                if(StripProvider(child.Text) == name)
                {
                    node = child;
                    break;
                }
            }
            return node;
        }

        private void PopulateFolder(TreeNode root, string path)
        {
            this.Cursor = Cursors.WaitCursor;
            DirectoryInfo info = new DirectoryInfo(Pad(path));
            if (info.Exists)
            {
                root.Tag = info;
                // remove empty node added to have [+] icon
                if (root.Nodes.Count == 1)
                {
                    root.Nodes.RemoveAt(0);
                }
                GetDirectories(info.GetDirectories(), root);
            }
            this.Cursor = Cursors.Default;
        }

        //This procedure populate the TreeView with the Drive list
        private void PopulateDriveList()
        {
            TreeNode nodeTreeNode;
            int imageIndex = 0;
            int selectIndex = 0;

            const int Removable = 2;
            const int LocalDisk = 3;
            const int Network = 4;
            const int CD = 5;
            //const int RAMDrive = 6;

            this.Cursor = Cursors.WaitCursor;
            //clear TreeView
            FolderTreeView.Nodes.Clear();
            nodeTreeNode = new TreeNode("My Computer", 0, 0);
            FolderTreeView.Nodes.Add(nodeTreeNode);

            //set node collection
            TreeNodeCollection nodeCollection = nodeTreeNode.Nodes;

            //Get Drive list
            ManagementObjectCollection queryCollection = GetDrives();
            foreach (ManagementObject mo in queryCollection)
            {
                int type = int.Parse(mo["DriveType"].ToString());
                switch (type)
                {
                    case Removable:			//removable drives
                        imageIndex = 5;
                        selectIndex = 5;
                        break;
                    case LocalDisk:			//Local drives
                        imageIndex = 6;
                        selectIndex = 6;
                        break;
                    case CD:				//CD rom drives
                        imageIndex = 7;
                        selectIndex = 7;
                        break;
                    case Network:			//Network drives
                        imageIndex = 8;
                        selectIndex = 8;
                        break;
                    default:				//defalut to folder
                        imageIndex = 2;
                        selectIndex = 3;
                        break;
                }

                //Debug.WriteLine(mo["Name"].ToString());
                //foreach (PropertyData p in mo.Properties)
                //{
                //    Debug.WriteLine(String.Format("{0}:{1}", p.Name, p.Value));
                //}
                //Debug.WriteLine("");
                //Debug.WriteLine("");

                //create new drive node
                DirectoryInfo info = new DirectoryInfo(Pad(mo["Name"].ToString()));
                if (info.Exists)
                {
                    string name = info.Root.Name;
                    if(type == Network)
                    {
                        name = String.Format("{0} ({1})", mo["ProviderName"], mo["Name"]);
                    }
                    nodeTreeNode = new TreeNode(Strip(name));
                    // add an empty node to give this a [+] icon, we will expand later if clicked
                    nodeTreeNode.Nodes.Add(new TreeNode());
                    nodeCollection.Add(nodeTreeNode);
                }
            }
            this.Cursor = Cursors.Default;

        }

        protected ManagementObjectCollection GetDrives()
        {
            //get drive collection
            ManagementObjectSearcher query = new ManagementObjectSearcher("SELECT * From Win32_LogicalDisk ");
            ManagementObjectCollection collection = query.Get();
            return collection;
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                if ((subDir.Attributes & (FileAttributes.Hidden)) != 0)
                {
                    continue;
                }
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.ImageKey = "folder";
                try
                {
                    subSubDirs = subDir.GetDirectories();
                    if (subSubDirs.Length == 0)
                    {
                        aNode.Tag = subDir;
                    }
                    else
                    {
                        // add an empty node to give this a [+] icon, we will expand later if clicked
                        aNode.Nodes.Add(new TreeNode());
                    }
                }
                catch(Exception)
                {
                }
                finally
                {
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private void ConfigureListView()
        {
            FileListView.Clear();

            if (mapping == null)
            {
                mapping = LoadMapping();
            }

            FileListView.Columns.Add("Filename");

            foreach(String line in mapping)
            { 
                string[] strings = line.Split(":".ToCharArray());
                if (EK.Capture.Dicom.DicomToolKit.Tag.TryParse(strings[0]))
                {
                    int width = 100;
                    if (Dictionary.Contains(strings[0]))
                    {
                        Tag tag = Dictionary.Instance[strings[0]];
                        switch (tag.VR)
                        {
                            case "PN":
                                width = 200;
                                break;
                            default:
                                break;
                        }
                    }
                    FileListView.Columns.Add((strings.Length > 1) ? strings[1] : Dictionary.Instance[strings[0]].Description, width);
                }
                else
                {
                    Logging.Log(LogLevel.Error, String.Format("Unable to parse, {0}", line));
                }
            }
        }

        internal List<String> Mapping
        {
            get
            {
                if (mapping == null)
                {
                    mapping = LoadMapping();
                }
                return mapping;
            }
            set
            {
                mapping = value;
                SaveMapping(mapping);
            }
        }

        private List<string> LoadMapping()
        {
            List<string> result = new List<string>();
            if (!File.Exists("explorer.txt"))
            {
                string[] tags = {
                            "(0018,0015)",  // Body Part Examined
                            "(0018,5101)",  // View Position
                            "(0029,1015)",  // View Name
                            "(0008,1030)",  // Study Description
                            "(0008,103E)",  // Series Description
                            "(0008,0022)",  // Acquisition Date
                            "(0008,0032)",  // Acquisition Time
                            "(0008,1090)",  // Manufacturer's Model Name
                            "(0028,0011)",  // Columns
                            "(0028,0010)"   // Rows
                                  };
                result = new List<string>(tags);
                SaveMapping(result);
            }
            else
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader("explorer.txt"))
                {
                    string line = String.Empty;
                    while ((line = file.ReadLine()) != null)
                    {
                        //Logging.Log(line);
                        string[] strings = line.Split(":".ToCharArray());
                        if (EK.Capture.Dicom.DicomToolKit.Tag.TryParse(strings[0]))
                        {
                            result.Add(line);
                        }
                        else
                        {
                            Logging.Log(LogLevel.Error, String.Format("Unable to parse, {0}", line));
                        }
                    }
                    file.Close();
                }
            }
            return result;
        }

        private void SaveMapping(List<string> collection)
        {
            using (System.IO.StreamWriter writer = new StreamWriter("explorer.txt"))
            {
                foreach (string temp in collection)
                {
                    writer.WriteLine(temp);
                }
                writer.Flush();
            }
        }

        private void PopulateListView(string path)
        {
            this.Cursor = Cursors.WaitCursor;
            if (Directory.Exists(path))
            {
               ConfigureListView();
               DirectoryInfo folder = new DirectoryInfo(path);

                files = folder.GetFiles("*.dcm");
                contents = new String[files.Length];
                index = Int32.MinValue;

                FileListView.VirtualListSize = files.Length;
            }
            this.Cursor = Cursors.Default;
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor current = Cursor.Current;
            try
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.RestoreDirectory = true;
                dialog.AddExtension = true;
                dialog.Filter = "CSV files|*.csv";
                dialog.FileName = "export.csv";
                dialog.ShowDialog();
                if (dialog.FileNames.Length != 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    Export(dialog.FileNames[0]);
                }
            }
            catch (Exception ex)
            {
                Logging.Log(ex);
            }
            finally
            {
                Cursor.Current = current;
            }
        }

        private void Export(string path)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    // header
                    writer.Write("\"Filename\"");
                    foreach (String text in mapping)
                    {
                        string[] strings = text.Split(":".ToCharArray());
                        writer.Write(",\"" + ((strings.Length > 1) ? strings[1] : Dictionary.Instance[strings[0]].Description) + "\"");
                    }
                    writer.WriteLine();

                    // rows
                    foreach (FileInfo file in files)
                    {
                        EK.Capture.Dicom.DicomToolKit.DataSet dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                        dicom.Read(file.FullName, 0x7F00);

                        writer.Write("\"" + file.FullName + "\"");

                        foreach (String text in mapping)
                        {
                            string[] strings = text.Split(":".ToCharArray());
                            string value = String.Empty;
                            try
                            {
                                value = (dicom.ValueExists(strings[0])) ? ToString(dicom[strings[0]]) : String.Empty;
                            }
                            catch (Exception ex)
                            {
                                Logging.Log(LogLevel.Error, String.Format("Export Value: {0}", ex.Message));
                            }
                            writer.Write(",\"" + value + "\"");
                        }
                        writer.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Log(LogLevel.Error, String.Format("Export: {0}", ex.Message));
            }
        }

        private void CompareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int first = FileListView.SelectedIndices[0];
            int second = FileListView.SelectedIndices[1];
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "DicomDiff.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = "\"" + files[first ].FullName + "\" \"" + files[second].FullName + "\"";

            Process.Start(startInfo);
        }

        private void ViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Launch("DicomViewer.exe");
        }

        private void EditToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Launch("DicomEditor.exe");
        }

        private void Launch(string application)
        {
            foreach(int n in FileListView.SelectedIndices)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.CreateNoWindow = false;
                startInfo.UseShellExecute = false;
                startInfo.FileName = application;
                startInfo.WindowStyle = ProcessWindowStyle.Normal;
                startInfo.Arguments = "\"" + files[n].FullName + "\"";

                Process.Start(startInfo);
            }
        }

        private void ListViewContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            int count = FileListView.SelectedIndices.Count;
            ExportToolStripMenuItem.Enabled = true;
            ViewToolStripMenuItem.Enabled = (count > 0);
            EditToolStripMenuItem.Enabled = (count > 0);
            CompareToolStripMenuItem.Enabled = (count == 2);
        }

        private void FileListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(e.ItemIndex);
            // if an index is being requested that is not the current one
            if (e.ItemIndex != index)
            {
                // set the current index and get the FileInfo
                index = e.ItemIndex;
                FileInfo file = files[index];
                try
                {
                    // see if we have already parsed this file
                    string line = contents[index];
                    if (line == null || line == String.Empty)
                    {
                        // we have not parsed the file, so we parse it, and store the results
                        line = ReadDicom(file);
                        lock (sentry)
                        {
                            contents[index] = line;
                        }
                    }
                    // we either parsed the file previously, or just did
                    if (line != null)
                    {
                        // we create an item from the information that we got
                        item = ParseListItem(file, line);
                    }
                    else
                    {
                        Logging.Log(LogLevel.Error, "Unexpected case of missing index.");
                    }
                }
                finally
                {
                    if (item == null)
                    {
                        // we need to return a ListViewItem with the expected nuber of columns.
                        item = new ListViewItem();
                        item.Text = file.Name;
                        for(int n = 1; n < FileListView.Columns.Count; n++)
                        {
                            item.SubItems.Add("error");
                        }
                    }
                }
            }
            e.Item = item;
        }

        private string ReadDicom(FileInfo file)
        {
            string error = "\uFFFD";
            StringBuilder line = new StringBuilder();
            try
            {
                EK.Capture.Dicom.DicomToolKit.DataSet dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                dicom.Read(file.FullName, 0x5000);
                for(int n = 0; n < mapping.Count; n++)
                {
                    String text = mapping[n];
                    string[] strings = text.Split(":".ToCharArray());
                    string value = String.Empty;
                    try
                    {
                        value = (dicom.ValueExists(strings[0])) ? ToString(dicom[strings[0]]) : String.Empty;
                    }
                    catch (Exception ex)
                    {
                        value = error;
                        Logging.Log(LogLevel.Error, String.Format("ReadDicom Value: {0}", ex.Message));
                    }
                    if (n > 0)
                    {
                        line.Append("|");
                    }
                    line.Append(value);
                }
            }
            catch (Exception ex)
            {
                line = new StringBuilder("");
                for (int n = 0; n < mapping.Count; n++)
                {
                    if (n > 0)
                    {
                        line.Append("|");
                    }
                    line.Append(error);
                }
                Logging.Log(LogLevel.Error, String.Format("ReadDicom: {0}", ex.Message));
            }
            return line.ToString();
        }

        private ListViewItem ParseListItem(FileInfo file, string text)
        {
            ListViewItem row = new ListViewItem();
            row.Text = file.Name;
            string[] columns = text.Split("|".ToCharArray());
            foreach (string column in columns)
            {
                row.SubItems.Add(column);
            }
            return row;
        }

        private string ToString(Element element)
        {
            String value = String.Empty;
            if (element.Value is Array)
            {
                StringBuilder text = new StringBuilder();
                if (((Array)element.Value).Length > 12)
                {
                    text.Append(String.Format("{0} byte(s).", element.Length));
                }
                else
                {
                    bool first = true;
                    foreach (object entry in element.Value as Array)
                    {
                        // we cannot base this on text.Length because the first string could be empty
                        if (!first)
                        {
                            text.Append("\\");
                        }
                        else
                        {
                            first = false;
                        }
                        text.Append(entry.ToString());
                        if (text.Length > 80)
                        {
                            text.Append(" ...");
                            break;
                        }
                    }
                }
                value = text.ToString();
            }
            else
            {
                object temp = element.Value;
                value = temp.ToString();
            }
            return value;
        }

        void FolderTreeView_MouseClick(object sender,  TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Parent != null)
            {
                DirectoryInfo info = (DirectoryInfo)e.Node.Tag;
                if (info == null)
                {
                    string parent = (e.Node.Parent.Tag != null) ? ((DirectoryInfo)e.Node.Parent.Tag).FullName : "";
                    PopulateFolder(e.Node, Path.Combine(parent, StripProvider(e.Node.Text)));
                    info = (DirectoryInfo)e.Node.Tag;
                    e.Node.Expand();
                }
                Invalidate();
                PopulateListView(info.FullName);
                Settings settings = new Settings("DicomExplorer");
                settings["Path"] = info.FullName;
            }
        }

        private string Pad(string text)
        {
            string result = text;
            if (!text.EndsWith("\\"))
            {
                result += "\\";
            }
            return result;
        }

        private string Strip(string text)
        {
            string result = text;
            if (text.EndsWith("\\"))
            {
                result = text.TrimEnd("\\".ToCharArray());
            }
            return result;
        }

        private string StripProvider(string text)
        {
            string result = text;
            string pattern = @"(?<provider>.+) \((?<path>.):\)";
            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
                result = String.Format("{0}:", match.Groups["path"].Value);
            }
            return result;
        }

        private void FileListView_DoubleClick(object sender, EventArgs e)
        {
            Launch("DicomViewer.exe");
        }
    }
}
