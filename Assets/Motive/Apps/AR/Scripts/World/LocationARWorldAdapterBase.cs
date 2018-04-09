// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive._3D.Models;
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.AR
{
    public class LocationARWorldAdapterBase : 
        ARWorldAdapterBase<LocationARWorldObject>,
        IARWorldAdapter<SceneARWorldObject>
    {
        Coordinates m_lastCoords;

        protected class SceneWorldObjectState
        {
            public bool IsSpawned { get; set; }
            public bool IsTracking { get; set; }
            public SceneARWorldObject WorldObject { get; private set; }

            public SceneWorldObjectState(SceneARWorldObject worldObject)
            {
                WorldObject = worldObject;
            }
        }

        protected Dictionary<SceneARWorldObject, SceneWorldObjectState> m_sceneObjects;
        
        protected override void Awake()
        {
            base.Awake();

            m_sceneObjects = new Dictionary<SceneARWorldObject, SceneWorldObjectState>();
        }

        public float DistanceScale = 1f;

        // Calculate actual and visual distance, taking into
        // account distance scaling and other options.
        protected double DistanceTo(LocationARWorldObject obj, out double actualDistance)
        {
            actualDistance = ForegroundPositionService.Instance.Position.GetDistanceFrom(obj.Coordinates);
            var visualDistance = actualDistance;

            // Default: z is north, x is east, y is up
            //
            if (obj.Options.DistanceVariation is FixedDistanceVariation)
            {
                var dfix = obj.Options.DistanceVariation as FixedDistanceVariation;

                visualDistance = dfix.Distance;
            }
            else
            {
                var dvar = obj.Options.DistanceVariation as LinearDistanceVariation;

                if (dvar != null)
                {
                    if (dvar.Range != null)
                    {
                        visualDistance = dvar.Range.Clamp(visualDistance);
                    }

                    if (dvar.Scale != null)
                    {
                        visualDistance = visualDistance * dvar.Scale.Value;
                    }
                }
            }

            return visualDistance;
        }

        public virtual void SetPosition(LocationARWorldObject obj)
        {
            double actualDistance;

            var dist = DistanceTo(obj, out actualDistance);

            if (obj.Options.VisibleRange != null)
            {
                if (!obj.Options.VisibleRange.IsInRange(actualDistance))
                {
                    obj.GameObject.SetActive(false);
                    return;
                }
            }

            obj.GameObject.SetActive(true);

            dist *= DistanceScale;

            var pos = GetPosition(obj, ForegroundPositionService.Instance.Position);

            obj.GameObject.transform.localPosition = pos;

            if (obj.Options != null && obj.Options.AlwaysFaceViewer)
            {
                obj.GameObject.transform.LookAt(this.gameObject.transform);
            }
        }

        protected virtual Vector3 GetPosition(LocationARWorldObject obj, Coordinates relativeCoords = null)
        {
            //double actualDistance;
            var coords = relativeCoords ?? ForegroundPositionService.Instance.Position;

            var dist = coords.GetDistanceFrom(obj.Coordinates);
            var bearing = coords.GetBearingTo(obj.Coordinates);

            //var compass = m_heading;

            var angle = MathHelper.GetDegreesInRange(90 - bearing);

            // TODO: this isn't quite right
            Vector3 pos =
                new Vector3(
                    (float)(MathHelper.CosDeg(angle) * dist),
                    (float)obj.Elevation,
                    (float)(MathHelper.SinDeg(angle) * dist));

            if (obj.Offset.HasValue)
            {
                pos.x += obj.Offset.Value.x;
                pos.y += obj.Offset.Value.y;
                pos.z += obj.Offset.Value.z;
            }

            return pos;
        }

        public override double GetDistance(LocationARWorldObject worldObj)
        {
            return ForegroundPositionService.Instance.Position.GetDistanceFrom(worldObj.Coordinates);
        }

        void AttachGameObject(GameObject gameObj, Transform parent)
        {
            /*
            if (!m_objectGroups.TryGetValue(groupName, out parent))
            {
                parent = AddObjectGroup(groupName);
            }*/

            gameObj.layer = this.gameObject.layer;
            gameObj.transform.SetParent(parent);
        }

        public override void AttachWorldObject(LocationARWorldObject worldObject, Transform parent)
        {
            AttachGameObject(worldObject.GameObject, parent);

            SetPosition(worldObject);
        }

        protected virtual Vector3 GetWorldDelta()
        {
            var coords = ForegroundPositionService.Instance.Position;

            if (m_lastCoords == null)
            {
                m_lastCoords = coords;

                return Vector3.zero;
            }

            var dist = m_lastCoords.GetDistanceFrom(coords);
            var bearing = m_lastCoords.GetBearingTo(coords);

            //var compass = m_heading;

            var angle = MathHelper.GetDegreesInRange(90 - bearing);

            // TODO: this isn't quite right
            Vector3 pos =
                new Vector3(
                    (float)(MathHelper.CosDeg(angle) * dist),
                    0,
                    (float)(MathHelper.SinDeg(angle) * dist));

            m_lastCoords = coords;

            return pos;
        }

        public override void Activate()
        {
            m_lastCoords = null;
            
            base.Activate();
        }

        protected void UpdateObjects()
        {
            foreach (var obj in m_worldObjects)
            {
                SetPosition(obj);
            }

            Vector3 worldDelta = GetWorldDelta();

            foreach (var state in m_sceneObjects.Values)
            {
                SetPosition(state, worldDelta);
            }
        }

        protected virtual void Update()
        {
            if (IsActive)
            {
                UpdateObjects();
            }
        }

        protected float GetWorldHeading(Transform worldAnchor, Transform objTransform)
        {
            //      (x,z)
            // |----*
            // |   / 
            // |  /
            // |a/ 
            // +---------
            // tan(a) = x / y

            var globalFwd = objTransform.TransformPoint(Vector3.forward);
            var globalUp = objTransform.TransformPoint(Vector3.up);

            var camFwd = worldAnchor.InverseTransformPoint(globalFwd);
            var camUp = worldAnchor.InverseTransformPoint(globalUp);

            var camFwdProject = new Vector3(camFwd.x, camFwd.z).normalized;
            var camUpProject = new Vector3(camUp.x, camUp.z);

            var cross = Vector3.Cross(camFwdProject, camUpProject);

            var camHdg = Mathf.Atan2(camFwd.x, camFwd.z) * Mathf.Rad2Deg;

#if UNITY_ANDROID
        if (camFwd.y > 0)
        {
            //hdgDiff += 180;
        }
#endif
            return camHdg;
        }
        /*
        protected double GetWorldHeading(Transform anchor, Transform objTransform)
        {
            var fwd = GetTransformForward(anchor, objTransform);

            var z = Vector3.Dot(fwd, Vector3.forward);

            return Math.Acos(z) * 180 / Math.PI;
        }*/

        protected virtual void SetSpawnPosition(SceneARWorldObject worldObject, Camera worldCamera, Transform worldAnchor)
        {
            float correctAngle = 0f;
            var gameObj = worldObject.GameObject;

            Vector3 spawnPos = Vector3.zero;

            if (worldObject.Position is RelativeWorldPosition)
            {
                var relPos = worldObject.Position as RelativeWorldPosition;

                var hdg = GetWorldHeading(worldAnchor, worldCamera.transform);

                var angle = hdg + relPos.Angle;
                correctAngle = (float)angle;
                var rangle = angle * Mathf.Deg2Rad;

                m_logger.Debug("Using relative position with angle={0} and cam hdg={1}",
                               relPos.Angle, hdg);

                var y = (float)relPos.Elevation;
                var x = (float)(relPos.Distance * Math.Sin(rangle));
                var z = (float)(relPos.Distance * Math.Cos(rangle));

                spawnPos = new Vector3(x, y, z);
            }
            else if (worldObject.Position is FixedWorldPosition)
            {
                var fixPos = worldObject.Position as FixedWorldPosition;

                var y = (float)fixPos.Position.Y;
                var x = (float)fixPos.Position.X;
                var z = (float)fixPos.Position.Z;

                spawnPos = new Vector3(x, y, z);
            }

            m_logger.Debug("Spawning {0} at {1}", worldObject.GameObject.name, spawnPos);

            gameObj.transform.localScale = Vector3.one;
            gameObj.transform.localRotation = Quaternion.identity;
            gameObj.transform.Rotate(Vector3.up, correctAngle);
            gameObj.transform.localPosition = spawnPos;
        }

        protected void SpawnObject(SceneARWorldObject worldObject, Camera worldCamera, Transform worldAnchor)
        {
            var gameObj = worldObject.GameObject;

            AttachGameObject(gameObj, worldAnchor);

            SetSpawnPosition(worldObject, worldCamera, worldAnchor);
        }

        protected virtual void SetPosition(SceneWorldObjectState state, Vector3 worldDelta)
        {
            if (!state.IsSpawned)
            {
                SpawnObject(state.WorldObject, WorldCamera, WorldAnchor.transform);

                state.IsSpawned = true;
            }
            else
            {
                state.WorldObject.GameObject.transform.Translate(-worldDelta, Space.World);

                if (state.WorldObject.Options != null)
                {
                    ApplyOptions(state.WorldObject.GameObject.transform, GetCameraForObject(state.WorldObject), state.WorldObject.Options);
                }
            }
        }

        /// <summary>
        /// TODO: refactor this
        /// </summary>
        /// <param name="worldObject"></param>
        public void RemoveWorldObject(SceneARWorldObject worldObject)
        {
            m_sceneObjects.Remove(worldObject);
        }

        public void AddWorldObject(SceneARWorldObject worldObject)
        {
            m_sceneObjects.Add(worldObject, new SceneWorldObjectState(worldObject));
        }

        public double GetDistance(SceneARWorldObject worldObject)
        {
            if (worldObject.GameObject != null)
            {
                var cam = GetCameraForObject(worldObject);

                return (worldObject.GameObject.transform.position -
                    cam.transform.position).magnitude;
            }

            return 0;
        }

        public virtual Camera GetCameraForObject(SceneARWorldObject worldObj)
        {
            return GetCamera();
        }
    }
}
