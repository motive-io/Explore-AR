// Copyright (c) 2018 RocketChicken Interactive Inc.
using UnityEngine;

namespace Motive.Unity.Utilities
{
    /// <summary>
    /// Helper class for using PlayerPrefs.
    /// </summary>
    public static class SettingsHelper
    {
        public static bool IsSet(string setting)
        {
            return PlayerPrefs.GetInt(setting) == 1;
        }

        public static void Set(string setting, bool value)
        {
            PlayerPrefs.SetInt(setting, value ? 1 : 0);

            PlayerPrefs.Save();
        }

        public static bool IsDebugSet(string p)
        {
            if (ThreadHelper.Instance.IsUnityThread)
            {
                return BuildSettings.IsDebug && IsSet(p);
            }

            return false;
        }
    }

}