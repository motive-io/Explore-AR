// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.UI.Framework;
using Motive.Unity.Webcam;
using UnityEngine;

namespace Motive.Unity.AR
{
    /// <summary>
    /// An AR world adapter that uses GPS tracking for location and
    /// scene AR objects.
    /// </summary>
    public class DefaultWorldAdapter : LocationARWorldAdapterBase
    {
        private bool m_doUpdate;

        public CameraGyroscope GyroCamera;
        public GameObject Cameras;
        public WebCamPreview BackgroundTexturePreview;

        protected override void Awake()
        {
            if (!GyroCamera)
            {
                GyroCamera = GetComponentInChildren<CameraGyroscope>();
            }

            base.Awake();

			Deactivate();
        }

        public override void Activate()
        {
            m_doUpdate = true;
            Cameras.SetActive(true);
            WorldAnchor.SetActive(true);

            if (GyroCamera)
            {
                GyroCamera.CalibrateCompass();
            }

            if (BackgroundTexturePreview)
            {
                BackgroundTexturePreview.StartCamera();
            }

            base.Activate();
        }

        public override void Deactivate()
        {
            Cameras.SetActive(false);
            WorldAnchor.SetActive(false);

            if (BackgroundTexturePreview)
            {
                BackgroundTexturePreview.StopCamera();
            }

            base.Deactivate();
        }
    }
}
