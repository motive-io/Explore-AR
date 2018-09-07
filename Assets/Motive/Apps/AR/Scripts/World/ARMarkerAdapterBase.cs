using System;
using System.Collections;
using System.Collections.Generic;
using Motive.AR.Models;
using UnityEngine;

using Logger = Motive.Core.Diagnostics.Logger;

namespace Motive.Unity.AR
{
    public class ARMarkerAdapterBase : MonoBehaviour, IARMarkerAdapter
    {
        public GameObject WorldAnchor;
        public Camera WorldCamera;

        protected Logger m_logger;

        public event EventHandler<ARMarkerTrackingEventArgs> StartedTracking;
        public event EventHandler<ARMarkerTrackingEventArgs> LostTracking;

        protected virtual void Awake()
        {
            m_logger = new Logger(this);
        }

        public virtual void Activate()
        {
        }

        public virtual void Deactivate()
        {
        }

        public virtual void Initialize()
        {
        }

        public virtual void RegisterMarker(IVisualMarker marker)
        {
        }

        protected virtual void OnStartedTracking(string markerIdentifier, GameObject gameObject)
        {
            if (StartedTracking != null)
            {
                StartedTracking(this, new ARMarkerTrackingEventArgs(markerIdentifier, gameObject));
            }
        }

        protected virtual void OnLostTracking(string markerIdentifier, GameObject gameObject)
        {
            if (LostTracking != null)
            {
                LostTracking(this, new ARMarkerTrackingEventArgs(markerIdentifier, gameObject));
            }
        }
    }

    public class ARMarkerAdapterBase<T> : ARMarkerAdapterBase
        where T : IVisualMarker
    {
        public virtual void RegisterMarker(T marker)
        {

        }

        public override void RegisterMarker(IVisualMarker marker)
        {
            if (marker is T)
            {
                RegisterMarker((T)marker);
            }
            else
            {
                SystemErrorHandler.Instance.ReportError("Unsupported marker type {0}", marker.Type);
            }
        }
    }
}
