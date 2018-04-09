﻿// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using Motive.AR.Models;
using System;

#if MOTIVE_ARKIT
using UnityEngine.XR.iOS;
#endif

namespace Motive.Unity.AR
{
	public class ARKitWorldAdapter : TrackableWorldAdapter
    {
		public ARKitCameraManager CameraManager;

		public Text TrackingState;
		public Text RotationState;
		public Text UsingARKitText;

#if MOTIVE_ARKIT

		protected override void Awake()
		{
			base.Awake ();

			UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
		}

		public override void Initialize()
        {
            CameraManager.Initialize();

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

            //m_logger.Debug("Frame updated trackingState={0} gps anchor={1}",
            //    camera.trackingState, (m_gpsAnchorCoords == null) ? "null" : m_gpsAnchorCoords.ToString());
            
            UpdateObjects(isArkitTracking);
		}

        public override void Activate ()
        {
            base.Activate();
			
			CameraManager.Resume();

			WorldCamera.enabled = true;
		}

		public override void Deactivate ()
		{
			CameraManager.Pause();

			WorldCamera.enabled = false;

			base.Deactivate ();
		}
        
		void MoveToCamera(Transform transform, bool resetHeight)
		{
			var y = resetHeight ? WorldCamera.transform.position.y : TrackingAnchor.transform.position.y;

			// X, Z come from World Camera, Y comes from World Anchor
			transform.position = 
				new Vector3(WorldCamera.transform.position.x, y, WorldCamera.transform.position.z);
		}

		protected override void EnableTracking()
		{
			CameraManager.EnableCameraTracking = true;
			CameraGyro.StopGyro();
			m_needsSetAnchor = true;
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
		}
#endif
    }
}
