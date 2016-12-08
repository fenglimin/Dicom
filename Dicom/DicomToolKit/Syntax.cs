using System;

namespace EK.Capture.Dicom.DicomToolKit
{
    /// <summary>
    /// Represents the different types of endian-ness.
    /// </summary>
    public enum Endian
    {
        Big,
        Little
    }

    /// <summary>
    /// Represents the different transfer syntaxes.
    /// </summary>
    public class Syntax
    {
        public const string Unknown = "";
        public const string ImplicitVrLittleEndian = "1.2.840.10008.1.2";
        public const string ExplicitVrLittleEndian = "1.2.840.10008.1.2.1";
        public const string ExplicitVrBigEndian =    "1.2.840.10008.1.2.2";

        public const string JPEGBaselineProcess1  = "1.2.840.10008.1.2.4.50";
        public const string JPEGExtendedProcess2n4  = "1.2.840.10008.1.2.4.51"; 
        public const string JPEGProgressiveProcess10n12 = "1.2.840.10008.1.2.4.55";
        public const string JPEGLosslessProcess14 = "1.2.840.10008.1.2.4.57";
        public const string JPEGLosslessProcess15 = "1.2.840.10008.1.2.4.58";
        public const string JPEGLosslessProcess14SelectionValue1  = "1.2.840.10008.1.2.4.70";
        public const string JPEGLSLossless = "1.2.840.10008.1.2.4.80";
        public const string JPEGLSNearlossless = "1.2.840.10008.1.2.4.81";
        public const string JPEG2000Lossless = "1.2.840.10008.1.2.4.90";
        public const string JPEG2000 = "1.2.840.10008.1.2.4.91";

        public const string RLELossless = "1.2.840.10008.1.2.5";

        /// <summary>
        /// Tests whether a syntax is Explicit.
        /// </summary>
        /// <param name="syntax">The syntax uid to test.</param>
        /// <returns>True if Explicit, false otherwise</returns>
        public static bool IsExplicit(string syntax)
        {
            bool result = false;
            switch (syntax)
            {
                case ExplicitVrBigEndian:
                case ExplicitVrLittleEndian:
                case JPEGBaselineProcess1:
                case JPEGExtendedProcess2n4:
                case JPEGProgressiveProcess10n12:
                case JPEGLosslessProcess14:
                case JPEGLosslessProcess15:
                case JPEGLosslessProcess14SelectionValue1:
                case JPEGLSLossless:
                case JPEGLSNearlossless:
                case JPEG2000Lossless:
                case JPEG2000:
                case RLELossless:
                    result = true;
                    break;
                case ImplicitVrLittleEndian:
                    result = false;
                    break;
                default:
                    throw new Exception(String.Format("Unknown syntax {0}.", syntax));
            }
            return result;
        }

        /// <summary>
        /// Returns the endian-ness of the specified syntax.
        /// </summary>
        /// <param name="syntax">The syntax to check.</param>
        /// <returns>The endian-ness of the syntax.</returns>
        /// <exception cref="System.Exception">An unknown syntax.</exception>
        public static Endian GetEndian(string syntax)
        {
            switch (syntax)
            {
                case ExplicitVrLittleEndian:
                case ImplicitVrLittleEndian:
                case JPEGBaselineProcess1:
                case JPEGExtendedProcess2n4:
                case JPEGProgressiveProcess10n12:
                case JPEGLosslessProcess14:
                case JPEGLosslessProcess15:
                case JPEGLosslessProcess14SelectionValue1:
                case JPEGLSLossless:
                case JPEGLSNearlossless:
                case JPEG2000Lossless:
                case JPEG2000:
                case RLELossless:
                    return Endian.Little;
                case ExplicitVrBigEndian:
                    return Endian.Big;
                default:
                    throw new Exception(String.Format("Unknown syntax {0}.", syntax));
            }
        }

        /// <summary>
        /// Tests whether a syntax can support Encapsulated Pixel Data.
        /// </summary>
        /// <param name="syntax">The syntax uid to test.</param>
        /// <returns>True if the syntax supporst encapsulated pixel data, false otherwise</returns>
        public static bool CanEncapsulatePixelData(string syntax)
        {
            bool result = false;
            switch (syntax)
            {
                case JPEGBaselineProcess1:
                case JPEGExtendedProcess2n4:
                case JPEGProgressiveProcess10n12:
                case JPEGLosslessProcess14:
                case JPEGLosslessProcess15:
                case JPEGLosslessProcess14SelectionValue1:
                case JPEGLSLossless:
                case JPEGLSNearlossless:
                case JPEG2000Lossless:
                case JPEG2000:
                case RLELossless:
                    result = true;
                    break;
                case ExplicitVrBigEndian:
                case ExplicitVrLittleEndian:
                case ImplicitVrLittleEndian:
                    result = false;
                    break;
                default:
                    throw new Exception(String.Format("Unknown syntax {0}.", syntax));
            }
            return result;
        }
    }
}
