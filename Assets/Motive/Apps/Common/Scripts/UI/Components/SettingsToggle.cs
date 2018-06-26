// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Used in the debug screen to toggle player settings.
    /// </summary>
    public class SettingsToggle : MonoBehaviour
    {
        public Toggle Toggle;
        public string SettingName;

        // Use this for initialization
        void Awake()
        {
            Toggle.isOn = PlayerPrefs.GetInt(SettingName) == 1;
        }

        public void ToggleValue()
        {
            PlayerPrefs.SetInt(SettingName, Toggle.isOn ? 1 : 0);

            PlayerPrefs.Save();
        }
    }

}