// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Core.Timing;
using Motive.Unity.Utilities;
using System;

namespace Motive.Unity.Timing
{
    /// <summary>
    /// A timer that fires on the Unity thread.
    /// </summary>
    public class UnityTimer
    {
        public Timer Timer { get; private set; }
        private UnityTimer(Timer timer)
        {
            Timer = timer;

            timer.Start();
        }

        public void Cancel()
        {
            if (Timer != null)
            {
                Timer.Cancel();
            }
        }

        static void OnFire(Action action)
        {
            if (action != null)
            {
                ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    action();
                });
            }
        }

        public static UnityTimer Call(DateTime fireTime, Action action, bool repeats = false)
        {
            Timer timer = new Timer(fireTime, () =>
                {
                    OnFire(action);
                }, repeats);

            return new UnityTimer(timer);
        }

        public static UnityTimer Call(TimeSpan duration, Action action, bool repeats = false)
        {
            Timer timer = new Timer(duration, () =>
            {
                OnFire(action);
            }, repeats);

            return new UnityTimer(timer);
        }

        public static UnityTimer Call(double duration, Action action, bool repeats = false)
        {
            Timer timer = new Timer(duration, () =>
            {
                OnFire(action);
            }, repeats);

            return new UnityTimer(timer);
        }

    }
}
