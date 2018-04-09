// Copyright (c) 2018 RocketChicken Interactive Inc.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Motive.UI.Framework
{
    /// <summary>
    /// Used to lay out screens with designated safe areas (like iPhone X).
    /// This represents the usable "safe" area.
    /// </summary>
    public class SafeAreaResize : MonoBehaviour
    {
        public bool Top = true;
        public bool Bottom = true;

        Rect m_safeArea;
        int m_screenWidth;
        int m_screenHeight;

        // Use this for initialization
        void Start()
        {
            Apply();
        }

        void Apply()
        {
            m_safeArea = Screen.safeArea;
            m_screenWidth = Screen.width;
            m_screenHeight = Screen.height;

#if UNITY_2017_2_OR_NEWER
            var tf = (RectTransform)this.transform;

            var dux = Screen.width - Screen.safeArea.width - Screen.safeArea.x;
            var duy = Screen.height - Screen.safeArea.height - Screen.safeArea.y;

            var left = Screen.safeArea.x / Screen.width;
            var right = (Screen.width - dux) / Screen.width;
            var bottom = Bottom ? Screen.safeArea.y / Screen.height : 0;
            var top = Top ? (Screen.height - duy) / Screen.height : 1;

            //right = Mathf.Min(right, 1);
            //top = Mathf.Min(top, 1);

            tf.anchorMin = new Vector2(left, bottom);
            tf.anchorMax = new Vector2(right, top);
            tf.sizeDelta = new Vector2(0, 0);

            //Debug.LogFormat("safe area min={0} max={1}", tf.anchorMin, tf.anchorMax);
            //Debug.LogFormat("safe area w={0} h={1} x={2} y={3}", Screen.safeArea.width, Screen.safeArea.height, Screen.safeArea.x, Screen.safeArea.y);
#endif
        }

        void Update()
        {
            // These values can seemingly change
            if (Screen.safeArea != m_safeArea ||
                Screen.width != m_screenWidth ||
                Screen.height != m_screenHeight)
            {
                Apply();
            }
        }
    }
}