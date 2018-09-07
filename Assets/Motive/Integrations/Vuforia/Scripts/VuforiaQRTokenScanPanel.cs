using Motive.UI.Framework;
using Motive.Unity.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.AR.Vuforia
{
    public class VuforiaQRTokenScanPanel : Panel
    {
        bool m_isScanning;

#if MOTIVE_VUFORIA && MOTIVE_USE_QR_TOKENS
        public override void DidShow()
        {
            base.DidShow();

            VuforiaQRTokenScanner.Instance.StartScanning(DidScanToken);
            m_isScanning = true;
        }

        public override void DidHide()
        {
            base.DidHide();

            VuforiaQRTokenScanner.Instance.StopScanning();
            m_isScanning = false;
        }

        void DidScanToken(string token)
        {
            if (token != null)
            {
                m_isScanning = false;

                WebServices.Instance.AuthenticateQRToken(token, (success, model) =>
                {
                    ThreadHelper.Instance.CallOnMainThread(() =>
                    {
                        if (success)
                        {
                            Close();

                            DynamicConfig.Instance.SetStatePublicSpace(model.Space, model.ProjectConfig);
                        }
                    });
                });
            }
        }
#endif
    }
}
