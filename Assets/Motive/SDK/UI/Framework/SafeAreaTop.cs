// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Used to lay out screens with designated safe areas (like iPhone X).
    /// </summary>
    public class SafeAreaTop : MonoBehaviour
    {
        //public RectTransform ReferenceRect;

        // Use this for initialization
        void Start()
        {
#if UNITY_2017_2_OR_NEWER
            var tf = (RectTransform)this.transform;
            var duy = Screen.height - Screen.safeArea.height - Screen.safeArea.y;
            var topPct = duy / Screen.height;

            tf.anchorMax = new Vector2(1, 1);
            tf.anchorMin = new Vector2(0, 1 - topPct);

            tf.sizeDelta = new Vector2(0, 0);
#endif
        }
    }

}