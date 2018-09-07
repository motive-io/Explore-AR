// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Motive.AR.LocationServices;
using Motive.Core.Utilities;
using Motive.AR.Models;
using Motive.Unity.AR;

#if MOTIVE_VUFORIA
using Vuforia;
#endif
using System;

namespace Motive.AR.Vuforia
{
    public class VuforiaWorldAdapter : TrackableWorldAdapter
	{
        public Text TrackingState;
        public Text VuforiaCameraPosition;
        public Text VuforiaCameraRotation;

        public Text LocationCameraPosition;
        public Text LocationCameraRotation;

        public VuforiaBehaviour ARCamera;

        public bool DebugInSync;

#if MOTIVE_VUFORIA
        PositionalDeviceTracker m_positionalDeviceTracker;
        bool m_adjustSwivel;
        float m_syncTime;

        protected override void Awake()
        {
            VuforiaARController.Instance.RegisterVuforiaStartedCallback(VuforiaStarted);

            base.Awake();
        }

        TrackableBehaviour.Status m_devicePoseStatus;

        void VuforiaStarted()
        {            
            m_positionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

            if (m_positionalDeviceTracker != null)
            {
                //var anchor = m_positionalDeviceTracker.CreateMidAirAnchor("mid air", WorldAnchor.transform);
            }

            DeviceTrackerARController.Instance.RegisterDevicePoseStatusChangedCallback((status) =>
            {
                m_devicePoseStatus = status;
            });

            DeviceTrackerARController.Instance.RegisterDevicePoseUpdatedCallback(() =>
            {
                m_syncTime = Time.time;
            });

            m_adjustSwivel = true;
        }

        void Dbg(Text text, string format, params object[] args)
        {
            if (!text)
            {
                return;
            }

            var msg = string.Format(format, args);

            text.text = msg;
        }

        public override void Activate()
        {
            m_syncTime = float.MinValue;

            base.Activate();
        }

        public bool IsInSync
        {
            get
            {
                if (Application.isMobilePlatform)
                {
                    return (Time.time - m_syncTime) < 0.1f;
                }

                return DebugInSync;
            }
        }

        protected override void Update()
        {
            var trackables = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();

            m_logger.Verbose("active={0} status={1}", 
                           m_positionalDeviceTracker != null ? m_positionalDeviceTracker.IsActive.ToString() : "null",
                          m_devicePoseStatus);

            /*
            Dbg(TrackingState, "active={0} status={1}",
                           m_positionalDeviceTracker != null ? m_positionalDeviceTracker.IsActive.ToString() : "null",
                m_devicePoseStatus);*/

            Dbg(TrackingState, IsInSync ? "<color=green>IN SYNC</color>" : "<color=red>OUT OF SYNC</color>");
            
            //Dbg(VuforiaCameraPosition, "vuforia cam position={0}", VuforiaWorld.Instance.ARCamera.transform.localPosition);
            //Dbg(VuforiaCameraRotation, "vuforia cam rotation={0}",
            //    GetWorldHeading(VuforiaWorld.Instance.WorldAnchor.transform,
            //                    VuforiaWorld.Instance.ARCamera.transform));
                
            Dbg(LocationCameraPosition, "world cam position={0}", WorldCamera.transform.localPosition);
            Dbg(LocationCameraRotation, "world cam rotation={0}",
                GetWorldHeading(WorldAnchor.transform, WorldCamera.transform));
            
            m_adjustSwivel = !IsInSync;

            UpdateObjects(IsInSync);

            base.Update();
        }

        void LateUpdate()
        {
            if (IsActive && m_adjustSwivel && IsInSync)
            {
                // Keep cameras lined up
                var worldCamHdg = GetWorldHeading(WorldAnchor.transform, WorldCamera.transform);
                var vuforiaHdg = GetWorldHeading(WorldAnchor.transform, ARCamera.transform);

                var delta = MathHelper.GetDegreesInRange(worldCamHdg - vuforiaHdg);

                ARCamera.transform.parent.localRotation =
                    Quaternion.Euler(0, delta + ARCamera.transform.parent.localRotation.eulerAngles.y
                /*+ transform.localEulerAngles.z*/, 0);

                m_adjustSwivel = false;
            }
        }

        protected override Vector3 GetWorldDelta()
        {
            if (IsInSync)
            {
                return Vector3.zero;
            }

            return base.GetWorldDelta();
        }

        public override Camera GetCameraForObject(SceneARWorldObject worldObj)
        {
            SceneWorldObjectState state = null;

            if (m_sceneObjects.TryGetValue(worldObj, out state))
            {
                if (state.IsTracking)
                {
                    return WorldCamera;
                }
            }

            return base.GetCameraForObject(worldObj);
        }
#endif
    }
}
