using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DicomViewer;
using EK.Capture.Dicom.DicomToolKit;

namespace ExtendedListTest.Image
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

    public class ImageLoader
    {
        /// <summary>
        /// 
        /// </summary>
        private EK.Capture.Dicom.DicomToolKit.DataSet dicom = null;
        
        /// <summary></summary>
        private ushort[] voilut = null;
        
        /// <summary>The ImageAttributes of the cached image.</summary>
        private ImageAttributes attributes;

        private bool isCtImage = false;

        private ushort pixelRepresentation = 0;

        /// <summary>Overlay planes read in from the DICOM DataSet</summary>
        private ushort[] overlay = null;
        /// <summary>Whether or not to display overlays.</summary>
        private bool overlays = false;
        /// <summary>optional bsm points</summary>
        private Point[] bsm = null;

        private bool invert = false;

        public Bitmap LoadBitmap(EK.Capture.Dicom.DicomToolKit.DataSet elements)
        {
            try
            {
                dicom = elements;
                SetIsCtImage();
                SetPixelRepresentation();
                voilut = GetVOILUT(dicom.Elements);

                attributes = Utilities.GetAttributes(dicom.Elements);

                // extract overlays, if any
                overlay = ExtractOverlays(dicom.Elements);
                overlays = (overlay != null);

                return GetPicture();
            }
            catch (Exception ex)
            {
                Logging.Log(ex);
                throw ex;
            }

            return null;
        }

        public Bitmap LoadBitmap(string fileName)
        {
            try
            {
                // we should either have a fileName or dataset
                if (fileName != null && fileName.Length > 0)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        dicom = OtherImageFormats.Read(fileName);
                    }
                    finally
                    {
                        //SetStatus("");
                        Cursor.Current = Cursors.Default;
                    }
                }
                if (dicom == null)
                {
                    throw new ArgumentException("No fileName or dataset.");
                }

                return LoadBitmap(dicom);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Logging.Log(ex));
            }

            return null;
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
            try
            {
                if (dicom.Contains(t.VOILUTSequence))
                {
                    lut = (ushort[]) dicom[t.VOILUTSequence + t.LUTData].Value;
                }
                else
                {
                    if (dicom.Contains(t.WindowCenter) && dicom.Contains(t.WindowWidth) && dicom.Contains(t.BitsStored))
                    {
                        double window = Double.Parse(((string[]) dicom[t.WindowWidth].Value)[0]);
                        double level = Double.Parse(((string[]) dicom[t.WindowCenter].Value)[0]);
                        ushort stored = (ushort) dicom[t.BitsStored].Value;

                        if (isCtImage)
                        {
                            float rescaleIntercept = float.Parse((string) dicom[t.RescaleIntercept].Value);
                            float rescaleSlope = float.Parse((string) dicom[t.RescaleSlope].Value);
                            lut = Utilities.GetCTWindowLevelLut((int) window, (int) level, stored, pixelRepresentation,
                                rescaleIntercept, rescaleSlope);
                        }
                        else
                        {
                            lut = Utilities.GetWindowLevelLut((int) window, (int) level, stored);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return lut;
        }

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
                                if (overlay[offset] == 0) overlay[offset] = (ushort)value;
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

        private Bitmap GetPicture()
        {
            //PictureBox.Location = new Point(0, 0);
            //PictureBox.Size = ClientRectangle.Size;

            ImageAttributes cropped = attributes; // OtherImageFormats.Crop(attributes);
            // if the application is minimized, PictureBox.Size has zero dimensions
            //Size size = new Size();
            //size.Height = 1000;
            //size.Width = 1000;

            //if (size.Width == 0 || size.Height == 0)
            //{
            //    // when the window is resized, we will render another image for display
            //    PictureBox.Image = null;
            //    return;
            //}

            //size = OtherImageFormats.Constrain(new Size(cropped.width, cropped.height), size);

            var size = new Size(cropped.width, cropped.height);
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

                //ImageAttributes temp = Utilities.Resample(source, (double)attributes.width / (double)size.Width);
                planes = (ushort[])source.buffer;
            }

            Bitmap bitmap = OtherImageFormats.GetBitmap(resampled, planes, voilut, invert);

            Bitmap final = (bitmap.Size != size) ? new Bitmap(bitmap, size) : bitmap;

            SurroundMask(final);

            return final;
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
    }
}
