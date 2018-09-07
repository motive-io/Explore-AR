// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace Motive.Core.Utilities
{
    public class PedometerCalculator : Singleton<PedometerCalculator>
    {
        public event EventHandler Updated;
        public float StepsPerMinute { get; private set; }
        public int TotalSteps { get; private set; }

        private int m_size = 60;
        private DateTime[] m_data;
        private int m_idx = 0;

        public PedometerCalculator() : base()
        {
            StepsPerMinute = 0;
            m_data = new DateTime[m_size];

            Platform.Instance.Pedometer.Stepped += Pedometer_Stepped;
            ThreadHelper.Instance.StartCoroutine(ComputeSteps());
        }

        private IEnumerator ComputeSteps()
        {
            while (true)
            {
                StateCheck();
                yield return new WaitForSeconds(1);
            }
        }

        private void Pedometer_Stepped(object sender, Motive.AR.Kinetics.PedometerEventArgs e)
        {
            //Record The DateTime of the step
            TotalSteps++;

            //Circle back to first index if array full
            if (m_idx == (m_size - 1))
            {
                m_data[m_idx] = e.Time;
                m_idx = 0;
            }
            else
            {
                m_data[m_idx] = e.Time;
                m_idx++;
            }
        }

        private void StateCheck()
        {
            if (m_data != null)
            {
                bool previousTimeFound = false;
                bool isExit = false;

                //Cycle through array - getting previous recorded idx
                int i = (m_idx == 0) ? (m_size - 1) : (m_idx - 1);

                DateTime currentTime = m_data[i];
                DateTime thisTime = DateTime.Now;

                int prevIdx = i;

                double delta = 0;
                int steps = 0;

                while (!isExit)
                {
                    prevIdx--;
                    prevIdx = (prevIdx < 0) ? (m_size - 1) : prevIdx;

                    DateTime previousTime = m_data[prevIdx];
                    double difference = (currentTime - previousTime).TotalSeconds;

                    //If time between steps is too long, or not enough data recorded, or time elapsed between is too long set to 0
                    if (difference > 5 || difference == 0 || (thisTime - currentTime).TotalSeconds > 3)
                    {
                        isExit = true;
                    }
                    else
                    {
                        delta = difference;
                        previousTimeFound = true;
                        steps++;
                    }
                }

                //Estimate SPM with last 5 seconds of data - This can determine our state over the last few seconds
                //If we have no previous data, start reducing the current steps per minute (indicates we have stopped moving)
                if (previousTimeFound)
                {
                    StepsPerMinute = (float)((60 / delta) * steps);
                }
                else
                {
                    StepsPerMinute = (float)MathHelper.Interpolate((double)StepsPerMinute, 0, 0.5);

                    if (StepsPerMinute < 1)
                        StepsPerMinute = 0;
                }

                if (Updated != null)
                {
                    Updated(this, EventArgs.Empty);
                }
            }
        }

    }
}