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

        // return true if you change the pdu, false if you did not
        return true;
    }
}