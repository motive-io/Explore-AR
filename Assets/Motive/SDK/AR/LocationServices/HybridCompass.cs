// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive;
using Motive.AR.LocationServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HybridCompass : ICompass
{
    public LocationTrackerCompass TrackerCompass { get; private set; }
    ICompass m_systemCompass;

    public ICompass CurrentCompass
    {
        get
        {
            if (Platform.Instance.IsInBackground)
            {
                return TrackerCompass;
            }

            return m_systemCompass;
        }
    }

    public HybridCompass(ICompass systemCompass)
    {
        m_systemCompass = systemCompass;
        TrackerCompass = new LocationTrackerCompass();
    }

    public double MagneticHeading
    {
        get 
        {
            return CurrentCompass.MagneticHeading;
        }
    }

    public double TrueHeading
    {
        get
        {
            return CurrentCompass.TrueHeading;
        }
    }

    public void Start()
    {
        m_systemCompass.Start();
        TrackerCompass.Start();
    }

    public void Stop()
    {
        m_systemCompass.Stop();
        TrackerCompass.Stop();
    }
}
