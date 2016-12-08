using System;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <remarks>
    /// DICOM grayscale standard display functions
    /// </remarks>
    public class Gsdf
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public Gsdf()
        {
        }

        // Public static methods:

        /// <summary>
        /// Convert the JND index into log10 luminance according to the Grayscale Standard Display Function.
        /// Input is a JND index in the range 1-1023 
        /// Output is log10 of luminance in the range .05-3985.913 cd/m^2
        /// </summary>
        /// <param name="dJND">Just Noticable Difference value.</param>
        /// <returns>Log base 10 luminance value.</returns>
        public static double JNDToLog10Lum(double dJND)
        {
            // Coefficients for the rational approximation
            const double da = -1.30118770000;
            const double db = -0.02584019100;
            const double dc = 0.08024263600;
            const double dd = -0.10320229000;
            const double de = 0.13646699000;
            const double df = 0.02874562000;
            const double dg = -0.02546840400;
            const double dh = -0.00319789770;
            const double dk = 0.00012992634;
            const double dm = 0.00136353340;

            double dln = Math.Log(dJND);

            double dNum = da + dln * (dc + dln * (de + dln * (dg + dln * dm)));
            double dDen = 1.0 + dln * (db + dln * (dd + dln * (df + dln * (dh + (dln * dk)))));

            double dLog10Lum = dNum / dDen;
            return dLog10Lum;
        }

        /// <summary>
        /// Convert log10 luminance into the JND index according to the Grayscale Standard Display Function.
        /// Input is log10 of luminance in the range .05-3985.913 cd/m^2
        /// Output is a JND index in the range 1-1023 
        /// </summary>
        /// <param name="dLog10Lum">Log base 10 luminance value to be converted to JND index.</param>
        /// <returns>Just Noticable Difference index.</returns>
        public static double Log10LumToJND(double dLog10Lum)
        {
            // Coefficients for the approximation of the inverse
            const double dA = 71.498068000;
            const double dB = 94.593053000;
            const double dC = 41.912053000;
            const double dD = 9.824700400;
            const double dE = 0.281754070;
            const double dF = -1.187845500;
            const double dG = -0.180143490;
            const double dH = 0.147108990;
            const double dI = -0.017046845;

            const double dTol = 0.0005;

            double dAvg = 0.0;
            double dLo = 0.0;
            double dHi = 0.0;
            double dVal = 0.0;

            dAvg = dA + dLog10Lum * (dB + dLog10Lum * (dC + dLog10Lum * (dD + dLog10Lum * (dE + dLog10Lum * (dF + dLog10Lum * (dG + dLog10Lum * (dH + dLog10Lum * dI)))))));

            // Set the bounding hi/lo values to +/- .20 of dAvg
            dLo = dAvg - .20;
            dHi = dAvg + .20;

            if (dLo < 1.0) dLo = 1.0;
            if (dHi > 1023.0) dHi = 1023.0;

            if (dLo > dHi) dLo = dHi;
            if (dHi < dLo) dHi = dLo;

            // Iterate to refine the estimate
            while (true)
            {
                dVal = JNDToLog10Lum(dAvg);

                if (dVal < dLog10Lum)
                {
                    dLo = dAvg;
                }
                else if (dVal > dLog10Lum)
                {
                    dHi = dAvg;
                }
                else
                {
                    break;
                }

                dAvg = (dLo + dHi) / 2.0;

                if (dHi - dLo < dTol)
                {
                    break;
                }
            }

            return dAvg;
        }

        /// <summary>
        /// Uses the parameters describing hardcopy lightbox viewing, the luminance
        /// and ambient, to convert the density values to normalized p-values via the
        /// grayscale standard display function (GSDF). The p-values are output in the
        /// interval [0,1].
        /// </summary>
        /// <param name="pvalues">Output p-values array.</param>
        /// <param name="density">Input optical density look-up table.</param>
        /// <param name="length">Length of the density and p-values arrays.</param>
        /// <param name="minOD">Minimum optical density, e.g., 0.21.</param>
        /// <param name="maxOD">Maximum optical density, e.g. 3.00.</param>
        /// <param name="lightboxLuminance">Lightbox luminance in candelas per square meter.</param>
        /// <param name="lightboxAmbient">Ambient luminance in candelas per square meter.</param>
        public static void DensityToPvalues(double[] pvalues, double[] density, int length, double minOD,
                                            double maxOD, double lightboxLuminance, double lightboxAmbient)
        {
            // Obtain the range of JNDs
            double dMinLum_cd = lightboxAmbient + lightboxLuminance * Math.Pow(10.0, -maxOD);
            double dMaxLum_cd = lightboxAmbient + lightboxLuminance * Math.Pow(10.0, -minOD);
            double dJnd0 = Log10LumToJND(Math.Log10(dMinLum_cd));
            double dJnd1 = Log10LumToJND(Math.Log10(dMaxLum_cd));

            double dScale = 1.0 / (dJnd1 - dJnd0);

            for (int i = 0; i < length; i++)
            {
                double dLum_cd = lightboxAmbient + lightboxLuminance * Math.Pow(10.0, -density[i]);
                double dJnd = Log10LumToJND(Math.Log10(dLum_cd));
                double dPval = (dJnd - dJnd0) * dScale;

                pvalues[i] = dPval;
            }
        }
    }
}
