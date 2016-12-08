using System;
using System.Drawing;
using System.Text.RegularExpressions;
using EK.Capture.Dicom.DicomToolKit;

namespace DicomViewer
{
    public struct ImageAttributes
    {
        // The type of the image
        public TypeCode type;
        // The image bits
        public Array buffer;
        // The width of the image in pixels.
        public int width;
        // The height of the image in pixels.
        public int height;
        // The width of the image buffer in image type units.
        public int stride;
        // The number of bits per pixel.
        public int bitsperpixel;
        // The pixel spacing in microns.
        public int spacing;
        // The photometric interpretation.
        public bool monochrome1;

        public ImageAttributes(TypeCode type, Array buffer, int width, int height, int stride, int bitsperpixel, int spacing, bool monochrome1)
        {
            this.type = type;
            this.buffer = buffer;
            this.width = width;
            this.height = height;
            this.stride = stride;
            this.bitsperpixel = bitsperpixel;
            this.spacing = spacing;
            this.monochrome1 = monochrome1;
        }

        public override string ToString()
        {
            return String.Format("ImageAttributes:{0}.{2}.{3}.{4}.{5}.{6}.{7}", type, buffer, width, height, stride, bitsperpixel, spacing, monochrome1);
        }

        public static bool TryParse(string text)
        {
            bool result = true;
            try
            {
                ImageAttributes temp = Parse(text);
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public static ImageAttributes Parse(string text)
        {
            ImageAttributes attributes = new ImageAttributes();
            string pattern = @"ImageAttributes:(?<type>.+)\.(?<width>.+)\.(?<height>.+)\.(?<stride>.+)\.(?<bitsperpixel>.+)\.(?<spacing>.+)\.(?<monochrome1>False|True).*";
            Match match = Regex.Match(text, pattern);
            if (match.Success)
            {
                attributes.type = (TypeCode)Enum.Parse(typeof(TypeCode), match.Groups["type"].Value);
                attributes.width = Int32.Parse(match.Groups["width"].Value);
                attributes.height = Int32.Parse(match.Groups["height"].Value);
                attributes.stride = Int32.Parse(match.Groups["stride"].Value);
                attributes.bitsperpixel = Int32.Parse(match.Groups["bitsperpixel"].Value);
                attributes.spacing = Int32.Parse(match.Groups["spacing"].Value);
                attributes.monochrome1 = Boolean.Parse(match.Groups["monochrome1"].Value);
            }
            return attributes;
        }
    }

    public class Utilities
    {
        public static unsafe int[] GetHistogram(Elements elements)
        {
            ImageAttributes attributes = GetAttributes(elements);
            return GetHistogram(attributes);
        }

        public static unsafe int[] GetHistogram(ImageAttributes source)
        {
            int maximum = 1 << source.bitsperpixel;

            int[] histogram = new int[maximum];

            fixed (int* hist = histogram)
            fixed (short* src = ((short[])source.buffer))
            {
                int offset = 0;
                for (int r = 0; r < source.height; r++)
                {
                    for (int c = 0; c < source.width; c++)
                    {
                        hist[src[offset+c]]++;
                    }
                    offset += source.stride;
                }
            }
            return histogram;
        }

        public unsafe static ImageAttributes GetAttributes(Elements dicom)
        {
            Elements elements = dicom;
            // we either have a Part 10 Image or an ImageBox
            if (elements.Contains(t.BasicGrayscaleImageSequence))
            {
                elements = ((Sequence)dicom[t.BasicGrayscaleImageSequence]).Items[0];
            }
            ushort columns = (ushort)elements[t.Columns].Value;
            ushort rows = (ushort)elements[t.Rows].Value;
            ushort allocated = (ushort)elements[t.BitsAllocated].Value;
            ushort stored = (ushort)elements[t.BitsStored].Value;
            if (stored != 8 && stored != 12 && stored != 14 && stored != 16 && allocated != 16)
            {
                throw new Exception("This program only handles 12/14/16 bit images, and this file is " + stored.ToString() + "/" + allocated.ToString());
            }
            string interp = ((string)elements[t.PhotometricInterpretation].Value).ToUpper();
            if (!interp.Contains("MONOCHROME"))
            {
                throw new Exception("This program only handles MONOCHROME images, and this file is " + interp);
            }
            bool monochrome1 = (interp == "MONOCHROME1");

            stored = (ushort)elements[t.BitsStored].Value;
            if (!elements.Contains(t.SamplesperPixel))
            {
                elements.Set(t.SamplesperPixel, 1);
            }
            //if (!elements.Contains(t.PixelAspectRatio))
            //{
            //    elements.Set(t.PixelAspectRatio, @"1\1");
            //}
            if (!elements.Contains(t.HighBit))
            {
                elements.Set(t.HighBit, stored - 1);
            }
            if (!elements.Contains(t.PixelRepresentation))
            {
                elements.Set(t.PixelRepresentation, 0);
            }

            ImageAttributes attributes = new ImageAttributes();
            attributes.type = TypeCode.UInt16;
            attributes.buffer = (Array)elements[t.PixelData].Value;
            attributes.width = columns;
            attributes.height = rows;
            attributes.stride = columns;
            // attributes.spacing = ;
            attributes.bitsperpixel = stored;
            attributes.monochrome1 = monochrome1;

            return attributes;
        }

        public static unsafe ushort[] GetWindowLevelLut(int window, int level, int bits, short[] modalityLut = null)
        {
            int lutSize = 1 << bits;

            ushort minimum = 0;
            ushort maximum = (ushort) (lutSize - 1);
            ushort[] lut = new ushort[lutSize];

            int left = level - ((window + 1)/2);
            int right = left + window;

            if (left > (lutSize - 2))
            {
                left = lutSize - 2;
            }

            if (right < 0)
            {
                right = 1;
            }

            // set everything up to left to minimum
            int n = 0;
            for (n = 0; n <= left; n++)
            {
                lut[n] = minimum;
            }

            int min = left + 1;
            if (min < 0)
            {
                min = 0;
            }
            int max = right;
            if (max >= lutSize)
            {
                max = lutSize - 1;
            }

            // find slope between left and right
            double slope = (maximum - minimum)/(double) (right - left);

            for (n = min; n <= max; n++)
            {
                lut[n] = (ushort) (minimum + slope*(double) (n - left));
            }

            // set everything after right to maximum
            for (n = right; n < lutSize; n++)
            {
                lut[n] = (ushort) (lutSize - 1);
            }

            return lut;
        }

        public static short[] GetModalityLut(int bits, uint pixelRepresentation = 0, float rescaleIntercept = 0.0f, float rescaleSlope = 1.0f)
        {
            int lutSize = 1 << bits;
            short[] lut = new short[lutSize];

            for (int i = 0; i < lutSize; ++i)
            {
                var val = 0.0f;

                if (pixelRepresentation == 0)
                {
                    val = i*rescaleSlope + rescaleIntercept;
                }
                else
                {
                    val = (float) (((i - lutSize/2.0)*rescaleSlope + rescaleIntercept));
                }

                lut[i] = (short)Math.Round(val);
            }

            return lut;
        }

        public static ushort[] GetCTWindowLevelLut(int window, int level, int bits, uint pixelRepresentation = 0, float rescaleIntercept = 0.0f,
                                                   float rescaleSlope = 1.0f)
        {
            int lutSize = 1 << bits;

            ushort minimum = 0;
            ushort maximum = (ushort) (lutSize - 1);
            ushort[] lut = new ushort[lutSize];
            short[] modalityLut = null;
            int left = level - ((window + 1)/2);
            int right = left + window;

            modalityLut = GetModalityLut(bits, pixelRepresentation, rescaleIntercept, rescaleSlope);
            float rate = lutSize*1.0f/window;
            float offset = lutSize*0.5f - level*rate;

            for (int i = 0; i < maximum; i++)
            {
                float val = modalityLut[i]*rate + offset;

                val = Math.Min(Math.Max(0.0f, val), lutSize - 1);
                lut[i] = (ushort) Math.Round(val);
            }

            return lut;
        }

        public static unsafe ImageAttributes Resample(ImageAttributes source, double factor)
        {
            const int BIT_SHIFT = 18;           // ideal for 12 bit pixels
            const double INT_SCALE = 262143.0;  // ideal for 12 bit pixels

            if (null == source.buffer)
            {
                string message = "Null source.buffer.";
                Logging.Log(LogLevel.Error, message);
                throw new ArgumentNullException("source");
            }

            ImageAttributes destination = new ImageAttributes();

            // this code will not allow a change in aspect ratio 
            int width = (int)((double)source.width/factor + 0.5);
            int height = (int)((double)source.height/factor + 0.5);
            int stride = source.stride;
            if (0 == stride)
            {
                stride = source.width;
            }

            // limit the resultant size to 275MB, 
            // this is about the size of a very large 12000x12000 printer resolution image
            if (width*height*sizeof(ushort) > 275*1024*1024)
            {
                string message = "Resampled image would exceed 275MB.";
                Logging.Log(LogLevel.Error, message);
                throw new ArgumentOutOfRangeException("factor");
            }

            // create a destination buffer that is NOT aligned, i.e. ssStride = ssWidth
            destination.buffer = new short[width*height];

            destination.type = TypeCode.UInt16;
            destination.bitsperpixel = source.bitsperpixel;
            destination.spacing = (int)((double)source.spacing * factor);

            destination.width = width;
            destination.height = height;
            destination.stride = width;
            destination.monochrome1 = source.monochrome1;

            // if factor is 1, there's no need to interpolate so just do a copy 
            if (1.0 == factor)
            {
                Array.Copy(source.buffer, destination.buffer, source.buffer.Length);
                return destination;
            }

            // allocate xy LUTs for high-speed scaling
            int[] temp1 = new int[height];
            int[] temp2 = new int[width];
            int[] temp3 = new int[height];
            int[] temp4 = new int[width];
            int[] temp5 = new int[height];
            int[] temp6 = new int[width];

            fixed (short* src = ((short[])source.buffer), dest = ((short[])destination.buffer))
            {
                fixed (int* rowStartLUT = temp1, colStartLUT = temp2, rowEndLUT = temp3, colEndLUT = temp4, vfrLUT = temp5, hfrLUT = temp6)
                {

                    // initialize loop variables
                    double xstep = (double)source.width / (double)width;
                    double ystep = (double)source.height / (double)height;
                    double hfrac, vfrac;
                    double x = 0.0;
                    double y = 0.0;

                    // initialize vertical scaling LUTs
                    for (int row = 0; row < height; row++)
                    {
                        vfrac = y - (double)((int)y);
                        vfrLUT[row] = (int)(vfrac * INT_SCALE);
                        rowStartLUT[row] = (int)y;
                        rowEndLUT[row] = rowStartLUT[row] + 1;
                        if (rowEndLUT[row] >= source.height)
                        {
                            rowEndLUT[row] = source.height - 1;
                        }
                        y += ystep;
                    }

                    // initialize horizontal scaling LUTs
                    for (int col = 0; col < width; col++)
                    {
                        hfrac = x - (double)((int)x);
                        hfrLUT[col] = (int)(hfrac * INT_SCALE);
                        colStartLUT[col] = (int)x;
                        colEndLUT[col] = colStartLUT[col] + 1;
                        if (colEndLUT[col] >= source.width)
                        {
                            colEndLUT[col] = source.width - 1;
                        }
                        x += xstep;
                    }

                    // resample image using bilinear interpolation
                    int xMin, xMax, yMinRow, yMaxRow;
                    int pix0, pix1, pix2, pix3;
                    int vfrLUTr;
                    int upperavg, loweravg, pixVal;
                    int n = 0;
                    for (int row = 0; row < height; row++)
                    {
                        yMinRow = rowStartLUT[row] * stride;
                        yMaxRow = rowEndLUT[row] * stride;
                        vfrLUTr = vfrLUT[row];
                        for (int col = 0; col < width; col++)
                        {
                            xMin = colStartLUT[col];
                            xMax = colEndLUT[col];
                            pix0 = src[yMinRow + xMin];
                            pix1 = src[yMinRow + xMax];
                            pix2 = src[yMaxRow + xMin];
                            pix3 = src[yMaxRow + xMax];

                            upperavg = ((pix0 << BIT_SHIFT) + hfrLUT[col] * (pix1 - pix0)) >> BIT_SHIFT;
                            loweravg = ((pix2 << BIT_SHIFT) + hfrLUT[col] * (pix3 - pix2)) >> BIT_SHIFT;
                            pixVal = ((upperavg << BIT_SHIFT) + vfrLUTr * (loweravg - upperavg)) >> BIT_SHIFT;

                            dest[n++] = (short)pixVal;
                        }
                    }
                }
            }
            return destination;
        }

        public static unsafe void RotateFlip(ref ImageAttributes source, RotateFlipType rft)
        {
            Bitmap bitmap = OtherImageFormats.GetBitmap(source, null, null, false);
            bitmap.RotateFlip(rft);
            DataSet dicom = OtherImageFormats.GetImage(bitmap);

            source.height = bitmap.Height;
            source.width = source.stride = bitmap.Width;
            Array.Copy((Array)dicom[t.PixelData].Value, source.buffer, source.buffer.Length);
        }
    }
}
