// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if MOTIVE_VUFORIA
using Vuforia;
namespace Motive.AR.Vuforia
{
    public class ImageTargetTrackableEventHandler : VuforiaTrackableEventHandler
    {
        private ImageTargetBehaviour m_trackableBehaviour;

        protected override void Start()
        {
            base.Start();

            m_trackableBehaviour = GetComponent<ImageTargetBehaviour>();

            if (m_trackableBehaviour)
            {
                OnTrackingLost();

                m_trackableBehaviour.RegisterTrackableEventHandler(this);
            }
        }

        protected override void OnTrackingFound()
        {
            TargetName = m_trackableBehaviour.ImageTarget.Name;

            base.OnTrackingFound();
        }

        protected override void StartExtendedTracking()
        {
            if (ExtendedTracking)
            {
                if (m_trackableBehaviour.ImageTarget != null)
                {
                    m_trackableBehaviour.ImageTarget.StartExtendedTracking();
                }
            }

            base.StartExtendedTracking();
        }

        protected override void StopExtendedTracking()
        {
            if (m_trackableBehaviour.ImageTarget != null)
            {
                m_trackableBehaviour.ImageTarget.StartExtendedTracking();
            }

            base.StopExtendedTracking();
        }

        protected override VuMarkIdentifier GetVuMarkIdentifier()
        {
            return new VuMarkIdentifier
            {
                TargetName = m_trackableBehaviour.ImageTarget.Name,
                DatabaseId = DatabaseId
            };
        }
    }
}
#endif