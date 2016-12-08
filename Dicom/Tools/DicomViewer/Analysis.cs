using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace DicomViewer
{
    public partial class Analysis : Form
    {
        private int[] histogram = null;

        public Analysis(int[] histogram)
        {
            this.histogram = histogram;
            InitializeComponent();
        }

        private void Analysis_Load(object sender, EventArgs e)
        {
            int[] smoothed = SmoothHistogram(histogram);
            PictureBox.Image = DrawHistogram(smoothed);
        }

        private static int[] SmoothHistogram(int[] histogram)
        {

            // Smooth the histogram.
            int i = 0;
            int winSize = 24;
            int winHalf = winSize / 2;
            double intermediateResult = 0.0;
            double maxInt = (double)Int32.MaxValue;
            int[] smoothed = new int[histogram.Length];

            for (i = 0; i < histogram.Length; i++)
            {
                intermediateResult = 0.0;
                int j = 0;
                bool replicating = false;

                for (int k = (i - winHalf); k < (i + winHalf); k++)
                {
                    j = k;
                    if (j < 0) j = 0;
                    else if (j >= histogram.Length) j = histogram.Length - 1;
                    if (!replicating && j != k) replicating = true;
                    intermediateResult += histogram[j];
                }

                if (replicating) intermediateResult /= (double)winSize;
                replicating = false;

                if (intermediateResult < maxInt)
                {
                    smoothed[i] = Convert.ToInt32(intermediateResult);
                }
                else
                {
                    smoothed[i] = Int32.MaxValue;
                }
            }
            return smoothed;
        }

        private Bitmap DrawHistogram(int[] smoothed)
        {
            Size size = PictureBox.Size;

            Bitmap bitmap = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);

            using (Pen pen = new Pen(Color.White))
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Black);

                int i = 0;
                int numCells = 8;
                int cellWidth = (size.Width - 8) / numCells;
                int cellHeight = (size.Height - 8) / numCells;
                int borderX = (size.Width - (cellWidth * numCells)) / 2;
                int borderY = (size.Height - (cellHeight * numCells)) / 2;

                int maximum = 0;
                for (int n = 0; n < smoothed.Length; n++)
                {
                    if (smoothed[n] > maximum) maximum = smoothed[n];
                }


                int width = cellWidth * numCells;
                int height = cellHeight * numCells;
                double slope = (double)width / (double)smoothed.Length;
                int x1 = borderX;

                if (maximum <= 0)
                {
                    throw new Exception();
                }

                int y1 = borderY + height - Convert.ToInt32(((double)height * (double)smoothed[0] / (double)maximum));
                int x2 = 0;
                int y2 = 0;
                int peakY = height + borderY + 1;

                for (i = 1; i < smoothed.Length; i++)
                {
                    x2 = borderX + Convert.ToInt32((slope * (double)(i)));
                    y2 = borderY + height - Convert.ToInt32(((double)height * (double)smoothed[i] / (double)maximum));

                    if (y2 < peakY) peakY = y2;

                    if (x2 != x1)
                    {
                        g.DrawLine(pen, x1, y1, x2, peakY);

                        x1 = x2;
                        y1 = peakY;

                        peakY = height + borderY + 1;
                    }
                }

                // Create the pens.
                using (Pen grayPen = new Pen(Color.FromArgb(96, 96, 96), 1))
                {
                    // Horizontal lines:
                    x1 = borderX;
                    y1 = borderY;
                    x2 = borderX + (cellWidth * numCells);

                    for (i = 0; i <= numCells; i++, y1 += cellHeight)
                    {
                        g.DrawLine(grayPen, x1, y1, x2, y1);
                    }

                    // Vertical lines:
                    y1 = borderY;
                    y2 = borderY + (cellHeight * numCells);
                    for (i = 0; i <= numCells; i++, x1 += cellWidth)
                    {
                        g.DrawLine(grayPen, x1, y1, x1, y2);
                    }
                }

            }
            return bitmap;
        }

    }
}
