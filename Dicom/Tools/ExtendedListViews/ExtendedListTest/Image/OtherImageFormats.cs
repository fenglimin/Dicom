using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EK.Capture.Dicom.DicomToolKit;
using ExtendedListTest;

namespace DicomViewer
{
    /// <summary>
    /// 
    /// </summary>
    public class OtherImageFormats
    {
        public static uint PixelRepresentation = 0;

        public static DataSet Read(string filename)
        {
            DataSet dicom = null;
            FileInfo info = new FileInfo(filename);
            FileStream stream = null;
            bool whatever = false;
            try
            {
                stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read);
                switch (info.Extension.ToLower())
                {
                    case ".dcm":
					case "":
                        dicom = ReadDicom(stream);
                        break;
                    case ".pgm":
                        dicom = ReadPgm(stream);
                        break;
                    case ".bit":
                    case ".raw":
                    case ".mask":
                        dicom = ReadRaw(stream);
                        break;
                    case ".xml":
                        dicom = ReadFilmBox(stream, Size.Empty);
                        break;
                    default:
                        whatever = true;
                        dicom = ReadWhatever(stream);
                        break;
                }
            }
            catch (Exception ex)
            {
                dicom = null;
                if (!whatever)
                {
                    try
                    {
                        dicom = ReadWhatever(stream);
                    }
                    catch
                    {
                    }
                }
                if (dicom == null)
                {
                    System.Windows.Forms.MessageBox.Show("Unable to read image. " + ex.Message);
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
            return dicom;
        }

        private static DataSet ReadDicom(FileStream stream)
        {
            DataSet dicom = new DataSet();

            try
            {
                dicom.Read(stream);
            }
            catch
            {
            }

	        var modality = dicom.GetSafeStringValue(t.Modality).ToUpper();
			if (modality == "SR" || modality == "PR")
	        {
		        // sr or pr
				return dicom;
	        }

	        if (dicom.GetSafeStringValue(t.MediaStorageSOPClassUID) == SOPClass.MediaStorageDirectoryStorage)
	        {
				//dicomdir
		        return dicom;
	        }

            if ((!dicom.Contains(t.PixelData) && !dicom.Contains(t.BasicGrayscaleImageSequence + t.Columns)) || 
                (!dicom.Contains(t.Rows) && !dicom.Contains(t.BasicGrayscaleImageSequence + t.Rows)) || 
                (!dicom.Contains(t.Columns) && !dicom.Contains(t.BasicGrayscaleImageSequence + t.Columns)))
            {
                throw new Exception("Probably not a valid DICOM file.");
            }
            if (Syntax.CanEncapsulatePixelData(dicom.TransferSyntaxUID))
            {
                throw new Exception("Unsupported Transfer Syntax.");
            }

            return dicom;
        }

        private unsafe static DataSet ReadPgm(FileStream stream)
        {
            /*
            1. A "magic number" for identifying the file type. A pgm image's magic number is the two 
               characters "P5".
            2. Whitespace (blanks, TABs, CRs, LFs).
            3. A width, formatted as ASCII characters in decimal.
            4. Whitespace.
            5. A height, again in ASCII decimal.
            6. Whitespace.
            7. The maximum gray value (Maxval), again in ASCII decimal. Must be less than 65536, and more than zero.
            8. A single whitespace character (usually a newline).

            A raster of Height rows, in order from top to bottom. Each row consists of Width gray values,
            in order from left to right. Each gray value is a number from 0 through Maxval, with 0 being
            black and Maxval being white. Each gray value is represented in pure binary by either 1 or 2
            bytes. If the Maxval is less than 256, it is 1 byte. Otherwise, it is 2 bytes. The most 
            significant byte is first.
            */
            string pattern = @"(?<magic>P5)\s+(?<width>\d+)\s+(?<height>\d+)\s+(?<end>((?<maximum>\d+)\s))";

            int size = 1024;
            BinaryReader reader = new BinaryReader(stream);
            byte[] chunk = reader.ReadBytes(size);
            String header = ASCIIEncoding.ASCII.GetString(chunk).ToUpper();

            DataSet dicom = new DataSet();
            Match match = Regex.Match(header, pattern);
            if (match.Success)
            {
                ImageAttributes attributes =
                    new ImageAttributes(TypeCode.UInt16, null, 0, 0, 0, 12, 168, true);

                attributes.width = int.Parse(match.Groups["width"].ToString());
                attributes.height = int.Parse(match.Groups["height"].ToString());

                int maximum = int.Parse(match.Groups["maximum"].ToString());
                attributes.bitsperpixel = (maximum <= 256) ? 8 : 12;

                int pos = match.Groups["end"].Index + match.Groups["end"].Length;
                stream.Seek(pos, SeekOrigin.Begin);

                dicom.Add(t.Columns, attributes.width);
                dicom.Add(t.Rows, attributes.height);
                dicom.Add(t.PhotometricInterpretation, "MONOCHROME1");

                ReadBytes(stream, dicom, attributes);
            }
            return dicom;
        }

        private unsafe static DataSet ReadRaw(FileStream stream)
        {
            DataSet dicom = new DataSet();

            ImageAttributes attributes =
                new ImageAttributes(TypeCode.UInt16, null, 0, 0, 0, 12, 168, true);

            try
            {
                string[] parts = stream.Name.ToLower().Split(".".ToCharArray());
                string extension = parts[parts.Length - 1].ToLower();
                switch (extension)
                {
                    case "bit":
                        attributes.bitsperpixel = 1;
                        break;
                    case "mask":
                        attributes.bitsperpixel = 8;
                        break;
                    default:
                        attributes.bitsperpixel = 12;
                        break;
                }
                if (parts.Length >= 4)
                {
                    attributes.width = UInt16.Parse(parts[parts.Length - 3]);
                    attributes.height = UInt16.Parse(parts[parts.Length - 2]);
                    attributes.stride = attributes.width;
                    attributes.monochrome1 = true;

                    dicom.Set(t.Columns, attributes.width);
                    dicom.Set(t.Rows, attributes.height);
                    dicom.Set(t.PhotometricInterpretation, "MONOCHROME1");

                    ReadBytes(stream, dicom, attributes);

                    if (parts.Length >= 7)
                    {
                        attributes.stride = UInt16.Parse(parts[parts.Length - 4]);
                        attributes.bitsperpixel = UInt16.Parse(parts[parts.Length - 5]);
                        attributes.spacing = UInt16.Parse(parts[parts.Length - 6]);
                    }
                }
            }
            catch
            {
            }

            if (attributes.height == 0 || attributes.width == 0)
            {
                Settings settings = new Settings("DicomViewer");
                if(ImageAttributes.TryParse(settings["attributes"]))
                {
                    attributes = ImageAttributes.Parse(settings["attributes"]);
                }

                ReadRawForm dialog = new ReadRawForm(stream.Name, attributes);
                if (DialogResult.OK == dialog.ShowDialog())
                {
                    attributes = dialog.Attributes;

                    settings["attributes"] = attributes.ToString();

                    dicom.Set(t.Columns, attributes.width);
                    dicom.Set(t.Rows, attributes.height);
                    dicom.Set(t.PhotometricInterpretation, attributes.monochrome1 ? "MONOCHROME1" : "MONOCHROME2");

                    ReadBytes(stream, dicom, attributes);
                }
            }

            return dicom;
        }

        private unsafe static void ReadBytes(Stream stream, DataSet dicom, ImageAttributes attributes)
        {
            EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Little);

            switch (attributes.bitsperpixel)
            {
                case 1:
                    {
                        dicom.Add(t.BitsAllocated, 8);
                        dicom.Add(t.BitsStored, 8);

                        int stride = (int)Math.Ceiling((double)attributes.width / 8.0);
                        byte[] bits = reader.ReadBytes(stride * attributes.height);
                        byte[] bytes = ConvertBitsToBytes(bits);
                        dicom.Add(t.PixelData, bytes);
                    }
                    break;
                case 8:
                    {
                        dicom.Add(t.BitsAllocated, 8);
                        dicom.Add(t.BitsStored, 8);

                        byte[] temp = reader.ReadBytes(attributes.width * attributes.height);
                        dicom.Add(t.PixelData, temp);
                    }
                    break;
                default:
                    {
                        dicom.Add(t.BitsAllocated, 16);
                        dicom.Add(t.BitsStored, 12);

                        short[] temp = reader.ReadWords(attributes.width * attributes.height);
                        dicom.Add(t.PixelData, temp);
                    }
                    break;
            }
        }

        private static DataSet ReadBitmap(FileStream stream)
        {
            Bitmap bitmap = new Bitmap(stream);
            return GetImage(bitmap);
        }

        private static DataSet ReadWhatever(FileStream stream)
        {
            try
            {
                return ReadDicom(stream);
            }
            catch
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            try
            {
                return ReadBitmap(stream);
            }
            catch
            {
                stream.Seek(0, SeekOrigin.Begin);
            }

            return ReadRaw(stream);
        }

        private static byte[] ConvertBitsToBytes(byte[] bits)
        {
            byte[] bytes = new byte[bits.Length * 8];

            int m = 0;
            foreach (byte b in bits)
            {
                for (byte bit = 0; bit < 8; bit++)
                {
                    bytes[m++] = ((b & (byte)(1 << bit)) == 0) ? (byte)255 : (byte)0;
                }
            }
            return bytes;
        }

        public static DataSet ReadFilmBox(FileStream stream, Size size)
        {
            System.Runtime.Serialization.Formatters.Soap.SoapFormatter formatter = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter();
            FilmBox page = (FilmBox)formatter.Deserialize(stream);
            return RenderPage(page, size);
        }

        public static DataSet RenderPage(FilmBox page, Size size)
        {
            string film = (string)page[t.FilmSizeID].Value;         // e.g., 14INX17IN

            Size filmsize = ParseFilmSizeID(film);
            Size resize = Constrain(filmsize, size);

            Bitmap bitmap = new Bitmap(resize.Width, resize.Height);

            string format = (string)page[t.ImageDisplayFormat].Value;   // e.g., ROW\2,2,2,2,1 or STANDARD\1,1
            int[] items = null;
            ParseImageDisplayFormat(ref format, ref items);

            // for each item , there are a number of columns
            // the item count determines the number
            // the image position 

            double factor = (double)resize.Width / (double)filmsize.Width;
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    graphics.FillRectangle(brush, 0, 0, resize.Width, resize.Height);
                }
                FillRows(graphics, page, factor, items);
            }

            DataSet dicom = GetImage(bitmap);
            return dicom;
        }

        public static DataSet RenderPage(FilmBox page)
        {
            string film = (string)page[t.FilmSizeID].Value;         // e.g., 14INX17IN

            Size filmsize = ParseFilmSizeID(film);

            Bitmap bitmap = new Bitmap(filmsize.Width, filmsize.Height);

            string format = (string)page[t.ImageDisplayFormat].Value;   // e.g., ROW\2,2,2,2,1 or STANDARD\1,1
            int[] items = null;
            ParseImageDisplayFormat(ref format, ref items);

            // for each item , there are a number of columns
            // the item count determines the number
            // the image position 

            double factor = 1.0;
            using (Graphics graphics = Graphics.FromImage(bitmap))
            {
                using (SolidBrush brush = new SolidBrush(Color.White))
                {
                    graphics.FillRectangle(brush, 0, 0, filmsize.Width, filmsize.Height);
                }
                FillRows(graphics, page, factor, items);
            }

            DataSet dicom = GetImage(bitmap);
	        return dicom;
			//foreach (var item in page.Dicom)
			//{
			//	dicom.Add(item);
			//}

			//foreach (var imageBox in page.ImageBoxes)
			//{
			//	foreach (var item in imageBox.Dicom)
			//	{
			//		dicom.Add(item);
			//	}
			//}

			//return dicom;
        }

        private static void ParseImageDisplayFormat(ref string format, ref int[] items)
        {
            string[] strings = format.Split(@"\".ToCharArray());
            format = strings[0].ToUpper();
            strings = strings[1].Split(",".ToCharArray());
            items = null;
            if (format == "STANDARD")
            {
                int columns = Int32.Parse(strings[0]);
                int rows = Int32.Parse(strings[1]);
                items = new int[rows];
                for (int n = 0; n < rows; n++)
                {
                    items[n] = columns;
                }
            }
            else if (format == "ROW")
            {
                items = new int[strings.Length];
                for (int n = 0; n < items.Length; n++)
                {
                    items[n] = Int32.Parse(strings[n]);
                }
            }
            else
            {
                throw new Exception(String.Format("ImageDisplayFormat={0} not supported.", format));
            }
        }

        // each column must have its height calculated and the width verified
        // once the width and height are known, we can then resize bitmaps into the image
        private static void FillRows(Graphics graphics, FilmBox page, double factor, int[] bands)
        {
            int position = 1;
            int top = 0;
            foreach (int column in bands)
            {
                // the row is divided into imageboxes of equal width
                int width = (int)(graphics.VisibleClipBounds.Width / column);
                int height = Int32.MinValue;

                // the height of the row is calcuated as the maximum height of each image
                // based on RequestedImageSize or the height implied by the width and aspect ratio.
                for (int p = 0; p < column; p++)
                {
                    ImageBox imagebox = page.FindImageBox(position + p);
                    if (imagebox != null)
                    {
                        ushort columns = (ushort)imagebox[t.BasicGrayscaleImageSequence + t.Columns].Value;
                        ushort rows = (ushort)imagebox[t.BasicGrayscaleImageSequence + t.Rows].Value;
                        if (imagebox.Dicom.ValueExists(t.RequestedImageSize))
                        {
                            int requested = (int)(Double.Parse((string)imagebox[t.RequestedImageSize].Value) * 10.0 * factor);
                            if (requested <= width)
                            {
                                int temp = requested * rows / columns;
                                if (temp > height)
                                {
                                    height = temp;
                                }
                            }
                            else
                            {
                                 throw new Exception("Width exceeded!");
                                //string behavior = (string)imagebox[t.RequestedDecimateCropBehavior].Value;
                                //if (behavior != "DECIMATE")
                                //    throw new Exception("Width exceeded!");
                            }
                        }
                        else
                        {
                            int temp = width * rows / columns;
                            if (temp > height)
                            {
                                height = temp;
                            }
                        }
                    }
                }

                // limit the height to what is left, important for STANDARD\1,1 LLI
                if (top + height > graphics.VisibleClipBounds.Height)
                {
                    height = (int)graphics.VisibleClipBounds.Height;
                }

                // once the width and height are known each column in this row is fit into the imagebox.
                for (int p = 0; p < column; p++)
                {
                    ImageBox imagebox = page.FindImageBox(position + p);
                    if (imagebox != null)
                    {
                        DrawImageBox(imagebox, new Rectangle(p * width, top, width, height), graphics);
                    }
                }
                // move the to the top of the next row
                top += height;
                // move to the imagebox o
                position += column;
            }
        }

        private static void DrawImageBox(ImageBox imagebox, Rectangle rectangle, Graphics graphics)
        {

            ImageAttributes attributes = Utilities.GetAttributes(imagebox.Dicom.Elements);

            Size size = Constrain(new Size(attributes.width, attributes.height), rectangle.Size);

            ImageAttributes resampled = Utilities.Resample(attributes, (double)attributes.width / (double)size.Width);

			Bitmap bitmap = GetBitmap(resampled, null, null, resampled.monochrome1);

            Point location = rectangle.Location;
            location.X += (rectangle.Size.Width - size.Width) / 2;
            location.Y += (rectangle.Size.Height - size.Height) / 2;

            graphics.DrawImage(bitmap, location);
        }

        private static Size ParseFilmSizeID(string filmsize)
        {
            string text = filmsize.ToUpper();
            /*
            8INX10IN
            8_5INX11IN
            10INX12IN
            10INX14IN
            11INX14IN
            11INX17IN
            14INX14IN
            14INX17IN
            24CMX24CM
            24CMX30CM
            A4
            A3

            10INX14IN corresponds with 25.7CMX36.4CM
            A4 corresponds with 210 x 297 millimeters
            A3 corresponds with 297 x 420 millimeters
             */
            Size size = new Size();
            string SearchPattern = @"(?<width>[0-9_]+)(?<middle>INX|CMX)(?<height>[0-9_]+)(?<units>CM|IN)";
            Match match = Regex.Match(text, SearchPattern);
            if (match.Success)
            {
                string temp = match.Groups["width"].ToString().Replace("_", ".");
                float width = float.Parse(temp, System.Globalization.NumberStyles.AllowDecimalPoint);

                temp = match.Groups["height"].ToString().Replace("_", ".");
                float height = float.Parse(temp, System.Globalization.NumberStyles.AllowDecimalPoint);

                string units = match.Groups["units"].ToString();
                if (units == "IN")
                {
                    size = new Size((int)(width * 254), (int)(height * 254));
                }
                else if (units == "CM")
                {
                    size = new Size((int)(width * 100), (int)(height * 100));
                }
                else
                {
                    throw new Exception("Unknown units.");
                }
            }
            else if (text == "A3")      // A3 corresponds with 297 x 420 millimeters
            {
                size = new Size(2970, 4200);
            }
            else if (text == "A4")      // A4 corresponds with 210 x 297 millimeters
            {
                size = new Size(2100, 2970);
            }
            else
            {
                throw new Exception("Unknown format.");
            }
            return size;
        }

        /// <summary>
        /// A structure that is used to access individual pixel color elements of 
        /// System.Drawing.Bitmap
        /// </summary>
        /// <remarks>
        /// The color pixels in an RGB Bitmap are arranged BGR
        /// </remarks>
        public struct PixelData
        {
            public byte blue;
            public byte green;
            public byte red;
        }

        public unsafe static DataSet GetImage(Bitmap bitmap)
        {
            DataSet dicom = new EK.Capture.Dicom.DicomToolKit.DataSet();

            int width = (ushort)bitmap.Width;
            int height = (ushort)bitmap.Height;
            dicom.Add(t.Columns, width);
            dicom.Add(t.Rows, height);
            dicom.Add(t.PhotometricInterpretation, "MONOCHROME2");
            dicom.Add(t.BitsAllocated, 16);
            dicom.Add(t.BitsStored, 12);

            short[] pixels = new short[width * height];
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            PixelData* BitmapPtr = (PixelData*)data.Scan0.ToPointer();
            int spacing = data.Stride;
            int n = 0;
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    PixelData* pRGBPixel = (PixelData*)(BitmapPtr + c);
                    pixels[n++] = (short)((int)(pRGBPixel->blue * 0.11 + pRGBPixel->red * 0.3 + pRGBPixel->green * 0.59) << 4);
                }
                BitmapPtr = (PixelData*)((byte*)BitmapPtr + spacing);
            }
            dicom.Add(t.PixelData, pixels);

            return dicom;
        }

        public static unsafe System.Drawing.Bitmap GetBitmap(ImageAttributes attributes, ushort[] overlays, ushort[] voilut, bool invert)
        {
            System.Drawing.Bitmap bitmap = null;
            System.Drawing.Imaging.BitmapData data = null;
            try
            {

                ushort clip = (ushort)((1 << attributes.bitsperpixel) - 1);

                byte[] inversion = null;
                if (invert)
                {
                    inversion = new byte[256];
                    for (int b = 0; b < 256; b++)
                    {
                        inversion[b] = (byte)(255 - b);
                    }
                }

                if (attributes.bitsperpixel == (ushort)8) Equalize(attributes);

                byte[] pixels = null;
                if (attributes.buffer is byte[])
                {
                    pixels = (byte[])attributes.buffer;
                    if (voilut != null)
                    {
                        fixed (byte* ptr = pixels)
                        {
                            for (int n = 0; n < pixels.Length; n++)
                            {
                                ptr[n] = (byte)voilut[ptr[n]];
                            }
                        }
                    }
                }
                else
                {
                    ushort[] imageBuffer = null;

                    if (PixelRepresentation == 1)
                    {
                        short[] signedImageBuffer = (short[])attributes.buffer;
                        imageBuffer = new ushort[signedImageBuffer.Length];

                        for (int index = 0; index < imageBuffer.Length; index++)
                        {
                            if (voilut != null)
                            {
                                imageBuffer[index] = (ushort) (signedImageBuffer[index] + voilut.Length/2);
                            }
                            else
                            {
                                imageBuffer[index] = (ushort)(signedImageBuffer[index]);
                            }
                        }
                    }
                    else
                    {
                        imageBuffer = (ushort[])attributes.buffer;
                    }
                   
                    pixels = new byte[imageBuffer.Length];
                    if (attributes.bitsperpixel > 8)
                    {
                        int shift = attributes.bitsperpixel - 8;
                        ushort value = 0;
                        fixed (ushort* ptr = imageBuffer)
                        {
                            for (int n = 0; n < imageBuffer.Length; n++)
                            {
                                value = ptr[n];
                                if (value > clip) value = clip;
                                value = (voilut != null) ? (ushort)voilut[value] : value;
                                pixels[n] = (byte) (value >> shift);
                            }
                        }
                    }
                }


                // create a new Bitmap object, we are going to write directly into its bits
                bitmap = new System.Drawing.Bitmap(attributes.width, attributes.height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                // lock down the bitmap
                data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, attributes.width, attributes.height), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);

                // get a pointer into the managed bitmap buffer
                PixelData* BitmapPtr = (PixelData*)data.Scan0.ToPointer();

                // the bitmap Scan0 buffer is likely an aligned image
                int rows = data.Height;
                int columns = data.Width;
                int spacing = data.Stride;

                Byte pixel = 0;
                int offset = 0;
                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        // get our bitmap rgb color pixel
                        PixelData* pRGBPixel = (PixelData*)(BitmapPtr + c);
                        // get our raw gray scale image pixel
                        pixel = (Byte)pixels[offset + c];
                        // clip if needed to avoid access violation
                        // and the pixel is shifted down to 8 bit
                        if (pixel > 255) pixel = 255;
                        if (inversion != null)
                        {
                            pixel = inversion[pixel];
                        }
                        // if we are applying overlays and have one
                        if (overlays != null)
                        {
                            pixel = overlays[offset + c] > 0 ? (byte)255 : (byte)pixel;
                        }
                        pRGBPixel->blue = pixel;
                        pRGBPixel->red = pixel;
                        pRGBPixel->green = pixel;
                    }
                    // take account of stride in the output
                    BitmapPtr = (PixelData*)((byte*)BitmapPtr + spacing);
                    offset += attributes.stride;
                }
            }
            // this is an internal method, and I will catch in the caller.
            finally
            {
                // I want to make sure that I unlock the bitmap
                if (bitmap != null && data != null)
                {
                    bitmap.UnlockBits(data);
                }
            }
            return bitmap;
        }

        private static unsafe void Equalize(ImageAttributes attributes)
        {
            int rows = attributes.height;
            int columns = attributes.width;
            byte[] pixels = (byte[])attributes.buffer;

            int maximum = 0;
            int minimum = Int32.MaxValue;

            byte pixel;
            int offset = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    //pixel = Math.Abs(pixels[offset + c]);
                    pixel = pixels[offset + c];
                    if (maximum < pixel) maximum = pixel;
                    if (minimum > pixel) minimum = pixel;
                }
                offset += attributes.stride;
            }

            int range = maximum - minimum;
            double scale = 255.0 / range;

            offset = 0;
            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < columns; c++)
                {
                    pixel = pixels[offset + c];
                    pixels[offset + c] = (byte)(pixel * scale);
                }
                offset += attributes.stride;
            }
        }

        public static Size Constrain(Size size, Size target)
        {
            Size resize = Size.Empty;
            // calculate each aspect ratio
            double source = (double)size.Width / (double)size.Height;
            double destination = (double)target.Width / (double)target.Height;

            if (source >= destination)
            {
                resize.Width = target.Width;
                resize.Height = (int)((double)target.Width / source);
            }
            else
            {
                resize.Height = target.Height;
                resize.Width = (int)((double)target.Height * source);
            }
            return resize;
        }

        public static Array Crop(short[] input, Size image, Rectangle crop)
        {
            short[] output = new short[crop.Size.Width * crop.Size.Height];

            int top = crop.Top;
            int bottom = top + crop.Height;
            int left = crop.Left;
            int right = left + crop.Width;

            int n = 0;
            for (int r = top; r < bottom; r++)
            {
                for (int c = left; c < right; c++)
                {
                    output[n++] = input[r * image.Width + c];
                }
            }

            return output;
        }

        public static ImageAttributes Crop(ImageAttributes source, Point center, double zoom)
        {
            if (zoom == 1.0)
            {
                return source;
            }

            ImageAttributes cropped = new ImageAttributes();

            cropped.bitsperpixel = source.bitsperpixel;
            cropped.monochrome1 = source.monochrome1;
            cropped.type = source.type;

            cropped.width = cropped.stride = (int)(source.width * zoom);
            cropped.height = (int)(source.height * zoom);
            cropped.spacing = (int)(source.spacing * zoom);

            Rectangle crop = new Rectangle(center.X - cropped.width / 2, center.Y - cropped.height / 2, cropped.width, cropped.height);

            cropped.buffer = Crop((short[])source.buffer, new Size(source.width, source.height), crop);

            return cropped;
        }


    }
}
