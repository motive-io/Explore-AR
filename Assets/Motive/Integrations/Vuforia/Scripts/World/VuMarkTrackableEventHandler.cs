// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Events;

#if MOTIVE_VUFORIA
using Vuforia;

namespace Motive.AR.Vuforia
{
    public class VuMarkTrackableEventHandler : VuforiaTrackableEventHandler
    {
        private VuMarkBehaviour m_trackableBehaviour;

        public string InstanceId;

        protected override VuMarkIdentifier GetVuMarkIdentifier()
        {
            return new VuMarkIdentifier
            {
                TargetName = TargetName,
                DatabaseId = DatabaseId,
                InstanceId = InstanceId
            };
        }

        public string GetTrackableInstanceId()
        {
            if (m_trackableBehaviour.VuMarkTarget != null)
            {
                return new String(m_trackableBehaviour.VuMarkTarget.InstanceId.StringValue.Where(v => v != '\0').ToArray());
            }

            return null;
        }

        public override void ResetIdentifier()
        {
            Debug.Log("Current status " + m_trackableBehaviour.CurrentStatus);

            if (m_trackableBehaviour.VuMarkTarget != null)
            {
                var trackableInstanceId = GetTrackableInstanceId();

                Debug.Log("curr instance id = " + trackableInstanceId);

                if (trackableInstanceId != InstanceId)
                {
                    Debug.LogError("Srsly wtf: " + trackableInstanceId + " " + InstanceId);
                }
            }
            else
            {
                Debug.Log("NO TRACKING!! " + InstanceId);
            }

            base.ResetIdentifier();
        }

        public override void OnTrackableStateChanged(TrackableBehaviour.Status previousStatus, TrackableBehaviour.Status newStatus)
        {
            if (m_trackableBehaviour.VuMarkTarget != null)
            {
                Debug.Log(">>>> tracking " + m_trackableBehaviour.VuMarkTarget.InstanceId.StringValue);
            }

            base.OnTrackableStateChanged(previousStatus, newStatus);
        }

        protected override void StartExtendedTracking()
        {
            if (ExtendedTracking)
            {
                if (m_trackableBehaviour.VuMarkTarget != null)
                {
                    m_trackableBehaviour.VuMarkTarget.StartExtendedTracking();
                }
            }

            base.StartExtendedTracking();
        }

        protected override void StopExtendedTracking()
        {
            if (m_trackableBehaviour.VuMarkTarget != null)
            {
                m_trackableBehaviour.VuMarkTarget.StopExtendedTracking();
            }

            base.StopExtendedTracking();
        }

        protected override void OnTrackingFound()
        {
            InstanceId = GetTrackableInstanceId();
            TargetName = m_trackableBehaviour.VuMarkTarget.Template.Name;

            base.OnTrackingFound();
        }

        protected override void Start()
        {
            base.Start();

            m_trackableBehaviour = GetComponent<VuMarkBehaviour>();

            if (m_trackableBehaviour)
            {
                OnTrackingLost();

                m_trackableBehaviour.RegisterVuMarkTargetAssignedCallback(() =>
                    {
                        Debug.Log("Trackable assigned " + GetTrackableInstanceId());
                    });

                m_trackableBehaviour.RegisterVuMarkTargetLostCallback(() =>
                    {
                        Debug.Log("Trackable lost " + GetTrackableInstanceId());
                    });
                m_trackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }   
    }
}
#endif