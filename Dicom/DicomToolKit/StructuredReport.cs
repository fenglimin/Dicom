using System;
using System.Collections.Generic;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit
{
    public class ValueType
    {
        public const string Code = "CODE";
        public const string Numeric = "NUM";
        public const string Text = "TEXT";
        public const string PersonName = "PNAME";
        public const string Date = "DATE";
        public const string Time = "TIME";
        public const string DateTime = "DATETIME";
        public const string UIDRef = "UIDREF";
        public const string Image = "IMAGE";
        public const string Waveform = "WAVEFORM";
        public const string SpatialCoordinates = "SCOORD";
        public const string TemporalCoordinates = "TCOORD";
        public const string Composite = "COMPOSITE";
        public const string Container = "CONTAINER";
    }

    public class RelationshipType
    {
        public const string Contains = "CONTAINS";
        public const string HasProperties = "HAS PROPERTIES";
        public const string InferredFrom = "INFERRED FROM";
        public const string SelectedFrom = "SELECTED FROM";
        public const string HasObservationContext = "HAS OBS CONTEXT";
        public const string HasAcquisitionContext = "HAS ACQ CONTEXT";
        public const string HasConceptModifier = "HAS CONCEPT MOD";
    }

    public class ContinuityOfContent
    {
        public const string Separate = "SEPARATE";
        public const string Continuous = "CONTINUOS";
    }

    public class Laterality
    {
        public static CodedEntry Left { get { return new CodedEntry("T-04030", "SNM3", "Left breast"); } }
        public static CodedEntry Right { get { return new CodedEntry("T-04020", "SNM3", "Right breast"); } }
    }

    public class ViewForMammography
    {
        public static CodedEntry ML { get { return new CodedEntry("R-10224", "SRT", "medio-lateral"); } }
        public static CodedEntry MLO { get { return new CodedEntry("R-10226", "SRT", "medio-lateral oblique"); } }
        public static CodedEntry LM { get { return new CodedEntry("R-10228", "SRT", "latero-medial"); } }
        public static CodedEntry LMO { get { return new CodedEntry("R-10230", "SRT", "latero-medial oblique"); } }
        public static CodedEntry CC { get { return new CodedEntry("R-10242", "SRT", "cranio-caudal"); } }
        public static CodedEntry FB { get { return new CodedEntry("R-10244", "SRT", "caudo-cranial (from below)"); } }
        public static CodedEntry SIO { get { return new CodedEntry("R-102D0", "SRT", "superolateral to inferomedial oblique"); } }
        public static CodedEntry ISO { get { return new CodedEntry("R-40AAA", "SRT", "inferomedial to superolateral oblique"); } }
        public static CodedEntry XCCL { get { return new CodedEntry("R-1024A", "SRT", "cranio-caudal exaggerated laterally"); } }
        public static CodedEntry XCCM { get { return new CodedEntry("R-1024B", "SRT", "cranio-caudal exaggerated medially"); } }
        public static CodedEntry SPECIMEN { get { return new CodedEntry("G-8310", "SRT", "tissue specimen from breast"); } } 
    }

    public class CodedEntry
    {
        private Elements elements;

        public CodedEntry()
        {
            elements = new Elements();
        }

        public CodedEntry(Elements elements)
        {
            this.elements = elements;
        }

        public CodedEntry(string value, string designator, string meaning)
            : this(value, designator, meaning, String.Empty)
        {
        }

        public CodedEntry(string value, string designator, string meaning, string version) 
            : this()
        {
            CodeValue = value;
            CodeMeaning = meaning;
            CodingSchemeDesignator = designator;
            if (version != null && version != String.Empty)
            {
                CodingSchemeVersion = version;
            }
        }

        public Elements Elements
        {
            get
            {
                return elements;
            }
        }

        public string CodeValue
        {
            get
            {
                string value = String.Empty;
                if (elements.ValueExists(t.CodeValue))
                {
                    value = (string)elements[t.CodeValue].Value;
                }
                return value;
            }
            set
            {
                elements.Set(t.CodeValue, value);
            }
        }

        public string CodeMeaning
        {
            get
            {
                string meaning = String.Empty;
                if (elements.ValueExists(t.CodeMeaning))
                {
                    meaning = (string)elements[t.CodeMeaning].Value;
                }
                return meaning;
            }
            set
            {
                elements.Set(t.CodeMeaning, value);
            }
        }

        public string CodingSchemeDesignator
        {
            get
            {
                string designator = String.Empty;
                if (elements.ValueExists(t.CodingSchemeDesignator))
                {
                    designator = (string)elements[t.CodingSchemeDesignator].Value;
                }
                return designator;
            }
            set
            {
                elements.Set(t.CodingSchemeDesignator, value);
            }
        }

        public string CodingSchemeVersion
        {
            get
            {
                string version = String.Empty;
                if (elements.ValueExists(t.CodingSchemeVersion))
                {
                    version = (string)elements[t.CodingSchemeVersion].Value;
                }
                return version;
            }
            set
            {
                elements.Set(t.CodingSchemeVersion, value);
            }
        }

        public override string ToString()
        {
            return string.Format("({0},{1},\"{2}\")", CodeValue, CodingSchemeDesignator, CodeMeaning);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        /// <remarks>Compares CodingSchemeDesignator only.</remarks>
        public override bool Equals(object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }
            // If parameter cannot be cast to CodedEntry return false.
            CodedEntry entry = obj as CodedEntry;
            if ((System.Object)entry == null)
            {
                return false;
            }

            // compares code value, coding scheme designator only
            return (CodeValue == entry.CodeValue && CodingSchemeDesignator == entry.CodingSchemeDesignator);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return CodeValue.GetHashCode() * 251 + CodingSchemeDesignator.GetHashCode();
        }
    }

    public class Node : IEnumerable<Node>
    {
        internal Node parent = null;
        protected List<Node> children;
        protected Elements elements;

        public Node(Elements elements)
        {
            this.elements = elements;
            children = new List<Node>();
        }

        public Node(CodedEntry name, string type)
        {
            this.elements = new Elements();
            children = new List<Node>();

            Element element = elements.Add(t.ValueType, type);
            if (name != null)
            {
                Name = name;
            }
        }

        public Elements Elements
        {
            get
            {
                return elements;
            }
        }

        public CodedEntry Name
        {
            get
            {
                CodedEntry name = null;
                if (elements.Contains(t.ConceptNameCodeSequence))
                {
                    name = new CodedEntry(((Sequence)elements[t.ConceptNameCodeSequence]).Items[0]);
                }
                return name;
            }
            set
            {
                Sequence sequence = null;
                if (!elements.Contains(t.ConceptNameCodeSequence))
                {
                    sequence = (Sequence)elements.Add(new Sequence(t.ConceptNameCodeSequence));
                    sequence.AddItem(value.Elements);
                }
                else
                {
                    sequence = elements[t.ConceptNameCodeSequence] as Sequence;
                    sequence.ReplaceItem(value.Elements, 0);
                }
            }
        }

        public Node Parent
        {
            get
            {
                return parent;
            }
            internal set
            {
                parent = value;
            }
        }

        public Node FirstChild
        {
            get
            {
                Node child = null;
                if(children.Count > 0)
                {
                    child = children[0];
                }
                return child;
            }
        }

        public string Identifier
        {
            get
            {
                string identifier = String.Empty;
                Node sibling = this;
                Node temp = null;
                do
                {
                    int count = 1;
                    while (sibling.PreviousSibling != null)
                    {
                        count++;
                        sibling = sibling.PreviousSibling;
                    }
                    if (identifier.Length > 0)
                    {
                        identifier = "." + identifier;
                    }
                    identifier += string.Format("{0}{1}", count.ToString(), identifier);
                    temp = sibling.Parent;
                } while(temp != null);
                return identifier;
            }
        }

        public Node LastChild
        {
            get
            {
                Node child = null;
                if (children.Count > 0)
                {
                    child = children[children.Count - 1];
                }
                return child;
            }
        }

        public Node PreviousSibling
        {
            get
            {
                Node child = null;
                if (parent != null)
                {
                    int index = Index;
                    if (index > 0)
                    {
                        child = parent[index - 1];
                    }
                }
                return child;
            }
        }

        public Node NextSibling
        {
            get
            {
                Node child = null;
                if (parent != null)
                {
                    int index = Index;
                    if (index < parent.children.Count)
                    {
                        child =  parent[index + 1];
                    }
                }
                return child;
            }
        }

        public Node this[int index]
        {
            get
            {
                return children[index];
            }
        }

        internal int Index
        {
            get
            {
                if (parent == null)
                    return 0;
                int n = 0;
                foreach (Node child in parent)
                {
                    if (child.Equals(this))
                    {
                        break;
                    }
                    n++;
                }
                return n;
            }
        }

        public virtual Object Value
        {
            get
            {
                throw new Exception("Value should be overridden in derived classes.");
            }
            set
            {
                throw new Exception("Value should be overridden in derived classes.");
            }
        }

        public string Type
        {
            get
            {
                if (elements.Contains(t.ValueType))
                {
                    return (string)elements[t.ValueType].Value;
                }
                // TODO why did I pick this default
                return "Code";
            }
            internal set
            {
                elements.Set(t.ValueType, value);
            }
        }

        public string RelationshipType
        {
            get
            {
                string type = string.Empty;
                if (elements.Contains(t.RelationshipType))
                {
                    type = (string)elements[t.RelationshipType].Value;
                }
                return type;
            }
        }

        public int Depth
        {
            get
            {
                int depth = 0;
                Node temp = parent;
                while (temp != null)
                {
                    depth++;
                    temp = temp.parent;
                }
                return depth;
            }
        }

        public void Insert(int index, Node node, string relationship)
        {
            node.Parent = this;
            node.Elements.Set(t.RelationshipType, relationship);
            GetSequence(t.ContentSequence).InsertItem(node.Elements, index);
            children.Insert(index, node);
        }

        public void Add(Node node, string relationship)
        {
            node.Parent = this;
            node.Elements.Set(t.RelationshipType, relationship);
            GetSequence(t.ContentSequence).AddItem(node.Elements);
            children.Add(node);
        }

        internal static Node Factory(Elements elements)
        {
            Node node = null;
            if (elements.Contains(t.ValueType))
            {
                string type = (string)elements[t.ValueType].Value;
                switch (type)
                {
                    case ValueType.Code:
                        node = new CodeNode(elements);
                        break;
                    case ValueType.Numeric:
                        node = new NumericNode(elements);
                        break;
                    case ValueType.Text:
                        node = new TextNode(elements);
                        break;
                    case ValueType.PersonName:
                        node = new PersonNameNode(elements);
                        break;
                    case ValueType.Date:
                        node = new DateNode(elements);
                        break;
                    case ValueType.Time:
                        node = new TimeNode(elements);
                        break;
                    case ValueType.DateTime:
                        node = new DateTimeNode(elements);
                        break;
                    case ValueType.UIDRef:
                        node = new UIDNode(elements);
                        break;
                    case ValueType.Container:
                        node = new ContainerNode(elements);
                        break;
                    case ValueType.Image:
                    case ValueType.Waveform:
                    case ValueType.SpatialCoordinates:
                    case ValueType.TemporalCoordinates:
                    case ValueType.Composite:
                        node = new Node(elements);
                        break;
                    default:
                        break;
                }
            }
            else if (elements.Contains(t.ReferencedContentItemIdentifier))
            {
                string type = (string)elements[t.RelationshipType].Value;
                switch(type)
                {
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.HasProperties:
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.HasObservationContext:
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext:
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.HasConceptModifier:
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.SelectedFrom:
                    case EK.Capture.Dicom.DicomToolKit.RelationshipType.InferredFrom:
                        node = new ReferenceNode(elements);
                        break;
                    default:
                        break;
                }
            }
            return node;
        }

        public static Node Parse(Elements elements)
        {
            Node node = Node.Factory(elements);
            return Parse(node);
        }

        internal static Node Parse(Node node)
        {
            int depth = 0;
            string tabs = new String(' ', depth * 2);

            object value = node.Value;
            //System.Diagnostics.Debug.WriteLine(String.Format("{0}<{1}:{2}:{3}={4}>", tabs, Reflection.GetName(typeof(RelationshipType), node.RelationshipType), Reflection.GetName(typeof(ValueType), node.Type), node.Name, (value != null) ? node.Value : (node.Type == "CONTAINER") ? (string)node.Elements[t.ContinuityOfContent].Value : ""));

            foreach (Element element in node.Elements)
            {
                if (element is Sequence)
                {
                    foreach (Elements item in ((Sequence)element).Items)
                    {
                        if (item.Contains(t.RelationshipType))
                        {
                            Node child = Node.Factory(item);
                            node.Add(child, (string)item[t.RelationshipType].Value);
                            Node.Parse(child);
                        }
                    }
                }
            }

            return node;
        }

        internal Sequence GetSequence(string path)
        {
            Tag[] tags = Tag.InternalParse(path);
            Sequence sequence = null;
            Elements item = elements;
            for (int n = 0; n < tags.Length; n++)
            {
                sequence = GetSequence(item, tags[n]);
                if (n < tags.Length - 1)
                {
                    if (sequence.Items.Count == 0)
                    {
                        sequence.NewItem();
                    }
                    item = sequence.Items[0];
                }
            }
            return sequence;
        }

        internal Sequence GetSequence(Elements item, Tag tag)
        {
            if (!item.Contains(tag.Name))
            {
                item.Add(tag.Name, null);
            }
            return item[tag.Name] as Sequence;
        }

        public IEnumerator<Node> Children
        {
            get
            {
                return GetEnumerator();
            }
        }

        #region IEnumerable<ReportNode> Members

        public IEnumerator<Node> GetEnumerator()
        {
            foreach (Node child in children)
            {
                yield return child;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Node>)this).GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0}<{1}:{2}:{3}={4}>", new String(' ', Depth * 2), Reflection.GetName(typeof(RelationshipType), RelationshipType), Reflection.GetName(typeof(ValueType), Type), Name, (Value != null) ? Value : (Type == "CONTAINER") ? (string)Elements[t.ContinuityOfContent].Value : "");
        }
    }

    public class ContainerNode : Node
    {
        public ContainerNode(CodedEntry name)
            : this(name, EK.Capture.Dicom.DicomToolKit.ContinuityOfContent.Separate)
        {
        }

        public ContainerNode(CodedEntry name, string continuity)
            : base(name, ValueType.Container)
        {
            ContinuityOfContent = continuity;
        }

        public ContainerNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                throw new Exception("A container does not have a value.");
            }
            set
            {
                throw new Exception("A container does not have a value.");
            }
        }

        public string ContinuityOfContent
        {
            get
            {
                string continuity = "SEPARATE";
                if (Elements.Contains(t.ContinuityOfContent))
                {
                    continuity = (string)Elements[t.ContinuityOfContent].Value;
                }
                return continuity;
            }
            set
            {
                Elements.Set(t.ContinuityOfContent, value);
            }
        }
    }

    public class NumericNode : Node
    {
        public NumericNode(CodedEntry name)
            : base(name, ValueType.Numeric)
        {
        }

        public NumericNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                string value = String.Empty;
                if (elements.Contains(t.MeasuredValueSequence))
                {
                    Sequence sequence = elements[t.MeasuredValueSequence] as Sequence;
                    if (sequence.Items.Count > 0)
                    {
                        Elements item = sequence.Items[0];
                        if (item.ValueExists(t.NumericValue))
                        {
                            if (item[t.NumericValue].Value is Array)
                            {
                                value = ((string[])item[t.NumericValue].Value)[0];
                            }
                            else
                            {
                                value = (string)item[t.NumericValue].Value;
                            }
                        }
                    }
                }
                return value;
            }
            set
            {
                Sequence sequence = GetSequence(t.MeasuredValueSequence);
                if (sequence.Items.Count == 0)
                {
                    Elements item = sequence.NewItem();
                }
                if (value is double || value is string)
                {
                    sequence.Items[0].Set(t.NumericValue, value.ToString());
                }
                else
                {
                    throw new Exception("NumericNode Invalid NumericValue.");
                }
            }
        }

        public CodedEntry Units
        {
            get 
            {
                CodedEntry units = null;
                if (elements.Contains(t.MeasuredValueSequence + t.MeasurementUnitsCodeSequence))
                {
                    Sequence sequence = elements[t.MeasuredValueSequence + t.MeasurementUnitsCodeSequence] as Sequence;
                    if (sequence.Items.Count > 0)
                    {
                        if (sequence.Items[0].ValueExists(t.CodeValue))
                        {
                            units = new CodedEntry(sequence.Items[0]);
                        }
                    }
                }
                return units;
            }
            set
            {
                Sequence sequence = GetSequence(t.MeasuredValueSequence + t.MeasurementUnitsCodeSequence);
                Elements item = (Elements)value.Elements.Clone();
                if (sequence.Items.Count == 0)
                {
                    sequence.AddItem(item);
                }
                else
                {
                    sequence.ReplaceItem(item, 0);
                }
            }
        }

    }

    public class TextNode : Node
    {
        public TextNode(CodedEntry name)
            : base(name, ValueType.Text)
        {
        }

        public TextNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                if (elements.Contains(t.TextValue))
                {
                    value = elements[t.TextValue].Value;
                }
                return value;
            }
            set
            {
                elements.Set(t.TextValue, value);
            }
        }
    }

    public class PersonNameNode : Node
    {
        public PersonNameNode(CodedEntry name)
            : base(name, ValueType.PersonName)
        {
        }

        public PersonNameNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                value = elements[t.PersonName].Value;
                return value;
            }
        }
    }

    public class UIDNode : Node
    {
        public UIDNode(CodedEntry name)
            : base(name, ValueType.UIDRef)
        {
        }

        public UIDNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                value = elements[t.UID].Value;
                return value;
            }
        }
    }

    public class DateNode : Node
    {
        public DateNode(CodedEntry name)
            : base(name, ValueType.Date)
        {
        }

        public DateNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                if (elements.Contains(t.Date))
                {
                    value = elements[t.Date].Value;
                }
                return value;
            }
            set
            {
                if (value is DateTime)
                {
                    elements.Set(t.Date, ((DateTime)value).ToString("yyyyMMdd"));
                }
                if (value is String)
                {
                    elements.Set(t.Date, value);
                }
                else
                {
                    throw new Exception("DateNode Invalid Value.");
                }
            }
        }
    }

    public class TimeNode : Node
    {
        public TimeNode(CodedEntry name)
            : base(name, ValueType.Time)
        {
        }

        public TimeNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                if (elements.Contains(t.Time))
                {
                    value = elements[t.Time].Value;
                }
                return value;
            }
            set
            {
                if (value is DateTime)
                {
                    elements.Set(t.Time, ((DateTime)value).ToString("HHmmss"));
                }
                if (value is String)
                {
                    elements.Set(t.Time, value);
                }
                else
                {
                    throw new Exception("TimeNode Invalid Value.");
                }
            }
        }
    }

    public class DateTimeNode : Node
    {
        public DateTimeNode(CodedEntry name)
            : base(name, ValueType.DateTime)
        {
        }

        public DateTimeNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                value = elements[t.DateTime].Value;
                return value;
            }
        }
    }

    public class CodeNode : Node
    {
        public CodeNode(CodedEntry name)
            : base(name, ValueType.Code)
        {
        }

        public CodeNode(Elements elements)
            : base(elements)
        {
        }

        public override Object Value
        {
            get
            {
                object value = null;
                if (elements.Contains(t.ConceptCodeSequence))
                {
                    Sequence sequence = elements[t.ConceptCodeSequence] as Sequence;
                    Elements item = sequence.Items[0];
                    CodedEntry code = new CodedEntry(item);
                    value = code;
                }
                return value;
            }
            set
            {
                if (!elements.Contains(t.ConceptCodeSequence))
                {
                    elements.Add(t.ConceptCodeSequence, null);
                }
                if (value is CodedEntry)
                {
                    Sequence sequence = elements[t.ConceptCodeSequence] as Sequence;
                    if (sequence.Items.Count == 0)
                    {
                        // TODO is Clone the right thing to do?
                        sequence.AddItem((Elements)(((CodedEntry)value).Elements.Clone()));
                    }
                    else
                    {
                        // TODO is Clone the right thing to do?
                        sequence.ReplaceItem((Elements)(((CodedEntry)value).Elements.Clone()), 0);
                    }
                }
                else if (value is CodeNode)
                {
                    // TODO is Clone the right thing to do?
                    ((Sequence)elements[t.ConceptCodeSequence]).AddItem((Elements)((Sequence)((CodeNode)value).Elements[t.ConceptNameCodeSequence]).Items[0].Clone());
                }
                else
                {
                    throw new Exception("CodeNode Invalid Value.");
                }
            }
        }
    }

    public class CompositeNode : Node
    {
        public CompositeNode(CodedEntry name, string @class, string instance)
            : this(name, ValueType.Composite, @class, instance)
        {
        }

        public CompositeNode(CodedEntry name, string type, string @class, string instance)
            : base(name, type)
        {
            SOPClassUID = @class;
            SOPInstanceUID = instance;
        }

        public CompositeNode(Elements elements)
            : base(elements)
        {
        }

        internal CompositeNode(CodedEntry name, string type)
            : base(name, type)
        {
        }

        public string SOPClassUID
        {
            get
            {
                string @class = null;
                if (elements.Contains(t.ReferencedSOPSequence + t.ReferencedSOPClassUID))
                {
                    @class = (string)elements[t.ReferencedSOPSequence + t.ReferencedSOPClassUID].Value;
                }
                return @class;
            }
            set
            {
                if (!elements.Contains(t.ReferencedSOPSequence))
                {
                    Sequence sequence = (Sequence)elements.Add(new Sequence(t.ReferencedSOPSequence));
                }
                if (((Sequence)elements[t.ReferencedSOPSequence]).Items.Count == 0)
                {
                    Elements item = ((Sequence)elements[t.ReferencedSOPSequence]).NewItem();
                }
                ((Sequence)elements[t.ReferencedSOPSequence]).Items[0].Set(t.ReferencedSOPClassUID, value);
            }
        }

        public string SOPInstanceUID
        {
            get
            {
                string instance = null;
                if (elements.Contains(t.ReferencedSOPSequence + t.ReferencedSOPInstanceUID))
                {
                    instance = (string)elements[t.ReferencedSOPSequence + t.ReferencedSOPInstanceUID].Value;
                }
                return instance;
            }
            set
            {
                if (!elements.Contains(t.ReferencedSOPSequence))
                {
                    Sequence sequence = (Sequence)elements.Add(new Sequence(t.ReferencedSOPSequence));
                }
                if (((Sequence)elements[t.ReferencedSOPSequence]).Items.Count == 0)
                {
                    Elements item = ((Sequence)elements[t.ReferencedSOPSequence]).NewItem();
                }
                ((Sequence)elements[t.ReferencedSOPSequence]).Items[0].Set(t.ReferencedSOPInstanceUID, value);
            }
        }
    }

    public class ImageNode : CompositeNode
    {
        public ImageNode(CodedEntry name, string @class, string instance)
            : base(name, ValueType.Image, @class, instance)
        {
        }

        public ImageNode(Elements elements)
            : base(elements)
        {
        }

        public CodedEntry ImageLaterality
        {
            get
            {
                return null;
            }
            set
            {
                if (children.Count > 1 && children[0].Name != null && children[0].Name.CodeValue == "111027")
                {
                    CodeNode laterality = FirstChild as CodeNode;
                    laterality.Value = value;
                }
                else
                {
                    CodeNode laterality = new CodeNode(new CodedEntry("111027", "DCM", "Image Laterality"));
                    laterality.Value = value;
                    Insert(0, laterality, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public CodedEntry ImageView
        {
            get
            {
                return null;
            }
            set
            {
                if (children.Count > 2 && children[1].Name != null && children[1].Name.CodeValue == "111031")
                {
                    CodeNode laterality = FirstChild as CodeNode;
                    laterality.Value = value;
                }
                else
                {
                    CodeNode laterality = new CodeNode(new CodedEntry("111031", "DCM", "Image View"));
                    laterality.Value = value;
                    Insert(1, laterality, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string PatientOrientationRow
        {
            get
            {
                return null;
            }
            set
            {
                if (children.Count > 3 && children[2].Name != null && children[2].Name.CodeValue == "111044")
                {
                    TextNode row = children[2] as TextNode;
                    row.Value = value;
                }
                else
                {
                    TextNode row = new TextNode(new CodedEntry("111044", "DCM", "Patient Orientation Row"));
                    row.Value = value;
                    Insert(2, row, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string PatientOrientationColumn
        {
            get
            {
                return null;
            }
            set
            {
                if (children.Count > 4 && children[3].Name != null && children[3].Name.CodeValue == "111043")
                {
                    TextNode row = children[3] as TextNode;
                    row.Value = value;
                }
                else
                {
                    TextNode row = new TextNode(new CodedEntry("111043", "DCM", "Patient Orientation Column"));
                    row.Value = value;
                    Insert(3, row, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string StudyDate
        {
            get
            {
                throw new Exception("StudyDate not yet implemented.");
            }
            set
            {
                if (children.Count > 5 && children[4].Name != null && children[4].Name.CodeValue == "111060")
                {
                    DateNode date = children[4] as DateNode;
                    date.Value = value;
                }
                else
                {
                    DateNode date = new DateNode(new CodedEntry("111060", "DCM", "Study Date"));
                    date.Value = value;
                    Insert(4, date, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string StudyTime
        {
            get
            {
                throw new Exception("StudyTime not yet implemented.");
            }
            set
            {
                if (children.Count > 6 && children[5].Name != null && children[5].Name.CodeValue == "111061")
                {
                    TimeNode time = children[5] as TimeNode;
                    time.Value = value;
                }
                else
                {
                    TimeNode time = new TimeNode(new CodedEntry("111061", "DCM", "Study Time"));
                    time.Value = value;
                    Insert(5, time, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string ContentDate
        {
            get
            {
                throw new Exception("ContentDate not yet implemented.");
            }
            set
            {
                if (children.Count > 7 && children[6].Name != null && children[6].Name.CodeValue == "111018")
                {
                    DateNode date = children[6] as DateNode;
                    date.Value = value;
                }
                else
                {
                    DateNode date = new DateNode(new CodedEntry("111018", "DCM", "Content Date"));
                    date.Value = value;
                    Insert(6, date, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string ContentTime
        {
            get
            {
                throw new Exception("ContentTime not yet implemented.");
            }
            set
            {
                if (children.Count > 8 && children[7].Name != null && children[7].Name.CodeValue == "111019")
                {
                    TimeNode time = children[7] as TimeNode;
                    time.Value = value;
                }
                else
                {
                    TimeNode time = new TimeNode(new CodedEntry("111019", "DCM", "Content Time"));
                    time.Value = value;
                    Insert(7, time, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string HorizontalImagerPixelSpacing
        {
            get
            {
                throw new Exception("HorizontalImagerPixelSpacing not yet implemented.");
            }
            set
            {
                if (children.Count > 9 && children[8].Name != null && children[8].Name.CodeValue == "111026")
                {
                    NumericNode number = children[8] as NumericNode;
                    number.Value = value;
                }
                else
                {
                    NumericNode number = new NumericNode(new CodedEntry("111026", "DCM", "Horizontal Imager Pixel Spacing"));
                    number.Value = value;
                    Insert(8, number, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }

        public string VerticalImagerPixelSpacing
        {
            get
            {
                throw new Exception("VerticalImagerPixelSpacing not yet implemented.");
            }
            set
            {
                if (children.Count > 10 && children[9].Name != null && children[9].Name.CodeValue == "111066")
                {
                    NumericNode number = children[9] as NumericNode;
                    number.Value = value;
                }
                else
                {
                    NumericNode number = new NumericNode(new CodedEntry("111066", "DCM", "Vertical Imager Pixel Spacing"));
                    number.Value = value;
                    Insert(9, number, EK.Capture.Dicom.DicomToolKit.RelationshipType.HasAcquisitionContext);
                }
            }
        }
    }

    public class WaveformNode : CompositeNode
    {
        public WaveformNode(CodedEntry name)
            : base(name, ValueType.Composite)
        {
        }

        public WaveformNode(Elements elements)
            : base(elements)
        {
        }
    }

    public class SpatialCoordinatesNode : Node
    {
        public SpatialCoordinatesNode(CodedEntry name)
            : base(name, ValueType.SpatialCoordinates)
        {
        }

        public SpatialCoordinatesNode(Elements elements)
            : base(elements)
        {
        }
    }

    public class TemporalCoordinatesNode : Node
    {
        public TemporalCoordinatesNode(CodedEntry name)
            : base(name, ValueType.TemporalCoordinates)
        {
        }

        public TemporalCoordinatesNode(Elements elements)
            : base(elements)
        {
        }
    }

    public class ReferenceNode : Node
    {
        public ReferenceNode(CodedEntry name)
            : base(name, ValueType.Code)
        {
        }

        public ReferenceNode(Elements elements)
            : base(elements)
        {
        }

        public string Reference
        {
            get
            {
                StringBuilder text = new StringBuilder();
                foreach (object entry in elements[t.ReferencedContentItemIdentifier].Value as Array)
                {
                    if (text.Length > 0)
                    {
                        text.Append(".");
                    }
                    text.Append(entry.ToString());
                }
                return text.ToString();
            }
        }

        public Node Node
        {
            get
            {
                return new Node(null, null);
            }
            set
            {
            }
        }

        public override string ToString()
        {
            return String.Format("{0}<{1}:{2}>", new String(' ', Depth * 2), Reflection.GetName(typeof(RelationshipType), RelationshipType), Reference);
        }
    }

    public class StructuredReport : IEnumerable<Node>
    {
        // need to be able to create a structured report from a patient/study/series ?

        private DataSet dataset = null;
        private Node root = null;

        private StructuredReport()
        {
        }

        public StructuredReport(string value, string meaning, string designator)
            : this(new CodedEntry(value, meaning, designator))
        {
        }

        public StructuredReport(CodedEntry name)
        {
            dataset = new DataSet();

            dataset.Set(t.ValueType, ValueType.Container);

            Sequence sequence = (Sequence)dataset.Add(new Sequence(t.ConceptNameCodeSequence));
            sequence.AddItem(name.Elements);

            dataset.Set(t.ContinuityOfContent, "SEPARATE");

            root = Node.Factory(dataset.Elements);
        }

        public StructuredReport(DataSet dataset)
        {
            if (dataset == null)
            {
                dataset = new DataSet();
            }
            this.dataset = dataset;
            dataset.Part10Header = true;

            Initialize();
        }

        public DataSet DataSet
        {
            get
            {
                if (dataset == null)
                {
                    dataset = new DataSet();
                    dataset.Part10Header = true;
                }
                return dataset;
            }
        }

        public static StructuredReport Read(System.IO.Stream stream)
        {
            StructuredReport report = new StructuredReport();
            long bytes = report.DataSet.Read(stream);
            report.Initialize();
            return report;
        }

        public static StructuredReport Read(string filename)
        {
            StructuredReport report = new StructuredReport();
            long bytes = report.DataSet.Read(filename);
            report.Initialize();
            return report;
        }

        public long Write(System.IO.Stream stream)
        {
            return DataSet.Write(stream);
        }

        public long Write(string filename)
        {
            return DataSet.Write(filename);
        }

        public Node Root
        {
            get
            {
                if (root == null)
                {
                    Initialize();
                }
                return root;
            }
        }

        public void Insert(int index, Node node, string relationship)
        {
            Root.Insert(index, node, relationship);
        }

        public void Add(Node node, string relationship)
        {
            Root.Add(node, relationship);
        }

        private void Initialize()
        {
            if (root == null)
            {
                root = Node.Parse(DataSet.Elements);
                if (root == null)
                {
                    throw new Exception("Not a structured report.");
                }
            }
        }

        public Node FindNode(string identifier)
        {
            Node node = null;
            string[] indexes = identifier.Split(".".ToCharArray());
            if (indexes.Length <= 0 || indexes[0] != "1")
            {
                throw new ArgumentException("Invalid identifier", "identifier");
            }
            node = Root;
            for (int n = 1; n < indexes.Length; n++)
            {
                node = node[Int32.Parse(indexes[n])-1];
            }
            return node;
        }

        #region IEnumerable<ReportNode> Members

        public IEnumerator<Node> GetEnumerator()
        {
            Initialize();
            foreach (Node child in root)
            {
                yield return child;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Node>)this).GetEnumerator();
        }

        #endregion

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();
            NodeToString(text, Root);
            return text.ToString();
        }

        internal void NodeToString(StringBuilder text, Node node)
        {
            text.Append(node.ToString());
            text.Append("\n");
            foreach (Node child in node)
            {
                NodeToString(text, child);
            }
        }
    }
}
