// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Timing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

#if MOTIVE_VUFORIA
using Vuforia;

namespace Motive.AR.Vuforia
{
    public abstract class VuforiaTrackableEventHandler : MonoBehaviour, ITrackableEventHandler, IPointerClickHandler
    {
        public UnityEvent Clicked;

        public string TargetName;
        public string DatabaseId;
        public bool ExtendedTracking;
         
        public bool IsTracking { get; private set; }

        private VuMarkIdentifier m_identifier;

        UnityTimer m_trackingTimer;

        public VuMarkIdentifier Identifier
        {
            get
            {
                if (m_identifier == null)
                {
                    m_identifier = GetVuMarkIdentifier();
                }

                return m_identifier;
            }
        }

        protected abstract VuMarkIdentifier GetVuMarkIdentifier();

        protected virtual void Start()
        {
            if (Clicked == null)
            {
                Clicked = new UnityEvent();
            }
        }

        protected virtual void StartExtendedTracking()
        {

        }

        protected virtual void StopExtendedTracking()
        {

        }

        public virtual void ResetIdentifier()
        {
            m_identifier = null;
        }

        public virtual void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
        {
            Debug.Log("tracking " + Identifier +
                " curr=" + IsTracking +
                " state=" + newStatus);
            /*
        if (m_trackableBehaviour && m_trackableBehaviour.VuMarkTarget != null && m_trackableBehaviour.VuMarkTarget.InstanceId != null)
        {
            Debug.Log("tracking " + m_trackableBehaviour.VuMarkTarget.InstanceId +
                " curr=" + IsTracking +
                " state=" + newStatus);
        }
         */

            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
                if (m_trackingTimer != null)
                {
                    m_trackingTimer.Cancel();
                    m_trackingTimer = null;
                }

                if (!IsTracking)
                {
                    StartExtendedTracking();

                    OnTrackingFound();
                }
                else if (newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
                {
                    if (m_trackingTimer != null)
                    {
                        m_trackingTimer.Cancel();
                        m_trackingTimer = null;
                    }

                    m_trackingTimer = UnityTimer.Call(3, StopExtendedTracking);
                }
            }
            else
            {
                if (IsTracking)
                {
                    OnTrackingLost();
                }
            }

            if (VuforiaWorld.Instance.TrackingConditionMonitor != null)
            {
                VuforiaWorld.Instance.TrackingConditionMonitor.TrackingMarkersUpdated();
            }
        }

        protected virtual void OnTrackingFound()
        {
            IsTracking = true;

            // Reset the identifier
            m_identifier = null;

            /*
        var instanceId = m_trackableBehaviour.VuMarkTarget.InstanceId.StringValue;

        var instances = GetComponentsInChildren<VuMarkTrackableInstance>(true).Where(i => i.InstanceId == instanceId);

        foreach (var instance in instances)
        {
            instance.gameObject.SetActive(true);
        }

        InstanceId = m_trackableBehaviour.VuMarkTarget.InstanceId.StringValue;
        */

            Debug.Log("Trackable " + Identifier.ToString() + " found");

            VuforiaWorld.Instance.StartTracking(this);
        }

        protected virtual void OnTrackingLost()
        {
            m_identifier = null;

            IsTracking = false;

            var instances = GetComponentsInChildren<VuMarkTrackableInstance>();

            foreach (var instance in instances)
            {
                instance.gameObject.SetActive(false);
            }

            VuforiaWorld.Instance.StopTracking(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Clicked != null)
            {
                Clicked.Invoke();
            }
        }
    }
}
#endif