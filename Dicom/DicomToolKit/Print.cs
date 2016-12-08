using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

namespace EK.Capture.Dicom.DicomToolKit
{
    [Serializable]
    public class PrintObject : ISerializable
    {
        protected DataSet dicom;
        protected string instance;

        public PrintObject()
        {
            this.instance = Element.NewUid();
            dicom = new DataSet();
        }

        public PrintObject(string instance, DataSet dicom)
        {
            this.instance = instance;
            this.dicom = dicom;
        }

        public PrintObject(SerializationInfo info, StreamingContext context)
        {
            try
            {
                MemoryStream stream = (MemoryStream)info.GetValue("dicom", typeof(MemoryStream));
                stream.Seek(0, SeekOrigin.Begin);
                dicom = new DataSet();
                dicom.Read(stream);
            }
            catch
            {
            }
            instance = info.GetString("instance");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (dicom != null)
            {
                MemoryStream stream = new MemoryStream();
                dicom.Write(stream);
                info.AddValue("dicom", stream);
            }
            info.AddValue("instance", instance);
        }

        public Element this[string key]
        {
            get
            {
                if (!dicom.Contains(key))
                {
                    dicom.Add(key, null);
                }
                return dicom[key];
            }
            set
            {
                dicom[key] = value;
            }
        }

        public DataSet Dicom
        {
            get
            {
                return dicom;
            }
            internal set
            {
                dicom = value;
            }
        }

        public string Instance
        {
            get
            {
                return instance;
            }
            set
            {
                this.instance = value;
            }
        }

        public void UpdateDataSet(DataSet dicom)
        {
            // TODO figure out if this gets all elements including those in sequences
            foreach (Element element in dicom)
            {
                if (element.VR != "SQ")
                {
                    this[element.Tag.ToString()].Value = element.Value;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string Dump()
        {
            return String.Format("({0}:{1})\n{2}", GetType().Name, instance, dicom.Dump());
        }
    }

    [Serializable]
    public class FilmSession : PrintObject, ISerializable
    {
        List<FilmBox> filmboxes = new List<FilmBox>();
        List<PresentationLUT> pluts = new List<PresentationLUT>();

        public FilmSession(string instance, DataSet data)
            : base(instance, data)
        {
        }

        public FilmSession() : base(Element.NewUid(), new DataSet())
        {
            // only tags where usage is mandatory for an SCU are added

            //dicom.Add(t.SpecificCharacterSet, null);
            //dicom.Add(t.NumberofCopies, null);
            //dicom.Add(t.PrintPriority, null);
            //dicom.Add(t.MediumType, null);
            //dicom.Add(t.FilmDestination, null);
            //dicom.Add(t.FilmSessionLabel, null);
            //dicom.Add(t.MemoryAllocation, null);
            //dicom.Add(t.OwnerID, null);
        }

        public FilmSession(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int boxes = info.GetInt32("filmboxes");
            for (int box = 0; box < boxes; box++)
            {
                filmboxes.Add((FilmBox)info.GetValue("filmbox" + box.ToString(), typeof(FilmBox)));
            }
            int luts = info.GetInt32("pluts");
            for (int lut = 0; lut < luts; lut++)
            {
                pluts.Add((PresentationLUT)info.GetValue("plut" + lut.ToString(), typeof(PresentationLUT)));
            }
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("filmboxes", filmboxes.Count);
            for (int box = 0; box < filmboxes.Count; box++)
            {
                info.AddValue("filmbox" + box.ToString(), filmboxes[box]);
            }

            info.AddValue("pluts", pluts.Count);
            for (int lut = 0; lut < pluts.Count; lut++)
            {
                info.AddValue("plut" + lut.ToString(), pluts[lut]);
            }
        }

        public List<FilmBox> FilmBoxes
        {
            get
            {
                return filmboxes;
            }
        }

        public void AddFilmBox(FilmBox filmbox)
        {
            filmbox.FilmSession = instance;
            filmboxes.Add(filmbox);
        }

        public FilmBox NewFilmBox()
        {
            FilmBox filmbox = new FilmBox(instance);
            filmboxes.Add(filmbox);
            return filmbox;
        }

        public FilmBox NewFilmBox(string instance, DataSet dicom)
        {
            FilmBox filmbox = new FilmBox(instance, dicom, this.instance);
            filmboxes.Add(filmbox);
            return filmbox;
        }

        public List<PresentationLUT> PresentationLUTs
        {
            get
            {
                return pluts;
            }
        }

        internal void AddPresentationLUT(FilmBox filmbox)
        {
            filmbox.FilmSession = instance;
            filmboxes.Add(filmbox);
        }

        public PresentationLUT NewPresentationLUT()
        {
            PresentationLUT plut = new PresentationLUT(instance);
            pluts.Add(plut);
            return plut;
        }

        public PresentationLUT NewPresentationLUT(string instance, DataSet dicom)
        {
            PresentationLUT plut = new PresentationLUT(instance, dicom, this.instance);
            pluts.Add(plut);
            return plut;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder(base.Dump());
            foreach (FilmBox filmbox in filmboxes)
            {
                text.Append(filmbox.Dump());
            }
            foreach (PresentationLUT plut in pluts)
            {
                text.Append(plut.Dump());
            }
            return text.ToString();
        }
    }

    [Serializable]
    public class PresentationLUT : PrintObject, ISerializable
    {
        string session;

        public PresentationLUT(string instance, DataSet data, string session)
            : base(instance, data)
        {
            this.session = session;
        }

        public PresentationLUT(string session)
            : base(Element.NewUid(), new DataSet())
        {
            this.session = session;

            // only tags where usage is mandatory (or highly likely) for an SCU are added

            /*
            Element sequence;
            Elements item;

            sequence = new Element(t.PresentationLUTSequence, "SQ");
            item = sequence.NewItem();
                item.Add(t.LUTDescriptor, null);
                item.Add(t.LUTData, null);
            dicom.Add(sequence);
            */

            dicom.Add(t.PresentationLUTShape, null);
        }

        public PresentationLUT(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            session = info.GetString("session");
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("session", session);
        }

        public string FilmSession
        {
            get
            {
                return session;
            }
            set
            {
                session = value;
            }
        }
    }

    [Serializable]
    public class FilmBox : PrintObject, ISerializable
    {
        string session;
        List<ImageBox> imageboxes = new List<ImageBox>();
        List<Annotation> annotations = new List<Annotation>();

        public FilmBox(string instance, DataSet data, string session)
            : base(instance, data)
        {
            this.session = session;
        }

        public FilmBox(string session)
            : base(Element.NewUid(), new DataSet())
        {
            this.session = session;

            Sequence sequence;
            Elements item;

            // only tags where usage is mandatory (or highly likely) for an SCU are added

            dicom.Add(t.ImageDisplayFormat, null);

            sequence = new Sequence(t.ReferencedFilmSessionSequence);
            item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, "1.2.840.10008.5.1.1.1");
                item.Add(t.ReferencedSOPInstanceUID, this.session);
            dicom.Add(sequence);

            /*
            sequence = new Element(t.ReferencedImageBoxSequence, "SQ");
            //item = sequence.NewItem();
                //item.Add(t.ReferencedSOPClassUID, null);
                //item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(sequence);

            /*
            sequence = new Element(t.ReferencedBasicAnnotationBoxSequence, "SQ");
            item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, null);
                item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(sequence);
            */

            //dicom.Add(t.FilmOrientation, null);
            //dicom.Add(t.FilmSizeID, null);
            //dicom.Add(t.MagnificationType, null);
            //dicom.Add(t.MaxDensity, null);
            //dicom.Add(t.ConfigurationInformation, null);

            /*
            sequence = new Element(t.ReferencedPresentationLUTSequence, "SQ");
            item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, null);
                item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(sequence);
            */

            dicom.Add(t.AnnotationDisplayFormatID, null);
            //dicom.Add(t.SmoothingType, null);
            //dicom.Add(t.BorderDensity, null);
            //dicom.Add(t.EmptyImageDensity, null);
            //dicom.Add(t.MinDensity, null);
            //dicom.Add(t.MaxDensity, null);
            //dicom.Add(t.Trim, null);
            //dicom.Add(t.Illumination, null);
            //dicom.Add(t.ReflectedAmbientLight, null);
            //dicom.Add(t.RequestedResolutionID, null);
            //dicom.Add(t.ICCProfile, null);
        }

        public FilmBox(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            session = info.GetString("session");

            int boxes = info.GetInt32("imageboxes");
            for (int box = 0; box < boxes; box++)
            {
                imageboxes.Add((ImageBox)info.GetValue("imagebox" + box.ToString(), typeof(ImageBox)));
            }
            int tations = info.GetInt32("annotations");
            for (int annotation = 0; annotation < tations; annotation++)
            {
                annotations.Add((Annotation)info.GetValue("annotation" + annotation.ToString(), typeof(Annotation)));
            }
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("session", session);

            info.AddValue("imageboxes", imageboxes.Count);
            for (int box = 0; box < imageboxes.Count; box++)
            {
                info.AddValue("imagebox" + box.ToString(), imageboxes[box]);
            }

            info.AddValue("annotations", annotations.Count);
            for (int annotation = 0; annotation < annotations.Count; annotation++)
            {
                info.AddValue("annotation" + annotation.ToString(), annotations[annotation]);
            }
        }

        public string FilmSession
        {
            get
            {
                return session;
            }
            set
            {
                dicom[t.ReferencedFilmSessionSequence + t.ReferencedSOPInstanceUID].Value = value;
                session = value;
            }
        }

        public List<ImageBox> ImageBoxes
        {
            get
            {
                return imageboxes;
            }
        }

        internal void AddImageBox(ImageBox imagebox)
        {
            imagebox.FilmBox = instance;
            imageboxes.Add(imagebox);
        }

        public ImageBox NewImageBox()
        {
            ImageBox imagebox = new ImageBox(instance);
            imageboxes.Add(imagebox);

            return imagebox;
        }

        public ImageBox NewImageBox(string instance, DataSet dicom)
        {
            ImageBox imagebox = new ImageBox(instance, dicom, this.instance);
            imageboxes.Add(imagebox);



            Sequence sequence = null;
            if (!this.dicom.Contains(t.ReferencedImageBoxSequence))
            {
                sequence = new Sequence(t.ReferencedImageBoxSequence);
                this.dicom.Add(sequence);
            }
            else
            {
                sequence = this.dicom[t.ReferencedImageBoxSequence] as Sequence;
            }
            Elements item = sequence.NewItem();
            item.Add(t.ReferencedSOPClassUID, SOPClass.BasicGrayscaleImageBoxSOPClass);
            item.Add(t.ReferencedSOPInstanceUID, imagebox.Instance);

            return imagebox;
        }

        public List<Annotation> Annotations
        {
            get
            {
                return annotations;
            }
        }

        internal void AddAnnotation(Annotation annotation)
        {
            annotation.FilmBox = instance;
            annotations.Add(annotation);
        }

        public Annotation NewAnnotation()
        {
            Annotation annotation = new Annotation(instance);
            annotations.Add(annotation);

            return annotation;
        }

        public Annotation NewAnnotation(string instance, DataSet dicom)
        {
            Annotation annotation = new Annotation(instance, dicom, this.instance);
            annotations.Add(annotation);

            Sequence sequence = null;
            if (!this.dicom.Contains(t.ReferencedBasicAnnotationBoxSequence))
            {
                sequence = new Sequence(t.ReferencedBasicAnnotationBoxSequence);
                this.dicom.Add(sequence);
            }
            else
            {
                sequence = this.dicom[t.ReferencedBasicAnnotationBoxSequence] as Sequence;
            }
            Elements item = sequence.NewItem();
            item.Add(t.ReferencedSOPClassUID, SOPClass.BasicAnnotationBoxSOPClass);
            item.Add(t.ReferencedSOPInstanceUID, annotation.Instance);

            return annotation;
        }

        public ImageBox FindImageBox(int position)
        {
            foreach (ImageBox imagebox in ImageBoxes)
            {
                if (position == (ushort)imagebox[t.ImageBoxPosition].Value)
                {
                    return imagebox;
                }
            }
            return null;
        }

        public override string Dump()
        {
            StringBuilder text = new StringBuilder(base.Dump());
            foreach (ImageBox imagebox in imageboxes)
            {
                text.Append(imagebox.Dump());
            }
            foreach (Annotation annotation in annotations)
            {
                text.Append(annotation.Dump());
            }
            return text.ToString();
        }
    }

    [Serializable]
    public class Annotation : PrintObject, ISerializable
    {
        string filmbox;

        public Annotation(string instance, DataSet data, string filmbox)
            : base(instance, data)
        {
            this.filmbox = filmbox;
        }

        public Annotation(string filmbox)
            : base(String.Empty, new DataSet())
        {
            this.filmbox = filmbox;

            // only tags where usage is mandatory (or highly likely) for an SCU are added
            
            dicom.Add(t.AnnotationPosition, null);
            dicom.Add(t.TextString, null);
        }

        public Annotation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            filmbox = info.GetString("filmbox");
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("filmbox", filmbox);
        }

        public string FilmBox
        {
            get
            {
                return filmbox;
            }
            set
            {
                filmbox = value;
            }
        }
    }

    [Serializable]
    public class ImageBox : PrintObject, ISerializable
    {
        string filmbox;
        PresentationLUT plut = null;

        public ImageBox(string instance, DataSet data, string filmbox)
            : base(instance, data)
        {
            this.filmbox = filmbox;
        }

        public ImageBox(string filmbox)
            : base(String.Empty, new DataSet())
        {
            this.filmbox = filmbox;

            Sequence sequence;
            Elements item;

            // only tags where usage is mandatory for an SCU are added
            
            dicom.Add(t.ImageBoxPosition, null);

            sequence = new Sequence(t.BasicGrayscaleImageSequence);
            item = sequence.NewItem();
                item.Add(t.SamplesperPixel, null);
                item.Add(t.PhotometricInterpretation, null);
                item.Add(t.Rows, null);
                item.Add(t.Columns, null);
                item.Add(t.PixelAspectRatio, null);
                item.Add(t.BitsAllocated, null);
                item.Add(t.BitsStored, null);
                item.Add(t.HighBit, null);
                item.Add(t.PixelRepresentation, null);
                item.Add(t.PixelData, null);
            dicom.Add(sequence);

            //dicom.Add(t.Polarity, null);
            //dicom.Add(t.MagnificationType, null);
            //dicom.Add(t.SmoothingType, null);
            //dicom.Add(t.MinDensity, null);
            //dicom.Add(t.MaxDensity, null);
            //dicom.Add(t.ConfigurationInformation, null);
            //dicom.Add(t.RequestedImageSize, null);
            //dicom.Add(t.RequestedDecimateCropBehavior, null);

            /*
            sequence = new Element(t.ReferencedPresentationLUTSequence, "SQ");
            item = sequence.NewItem();
                item.Add(t.ReferencedSOPClassUID, null);
                item.Add(t.ReferencedSOPInstanceUID, null);
            dicom.Add(sequence);
            */
        }

        public ImageBox(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            filmbox = info.GetString("filmbox");
            plut = (PresentationLUT)info.GetValue("plut", typeof(PresentationLUT));
        }

        public new void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("filmbox", filmbox);
            info.AddValue("plut", plut);
        }

        public string FilmBox
        {
            get
            {
                return filmbox;
            }
            set
            {
                filmbox = value;
            }
        }

        public PresentationLUT PresentationLUT
        {
            get
            {
                return plut;
            }
            set
            {
                plut = value;
                if (value != null)
                {
                    if(!dicom.Contains(t.ReferencedPresentationLUTSequence))
                    {
                        Sequence sequence = new Sequence(t.ReferencedPresentationLUTSequence);
                        Elements item = sequence.NewItem();
                        dicom.Add(sequence);
                    }
                    if (!dicom.Contains(t.ReferencedPresentationLUTSequence + t.ReferencedSOPClassUID))
                    {
                        Elements item = ((Sequence)dicom[t.ReferencedPresentationLUTSequence]).Items[0];
                        item.Add(t.ReferencedSOPClassUID, null);
                    }
                    dicom[t.ReferencedPresentationLUTSequence + t.ReferencedSOPClassUID].Value = SOPClass.PresentationLUTSOPClass;
                    if (!dicom.Contains(t.ReferencedPresentationLUTSequence + t.ReferencedSOPInstanceUID))
                    {
                        Elements item = ((Sequence)dicom[t.ReferencedPresentationLUTSequence]).Items[0];
                        item.Add(t.ReferencedSOPInstanceUID, null);
                    }
                    dicom[t.ReferencedPresentationLUTSequence + t.ReferencedSOPInstanceUID].Value = plut.Instance;
                }
                else
                {
                    if (dicom.Contains(t.ReferencedPresentationLUTSequence))
                    {
                        Element sequence = dicom[t.ReferencedPresentationLUTSequence];
                        dicom.Remove(sequence);
                    }
                }
            }
        }
    }

    public delegate void PrinterStatusEventHandler(object sender, PrinterStatusEventArgs e);

    public class PrintServiceSCU : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string AffectedSOPClassUID;
        string AffectedSOPInstanceUID;
        ushort MessageId;

        //string StatusAffectedSOPClassUID;
        string StatusAffectedSOPInstanceUID;
        ushort StatusMessageId;

        public event PrinterStatusEventHandler PrinterStatus;

        FilmSession session = null;

        public PrintServiceSCU(string uid)
            : base(uid)
        {
        }

        public PrintServiceSCU(PrintServiceSCU other)
            : base(other)
        {
        }

        public override object Clone()
        {
            PrintServiceSCU temp = new PrintServiceSCU(this);
            temp.PrinterStatus = this.PrinterStatus;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("PrintServiceSCU.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            Logging.Log(String.Format("PrintServiceSCU.OnData: command={0:x4}", command));
            switch ((CommandType)command)
            {
                case CommandType.N_EVENT_REPORT_RQ:
                    NEventReport(control, dicom);
                    break;
                case CommandType.N_GET_RSP:
                    NGet(control, dicom);
                    break;
                case CommandType.N_SET_RSP:
                    NSet(control, dicom);
                    break;
                case CommandType.N_ACTION_RSP:
                    NAction(control, dicom);
                    break;
                case CommandType.N_CREATE_RSP:
                    NCreate(control, dicom);
                    break;
                case CommandType.N_DELETE_RSP:
                    NDelete(control, dicom);
                    break;
            }
        }

        private void NEventReport(MessageType control, DataSet dicom)
        {

            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-EVENT-REPORT-RQ");

                //StatusAffectedSOPClassUID = (string)dicom[t.AffectedSOPClassUID].Value;
                StatusAffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;
                StatusMessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-EVENT-REPORT-RQ-DATA");

                PrinterStatusEventArgs status = new PrinterStatusEventArgs(dicom);
                if (PrinterStatus != null)
                {
                    PrinterStatus(this, status);
                }

                DataSet response = new DataSet();

                response.Add(t.GroupLength(0), (uint)110);//
                response.Add(t.AffectedSOPClassUID, SOPClass.PrinterSOPClass);//
                //response.Add(t.AffectedSOPClassUID, SOPClass.StatusAffectedSOPClassUID);//
                response.Add(t.CommandField, (ushort)CommandType.N_EVENT_REPORT_RSP);//
                response.Add(t.MessageIdBeingRespondedTo, StatusMessageId);
                response.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
                response.Add(t.Status, 0);
                response.Add(t.AffectedSOPInstanceUID, StatusAffectedSOPInstanceUID);

                SendCommand("N-EVENT-REPORT-RSP", response);
                
                // do not set the completeEvent, because this is not what we are waiting for
            }
        }

        private void NGet(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-GET-RSP");
                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-GET-RSP-DATA");
                completeEvent.Set();
            }
        }

        private void NSet(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-SET-RSP");
                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-SET-RSP-DATA");
                completeEvent.Set();
            }
        }

        private void NAction(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-ACTION-RSP");
                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-ACTION-DATA");
                completeEvent.Set();
            }
        }

        private void NDelete(MessageType control, DataSet dicom)
        {
            Logging.Log("<< N-DELETE-RSP");
            completeEvent.Set();
        }

        private void NCreate(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-CREATE-RSP"); 

                AffectedSOPClassUID = (string)dicom[t.AffectedSOPClassUID].Value;
                AffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;

                if ((DataSetType)dicom[t.CommandDataSetType].Value == DataSetType.DataSetNotPresent)
                {
                    completeEvent.Set();
                }
            }
            else
            {
                Logging.Log("<< N-CREATE-RSP-DATA");

                if (AffectedSOPClassUID == SOPClass.BasicFilmSessionSOPClass)
                {
                }
                else if (AffectedSOPClassUID == SOPClass.BasicFilmBoxSOPClass)
                {
                    // find the Filmbox in the current FilmSession
                    FilmBox filmbox = FindFilmBox(AffectedSOPInstanceUID, session);
                    if(filmbox == null)
                        return;
                    
                    // apply settings sent back in the response
                    filmbox.UpdateDataSet(dicom);

                    // we need to associate the imageboxes that we have on the SCU side
                    // with the image boxes that the SCP has just created
                    AssociateImageBoxes(filmbox, dicom);

                    // we also need to associate the annotations
                    AssociateAnnotations(filmbox, dicom);
                }
                else
                {
                    Logging.Log("N-CREATE:Unknown SOP class {0}", AffectedSOPClassUID);
                    return;
                }
                completeEvent.Set();
            }
        }

        private void AssociateAnnotations(FilmBox filmbox, DataSet dicom)
        {
            if (!dicom.Contains(t.ReferencedBasicAnnotationBoxSequence))
            {
                return;
            }
            // find or create the annotation sequence on the SCU side
            Sequence parentsq = null;
            if (filmbox.Dicom.Contains(t.ReferencedBasicAnnotationBoxSequence))
            {
                parentsq = filmbox[t.ReferencedBasicAnnotationBoxSequence] as Sequence;
            }
            else
            {
                parentsq = new Sequence(t.ReferencedBasicAnnotationBoxSequence);
                filmbox.Dicom.Add(parentsq);
            }
            // get the annotations sent from the SCP
            Sequence sequence = dicom[t.ReferencedBasicAnnotationBoxSequence] as Sequence;
            // for each annotation created by the SCP
            foreach (Elements item in sequence.Items)
            {
                // get the uid created by the SCP
                string uid = (string)item[t.ReferencedSOPInstanceUID].Value;

                // find an unassigned annotation on the SCU side
                Annotation annotation = null;
                foreach (Annotation temp in filmbox.Annotations)
                {
                    if (temp.Instance == String.Empty)
                    {
                        annotation = temp;
                        break;
                    }
                }
                // if we have found an unassigned Annotation
                if (annotation != null)
                {
                    annotation.Instance = uid;
                    Elements blah = parentsq.NewItem();
                    blah.Add(t.ReferencedSOPClassUID, SOPClass.BasicAnnotationBoxSOPClass);
                    blah.Add(t.ReferencedSOPInstanceUID, annotation.Instance);
                }
                else
                {
                    throw new Exception("Mismatch between AnnotationDisplayFormatID and number of Annotations");
                }
            }
        }

        private void AssociateImageBoxes(FilmBox filmbox, DataSet dicom)
        {
            if (!dicom.Contains(t.ReferencedImageBoxSequence))
            {
                return;
            }

            // insure that all imageboxes are disassociated, this is necessary when a session is reused.
            foreach (ImageBox temp in filmbox.ImageBoxes)
            {
                temp.Instance = String.Empty;
            }

            // find or create the image box sequence on the SCU side
            Sequence parentsq = null;
            if (filmbox.Dicom.Contains(t.ReferencedImageBoxSequence))
            {
                parentsq = filmbox[t.ReferencedImageBoxSequence] as Sequence;
            }
            else
            {
                parentsq = new Sequence(t.ReferencedImageBoxSequence);
                filmbox.Dicom.Add(parentsq);
            }
            // get the image boxes sent from the SCP
            Sequence sequence = dicom[t.ReferencedImageBoxSequence] as Sequence;
            // for each imagebox created by the SCP
            foreach (Elements item in sequence.Items)
            {
                // get the uid created by the SCP
                string uid = (string)item[t.ReferencedSOPInstanceUID].Value;

                // find an unassigned ImageBox on the SCU side
                ImageBox imagebox = null;
                foreach (ImageBox temp in filmbox.ImageBoxes)
                {
                    if (temp.Instance == String.Empty)
                    {
                        imagebox = temp;
                        break;
                    }
                }
                // if we have found an unassigned ImageBox
                if (imagebox != null)
                {
                    imagebox.Instance = uid;
                    Elements blah = parentsq.NewItem();
                    blah.Add(t.ReferencedSOPClassUID, SOPClass.BasicGrayscaleImageBoxSOPClass);
                    blah.Add(t.ReferencedSOPInstanceUID, imagebox.Instance);
                }
                else
                {
                    throw new Exception("Mismatch between ImageDisplayFormat and number of ImageBoxes");
                }
            }
        }

        private FilmBox FindFilmBox(string uid, FilmSession session)
        {
            // find the Filmbox in the current FilmSession
            FilmBox filmbox = null;
            foreach (FilmBox temp in session.FilmBoxes)
            {
                if (temp.Instance == uid)
                {
                    filmbox = temp;
                    break;
                }
            }
            return filmbox;
        }

        private void CreateFilmSession()
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClass.BasicFilmSessionSOPClass);
            command.Add(t.CommandField, (ushort)CommandType.N_CREATE_RQ);
            command.Add(t.MessageId, MessageId++);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.AffectedSOPInstanceUID, session.Instance);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-CREATE-RQ", pdu);
            SendDataPdu("N-CREATE-RQ DATA", session.Dicom);

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B600", "Memory allocation not supported");

            ProcessResponse("N-CREATE-RQ (film session)", 8000, null, warnings);
        }

        private void CreatePresentationLUTs()
        {
            // N-CREATE pluts if negotiated, accepted and created
            if (session.PresentationLUTs.Count > 0)
            {
                // first find an active presentation context for the Plut
                ServiceClass service = null;
                foreach (ServiceClass temp in association.Services)
                {
                    if (temp.SOPClassUId == SOPClass.PresentationLUTSOPClass && temp.Active)
                    {
                        service = temp;
                        break;
                    }
                }
                if (service != null)
                {
                    foreach (PresentationLUT plut in session.PresentationLUTs)
                    {
                        PresentationDataPdu pdu2 = new PresentationDataPdu(service.Syntaxes[0]);
                        PresentationDataValue pdv2 = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], MessageType.LastCommand);

                        DataSet command2 = new DataSet();

                        command2.Add(t.GroupLength(0), (uint)0);
                        command2.Add(t.AffectedSOPClassUID, SOPClass.PresentationLUTSOPClass);
                        command2.Add(t.CommandField, (ushort)CommandType.N_CREATE_RQ);
                        command2.Add(t.MessageId, MessageId++);
                        command2.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
                        command2.Add(t.AffectedSOPInstanceUID, plut.Instance);

                        pdv2.Dicom = command2;
                        pdu2.Values.Add(pdv2);

                        service.SendPdu("N-CREATE-RQ", pdu2);
                        service.SendDataPdu("N-CREATE-RQ DATA", plut.Dicom);

                        Dictionary<string, string> warnings = new Dictionary<string, string>();
                        warnings.Add("B605", "Requested Min Density or Max Density outside of printer’s operating range. The printer will use its respective minimum or maximum density value instead.");

                        service.ProcessResponse("N-CREATE-RQ (Presentation Lut)", 8000, null, warnings);
                    }
                }
            }
        }

        private void CreateFilmBox(FilmBox filmbox)
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.AffectedSOPClassUID, SOPClass.BasicFilmBoxSOPClass);
            command.Add(t.CommandField, (ushort)CommandType.N_CREATE_RQ);
            command.Add(t.MessageId, MessageId++);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command.Add(t.AffectedSOPInstanceUID, filmbox.Instance);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-CREATE-RQ", pdu);
            SendDataPdu("N-CREATE-RQ DATA", filmbox.Dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("C616","There is an existing Film Box that has not been printed and N-ACTION at the Film Session level is not supported. A new Film Box will not be created when a previous Film Box has not been printed.");

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B605","Requested Min Density or Max Density outside of printer’s operating range. The printer will use its respective minimum or maximum density value instead.");

            ProcessResponse("N-CREATE-RQ (Film Box)", 8000, errors, warnings);
        }

        private void SetImageBox(ImageBox imagebox)
        {
            PresentationDataPdu pdu2 = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv2 = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command2 = new DataSet();

            command2.Add(t.GroupLength(0), (uint)0);
            command2.Add(t.RequestedSOPClassUID, SOPClass.BasicGrayscaleImageBoxSOPClass);
            command2.Add(t.CommandField, (ushort)CommandType.N_SET_RQ);
            command2.Add(t.MessageId, MessageId++);
            command2.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command2.Add(t.RequestedSOPInstanceUID, imagebox.Instance);

            pdv2.Dicom = command2;
            pdu2.Values.Add(pdv2);

            SendPdu("N-SET-RQ", pdu2);
            SendDataPdu("N-SET DATA", imagebox.Dicom);

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("C603","Image size is larger than image box size");
            errors.Add("C605","Insufficient memory in printer to store the image");
            errors.Add("C613", "Combined Print Image size is larger than the Image Box size");

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B604","Image size larger than image box size, the image has been demagnified.");
            warnings.Add("B605","Requested Min Density or Max Density outside of printer’s operating range. The printer will use its respective minimum or maximum density value instead.");
            warnings.Add("B609","Image size is larger than the Image Box size. The Image has been cropped to fit.");
            warnings.Add("B60A","Image size or Combined Print Image size is larger than the Image Box size. The Image or Combined Print Image has been decimated to fit.");

            ProcessResponse("N-SET-RQ (Image Box)", 30000, errors, warnings);

        }

        private void SetAnnotations(FilmBox filmbox)
        {
            // N-SET each annotation if negotiated and accepted
            // first find an active presentation context for the Annotations
            ServiceClass service = null;
            foreach (ServiceClass temp in association.Services)
            {
                if (temp.SOPClassUId == SOPClass.BasicAnnotationBoxSOPClass && temp.Active)
                {
                    service = temp;
                    break;
                }
            }
            if (service != null)
            {
                foreach (Annotation annotation in filmbox.Annotations)
                {
                    PresentationDataPdu pdu2 = new PresentationDataPdu(service.Syntaxes[0]);
                    PresentationDataValue pdv2 = new PresentationDataValue(service.PresentationContextId, service.Syntaxes[0], MessageType.LastCommand);

                    DataSet command2 = new DataSet();

                    command2.Add(t.GroupLength(0), (uint)0);
                    command2.Add(t.RequestedSOPClassUID, SOPClass.BasicAnnotationBoxSOPClass);
                    command2.Add(t.CommandField, (ushort)CommandType.N_SET_RQ);
                    command2.Add(t.MessageId, MessageId++);
                    command2.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
                    command2.Add(t.RequestedSOPInstanceUID, annotation.Instance);

                    pdv2.Dicom = command2;
                    pdu2.Values.Add(pdv2);

                    service.SendPdu("N-SET-RQ", pdu2);
                    service.SendDataPdu("N-SET DATA", annotation.Dicom);

                    service.ProcessResponse("N-SET-RQ (Annotation Box)", 8000, null, null);
                }
            }
        }

        private void PrintFilmBox(FilmBox filmbox)
        {
            PresentationDataPdu pdu3 = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv3 = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command3 = new DataSet();

            command3.Add(t.GroupLength(0), (uint)0);
            command3.Add(t.RequestedSOPClassUID, SOPClass.BasicFilmBoxSOPClass);
            command3.Add(t.CommandField, (ushort)CommandType.N_ACTION_RQ);
            command3.Add(t.MessageId, MessageId++);
            command3.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);
            command3.Add(t.RequestedSOPInstanceUID, filmbox.Instance);
            command3.Add(t.ActionTypeID, "1");

            pdv3.Dicom = command3;
            pdu3.Values.Add(pdv3);

            SendPdu("N-ACTION-RQ", pdu3);
            SendDataPdu("N-ACTION-RQ DATA", filmbox.Dicom);

            Logging.Log(filmbox.Dicom.Dump());

            Dictionary<string, string> errors = new Dictionary<string, string>();
            errors.Add("C602","Unable to create Print Job SOP Instance; print queue is full");
            errors.Add("C603","Image size is larger than image box size");
            errors.Add("C613","Combined Print Image size is larger than the Image Box size");

            Dictionary<string, string> warnings = new Dictionary<string, string>();
            warnings.Add("B603","Film Box SOP Instance hierarchy does not contain Image Box SOP Instances (empty page)");
            warnings.Add("B604","Image size is larger than image box size, the image has been demagnified.");
            warnings.Add("B609","Image size is larger than the Image Box size. The Image has been cropped to fit.");
            warnings.Add("B60A","Image size or Combined Print Image size is larger than the Image Box size. Image or Combined Print Image has been decimated to fit.");

            ProcessResponse("N-ACTION-RQ (Film Box)", 8000, errors, warnings);
       }

        public void Print(FilmSession session)
        {
            MessageId = 0;
            this.session = session;

            // N-CREATE Film Session
            CreateFilmSession();
            // N-CREATE Presentation LUTs
            CreatePresentationLUTs();
            foreach (FilmBox filmbox in session.FilmBoxes)
            {
                // N-CREATE Film Box
                CreateFilmBox(filmbox);
                foreach (ImageBox imagebox in filmbox.ImageBoxes)
                {
                    // N-SET Image Box
                    SetImageBox(imagebox);
                }
                // N-SET Annotations
                SetAnnotations(filmbox);
                // N-ACTION on Film Box
                PrintFilmBox(filmbox);
            }
        }

        public PrinterStatusEventArgs GetPrinterStatus()
        {
            PresentationDataPdu pdu = new PresentationDataPdu(Syntaxes[0]);
            PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet command = new DataSet();

            command.Add(t.GroupLength(0), (uint)0);
            command.Add(t.RequestedSOPClassUID, SOPClass.PrinterSOPClass);
            command.Add(t.CommandField, (ushort)CommandType.N_GET_RQ);
            command.Add(t.MessageId, 0);
            command.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
            // The Printer SOP Instance is created by the SCP during start-up of the hard copy printer and has a well-known SOP Instance UID.
            command.Add(t.RequestedSOPInstanceUID, SOPClass.PrinterSOPInstance);

            pdv.Dicom = command;
            pdu.Values.Add(pdv);

            SendPdu("N-GET-RQ", pdu);

            ProcessResponse("N-GET-RQ", 8000, null, null);

            return new PrinterStatusEventArgs(LastMessage.Dicom);
        }
    }

	public class PrintEventArgs : DicomEventArgs
    {
        private bool cancel;

        public PrintEventArgs()
        {
            this.cancel = false;
        }

        public bool Cancel
        {
            get
            {
                return cancel;
            }
            set
            {
                cancel = value;
            }
        }
    }

    public class PrintJobEventArgs : PrintEventArgs
    {
        private FilmSession session;

        public PrintJobEventArgs(FilmSession session)
        {
            this.session = session;
        }

        public FilmSession Session
        {
            get
            {
                return session;
            }
        }


    }

    public class PrintPageEventArgs : PrintEventArgs
    {
        private FilmBox page;

        public PrintPageEventArgs(FilmBox page)
        {
            this.page = page;
        }

        public FilmBox Page
        {
            get
            {
                return page;
            }
        }
    }

    public delegate void PrintJobEventHandler(object sender, PrintJobEventArgs e);
    public delegate void PrintPageEventHandler(object sender, PrintPageEventArgs e);

    public class PrintServiceSCP : ServiceClass, IPresentationDataSink
    {
        ushort command;
        string AffectedSOPClassUID;
        string AffectedSOPInstanceUID;
        string RequestedSOPClassUID;
        string RequestedSOPInstanceUID;
        ushort MessageId;
        Dictionary<string, FilmSession> sessions = new Dictionary<string, FilmSession>();
        Dictionary<string, FilmBox> filmboxes = new Dictionary<string, FilmBox>();
        Dictionary<string, ImageBox> imageboxes = new Dictionary<string, ImageBox>();
        Dictionary<string, Annotation> annotations = new Dictionary<string, Annotation>();

        public event PrintJobEventHandler JobPrinted;
        public event PrintPageEventHandler PagePrinted;

        public PrintServiceSCP(string uid)
            : base(uid)
        {
        }

        public PrintServiceSCP(PrintServiceSCP other)
            : base(other)
        {
        }

        public override object Clone()
        {
            PrintServiceSCP temp = new PrintServiceSCP(this);
            temp.JobPrinted = this.JobPrinted;
            return temp;
        }

        public void OnData(MessageType control, Message message)
        {
            Logging.Log("PrintServiceSCP.OnData");

            DataSet dicom = message.Dicom;

            if (MessageControl.IsCommand(control))
            {
                command = (ushort)dicom[t.CommandField].Value;
            }
            switch ((CommandType)command)
            {
                case CommandType.N_EVENT_REPORT_RSP:
                    NEventReport(control, dicom);
                    break;
                case CommandType.N_GET_RQ:
                    NGet(control, dicom);
                    break;
                case CommandType.N_SET_RQ:
                    NSet(control, dicom);
                    break;
                case CommandType.N_ACTION_RQ:
                    NAction(control, dicom);
                    break;
                case CommandType.N_CREATE_RQ:
                    NCreate(control, dicom);
                    break;
                case CommandType.N_DELETE_RQ:
                    NDelete(control, dicom);
                    break;
            }
        }

        private void NEventReport(MessageType control, DataSet dicom)
        {
            Logging.Log("<< N-EVENT-REPORT-RSP");
            //N-EVENT-REPORT-RSP
        }

        private void NGet(MessageType control, DataSet dicom)
        {
            Logging.Log((MessageControl.IsCommand(control)) ? "<< N-GET-RQ" : "<< N-GET-DATA");

            if (MessageControl.IsLast(control))
            {
                RequestedSOPClassUID = (string)dicom[t.RequestedSOPClassUID].Value;

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                MessageId = (ushort)dicom[t.MessageId].Value;

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, RequestedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_GET_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);//
                fragment.Add(t.Status, (ushort)0x0);
                fragment.Add(t.AffectedSOPInstanceUID, SOPClass.PrinterSOPInstance);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                //request.Dump();

                SendPdu("N-GET-RSP", request);

                pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastDataSet);
                PresentationDataPdu data = new PresentationDataPdu(syntaxes[0]);

                DataSet response = new DataSet();

                response.Add(t.Manufacturer, "Eastman Kodak");
                response.Add(t.ManufacturerModelName, "8900");
                response.Add(t.DeviceSerialNumber, "K123");
                response.Add(t.SoftwareVersions, "5.6.b18");
                response.Add(t.PrinterStatus, "WARNING");
                response.Add(t.PrinterStatusInfo, "CALIBRATION ERR");
                response.Add(t.PrinterName, "8900");

                pdv.Dicom = response;

                data.Values.Add(pdv);

                //dicom.Dump();

                SendPdu("N-GET-DATA", data);
            }
        }

        private void NSet(MessageType control, DataSet dicom)
        {
            //Basic Grayscale Image Box SOP Class

            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-SET-RQ");

                RequestedSOPClassUID = (string)dicom[t.RequestedSOPClassUID].Value;
                RequestedSOPInstanceUID = (string)dicom[t.RequestedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-SET-DATA");

                if (RequestedSOPClassUID == SOPClass.BasicGrayscaleImageBoxSOPClass)
                {
                    ImageBox imagebox = imageboxes[RequestedSOPInstanceUID];
                    imagebox.Dicom = dicom;
                }
                else
                {
                    Logging.Log("N-SET: Unknown SOP class {0}", RequestedSOPClassUID);
                    return;
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, RequestedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_SET_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);//
                fragment.Add(t.Status, (ushort)0);
                fragment.Add(t.AffectedSOPInstanceUID, RequestedSOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-SET-RSP", request);

                pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastDataSet);
                PresentationDataPdu data = new PresentationDataPdu(syntaxes[0]);

                DataSet response = new DataSet();

                response.Add(t.MagnificationType, "CUBIC");
                response.Add(t.SmoothingType, "0");
                response.Add(t.RequestedDecimateCropBehavior, "DECIMATE");

                pdv.Dicom = response;

                data.Values.Add(pdv);

                SendPdu("N-SET-RSP-DATA", data);
            }

        }

        private void NAction(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-ACTION-RQ");

                RequestedSOPClassUID = (string)dicom[t.RequestedSOPClassUID].Value;
                RequestedSOPInstanceUID = (string)dicom[t.RequestedSOPInstanceUID].Value;
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-ACTION-DATA");

                dicom.Add(t.GroupLength(2), (ulong)0);
                dicom.Add(t.FileMetaInformationVersion, new byte[] { 0, 1 });
                dicom.Add(t.MediaStorageSOPClassUID, this.SOPClassUId);
                dicom.Add(t.MediaStorageSOPInstanceUID, this.AffectedSOPInstanceUID);
                dicom.Add(t.TransferSyntaxUID, this.syntaxes[0]);
                dicom.Add(t.ImplementationClassUID, "1.2.3.4");
                dicom.Add(t.ImplementationVersionName, "not specified");

                dicom.Part10Header = true;
                dicom.TransferSyntaxUID = syntaxes[0];


                bool cancel = false;
                PrintJobEventArgs job = null;
                PrintPageEventArgs page = null;
                // if the entire film session was printed
                if (RequestedSOPClassUID == SOPClass.BasicFilmSessionSOPClass)
                {
                    FilmSession session = sessions[RequestedSOPInstanceUID];
                    // output the entire session if we have a subscriber
                    if (JobPrinted != null)
                    {
                        job = new PrintJobEventArgs(session);
	                    job.CallingAeTitle = association.CallingAeTitle;
	                    job.CallingAeIpAddress = association.CallingAeIpAddress;
						
                        JobPrinted(this, job);
                        cancel = job.Cancel;
                    }
                    // output each filmbox if we have that subscriber
                    if (!cancel && PagePrinted != null)
                    {
                        foreach (FilmBox filmbox in session.FilmBoxes)
                        {
                            page = new PrintPageEventArgs(filmbox);
                            PagePrinted(this, page);
                            // if this page is cancelled, stop notifications, i.e. 'printing'
                            if (page.Cancel)
                            {
                                break;
                            }
                        }
                    }
                }
                // if an individual page was printed
                else if (RequestedSOPClassUID == SOPClass.BasicFilmBoxSOPClass)
                {
                    // send notifications based on subscribers
                    if (JobPrinted != null)
                    {
                        FilmSession session = null;
                        foreach (KeyValuePair<string, FilmSession> kvp in sessions)
                        {
                            foreach(FilmBox filmbox in kvp.Value.FilmBoxes)
                            {
                                if(filmbox.Instance == RequestedSOPInstanceUID)
                                {
                                    session = kvp.Value;
                                    break;
                                }
                            }
                            if(session != null)
                                break;
                        }
                        if(session != null)
                        {
                            job = new PrintJobEventArgs(session);
							job.CallingAeTitle = association.CallingAeTitle;
							job.CallingAeIpAddress = association.CallingAeIpAddress;
                            JobPrinted(this, job);
                            cancel = job.Cancel;
                        }
                        else
                        {
                            Logging.Log(LogLevel.Warning, "Unable to find FilmSession");
                        }
                    }
                    if (!cancel && PagePrinted != null)
                    {
                        FilmBox filmbox = filmboxes[RequestedSOPInstanceUID];
                        page = new PrintPageEventArgs(filmbox);
                        PagePrinted(this, page);
                        cancel = page.Cancel;
                    }
                }
                if (cancel)
                {
                    Logging.Log(LogLevel.Info, "Some print operations were cancelled.");
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();

                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, RequestedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_ACTION_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);
                // report a cancellation from any of the notifications
                fragment.Add(t.Status, (int)((page != null && page.Cancel) ? 0xC000 : 0));
                fragment.Add(t.AffectedSOPInstanceUID, RequestedSOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-ACTION-RSP", request);

                sessions.Clear();
                filmboxes.Clear();
                imageboxes.Clear();
            }
        }

        private void NDelete(MessageType control, DataSet dicom)
        {
            Logging.Log("<< N-DELETE-RQ");
            //N-DELETE-RSP
        }

        private void CreateFilmSession(DataSet dicom)
        {
            FilmSession session = new FilmSession(AffectedSOPInstanceUID, dicom);
            sessions.Add(AffectedSOPInstanceUID, session);
        }

        private void CreateFilmBox(DataSet dicom)
        {
            // find the session that this filmbox will belong to
            string id = (string)dicom[t.ReferencedFilmSessionSequence + t.ReferencedSOPInstanceUID].Value;
            FilmSession session = sessions[id];

            // add a filmbox to the session
            FilmBox filmbox = session.NewFilmBox(AffectedSOPInstanceUID, dicom);
            filmboxes.Add(AffectedSOPInstanceUID, filmbox);

            // figure out how many image boxes to create
            string format = (string)dicom[t.ImageDisplayFormat].Value;
            int count = 0;
            string[] temp = format.Split(@"\,".ToCharArray());
            switch (temp[0])
            {
                case "STANDARD":
                    count = Int32.Parse(temp[1]) * Int32.Parse(temp[2]);
                    break;
                case "ROW":
                case "COL":
                    for (int m = 1; m < temp.Length; m++)
                    {
                        count += Int32.Parse(temp[m]);
                    }
                    break;
            }

            for (int n = 0; n < count; n++)
            {
                ImageBox imagebox = filmbox.NewImageBox(Element.NewUid(), null);
                imageboxes.Add(imagebox.Instance, imagebox);
            }

            if (dicom.ValueExists(t.AnnotationDisplayFormatID))
            {
                // figure out how many annotations to create
                format = (string)dicom[t.AnnotationDisplayFormatID].Value.ToString().ToUpper();
                switch (format)
                {
                    case "LABEL":               // one for each ImageBox
                        break;
                    case "COMBINED":            // one for each ImageBox plus the page
                        count = count + 1;
                        break;
                    case "BOTTOM":              // one for the page
                        count = 1;
                        break;

                }

                for (int n = 0; n < count; n++)
                {
                    Annotation annotation = filmbox.NewAnnotation(Element.NewUid(), null);
                    annotations.Add(annotation.Instance, annotation);
                }
            }
        }

        private void CreatePresentationLUT(DataSet dicom)
        {
            PresentationDataValue pdv2 = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

            DataSet fragment2 = new DataSet();

            fragment2.Add(t.GroupLength(0), (uint)0);//
            fragment2.Add(t.AffectedSOPClassUID, AffectedSOPClassUID);//
            fragment2.Add(t.CommandField, (ushort)CommandType.N_CREATE_RSP);//
            fragment2.Add(t.MessageIdBeingRespondedTo, MessageId);
            fragment2.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetNotPresent);//
            fragment2.Add(t.Status, (ushort)0);
            fragment2.Add(t.AffectedSOPInstanceUID, AffectedSOPInstanceUID);

            pdv2.Dicom = fragment2;

            PresentationDataPdu request2 = new PresentationDataPdu(syntaxes[0]);
            request2.Values.Add(pdv2);

            SendPdu("N-CREATE-RSP", request2);
        }

        private void NCreate(MessageType control, DataSet dicom)
        {
            if (MessageControl.IsCommand(control))
            {
                Logging.Log("<< N-CREATE-RQ"); 

                AffectedSOPClassUID = (string)dicom[t.AffectedSOPClassUID].Value;
                if (dicom.Contains(t.AffectedSOPInstanceUID))
                {
                    AffectedSOPInstanceUID = (string)dicom[t.AffectedSOPInstanceUID].Value;
                }
                else
                {
                    AffectedSOPInstanceUID = Element.NewUid();
                    Logging.Log(String.Format("N-CREATE-RQ: Substituting AffectedSOPInstanceUID={0} for AffectedSOPClassUID={1}", AffectedSOPInstanceUID, AffectedSOPClassUID));
                }
                MessageId = (ushort)dicom[t.MessageId].Value;
            }
            else
            {
                Logging.Log("<< N-CREATE-RQ DATA");

                switch(AffectedSOPClassUID)
                {
                    case SOPClass.BasicFilmSessionSOPClass:
                        CreateFilmSession(dicom);
                        break;
                    case SOPClass.BasicFilmBoxSOPClass:
                        CreateFilmBox(dicom);
                        break;
                    case SOPClass.PresentationLUTSOPClass:
                        CreatePresentationLUT(dicom);
                        return;
                    default:
                        Logging.Log("N-CREATE: Unknown SOP class {0}", AffectedSOPClassUID);
                        return;
                }

                PresentationDataValue pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastCommand);

                DataSet fragment = new DataSet();
                
                fragment.Add(t.GroupLength(0), (uint)0);//
                fragment.Add(t.AffectedSOPClassUID, AffectedSOPClassUID);//
                fragment.Add(t.CommandField, (ushort)CommandType.N_CREATE_RSP);//
                fragment.Add(t.MessageIdBeingRespondedTo, MessageId);
                fragment.Add(t.CommandDataSetType, (ushort)DataSetType.DataSetPresent);//
                fragment.Add(t.Status, (ushort)0);
                fragment.Add(t.AffectedSOPInstanceUID, AffectedSOPInstanceUID);

                pdv.Dicom = fragment;

                PresentationDataPdu request = new PresentationDataPdu(syntaxes[0]);
                request.Values.Add(pdv);

                SendPdu("N-CREATE-RSP", request);
                
                pdv = new PresentationDataValue(PresentationContextId, Syntaxes[0], MessageType.LastDataSet);
                PresentationDataPdu data = new PresentationDataPdu(syntaxes[0]);

                DataSet response = new DataSet();

                if (AffectedSOPClassUID == SOPClass.BasicFilmSessionSOPClass)
                {
                    Logging.Log(SOPClass.BasicFilmSessionSOPClass.ToString());
                    response.Add(t.MediumType, "BLUE FILM");
                }
                else if (AffectedSOPClassUID == SOPClass.BasicFilmBoxSOPClass)
                {
                    Logging.Log(SOPClass.BasicFilmBoxSOPClass.ToString());
                    Sequence sequence;
                    Elements item;

                    response.Add(t.FilmSizeID, "14INX17IN");
                    response.Add(t.BorderDensity, "BLACK");
                    response.Add(t.Illumination, (ushort)2000);
                    response.Add(t.ReflectedAmbientLight, (ushort)10);

                    sequence = new Sequence(t.ReferencedImageBoxSequence);
                    response.Add(sequence);
                    foreach (ImageBox imagebox in filmboxes[AffectedSOPInstanceUID].ImageBoxes)
                    {
                        item = sequence.NewItem();
                        item.Add(t.ReferencedSOPClassUID, SOPClass.BasicGrayscaleImageBoxSOPClass);
                        item.Add(t.ReferencedSOPInstanceUID, imagebox.Instance);
                    }
                    if (filmboxes[AffectedSOPInstanceUID].Annotations.Count > 0)
                    {
                        sequence = new Sequence(t.ReferencedBasicAnnotationBoxSequence);
                        response.Add(sequence);
                        foreach (Annotation annotation in filmboxes[AffectedSOPInstanceUID].Annotations)
                        {
                            item = sequence.NewItem();
                            item.Add(t.ReferencedSOPClassUID, SOPClass.BasicAnnotationBoxSOPClass);
                            item.Add(t.ReferencedSOPInstanceUID, annotation.Instance);
                        }
                    }
                }

                pdv.Dicom = response;

                data.Values.Add(pdv);

                SendPdu("N-CREATE-DATA", data);
            }
        }
    }

    /// <summary>
    /// EventArgs sent with a PrinterStatusEventHandler that contains the status.
    /// </summary>
    public class PrinterStatusEventArgs : EventArgs
    {
        private DataSet dicom;

        public PrinterStatusEventArgs(DataSet dicom)
        {
            this.dicom = dicom;
            if (!dicom.Contains(t.PrinterStatus))
            {
                PrinterStatus = "NORMAL";
            }
            if (!dicom.Contains(t.PrinterStatusInfo))
            {
                PrinterStatusInfo = "NORMAL";
            }
        }

        public DataSet DataSet
        {
            get
            {
                return dicom;
            }
        }

        public string PrinterStatus
        {
            get
            {
                return (string)dicom[t.PrinterStatus].Value;
            }
            set
            {
                dicom.Set(t.PrinterStatus, value);
            }
        }

        public string PrinterStatusInfo
        {
            get
            {
                return (string)dicom[t.PrinterStatusInfo].Value;
            }
            set
            {
                dicom.Set(t.PrinterStatusInfo, value);
            }
        }

    }

}
