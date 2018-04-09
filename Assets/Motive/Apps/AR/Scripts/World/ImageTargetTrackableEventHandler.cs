// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.AR.Vuforia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#if VUFORIA_ANDROID_SETTINGS || VUFORIA_IOS_SETTINGS
using Vuforia;

public class ImageTargetTrackableEventHandler : VuforiaTrackableEventHandler
{
    private ImageTargetBehaviour m_trackableBehaviour;

    public string TargetName;
    public string DatabaseId;

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

    protected override VuMarkIdentifier GetVuMarkIdentifier()
    {
        return new VuMarkIdentifier
        {
            TargetName = m_trackableBehaviour.ImageTarget.Name,
            DatabaseId = DatabaseId
        };
    }
}
#endif