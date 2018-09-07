// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using Motive.AR.Models;
using System;
using Motive.Unity.Utilities;
using System.Runtime.InteropServices;

#if MOTIVE_ARKIT
using UnityEngine.XR.iOS;
#endif

namespace Motive.Unity.AR
{
	public class ARKitWorldAdapter : TrackableWorldAdapter
    {
        [DllImport("__Internal")]
        static extern void ARKit_AddTrackableImage(IntPtr pSession, [MarshalAs(UnmanagedType.LPStr)] string imageFile, [MarshalAs(UnmanagedType.LPStr)] string imageName, double width);

        [DllImport("__Internal")]
        private static extern void ARKit_StartWorldTrackingSession(IntPtr nativeSession);

        public bool IsRunning { get; private set; }

		public ARKitCameraManager CameraManager;

		public Text TrackingState;
		public Text RotationState;
		public Text UsingARKitText;

        public GameObject[] ShowWhenTracking;
        public GameObject[] ShowWhenNotTracking;

#if MOTIVE_ARKIT

        private IntPtr m_nativeSessionHandle;
        private UnityARSessionNativeInterface m_session;

		protected override void Awake()
		{
			base.Awake ();

			UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
		}

		public override void Initialize()
        {
            CameraManager.Initialize();

            m_session = UnityARSessionNativeInterface.GetARSessionNativeInterface();

//#if !UNITY_EDITOR && UNITY_IOS
            try
            {
                var itf = UnityARSessionNativeInterface.GetARSessionNativeInterface();

                m_logger.Debug("Got itf");

                var fieldDef = itf.GetType().GetField("m_NativeARSession",
                                                      System.Reflection.BindingFlags.NonPublic |
                                                      System.Reflection.BindingFlags.Instance);

                m_logger.Debug("Got fieldDef {0} from type {1}", fieldDef, itf.GetType());

                m_nativeSessionHandle = (IntPtr)fieldDef.GetValue(itf);

                m_logger.Debug("Got session handle {0}", m_nativeSessionHandle);
            }
            catch (Exception x)
            {
                m_logger.Exception(x);
            }
//#endif

            base.Initialize ();
		}

		bool IsTracking(UnityARCamera arCam)
		{
			// todo: is limited ok for tracking?
            return arCam.trackingState == ARTrackingState.ARTrackingStateNormal ||
                        arCam.trackingState == ARTrackingState.ARTrackingStateLimited;
		}

		public void ARFrameUpdated(UnityARCamera camera)
		{
			if (TrackingState)
			{
				TrackingState.text = camera.trackingState.ToString() + " " + m_resetCount + " " +
					ForegroundPositionService.Instance.Compass.TrueHeading;
			}

			if (WorldLight)
			{
                WorldLight.colorTemperature = camera.lightData.arLightEstimate.ambientColorTemperature;
                WorldLight.intensity = camera.lightData.arLightEstimate.ambientIntensity / 1000f;
			}

            bool isArkitTracking = IsTracking(camera);

            if (isArkitTracking)
            {
                CameraManager.SetPose();
            }

            //m_logger.Debug("Frame updated trackingState={0} gps anchor={1}",
            //    camera.trackingState, (m_gpsAnchorCoords == null) ? "null" : m_gpsAnchorCoords.ToString());

            //Recalibrate();

            ObjectHelper.SetObjectsActive(ShowWhenTracking, isArkitTracking);
            ObjectHelper.SetObjectsActive(ShowWhenNotTracking, !isArkitTracking);

            UpdateObjects(isArkitTracking);
		}

        public override void Activate ()
        {
            base.Activate();
			
			Resume();

            WorldCamera.enabled = true;
		}

		public override void Deactivate ()
		{
			Pause();

			WorldCamera.enabled = false;

			base.Deactivate ();
		}

		protected override void EnableTracking()
		{
			CameraManager.EnableCameraTracking = true;
            CameraGyro.StopGyro(reset:true);
			m_needsSetAnchor = true;
		}

        internal void AddTrackableImage(string imageUrl, string name, double width)
        {
//#if !UNITY_EDITOR && UNITY_IOS
            ARKit_AddTrackableImage(m_nativeSessionHandle, imageUrl, name, width);
//#endif
        }

        protected override void DisableTracking()
		{
			CameraManager.EnableCameraTracking = false;
			CameraGyro.StartGyro();
            CalibrateCompass(WorldAnchor.transform);

			MoveToCamera(WorldAnchor.transform, true);
		}

        protected override void Update()
        {
            base.Update();

            if (RotationState)
			{
				RotationState.text = Platform.Instance.Compass.TrueHeading.ToString("F0");
			}

			if (UsingARKitText)
			{
				UsingARKitText.text = UsingTracking ? "ARKit Active" : "ARKit Inactive";
			}

#if UNITY_EDITOR
            UpdateObjects(true);
#endif            
		}
#endif

        public void Pause()
        {
            if (IsRunning)
            {
#if MOTIVE_ARKIT
                m_session.Pause();
#endif

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
            //#if !UNITY_EDITOR
            //ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();
            //config.planeDetection = planeDetection;
            //config.alignment = startAlignment;
            //config.getPointCloudData = getPointCloud;
            //config.enableLightEstimation = enableLightEstimation;
            //m_session.RunWithConfig(config);
#if MOTIVE_ARKIT
            ARKit_StartWorldTrackingSession(m_nativeSessionHandle);
#endif
//#endif

            IsRunning = true;
        }
    }
}
