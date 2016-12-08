using System;
using System.Drawing;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// A structure that is used to access individual pixel color elements of 
    /// System.Drawing.Bitmap
    /// </summary>
    /// <remarks>
    /// The color pixels in an RGB Bitmap are arranged BGR
    /// </remarks>
    public struct Format24bppRgbPixelLayout
    {
        public byte blue;
        public byte green;
        public byte red;
    }

    public static class Tools
    {
        public static unsafe System.Drawing.Bitmap GetBitmap(Elements elements, Size size)
        {
            System.Drawing.Bitmap bitmap = null;
            System.Drawing.Imaging.BitmapData data = null;
            try
            {
                // we either have a Part 10 Image or an ImageBox
                if (elements.Contains(t.BasicGrayscaleImageSequence))
                {
                    elements = ((Sequence)elements[t.BasicGrayscaleImageSequence]).Items[0];
                }

                int frames = 1;
                if (elements.Contains(t.NumberofFrames))
                {
                    frames = Int32.Parse((string)elements[t.NumberofFrames].Value);
                }

                ushort width = (ushort)elements[t.Columns].Value;
                ushort height = (ushort)elements[t.Rows].Value;
                ushort allocated = (ushort)elements[t.BitsAllocated].Value;
                ushort stored = (ushort)elements[t.BitsStored].Value;
                ushort bitsperpixel = (ushort)(allocated * (ushort)elements[t.SamplesperPixel].Value);

                Array pixeldata = (Array)elements[t.PixelData].Value;

                // create a new Bitmap object, we are going to write directly into its bits
                bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                // lock down the bitmap
                data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // get a pointer into the managed bitmap buffer
                Format24bppRgbPixelLayout* BitmapPtr = (Format24bppRgbPixelLayout*)data.Scan0.ToPointer();

                // the bitmap Scan0 buffer is likely an aligned image
                int rows = data.Height;
                int columns = data.Width;
                int spacing = data.Stride;

                byte value = 0;
                int offset = 0;

                if (bitsperpixel == 8)
                {
                    byte[] bpixels = pixeldata as byte[];
                    byte pixel = 0;

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            // get our bitmap rgb color pixel
                            Format24bppRgbPixelLayout* pRGBPixel = (Format24bppRgbPixelLayout*)(BitmapPtr + c);
                            // get our raw gray scale image pixel
                            pixel = (byte)bpixels[offset + c];

                            pRGBPixel->blue = pixel;
                            pRGBPixel->red = pixel;
                            pRGBPixel->green = pixel;
                        }
                        // take account of stride in the output
                        BitmapPtr = (Format24bppRgbPixelLayout*)((byte*)BitmapPtr + spacing);
                        offset += width;
                    }
                }
                if (bitsperpixel == 16)
                {
                    ushort[] uspixels = pixeldata as ushort[];
                    ushort pixel = 0;
                    ushort bitshift = (ushort)(stored - 8);

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            // get our bitmap rgb color pixel
                            Format24bppRgbPixelLayout* pRGBPixel = (Format24bppRgbPixelLayout*)(BitmapPtr + c);
                            // get our raw gray scale image pixel
                            pixel = (ushort)uspixels[offset + c];

                            // the 12 bit image is converted to 8 bit
                            value = (byte)(pixel >> bitshift);

                            // clip if needed to avoid access violation
                            // and the pixel is shifted down to 8 bit
                            if (value > 255) value = 255;

                            pRGBPixel->blue = value;
                            pRGBPixel->red = value;
                            pRGBPixel->green = value;
                        }
                        // take account of stride in the output
                        BitmapPtr = (Format24bppRgbPixelLayout*)((byte*)BitmapPtr + spacing);
                        offset += width;
                    }
                }
                else if (bitsperpixel == 24)
                {
                    byte[] bpixels = pixeldata as byte[];

                    for (int r = 0; r < rows; r++)
                    {
                        for (int c = 0; c < columns; c++)
                        {
                            // get our bitmap rgb color pixel
                            Format24bppRgbPixelLayout* pRGBPixel = (Format24bppRgbPixelLayout*)(BitmapPtr + c);

                            pRGBPixel->blue = (byte)bpixels[offset + c * 3 + 2];
                            pRGBPixel->red = (byte)bpixels[offset + c * 3];
                            pRGBPixel->green = (byte)bpixels[offset + c * 3 + 1];
                        }
                        // take account of stride in the output
                        BitmapPtr = (Format24bppRgbPixelLayout*)((byte*)BitmapPtr + spacing);
                        offset += width * 3;
                    }
                }
            }
            catch
            {
                bitmap = null;
            }
            finally
            {
                if (bitmap != null && data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }

            if (bitmap != null)
            {
                float source = (float)bitmap.Width / (float)bitmap.Height;
                float destination = (float)size.Width / (float)size.Height;
                if (source >= destination)
                {
                    size.Height = (int)(size.Width / source);
                }
                else
                {
                    size.Width = (int)(size.Height * source);
                }
                bitmap = new Bitmap(bitmap, size);
            }
            return bitmap;
        }
    }
}
