// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Utilities;
using Motive.UI.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.Unity.AR
{
    /// <summary>
    /// An base class for AR World Adapters with world-tracking ability,
    /// e.g., ARKit, ARCore, Vuforia Fusion.
    /// </summary>
    public class TrackableWorldAdapter : LocationARWorldAdapterBase
    {
        public GameObject DefaultAnchor;
        public GameObject TrackingAnchor;

        protected enum ObjectTrackingState
        {
            NotSet,
            Tracking,
            NotTracking
        }

        protected class TrackedWorldObjectState
        {
            public bool Added { get; set; }
            public ObjectTrackingState TrackingState { get; set; }
            public GameObject WorldObjectParent { get; set; }
        }

        private bool m_doUpdate;

        public CameraGyroscope CameraGyro;
        public GameObject Cameras;
        public Light WorldLight;

        protected Dictionary<LocationARWorldObject, TrackedWorldObjectState> m_objStates;
        protected Coordinates m_gpsAnchorCoords;
        protected bool m_needsSetAnchor;

        public bool UsingTracking { get; protected set; } 
        
        public override void Initialize()
        {
            m_needsSetAnchor = true;

            m_objStates = new Dictionary<LocationARWorldObject, TrackedWorldObjectState>();

            if (!CameraGyro)
            {
                CameraGyro = WorldCamera.GetComponent<CameraGyroscope>();
            }

            base.Initialize();

            EnableTracking();
        }

        public void Recalibrate()
        {
            SetAnchor();
        }

        public override void Activate()
        {
            m_needsSetAnchor = true;

            Recalibrate();

            base.Activate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
        }

        protected float GetCompassRotation(Transform cameraTransform, Transform swivelTransform, double heading)
        {
            m_logger.Debug("Get rotation for cam=({0}) swivel=({1})",
                cameraTransform.rotation.eulerAngles, swivelTransform.rotation.eulerAngles);

            //// TODO: put this in a common space once I figure out wth it's doing
            /// 
            var globalCamFwd = cameraTransform.TransformPoint(Vector3.forward);
            var globalCamUp = cameraTransform.TransformPoint(Vector3.up);

            var swivelCamFwd = swivelTransform.InverseTransformPoint(globalCamFwd);
            var swivelCamUp = swivelTransform.InverseTransformPoint(globalCamUp);

            // Normalized forward projection in swivel space
            var camFwdProject = new Vector3(swivelCamFwd.x, swivelCamFwd.z).normalized;
            var camUpProject = new Vector3(swivelCamUp.x, swivelCamUp.z);

            var cross = Vector3.Cross(camFwdProject, camUpProject);

            var camHdg = Mathf.Atan2(swivelCamFwd.x, swivelCamFwd.z) * Mathf.Rad2Deg;

            var camTilt = Mathf.Asin(cross.z) * Mathf.Rad2Deg;

            // Adjust heading based on tilt
            var hdgDiff = MathHelper.GetDegreesInRange(camHdg - heading);

            m_logger.Debug("hdgDiff={0} tilt={1}", hdgDiff, camTilt);

            var delta = hdgDiff - camTilt;

            return (float)delta;
            ///////
        }

        protected void CalibrateCompass(Transform worldAnchor)
        {
            var heading = ForegroundPositionService.Instance.Compass.TrueHeading;

            var rotation = GetCompassRotation(WorldCamera.transform, worldAnchor, heading);
            
            m_logger.Debug("BEFORE: heading={0} rotation={1} pivot={2}", heading, rotation, worldAnchor.rotation.eulerAngles);

            worldAnchor.Rotate(new Vector3(0, rotation, 0));
            //WorldPivot.transform.RotateAround(Vector3.up, rotation);
            //WorldPivot.transform.localRotation = Quaternion.Euler(0, 
            //		rotation
            //		/*+ transform.localEulerAngles.z*/, 0);

            m_logger.Debug("AFTER: heading={0} rotation={1} pivot={2}", heading, rotation, worldAnchor.rotation.eulerAngles);
        }

        protected virtual void SetObjectPosition(LocationARWorldObject worldObject, GameObject gameObj, Coordinates coords)
        {
            var pos = GetPosition(worldObject, coords);

            // Set the object to be level with the camera: may want to revisit 
            // this if we do ground surface tracking
            pos.y += (float)worldObject.Elevation;

            gameObj.transform.localPosition = pos;
            gameObj.transform.localScale = Vector3.one;
            gameObj.transform.localRotation = Quaternion.identity;
        }

        protected void SetObjectPosition(LocationARWorldObject worldObject, TrackedWorldObjectState state, Coordinates coords)
        {
            SetObjectPosition(worldObject, state.WorldObjectParent, coords);
        }

        public override double GetDistance(LocationARWorldObject worldObj)
        {
            // TODO: if tracking..
            var dist = (worldObj.GameObject.transform.position - WorldCamera.transform.position).magnitude;

            if (worldObj.Options != null &&
                worldObj.Options.DistanceVariation is LinearDistanceVariation)
            {
                var linVar = (LinearDistanceVariation)worldObj.Options.DistanceVariation;

                dist /= (float)linVar.Scale.GetValueOrDefault(1);
            }

            return dist;
        }

        protected void AdjustTrackedObject(ARWorldObject obj, TrackedWorldObjectState state)
        {
            ApplyOptions(state.WorldObjectParent.transform, WorldCamera, obj.Options);
        }

        protected void AdjustObject(LocationARWorldObject worldObject, bool isCameraTracking, TrackedWorldObjectState state)
        {
            state.WorldObjectParent.SetActive(true);

            bool isTracking = UsingTracking && isCameraTracking;

            if (!isTracking)
            {
                // Use the default rendering. We do this if ARKit stopped tracking.
                if (state.TrackingState != ObjectTrackingState.NotTracking)
                {
                    state.TrackingState = ObjectTrackingState.NotTracking;

                    state.WorldObjectParent.SetActive(false);

                    worldObject.GameObject.transform.SetParent(DefaultAnchor.transform);

                    worldObject.GameObject.transform.localRotation = Quaternion.identity;
                    worldObject.GameObject.transform.localScale = Vector3.one;
                }

                SetPosition(worldObject);
            }
            else
            {
                if (state.TrackingState != ObjectTrackingState.Tracking)
                {
                    state.WorldObjectParent.transform.SetParent(TrackingAnchor.transform);

                    worldObject.GameObject.transform.SetParent(state.WorldObjectParent.transform);

                    worldObject.GameObject.transform.localPosition = Vector3.zero;
                    worldObject.GameObject.transform.localScale = Vector3.one;
                    worldObject.GameObject.transform.localRotation = Quaternion.identity;

                    SetObjectPosition(worldObject, state, m_gpsAnchorCoords);

                    state.TrackingState = ObjectTrackingState.Tracking;
                }

                AdjustTrackedObject(worldObject, state);
            }
        }

        protected void UpdateObjects(bool isCameraTracking)
        {
            if (IsActive)
            {
                m_needsSetAnchor |= !isCameraTracking;

                if (UsingTracking &&
                    m_needsSetAnchor &&
                    ForegroundPositionService.Instance.HasLocationData &&
                    isCameraTracking)
                {
                    SetAnchor();
                    m_needsSetAnchor = false;

                    // Reset all world objects?
                }

                foreach (var obj in m_worldObjects)
                {
                    var state = m_objStates[obj];

                    AdjustObject(obj, isCameraTracking, state);
                }

                Vector3 worldDelta = GetWorldDelta();

                foreach (var state in m_sceneObjects.Values)
                {
                    SetPosition(state, isCameraTracking, worldDelta);
                }
            }
        }

        public override void AddWorldObject(LocationARWorldObject worldObject)
        {
            var state = new TrackedWorldObjectState();

            var parent = new GameObject("AR Object");
            state.WorldObjectParent = parent;
            worldObject.GameObject.transform.SetParent(state.WorldObjectParent.transform);

            worldObject.GameObject.transform.localPosition = Vector2.zero;
            worldObject.GameObject.transform.localScale = Vector3.one;
            worldObject.GameObject.transform.rotation = Quaternion.identity;

            m_objStates.Add(worldObject, state);

            // Leave the object inactive until we parent it to the correct anchor.
            state.WorldObjectParent.SetActive(false);

            base.AddWorldObject(worldObject);
        }

        public override void RemoveWorldObject(LocationARWorldObject worldObject)
        {
            m_objStates.Remove(worldObject);

            base.RemoveWorldObject(worldObject);
        }

        protected virtual void SetPosition(LocationARWorldAdapterBase.SceneWorldObjectState state, bool isCameraTracking, Vector3 worldDelta)
        {
            if (isCameraTracking)
            {
                if (!state.IsTracking)
                {
                    if (!state.IsSpawned)
                    {
                        SpawnObject(state.WorldObject, GetCameraForObject(state.WorldObject), TrackingAnchor.transform);

                        state.IsSpawned = true;
                        state.IsTracking = true;
                    }
                    else
                    {
                        var pos = state.WorldObject.GameObject.transform.localPosition;

                        state.WorldObject.GameObject.transform.SetParent(TrackingAnchor.transform.transform, false);
                        state.WorldObject.GameObject.transform.localPosition = pos;

                        state.IsTracking = true;
                    }

                    if (state.WorldObject.Options != null)
                    {
                        ApplyOptions(state.WorldObject.GameObject.transform, GetCameraForObject(state.WorldObject), state.WorldObject.Options);
                    }
                }
            }
            else
            {
                if (state.IsTracking)
                {
                    // Transpose onto other world
                    var pos = state.WorldObject.GameObject.transform.localPosition;

                    state.WorldObject.GameObject.transform.SetParent(DefaultAnchor.transform, false);
                    state.WorldObject.GameObject.transform.localPosition = pos;

                    state.IsTracking = false;

                    if (state.WorldObject.Options != null)
                    {
                        ApplyOptions(state.WorldObject.GameObject.transform, GetCameraForObject(state.WorldObject), state.WorldObject.Options);
                    }
                }
                else
                {
                    base.SetPosition(state, worldDelta);
                }
            }
        }

        protected int m_resetCount;

        protected void SetAnchor()
        {
            m_resetCount++;

            CalibrateCompass(WorldAnchor.transform);

            m_gpsAnchorCoords = ForegroundPositionService.Instance.Position;

            // Reposition any ARKit objects so that they keep in sync with the GPS
            // position.
            foreach (var kv in m_objStates)
            {
                var state = kv.Value;
                var obj = kv.Key;

                if (state.TrackingState == ObjectTrackingState.Tracking)
                {
                    state.TrackingState = ObjectTrackingState.NotSet;

                    SetObjectPosition(obj, state, m_gpsAnchorCoords);
                }
            }
        }

        protected virtual void EnableTracking() { }

        protected virtual void DisableTracking() { }

        public void SetUseTracking(bool useTracking)
        {
            if (useTracking == UsingTracking)
            {
                return;
            }

            UsingTracking = useTracking;

            if (useTracking)
            {
                EnableTracking();
            }
            else
            {
                DisableTracking();
            }
        }

        public void ToggleUseTracking()
        {
            SetUseTracking(!UsingTracking);
        }

        protected override void Update()
        {
            // Rely on implementers to call UpdateObjects(isCameraTracking)
            //base.Update();
        }
    }
}
