using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZBD
{
    class AndroidPluginCallback : AndroidJavaProxy
    {

        public AndroidPluginCallback() : base("io.zebedee.zbdsdk.RequestIdCallback")
        {
        }

        public void onRequestIdReceived(string result, string errorMessage)
        {
            ZBDFingerprintCallback fC = new ZBDFingerprintCallback();
            fC.requestId = result;
            fC.error = errorMessage;
            Utilities.Instance.fingerPrintCallback(fC);

        }

    }
}
