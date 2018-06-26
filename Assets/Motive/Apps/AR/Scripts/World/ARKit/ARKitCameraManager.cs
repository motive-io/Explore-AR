// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if MOTIVE_ARKIT
using UnityEngine.XR.iOS;
#endif

namespace Motive.Unity.AR
{
    /// <summary>
    /// Cloned from UnityARCameraManager with some additional features to support Motive integration.
    /// </summary>
    public class ARKitCameraManager : MonoBehaviour
    {

        public Camera m_camera;

#if MOTIVE_ARKIT
        private UnityARSessionNativeInterface m_session;
        private Material savedClearMaterial;

        [Header("AR Config Options")]
        public UnityARAlignment startAlignment = UnityARAlignment.UnityARAlignmentGravity;
        public UnityARPlaneDetection planeDetection = UnityARPlaneDetection.Horizontal;
        public bool getPointCloud = true;
        public bool enableLightEstimation = true;

        public bool IsInitialized { get; private set; }
        public bool IsRunning { get; private set; }

        public bool EnableCameraTracking;
        public bool IgnoreProjectionMatrix;

        // Update is called once per frame

        public void SetPose()
        {
            if (EnableCameraTracking)
            {
                if (m_camera != null)
                {
                    // JUST WORKS!
                    Matrix4x4 matrix = m_session.GetCameraPose();
                    m_camera.transform.localPosition = UnityARMatrixOps.GetPosition(matrix);
                    m_camera.transform.localRotation = UnityARMatrixOps.GetRotation(matrix);

                    if (!IgnoreProjectionMatrix)
                    {
                        m_camera.projectionMatrix = m_session.GetCameraProjection();
                    }
                }
            }
        }

        public void Initialize()
        {
            m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

#if !UNITY_EDITOR
		Application.targetFrameRate = 60;

		StartRunning();

		if (m_camera == null) {
		m_camera = Camera.main;
		}
#else
            //put some defaults so that it doesnt complain
            UnityARCamera scamera = new UnityARCamera();
            scamera.worldTransform = new UnityARMatrix4x4(new Vector4(1, 0, 0, 0), new Vector4(0, 1, 0, 0), new Vector4(0, 0, 1, 0), new Vector4(0, 0, 0, 1));
            Matrix4x4 projMat = Matrix4x4.Perspective(60.0f, 1.33f, 0.1f, 30.0f);
            scamera.projectionMatrix = new UnityARMatrix4x4(projMat.GetColumn(0), projMat.GetColumn(1), projMat.GetColumn(2), projMat.GetColumn(3));

            UnityARSessionNativeInterface.SetStaticCamera(scamera);

#endif

            IsInitialized = true;
        }

        public void Pause()
        {
            if (IsRunning)
            {
                m_session.Pause();

                IsRunning = false;
            }
        }

        public void Resume()
        {
            if (!IsRunning)
            {
                StartRunning();
            }
        }

        void StartRunning()
        {
#if !UNITY_EDITOR
		ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
		config.planeDetection = planeDetection;
		config.alignment = startAlignment;
		config.getPointCloudData = getPointCloud;
		config.enableLightEstimation = enableLightEstimation;
		m_session.RunWithConfig(config);
#endif

            IsRunning = true;
        }

#endif
    } 
}
