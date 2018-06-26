// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Used to lay out screens with designated safe areas (like iPhone X).
    /// This resizes the bottom of the screen.
    /// </summary>
    public class SafeAreaBottom : MonoBehaviour
    {
        //public RectTransform ReferenceRect;

        // Use this for initialization
        void Start()
        {
#if UNITY_2017_2_OR_NEWER
            var tf = (RectTransform)this.transform;
            var duy = Screen.safeArea.y;
            var topPct = duy / Screen.height;

            tf.anchorMax = new Vector2(1, topPct);
            tf.anchorMin = new Vector2(0, 0);

            tf.sizeDelta = new Vector2(0, 0);
#endif
        }
    }

}