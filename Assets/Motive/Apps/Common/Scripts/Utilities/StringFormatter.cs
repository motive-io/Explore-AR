// Copyright (c) 2018 RocketChicken Interactive Inc.
using System;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Handles various string formats for consistency.
    /// </summary>
    public static class StringFormatter
    {
        public static string FormatHexColor(UnityEngine.Color color)
        {
            return string.Format("{0:x2}{1:x2}{2:x2}{3:x2}",
                (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255), (int)(color.a * 255));
        }

        public static string FormatTimespan(TimeSpan ts)
        {
            return string.Format("{0:0}:{1:00}", ts.Minutes, ts.Seconds);
        }

        internal static object FormatTimespan(double p)
        {
            return FormatTimespan(TimeSpan.FromSeconds(p));
        }

        public static string FormatDistance(double distance)
        {
            if (distance < 1000)
            {
                return string.Format("{0:0}m", distance);
            }
            if (distance < 10000)
            {
                return string.Format("{0:0.0}km", distance / 1000);
            }
            else
            {
                return string.Format("{0:0}km", distance / 1000);
            }
        }

        public static string GetGiveCountString(int has, int required, UnityEngine.Color unsatisfiedTextColor)
        {
            if (has >= required)
            {
                return string.Format("{0}/{1}", has, required);
            }
            else
            {
                return string.Format("<color='#{0}'>{1}</color>/{2}", StringFormatter.FormatHexColor(unsatisfiedTextColor), has, required);
            }
        }
    }

}