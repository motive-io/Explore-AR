// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Timing;
using System;
using UnityEngine;

namespace Motive.Unity.Timing
{
    /// <summary>
    /// Lets developers change clock settings for debugging.
    /// </summary>
    public class DebugClockController : MonoBehaviour
    {
        public bool UseDebugClock;
        public string BaseDate;
        public string BaseTime;
        public float ClockSpeedup = 1;
        public float TimerSpeedup = 1;

        void Awake()
        {
            if (UseDebugClock)
            {
                var baseTime = DateTime.Now;
                var baseDate = baseTime.Date;

                if (!string.IsNullOrEmpty(BaseDate))
                {
                    baseDate = DateTime.Parse(BaseDate).Date;
                    baseTime = baseDate + baseTime.TimeOfDay;
                }

                if (!string.IsNullOrEmpty(BaseTime))
                {
                    var tod = TimeSpan.Parse(BaseTime);
                    baseTime = baseDate + tod;
                }

                var provider = new DebugClockProvider
                {
                    ClockSpeedup = ClockSpeedup,
                    TimerSpeedup = TimerSpeedup,
                    BaseTime = baseTime
                };

                ClockManager.Instance.SetClockProvider(provider);
            }
        }
    }
}
