
The program automatically starts with the most recently used values when launched.

Add scripts to the same folder as the executable.

If you change something Stop and Start DicomPipe to have the change take effect.


The script needs to have a public class called Script that has a method with the following signature

	public static bool OnPdu(EK.Capture.Dicom.DicomToolKit.ProtocolDataUnit pdu)

The method should return true if it changed the pdu, or false otherwise.




using System;
using EK.Capture.Dicom.DicomToolKit;

public class Script
{
    public static bool OnPdu(EK.Capture.Dicom.DicomToolKit.ProtocolDataUnit pdu)
    {
        switch (pdu.PduType)
        {
            case ProtocolDataUnit.Type.A_ASSOCIATE_RQ:
                {
                    AssociateRequestPdu request = pdu as AssociateRequestPdu;
                }
                break;
            case ProtocolDataUnit.Type.A_ASSOCIATE_AC:
                {
                    AssociateRequestPdu response = pdu as AssociateRequestPdu;
                }
                break;
            case ProtocolDataUnit.Type.A_ASSOCIATE_RJ:
                {
                    AssociateRejectPdu reject = pdu as AssociateRejectPdu;
                }
                break;
            case ProtocolDataUnit.Type.P_DATA_TF:
                {
                    PresentationDataPdu message = pdu as PresentationDataPdu;
                }
                break;
            case ProtocolDataUnit.Type.A_RELEASE_RQ:
                {
                    AssociationReleasePdu request = pdu as AssociationReleasePdu;
                }
                break;
            case ProtocolDataUnit.Type.A_RELEASE_RP:
                {
                    AssociationReleasePdu response = pdu as AssociationReleasePdu;
                }
                break;
            case ProtocolDataUnit.Type.A_ABORT:
                {
                    AssociateAbortPdu abort = pdu as AssociateAbortPdu;
                }
                break;
        }

        return true;
    }
}