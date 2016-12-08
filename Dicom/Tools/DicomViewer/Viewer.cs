using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomViewer
{

    /// <summary>
    /// A structure that is used to access individual pixel color elements of 
    /// System.Drawing.Bitmap
    /// </summary>
    /// <remarks>
    /// The color pixels in an RGB Bitmap are arranged in BGR byte order
    /// </remarks>
    internal struct Format24bppRgbPixel
    {
        public byte blue;
        public byte green;
        public byte red;

        public Format24bppRgbPixel(byte red, byte green, byte blue)
        {
            this.blue = blue;
            this.green = green;
            this.red = red;
        }

        public Color Color
        {
            get
            {
                return Color.FromArgb(red, green, blue);
            }
        }
    }

    public partial class Viewer : Form
    {
        #region Fields

        /// <summary>Filename if loaded from a file, String.Empty if loaded by DataSet in memory</summary>
        private string filename = String.Empty;
        /// <summary>The original image along with it's meta-data</summary>
        private EK.Capture.Dicom.DicomToolKit.DataSet dicom = null;
        /// <summary></summary>
        private ushort[] voilut = null;
        /// <summary>The ImageAttributes of the cached image.</summary>
        private ImageAttributes attributes;

        /// <summary>Initial Point for mouse operations, null means no current operation</summary>
        private Point? start = null;
        /// <summary>Contains the previous roi, used to undraw the previous roi</summary>
        private Rectangle previous;
        /// <summary>The current roi</summary>
        private Rectangle rectangle;

        /// <summary>The current crop center</summary>
        //private Point center;
        /// <summary>The current crop zoom factor, ratio of display size to image size.</summary>
        //private double zoom = 1.0;

        /// <summary>Overlay planes read in from the DICOM DataSet</summary>
        private ushort[] overlay = null;
        /// <summary>Whether or not to display overlays.</summary>
        private bool overlays = false;
        /// <summary>optional bsm points</summary>
        private Point[] bsm = null;

        private bool invert = false;

        private bool isCtImage = false;

        private ushort pixelRepresentation = 0;

        [DllImport("user32.dll")]
        public static extern bool MessageBeep(int Sound);

        /// <summary></summary>
        public static string Filter = "Dicom Files (*.dcm)|*.dcm|Raw Files (*.raw)|*.raw|Mask Files (*.mask)|*.mask|Bit Files (*.bit)|*.bit|PGM Files (*.pgm)|*.pgm|Bitmap Files (*.bmp)|*.bmp|Jpeg Files (*.jpg)|*.jpg|Png Files (*.png)|*.png|Gif Files (*.gif)|*.gif|Tiff Files (*.tiff;*.tif)|*.tiff;*.tif|Emf Files (*.emf)|*.emf|Exif Files (*.exif)|*.exif|Wmf Files (*.wmf)|*.wmf|Filmboxes (*.xml)|*.xml|All Image Files |*.dcm;*.raw;*.mask;*.pgm;*.bmp;*.jpg;*.png;*.gif;*.tiff;*.tif;*.emf;*.exif;*.wmf;*.xml|All files|*.*";
        private Settings settings = new Settings("DicomViewer");

        #endregion

        #region Properties

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

        #endregion Properties

        #region Construction

        public Viewer()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public Viewer(string filename)
        {
            try
            {
                this.filename = filename;
                InitializeComponent();
                SetTitles();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        public Viewer(EK.Capture.Dicom.DicomToolKit.DataSet dicom)
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

        public Viewer(FilmBox page)
            : this("")
        {
            try
            {
                this.dicom = OtherImageFormats.RenderPage(page, PictureBox.Size);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            GC.Collect();
            base.Dispose(disposing);
        }

        #endregion Construction

        private void Viewer_Load(object sender, EventArgs e)
        {
            try
            {
                // we should either have a filename or dataset
                if (filename != null && filename.Length > 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        dicom = OtherImageFormats.Read(filename);
                    }
                    finally
                    {
                        SetStatus("");
                        Cursor.Current = Cursors.Default;
                    }
                }
                if (dicom == null)
                {
                    throw new ArgumentException("No filename or dataset.");
                }

                SetIsCtImage();
                SetPixelRepresentation();
                voilut = GetVOILUT(dicom.Elements);

                attributes = Utilities.GetAttributes(dicom.Elements);
                // if the image is large, lets reduce memory consumption and decimate the source
                if (attributes.buffer.Length > 20000000)
                {
                    ReduceImage(dicom.Elements);
                }

                // extract overlays, if any
                overlay = ExtractOverlays(dicom.Elements);
                overlays = (overlay != null);

                GetPicture();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }
        }

        private void SetIsCtImage()
        {
            Elements dicomElemnets = dicom.Elements;
            if (dicomElemnets != null && dicomElemnets.Contains(t.Modality) && ((string)dicomElemnets[t.Modality].Value == "CT"))
            {
                isCtImage = true;
            }
        }

        private void SetPixelRepresentation()
        {
            if (dicom != null)
            {
                Elements dicomElemnets = dicom.Elements;
                if (dicomElemnets != null && dicomElemnets.Contains(t.PixelRepresentation))
                {
                    pixelRepresentation = (ushort)dicom[t.PixelRepresentation].Value;
                    OtherImageFormats.PixelRepresentation = pixelRepresentation;
                }
            }
        }

        private ushort[] GetVOILUT(Elements dicom)
        {
            ushort[] lut = null;
            if (dicom.Contains(t.VOILUTSequence))
            {
                lut = (ushort[])dicom[t.VOILUTSequence + t.LUTData].Value;
            }
            else
            {
                if (dicom.Contains(t.WindowCenter) && dicom.Contains(t.WindowWidth) && dicom.Contains(t.BitsStored))
                {
                    double window = Double.Parse(((string[]) dicom[t.WindowWidth].Value)[0]);
                    double level = Double.Parse(((string[])dicom[t.WindowCenter].Value)[0]);
                    ushort stored = (ushort) dicom[t.BitsStored].Value;

                    if (isCtImage)
                    {
                        float rescaleIntercept = float.Parse((string)dicom[t.RescaleIntercept].Value);
                        float rescaleSlope = float.Parse((string)dicom[t.RescaleSlope].Value);
                        lut = Utilities.GetCTWindowLevelLut((int)window, (int)level, stored, pixelRepresentation, rescaleIntercept, rescaleSlope);
                    }
                    else
                    {
                        lut = Utilities.GetWindowLevelLut((int)window, (int)level, stored);
                    }
                }
            }
            return lut;
        }

        static int nint(double value)
        {
            if (value >= 0.0)
                return ((int)(value + 0.5));
            else
                return ((int)(value - 0.5));
        }

        static Point intersection(double theta1, double rho1, double theta2, double rho2)
        {
            Point point = Point.Empty;

            double dSin = Math.Sin(theta2 - theta1);

            // can't find intersection point of lines that are virtually parallel
            if (Math.Abs(dSin) < (1.0 / 4096.0))
                return point;

            point.X = nint((rho2 * Math.Cos(theta1) - rho1 * Math.Cos(theta2)) / dSin);
            point.Y = nint((rho1 * Math.Sin(theta2) - rho2 * Math.Sin(theta1)) / dSin);

            return point;
        }

        public struct Blade
        {
            public double theta;
            public double rho;
            public double diag;
        }

        public Blade GetBlade(XmlNode node)
        {
            Blade blade = new Blade();

            XmlNode theta = node.SelectSingleNode("theta");
            if (theta != null) blade.theta = Double.Parse(theta.InnerText);
            XmlNode rho = node.SelectSingleNode("rho");
            if (rho != null) blade.rho = Double.Parse(rho.InnerText);
            XmlNode diag = node.SelectSingleNode("diag");
            if (diag != null) blade.diag = Double.Parse(diag.InnerText);

            return blade;
        }

        public void ApplyBsm(string filename)
        {
            try
            {
                using (FileStream stream = new FileStream(filename, FileMode.Open))
                {
                    // get blades from xml file
                    XmlDocument document = new XmlDocument();
                    document.Load(stream);

                    // Get to the SOAP-ENV:Envelope/SOAP-ENV:Body node.
                    XmlNode body = document.DocumentElement.FirstChild;

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
                    nsmgr.AddNamespace("a1", body.FirstChild.NamespaceURI);
                    XmlNodeList exposures = body.SelectNodes("a1:ExposureField", nsmgr);

                    if (exposures.Count == 1)
                    {
                        bsm = new Point[4];
                        Blade[] blades = new Blade[4];

                        blades[0] = GetBlade(exposures[0].SelectSingleNode("top"));
                        blades[1] = GetBlade(exposures[0].SelectSingleNode("left"));
                        blades[2] = GetBlade(exposures[0].SelectSingleNode("bottom"));
                        blades[3] = GetBlade(exposures[0].SelectSingleNode("right"));

                        // call intersection for blade pairs 0 & 1, 1 & 2, 2 & 3, 3 & 0
                        for (int j = 0, k = 1; j < blades.Length; j++, k++)
                        {
                            // wrap at NUM_BLADES
                            if (blades.Length == k)
                                k = 0;
                            bsm[j] = intersection(blades[j].theta, (float)(blades[j].rho * blades[j].diag),
                                blades[k].theta, (float)(blades[k].rho * blades[k].diag));
                        }
                    }
                }
            }
            catch
            {
                bsm = null;
            }
            GetPicture();
            Invalidate();
        }

        public void ApplyLut(string filename)
        {
            using (FileStream stream = new FileStream(filename, FileMode.Open))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Little);
                ushort[] temp = (ushort[])(object)reader.ReadWords(4096);
                if (voilut != null)
                {
                    for (int n = 0; n < 4096; n++)
                        voilut[n] = temp[voilut[n]];
                }
                else
                {
                    voilut = temp;
                }
            }
            GetPicture();
            Invalidate();
        }

        private void Viewer_Move(object sender, EventArgs e)
        {
            if (attributes.buffer != null)
            {
                GetPicture();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (rectangle != Rectangle.Empty)
            {
                if (previous != Rectangle.Empty)
                {
                    ControlPaint.DrawReversibleFrame(RectangleToScreen(previous), Color.Yellow, FrameStyle.Dashed);
                }
                previous = rectangle;
                ControlPaint.DrawReversibleFrame(RectangleToScreen(rectangle), Color.Yellow, FrameStyle.Dashed);
            }
        }

        public void SaveAs(string filename)
        {
            SaveAs(dicom, filename, invert, 0, false);
        }

        public void SaveAs(DataSet dicom, string filename, bool invert, int rotation, bool flip)
        {
            try
            {

                if (rotation != 0 || flip)
                {
                    string text = String.Format("Rotate{0}Flip{1}", rotation, (flip) ? "X" : "None");
                    RotateFlipType rft = (RotateFlipType)Enum.Parse(typeof(RotateFlipType), text);
                    RotateFlip(dicom, rft);
                }

                ImageAttributes attributes = Utilities.GetAttributes(dicom.Elements);
                ushort[] voilut = GetVOILUT(dicom.Elements);
                ushort[] overlays = ExtractOverlays(dicom.Elements);

                FileInfo info = new FileInfo(filename);

                ImageFormat format = ImageFormat.Bmp;
                switch (info.Extension.ToLower())
                {
                    case ".dcm":
                        dicom.TransferSyntaxUID = Syntax.ExplicitVrLittleEndian;
                        dicom.Part10Header = true;
                        dicom.Write(filename);
                        return;
                    case ".raw":
                        filename = String.Format("{0}.{1}.{2}.raw", info.FullName.Replace(info.Extension, ""), attributes.width, attributes.height);
                        if (File.Exists(filename))
                        {
                            DialogResult result = MessageBox.Show("File Exists.\nDo you want to overwrite?", "Dicom Viewer", MessageBoxButtons.YesNo);
                            if (result == DialogResult.No)
                            {
                                return;
                            }
                        }
                        using (FileStream stream = new FileStream(filename, FileMode.Create, FileAccess.Write))
                        {
                            EndianBinaryWriter writer = new EndianBinaryWriter(stream, Endian.Little);
                            writer.WriteWords((short[])dicom[t.PixelData].Value);
                        }
                        return;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".tif":
                    case ".tiff":
                        format = ImageFormat.Tiff;
                        break;
                    case ".gif":
                        format = ImageFormat.Gif;
                        break;
                    case ".png":
                        format = ImageFormat.Png;
                        break;
                    case ".emf":
                        format = ImageFormat.Emf;
                        break;
                    case ".exif":
                        format = ImageFormat.Exif;
                        break;
                    case ".wmf":
                        format = ImageFormat.Wmf;
                        break;
                    default:
                        throw new ArgumentException("Unsupported extension");
                }
                Bitmap bitmap = OtherImageFormats.GetBitmap(attributes, overlays, voilut, invert);
                bitmap.Save(filename, format);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }

        }

        private void PictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && ImageInClient.Contains(new Rectangle(e.Location, Size.Empty)))
            {
                start = e.Location;
                rectangle = Rectangle.Empty;
                Invalidate();
            }
        }

        private void PictureBox_MouseMove(object sender, MouseEventArgs mouse)
        {
            if (start != null)
            {
                // northwest
                if (mouse.X < start.Value.X && mouse.Y < start.Value.Y)
                {
                    rectangle = new Rectangle(mouse.Location, new Size(start.Value.X - mouse.X, start.Value.Y - mouse.Y));
                }
                // northeast
                else if (mouse.X > start.Value.X && mouse.Y < start.Value.Y)
                {
                    rectangle = new Rectangle(new Point(start.Value.X, mouse.Y), new Size(mouse.X - start.Value.X, start.Value.Y - mouse.Y));
                }
                // southwest
                else if (mouse.X < start.Value.X && mouse.Y > start.Value.Y)
                {
                    rectangle = new Rectangle(new Point(mouse.X, start.Value.Y), new Size(start.Value.X - mouse.X, mouse.Y - start.Value.Y));
                }
                // southeast
                else
                {
                    rectangle = new Rectangle(start.Value, new Size(mouse.X - start.Value.X, mouse.Y - start.Value.Y));
                }
                rectangle.Intersect(ImageInClient);
                Invalidate();
            }
            //temp resolve the crash of view overlay bit file
            if(attributes.bitsperpixel>8)
            {
                Point image = ClientToImage(mouse.Location);

                short value = (short)attributes.buffer.GetValue(image.Y * attributes.stride + image.X);

                if (voilut != null && !isCtImage)
                {
                    value = (short)voilut[value];
                }
                ((MainForm)MdiParent).SetStatus(String.Format("{0} : {1}", image, value));
            }
    
        }

        private Point ClientToImage(Point client)
        {
            Size box = PictureBox.Size;
            Size image = PictureBox.Image.Size;
            Size original = new Size(attributes.width, attributes.height);
            
            PointF offset = new PointF((float)(image.Width - box.Width) / 2.0f, (float)(image.Height - box.Height) / 2.0f);
            PointF temp = new PointF(client.X + offset.X, client.Y + offset.Y);

            float x = (float)original.Width / image.Width;
            float y = (float)original.Height / image.Height;
            temp = new PointF(temp.X * x, temp.Y * y);

            Point point = new Point((int)(temp.X + 0.5), (int)(temp.Y + 0.5));

            if (point.X < 0) point.X = 0;
            if (point.X > attributes.width - 1) point.X = attributes.width - 1;
            if (point.Y < 0) point.Y = 0;
            if (point.Y > attributes.height - 1) point.Y = attributes.height - 1;

            return point;
        }

        private void PictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            start = null;
        }

        private void ViewerContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            CropToolStripMenuItem.Enabled = (rectangle != Rectangle.Empty);
            if (overlay != null)
            {
                ViewerContextMenuStrip.Items.Add(ViewOverlaysToolStripMenuItem);
            }
            e.Cancel = false;
        }

        private void ViewOverlaysToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (overlay != null)
            {
                overlays = ViewOverlaysToolStripMenuItem.Checked;
                GetPicture();
            }
            else
            {
                MessageBeep(0);
            }
        }

        private void CropToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double scale = ScaleFactor;
            Rectangle shift = ImageInClient;

            rectangle = new Rectangle((int)((rectangle.X - shift.X) / scale), (int)((rectangle.Y - shift.Y) / scale), (int)(rectangle.Width / scale), (int)(rectangle.Height / scale));

            attributes.buffer = OtherImageFormats.Crop((short[])attributes.buffer, new Size(attributes.width, attributes.height), rectangle);

            attributes.width = (ushort)rectangle.Width;
            attributes.height = (ushort)rectangle.Height;
            attributes.stride = attributes.width;

            start = null;
            rectangle = previous = Rectangle.Empty;

            GetPicture();
            Invalidate();
        }

        private void InvertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            invert = !invert;
            GetPicture();
            Invalidate();
        }

        #region Utilities

        private void GetPicture()
        {
            PictureBox.Location = new Point(0, 0);
            PictureBox.Size = ClientRectangle.Size;

            ImageAttributes cropped = attributes; // OtherImageFormats.Crop(attributes);
            // if the application is minimized, PictureBox.Size has zero dimensions
            Size size = PictureBox.Size;

            if (size.Width == 0 || size.Height == 0)
            {
                // when the window is resized, we will render another image for display
                PictureBox.Image = null;
                return;
            }

            size = OtherImageFormats.Constrain(new Size(cropped.width, cropped.height), size);

            ImageAttributes resampled = (cropped.bitsperpixel == 8) ? 
                cropped : Utilities.Resample(cropped, (double)attributes.width / (double)size.Width);

            ushort[] planes = null;
            if (overlays && overlay != null)
            {
                ImageAttributes source = new ImageAttributes();
                source.type = TypeCode.UInt16;
                source.buffer = overlay;
                source.width = attributes.width;
                source.height = attributes.height;
                source.stride = attributes.width;
                // attributes.spacing = ;
                source.bitsperpixel = 12;
                source.monochrome1 = true;

                ImageAttributes temp = Utilities.Resample(source, (double)attributes.width / (double)size.Width);
                planes = (ushort[])temp.buffer;
            }

            Bitmap bitmap = OtherImageFormats.GetBitmap(resampled, planes, voilut, invert);

            Bitmap final = (bitmap.Size != size) ? new Bitmap(bitmap, size) : bitmap;

            SurroundMask(final);

            PictureBox.Image = final;
        }

        private unsafe void SurroundMask(Bitmap bitmap)
        {
            if (bsm != null)
            {
                System.Drawing.Imaging.BitmapData data = null;
                try
                {
                    Point[] points = (Point[])bsm.Clone();
                    double factor = (double)bitmap.Size.Width / (double)attributes.width;
                    for (int n = 0; n < points.Length; n++)
                    {
                        points[n] = new Point((int)(points[n].X * factor), (int)(points[n].Y * factor));
                    }

                    int length = points.Length;
                    data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    Format24bppRgbPixel* BitmapPtr = (Format24bppRgbPixel*)data.Scan0.ToPointer();


                    int width = data.Width;
                    int height = data.Height;
                    int stride = data.Stride;

                    Format24bppRgbPixel black = new Format24bppRgbPixel();

                    for (int r = 0; r < height; r++)
                    {
                        for (int c = 0; c < width; c++)
                        {
                            if (!contains(points, new Point(c, r)))
                            {
                                BitmapPtr[c] = black;
                            }
                        }
                        BitmapPtr = (Format24bppRgbPixel*)((byte*)BitmapPtr + stride);
                    }
                }
                finally
                {
                    if (data != null)
                    {
                        bitmap.UnlockBits(data);
                    }
                }
            }
        }

        /// <summary>
        /// Returns whether or not a point lies within a polygon.
        /// </summary>
        /// <param name="points">The vertices of the polygon.</param>
        /// <param name="point">The point to test.</param>
        /// <returns></returns>
        public static bool contains(Point[] points, Point point)
        {
            int i, j;
            bool contains = false;
            int length = points.Length;

            // the bottom algorithm does not always detect points on a horizontal edge on the same image row
            // so we first see if the point lies on a horizontal edge at the same Y coordinate

            // we need to find at least two vertices with the same Y coordinate 
            // for there to be a horizontal edge
            int left = Int32.MaxValue, right = Int32.MinValue;
            // for each vertex
            for (i = 0; i < length; i++)
            {
                // if the vertex is on the same row as our point
                if (points[i].Y == point.Y)
                {
                    // it is either a leftmost or rightmost point
                    if (left > points[i].X)
                        left = points[i].X;
                    if (right < points[i].X)
                        right = points[i].X;
                }
            }
            // if we have two points on the smae row, at least one is leftmost and one is rightmost
            if (left != right && left != Int32.MaxValue && right != Int32.MinValue)
            {
                // so test if we are between the two vertices
                contains = (left <= point.X && right >= point.X);
            }
            else
            {
                // this method is an adaptation of the Jordan curve theorem

                // an imaginary horizontal line is extended to the right of our point
                // for each edge of the polygon, 
                // if the point is in the half plane to the left of the extended edge and
                // the point's Y coordinate within the edge's Y range, then that would
                // mean a crossing of the line, and we toggle, even we are outside, odd we are inside

                for (i = 0, j = length - 1; i < length; j = i++)
                {
                    if (((points[i].Y > point.Y) != (points[j].Y > point.Y)) && (point.X < (points[j].X - points[i].X) * (point.Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                        contains = !contains;
                }
            }
            return contains;
        }

        /// <summary>
        /// Create an in memory array containg the overlays
        /// </summary>
        private static ushort[] ExtractOverlays(Elements elements)
        {
            // TODO accomodate an offset OverlayOrigin
            ushort[] overlay = null;
            try
            {
                for (int n = 0; n < 16; n++)
                {
                    if (elements.Contains(Element.GetRepeatingGroup(t.OverlayData, n)))
                    {

                        ImageAttributes attributes = Utilities.GetAttributes(elements);

                        ushort rows = (ushort)elements[Element.GetRepeatingGroup(t.OverlayRows, n)].Value;
                        ushort columns = (ushort)elements[Element.GetRepeatingGroup(t.OverlayColumns, n)].Value;
                        short[] origin = (short[])elements[Element.GetRepeatingGroup(t.OverlayOrigin, n)].Value;
                        byte[] bytes = (byte[])elements[Element.GetRepeatingGroup(t.OverlayData, n)].Value;

                        if (overlay == null)
                        {
                            overlay = new ushort[attributes.width * attributes.height];
                        }
                        for (int r = 0; r < attributes.height; r++)
                        {
                            for (int c = 0; c < attributes.width; c++)
                            {
                                int offset = r * attributes.width + c;
                                int b = bytes[offset / 8];
                                int position = (r * attributes.width + c) % 8;
                                int mask = (short)(0x01 << position);
                                ushort value = ((ushort)(b & mask) == (ushort)0) ? (ushort)0 : (ushort)1;
                                // do not overwrite any previous overlay data
                                if(overlay[offset] == 0) overlay[offset] = (ushort)value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                overlay = null;
            }
            return overlay;
        }

        /// <summary>
        /// Decimates the image and replaces the bits in the dataset to free some memory
        /// </summary>
        /// <param name="elements"></param>
        private void ReduceImage(Elements elements)
        {
            try
            {
				ushort w = (ushort)(attributes.width / 2.0);
                ushort h = (ushort)(attributes.height / 2.0);

				// only do for the short format image
	            if (attributes.bitsperpixel > 8 && attributes.bitsperpixel < 17)
	            {
					short[] pixels = (short[])attributes.buffer;

					short[] temp = new short[w * h];
					int n = 0;
					for (int r = 0; r < attributes.height - 1; r += 2)
					{
						for (int c = 0; c < attributes.width - 1; c += 2)
						{
							temp[n++] = pixels[r * attributes.width + c];
						}
					}

					// we don't get the memory back until we replace the source
					elements[t.Rows].Value = attributes.height = h;
					elements[t.Columns].Value = attributes.width = w;
					elements[t.PixelData].Value = temp;

					attributes = Utilities.GetAttributes(elements);		            
	            }
               
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void SetTitles()
        {
            // set the window title
            if (filename != null && filename.Length > 0)
            {
                this.Text = FileName;
            }
        }

        private void SetStatus(string message)
        {
            ((MainForm)this.Parent.Parent).SetStatus(message);
        }

        private double Aspect(Size size)
        {
            return (double)size.Width / (double)size.Height;
        }

        private Rectangle ImageInClient
        {
            get
            {
                double image = Aspect(new Size(attributes.width, attributes.height));
                Rectangle temp;
                if (image < Aspect(ClientRectangle.Size))
                {
                    // client height is image height
                    Size size = new Size((int)(image * ClientRectangle.Height), ClientRectangle.Height);
                    temp = new Rectangle(new Point((ClientRectangle.Width - size.Width) / 2, 0), size);
                }
                else
                {
                    // client width is image width
                    Size size = new Size(ClientRectangle.Width, (int)(ClientRectangle.Height / image));
                    temp = new Rectangle(new Point(0, (ClientRectangle.Height - size.Height) / 2), size);
                }
                //System.Diagnostics.Debug.WriteLine("ImageInClient " + temp.ToString());
                return temp;
            }
        }

        private double ScaleFactor
        {
            get
            {
                double image = Aspect(new Size(attributes.width, attributes.height));
                double temp;
                if (image < Aspect(ClientRectangle.Size))
                {
                    // client height is image height
                    temp = (double)ClientRectangle.Height / (double)attributes.height;
                }
                else
                {
                    // client width is image width
                    temp = (double)ClientRectangle.Width / (double)attributes.width;
                }
                return temp;
            }
        }

        #endregion Utilites

        private void HistogramToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int[] histogram = Utilities.GetHistogram(dicom.Elements);
            Analysis dialog = new Analysis(histogram);
            dialog.ShowDialog();
        }

        private void RotateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RotateFlip(dicom, RotateFlipType.Rotate90FlipNone);
            attributes = Utilities.GetAttributes(dicom.Elements);
            GetPicture();
            Invalidate();
        }

        public static void RotateFlip(DataSet dicom, RotateFlipType rft)
        {
            if (dicom.Contains(t.OverlayData) || dicom.Contains("(0029,1018)"))
            {
                throw new NotSupportedException("Rotation of an image with Overlays or an Eclipse State.");
            }
            ImageAttributes attributes = Utilities.GetAttributes(dicom.Elements);

            Utilities.RotateFlip(ref attributes, rft);

            double temp = (ushort)dicom[t.Rows].Value;
            dicom[t.Rows].Value = dicom[t.Columns].Value;
            dicom[t.Columns].Value = (ushort)temp;
            if (dicom.Contains(t.ImagerPixelSpacing) && dicom[t.ImagerPixelSpacing].Value is string[])
            {
                string[] values = (string[])dicom[t.ImagerPixelSpacing].Value;
                string text = values[0];
                values[0] = values[1];
                values[1] = text;
            }
            if (dicom.Contains(t.PixelAspectRatio) && dicom[t.PixelAspectRatio].Value is string[])
            {
                string[] values = (string[])dicom[t.PixelAspectRatio].Value;
                string text = values[0];
                values[0] = values[1];
                values[1] = text;
            }
        }

        private void FlipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dicom.Contains(t.OverlayData))
            {
                throw new Exception("Cannot flip images with Overlays.");
            }

            attributes = Utilities.GetAttributes(dicom.Elements);

            Utilities.RotateFlip(ref attributes, RotateFlipType.RotateNoneFlipX);

            GetPicture();
            Invalidate();
        }

        private void Viewer_Resize(object sender, EventArgs e)
        {
            if (attributes.buffer != null)
            {
                GetPicture();
            }
        }

        private void Viewer_Activated(object sender, EventArgs e)
        {
            SetPixelRepresentation();
        }
    }
}