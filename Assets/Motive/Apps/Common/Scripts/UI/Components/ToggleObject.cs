// Copyright (c) 2018 RocketChicken Interactive Inc.
using Motive.Unity.Utilities;
using UnityEngine;

namespace Motive.Unity.UI
{
    /// <summary>
    /// Utility component that toggles game objects.
    /// </summary>
    public class ToggleObject : MonoBehaviour
    {
        public GameObject[] ShowWhenOn;
        public GameObject[] ShowWhenOff;

        public bool IsOn;

        void Awake()
        {
            SetIsOn(IsOn);
        }

        public void SetIsOn(bool isOn)
        {
            this.IsOn = isOn;

            ObjectHelper.SetObjectsActive(ShowWhenOn, isOn);
            ObjectHelper.SetObjectsActive(ShowWhenOff, !isOn);
        }

        public void Toggle()
        {
            SetIsOn(!IsOn);
        }
    }

}