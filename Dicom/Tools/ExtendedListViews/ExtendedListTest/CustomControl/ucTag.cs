using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest.Image;
using ExtendedListTest.Service;
using SynapticEffect.Forms;

namespace ExtendedListTest.CustomControl
{
	public partial class ucTag : UserControl, IElementsBase
	{
		private ReceivedDicomElements receivedDicomElements;
        private string lastError;

		public ucTag(ReceivedDicomElements receivedDicomElements)
		{
			InitializeComponent();

		    try
		    {
                tagTreeList.BeforeLabelEdit += OnBeforeLabelEdit;
                this.receivedDicomElements = receivedDicomElements;
                LoadTagList(receivedDicomElements.Elements);
		    }
		    catch (Exception ex)
		    {
		        lastError = ex.Message;
		    }
		}

        protected void OnBeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
        }

        private void LoadTagList(DataSet elements)
        {
            try
            {
                foreach (Element element in elements)
                {
                    // do not show group length tags
                    if (element.element == 0)
                        continue;

                    var node = CreateNode(element);
                    tagTreeList.Nodes.Add(node);
                    AddChild(element, node);
                    node.ExpandAll();
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private TreeListNode CreateNode(Element element)
        {
            var node = new TreeListNode();
            node.Text = EK.Capture.Dicom.DicomToolKit.Tag.ToString(element.Group, element.element);
            node.Key = element.GetPath();

            node.SubItems.Add(element.VR.ToString());
            node.SubItems.Add(element.Description);
            node.SubItems.Add(GetElementValue(element));

            return node;
        }

        private TreeListNode CreateItemNode(Element element)
        {
            var node = new TreeListNode();
            node.Text = "Item";
            node.Key = element.GetPath();

            return node;
        }

		private void AddChild(Element element, TreeListNode node)
		{
			if (element is Sequence)
			{
				int count = ((Sequence)element).Items.Count;
				for (int n = 0; n < count; n++)
				{
					Elements item = ((Sequence)element).Items[n];

                    var itemNode = CreateItemNode(element);

                    node.Nodes.Add(itemNode);
					
					foreach (Element child in item)
					{
						// do not show group length tags
						if (child.element == 0)
							continue;

                        var childNode = CreateNode(child);

						itemNode.Nodes.Add(childNode);
						AddChild(child, childNode);
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
		}

		private string GetElementValue(Element element)
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
