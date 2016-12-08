using System;
using EK.Capture.Dicom.DicomToolKit;

public class Script
{
    public static bool OnPdu(EK.Capture.Dicom.DicomToolKit.ProtocolDataUnit pdu)
    {
        // delay for half a second
        System.Threading.Thread.Sleep(300);
        return false;
    }
}