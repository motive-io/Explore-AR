// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;
using System;
using System.Collections;
using Motive.AR.Kinetics;

namespace Motive.AR.Kinetics
{
    public class DebugStepTracker : MonoBehaviour, IStepTracker
    {

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (SteppedDown != null)
                {
                    SteppedDown(this, new StepTrackerEventArgs());
                }

                if (SteppedUp != null)
                {
                    SteppedUp(this, new StepTrackerEventArgs());
                }
            }
        }

        public event System.EventHandler<StepTrackerEventArgs> SteppedDown;

        public event System.EventHandler<StepTrackerEventArgs> SteppedUp;

        public void Stop()
        {
        }
    }
}