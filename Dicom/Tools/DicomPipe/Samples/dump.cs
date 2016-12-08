using System;
using EK.Capture.Dicom.DicomToolKit;

public class Script
{
    // this script logs all pdus to the Debug output
    public static bool OnPdu(EK.Capture.Dicom.DicomToolKit.ProtocolDataUnit pdu)
    {
        Logging.Log(LogLevel.Verbose, pdu.Name);
        string text = String.Format("{0} bytes.", pdu.Length);
        if (pdu.Length < 8192)
        {
            text = pdu.ToText();
        }
        Logging.Log(LogLevel.Verbose, text);

        // we do not change the pdu so we return false
        return false;
    }
}