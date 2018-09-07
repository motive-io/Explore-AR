using Motive.Core;
using Motive.Core.Utilities;
using System;
using UnityEngine;
#if MOTIVE_VUFORIA && MOTIVE_USE_QR_TOKENS
using Vuforia;
using ZXing;
#endif

namespace Motive.AR.Vuforia
{
    public class VuforiaQRTokenScanner : Singleton<VuforiaQRTokenScanner>
    {
#if MOTIVE_VUFORIA && MOTIVE_USE_QR_TOKENS
        private BarcodeReader m_barcodeReader;
        Action<string> m_onTokenScanned;
        Image.PIXEL_FORMAT m_pixelFormat;
        RGBLuminanceSource.BitmapFormat m_bitmapFormat;

        int m_frameCaptureMod = 5;
        int m_frameCaptureCount = 0;

        public VuforiaQRTokenScanner()
        {
            m_barcodeReader = new BarcodeReader();


#if UNITY_EDITOR
            m_pixelFormat = Image.PIXEL_FORMAT.GRAYSCALE; // Need Grayscale for Editor
            m_bitmapFormat = RGBLuminanceSource.BitmapFormat.Gray8;
#else
            m_pixelFormat = Image.PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
            m_bitmapFormat = RGBLuminanceSource.BitmapFormat.RGB24;
#endif
        }

        public void StartScanning(Action<string> onScan)
        {
            m_onTokenScanned = onScan;

            if (!CameraDevice.Instance.SetFrameFormat(m_pixelFormat, true))
            {
                Debug.LogError("Could not set frame format");
            }

            m_frameCaptureCount = 0;

            VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(TryScanToken);
        }

        public void StopScanning()
        {
            CameraDevice.Instance.SetFrameFormat(m_pixelFormat, false);

            VuforiaARController.Instance.UnregisterTrackablesUpdatedCallback(TryScanToken);
        }

        void TryScanToken()
        {
            if (m_frameCaptureCount++ % m_frameCaptureMod != 0)
            {
                return;
            }

            var img = CameraDevice.Instance.GetCameraImage(m_pixelFormat);

            if (img == null)
            {
                return;
            }
            
            Result result = m_barcodeReader.Decode(img.Pixels, img.Width, img.Height, m_bitmapFormat);

            if (result == null)
            {
                return;
            }

            var shareToken = ShareTokenManager.Instance.ParseOutToken(result.Text);
            if (shareToken != null)
            {
                Debug.Log("Share TOKEN: " + shareToken);

                StopScanning();

                if (m_onTokenScanned != null)
                {
                    m_onTokenScanned(shareToken);
                }
            }
            else
            {
                Debug.LogWarning("qr code found, not a share token. qr text: " + result.Text);
            }
        }
#endif
    }
}
