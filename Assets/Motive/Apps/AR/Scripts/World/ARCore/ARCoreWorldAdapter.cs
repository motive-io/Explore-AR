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
using UnityEngine.SpatialTracking;

#if MOTIVE_ARCORE
using GoogleARCore;
#endif

namespace Motive.Unity.AR
{
    public class ARCoreWorldAdapter : TrackableWorldAdapter
    {
        public Text TrackingState;
        public Text CompassHeading;
        public Text CameraHeading;
        public Text UsingARCoreState;

        public TrackedPoseDriver PoseDriver;

#if MOTIVE_ARCORE

        public bool IsTracking
        {
            get
            {
                return Session.Status == SessionStatus.Tracking;
            }
        }

        bool m_wasTracking;

        public void UpdateFrame()
        {
            if (TrackingState)
            {
                TrackingState.text = Session.Status.ToString() + " " + m_resetCount + " " +
                    ForegroundPositionService.Instance.Compass.TrueHeading;
            }

            // Logic gets a bit messy switching between tracking/not tracking states.
            // This can definitely be cleaned up, but essentially what's happening:
            // 1. Disable the PoseDriver
            // 2. Enable the Gyro
            // 3. On the next iteration (after the gyro has settled in), re-calibrate
            //    the compass to sync up with the gyro values.
            bool needsCal = false;

            if (!IsTracking && m_needsSetAnchor)
            {
                CalibrateCompass();
                m_needsSetAnchor = false;
            }

            if (IsTracking && !m_wasTracking)
            {
                needsCal = true;
                EnableTracking();
            }
            else if (!IsTracking && m_wasTracking)
            {
                needsCal = true;
                DisableTracking();
                CalibrateCompass();
            }

            m_wasTracking = IsTracking;
            
            UpdateObjects(IsTracking);

            // TODO: re-run set anchor if we've just enabled tracking..
            m_needsSetAnchor = needsCal;
        }

        public override void Activate()
        {
            try
            {
                base.Activate();

                WorldCamera.enabled = true;

                ResetWorldHeight();
            }
            catch (Exception x)
            {
                Debug.LogException(x);
            }
        }

        public override void Deactivate()
        {
            WorldCamera.enabled = false;

            base.Deactivate();
        }
        
        protected override void EnableTracking()
        {
            CameraGyro.StopGyro();
            CameraGyro.ResetGyro();

            PoseDriver.enabled = true;

            m_needsSetAnchor = true;

            //MoveToCamera(WorldAnchor.transform, true);
            ResetWorldHeight();
        }

        protected override void DisableTracking()
        {
            //var rotation = TransformHelper.GetForwardAngle(WorldCamera.transform, WorldAnchor.transform);

            //WorldCamera.transform.localRotation = Quaternion.identity;

            PoseDriver.enabled = false;

            CameraGyro.StartGyro(/*Quaternion.Euler(Vector3.up * rotation)*/);
            //CameraGyro.CalibrateCompass();

            /*
            var pos = Get2DPosition(GetCamera().transform);

            WorldAnchor.transform.RotateAround(pos, Vector3.up, -rotation);
            */

            //MoveToCamera(WorldAnchor.transform, true);
        }

        protected override void Update()
        {
            base.Update();

            if (CompassHeading)
            {
                CompassHeading.text = Platform.Instance.Compass.TrueHeading.ToString("F0");
            }

            if (CameraHeading)
            {
                var rotation = TransformHelper.GetForwardAngle(WorldCamera.transform, WorldAnchor.transform);

                CameraHeading.text = rotation.ToString("F0");
            }

            if (UsingARCoreState)
            {
                UsingARCoreState.text = UsingTracking ? "ARCore Active" : "ARCore Inactive";
            }

            UpdateFrame();
        }
#endif
    }
}
