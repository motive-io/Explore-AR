// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.LocationServices;
using Motive.AR.Models;
using Motive.Core.Utilities;
using Motive.Unity.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.AR
{
    /// <summary>
    /// Base class for implementing AR world adapters.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ARWorldAdapterBase<T> : MonoBehaviour, 
        IARWorldAdapter<T> where T : ARWorldObject
    {
        public Camera WorldCamera;

        public GameObject WorldAnchor;

		public Canvas Canvas;

		public GameObject[] ActivateWhenActive;

        protected HashSet<T> m_worldObjects;

        public bool IsActive { get; private set; }

        protected Logger m_logger;

        protected virtual void Awake()
        {
            m_logger = new Logger(this);

            m_worldObjects = new HashSet<T>();
        }

		protected virtual void Start()
		{
		}

		public virtual void Initialize()
		{
			if (!WorldCamera)
			{
				WorldCamera = Camera.main;
			}
		}

        public virtual void Deactivate()
        {
			ObjectHelper.SetObjectsActive(ActivateWhenActive, false);

			if (Canvas)
			{
				Canvas.enabled = false;
			}

            IsActive = false;
        }

        public virtual void Activate()
        {
			ObjectHelper.SetObjectsActive(ActivateWhenActive, true);

			if (Canvas)
			{
				Canvas.enabled = true;
			}

            IsActive = true;
        }
        
        public virtual void RemoveWorldObject(T worldObject)
        {
            m_worldObjects.Remove(worldObject);
        }

        public virtual void AddWorldObject(T worldObject, bool attach)
        {
            m_worldObjects.Add(worldObject);

            if (attach)
            {
                AttachWorldObject(worldObject, WorldAnchor.transform);
            }
        }

        public virtual void AddWorldObject(T worldObject)
        {
            AddWorldObject(worldObject, true);
        }

        public virtual Camera GetCamera()
        {
            return WorldCamera;
        }

        public abstract void AttachWorldObject(T worldObject, Transform parent);

        public virtual double GetDistance(T worldObject)
        {
            if (worldObject.GameObject != null)
            {
                return (worldObject.GameObject.transform.position -
                    WorldCamera.transform.position).magnitude;
            }

            return 0;
        }

        public virtual Camera GetCameraForObject(T worldObj)
        {
            return GetCamera();
        }

        protected Vector3 Get2DPosition(Transform transform)
        {
            return new Vector3(transform.position.x, 0, transform.position.z);
        }
        
        protected void ApplyOptions(Transform objectTransform, Camera worldCamera, ILocationAugmentedOptions options)
        {
            if (options == null)
            {
                return;
            }

            var p1 = Get2DPosition(objectTransform);
            var p2 = Get2DPosition(worldCamera.transform);

            var distance = (p2 - p1).magnitude;

            if (options.VisibleRange != null &&
               !options.VisibleRange.IsInRange(distance))

            {
                objectTransform.gameObject.SetActive(false);
                return;
            }

            objectTransform.gameObject.SetActive(true);

            double? fixedDistance = null;

            if (options.DistanceVariation is LinearDistanceVariation)
            {
                var distanceOpts = (LinearDistanceVariation)options.DistanceVariation;

                // Only thing to do is adjust distance scale if necessary
                if (distanceOpts.Scale.GetValueOrDefault(1) != 1)
                {
                    var scale = distanceOpts.Scale.Value;

                    // In the default case, we actually vary the distance placed by DistanceScale.
                    // For ARKit, we achieve the same effect by varying the scale of the obejct
                    // by the inverse computed scale.
                    if (scale != 0)
                    {
                        objectTransform.transform.localScale = Vector3.one / (float)distanceOpts.Scale.Value;
                    }
                }

                if (distanceOpts.Range != null && !distanceOpts.Range.IsInRange(distance))
                {
                    fixedDistance = distanceOpts.Range.Clamp(distance);
                }
            }
            else if (options.DistanceVariation is FixedDistanceVariation)
            {
                fixedDistance = ((FixedDistanceVariation)options.DistanceVariation).Distance;
            }

            if (fixedDistance.HasValue)
            {
                var camPos = Get2DPosition(WorldCamera.transform);
                var objPos = Get2DPosition(objectTransform.transform);

                var deltaPos = camPos - objPos;

                var relativePos = deltaPos.normalized * (float)fixedDistance.Value;

                var worldPos = camPos - relativePos;
                worldPos.y = objectTransform.transform.position.y;

                objectTransform.position = worldPos;
            }

            if (options.AlwaysFaceViewer)
            {
                objectTransform.LookAt(worldCamera.transform);
            }
        }

    }
}
