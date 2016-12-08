using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomEditor
{
    public partial class Browser : Form, IFindable
    {
        bool dicomdir = false;
        string filename = String.Empty;

        EK.Capture.Dicom.DicomToolKit.DataSet dicom = null;
        FileStream stream = null;

        public Browser(string filename) 
        {
            try
            {
               this.filename = filename;
                if (File.Exists(filename))
                {
                    FileInfo info = new FileInfo(filename);
                    if (".txt" == info.Extension.ToLower())
                    {
                        dicom = BatchEditor.ProcessFile(filename);
                        dicom.Part10Header = true;
                        dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;
                    }
                }
                InitializeComponent();
                // set the Title after the component is initialized.
                SetTitles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public Browser(EK.Capture.Dicom.DicomToolKit.DataSet dicom)
            : this("")
        {
            try
            {
                this.dicom = dicom;
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public string FileName
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
                SetTitles();
            }
        }

        public DataSet Dicom
        {
            get
            {
                return dicom;
            }
        }

        public bool DICOMDIR
        {
            get
            {
                return dicomdir;
            }
            set
            {
                if (dicomdir != value)
                {
                    dicomdir = value;
                    FillTreeView();
                }
            }
        }

        public void Save()
        {
            if (stream != null)
            {
                stream.Close();
                stream.Dispose();
                stream = null;
            }

            try
            {
                // TODO if an exception is thrown, the old file is truncated
                dicom.Write(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                    stream.Dispose();
                    stream = null;
                }
            }
        }

        private void SetTitles()
        {
            // set the window title
            if (filename != null && filename.Length > 0)
            {
                this.Text = FileName;
            }
            // and the label text of the root node if any
            if (TreeView.Nodes.Count > 0)
            {
                TreeView.Nodes[0].Text = FileName;
            }
        }

        private void Browser_Load(object sender, EventArgs e)
        {
            try
            {
                if (dicom == null)  
                {
                    if (filename != null && filename.Length > 0)
                    {
                        try
                        {
                            if (filename != null && filename.Length > 0)
                            {
                                stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                                dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                                dicom.Read(stream);
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!dicom.Part10Header)
                            {
                                System.Windows.Forms.MessageBox.Show("Unable to parse dicom fragment, " + ex.Message);
                            }
                            else
                            {
                                System.Windows.Forms.MessageBox.Show("Valid Dicom header found. Unable to parse dicom file, " + ex.Message);
                            }
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Close();
                                stream.Dispose();
                                stream = null;
                            }
                        }
                    }
                    else
                    {
                        dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();
                        dicom.Part10Header = true;
                    }
                }
                FillTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        private void Browser_Move(object sender, EventArgs e)
        {
            TreeView.Location = new Point(0, 0);
            TreeView.Size = ClientRectangle.Size;
        }

        private void FillTreeView()
        {
            try
            {
                TreeView.Nodes.Clear();
                string title = (filename != null && filename.Length > 0) ? filename : "untitled";
                TreeNode root = TreeView.Nodes.Add("root", title);
                if (dicomdir)
                {
                    FileInfo info = new FileInfo(filename);
                    if (info.Name=="DICOMDIR")
                    {
                        DicomDir dir = new DicomDir(info.DirectoryName);
                        FillElement(dir.Patients, root);
                    }
                }
                else
                {
                    foreach (Element element in dicom)
                    {
                        // do not show group length tags
                        if (element.element == 0)
                            continue;
                        TreeNode node = root.Nodes.Add(element.GetPath() + " tag", Title(element));
                        FillElement(element, node);
                    }
                }
                root.Expand();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Problems encountered, " + ex.Message);
            }
        }

        private void FillElement(List<DirectoryRecord> records, TreeNode root)
        {
            foreach (DirectoryRecord record in records)
            {
                TreeNode node = root.Nodes.Add(record.DirectoryRecordType);
                foreach (Element element in record.Elements)
                {
                    TreeNode subNode = node.Nodes.Add(element.GetPath() + " tag", Title(element));
                    FillElement(element, subNode);
                }
                if (record is DirectoryParent)
                {
                    FillElement(((DirectoryParent)record).Children, node);
                }
            }
        }

        private void FillElement(Element element, TreeNode node)
        {
            if (element is Sequence)
            {
                int count = ((Sequence)element).Items.Count;
                for (int n = 0; n < count; n++)
                {
                    Elements item = ((Sequence)element).Items[n];
                    TreeNode itemNode = node.Nodes.Add(element.GetPath() + " item" + n.ToString(), "Item");
                    foreach (Element child in item)
                    {
                        // do not show group length tags
                        if (child.element == 0)
                            continue;
                        TreeNode subNode = itemNode.Nodes.Add(child.GetPath() + " tag", Title(child));
                        FillElement(child, subNode);
                    }
                }
            }
            else if (element is PixelData)
            {
                if (((PixelData)element).IsEncapsulated)
                {
                    int count = ((PixelData)element).Frames.Count;
                    for (int n = 0; n < count; n++)
                    {
                        TreeNode frameNode = node.Nodes.Add(element.GetPath() + " frame" + n.ToString(), "Frame");
                        string text = String.Format("{0} byte(s).", ((PixelData)element).Frames[n].Length);
                        frameNode.Nodes.Add(element.GetPath() + " value", text);
                    }
                }
                else
                {
                    FillValue(element, node);
                }
            }
            else
            {
                FillValue(element, node);
            }
        }

        private void FillValue(Element element, TreeNode node)
        {
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
                        // we cannot bas this on text.Length because the fist string could be empty
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
                node.Nodes.Add(element.GetPath() + " value", text.ToString());
            }
            else
            {
                object value = element.Value;
                node.Nodes.Add(element.GetPath() + " value", (value == null) ? String.Empty : value.ToString());
            }
        }

        private string Key(string name)
        {
            int position = name.IndexOf(' ');

            return (position >= 0) ? name.Substring(0, position) : String.Empty;
        }

        private string Title(Element element)
        {
            return String.Format("{0} {1} - {2}", EK.Capture.Dicom.DicomToolKit.Tag.ToString(element.Group, element.element), element.VR, element.Description);
        }

        private void TreeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // if this is a sequence or item do not edit
            if (e.Node.Nodes.Count > 0)
            {
                e.CancelEdit = true;
            }
            // there are some type that we do not support
            string key = Key(e.Node.Name);
            if (key != null && key != String.Empty)
            {
                Element element = dicom[key];
                if (element.VR == "AT" || element.VR == "UN" || element.VR == "OB" ||
                    element.VR == "OW" || element.VR == "OF" || element.VR == "SQ")
                {
                    e.CancelEdit = true;
                }
            }
        }

        private void TreeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            Element element = dicom[Key(e.Node.Name)];
            try
            {
                // a cancelled edit leaves e.Label null
                if (element != null && e.Label != null)
                {
                    element.Value = e.Label;
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                e.CancelEdit = true;
            }
        }

        private void TagContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                //Debug.WriteLine(node.Name);
                if (node.Name.IndexOf(" value") < 0)
                {
                    e.Cancel = false;
                }
                
                NewTagToolStripMenuItem.Enabled = (node.Name.Contains("root") || node.Name.Contains(" item") || (node.Name.Contains(" tag") && node.Text.Contains(" SQ ")));
                NewTagToolStripMenuItem.Text = (node.Name.Contains(" tag") && node.Text.Contains(" SQ ")) ? "Add Item" : "Add";

                CopyToolStripMenuItem.Enabled = (node.Name.Contains(" tag") && !node.Text.Contains(" SQ "));

                bool exporatble = IsExportable(node);
                ImportValueToolStripMenuItem.Enabled = exporatble;
                ExportValueToolStripMenuItem.Enabled = exporatble;
            }
        }

        private bool IsExportable(TreeNode node)
        {
            string key = Key(node.Name);
            if (key == String.Empty) return false;

            Element element = dicom[key];
            bool encapsulated = (element is PixelData && !node.Name.Contains("frame") && ((PixelData)element).IsEncapsulated);

            // exportable meand binary or long text and frames, but not the entire encapsulated PixelData tag
            return (!encapsulated && (IsBinary(node) || IsLongText(node) || node.Name.Contains(" frame")));
        }

        private bool IsLongText(TreeNode node)
        {
            bool sizeable = false;
            if (node.Name.Contains(" tag"))
            {
                Tag tag = new Tag(Key(node.Name));
                if (tag.VR == "UT" || tag.VR == "LT" || tag.VR == "ST")
                {
                    sizeable = true;
                }
            }
            return sizeable;
        }

        private bool IsBinary(TreeNode node)
        {
            bool binary = false;
            if (node.Name.Contains(" tag"))
            {
                Tag tag = new Tag(Key(node.Name));
                if(tag.VR.Contains("OW") || tag.VR.Contains("OB") || tag.VR == "OF" ||
                    ((tag.VM.Contains("-") || !tag.VM.Contains("1")) && (tag.VR == "FL" || tag.VR == "FD" || tag.VR == "SL" || tag.VR.Contains("SS") || tag.VR == "UL" || tag.VR.Contains("US"))))
                {
                    binary = true;
                }
            }
            return binary;
        }

        private int FindInsertionIndex(string key, TreeNodeCollection nodes)
        {
            int count = nodes.Count;
            int n = 0;
            for (; n < count; n++)
            {
                TreeNode node = nodes[n];
                if (String.Compare(node.Name, key) >= 0)
                {
                    break;
                }
            }
            return n;
        }

        private void NewTagToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNode node = this.TreeView.SelectedNode;
                if (node != null)
                {
                    if (node.Name.Contains(" tag") && node.Text.Contains(" SQ "))
                    {
                        Sequence sequence = dicom[Key(node.Name)] as Sequence;
                        sequence.NewItem();

                        TreeNode child = node.Nodes.Add(sequence.GetPath() + " item" + node.Nodes.Count.ToString(), "Item");
                        TreeView.SelectedNode = child;
                    }
                    else
                    {
                        NewTagForm dialog = new NewTagForm();
                        DialogResult result = dialog.ShowDialog();
                        if (result == DialogResult.OK)
                        {
                            if (node.Name.Contains(" item") || node.Name.Contains("root"))
                            {
                                TreeNode child = null;
                                foreach (string selection in dialog.Selection)
                                {
                                    Tag tag = EK.Capture.Dicom.DicomToolKit.Tag.Parse(selection);
                                    if((tag.IsPrivate && !tag.IsPrivateCreator) || (tag.IsStandard && !Dictionary.Contains(selection)))
                                    {
                                        VRForm form = new VRForm();
                                        result = form.ShowDialog();
                                        if (result == DialogResult.OK)
                                        {
                                            // automatically add the Private Creator if not found
                                            if (tag.IsPrivate)
                                            {
                                                // e.g., (0029,1000)
                                                string creator = selection.Substring(0,6) + "00" + selection.Substring(6,2) + ")";
                                                if (!dicom.Contains(creator))
                                                {
                                                    dicom.Add(creator, null);

                                                    Element temp = dicom[Key(node.Name) + creator];
                                                    int i = FindInsertionIndex(creator, node.Nodes);
                                                    if (i <= node.Nodes.Count || node.Nodes.Count == 0)
                                                    {
                                                        child = node.Nodes.Insert(i, temp.GetPath() + " tag", Title(temp));
                                                        child.Nodes.Add(temp.GetPath() + " value", String.Empty);
                                                        child.Expand();
                                                    }
                                                }
                                            }/**/
                                            dicom.Add(selection, form.VR, null);
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else if (node.Name.Contains("root"))
                                    {
                                        dicom.Add(selection, null);
                                    }
                                    else
                                    {
                                        Sequence sequence = dicom[Key(node.Name)] as Sequence;
                                        int item = 0;
                                        sequence.Items[item].Add(selection, null);
                                    }
                                    Element element = dicom[Key(node.Name) + selection];

                                    int n = FindInsertionIndex(selection, node.Nodes);

                                    if (n <= node.Nodes.Count || node.Nodes.Count == 0)
                                    {
                                        child = node.Nodes.Insert(n, element.GetPath() + " tag", Title(element));
                                        if (element.VR != "SQ")
                                        {
                                            child = child.Nodes.Add(element.GetPath() + " value", String.Empty);
                                        }
                                    }
                                }
                                if (child != null)
                                {
                                    TreeView.SelectedNode = child;
                                    TreeView.SelectedNode.BeginEdit();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                Tag head = EK.Capture.Dicom.DicomToolKit.Tag.Head(node.Name.Substring(0, node.Name.IndexOf(' ')));
                dicom.Remove(head.ToString());
                node.Parent.Nodes.Remove(node);
            }
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                Tag head = EK.Capture.Dicom.DicomToolKit.Tag.Head(node.Name.Substring(0, node.Name.IndexOf(' ')));
                Element element = dicom[head.ToString()];
                string text = "Not implemented!";
                if (element.VR != "SQ")
                {
                    if (element.Value is Array)
                    {
                        if (element.VR == "OB" || element.VR == "UN")
                        {
                            text = DicomObject.ToText((byte[])element.Value);
                        }
                        else if (element.VR == "OW")
                        {
                            short[] source = (short[])element.Value;
                            int count = source.Length * sizeof(short);
                            byte[] bytes = new byte[count];
                            Buffer.BlockCopy((short[])element.Value, 0, bytes, 0, count);
                            text = DicomObject.ToText(bytes);
                        }
                        else
                        {
                            text = String.Empty;
                            foreach (object value in element.Value as Array)
                            {
                                if (text.Length > 0)
                                {
                                    text += @"\";
                                }
                                text += value.ToString().TrimEnd();
                            }
                        }
                    }
                    else
                    {
                        text = element.Value.ToString();
                    }
                }
                Clipboard.SetText(text);
            }
        }

        private void TreeView_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                // Select the clicked node
                TreeView.SelectedNode = TreeView.GetNodeAt(e.X, e.Y);
                if(TreeView.SelectedNode != null)
                {
                    TagContextMenuStrip.Show(TreeView, e.Location);
                }
            }
        }

        private void ExpandAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                node.ExpandAll();
            }
        }

        private void ViewBinaryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                MemoryStream stream = new MemoryStream();

                string key = Key(node.Name);
                if (key != String.Empty)
                {
                    Element element = dicom[key];

                    if (node.Name.Contains(" frame"))
                    {
                        PixelData pixeldata = element as PixelData;
                        int frame = Int32.Parse(node.Name.Substring(node.Name.IndexOf(' ')).Replace("frame", ""));
                        element = new Element(t.PixelData, pixeldata.Frames[frame]);
                    }

                    element.Write(stream, dicom.TransferSyntaxUID, SpecificCharacterSet.Default, new DataSetOptions());
                }
                else
                {
                    dicom.Write(stream);
                }

                HexControl.Bytes = stream.ToArray();
                HexControl.Location = node.Bounds.Location;
                if (node.Bounds.Location.Y + HexControl.Height > this.Height)
                {
                    HexControl.Location = new Point(HexControl.Location.X, HexControl.Location.Y - HexControl.Height);
                }
                HexControl.Visible = true;
            }
        }

        private void HexControl_Leave(object sender, EventArgs e)
        {
            HexControl.Visible = false;
        }

        public void DeIdentify()
        {
            try
            {
                filename = this.Text = String.Empty;
                dicom.DeIdentify();
                SetTitles();
                FillTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public bool VerifyIOD()
        {
            bool result = true;
            try
            {
                filename = this.Text = String.Empty;
                result = Iod.Verify(dicom.Elements);
                SetTitles();
                FillTreeView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
            return result;
        }


        private List<TreeNode> CollectNodes()
        {
            List<TreeNode> nodes = new List<TreeNode>();
            TreeNode tops = TreeView.Nodes[0];
            nodes.Add(tops);
            CollectNodes(tops, nodes);
            return nodes;
        }

        private void CollectNodes(TreeNode start, List<TreeNode> collection)
        {
            foreach(TreeNode node in start.Nodes)
            {
                if (node.Text != "Item")
                {
                    collection.Add(node);
                }
                CollectNodes(node, collection);
            }
        }

        public void FindNext(string text, bool forward)
        {
            try
            {
                List<TreeNode> nodes = CollectNodes();

                TreeNode current = this.TreeView.SelectedNode;
                if (current == null)
                {
                    current = TreeView.TopNode.FirstNode;
                }
                int n;
                for (n = 0; n < nodes.Count; n++)
                {
                    if (current == nodes[n])
                    {
                        n += (forward) ? 1 : -1;
                        if (n < 0)
                        {
                            n = nodes.Count - 1;
                        }
                        else if (n >= nodes.Count)
                        {
                            n = 0;
                        }
                        break;
                    }
                }
                if (n == nodes.Count)
                {
                    return;
                }
                string search = text.ToLower();

                bool looped = false;

            restart:
                if (forward)
                {
                    for (; n < nodes.Count; n++)
                    {
                        if (nodes[n].Text.ToLower().Contains(search))
                        {
                            TreeView.SelectedNode = nodes[n];
                            break;
                        }
                    }
                    if (n >= nodes.Count)
                    {
                        n = 0;
                        if (!looped)
                        {
                            looped = true;
                            goto restart;
                        }
                    }
                }
                else
                {
                    for (; n >= 0; n--)
                    {
                        if (nodes[n].Text.ToLower().Contains(search))
                        {
                            TreeView.SelectedNode = nodes[n];
                            break;
                        }
                    }
                    if (n < 0)
                    {
                        n = nodes.Count - 1;
                        if (!looped)
                        {
                            looped = true;
                            goto restart;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public void Find()
        {
            try
            {
                FindForm find = new FindForm(this);
                //if (DialogResult.OK == find.ShowDialog(this))
                //{
                //   FindNext(find.FindText, find.Forward);
                //}
                find.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        private void FindToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Find();
        }

        private void ImportValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                if (IsLongText(node) || IsBinary(node))
                {
                    OpenFileDialog dialog = new OpenFileDialog();
                    dialog.DefaultExt = "*";
                    dialog.Filter = "All files|*.*";
                    dialog.Multiselect = false;
                    dialog.ShowDialog();
                    if (dialog.FileNames.Length != 0)
                    {
                        string key = Key(node.Name);
                        Element element = dicom[key];
                        element.ReadValueFromStream(dialog.FileNames[0]);
                        if (node.FirstNode != null)
                        {
                            node.FirstNode.Remove();
                        }
                        FillElement(element, node);
                    }
                }
            }
        }

        private void ExportValueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TreeNode node = this.TreeView.SelectedNode;
            if (node != null)
            {
                if (IsExportable(node))
                {
                    string key = Key(node.Name);
                    Element element = dicom[key];
                    string description = element.Description.Replace(" ", "");

                    if (node.Name.Contains(" frame"))
                    {
                        PixelData pixeldata = element as PixelData;
                        int frame = Int32.Parse(node.Name.Substring(node.Name.IndexOf(' ')).Replace("frame", ""));
                        description = String.Format("Frame{0}", frame);
                        element = new Element(t.PixelData, pixeldata.Frames[frame]);
                    }

                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.RestoreDirectory = true;
                    dialog.AddExtension = true;
                    dialog.Filter = "All files|*.*";
                    dialog.FileName = description;
                    dialog.ShowDialog();
                    if (dialog.FileNames.Length != 0)
                    {
                        element.WriteValueOnStream(dialog.FileNames[0]);
                    }
                }
            }

        }

    }
}