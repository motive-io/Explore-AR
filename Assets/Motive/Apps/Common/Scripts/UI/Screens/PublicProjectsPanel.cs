using Motive.Core.Models;
using Motive.UI.Framework;
using Motive.Unity.Utilities;
using Motive.Unity.Webcam;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// QR Code scanner for shared configs.
    /// </summary>
    class PublicProjectsPanel : Panel<string>
    {
        public UIWebCamPreview m_UIWebCamPreview;

        public GameObject[] ShowOnError;
        public GameObject[] ShowOnSuccess;

        private QRTokenSpaceModel m_scannedToken;

#if MOTIVE_USE_QR_CODE
        public void ScanForQRCode()
        {
            ObjectHelper.SetObjectsActive(ShowOnError, false);
            ObjectHelper.SetObjectsActive(ShowOnSuccess, false);

            m_scannedToken = null;

            if (!m_UIWebCamPreview)
            {
                Debug.LogError("No Raw Image set.");
                return;
            }

            QRTokenScanner.Instance.Scan(m_UIWebCamPreview, OnQRCodeAuthResponse);
        }

        public override void DidPush(object data)
        {
            ObjectHelper.SetObjectsActive(ShowOnError, false);
            ObjectHelper.SetObjectsActive(ShowOnSuccess, false);

            if (data == null)
            {
                ScanForQRCode();
            }

            base.DidPush(data);
        }

        public override void Populate(string data)
        {
            base.Populate(data);

            if (!string.IsNullOrEmpty(Data))
            {
                QRTokenScanner.Instance.AuthQRToken(Data, OnQRCodeAuthResponse);
            }
        }

        public void Launch()
        {
            if (m_scannedToken != null)
            {
                Close();

                DynamicConfig.Instance.SetStatePublicSpace(m_scannedToken.Space, m_scannedToken.ProjectConfig);
            }
        }

        public void OnQRCodeAuthResponse(bool success, QRTokenSpaceModel qrTokModel)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
            {
                if (success)
                {
                    m_scannedToken = qrTokModel;

                    ObjectHelper.SetObjectsActive(ShowOnSuccess, true);
                }
                else
                {
                    ObjectHelper.SetObjectsActive(ShowOnError, true);
                }
            });
        }
#endif
    }
}