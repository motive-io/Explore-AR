// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using Motive.AR.Models;

#if MOTIVE_ARCORE
using GoogleARCore;
#endif

namespace Motive.Unity.AR
{
    public class ARCoreWorldAdapter : TrackableWorldAdapter
    {
        public Text TrackingState;
        public Text RotationState;
        public Text UsingARCoreState;

#if MOTIVE_ARCORE

        public bool IsTracking
        {
            get
            {
                return Session.Status == SessionStatus.Tracking;
            }
        }

        public void UpdateFrame()
        {
            if (TrackingState)
            {
                TrackingState.text = Session.Status.ToString() + " " + m_resetCount + " " +
                    ForegroundPositionService.Instance.Compass.TrueHeading;
            }

            UpdateObjects(IsTracking);
        }

        public override void Activate()
        {
            base.Activate();

            WorldCamera.enabled = true;
        }

        public override void Deactivate()
        {
            WorldCamera.enabled = false;

            base.Deactivate();
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
            CameraGyro.StopGyro();
            m_needsSetAnchor = true;
        }

        protected override void DisableTracking()
        {
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

            if (UsingARCoreState)
            {
                UsingARCoreState.text = UsingTracking ? "ARCore Active" : "ARCore Inactive";
            }

            UpdateFrame();
        }
#endif
    }
}
