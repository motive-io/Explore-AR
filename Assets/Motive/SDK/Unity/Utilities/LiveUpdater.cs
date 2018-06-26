// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Calls Update on a regular timer in a non-Unity thread.
    /// </summary>
    public class LiveUpdater
    {
        System.Threading.Timer m_timer;

        public LiveUpdater()
        {
        }

        public void Start(TimeSpan interval, Func<bool> call)
        {
            m_timer = new System.Threading.Timer((state) => {
                if (call != null)
                {
                    if (!call())
                    {
                        m_timer.Dispose();
                        m_timer = null;

                        return;
                    };
                }

                m_timer.Change(interval, TimeSpan.FromMilliseconds(-1));
            }, this, interval, TimeSpan.FromMilliseconds(-1));
        }

        public static void Run(TimeSpan interval, Func<bool> call)
        {
            var updater = new LiveUpdater();

            updater.Start(interval, call);
        }

        public static void Run(TimeSpan interval, Func<TimeSpan, bool> call)
        {
            var start = DateTime.Now;

            Run(interval,
                () => {
                    var span = DateTime.Now - start;

                    return call(span);
                });
        }

        public static void Interpolate(float start,
                                float finish,
                                TimeSpan duration,
                                TimeSpan interval,
                                Action<float> updater,
                                Action onComplete)
        {
            Run(interval,
                 (time) => {
                     if (time < duration)
                     {
                         var pct = (float)(time.TotalSeconds / duration.TotalSeconds);
                         var curr = start + (finish - start) * pct;

                         updater(curr);

                         return true;
                     }
                     else
                     {
                         updater(finish);

                         if (onComplete != null)
                         {
                             onComplete();
                         }

                         return false;
                     }
                 });
        }
    }
}

