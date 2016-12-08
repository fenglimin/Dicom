using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest.Service;
using SynapticEffect.Forms;

namespace ExtendedListTest.CustomControl
{
    public partial class ucDoseReport : UserControl, IElementsBase
    {
	    private ReceivedDicomElements receivedDicomElements;
        private string lastError;

        public class DoseDataItem
        {
            public string Tag;
            public string RelationshipType;
            public string ValueType;
            public string ConceptNameCodeValue;
            public string ConceptNameCodingSchemeDesignator;
            public string ConceptNameCodeMeaning;
            public string ConceptCodeValue;
            public string ConceptCodingSchemeDesignator;
            public string ConceptCodeMeaning;
            public string Units;
        };


		public ucDoseReport(ReceivedDicomElements receivedDicomElements)
        {
            InitializeComponent();

		    try
		    {
                this.receivedDicomElements = receivedDicomElements;
                LoadTagList(receivedDicomElements.Elements);
		    }
		    catch (Exception ex)
		    {
		        lastError = ex.Message;
		    }
        }

        private void LoadTagList(EK.Capture.Dicom.DicomToolKit.DataSet elements)
        {
            try
            {
                foreach (Element element in elements)
                {
                    // do not show group length tags
                    if (element.element == 0)
                        continue;

                    var node = CreateSimpleNode(element);
                    doseReportTreeList.Nodes.Add(node);
                    AddChild(element, node);
                    node.ExpandAll();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show((ex.Message));
            }
        }

        private TreeListNode CreateSimpleNode(Element element)
        {
            var node = new TreeListNode();
            node.Text = element.Description;
            node.Tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(element.Group, element.element);
            node.Key = element.GetPath();

            node.SubItems.Add(element.VR.ToString());
            node.SubItems.Add(GetElementValue(element));
            node.SubItems.Add(string.Empty);
            node.SubItems.Add(string.Empty);
            node.SubItems.Add(string.Empty);
            node.SubItems.Add(string.Empty);
            node.SubItems.Add(string.Empty);
            node.SubItems.Add(string.Empty);
            

            return node;
        }

        private void AddChild(Element element, TreeListNode parentNode)
        {
            if (element is Sequence)
            {
                int count = ((Sequence)element).Items.Count;
                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(element.Group, element.element);

                if (tag == "(0040,a043)" || tag == "(0040,a170)")                
                {
                    var data = CreateDataFromNode(parentNode);
                    FillConceptNameCodeSequence(element, ref data);
                    UpdateNodeFromData(data, ref parentNode);
                    return;
                }

                for (int n = 0; n < count; n++)
                {
                    Elements seqItems = ((Sequence)element).Items[n];

                    if (IsContentNode(seqItems))
                    {
                        var childNode = FillContentNode(seqItems);
                        parentNode.Nodes.Add(childNode);
                    }
                    else
                    {
                        var text = string.Format("{0} {1}", element.Description, n + 1);
                        var seqNode = new TreeListNode();
                        seqNode.Text = text;

                        parentNode.Nodes.Add(seqNode);

                        foreach (Element child in seqItems)
                        {
                            var childNode = CreateSimpleNode(child);
                            seqNode.Nodes.Add(childNode);
                            AddChild(child, childNode);
                        }
                    }
                }
            }
            else if (element is PixelData)
            {
                //if (((PixelData)element).IsEncapsulated)
                //{
                //	int count = ((PixelData)element).Frames.Count;
                //	for (int n = 0; n < count; n++)
                //	{
                //		TreeNode frameNode = node.Nodes.Add(element.GetPath() + " frame" + n.ToString(), "Frame");
                //		string text = String.Format("{0} byte(s).", ((PixelData)element).Frames[n].Length);
                //		frameNode.Nodes.Add(element.GetPath() + " value", text);
                //	}
                //}
                //else
                //{
                //	FillValue(element, node);
                //}
            }
            else
            {
                //var childNode = CreateNode(element);
                //parentNode.Nodes.Add(childNode);
            }
        }

        private bool IsContentNode(Elements elements)
        {
            foreach (Element child in elements)
            {
                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(child.Group, child.element);
                if (tag == "(0040,a010)" || tag == "(0008,0100)")
                    return true;
            }

            return false;
        }

        private TreeListNode CreateNodeFromData(DoseDataItem doseDataItem)
        {
            var node = new TreeListNode();
            FillNodeFromData(doseDataItem, ref node);
            return node;
        }

        private void FillNodeFromData(DoseDataItem doseDataItem, ref TreeListNode node)
        {
            node.Text = doseDataItem.ConceptNameCodeMeaning;
            node.SubItems.Add(doseDataItem.ValueType);
            node.SubItems.Add(doseDataItem.ConceptCodeMeaning);
            node.SubItems.Add(doseDataItem.Units); 
            node.SubItems.Add(doseDataItem.ConceptNameCodeValue);
            node.SubItems.Add(doseDataItem.ConceptNameCodingSchemeDesignator);
            node.SubItems.Add(doseDataItem.ConceptNameCodeMeaning);
            node.SubItems.Add(doseDataItem.ConceptCodeValue);
            node.SubItems.Add(doseDataItem.ConceptCodingSchemeDesignator);
            

            if (node.Text == string.Empty)
                node.Text = "Get Error!";
        }

        private void UpdateNodeFromData(DoseDataItem doseDataItem, ref TreeListNode node)
        {
            node.Text = doseDataItem.ConceptNameCodeMeaning;
            node.SubItems[0].Text = doseDataItem.ValueType;
            node.SubItems[1].Text = doseDataItem.ConceptCodeMeaning;
            node.SubItems[2].Text = doseDataItem.Units;
            node.SubItems[3].Text = doseDataItem.ConceptNameCodeValue;
            node.SubItems[4].Text = doseDataItem.ConceptNameCodingSchemeDesignator;
            node.SubItems[5].Text = doseDataItem.ConceptNameCodeMeaning;
            node.SubItems[6].Text = doseDataItem.ConceptCodeValue;
            node.SubItems[7].Text = doseDataItem.ConceptCodingSchemeDesignator;
        }

        private DoseDataItem CreateDataFromNode(TreeListNode node)
        {
            var doseDataItem = new DoseDataItem();
            doseDataItem.ValueType = node.SubItems[0].Text;
            doseDataItem.ConceptCodeMeaning = node.SubItems[1].Text;
            doseDataItem.Units = node.SubItems[2].Text;
            doseDataItem.ConceptNameCodeValue = node.SubItems[3].Text;
            doseDataItem.ConceptNameCodingSchemeDesignator = node.SubItems[4].Text;
            doseDataItem.ConceptNameCodeMeaning = node.SubItems[5].Text;
            doseDataItem.ConceptCodeValue = node.SubItems[6].Text;
            doseDataItem.ConceptCodingSchemeDesignator = node.SubItems[7].Text;


            return doseDataItem;
        }

        private TreeListNode FillContentNode(Elements elements)
        {
            var content = new DoseDataItem();
            var node = new TreeListNode();

            foreach (Element element in elements)
            {
                // do not show group length tags
                if (element.element == 0)
                    continue;


                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(element.Group, element.element);
                if (tag == "(0040,a010)")
                {
                    content.RelationshipType = GetElementValue(element);
                }
                else if (tag == "(0040,a040)")
                {
                    content.ValueType = GetElementValue(element);
                }
                else if (tag == "(0040,a043)")
                {
                    FillConceptNameCodeSequence(element, ref content);
                }
                else if (tag == "(0040,a168)")
                {
                    FillConceptCodeSequence(element, ref content);
                }
                else if (tag == "(0040,a300)")
                {
                    FillMeasuredValueSequence(element, ref content);
                }
                else if (tag == "(0040,a124)" || tag == "(0040,a160)")
                {
                    content.ConceptCodeMeaning = GetElementValue(element);
                }
                else
                {
                    AddChild(element, node);
                }
            }

            FillNodeFromData(content, ref node);
            return node;
        }

        private void FillConceptNameCodeSequence(Element element, ref DoseDataItem content)
        {
            Elements item = ((Sequence)element).Items[0];
            foreach (Element child in item)
            {
                // do not show group length tags
                if (child.element == 0)
                    continue;

                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(child.Group, child.element);
                var value = GetElementValue(child);
                if (tag == "(0008,0100)")
                {
                    content.ConceptNameCodeValue = value;
                }
                else if (tag == "(0008,0102)")
                {
                    content.ConceptNameCodingSchemeDesignator = value;
                }
                else if (tag == "(0008,0104)")
                {
                    content.ConceptNameCodeMeaning = value;
                }
                else
                {
                    MessageBox.Show("Unknown tag in FillConceptNameCodeSequence()!");
                }
            }
        }

        private void FillConceptCodeSequence(Element element, ref DoseDataItem content)
        {
            Elements item = ((Sequence)element).Items[0];
            foreach (Element child in item)
            {
                // do not show group length tags
                if (child.element == 0)
                    continue;

                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(child.Group, child.element);
                var value = GetElementValue(child);
                if (tag == "(0008,0100)")
                {
                    content.ConceptCodeValue = value;
                }
                else if (tag == "(0008,0102)")
                {
                    content.ConceptCodingSchemeDesignator = value;
                }
                else if (tag == "(0008,0104)")
                {
                    content.ConceptCodeMeaning = value;
                }
                else
                {
                    MessageBox.Show("Unknown tag in FillConceptCodeSequence()!");
                }
            }
        }

        private void FillMeasuredValueSequence(Element element, ref DoseDataItem content)
        {
            Elements item = ((Sequence)element).Items[0];
            foreach (Element child in item)
            {
                // do not show group length tags
                if (child.element == 0)
                    continue;

                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(child.Group, child.element);
                var value = GetElementValue(child);
                if (tag == "(0040,08ea)")
                {
                    FillMeasuredUnitsValueSequence(child, ref content);
                }
                else if (tag == "(0040,a30a)")
                {
                    content.ConceptCodeMeaning = value;
                }
                else
                {
                    MessageBox.Show("Unknown tag in FillMeasuredValueSequence()!");
                }
            }
        }


        private void FillMeasuredUnitsValueSequence(Element element, ref DoseDataItem content)
        {
            Elements item = ((Sequence)element).Items[0];
            foreach (Element child in item)
            {
                // do not show group length tags
                if (child.element == 0)
                    continue;

                var tag = EK.Capture.Dicom.DicomToolKit.Tag.ToString(child.Group, child.element);
                var value = GetElementValue(child);
                if (tag == "(0008,0104)")
                {
                    content.Units = value;
                    return;
                }
            }
        }

        private string GetElementValue(Element element)
        {
            try
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

                    return text.ToString();
                }
                else
                {
                    object value = element.Value;
                    return (value == null) ? String.Empty : value.ToString();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return string.Empty;
            }

        }

	    public ReceivedDicomElements GetReceivedDicomElements()
	    {
		    return receivedDicomElements;
	    }

        public string GetLastError()
        {
            return lastError;
        }
    }
}
