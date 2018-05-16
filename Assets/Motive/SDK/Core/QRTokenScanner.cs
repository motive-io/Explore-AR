using Motive.Core;
using Motive.Core.Models;
using Motive.Unity.Utilities;
using Motive.Unity.Webcam;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if MOTIVE_USE_QR_CODE
using ZXing;
#endif
/// <summary>
/// Credit: https://github.com/codebude/QRCoder
/// </summary>
namespace Motive.Unity
{
    public class QRTokenScannedEvent : UnityEvent<string> { }

    /// <summary>
    /// 
    /// </summary>
    class QRTokenScanner : SingletonComponent<QRTokenScanner>
    {
#if MOTIVE_USE_QR_CODE
        public QRTokenScannedEvent TokenScanned;

        private BarcodeReader m_barcodeReader;

        private UIWebCamPreview m_webCam;
        private Action<bool, QRTokenSpaceModel> m_onQRCodeResponse;
        private RawImage m_rawImage;
        private bool m_isScanning;
        private bool m_alternateUpdate;


        protected override void Start()
        {
            base.Start();

            m_barcodeReader = new BarcodeReader();
            TokenScanned = new QRTokenScannedEvent();
            TokenScanned.AddListener(AuthQRToken_callback);

        }

        public void Scan(UIWebCamPreview _camPreview, Action<bool, QRTokenSpaceModel> _onQRCodeAuthenticated)
        {
            if (!_camPreview) return;
            m_webCam = _camPreview;
            m_rawImage = _camPreview.PreviewImage;

            m_onQRCodeResponse = _onQRCodeAuthenticated;

            m_webCam.StartCamera();
            m_isScanning = true;
        }

        void Update()
        {
            // Update() half as often
            m_alternateUpdate = !m_alternateUpdate;
            if (m_alternateUpdate) return;

            if (!m_isScanning || !m_rawImage || !m_webCam || !m_webCam.PreviewImage || !m_webCam.PreviewImage.texture) return;

            try
            {

                CameraManager.Instance._camera.TakePicture(m_rawImage.texture.width, m_rawImage.texture.height, (tex2D) =>
                {
                    if (tex2D == null) return;
                    var pixels = tex2D.GetPixels32();

                    if (pixels == null ||
                        tex2D.width <= 16)
                    {
                        Debug.Log("No pixels loaded.");
                        return;
                    }

                    Result result = m_barcodeReader.Decode(pixels, tex2D.width, tex2D.height);
                    if (result == null)
                    {
                        return;
                    }

                    var shareToken = ShareTokenManager.Instance.ParseOutToken(result.Text);
                    if (shareToken != null)
                    {
                        Debug.Log("Share TOKEN: " + shareToken);

                        m_webCam.StopCamera();
                        this.TokenScanned.Invoke(shareToken);
                        m_isScanning = false;
                    }
                    else
                    {
                        Debug.LogWarning("qr code found, not a share token. qr text: " + result.Text);
                    }

                });

            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private void TearDown()
        {
            m_onQRCodeResponse = null;
            if (m_webCam) m_webCam.StopCamera();
            m_webCam = null;
            m_isScanning = false;
        }

        /// <summary>
        /// Sends http req.
        /// 
        /// Is listening for QRTokenScannedEvent
        /// Listens for 
        /// </summary>
        /// <param name="token"></param>
        public void AuthQRToken(string token, Action<bool, QRTokenSpaceModel> _onResponse = null)
        {
            Action<bool, QRTokenSpaceModel> onResponse = m_onQRCodeResponse;

            if (_onResponse != null)
            {
                onResponse = _onResponse;
            }

            Motive.WebServices.Instance.AuthenticateQRToken(token, onResponse);
        }

        protected void AuthQRToken_callback(string token)
        {
            AuthQRToken(token, null);
        }
#endif
    }
}
