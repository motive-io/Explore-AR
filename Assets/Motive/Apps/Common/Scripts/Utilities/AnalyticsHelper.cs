// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections.Generic;
using UnityEngine.Analytics;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Wrapper class to connect to custom analytics system.
    /// </summary>
    public static class AnalyticsHelper
    {
        // For now just uses Unity Analytics
        public static void FireEvent(string eventName, Dictionary<string, object> args = null)
        {
            ThreadHelper.Instance.CallOnMainThread(() =>
                {
                    Analytics.CustomEvent(eventName, args);
                });
        }
    }
}