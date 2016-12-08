using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EK.Capture.Dicom.DicomToolKit.Test
{
    class Tools
    {
        /// <summary>
        /// The local path up to \ImageProcessing
        /// </summary>
        public static string RootFolder
        {
            get
            {
                string fragment = @"ImageProcessing";
                string folder = Directory.GetCurrentDirectory();
                folder = folder.Substring(0, folder.IndexOf(fragment)+fragment.Length);
                return folder;
            }
        }
    }
}
